using LogicBioSite.Models.CalculateCts;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicBioSite.Logic
{
    public interface ICalculateCtLogic
    {
        Tuple<List<string>, List<List<double>>> CalculateThreshold();
        Tuple<List<string>, List<List<double>>> CalculateThreshold(IEnumerable<AmplificationData> data);
        IList<Tuple<Double, Double>> PointsPrediction(Tuple<Double, Double> a, Tuple<Double, Double> b, ulong count);
        CtViewModel CalculateCt();
        CtViewModel CalculateCt(IEnumerable<AmplificationData> data);
    }

    public class CalculateCtLogic : ICalculateCtLogic
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private ReadFileLogic _ReadFileLogic = new ReadFileLogic();
        public Tuple<List<string>, List<List<double>>> CalculateThreshold()
        {
            var data = _ReadFileLogic.GetAmplificationData();           
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
                if (maxValue >= 40000)
                {
                    names.Add(uniqueWellNames[index]);
                    result.Add(new List<double>() { yAxisLessThanM, xAxisLessThanM, yAxisGreaterThanM, xAxisGreaterThanM, M });
                }
                index++;
            }

            return new Tuple<List<string>, List<List<double>>>(names, result);
        }

        public Tuple<List<string>, List<List<double>>> CalculateThreshold(IEnumerable<AmplificationData> data)
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
                if (maxValue >= 40000)
                {
                    names.Add(uniqueWellNames[index]);
                    result.Add(new List<double>() { yAxisLessThanM, xAxisLessThanM, yAxisGreaterThanM, xAxisGreaterThanM, M });
                }
                index++;
            }

            return new Tuple<List<string>, List<List<double>>>(names, result);
        }

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

        public CtViewModel CalculateCt()
        {
            var result = new List<CalculateCt>();
            var cts = new List<double>();
            var values = CalculateThreshold();
            var index = 0;
            foreach (var item in values.Item2)
            {
                IList<Tuple<Double, Double>> points = PointsPrediction(new Tuple<Double, Double>(item[0], item[1]), new Tuple<Double, Double>(item[2], item[3]), 10);
                foreach (var point in points)
                {
                    if (point.Item1.ToString().Contains(item[4].ToString("F2")))
                    {
                        cts.Add(point.Item2);
                    }
                }

                result.Add(new CalculateCt() { Well = values.Item1[index], Ct = cts.Count > 0 ? cts.Average() : 0, ThresholdValue = item[4] });
                cts = new List<double>();
                index++;
            }

            return new CtViewModel() { Cts = result, Mean = result.Select(p => p.Ct).Average(), StandardDeviation = Math.Sqrt(result.Select(p => p.Ct).Average(v => Math.Pow(v - result.Select(p => p.Ct).Average(), 2))) };
        }

        public CtViewModel CalculateCt(IEnumerable<AmplificationData> data)
        {
            var result = new List<CalculateCt>();
            var cts = new List<double>();
            var lowCts = new List<double>();
            var values = CalculateThreshold(data);
            var index = 0;
            foreach (var item in values.Item2)
            {
                IList<Tuple<Double, Double>> points = PointsPrediction(new Tuple<Double, Double>(item[0], item[1]), new Tuple<Double, Double>(item[2], item[3]), 400000); //1mln
                foreach (var point in points)
                {
                    if (point.Item1.ToString("F2").Equals(item[4].ToString("F2")))
                    {
                        cts.Add(point.Item2);
                    }
                    //else if (point.Item1.ToString().Contains(item[4].ToString("F1")))
                    //{
                    //    lowCts.Add(point.Item2);
                    //}
                }
                //if (cts.Count.Equals(0) && lowCts.Count.Equals(0)) lowCts.Add(0);
                result.Add(new CalculateCt() { Well = values.Item1[index], Ct = cts.Count > 0 ? cts.Average() : 0, ThresholdValue = item[4] });
                cts = new List<double>();
                //lowCts = new List<double>();
                index++;
            }

            var meanCt = result.Select(p => p.Ct).Average();
            var retur = new CtViewModel() { Cts = result, Mean = meanCt, StandardDeviation = Math.Sqrt(result.Select(p => p.Ct).Average(v => Math.Pow(v - result.Select(p => p.Ct).Average(), 2))) };
            
            foreach (var item in retur.Cts)
            {
                item.ΔCt = item.Ct - meanCt;
                item.meanCt = Math.Pow(2, -item.ΔCt);
            }

            return retur;
        }
    }
}
