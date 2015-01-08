using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ImageHashingTest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServiceSupport
{
    public class ServiceUtils
    {
        private SqlRepository _imageHashRepo;

        public ServiceUtils(SqlRepository imageHashRepo)
        {
            _imageHashRepo = imageHashRepo;
        }

        public List<PhotoIssue> GetPhotoIssuesForZip(string stateCode, int zip)
        {
            List<PhotoIssue> imageHashesSharedByMultipleListings = _imageHashRepo.QueryForSharedImagesHashes(stateCode, zip, 2, "");

            return imageHashesSharedByMultipleListings;
        }

        public PhotoIssue GetPhotoIssueDetails(PhotoIssue issue)
        {
            var result = new List<PhotoIssue>();

            List<ListingImage> listingsSharingImage = _imageHashRepo.GetListingsSharingImageHash(issue.StateCode, issue.ImageHash);
            foreach (var listing in listingsSharingImage)
            {
                listing.OtherListingImages = _imageHashRepo.GetAllImagesForListing(listing.ListingId, issue.StateCode);
            }

            issue.ListingsSharingImage = listingsSharingImage;

            return issue;
        }


        public string GetPhotoIssuesForZipAsJsonString(string stateCode, int zip)
        {
            var obj = GetPhotoIssuesForZip(stateCode, zip);
            var result = JsonConvert.SerializeObject(obj);
            return result;
        }


    }
}
