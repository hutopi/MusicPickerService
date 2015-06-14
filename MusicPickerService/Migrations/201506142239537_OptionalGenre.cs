namespace MusicPickerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OptionalGenre : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tracks", "GenreId", "dbo.Genres");
            DropIndex("dbo.Tracks", new[] { "GenreId" });
            AlterColumn("dbo.Tracks", "GenreId", c => c.Int());
            CreateIndex("dbo.Tracks", "GenreId");
            AddForeignKey("dbo.Tracks", "GenreId", "dbo.Genres", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tracks", "GenreId", "dbo.Genres");
            DropIndex("dbo.Tracks", new[] { "GenreId" });
            AlterColumn("dbo.Tracks", "GenreId", c => c.Int(nullable: false));
            CreateIndex("dbo.Tracks", "GenreId");
            AddForeignKey("dbo.Tracks", "GenreId", "dbo.Genres", "Id", cascadeDelete: true);
        }
    }
}
