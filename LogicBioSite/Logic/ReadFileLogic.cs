using ExcelDataReader;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace LogicBioSite.Logic
{
    /// <summary>
    /// Interface zawierajacy metody wczytujace dane
    /// </summary>
    public interface IReadFileLogic
    {
        /// <summary>
        /// Zapisuje wybrany plik do folderu Uploads wewnatrz aplikacji
        /// </summary>
        /// <returns>Zwraca sciezke do pliku juz wewnatrz aplikacji</returns>
        string SaveFileInApplicationUploads(HttpPostedFileBase postedFile);

        /// <summary>
        /// Wczytuje dane z dowolnego pliku (1karta) 5 kolumnowego
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        IList<AmplificationData> AmplificationDataCSV(string file, int dataFirstLine);

        /// <summary>
        /// Wczytanie danych otrzymanych ze sekwencjonatora
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        IList<AmplificationData> AmplificationDataStepOne(string file);

        /// <summary>
        /// Zapis wczytanych danych do bazy danych
        /// </summary>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca komunikat o statusie operacji</returns>
        string SaveFileToDatabase(IEnumerable<AmplificationData> data);

        /// <summary>
        /// Pobranie danych z bazy danych
        /// </summary>
        /// <returns>Zwraca dane z bazy danych zgodne z modelem AmplificationData</returns>
        IEnumerable<AmplificationData> GetAmplificationData();

        /// <summary>
        /// Dodaje nazwy dla poszczególnych dołków z pliku zewnetrznego
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <param name="data">plik z aktualnie wczytanymi danymi</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        IList<AmplificationData> ChangeNames(string file, IEnumerable<AmplificationData> data);
    }
    public class ReadFileLogic : IReadFileLogic
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        /// <summary>
        /// Zapisuje wybrany plik do folderu Uploads wewnatrz aplikacji
        /// </summary>
        /// <returns>Zwraca sciezke do pliku juz wewnatrz aplikacji</returns>
        public string SaveFileInApplicationUploads(HttpPostedFileBase postedFile)
        {
            string filePath = string.Empty;

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                string path = HttpContext.Current.Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = path + Path.GetFileName(postedFile.FileName);
                string extension = Path.GetExtension(postedFile.FileName);
                postedFile.SaveAs(filePath);
            }

            return filePath;
        }

        /// <summary>
        /// Wczytuje dane z dowolnego pliku (1karta) 5 kolumnowego
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        public IList<AmplificationData> AmplificationDataCSV(string file, int dataFirstLine)
        {
            var source = File.ReadLines(file).Select(line => line.Split(';'));
            IList<AmplificationData> data = source.Skip(dataFirstLine-1).Select(p => new AmplificationData { Well = p[0], Cycle = p[1], TargetName = p[2], Rn = p[3], ΔRn = p[4] }).ToList();
            
            return data;
        }

        /// <summary>
        /// Wczytanie danych otrzymanych ze sekwencjonatora
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        public IList<AmplificationData> AmplificationDataStepOne(string file)
        {
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(stream);
            var result = excelReader.AsDataSet();
            var data = result.Tables[3].Rows.Cast<DataRow>().Skip(8).Select(p => new AmplificationData { Well = p.ItemArray[0].ToString(), Cycle = p.ItemArray[1].ToString(), TargetName = p.ItemArray[2].ToString(), Rn = p.ItemArray[3].ToString(), ΔRn = p.ItemArray[4].ToString() }).ToList();
            return data;
        }

        /// <summary>
        /// Zapis wczytanych danych do bazy danych
        /// </summary>
        /// <param name="data">wczytane dane</param>
        /// <returns>Zwraca komunikat o statusie operacji</returns>
        public string SaveFileToDatabase(IEnumerable<AmplificationData> data)
        {
            try
            {
                _db.AmplificationData.AddRange(data);
                _db.SaveChanges();
                return "Dane zostały pomyślnie zapisane w bazie";
            }
            catch (System.Exception e)
            {
                return $"Wystapił problem ({e.Message}), dane nie zostały zapisane w bazie";
            }
        }

        /// <summary>
        /// Pobranie danych z bazy danych
        /// </summary>
        /// <returns>Zwraca dane z bazy danych zgodne z modelem AmplificationData</returns>
        public IEnumerable<AmplificationData> GetAmplificationData()
        {
            return _db.AmplificationData.AsEnumerable();
        }

        /// <summary>
        /// Dodaje nazwy dla poszczególnych dołków z pliku zewnetrznego
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <param name="data">plik z aktualnie wczytanymi danymi</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        public IList<AmplificationData> ChangeNames(string file, IEnumerable<AmplificationData> data)
        {
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateReader(stream);
            var result = excelReader.AsDataSet();
            var miRname = result.Tables[0].Rows.Cast<DataRow>().Take(100).Skip(4).Select(p => new MiRname
            { Well = p.ItemArray[3].ToString()[1] != '0' ? p.ItemArray[3].ToString() : $"{p.ItemArray[3].ToString()[0]}{p.ItemArray[3].ToString()[2]}"
                , Name = p.ItemArray[0].ToString() });
            var dataAsList = data.ToList();
            foreach (var name in dataAsList)
            {
                name.miRname = miRname.First(p => p.Well.Equals(name.Well)).Name;
            }

            HttpContext.Current.Session["uniqueMiRname"] = miRname;
            return dataAsList;
        }
    }
}
