using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public class Db : IDB
    {
        private readonly LiteDatabase _db;
        public Db()
        {
            _db = new LiteDatabase(@"data.db");
        }

        public Task Deleteconfig()
        {
            return Task.Factory.StartNew(() =>
            {
                var configs = _db.GetCollection<Config>("configs");
                configs.Delete(c => true);
            });
        }

        public Task<Config> Getconfig()
        {
            return Task.Factory.StartNew<Config>(() =>
            {
                var configs = _db.GetCollection<Config>("configs");
                return configs.FindOne(c => true);
            });
        }

        public Task Setconfig(Config config)
        {
            return Task.Factory.StartNew(() =>
            {
                var configs = _db.GetCollection<Config>("configs");
                configs.Delete(c => true);

                configs.Insert(config);
            });
        }
    }
}
