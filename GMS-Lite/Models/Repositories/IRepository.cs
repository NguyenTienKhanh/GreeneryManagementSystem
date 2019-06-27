using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class IRepository
    {
        public MongoClient mongoClient;
        public IMongoDatabase db;
        public IRepository()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            mongoClient = new MongoClient(config.GetConnectionString("Mongo"));
            db = mongoClient.GetDatabase("Mongo");
        }
    }
}
