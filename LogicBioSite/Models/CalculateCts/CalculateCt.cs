using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicBioSite.Models.CalculateCts
{
    public class CalculateCt
    {
        public int id { get; set; }
        public string Well { get; set; }
        public string miRname { get; set; }
        public double ThresholdValue { get; set; }
        public double Ct { get; set; }
        public double ΔCt { get; set; }
        public double meanCt { get; set; }
        public double[] readValues { get; set; }
    }
}
