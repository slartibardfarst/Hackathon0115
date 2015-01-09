using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
                var hashes = imageHashesSharedByMultipleListings.Select(i => i.ImageHash).ToArray();
                Dictionary<ulong, string> hashUrls = _imageHashRepo.QueryForFirstUrlGivenHashes(stateCode, hashes);

                foreach (var issue in imageHashesSharedByMultipleListings)
                {
                    string val;
                    if (hashUrls.TryGetValue(issue.ImageHash, out val))
                        issue.ThumbnailUrl = val;
                }
            }

            imageHashesSharedByMultipleListings.Reverse();

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
