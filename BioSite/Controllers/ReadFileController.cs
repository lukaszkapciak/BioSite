using System;
using System.Web;
using System.Web.Mvc;
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

        public ActionResult ReadCsvData()
        {
            return View();
        }

        public ActionResult ReadStepOneData()
        {
            return View();
        }

        public ActionResult ReadAmplificationData()
        {
            var data = (IList<AmplificationData>)Session["userCurrentData"] != null ? (IList<AmplificationData>)Session["userCurrentData"] : new List<AmplificationData>();
            return View(data);
        }

        /// <summary>
        /// Wczytywanie pliku
        /// </summary>
        /// <param name="postedFile">wybrany plik</param>
        /// <param name="postedFileNames">wybrany plik z nazwami</param>
        /// <param name="dataFirstLine">numer pierwszego wiersza z danymi</param>
        /// <returns>Zwraca odczytane dane, zbindowane do modelu i gotowe do obliczen</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReadCsvAmplificationData(HttpPostedFileBase postedFile, HttpPostedFileBase postedFileNames, int dataFirstLine)
        {
            try
            {
                var filePath = _IReadFileLogic.SaveFileInApplicationUploads(postedFile);
                var data = _IReadFileLogic.AmplificationDataCSV(filePath, dataFirstLine);
                ViewData["dataReadMsg"] = new[] { "success", "Data loaded successfully" };
                if (postedFileNames != null && postedFileNames.ContentLength > 0)
                {
                    var fileNamesPath = _IReadFileLogic.SaveFileInApplicationUploads(postedFileNames);
                    data = _IReadFileLogic.ChangeNames(fileNamesPath, data);
                }

                Session["userCurrentData"] = data;

                return View("ReadAmplificationData", data);
            }
            catch (Exception)
            {
                ViewData["dataReadMsg"] = new[] { "warning", $"An error occurred while loading data" };
                return View("ReadAmplificationData");
            }
        }

        /// <summary>
        /// Wczytywanie pliku
        /// </summary>
        /// <param name="postedFile">wybrany plik</param>
        /// <param name="postedFileNames">wybrany plik z nazwami</param>
        /// <returns>Zwraca odczytane dane, zbindowane do modelu i gotowe do obliczen</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReadStepOneAmplificationData(HttpPostedFileBase postedFile, HttpPostedFileBase postedFileNames)
        {
            try
            {
                var filePath = _IReadFileLogic.SaveFileInApplicationUploads(postedFile);
                var data = _IReadFileLogic.AmplificationDataStepOne(filePath);
                ViewData["dataReadMsg"] = new[] { "success", "Data loaded successfully (StepOne qpcr)" };
                if (postedFileNames != null && postedFileNames.ContentLength > 0)
                {
                    var fileNamesPath = _IReadFileLogic.SaveFileInApplicationUploads(postedFileNames);
                    data = _IReadFileLogic.ChangeNames(fileNamesPath, data);
                }

                Session["userCurrentData"] = data;

                return View("ReadAmplificationData", data);
            }
            catch (Exception)
            {
                ViewData["dataReadMsg"] = new[] { "warning", $"An error occurred while loading data" };
                return View("ReadAmplificationData");
            }
        }
        #endregion
    }
}
