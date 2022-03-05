using HotChocolate;
using HotChocolate.Types;
using System.Linq;
using MongoDB.Entities;
using UATL.MailSystem.Models.Models;


namespace UATL.Mail.GraphQL.Queries
{
    
    public class MailQuery
    {

        public async Task<List<MailModel>> GetMails()
        {
            return await DB.Find<MailModel>().ExecuteAsync();
        }
    }
}
