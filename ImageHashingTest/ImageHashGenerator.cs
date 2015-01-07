using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
            var listingImages = _dataSourceRepo.QueryListingImages(3, null, "WHERE l.state_code = 'NY' AND l.listing_status_id = 6");
            foreach (var image in listingImages)
            {
                image.ImageHash = ImageHashing.ImageHashing.AverageHash(new Uri(image.ImageUrl));
                _dataDestRepo.UpsertListingImageDetails(3, image);
            }
        }
    }
}
