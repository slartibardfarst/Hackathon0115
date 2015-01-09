using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using ImageHashing;
using SDS.Providers.MPRRouter;
using ServiceSupport;

namespace ImageHashingTest
{
    class Program
    {
        //public static string diff_1 = "diff1.jpg";
        //public static string same_1 = "same1.jpg";  // My original test has same_1 and same_2 as the same image
        //public static string same_2 = "same2.jpg";  // at different resolutions, so similarity = 100%

        static void Main(string[] args)
        {
            //Testit();
            TestServiceSupport();

            //var hashGenerator = new ImageHashGenerator();
            //hashGenerator.Go();

            //Console.WriteLine("Enter to Exit");
            //Console.ReadLine();
        }


        private static void Testit()
        {
            var same_1 = @"..\..\..\img1.jpg";  // My original test has same_1 and same_2 as the same image
            var same_2 = @"..\..\..\img2.jpg";  // at different resolutions, so similarity = 100%
            var diff_1 = @"..\..\..\img3.jpg";

            var a = @"..\..\..\l2e681455-r0s.jpg";
            var b = @"..\..\..\l2e681455-r0xd-w640_h480_q80.jpg";
            var c = @"..\..\..\l79f31455-r0xd-w640_h480_q80.jpg";
            var d = @"..\..\..\l79f31455-r0xd-w640_h480_q80_fliphoriz.jpg";
            var e = @"..\..\..\l79f31455-r0xd-w640_h480_q80_rr.jpg";



            //Console.WriteLine(String.Format("Similarity, diff-same: {0}", ImageHashing.ImageHashing.Similarity(diff_1, same_1)));
            //Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(same_1, same_2)));
            Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(a, b)));
            Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(b, c)));
            Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(a, c)));
            Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(b, d)));
            Console.WriteLine(String.Format("Similarity, same-same: {0}", ImageHashing.ImageHashing.Similarity(b, e)));

            var x = ImageHashing.ImageHashing.AverageHash(new Uri("http://r.rdcpix.com/v01/l2e681455-r0o.jpg"));

            Uri uriA = new Uri("http://r.rdcpix.com/v02/c14810600-r26o.jpg");
            Uri uriB = new Uri("http://r.rdcpix.com/v01/c14810600-r28o.jpg");

            var aa = ImageHashing.ImageHashing.AverageHash(uriA);
            var bb = ImageHashing.ImageHashing.AverageHash(uriB);
            var diff = ImageHashing.ImageHashing.Similarity(uriA, uriB);
        }

        private static void TestServiceSupport()
        {
            MPRRedirect mprRedirect = new MPRRedirect(ConfigurationManager.ConnectionStrings["DestDB"].ConnectionString);

            var imageHashRepo = new SqlRepository(mprRedirect);
            var serviceUtils = new ServiceUtils(imageHashRepo);
            var issues = serviceUtils.GetPhotoIssuesForZip("NY", 11215);

            int ii = 1;

            for (int i = 0; i < 10; i++)
                serviceUtils.PopulatePhotoIssueDetails(issues[i], false);
        }

    }
}
