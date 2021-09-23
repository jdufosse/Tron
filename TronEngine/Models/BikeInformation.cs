using System.Collections.Generic;
using TronEngine.Enums;

namespace TronEngine.Models
{
    public class BikeInformation
    {
        public Direction Direction { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public List<Trace> Traces { get; set; }
    }
}
