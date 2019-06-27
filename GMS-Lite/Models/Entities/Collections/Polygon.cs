using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Polygon
    {
        public Polygon(List<Coordinate> coords)
        {
            Coords = coords;
        }

        public List<Coordinate> Coords { get; set; }
    }
}
