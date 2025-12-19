using FluentMigrator;
using Mysqlx.Crud;

namespace MatchQuest.Migrations.Migrations;

[Migration(2025121100)]
public class _2025121100: Migration {
    
    public override void Up()
    {
        Rename.Column("like_id").OnTable("reactions").To("reaction_id");
        Rename.Column("IsLike").OnTable("reactions").To("is_like");
    }

    public override void Down()
    {
        Rename.Column("reaction_id").OnTable("reactions").To("like_id");
        Rename.Column("is_like").OnTable("reactions").To("IsLike");
    }
    
}