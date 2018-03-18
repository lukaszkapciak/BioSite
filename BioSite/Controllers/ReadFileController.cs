using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;
using System.Collections.Generic;
using System.Linq;
using LogicBioSite.Models.ReadFile;
using Microsoft.Ajax.Utilities;
using WebGrease.Css.Extensions;

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
        /// <param name="postedFileNames">wybrany plik z nazwami</param>
        /// <param name="type">(bool) plik ze sekwencjonatora lub zwykly</param>
        /// <returns>Zwraca odczytane dane, zbindowane do modelu i gotowe do obliczen</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReadAmplificationData(HttpPostedFileBase postedFile, HttpPostedFileBase postedFileNames, bool type = false)
        {
            try
            {
                var filePath = _IReadFileLogic.SaveFileInApplicationUploads(postedFile);

                IEnumerable<AmplificationData> data;
                if (type)
                {
                    data = _IReadFileLogic.AmplificationDataStepOne(filePath);
                    ViewData["dataReadMsg"] = new[] { "success", "Data loaded successfully (StepOne qpcr)" };
                    if (postedFileNames != null && postedFileNames.ContentLength > 0)
                    {
                        var fileNamesPath = _IReadFileLogic.SaveFileInApplicationUploads(postedFileNames);
                        data = _IReadFileLogic.ChangeNames(fileNamesPath, data);
                    }
                }
                else
                {
                    data = _IReadFileLogic.AmplificationData(filePath);
                    ViewData["dataReadMsg"] = new[] { "success", "Data loaded successfully" };
                }
                //save to db (prawdopodobnie zapis bedzie zmieniony i wystapi jako osobna logika/metody)
                //_IReadFileLogic.SaveFileToDatabase(data);
                Session["userCurrentData"] = data;

                return View(data);
            }
            catch (Exception)
            {
                ViewData["dataReadMsg"] = new[] { "warning", $"An error occurred while loading data" };
                return View();
            }
        }
        #endregion
    }
}
