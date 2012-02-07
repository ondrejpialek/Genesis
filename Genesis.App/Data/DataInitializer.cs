using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Genesis.App.Data
{
    //class DataInitializer : DropCreateDatabaseAlways<GenesisContext>
    class DataInitializer : DropCreateDatabaseIfModelChanges<GenesisContext>
    {
        protected override void Seed(GenesisContext context)
        {
            var m = new Species()
            {
                Name = "Mus muscuslus musculus"
            };
            
            context.Species.Add(m);

            var d = new Species()
            {
                Name = "Mus muscuslus domesticus"
            };

            context.Species.Add(d);

            context.Chromosomes.Add(new MtDNAChromosome()
            {
                Name = "MtDNA"
            });

            context.Chromosomes.Add(new XChromosome()
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

            autosomalChrom.Genes.Add(btk);

            btk.Alleles.Add(new Allele()
            {
                Value = "d",
                Species = d
            });

            btk.Alleles.Add(new Allele()
            {
                Value = "m",
                Species = m
            });
        }
    }
}
