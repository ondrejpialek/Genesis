using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis.Analysis
{
    public abstract class Analyzer<TAnalysisSettings, TResult>
    {
        public string Title { get; set; }
        public bool Done { get; set; }
        public double Progress { get; set; }

        protected TAnalysisSettings settings;

        public Analyzer(string title, TAnalysisSettings settings)
        {
            this.Title = title;
            this.settings = settings;
        }

        public abstract TResult Analyse(GenesisContext context);
    }
}
