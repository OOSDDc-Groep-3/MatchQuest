using FluentMigrator;

namespace MatchQuest.Migrations.Migrations;

[Migration(2025120501)]
public class _2025120501 : Migration {

    public override void Up()
    {
        // change profile_picture column to text to allow longer URLs
        Alter.Column("profile_picture").OnTable("users").AsCustom("LONGTEXT").Nullable();
    }
    
    public override void Down()
    {
        // revert profile_picture column back to string(255)
        Alter.Column("profile_picture").OnTable("users").AsString(255).Nullable();
    }
}