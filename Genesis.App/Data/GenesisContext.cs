using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Genesis.App.Data;

namespace Genesis
{
    public class GenesisContext : DbContext
    {
        static GenesisContext()
        {
            Database.SetInitializer<GenesisContext>(new DataInitializer());
        }

        public DbSet<Allele> Alleles { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<Mouse> Mice { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Chromosome> Chromosomes { get; set; }
        public DbSet<Gene> Genes { get; set; }
        public DbSet<FrequencyAnalysis> FrequencyAnalysis { get; set; }
    }
}
