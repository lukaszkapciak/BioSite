using LogicBioSite.Models.DbContext;
using LogicBioSite.Models.ReadFile;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogicBioSite.Logic
{
    public interface IReadFileLogic
    {
        IEnumerable<AmplificationData> AmplificationData(string file);

        void SaveFileToDatabase(IEnumerable<AmplificationData> data);

        IEnumerable<AmplificationData> GetAmplificationData();
    }
    public class ReadFileLogic : IReadFileLogic
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        public IEnumerable<AmplificationData> AmplificationData(string file)
        {
            var source = File.ReadLines(file).Select(line => line.Split(';'));
            IEnumerable<AmplificationData> data = source.Skip(8).Select(p => new AmplificationData { Well = p[0], Cycle = p[1], TargetName = p[2], Rn = p[3], ΔRn = p[4] });
            return data;
        }

        public void SaveFileToDatabase(IEnumerable<AmplificationData> data)
        {
            _db.AmplificationData.AddRange(data);
            _db.SaveChanges();
        }

        public IEnumerable<AmplificationData> GetAmplificationData()
        {
            return _db.AmplificationData.AsEnumerable();
        }
    }
}
