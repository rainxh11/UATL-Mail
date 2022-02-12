using UATL.MailSystem.Models;
using MongoDB.Entities;
using MongoDB.Driver;
using UATL.MailSystem.Models.Models;
using MongoDB;

namespace UATL.MailSystem.Helpers
{
    public class DatabaseHelper
    {
        public static async Task InitDb(string dbName, string connectionString)
        {
            await DB.InitAsync(dbName, MongoClientSettings.FromConnectionString(connectionString));
            await CreateIndices();
        }
        private static async Task CreateIndices()
        {
            var draftIndex = DB.Index<Draft>()
                .Key(b => b.Subject, KeyType.Text)
                .Key(b => b.Body, KeyType.Text)
                .Key(b => b.From.Name, KeyType.Text)
                .Key(b => b.From.UserName, KeyType.Text)
                .Key(b => b.ID, KeyType.Text)
                .Key(b => b.From.ID, KeyType.Text)
                .CreateAsync();

            var accountIndex = DB.Index<Account>()
                .Key(b => b.Name, KeyType.Text)
                .Key(b => b.UserName, KeyType.Text)
                .Key(b => b.ID, KeyType.Text)
                .CreateAsync();

            await DB.Index<Account>()
                .Key(b => b.UserName, KeyType.Ascending)
                .Option(x => x.Unique = true)
                .CreateAsync();


            await Task.WhenAll(draftIndex, accountIndex);
        }
    }
}
