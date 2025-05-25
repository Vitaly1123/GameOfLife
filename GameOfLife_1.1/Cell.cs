using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife.Models
{
    public class Cell
    {
        public bool IsAlive { get; set; }
        public bool JustBorn { get; set; }
        public int Age { get; set; }

        public Cell()
        {
            IsAlive = false;
            JustBorn = false;
            Age = 0;
        }

        public void Die()
        {
            IsAlive = false;
            JustBorn = false;
            Age = 0;
        }

        public void Revive()
        {
            IsAlive = true;
            JustBorn = true;
            Age = 1;
        }

        public void IncrementAge()
        {
            if (IsAlive) Age++;
        }
    }
}
