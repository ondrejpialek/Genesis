using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Genesis.Analysis
{
    public class FstAnalyzer : Analyzer<FstAnalyzer.Settings, FstAnalysis>
    {
        public class Settings {
            public IEnumerable<Gene> Genes { get; set; }
            public Species Species { get; set; }
        }

        public FstAnalyzer(string title, Settings settings) : base(title, settings) { }

        public override FstAnalysis Analyse(GenesisContext context)
        {
            var genes = settings.Genes.Select(g => g.Id).ToList();
            var categories = context.Categories.Where(c => genes.Contains(c.TraitId)).ToList();
            var species = categories.OfType<Allele>().Select(a => a.Species).Distinct().ToList();

            var result = new FstAnalysis {Analyzed = DateTime.Now, Name = Title};

            context.Alleles.Load();
            context.Mice.Include(m => m.Records).Load();

            var frequencies = context.Localities.Include(l => l.Mice).ToList().Select(l =>
            {
                var analysis = FrequencyAnalyzer.Frequencies(l, genes, categories, species).First(f => f.Name.StartsWith(settings.Species.Name));

                if (analysis.SampleSize == 0)
                    return null;

                return new Frequency()
                {
                    Locality = l,
                    SampleSize = analysis.SampleSize,
                    Value = analysis.Value
                };
            })
            .Where(r => r != null)
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
