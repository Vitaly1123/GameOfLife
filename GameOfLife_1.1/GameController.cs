using GameOfLife.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;

namespace GameOfLife.Controllers
{
    public class GameController
    {
        private Timer _timer;
        private BaseGrid _grid;
        private Queue<string> lastHashes = new Queue<string>();
        private HashSet<string> seenHashes = new HashSet<string>();

        public BaseGrid Grid => _grid;
        public event Action GridUpdated;

        public GameController(int rows, int cols)
        {
            _grid = new GameGrid(rows, cols);
            _timer = new Timer(200);
            _timer.Elapsed += (s, e) => StepOnce();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        public void SetTimerInterval(int interval)
        {
            _timer.Interval = interval;
        }

        public void StepOnce()
        {
            if (_grid.CountAlive() == 0)
            {
                Stop();
                MessageBox.Show("Усі клітини мертві. Гра завершена.", "Кінець",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _grid.Step();
            GridUpdated?.Invoke();

            string currentHash = GetGridHash();

            if (lastHashes.Any() && lastHashes.Last() == currentHash)
            {
                Stop();
                MessageBox.Show("Стабільне поєднання клітин. Гра завершена.", "Стабільність",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                lastHashes.Clear();
                seenHashes.Clear();
                return;
            }

            lastHashes.Enqueue(currentHash);
            if (lastHashes.Count > 5)
                lastHashes.Dequeue();

            if (seenHashes.Contains(currentHash))
            {
                Stop();
                MessageBox.Show("Виявлено цикл. Гра завершена.", "Цикл",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                lastHashes.Clear();
                seenHashes.Clear();
                return;
            }

            seenHashes.Add(currentHash);
        }

        private string GetGridHash()
        {
            var gameGrid = _grid as GameGrid;
            if (gameGrid == null) return "";

            var sb = new StringBuilder();
            for (int i = 0; i < gameGrid.Rows; i++)
                for (int j = 0; j < gameGrid.Cols; j++)
                    sb.Append(gameGrid.Cells[i][j].IsAlive ? $"1{i}, {j};" : "");

            return sb.ToString();
        }

        public void Clear()
        {
            _grid.Clear();
            lastHashes.Clear();
            seenHashes.Clear();
            GridUpdated?.Invoke();
        }

        public void RandomFill()
        {
            var rand = new Random();
            for (int i = 0; i < _grid.Rows; i++)
            {
                for (int j = 0; j < _grid.Cols; j++)
                {
                    var cell = ((_grid as GameGrid)?.Cells[i][j]);
                    if (cell != null)
                    {
                        cell.IsAlive = rand.Next(2) == 0;
                        cell.Age = cell.IsAlive ? 1 : 0;
                        cell.JustBorn = cell.IsAlive;
                    }
                }
            }
            lastHashes.Clear();
            seenHashes.Clear();
            GridUpdated?.Invoke();
        }

        public void SetGrid(BaseGrid newGrid)
        {
            _grid = newGrid;
            lastHashes.Clear();
            seenHashes.Clear();
            GridUpdated?.Invoke();
        }
    }
}