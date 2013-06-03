namespace Genesis.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixAlleleRecordFKs : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.AlleleRecords", new[] { "Id" });
            AddPrimaryKey("dbo.AlleleRecords", new[] { "Id", "Mouse_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.AlleleRecords", new[] { "Id", "Mouse_Id" });
            AddPrimaryKey("dbo.AlleleRecords", "Id");
        }
    }
}
