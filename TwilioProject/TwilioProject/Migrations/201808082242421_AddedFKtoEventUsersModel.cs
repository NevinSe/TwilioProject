namespace TwilioProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFKtoEventUsersModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventUsers", "AppUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.EventUsers", "AppUserId");
            AddForeignKey("dbo.EventUsers", "AppUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventUsers", "AppUserId", "dbo.AspNetUsers");
            DropIndex("dbo.EventUsers", new[] { "AppUserId" });
            DropColumn("dbo.EventUsers", "AppUserId");
        }
    }
}
