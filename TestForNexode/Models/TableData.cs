using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForTexode.Models
{
    public class TableData
    {
        public string Name { get; set; }
        public int Average { get; set; }
        public int StepsMax { get; set; }
        public int StepsMin { get; set; }
        public TableData(string name, int average, int stepsMax, int stepsMin)
            => (Name, Average, StepsMax, StepsMin) = (name, average, stepsMax, stepsMin);
    }
}
