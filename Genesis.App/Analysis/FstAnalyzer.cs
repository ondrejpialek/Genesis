using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis.Analysis
{
    public class FstAnalyzer : Analyzer<FstAnalyzer.Settings, FstAnalysis>
    {
        public class Settings {
            public IEnumerable<Gene> Genes { get; set; }
            public Species Species { get; set; }
        }

        public FstAnalyzer(string title, FstAnalyzer.Settings settings) : base(title, settings) { }

        public override FstAnalysis Analyse(GenesisContext context)
        {
            var genes = this.settings.Genes.Select(g => g.Id).ToList();
            int species = this.settings.Species.Id;

            var result = new FstAnalysis();
            result.Analyzed = DateTime.Now;
            result.Name = this.Title;

            var frequencies = context.Localities.ToList().Select(l =>
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
            .Where(r => r.SampleSize > 0)
            .ToList();

            var n = frequencies.Sum(f => f.SampleSize);
            var Hs = frequencies.Sum(f => f.SampleSize * 2 * f.Value * (1 - f.Value)) / n;
            var pt = frequencies.Sum(f => f.SampleSize * f.Value) / n;
            var Ht = 2 * pt * (1 - pt);

            result.Fst = (Ht - Hs) / Ht;
            return result;
        }
    }
}
