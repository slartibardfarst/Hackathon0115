using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceSupport
{
    public class ListingImage
    {
        public ulong MprId;
        public ulong MlId;
        public int ListingId;
        public string ImageUrl;
        public List<ListingImage> OtherListingsSharingImage;
    }
}
