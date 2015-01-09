using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageHashingTest;
using SDS.Providers.MPRRouter;
using ServiceSupport;

namespace ImageWebTool.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public static MPRRedirect mprRedirect = new MPRRedirect(ConfigurationManager.ConnectionStrings["DestDB"].ConnectionString);
        public static SqlRepository imageHashRepo = new SqlRepository(mprRedirect);
        public static ServiceUtils serviceUtils = new ServiceUtils(imageHashRepo);

        public List<PhotoIssue> issues = serviceUtils.GetPhotoIssuesForZip("NY", 11215);

        public ActionResult Index(string returnUrl)
        {
            ViewBag.Issues = issues;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult ShowDuplicateImages(int index)
        {
            var photoIssue = serviceUtils.GetPhotoIssueDetails(issues[index]);

            return null;
        }

    }
}
