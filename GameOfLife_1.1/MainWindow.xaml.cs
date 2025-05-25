using GameOfLife.Controllers;
using GameOfLife.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            SizeChanged += (s, e) => DrawGrid();
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
            double cellSize = Math.Min(GameCanvas.ActualWidth / cols, GameCanvas.ActualHeight / rows);
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
                    double cellSize = Math.Min(GameCanvas.ActualWidth / cols, GameCanvas.ActualHeight / rows);
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
                });
            }
            catch (TaskCanceledException)
            {
                // Завдання було скасовано — ігноруємо або логгування, якщо потрібно
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час малювання: {ex.Message}");
            }
        }



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            controller.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            controller.Stop();
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            controller.StepOnce();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            controller.Clear();
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            controller.RandomFill();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "JSON Files|*.json" };
            if (dialog.ShowDialog() == true)
            {
                var json = JsonSerializer.Serialize(controller.Grid as GameGrid);
                File.WriteAllText(dialog.FileName, json);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "JSON Files|*.json" };
            if (dialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(dialog.FileName);
                var grid = JsonSerializer.Deserialize<GameGrid>(json);
                controller.SetGrid(grid);
            }
        }
    }
}
