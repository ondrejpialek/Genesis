using System.Data.Entity;
using Genesis.Core.Data;
using Genesis.Core.Migrations;

namespace Genesis
{
    [DbConfigurationType(typeof(EFCodeConfig))]
    public class GenesisContext : DbContext
    {
        static GenesisContext()
        {
            //Database.SetInitializer(new RecreateDatabaseInitializer());           
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<GenesisContext, MigrationsConfiguration>());
        }

        public DbSet<Record> Records { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Allele> Alleles { get; set; }

        public DbSet<Species> Species { get; set; }
        public DbSet<Mouse> Mice { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Trait> Traits { get; set; }

        public DbSet<Chromosome> Chromosomes { get; set; }
        public DbSet<Gene> Genes { get; set; }
        public DbSet<FrequencyAnalysis> FrequencyAnalysis { get; set; }
        public DbSet<FstAnalysis> FstAnalysis { get; set; }

        public GenesisContext() { }

        public GenesisContext(string connectionStringOrName) : base(connectionStringOrName) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FrequencyAnalysis>().HasMany(f => f.Frequencies).WithRequired().WillCascadeOnDelete();

            //No cascade due to:
            //System.Data.SqlClient.SqlException (0x80131904): 
            //Introducing FOREIGN KEY constraint 'FK_dbo.Records_dbo.Categories_CategoryId' on table 'Records' may cause cycles 
            //or multiple cascade paths. Specify ON DELETE NO ACTION or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints.
            modelBuilder.Entity<NominalRecord>().HasRequired(f => f.Category).WithMany().Map(c => c.MapKey("CategoryId")).WillCascadeOnDelete(false);
            modelBuilder.Entity<Record>().HasKey(r => new {r.Id, r.MouseId});
        }
    }
}
