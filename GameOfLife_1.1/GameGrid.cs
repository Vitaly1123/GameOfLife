using System;
using System.Collections.Generic;

namespace GameOfLife.Models
{
    public class GameGrid : BaseGrid
    {
        public List<List<CellModel>> Cells { get; set; }

        public override int Rows => Cells.Count;
        public override int Cols => Cells[0].Count;

        public GameGrid(int rows, int cols)
        {
            Cells = new List<List<CellModel>>();
            for (int i = 0; i < rows; i++)
            {
                var row = new List<CellModel>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add(new CellModel());
                }
                Cells.Add(row);
            }
        }

        public override void Step()
        {
            var newCells = new List<List<CellModel>>();
            for (int i = 0; i < Rows; i++)
            {
                var newRow = new List<CellModel>();
                for (int j = 0; j < Cols; j++)
                {
                    int aliveNeighbors = CountAliveNeighbors(i, j);
                    bool isAlive = Cells[i][j].IsAlive;
                    var newCell = new CellModel();

                    if (isAlive)
                    {
                        if (aliveNeighbors < 2 || aliveNeighbors > 3)
                        {
                            newCell.IsAlive = false;
                            newCell.Age = 0;
                            newCell.JustBorn = false;
                        }
                        else
                        {
                            newCell.IsAlive = true;
                            newCell.Age = Cells[i][j].Age + 1;
                            newCell.JustBorn = false;
                        }
                    }
                    else
                    {
                        if (aliveNeighbors == 3)
                        {
                            newCell.IsAlive = true;
                            newCell.Age = 1;
                            newCell.JustBorn = true;
                        }
                        else
                        {
                            newCell.IsAlive = false;
                            newCell.Age = 0;
                            newCell.JustBorn = false;
                        }
                    }
                    newRow.Add(newCell);
                }
                newCells.Add(newRow);
            }
            Cells = newCells;
            Generation++;
        }

        public override void Clear()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Cells[i][j] = new CellModel();
                }
            }
            Generation = 0;
        }

        public override int CountAlive()
        {
            int count = 0;
            foreach (var row in Cells)
                foreach (var cell in row)
                    if (cell.IsAlive) count++;
            return count;
        }

        private int CountAliveNeighbors(int row, int col)
        {
            int count = 0;
            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = col - 1; j <= col + 1; j++)
                {
                    if (i == row && j == col)
                        continue;
                    if (i >= 0 && i < Rows && j >= 0 && j < Cols)
                        if (Cells[i][j].IsAlive)
                            count++;
                }
            }
            return count;
        }
    }
}