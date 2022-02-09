using System;
using System.Collections.Generic;
using System.Text;
using Invio.Hashing;
using System.Text.Json;
using UATL.Mail.Models.Models;

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
    public class AttachementEqualityComparer : IEqualityComparer<Attachement>
    {
        public bool Equals(Attachement x, Attachement y)
        {
            return x.MD5 == y.MD5 && x.FileSize == y.FileSize;
        }

        public int GetHashCode(Attachement obj)
        {
            unchecked
            {
                return Invio.Hashing.HashCode.From(obj.MD5, obj.FileSize);
            }
        }
    }
}
