using UATL.Mail.Models;
using MongoDB.Entities;
using MongoDB.Driver;

namespace UATL.Mail.Helpers
{
    public class DatabaseHelper
    {
        public static async Task InitDb(string dbName, string connectionString)
        {
            await DB.InitAsync(dbName, MongoClientSettings.FromConnectionString(connectionString));
        }
    }
}
