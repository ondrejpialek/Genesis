namespace Genesis.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alleles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false),
                        Gene_Id = c.Int(nullable: false),
                        Species_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Genes", t => t.Gene_Id, cascadeDelete: true)
                .ForeignKey("dbo.Species", t => t.Species_Id, cascadeDelete: true)
                .Index(t => t.Gene_Id)
                .Index(t => t.Species_Id);
            
            CreateTable(
                "dbo.Genes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        StartBasePair = c.Int(nullable: false),
                        Chromosome_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chromosomes", t => t.Chromosome_Id, cascadeDelete: true)
                .Index(t => t.Chromosome_Id);
            
            CreateTable(
                "dbo.Chromosomes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Species",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Mice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Sex = c.Int(nullable: false),
                        Locality_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Localities", t => t.Locality_Id, cascadeDelete: true)
                .Index(t => t.Locality_Id);
            
            CreateTable(
                "dbo.Localities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Code = c.String(nullable: false),
                        Location = c.Geography(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AlleleRecords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Allele_Id = c.Int(nullable: false),
                        Mouse_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Alleles", t => t.Allele_Id, cascadeDelete: true)
                .ForeignKey("dbo.Mice", t => t.Mouse_Id, cascadeDelete: true)
                .Index(t => t.Allele_Id)
                .Index(t => t.Mouse_Id);
            
            CreateTable(
                "dbo.FrequencyAnalysis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Analyzed = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Frequencies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.Double(nullable: false),
                        SampleSize = c.Int(nullable: false),
                        Locality_Id = c.Int(nullable: false),
                        FrequencyAnalysis_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Localities", t => t.Locality_Id, cascadeDelete: true)
                .ForeignKey("dbo.FrequencyAnalysis", t => t.FrequencyAnalysis_Id, cascadeDelete: true)
                .Index(t => t.Locality_Id)
                .Index(t => t.FrequencyAnalysis_Id);
            
            CreateTable(
                "dbo.FstAnalysis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Analyzed = c.DateTime(nullable: false),
                        Fst = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Frequencies", "FrequencyAnalysis_Id", "dbo.FrequencyAnalysis");
            DropForeignKey("dbo.Frequencies", "Locality_Id", "dbo.Localities");
            DropForeignKey("dbo.AlleleRecords", "Mouse_Id", "dbo.Mice");
            DropForeignKey("dbo.AlleleRecords", "Allele_Id", "dbo.Alleles");
            DropForeignKey("dbo.Mice", "Locality_Id", "dbo.Localities");
            DropForeignKey("dbo.Genes", "Chromosome_Id", "dbo.Chromosomes");
            DropForeignKey("dbo.Alleles", "Species_Id", "dbo.Species");
            DropForeignKey("dbo.Alleles", "Gene_Id", "dbo.Genes");
            DropIndex("dbo.Frequencies", new[] { "FrequencyAnalysis_Id" });
            DropIndex("dbo.Frequencies", new[] { "Locality_Id" });
            DropIndex("dbo.AlleleRecords", new[] { "Mouse_Id" });
            DropIndex("dbo.AlleleRecords", new[] { "Allele_Id" });
            DropIndex("dbo.Mice", new[] { "Locality_Id" });
            DropIndex("dbo.Genes", new[] { "Chromosome_Id" });
            DropIndex("dbo.Alleles", new[] { "Species_Id" });
            DropIndex("dbo.Alleles", new[] { "Gene_Id" });
            DropTable("dbo.FstAnalysis");
            DropTable("dbo.Frequencies");
            DropTable("dbo.FrequencyAnalysis");
            DropTable("dbo.AlleleRecords");
            DropTable("dbo.Localities");
            DropTable("dbo.Mice");
            DropTable("dbo.Species");
            DropTable("dbo.Chromosomes");
            DropTable("dbo.Genes");
            DropTable("dbo.Alleles");
        }
    }
}
