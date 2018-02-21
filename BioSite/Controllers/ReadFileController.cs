using System;
using System.Web;
using System.Web.Mvc;
using System.IO;
using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;

namespace BioSite.Controllers
{
    public class ReadFileController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        #region Ninject
        private readonly IReadFileLogic _IReadFileLogic;
        public ReadFileController(IReadFileLogic readFileLogic)
        {
            _IReadFileLogic = readFileLogic;
        }
        #endregion

        #region Read and save to database
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReadAmplificationData(HttpPostedFileBase postedFile)
        {
            string filePath = string.Empty;

            if (postedFile != null && postedFile.ContentLength > 0)
            {    
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);
            }

            try
            {
                var data = _IReadFileLogic.AmplificationData(filePath);

                //save to db
                //_IReadFileLogic.SaveFileToDatabase(data);
                Session["csvData"] = data;
                ViewData["dataReadErrorMsg"] = "";
                return View(data);
            }
            catch (Exception e)
            {
                ViewData["dataReadErrorMsg"] = "Wystąpił błąd podczas wczytywania danych";
                return View();
            }
        }
        #endregion


    }
}
