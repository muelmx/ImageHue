using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHue.Model
{
    public class DB : IDB
    {
        private LiteDatabase db;
        public DB()
        {
            db = new LiteDatabase(@"data.db");
        }

        public Task Deleteconfig()
        {
            return Task.Factory.StartNew(() =>
            {
                var configs = db.GetCollection<Config>("configs");
                configs.Delete(c => true);
            });
        }

        public Task<Config> Getconfig()
        {
            return Task.Factory.StartNew<Config>(() =>
            {
                var configs = db.GetCollection<Config>("configs");
                return configs.FindOne(c => true);
            });
        }

        public Task Setconfig(Config config)
        {
            return Task.Factory.StartNew(() =>
            {
                var configs = db.GetCollection<Config>("configs");
                configs.Delete(c => true);

                configs.Insert(config);
            });
        }
    }
}
