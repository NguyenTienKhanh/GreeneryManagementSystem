using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Tree
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("date")]
        public DateTime RegisDate { get; set; }

        [BsonElement("status")]
        public TreeHistory Status { get; set; }

        [BsonElement("height")]
        public double Height { get; set; }

        [BsonElement("width")]
        public double Width { get; set; }

        [BsonElement("coord")]
        public Coordinate Coord { get; set; }

        [BsonElement("managerId")]
        public string ManagerId { get; set; }

        [BsonElement("createUserId")]
        public string CreateUserId { get; set; }

        [BsonElement("polygonId")]
        public string PolygonId { get; set; }

        [BsonElement("history")]
        public List<string> History { get; set; }

        [BsonElement("note")]
        public string Note { get; set; }
    }
}
