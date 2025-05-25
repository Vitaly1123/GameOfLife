// Controllers/GameController.cs
using GameOfLife.Models;
using System;
using System.Timers;

namespace GameOfLife.Controllers
{
    public class GameController
    {
        private Timer _timer;
        private BaseGrid _grid;

        public BaseGrid Grid => _grid;

        public event Action GridUpdated;

        public GameController(int rows, int cols)
        {
            _grid = new GameGrid(rows, cols); // Поліморфізм: використовуємо базовий тип
            _timer = new Timer(200);
            _timer.Elapsed += (s, e) => StepOnce();
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        public void StepOnce()
        {
            _grid.Step();
            GridUpdated?.Invoke();
        }

        public void Clear()
        {
            _grid.Clear();
            GridUpdated?.Invoke();
        }

        public void RandomFill()
        {
            var rand = new Random();
            for (int i = 0; i < _grid.Rows; i++)
            {
                for (int j = 0; j < _grid.Cols; j++)
                {
                    var cell = ((_grid as GameGrid)?.Cells[i][j]); // Легкий каст до GameGrid
                    if (cell != null)
                    {
                        cell.IsAlive = rand.Next(2) == 0;
                        cell.Age = cell.IsAlive ? 1 : 0;
                        cell.JustBorn = cell.IsAlive;
                    }
                }
            }
            GridUpdated?.Invoke();
        }

        public void SetGrid(BaseGrid newGrid)
        {
            _grid = newGrid;
            GridUpdated?.Invoke();
        }
    }
}
