using System.Collections.Generic;
using System.Text.Json;

namespace UATL.MailSystem.Models.EqualityComparer
{
    public class ListEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y);
        }

        public int GetHashCode(T obj)
        {
            unchecked
            {
                return Invio.Hashing.HashCode.From(obj);
            }
        }
    }
}
