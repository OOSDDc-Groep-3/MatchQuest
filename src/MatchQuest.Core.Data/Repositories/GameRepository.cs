using System.Collections.Generic;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories;

public class GameRepository : DatabaseConnection, IGameRepository
{
    public List<Game> GetAll()
    {
        var games = new List<Game>();

        OpenConnection();
        using var cmd = Connection.CreateCommand();

        // Pas kolomnamen aan indien nodig (game_id / name)
        cmd.CommandText = @"SELECT game_id, name 
                            FROM games
                            ORDER BY name ASC;";

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            var g = new Game
            {
                Id = rdr.GetInt32("game_id"),
                Name = rdr.GetString("name")
            };

            games.Add(g);
        }

        CloseConnection();
        return games;
    }
}