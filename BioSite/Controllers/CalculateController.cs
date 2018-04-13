using LogicBioSite.Logic;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LogicBioSite.Models.CalculateCts;

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

        //public ActionResult CalculateCt()
        //{
        //    try
        //    {
        //        var data = _ICalculateCtLogic.CalculateCt();

        //        return View(data);
        //    }
        //    catch (Exception e)
        //    {

        //        throw;
        //    }
        //}
        public ActionResult CalculateCtForm()
        {
            return View();
        }

        public ActionResult CalculateCt(double rMax, ulong predicted)
        {
            try
            {
                var data = _ICalculateCtLogic.CalculateCt(rMax, predicted, Session["userCurrentData"] as IEnumerable<AmplificationData>);
                Session["CalculatedCtsΔCtsmeanCts"] = data;
                return View("CalculateCt", data);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public ActionResult CurrentResults()
        {
            try
            {
                var data = (CtViewModel) Session["CalculatedCtsΔCtsmeanCts"];
                return View("CalculateCt", data);
            }
            catch (Exception e)
            {

                throw;
            }
        }

        [HttpPost]
        public JsonResult RecalculateCt(string newCtWell, string newCtMiRname, string newCtInputId, double lessX, double greaterX, double lessY, double greaterY, double treshold, ulong predicted)
        {
            try
            {
                var oldData = Session["CalculatedCtsΔCtsmeanCts"] as CtViewModel;
                var oldCts = oldData?.Cts.Select(item => new CalculateCt() {Ct = item.Ct, readValues = item.readValues, ThresholdValue = item.ThresholdValue, Well = item.Well, meanCt = item.meanCt, ΔCt = item.ΔCt, id = item.id, miRname = item.miRname}).ToList();
                var previousState = new CtViewModel { Cts = oldCts, Mean = oldData.Mean, StandardDeviation = oldData.StandardDeviation };
                Session["CalculatedCtsΔCtsmeanCtsPreviousState"] = previousState;
                var ctValue = _ICalculateCtLogic.ReCalculateCt(lessX, greaterX, lessY, greaterY, treshold, predicted);
                
                oldData.Cts.Single(p => p.Well.Equals(newCtWell)).Ct = ctValue;
                Session["CalculatedCtsΔCtsmeanCts"] = oldData;

                return Json(new { newCtInputId, newCtWell, newCtMiRname, ctValue = ctValue.ToString() });
            }
            catch (Exception e)
            {
                return Json(new { message = e.Message });
            }
        }

        public ActionResult ReCalculateCtsDeltaCtsmeanCts()
        {
            var data = Session["CalculatedCtsΔCtsmeanCts"] as CtViewModel;

            if (data != null)
            {
                var meanCts = data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average();
                var stnDev = Math.Sqrt(data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average(v => Math.Pow(v - data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average(), 2)));
                foreach (var item in data.Cts.Where(p => p.Ct > 0))
                {
                    item.ΔCt = item.Ct - meanCts;
                    item.meanCt = Math.Pow(2, -item.ΔCt);
                }

                data.Mean = meanCts;
                data.StandardDeviation = stnDev;
            }

            Session["CalculatedCtsΔCtsmeanCts"] = data;

            return View("CalculateCt", data);
        }

        public ActionResult ReadPreviousState()
        {
            var data = Session["CalculatedCtsΔCtsmeanCtsPreviousState"] as CtViewModel;
            Session["CalculatedCtsΔCtsmeanCts"] = data;
            return View("CalculateCt", data);
        }
    }
}