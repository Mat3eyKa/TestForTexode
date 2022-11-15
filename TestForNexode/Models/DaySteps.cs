using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForTexode.Models
{
    public class DaySteps
    {
        public int Rank { get; set; }
        public int Day { get; }
        public string Status { get; }
        public int Steps { get; }
        public DaySteps(int rank, int day, int steps, string status) => (Rank, Day, Steps, Status) = (rank, day, steps, status);
    }
}
