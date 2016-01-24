using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Genesis.Analysis
{
    public class FrequencyAnalyzer : Analyzer<FrequencyAnalyzer.Settings, FrequencyAnalysis>
    {
        public struct Result
        {
            public double Frequency;
            public int SampleSize;
        }

        public class Settings
        {
            public IEnumerable<NominalTrait> Traits { get; set; }
        }

        public FrequencyAnalyzer(string title, Settings settings) : base(title, settings) { }

        public override FrequencyAnalysis Analyse(GenesisContext context)
        {
            var traitIds = settings.Traits.Select(t => t.Id).ToList();
            var analyzeSpecies = settings.Traits.All(t => t is Gene);

            var analysis = new FrequencyAnalysis
            {
                Analyzed = DateTime.Now,
                Name = Title
            };

            //prepare context
            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.ValidateOnSaveEnabled = false;
            context.Mice.Include(m => m.Records).Load();

            var localities = context.Localities.Include(l => l.Mice).ToList();
            var localitiesCount = localities.Count;

            if (localitiesCount > 0)
            {
                context.Localities.ToObservable(Scheduler.TaskPool).SelectMany(l => Frequencies(l, traitIds))
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

        public static IEnumerable<Frequency> Frequencies(Locality locality, List<int> traitIds)
        {
            var records = (from mouse in locality.Mice
                from record in mouse.Records.OfType<NominalRecord>()
                where traitIds.Contains(record.Trait.Id)
                select new RawRecord(mouse, record.Category, (record.Category as Allele)?.Species)).ToList();

            if (records.Count > 0)
            {
                foreach (var frequency in SpeciesFrequencies(locality, records)) yield return frequency;
                foreach (var frequency in ValuesFrequencies(locality, records)) yield return frequency;
            }
        }

        private static IEnumerable<Frequency> SpeciesFrequencies(Locality locality, List<RawRecord> records)
        {
            var species = records.GroupBy(v => v.Species).Where(g => g.Key != null).ToList();
            var total = species.Sum(s => s.Count());
            var sampleSize = records.Where(v => v.Species != null).Select(v => v.Mouse).Distinct().Count();
            foreach (var speciesResults in species)
            {
                yield return new Frequency
                {
                    Locality = locality,
                    SampleSize = sampleSize,
                    Name = $"{speciesResults.Key.Name} [species]",
                    Value = (double)speciesResults.Count() / total
                };
            }
        }

        private static IEnumerable<Frequency> ValuesFrequencies(Locality locality, List<RawRecord> records)
        {
            var valueGroups = records.GroupBy(v => v.Value).Where(g => g.Key != null).ToList();
            var total = valueGroups.Sum(s => s.Count());
            var sampleSize = records.Where(v => v.Species != null).Select(v => v.Mouse).Distinct().Count();
            foreach (var values in valueGroups)
            {
                yield return new Frequency
                {
                    Locality = locality,
                    SampleSize = sampleSize,
                    Name = $"{values.Key.Value} [value]",
                    Value = (double)values.Count() / total
                };
            }
        }
    }
}
