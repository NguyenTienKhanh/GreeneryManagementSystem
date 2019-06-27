using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Coordinate
    {
        public Coordinate(string latitude, string longtitude)
        {
            Latitude = latitude;
            Longtitude = longtitude;
        }

        public string Latitude { get; set; }
        public string Longtitude { get; set; }
    }
}
