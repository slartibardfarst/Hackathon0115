using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSupport
{
    public class PhotoIssue
    {
        public ulong ImageHash;
        public int NumListingsSharingImage;
        public int Zip;
        public string StateCode;
        public string ThumbnailUrl;
        public List<ListingImage> ListingsSharingImage;
    }
}
