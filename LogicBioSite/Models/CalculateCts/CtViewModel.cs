using System.Collections.Generic;

namespace LogicBioSite.Models.CalculateCts
{
    public class CtViewModel
    {
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public IEnumerable<CalculateCt> Cts { get; set; }
    }
}
