namespace LogicBioSite.Models.CalculateCts
{
    public class CalculateCt
    {
        public int id { get; set; }
        public string Well { get; set; }
        public string miRname { get; set; }
        public double ThresholdValue { get; set; }
        public double Ct { get; set; }
        public double ΔΔCt { get; set; }
        public double R { get; set; }
        public double[] readValues { get; set; }
    }
}
