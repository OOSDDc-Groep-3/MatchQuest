using System.Data;
using FluentMigrator;

namespace MatchQuest.Migrations.Migrations;

[Migration(20251119)]
public class _20251119: Migration {
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("user_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("password").AsString(255).NotNullable()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("birth_date").AsDate().NotNullable()
            .WithColumn("region").AsString(255).Nullable()
            .WithColumn("bio").AsString(int.MaxValue).Nullable() 
            .WithColumn("profile_picture").AsString(255).Nullable()
            .WithColumn("role").AsString(255).NotNullable()
            .WithColumn("is_active").AsBoolean().WithDefaultValue(true) 
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();
            
        Create.Table("games")
            .WithColumn("game_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("type").AsString(255).NotNullable()
            .WithColumn("approved").AsBoolean().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();

        Create.Table("user_games")
            .WithColumn("user_game_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
                .ForeignKey("FK_UserGames_Users", "users", "user_id")
            .WithColumn("game_id").AsInt32().NotNullable()
                .ForeignKey("FK_UserGames_Games", "games", "game_id")
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();


        Create.Table("likes")
            .WithColumn("like_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("from_user_id").AsInt32().NotNullable()
                .ForeignKey("FK_Likes_FromUser", "users", "user_id")
            .WithColumn("to_user_id").AsInt32().NotNullable()
                .ForeignKey("FK_Likes_ToUser", "users", "user_id")
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();


        Create.Table("matches")
            .WithColumn("match_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user1_id").AsInt32().NotNullable()
                .ForeignKey("FK_Matches_User1", "users", "user_id")
            .WithColumn("user2_id").AsInt32().NotNullable()
                .ForeignKey("FK_Matches_User2", "users", "user_id")
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();


        Create.Table("chats")
            .WithColumn("chat_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("match_id").AsInt32().NotNullable()
                .ForeignKey("FK_Chats_Matches", "matches", "match_id").OnDelete(Rule.Cascade)
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();


        Create.Table("messages")
            .WithColumn("message_id").AsInt32().PrimaryKey().Identity()
            .WithColumn("chat_id").AsInt32().NotNullable()
                .ForeignKey("FK_Messages_Chats", "chats", "chat_id").OnDelete(Rule.Cascade)
            .WithColumn("sender_id").AsInt32().NotNullable()
                .ForeignKey("FK_Messages_Sender", "users", "user_id")
            .WithColumn("message_text").AsString(int.MaxValue).NotNullable() 
            .WithColumn("created_at").AsDateTime().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("updated_at").AsDateTime().Nullable();
    }

    public override void Down() {
        Delete.Table("messages");
        Delete.Table("chats");
        Delete.Table("matches");
        Delete.Table("likes");
        Delete.Table("user_games");
        Delete.Table("games");
        Delete.Table("users");
    }
}