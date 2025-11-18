using FluentMigrator;

namespace MatchQuest.Migrations.Migrations;

/*
 * <summary>
 * This is an example Migration for MatchQuest. It creates a new table called "Matches" with a single column "Id" of type GUID.
 * </summary>
 */
[Migration(20251118)]
public class _20251118: Migration {
    public override void Up()
    {
        Create.Table("Matches")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey();
    }

    public override void Down() {
        Delete.Table("Matches");
    }
}