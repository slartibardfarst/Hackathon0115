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
        public ulong ImageHash;
        public string AddressLine;
        public string City;
        public string State;
        public string Zip;
        public List<string> OtherListingImages;
    }
}
