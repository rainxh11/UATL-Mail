using System;
using System.Collections.Generic;
using System.Text;
using Invio.Hashing;
using System.Text.Json;

namespace UATL.Mail.Models.EqualityComparer
{

    public class AccountsEqualityComparer : IEqualityComparer<List<Account>>
    {
        public bool Equals(List<Account> x, List<Account> y)
        {
            return JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y);
        }

        public int GetHashCode(List<Account> obj)
        {
            unchecked 
            {
                return Invio.Hashing.HashCode.From(obj);
            }
        }
    }
}
