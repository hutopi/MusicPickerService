namespace MusicPickerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AlbumArtwork : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Albums", "Artwork", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Albums", "Artwork");
        }
    }
}
