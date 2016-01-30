using System;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;

namespace Genesis.Core.Migrations
{
    internal sealed class MigrationsConfiguration : DbMigrationsConfiguration<GenesisContext>
    {
        public MigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Genesis.GenesisContext";
        }

        protected override void Seed(GenesisContext context)
        {
            if (!context.Species.Any())
            {

                var m = new Species()
                {
                    Name = "Mus musculus musculus"
                };

                context.Species.Add(m);

                var d = new Species()
                {
                    Name = "Mus musculus domesticus"
                };

                context.Species.Add(d);

                context.Chromosomes.Add(new MtDNAChromosome()
                {
                    Name = "MtDNA"
                });

                var Xchrom = context.Chromosomes.Add(new XChromosome()
                {
                    Name = "X"
                });

                context.Chromosomes.Add(new YChromosome()
                {
                    Name = "Y"
                });

                var autosomalChrom = new AutosomalChromosome()
                {
                    Name = "Autosomal"
                };
                context.Chromosomes.Add(autosomalChrom);

                var btk = new Gene()
                {
                    Name = "Btk"
                };

                Xchrom.Genes.Add(btk);

                btk.Categories.Add(new Allele()
                {
                    Value = "d",
                    Species = d
                });

                btk.Categories.Add(new Allele()
                {
                    Value = "m",
                    Species = m
                });
            }

            context.SaveChanges();

            try
            {
                if (!context.Records.Any())
                {
                    var locality = new Locality("TEST", "Test Locality");
                    context.Localities.Add(locality);

                    var mouse = new Mouse("T001", Sex.Male, locality);
                    context.Mice.Add(mouse);

                    mouse.Records.Add(new NominalRecord(context.Categories.First(), mouse));

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Debugger.Break();
                throw;
            }

        }
    }
}
