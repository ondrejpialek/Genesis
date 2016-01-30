using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Genesis.Analysis
{
    public class FrequencyAnalyzer : Analyzer<FrequencyAnalyzer.Settings, FrequencyAnalysis>
    {
        public class Settings
        {
            public IEnumerable<NominalTrait> Traits { get; set; }
        }

        public FrequencyAnalyzer(string title, Settings settings) : base(title, settings) { }

        public override FrequencyAnalysis Analyse(GenesisContext context)
        {
            var traitIds = settings.Traits.Select(t => t.Id).ToList();

            var analysis = new FrequencyAnalysis
            {
                Analyzed = DateTime.Now,
                Name = Title
            };

            //prepare context
            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.ValidateOnSaveEnabled = false;
            context.Mice.Include(m => m.Records).Load();

            var categories = context.Categories.Where(c => traitIds.Contains(c.TraitId)).ToList();
            var species = categories.OfType<Allele>().Select(a => a.Species).Distinct().ToList();

            var localities = context.Localities.Include(l => l.Mice).ToList();
            var localitiesCount = localities.Count;

            if (localitiesCount > 0)
            {
                localities.ToObservable(Scheduler.TaskPool).SelectMany(l => Frequencies(l, traitIds, categories, species))
                .Do(_ => Progress += (double)1 / localitiesCount)
                .Where(r => r != null)
                .Do(f =>
                {
                    analysis.Frequencies.Add(f);
                }).Wait();

                Done = true;
            }
            else
            {
                Done = true;
            }


            return analysis;
        }

        public class RawRecord
        {
            private readonly Mouse mouse;
            private readonly Category value;
            private readonly Species species;

            public Mouse Mouse
            {
                get { return mouse; }
            }

            public Category Value
            {
                get { return value; }
            }

            public Species Species
            {
                get { return species; }
            }

            public RawRecord(Mouse mouse, Category value, Species species)
            {
                this.mouse = mouse;
                this.value = value;
                this.species = species;
            }
        }

        public static IEnumerable<Frequency> Frequencies(Locality locality, List<int> traitIds, List<Category> categories, List<Species> species)
        {
            var records = (from mouse in locality.Mice
                from record in mouse.Records.OfType<NominalRecord>()
                where traitIds.Contains(record.TraitId)
                select new RawRecord(mouse, record.Category, (record.Category as Allele)?.Species)).ToList();

            if (records.Count > 0)
            {
                foreach (var frequency in SpeciesFrequencies(locality, records, species)) yield return frequency;
                foreach (var frequency in ValuesFrequencies(locality, records, categories)) yield return frequency;
            }
        }

        private static IEnumerable<Frequency> SpeciesFrequencies(Locality locality, List<RawRecord> records, List<Species> species)
        {
            var foundSpecies = (from s in species
                join record in records on s equals record.Species into found
                select new { Species = s, Count = found.Count() }).ToList();

            var total = foundSpecies.Sum(s => s.Count);
            var sampleSize = records.Where(v => v.Species != null).Select(v => v.Mouse).Distinct().Count();

            return foundSpecies.Select(speciesResult => new Frequency
            {
                Locality = locality,
                SampleSize = sampleSize,
                Name = $"{speciesResult.Species.Name} [species]",
                Value = (double)speciesResult.Count / total
            });
        }

        private static IEnumerable<Frequency> ValuesFrequencies(Locality locality, List<RawRecord> records, List<Category> categories)
        {
            var results = (from category in categories
                                join record in records on category equals record.Value into found
                                select new { Category = category, Count = found.Count() }).ToList();

            var total = results.Sum(s => s.Count);
            var sampleSize = records.Where(v => v.Species != null).Select(v => v.Mouse).Distinct().Count();

            return results.Select(categoryResult => new Frequency
            {
                Locality = locality,
                SampleSize = sampleSize,
                Name = $"{categoryResult.Category.Value} [value]",
                Value = (double)categoryResult.Count / total
            });
        }
    }
}
