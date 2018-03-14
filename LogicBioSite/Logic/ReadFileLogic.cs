﻿using ExcelDataReader;
using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System;
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
        IList<AmplificationData> AmplificationData(string file);

        /// <summary>
        /// Wczytanie danych otrzymanych ze sekwencjonatora
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        IList<AmplificationData> AmplificationDataSekwencjonator(string file);

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
        public IList<AmplificationData> AmplificationData(string file)
        {
            var source = File.ReadLines(file).Select(line => line.Split(';'));
            IList<AmplificationData> data = source.Skip(1).Select(p => new AmplificationData { Well = p[0], Cycle = p[1], TargetName = p[2], Rn = p[3], ΔRn = p[4] }).ToList();
            
            return data;
        }

        /// <summary>
        /// Wczytanie danych otrzymanych ze sekwencjonatora
        /// </summary>
        /// <param name="file">sciezka do pliku pobranego do folderu upload wewnatrz aplikacji</param>
        /// <returns>Zwraca wczytane dane zgodne z modelem AmplificationData</returns>
        public IList<AmplificationData> AmplificationDataSekwencjonator(string file)
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
                return $"Wystapił problem ({e}), dane nie zostały zapisane w bazie";
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
    }
}
