using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebGrease.Css.Extensions;

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

        public ActionResult CalculateCtWithoutSaveData(double rMax, double digits, ulong predicted)
        {
            try
            {
                var data = _ICalculateCtLogic.CalculateCt(rMax, digits, predicted, Session["userCurrentData"] as IEnumerable<AmplificationData>);
                return View("CalculateCt", data);
            }
            catch (Exception e)
            {

                throw;
            }
        }
    }
}