namespace MusicPickerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class YearField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Albums", "Year", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Albums", "Year");
        }
    }
}
