using LogicBioSite.Models.CalculateCts;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LogicBioSite.Logic
{
    public interface ICalculateCtLogic
    {
        /// <summary>
        /// Metoda obliczajaca wartosc linii progowej
        /// </summary>
        /// <param name="rMax">minimalna wartosc jaka musi osiagac pomiar w danym dolku</param>
        /// <param name="data">wartosci pomiarow z dolka</param>
        /// <returns>Zwraca obliczona wartosc linii progowej</returns>
        Tuple<List<string>, List<List<double>>, List<List<double>>> CalculateThreshold(double rMax, IEnumerable<AmplificationData> data);

        /// <summary>
        /// Metoda przewiduje punkty pomeidzy punktem a  i b w ilosci podanej przez uzytkownika
        /// </summary>
        /// <param name="a">pierwszy punkt</param>
        /// <param name="b">drugi punkt</param>
        /// <param name="count">ilosc punktow przewidywanych pomiedzy punktem a  i b</param>
        /// <returns>Zwraca liste predykowanych punktow</returns>
        IList<Tuple<Double, Double>> PointsPrediction(Tuple<Double, Double> a, Tuple<Double, Double> b, ulong count);

        /// <summary>
        /// Metoda obliczajaca wartosci Ct
        /// </summary>
        /// <param name="rMax">wartosc od ktorej musi byc wieksza maksymalna wartosc pomiaru w dolku aby zostaly dla niej wykonane obliczenia</param>
        /// <param name="predicted">ilosc punktow przewidywanych miedzy punktem mniejszym i wiekszym od wartosci Th na wykresie</param>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca obliczone dane</returns>
        CtViewModel CalculateCt(double rMax, ulong predicted, IEnumerable<AmplificationData> data);

        /// <summary>
        /// Metoda przeliczajaca Ct dla wybranego dolka na nowo. Dodatkowo zachowuje poprzedni stan danych przed przeliczeniem.
        /// </summary>
        /// <param name="lessX">mniejsza wartosc z osi X</param>
        /// <param name="greaterX">wieksza wartosc z osi X</param>
        /// <param name="lessY">mniejsza wartosc z osi Y</param>
        /// <param name="greaterY">wieksza wartosc z osi Y</param>
        /// <param name="treshold">wartosc linii progowej</param>
        /// <param name="predicted">ilosc liczb ktora zostanie przewidziana pomiedzy 2ma punktami</param>
        /// <returns>Zwraca nowo przeliczona wartosc Ct</returns>
        double ReCalculateCt(double lessX, double greaterX, double lessY, double greaterY, double treshold, ulong predicted);

        /// <summary>
        /// Metoda przeliczajaca na nowo wartosci Ct oraz deltaCt dla calych danych
        /// </summary>
        /// <param name="data">dane z obliczonymi wartosciami Ct</param>
        /// <param name="mean">srednia wartosc Ct</param>
        /// <returns>Zwraca przeliczone wartosci dla calych danych</returns>
        CtViewModel CalculateΔCtsMeanCts(CtViewModel data, double mean);

        /// <summary>
        /// Metoda ustawiajaca nazwy dla poszczegolnych dolkow
        /// </summary>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca dane ze zmienionymi nazwami</returns>
        CtViewModel SetMiRnames(CtViewModel data);
    }

    public class CalculateCtLogic : ICalculateCtLogic
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private ReadFileLogic _ReadFileLogic = new ReadFileLogic();

        /// <summary>
        /// Metoda obliczajaca wartosc linii progowej
        /// </summary>
        /// <param name="rMax">minimalna wartosc jaka musi osiagac pomiar w danym dolku</param>
        /// <param name="data">wartosci pomiarow z dolka</param>
        /// <returns>Zwraca obliczona wartosc linii progowej</returns>
        public Tuple<List<string>, List<List<double>>, List<List<double>>> CalculateThreshold(double rMax, IEnumerable<AmplificationData> data)
        {
            var uniqueWellNames = new List<string>();
            foreach (var item in data)
            {
                if (!uniqueWellNames.Contains(item.Well))
                {
                    uniqueWellNames.Add(item.Well);
                }
            }

            var dataSamples = new List<List<double>>();
            foreach (var item in uniqueWellNames)
            {
                var dataΔRn = data.Where(n => n.Well == item).Select(p => double.Parse(p.ΔRn)).ToList();
                dataSamples.Add(dataΔRn);
            }

            var names = new List<string>();
            var result = new List<List<double>>();
            var plotData = new List<List<double>>();
            int index = 0;
            foreach (var item in dataSamples)
            {
                double avg = item.Take(10).Average();
                double maxValue = item.Max();
                //stand. dev.
                var sd = Math.Sqrt(item.Take(10).Average(v => Math.Pow(v - avg, 2)));
                //threshold M = Rnoise * sqrt(Rmax / Rnoise)
                double M = sd * Math.Sqrt(maxValue / sd);
                var yAxisLessThanM = item.TakeWhile(p => p < M).Any() ? item.TakeWhile(p => p < M).Last() : 0;
                var xAxisLessThanM = yAxisLessThanM > 0 ? item.IndexOf(yAxisLessThanM) + 1 : 0;
                var yAxisGreaterThanM = item.SkipWhile(p => p <= M).Any() ? item.SkipWhile(p => p <= M).First() : 0;
                var xAxisGreaterThanM = yAxisGreaterThanM > 0 ? item.IndexOf(yAxisGreaterThanM) + 1 : 0;
                if (maxValue >= rMax)
                {
                    names.Add(uniqueWellNames[index]);
                    plotData.Add(item);
                    result.Add(new List<double>()
                    {
                        yAxisLessThanM,
                        xAxisLessThanM,
                        yAxisGreaterThanM,
                        xAxisGreaterThanM,
                        M
                    });
                }
                else
                {
                    names.Add(uniqueWellNames[index]);
                    plotData.Add(item);
                    result.Add(new List<double>(){M});
                }
                index++;
            }

            return new Tuple<List<string>, List<List<double>>, List<List<double>>>(names, result, plotData);
        }

        /// <summary>
        /// Metoda przewiduje punkty pomeidzy punktem a  i b w ilosci podanej przez uzytkownika
        /// </summary>
        /// <param name="a">pierwszy punkt</param>
        /// <param name="b">drugi punkt</param>
        /// <param name="count">ilosc punktow przewidywanych pomiedzy punktem a  i b</param>
        /// <returns>Zwraca liste predykowanych punktow</returns>
        public IList<Tuple<Double, Double>> PointsPrediction(Tuple<Double, Double> a, Tuple<Double, Double> b, ulong count)
        {
            var diffX = b.Item1 - a.Item1;
            var diffY = b.Item2 - a.Item2;
            ulong pointNum = count;

            var intervalX = diffX / (pointNum + 1);
            var intervalY = diffY / (pointNum + 1);

            IList<Tuple<Double, Double>> points = new List<Tuple<Double, Double>>();
            for (ulong i = 1; i <= pointNum; i++)
            {
                points.Add(new Tuple<Double, Double>(a.Item1 + intervalX * i, a.Item2 + intervalY * i));
            }
            //count = count + 1;

            //Double d = Math.Sqrt((a.Item1 - b.Item1) * (a.Item1 - b.Item1) + (a.Item2 - b.Item2) * (a.Item2 - b.Item2)) / count;
            //Double fi = Math.Atan2(b.Item2 - a.Item2, b.Item1 - a.Item1);

            //IList<Tuple<Double, Double>> points = new List<Tuple<Double, Double>>();

            //for (ulong i = 0; i <= count; ++i)
            //    points.Add(new Tuple<Double, Double>(a.Item1 + i * d * Math.Cos(fi), a.Item2 + i * d * Math.Sin(fi)));

            return points;
        }

        /// <summary>
        /// Metoda obliczajaca wartosci Ct
        /// </summary>
        /// <param name="rMax">wartosc od ktorej musi byc wieksza maksymalna wartosc pomiaru w dolku aby zostaly dla niej wykonane obliczenia</param>
        /// <param name="predicted">ilosc punktow przewidywanych miedzy punktem mniejszym i wiekszym od wartosci Th na wykresie</param>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca obliczone dane</returns>
        public CtViewModel CalculateCt(double rMax, ulong predicted, IEnumerable<AmplificationData> data)
        {
            var result = new List<CalculateCt>();
            var cts = new List<double>();
            var lowCts = new List<double>();
            var values = CalculateThreshold(rMax, data);
            var index = 0;
            foreach (var item in values.Item2)
            {
                if (item.Count > 1)
                {
                    IList<Tuple<Double, Double>> points = PointsPrediction(new Tuple<Double, Double>(item[0], item[1]),
                        new Tuple<Double, Double>(item[2], item[3]), predicted); //1mln
                    foreach (var point in points)
                    {
                        if (point.Item1.ToString("F2").Equals(item[4].ToString("F2")))
                        {
                            cts.Add(point.Item2);
                        }
                    }
                    if (cts.Count.Equals(0) && points.Any(p => p.Item1.ToString("F1").Equals(item[4].ToString("F1"))))
                    {
                        lowCts.Add(points.First(p => p.Item1.ToString("F1").Equals(item[4].ToString("F1"))).Item2);
                    }
                    else
                    {
                        lowCts.Add(0);
                    }
                    result.Add(new CalculateCt()
                    {
                        Well = values.Item1[index],
                        Ct = cts.Count > 0 ? cts.Average() : lowCts.First(),
                        ThresholdValue = item[4],
                        readValues = values.Item3[index].ToArray()
                    });
                    cts = new List<double>();
                    lowCts = new List<double>();
                    index++;
                }
                else
                {
                    result.Add(new CalculateCt()
                    {
                        Well = values.Item1[index],
                        Ct = 0,
                        ThresholdValue = item[0],
                        readValues = values.Item3[index].ToArray()
                    });
                    cts = new List<double>();
                    lowCts = new List<double>();
                    index++;
                }
                
            }

            var meanCt = result.Where(p => p.Ct > 0).Select(p => p.Ct).Average();
            var calculatedData = new CtViewModel() { Cts = result, Mean = meanCt, StandardDeviation = Math.Sqrt(result.Where(p => p.Ct > 0).Select(p => p.Ct).Average(v => Math.Pow(v - result.Where(p => p.Ct > 0).Select(p => p.Ct).Average(), 2))) };
            calculatedData = CalculateΔCtsMeanCts(calculatedData, meanCt);
            calculatedData = SetMiRnames(calculatedData);

            return calculatedData;
        }

        /// <summary>
        /// Metoda przeliczajaca Ct dla wybranego dolka na nowo. Dodatkowo zachowuje poprzedni stan danych przed przeliczeniem.
        /// </summary>
        /// <param name="lessX">mniejsza wartosc z osi X</param>
        /// <param name="greaterX">wieksza wartosc z osi X</param>
        /// <param name="lessY">mniejsza wartosc z osi Y</param>
        /// <param name="greaterY">wieksza wartosc z osi Y</param>
        /// <param name="treshold">wartosc linii progowej</param>
        /// <param name="predicted">ilosc liczb ktora zostanie przewidziana pomiedzy 2ma punktami</param>
        /// <returns>Zwraca nowo przeliczona wartosc Ct</returns>
        public double ReCalculateCt(double lessX, double greaterX, double lessY, double greaterY, double treshold, ulong predicted)
        {
            var cts = new List<double>();
            var lowCts = new List<double>();
            IList<Tuple<Double, Double>> points = PointsPrediction(new Tuple<Double, Double>(lessY, lessX), new Tuple<Double, Double>(greaterY, greaterX), predicted); //1mln
            foreach (var point in points)
            {
                if (point.Item1.ToString("F2").Equals(treshold.ToString("F2")))
                {
                    cts.Add(point.Item2);
                }
            }
            if (cts.Count.Equals(0) && points.Any(p => p.Item1.ToString("F1").Equals(treshold.ToString("F1"))))
            {
                lowCts.Add(points.First(p => p.Item1.ToString("F1").Equals(treshold.ToString("F1"))).Item2);
            }
            else
            {
                lowCts.Add(0);
            }

            return cts.Count > 0 ? cts.Average() : lowCts.First();
        }

        /// <summary>
        /// Metoda przeliczajaca na nowo wartosci Ct oraz deltaCt dla calych danych
        /// </summary>
        /// <param name="data">dane z obliczonymi wartosciami Ct</param>
        /// <param name="mean">srednia wartosc Ct</param>
        /// <returns>Zwraca przeliczone wartosci dla calych danych</returns>
        public CtViewModel CalculateΔCtsMeanCts(CtViewModel data, double mean)
        {
            foreach (var item in data.Cts.Where(p => p.Ct > 0))
            {
                item.ΔCt = item.Ct - mean;
                item.meanCt = Math.Pow(2, -item.ΔCt);
            }

            return data;
        }

        /// <summary>
        /// Metoda ustawiajaca nazwy dla poszczegolnych dolkow
        /// </summary>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca dane ze zmienionymi nazwami</returns>
        public CtViewModel SetMiRnames(CtViewModel data)
        {
            if (HttpContext.Current.Session["uniqueMiRname"] != null)
            {
                var names = HttpContext.Current.Session["uniqueMiRname"] as IEnumerable<MiRname>;
                foreach (var name in data.Cts)
                {
                    name.miRname = names.First(p => p.Well.Equals(name.Well)).Name;
                }
            }

            return data;
        }
    }
}
