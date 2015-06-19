namespace MusicPickerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MetaIndexes : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Albums", new[] { "ArtistId" });
            DropIndex("dbo.Tracks", new[] { "AlbumId" });
            DropIndex("dbo.DeviceTracks", new[] { "TrackId" });
            AlterColumn("dbo.Albums", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Artists", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Tracks", "Name", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.Albums", new[] { "Name", "ArtistId" }, name: "IX_NameAndArtist");
            CreateIndex("dbo.Artists", "Name");
            CreateIndex("dbo.Tracks", new[] { "Name", "AlbumId" }, name: "IX_NameAndAlbum");
            CreateIndex("dbo.DeviceTracks", "TrackId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DeviceTracks", new[] { "TrackId" });
            DropIndex("dbo.Tracks", "IX_NameAndAlbum");
            DropIndex("dbo.Artists", new[] { "Name" });
            DropIndex("dbo.Albums", "IX_NameAndArtist");
            AlterColumn("dbo.Tracks", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Artists", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.Albums", "Name", c => c.String(nullable: false));
            CreateIndex("dbo.DeviceTracks", "TrackId");
            CreateIndex("dbo.Tracks", "AlbumId");
            CreateIndex("dbo.Albums", "ArtistId");
        }
    }
}
