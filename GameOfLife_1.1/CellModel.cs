using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Models
{
    public class CellModel
    {
        public bool IsAlive { get; set; }
        public int Age { get; set; }
        public bool JustBorn { get; set; }
    }
}
