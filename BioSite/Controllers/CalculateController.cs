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

        /// <summary>
        /// Widok formularza dla obliczen Ct
        /// </summary>
        /// <returns></returns>
        public ActionResult CalculateCtForm()
        {
            return View();
        }

        /// <summary>
        /// Metoda obliczajaca wartosci Ct
        /// </summary>
        /// <param name="rMax">wartosc od ktorej musi byc wieksza maksymalna wartosc pomiaru w dolku aby zostaly dla niej wykonane obliczenia</param>
        /// <param name="predicted">ilosc punktow przewidywanych miedzy punktem mniejszym i wiekszym od wartosci Th na wykresie</param>
        /// <returns>Zwraca obliczone dane</returns>
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

        /// <summary>
        /// Aktualnie obliczone dane
        /// </summary>
        /// <returns>Zwraca dane ktore zostaly wczesniej przeliczone</returns>
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

        /// <summary>
        /// Metoda przeliczajaca Ct dla wybranego dolka na nowo. Dodatkowo zachowuje poprzedni stan danych przed przeliczeniem.
        /// </summary>
        /// <param name="newCtWell">nr plytki</param>
        /// <param name="newCtMiRname">nazwa</param>
        /// <param name="newCtInputId">id inputa do ktorego przekazywana jest przeliczona wartosc</param>
        /// <param name="lessX">mniejsza wartosc z osi X</param>
        /// <param name="greaterX">wieksza wartosc z osi X</param>
        /// <param name="lessY">mniejsza wartosc z osi Y</param>
        /// <param name="greaterY">wieksza wartosc z osi Y</param>
        /// <param name="treshold">wartosc linii progowej</param>
        /// <param name="predicted">ilosc liczb ktora zostanie przewidziana pomiedzy 2ma punktami</param>
        /// <returns>Zwraca nowo przeliczona wartosc Ct</returns>
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

        /// <summary>
        /// Metoda przeliczajaca na nowo wartosci Ct oraz deltaCt dla calych danych
        /// </summary>
        /// <returns>Zwraca przeliczone wartosci dla calych danych</returns>
        public ActionResult ReCalculateCtsDeltaCtsmeanCts()
        {
            var data = Session["CalculatedCtsΔCtsmeanCts"] as CtViewModel;

            if (data != null)
            {
                var meanCts = data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average();
                var stnDev = Math.Sqrt(data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average(v => Math.Pow(v - data.Cts.Where(p => p.Ct > 0).Select(p => p.Ct).Average(), 2)));
                data = _ICalculateCtLogic.CalculateΔCtsMeanCts(data, meanCts);
                data.Mean = meanCts;
                data.StandardDeviation = stnDev;
            }

            Session["CalculatedCtsΔCtsmeanCts"] = data;

            return View("CalculateCt", data);
        }

        /// <summary>
        /// Metoda wczytujaca stan przed przeliczeniem
        /// </summary>
        /// <returns>Zwraca dane w stanie przed przelcizeniem na nowo wartosci Ct</returns>
        public ActionResult ReadPreviousState()
        {
            var data = Session["CalculatedCtsΔCtsmeanCtsPreviousState"] as CtViewModel;
            Session["CalculatedCtsΔCtsmeanCts"] = data;
            return View("CalculateCt", data);
        }
    }
}