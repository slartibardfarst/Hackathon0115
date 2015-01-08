using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

            int bufferSize = int.Parse(ConfigurationManager.AppSettings["MessageBufferSize"]);
            var messageQueue = new BlockingCollection<ListingImageDetails>(bufferSize);
            var tasks = StartProcessingThreads(messageQueue);

            //var listingImages = _dataSourceRepo.QueryListingImages(3, 100000, null, "WHERE l.state_code = 'NY' AND l.listing_status_id = 6 and l.zip = '11215'");
            var listingImages = _dataSourceRepo.QueryListingImages(3, 100000, null, "WHERE l.state_code = 'NY' AND l.listing_status_id = 6");
            foreach (var image in listingImages)
            {
                //LogImageHashToDatabase(image);
                messageQueue.Add(image);

                count++;
                if (count % 100 == 0) Console.Write(".");
                if (count % 1000 == 0) Console.WriteLine();
            }

            messageQueue.CompleteAdding();
            Task.WaitAll(tasks);

        }


        private Task[] StartProcessingThreads(BlockingCollection<ListingImageDetails> imagesQueue)
        {
            List<Task> tasksList = new List<Task>();

            int numTasks = int.Parse(ConfigurationManager.AppSettings["NumProcessingThreads"]);
            for (int i = 0; i < numTasks; i++)
                tasksList.Add(Task.Factory.StartNew(ProcessImagesTaskProc, new object[] { (object)imagesQueue }));

            return tasksList.ToArray();
        }

        private void ProcessImagesTaskProc(object obj)
        {
            bool keepProcessing = true;

            object[] paramsArray = (object[])obj;
            var messageQueue = (BlockingCollection<ListingImageDetails>)paramsArray[0];

            while (keepProcessing)
            {
                try
                {
                    var imageDetails = messageQueue.Take();
                    LogImageHashToDatabase(imageDetails);
                }
                catch (InvalidOperationException)
                {
                    // An InvalidOperationException means that Take() was called on a completed collection
                    keepProcessing = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ignoring unexpected exception in ProcessListings: {0}", ex.Message);
                }
            }
        }

        private void LogImageHashToDatabase(ListingImageDetails imageDetails)
        {
            try
            {
                var originalImageUrl = Regex.Replace(imageDetails.ImageUrl, "(.*)(s)(.jpg)$", "$1O$3");

                imageDetails.ImageUrl = originalImageUrl;
                imageDetails.ImageHash = GetHashFromUrl(originalImageUrl);
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
