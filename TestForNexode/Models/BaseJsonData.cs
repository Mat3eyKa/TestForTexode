using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestForTexode.Models
{
    public class BaseJsonData
    {
        public int Rank { get; }
        public string User { get; }
        public int Steps { get; }
        public string Status { get; }
        public int Day { get; set; }
        public BaseJsonData(int rank, string user, int steps, string status)
           => (Rank, User, Steps, Status) = (rank, user, steps, status);
    }
}
