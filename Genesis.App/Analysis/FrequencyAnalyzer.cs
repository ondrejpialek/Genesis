using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Genesis.Analysis
{
    public class FrequencyAnalyzer : Analyzer<FrequencyAnalyzer.Settings, FrequencyAnalysis>
    {
        public class Settings
        {
            public IEnumerable<Gene> Genes { get; set; }
            public Species Species { get; set; }
        }

        public FrequencyAnalyzer(string title, FrequencyAnalyzer.Settings settings) : base(title, settings) { }

        public override FrequencyAnalysis Analyse(GenesisContext context)
        {
            var genes = this.settings.Genes.Select(g => g.Id).ToList();
            int species = this.settings.Species.Id;

            var analysis = new FrequencyAnalysis();
            analysis.Analyzed = DateTime.Now;
            analysis.Name = this.Title;

            var localities = context.Localities.ToList();
            var recordCount = localities.Count;
  
            if (recordCount > 0)
            {
                context.Localities.ToObservable(Scheduler.TaskPool).Select(l =>
                {
                    var alleles = (from mouse in l.Mice
                                    from record in mouse.Alleles
                                    where genes.Contains(record.Allele.Gene.Id)
                                    select new { Mouse = mouse, AlleleSpecies = record.Allele.Species }).ToList();

                    double frequency = 0;
                    int sampleSize = 0;
                    if (alleles.Count > 0)
                    {
                        double spec = alleles.Where(s => s.AlleleSpecies.Id == species).Count();
                        frequency = spec / alleles.Count;

                        sampleSize = alleles.Select(a => a.Mouse).Distinct().Count();
                    }

                    return new Frequency
                    {
                        Locality = l,
                        SampleSize = sampleSize,
                        Value = frequency
                    };
                })
                .Do((_) => this.Progress += 1 / recordCount)
                .Where(r => r.SampleSize > 0)
                .Do(f =>
                {
                    analysis.Frequencies.Add(f);
                }).Wait();

                this.Done = true;
            }
            else
            {
                this.Done = true;
            };


            return analysis;
        }
    }
}
