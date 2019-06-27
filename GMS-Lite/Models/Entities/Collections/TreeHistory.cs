using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class TreeHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("detail")]
        public string Detail { get; set; }

        [BsonElement("treeId")]
        public string TreeId { get; set; }

        [BsonElement("polygonId")]
        public string PolygonId { get; set; }
        
    }
}
