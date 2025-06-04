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
        private List<GameGrid> _history;
        private int _historyCapacity = 100;

        public BaseGrid Grid => _grid;
        public event Action GridUpdated;

        public GameController(int rows, int cols)
        {
            _grid = new GameGrid(rows, cols);
            _timer = new Timer(200);
            _timer.Elapsed += (s, e) => StepOnce();
            _history = new List<GameGrid>();
            _history.Add(DeepCopyGrid(_grid as GameGrid));
            ResetHashTracking();
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
                ResetHashTracking();
                return;
            }

            _grid.Step();

            if (_history.Count > _grid.Generation)
            {
                _history.RemoveRange((int)_grid.Generation, _history.Count - (int)_grid.Generation);
            }

            _history.Add(DeepCopyGrid(_grid as GameGrid));

            if (_history.Count > _historyCapacity)
            {
                _history.RemoveAt(0);
            }

            GridUpdated?.Invoke();

            string currentHash = GetGridHash();
            if (lastHashes.Any() && lastHashes.Last() == currentHash)
            {
                Stop();
                MessageBox.Show("Стабільне поєднання клітин. Гра завершена.", "Стабільність",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ResetHashTracking();
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
                ResetHashTracking();
                return;
            }
            seenHashes.Add(currentHash);
        }

        public void StepBack()
        {
            if (_grid.Generation > 0)
            {
                _grid = DeepCopyGrid(_history[(int)_grid.Generation - 1]);
                GridUpdated?.Invoke();
                Stop();

                ResetHashTracking();
            }
            else
            {
                MessageBox.Show("Немає попередніх кроків.", "Крок назад", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
            ResetHashTracking();
            _history.Clear();
            _history.Add(DeepCopyGrid(_grid as GameGrid));
            GridUpdated?.Invoke();
        }

        public void RandomFill()
        {
            _grid.Clear();
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
            _grid.Generation = 0;
            ResetHashTracking();
            _history.Clear();
            _history.Add(DeepCopyGrid(_grid as GameGrid));
            GridUpdated?.Invoke();
        }

        public void SetGrid(BaseGrid newGrid)
        {
            _grid = newGrid;
            ResetHashTracking();
            _history.Clear();
            _history.Add(DeepCopyGrid(_grid as GameGrid));
            GridUpdated?.Invoke();
        }

        private GameGrid DeepCopyGrid(GameGrid originalGrid)
        {
            if (originalGrid == null) return null;

            var newGrid = new GameGrid(originalGrid.Rows, originalGrid.Cols)
            {
                Generation = originalGrid.Generation
            };

            for (int i = 0; i < originalGrid.Rows; i++)
            {
                for (int j = 0; j < originalGrid.Cols; j++)
                {
                    var originalCell = originalGrid.Cells[i][j];
                    newGrid.Cells[i][j] = new CellModel
                    {
                        IsAlive = originalCell.IsAlive,
                        Age = originalCell.Age,
                        JustBorn = originalCell.JustBorn
                    };
                }
            }
            return newGrid;
        }

        private void ResetHashTracking()
        {
            lastHashes.Clear();
            seenHashes.Clear();
            seenHashes.Add(GetGridHash());
        }
    }
}