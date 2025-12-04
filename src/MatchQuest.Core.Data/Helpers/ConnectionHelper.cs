using Microsoft.Extensions.Configuration;

namespace MatchQuest.Core.Data.Helpers
{
    public static class ConnectionHelper
    {
        public static string? ConnectionStringValue(string name)
        {
            return "Server=localhost;Port=3306;Database=match quest;Uid=root;Pwd=password123!";
        }
    }
}
