using UATL.MailSystem.Models;
using MongoDB.Entities;
using MongoDB.Driver;
using UATL.MailSystem.Models.Models;
using MongoDB;
using Akavache;
using Akavache.Sqlite3;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Compression;

namespace UATL.MailSystem.Helpers
{
    public class DatabaseHelper
    {
        public static async Task InitDb(string dbName, string connectionString)
        {
            var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
            /*clientSettings.Compressors = new List<CompressorConfiguration>() 
            {
                new CompressorConfiguration(CompressorType.ZStandard)
            };*/
            clientSettings.ApplicationName = "UATL Mail Server";

            await DB.InitAsync(dbName, clientSettings);
            await CreateIndices();
        }
        public static void InitCache()
        {
            if (!Directory.Exists(AppContext.BaseDirectory + @"\Cache"))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + @"\Cache");
            }

            BlobCache.LocalMachine = new SqlRawPersistentBlobCache(AppContext.BaseDirectory + @"\Cache\cache.db");

            Akavache.Registrations.Start("UATL-Mail");
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
                .Key(b => b.Attachments.Select(x => x.Name), KeyType.Text)
                .CreateAsync();

            var mailIndex = DB.Index<MailModel>()
                .Key(b => b.Subject, KeyType.Text)
                .Key(b => b.Body, KeyType.Text)
                .Key(b => b.From.Name, KeyType.Text)
                .Key(b => b.From.UserName, KeyType.Text)
                .Key(b => b.To.Name, KeyType.Text)
                .Key(b => b.To.UserName, KeyType.Text)
                .Key(b => b.ID, KeyType.Text)
                .Key(b => b.From.ID, KeyType.Text)
                .Key(b => b.Attachments.Select(x => x.Name), KeyType.Text)
                .CreateAsync();

            var accountIndex = DB.Index<Account>()
                .Key(b => b.Name, KeyType.Text)
                .Key(b => b.UserName, KeyType.Text)
                .Key(b => b.ID, KeyType.Text)
                .Key(b => b.Description, KeyType.Text)
                .CreateAsync();

            await DB.Index<Account>()
                .Key(b => b.UserName, KeyType.Ascending)
                .Option(x => x.Unique = true)
                .CreateAsync();



            await Task.WhenAll(draftIndex, accountIndex);
        }
    }
}
