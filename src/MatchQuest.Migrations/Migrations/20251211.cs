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
            .AsCustom("TEXT")  // of AsString(255) als je het kort wil houden
            .Nullable();
    }

    public override void Down()
    {
        // Verwijder image kolom indien migration wordt teruggedraaid
        Delete.Column("image").FromTable("games");
    }
}