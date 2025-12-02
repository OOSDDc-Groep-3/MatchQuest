using MatchQuest.Core.Data.Helpers;
using MySql.Data.MySqlClient;
using System.Data;

namespace MatchQuest.Core.Data
{
    public abstract class DatabaseConnection : IDisposable
    {
        protected MySqlConnection Connection { get; }
        string connectionString;

        public DatabaseConnection()
        {
            // Use the config key that contains your MySQL connection string (appsettings.json uses "DefaultConnection")
            var connStr = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connStr))
                throw new InvalidOperationException("MySQL connection string not found in configuration (key: DefaultConnection).");

            connectionString = connStr;
            Connection = new MySqlConnection(connectionString);
        }

        protected void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();
        }

        protected void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed) Connection.Close();
        }

        public void InsertMultipleWithTransaction(List<string> linesToInsert)
        {
            if (linesToInsert == null || linesToInsert.Count == 0) return;

            OpenConnection();
            using var transaction = Connection.BeginTransaction();
            using var command = Connection.CreateCommand();
            command.Transaction = transaction;

            try
            {
                foreach (var sql in linesToInsert)
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                try { transaction.Rollback(); } catch { /* ignore rollback errors */ }
            }
        }

        public void Dispose()
        {
            CloseConnection();
            Connection?.Dispose();
        }
    }
}
