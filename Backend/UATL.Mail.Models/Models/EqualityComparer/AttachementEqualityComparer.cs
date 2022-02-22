using System.Collections.Generic;
using UATL.MailSystem.Models.Models;

namespace UATL.MailSystem.Models.EqualityComparer
{
    public class AttachementEqualityComparer : IEqualityComparer<Attachment>
    {
        public bool Equals(Attachment x, Attachment y)
        {
            return string.Equals(x.MD5, y.MD5, System.StringComparison.OrdinalIgnoreCase) && x.FileSize == y.FileSize;
        }

        public int GetHashCode(Attachment obj)
        {
            unchecked
            {
                return Invio.Hashing.HashCode.From(obj.MD5, obj.FileSize);
            }
        }
    }
}
