namespace MusicPickerService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackDurations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceTracks", "TrackDuration", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeviceTracks", "TrackDuration");
        }
    }
}
