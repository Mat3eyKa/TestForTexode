using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForTexode.Models
{
    public class UserWithAllHisData
    {
        public string Name { get; }
        public List<DaySteps> Steps { get; }
        public UserWithAllHisData(string name) => (Name, Steps) = (name, new List<DaySteps>());
    }
}