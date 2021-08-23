using System.IO;
using System.Web.Mvc;

namespace votaciones.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public virtual ActionResult DownloadManual()
        {
            string fullPath = Path.Combine(Server.MapPath("~/Content/Data/manual.pdf"));
            return File(fullPath, "application/octet-stream", "manual.pdf");
        }
    }
}