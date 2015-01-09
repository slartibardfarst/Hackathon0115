using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            if ((null != imageHashesSharedByMultipleListings) && (imageHashesSharedByMultipleListings.Count > 0))
            {
                foreach (var issue in imageHashesSharedByMultipleListings)
                {
                    var origUrl = _imageHashRepo.QueryForFirstUrlGivenHash(stateCode, issue.ImageHash);
                    issue.ThumbnailUrl = Regex.Replace(origUrl, @"^(.*)(\w)(.jpg)$", "$1s$3");
                }
            }

            return imageHashesSharedByMultipleListings;
        }


        public PhotoIssue PopulatePhotoIssueDetails(PhotoIssue issue, bool getOtherPhotosForListing = false)
        {
            List<ListingImage> listingsSharingImage = _imageHashRepo.GetListingsSharingImageHash(issue.StateCode, issue.ImageHash);

            if (getOtherPhotosForListing)
            {
                foreach (var listing in listingsSharingImage)
                {
                    listing.OtherListingImages = _imageHashRepo.GetAllImagesForListing(listing.ListingId, issue.StateCode);
                }
            }

            issue.ListingsSharingImage = listingsSharingImage;

            return issue;
        }

        public ListingImage PopulateOtherImagesForListing(ListingImage listingImage, string stateCode)
        {
            listingImage.OtherListingImages = _imageHashRepo.GetAllImagesForListing(listingImage.ListingId, stateCode);
            return listingImage;
        }
    }
}
