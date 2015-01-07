using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using SDS.Providers.MPRRouter;

namespace ImageHashingTest
{
    public class ImageHashGenerator
    {
        private MPRRedirect _dataSourceMprRedirect;
        private MPRRedirect _dataDestMprRedirect;
        private SqlRepository _dataSourceRepo;
        private SqlRepository _dataDestRepo;


        public ImageHashGenerator()
        {
            _dataSourceMprRedirect = new MPRRedirect(ConfigurationManager.ConnectionStrings["SourceDB"].ConnectionString);
            _dataDestMprRedirect = new MPRRedirect(ConfigurationManager.ConnectionStrings["DestDB"].ConnectionString);

            _dataSourceRepo = new SqlRepository(_dataSourceMprRedirect);
            _dataDestRepo = new SqlRepository(_dataDestMprRedirect);
        }

        internal void Go()
        {
            int count = 0;

            var listingImages = _dataSourceRepo.QueryListingImages(3, 10000, null, "WHERE l.state_code = 'NY' AND l.listing_status_id = 6 and l.zip = '11215'");
            foreach (var image in listingImages)
            {
                LogImageHashToDatabase(image);

                count++;
                if (count % 100 == 0) Console.Write(".");
                if (count % 1000 == 0) Console.WriteLine();
            }
        }

        private void LogImageHashToDatabase(ListingImageDetails imageDetails)
        {
            try
            {
                imageDetails.ImageHash = GetHashFromUrl(imageDetails.ImageUrl);
                _dataDestRepo.UpsertListingImageDetails(3, "[MasterPropertyRecord].[dbo].[zzz_hackathon_0115_image_hashes]", imageDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: failed to get image hash for listing id {0}. Error message: {1}", imageDetails.ListingId, ex.Message);
            }
        }

        private ulong GetHashFromUrl(string imageUrl)
        {
            int retriesSoFar = 0;

            while (true)
            {
                try
                {
                    return ImageHashing.ImageHashing.AverageHash(new Uri(imageUrl));
                }
                catch (Exception)
                {
                    if (retriesSoFar++ < 3)
                        Thread.Sleep(100);
                    else
                        throw;
                }
            }
        }
    }
}
