using System.IO;
using System.Web.Mvc;

namespace BioSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Metoda zwracajaca przykładowe dane dla użytkownika do pierwszej analizy
        /// </summary>
        /// <returns>Zwraca dane w formacie CSV</returns>
        public FileResult ExampleData()
        {
            string file = Server.MapPath("~/Content/Help/data/exampleData.csv");
            return File(file, Path.GetFileName(file));
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}