using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Models
{
    public class GridModel
    {
        public CellModel[,] Cells { get; set; }
        public int Generation { get; set; }

        public int Rows => Cells.GetLength(0);
        public int Cols => Cells.GetLength(1);

        public int CountAlive()
        {
            int count = 0;
            foreach (var cell in Cells)
                if (cell.IsAlive) count++;
            return count;
        }
    }
}
