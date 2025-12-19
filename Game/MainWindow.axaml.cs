using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public partial class MainWindow : Window
    {
        // Логика
        public class GameObject
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }

            public bool CollidesWith(GameObject other)
            {
                return X < other.X + other.Width && X + Width > other.X &&
                       Y < other.Y + other.Height && Y + Height > other.Y;
            }
        }

        public class Player : GameObject
        {
            public int MaxTraps { get; set; } = 5;
            public int ActiveTraps { get; set; } = 0;

            public void Move(double dx, double dy, double fieldWidth, double fieldHeight)
            {
                X = Math.Clamp(X + dx, 0, fieldWidth - Width);
                Y = Math.Clamp(Y + dy, 0, fieldHeight - Height);
            }
        }

        public class Enemy : GameObject
        {
            public double DX { get; set; }
            public double DY { get; set; }

            public void Move(double fieldWidth, double fieldHeight)
            {
                X += DX;
                Y += DY;

                if (X < 0 || X + Width > fieldWidth) DX *= -1;
                if (Y < 0 || Y + Height > fieldHeight) DY *= -1;
            }
        }

        public class Trap : GameObject { }

        public class GameEngine
        {
            public Player Player { get; set; }
            public List<Enemy> Enemies { get; set; } = new();
            public List<Trap> Traps { get; set; } = new();
            public int Score { get; set; } = 0;
            public int FieldWidth { get; set; }
            public int FieldHeight { get; set; }
            public int MaxScore { get; } = 25;
            public bool GameOver { get; private set; } = false;


            private Random rnd = new Random();

            public GameEngine(int width, int height)
            {
                FieldWidth = width;
                FieldHeight = height;
                Player = new Player { X = 50, Y = 50, Width = 20, Height = 20 };
                for (int i = 0; i < 5; i++) SpawnEnemy();
            }

            public void SpawnEnemy()
            {
                Enemies.Add(new Enemy
                {
                    X = rnd.Next(0, FieldWidth - 15),
                    Y = rnd.Next(0, FieldHeight - 15),
                    Width = 15,
                    Height = 15,
                    DX = rnd.NextDouble() * 4 - 2,
                    DY = rnd.NextDouble() * 4 - 2
                });
            }

            public void Update()
            {
                if (GameOver)
                    return;
                // Движение врагов и проверка ловушек
                foreach (var enemy in Enemies.ToList())
                {
                    enemy.Move(FieldWidth, FieldHeight);

                    foreach (var trap in Traps.ToList())
                    {
                        if (enemy.CollidesWith(trap))
                        {
                            Enemies.Remove(enemy);
                            Traps.Remove(trap);
                            Player.ActiveTraps--;
                            Score++;
                            if (Score >= MaxScore)
                            {
                                GameOver = true;
                            }
                            break;
                        }
                    }
                }

                // Проверка столкновения с игроком
                foreach (var enemy in Enemies)
                {
                    if (enemy.CollidesWith(Player))
                    {
                        Reset();
                        return;
                    }
                }

                if (Score > 0 && Score % 5 == 0 && Enemies.Count < Score / 5 + 5)
                    SpawnEnemy();
            }

            public void PlaceTrap()
            {
                if (Player.ActiveTraps < Player.MaxTraps)
                {
                    Traps.Add(new Trap { X = Player.X, Y = Player.Y, Width = 20, Height = 20 });
                    Player.ActiveTraps++;
                }
            }

            public void Reset()
            {
                Player.X = 50;
                Player.Y = 50;
                Player.ActiveTraps = 0;
                Enemies.Clear();
                Traps.Clear();
                Score = 0;
                for (int i = 0; i < 5; i++) SpawnEnemy();
            }
        }

        private GameEngine engine;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            StartButton.Click += (s, e) =>
            {
                engine.Reset();
                WinText.IsVisible = false;
            };


            // Оьработка клавиш
            this.Focusable = true;
            this.Focus();
            this.KeyDown += MainWindow_KeyDown;

            engine = new GameEngine(500, 500);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            engine.Update();
            Draw();
        }

        private void Draw()
        {
            GameCanvas.Children.Clear();

            // Игрок
            var playerRect = new Rectangle
            {
                Width = engine.Player.Width,
                Height = engine.Player.Height,
                Fill = Brushes.Blue
            };
            Canvas.SetLeft(playerRect, engine.Player.X);
            Canvas.SetTop(playerRect, engine.Player.Y);
            GameCanvas.Children.Add(playerRect);

            // Враги
            foreach (var enemy in engine.Enemies)
            {
                var enemyRect = new Rectangle
                {
                    Width = enemy.Width,
                    Height = enemy.Height,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(enemyRect, enemy.X);
                Canvas.SetTop(enemyRect, enemy.Y);
                GameCanvas.Children.Add(enemyRect);
            }

            // Ловушки
            foreach (var trap in engine.Traps)
            {
                var trapRect = new Rectangle
                {
                    Width = trap.Width,
                    Height = trap.Height,
                    Fill = Brushes.Black
                };
                Canvas.SetLeft(trapRect, trap.X);
                Canvas.SetTop(trapRect, trap.Y);
                GameCanvas.Children.Add(trapRect);
            }

            ScoreText.Text = $"Score: {engine.Score}";
            TrapText.Text = $"Traps: {engine.Player.ActiveTraps}/{engine.Player.MaxTraps}";

            WinText.IsVisible = engine.GameOver && engine.Score >= engine.MaxScore;
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            if (engine.GameOver)
                return;

            switch (e.Key)
            {
                case Key.Left: engine.Player.Move(-5, 0, engine.FieldWidth, engine.FieldHeight); break;
                case Key.Right: engine.Player.Move(5, 0, engine.FieldWidth, engine.FieldHeight); break;
                case Key.Up: engine.Player.Move(0, -5, engine.FieldWidth, engine.FieldHeight); break;
                case Key.Down: engine.Player.Move(0, 5, engine.FieldWidth, engine.FieldHeight); break;
                case Key.Space: engine.PlaceTrap(); break;
            }
        }
    }
}
