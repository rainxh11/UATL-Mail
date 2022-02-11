using System.Collections.Generic;
using UATL.Mail.Models.Models;

namespace UATL.Mail.Models.EqualityComparer
{
    public class AttachementEqualityComparer : IEqualityComparer<Attachement>
    {
        public bool Equals(Attachement x, Attachement y)
        {
            return string.Equals(x.MD5, y.MD5, System.StringComparison.OrdinalIgnoreCase) && x.FileSize == y.FileSize;
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
