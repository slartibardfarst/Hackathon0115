using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ImageHashing;

namespace ImageHashingTest
{
    class Program
    {
        //public static string diff_1 = "diff1.jpg";
        //public static string same_1 = "same1.jpg";  // My original test has same_1 and same_2 as the same image
        //public static string same_2 = "same2.jpg";  // at different resolutions, so similarity = 100%

        static void Main(string[] args)
        {
            Testit();

            //var hashGenerator = new ImageHashGenerator();
            //hashGenerator.Go();
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

            Uri uriA = new Uri("http://r.rdcpix.com/v02/c14810600-r26s.jpg");
            Uri uriB = new Uri("http://r.rdcpix.com/v01/c9df70500-r38s.jpg");

            var aa = ImageHashing.ImageHashing.AverageHash(uriA);
            var bb = ImageHashing.ImageHashing.AverageHash(uriB);
            var diff = ImageHashing.ImageHashing.Similarity(uriA, uriB);
        }
    }
}
