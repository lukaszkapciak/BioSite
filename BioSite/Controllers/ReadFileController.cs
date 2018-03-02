using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;
using System.Collections.Generic;
using LogicBioSite.Models.ReadFile;

namespace BioSite.Controllers
{
    public class ReadFileController : Controller
    {
        #region dbContext
        private ApplicationDbContext _db = new ApplicationDbContext();
        #endregion

        #region Ninject
        private readonly IReadFileLogic _IReadFileLogic;
        public ReadFileController(IReadFileLogic readFileLogic)
        {
            _IReadFileLogic = readFileLogic;
        }
        #endregion

        #region Read Files
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Wczytywanie pliku
        /// </summary>
        /// <param name="postedFile">wybrany plik</param>
        /// <param name="type">(bool) plik ze sekwencjonatora lub zwykly</param>
        /// <returns>Zwraca odczytane dane, zbindowane do modelu i gotowe do obliczen</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReadAmplificationData(HttpPostedFileBase postedFile, bool type = false)
        {
            try
            {
                var filePath = _IReadFileLogic.SaveFileInApplicationUploads(postedFile);

                IEnumerable<AmplificationData> data;
                if (type)
                {
                    data = _IReadFileLogic.AmplificationDataSekwencjonator(filePath);
                    ViewData["dataReadMsg"] = new[] { "success", "Dane wczytano pomyślnie (sekwencjonator)" };
                }
                else
                {
                    data = _IReadFileLogic.AmplificationData(filePath);
                    ViewData["dataReadMsg"] = new[] { "success", "Dane wczytano pomyślnie" };
                }
                //save to db (prawdopodobnie zapis bedzie zmieniony i wystapi jako osobna logika/metody)
                //_IReadFileLogic.SaveFileToDatabase(data);
                Session["userCurrentData"] = data;
                return View(data);
            }
            catch (Exception e)
            {
                ViewData["dataReadMsg"] = new[] { "error", $"Wystąpił błąd ({e}) podczas wczytywania danych" };
                return View();
            }
        }
        #endregion
    }
}
