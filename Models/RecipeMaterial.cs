using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FactoryPlanner.Models
{
    public class RecipeMaterial
    {
        public RecipeMaterial(string name, double rate)
        {
            Name = name;
            Rate = rate;
        }

        public string Name { get; set; } = "";
        public double Rate { get; set; }
        public override string ToString()
        {
            return $"{Rate}x {Name}";
        }

    }
}
