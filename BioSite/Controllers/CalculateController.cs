using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BioSite.Controllers
{
    public class CalculateController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        #region Ninject
        private readonly ICalculateCtLogic _ICalculateCtLogic;
        public CalculateController(ICalculateCtLogic calculateCtLogic)
        {
            _ICalculateCtLogic = calculateCtLogic;
        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CalculateCt()
        {
            try
            {
                var data = _ICalculateCtLogic.CalculateCt();

                return View(data);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public ActionResult CalculateCtWithoutSaveData()
        {
            try
            {
                var data = _ICalculateCtLogic.CalculateCt(Session["userCurrentData"] as IEnumerable<AmplificationData>);

                return View("CalculateCt", data);
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}