using FluentMigrator;

namespace MatchQuest.Migrations.Migrations;

[Migration(20251211)]
public class _20251206 : Migration
{
    public override void Up()
    {
        // Voeg image kolom toe aan games table
        Alter.Table("games")
            .AddColumn("image")
            .AsCustom("TEXT")  
            .Nullable();
    }

    public override void Down()
    {
        Delete.Column("image").FromTable("games");
    }
}