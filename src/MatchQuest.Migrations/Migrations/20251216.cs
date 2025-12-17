using FluentMigrator;

namespace MatchQuest.Migrations.Migrations;

[Migration(20251216)]
public class _20251216 : Migration {

    public override void Up()
    {
        Alter.Column("type").OnTable("games").AsInt32().Nullable();
    }
    
    public override void Down()
    {
        Alter.Column("type").OnTable("games").AsInt32().NotNullable();
    }
}