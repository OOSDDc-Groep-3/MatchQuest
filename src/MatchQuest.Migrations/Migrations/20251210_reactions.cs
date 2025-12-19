using FluentMigrator;
using Mysqlx.Crud;

namespace MatchQuest.Migrations.Migrations;

[Migration(20251210)]
public class _20251210: Migration {
    
    public override void Up()
    {
        Rename.Table("likes").To("reactions");
        Rename.Column("from_user_id").OnTable("reactions").To("user_id");
        Rename.Column("to_user_id").OnTable("reactions").To("target_user_id");
        Alter.Table("reactions")
            .AddColumn("isLike").AsBoolean().NotNullable().WithDefaultValue(true);
    }

    public override void Down()
    {
        // Revert the changes made in Up()
        Delete.Column("isLike").FromTable("reactions");
        Rename.Column("user_id").OnTable("reactions").To("from_user_id");
        Rename.Column("target_user_id").OnTable("reactions").To("to_user_id");
        Rename.Table("reactions").To("likes");
    }
    
}