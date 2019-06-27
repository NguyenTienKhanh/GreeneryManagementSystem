using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Area
    {
        public Area()
        {
            this.IsDeleted = false;
            this.Trees = new List<string>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("polygon")]
        public Polygon Polygon { get; set; }

        [BsonElement("center")]
        public Coordinate Center { get; set; }

        [BsonElement("farthest")]
        public double Farthest { get; set; }

        [BsonElement("managerId")]
        public string ManagerId { get; set; }

        [BsonElement("trees")]
        public List<string> Trees { get; set; }
    }
}
