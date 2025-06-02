using GameOfLife.Controllers;
using GameOfLife.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Timers;

namespace GameOfLife
{
    public partial class MainWindow : Window
    {
        private GameController controller;
        private Rectangle[,] cellRects;
        private int rows = 30;
        private int cols = 30;
        private bool isMouseDown = false;
        private bool isErasing = false;
        private List<string> previousHashes = new();
        private int stabilityCounter = 0;
        private const int StabilityThreshold = 3;
        private int lastAliveCount = -1;
        private bool gameWasStopped = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            SizeChanged += (s, e) => DrawGrid();

            if (controller != null)
            {
                controller.SetTimerInterval((int)SpeedSlider.Value);
            }
        }

        private void InitializeGame()
        {
            controller = new GameController(rows, cols);
            controller.GridUpdated += DrawGrid;
            cellRects = new Rectangle[rows, cols];

            GameCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            GameCanvas.MouseLeftButtonUp += (s, e) => isMouseDown = false;
            GameCanvas.MouseRightButtonDown += (s, e) => { isMouseDown = true; isErasing = true; };
            GameCanvas.MouseRightButtonUp += (s, e) => { isMouseDown = false; isErasing = false; };
            GameCanvas.MouseMove += Canvas_MouseMove;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var rect = new Rectangle
                    {
                        Stroke = Brushes.Gray,
                        Fill = Brushes.White
                    };
                    GameCanvas.Children.Add(rect);
                    cellRects[i, j] = rect;
                }
            }

            DrawGrid();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            isErasing = false;
            ModifyCellAtMouse(e);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                ModifyCellAtMouse(e);
            }
        }

        private void ModifyCellAtMouse(MouseEventArgs e)
        {
            Point position = e.GetPosition(GameCanvas);
            double cellSize = Math.Min(GameCanvas.ActualWidth / cols,
                                      GameCanvas.ActualHeight / rows);

            int j = (int)(position.X / cellSize);
            int i = (int)(position.Y / cellSize);
            var gameGrid = controller.Grid as GameGrid;
            if (gameGrid == null) return;

            if (i >= 0 && i < rows && j >= 0 && j < cols)
            {
                var cell = gameGrid.Cells[i][j];
                if (isErasing && cell.IsAlive)
                {
                    cell.IsAlive = false;
                    cell.Age = 0;
                    cell.JustBorn = false;
                    DrawGrid();
                }
                else if (!isErasing && !cell.IsAlive)
                {
                    cell.IsAlive = true;
                    cell.Age = 1;
                    cell.JustBorn = true;
                    DrawGrid();
                }
            }
        }

        private void DrawGrid()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    double cellSize = Math.Min(GameCanvas.ActualWidth / cols,
                                              GameCanvas.ActualHeight / rows);
                    var gameGrid = controller.Grid as GameGrid;
                    if (gameGrid == null) return;

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            var cell = gameGrid.Cells[i][j];
                            var rect = cellRects[i, j];

                            rect.Width = rect.Height = cellSize;
                            Canvas.SetLeft(rect, j * cellSize);
                            Canvas.SetTop(rect, i * cellSize);

                            rect.Fill = cell.IsAlive
                                ? (cell.JustBorn ? Brushes.LightGreen : Brushes.Black)
                                : Brushes.White;
                        }
                    }

                    GenerationText.Text = $"Покоління: {controller.Grid.Generation}";
                    AliveCountText.Text = $"Живих клітин: {controller.Grid.CountAlive()}";

                    var hash = GetGridHash(gameGrid);
                    int currentAlive = controller.Grid.CountAlive();

                    if (currentAlive == lastAliveCount && previousHashes.Count > 0 &&
                        previousHashes[^1] == hash)
                    {
                        stabilityCounter++;
                    }
                    else
                    {
                        stabilityCounter = 0;
                    }

                    lastAliveCount = currentAlive;
                    previousHashes.Add(hash);

                    if (previousHashes.Count > StabilityThreshold)
                        previousHashes.RemoveAt(0);
                });
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час малювання: {ex.Message}");
            }
        }

        private string GetGridHash(GameGrid grid)
        {
            var bits = new StringBuilder();
            foreach (var row in grid.Cells)
                foreach (var cell in row)
                    bits.Append(cell.IsAlive ? '1' : '0');
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(bits.ToString()));
            return Convert.ToBase64String(hashBytes);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (controller.Grid.CountAlive() == 0)
            {
                MessageBox.Show("Поле порожнє. Додайте хоча б одну живу клітину, щоб почати гру.", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!gameWasStopped)
            {
                controller.Grid.Generation = 0;
            }
            else
            {
                gameWasStopped = false;
            }
            controller.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            controller.Stop();
            gameWasStopped = true;
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            controller.StepOnce();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            controller.Clear();
            gameWasStopped = false;
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            controller.RandomFill();
            gameWasStopped = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(controller.Grid as GameGrid);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "JSON Files (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(dialog.FileName);
                var grid = JsonSerializer.Deserialize<GameGrid>(json);
                controller.SetGrid(grid);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (controller != null)
            {
                double sliderValue = e.NewValue;
                double newInterval = (SpeedSlider.Minimum + SpeedSlider.Maximum) - sliderValue;
                controller.SetTimerInterval((int)newInterval);
            }
        }
    }
}