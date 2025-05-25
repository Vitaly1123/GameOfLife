using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Models
{
    public abstract class BaseGrid
    {
        public int Generation { get; set; }

        public abstract int Rows { get; }
        public abstract int Cols { get; }

        public abstract void Step();
        public abstract void Clear();
        public abstract int CountAlive();
    }
}
