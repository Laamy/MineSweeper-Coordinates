using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Image = System.Drawing.Image;

class MinesweeperGame : Form
{
    private const int cellSize = 30;
    private int gridSizeX = 10;
    private int gridSizeY = 10;
    private int bombCount = 9;

    private Tile[,] tiles;

    private int bombsLeft;

    private Panel GameRenderer;
    private MenuStrip GameStrip;

    private Image flagImage = Minesweeper.Properties.Resources.flag;
    private Image bombImage = Minesweeper.Properties.Resources.bomb;

    private ToolStripMenuItem BombsLeftDisplay;

    private Level CustomLevel = new Level() { Bombs = 9, GridX = 10, GridY = 10 };

    public MinesweeperGame()
    {
        InitializeUI();
        ResetGame();
    }

    public void ResetGame()
    {
        Size newWinSize = new Size(
            (int)((gridSizeX + 0.5f) * cellSize),
            (int)(((gridSizeY + 1.2f) * cellSize) + GameStrip.Height)
        );

        MinimumSize = newWinSize;
        MaximumSize = newWinSize;

        tiles = new Tile[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                tiles[x, y] = new Tile();
            }
        }

        bombsLeft = bombCount;
        BombsLeftDisplay.Text = bombsLeft.ToString();

        InitializeGame();
        RedrawGame();
    }

    public void ForTiles(Action<int, int, Tile> action)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                action(x, y, tiles[x, y]);
            }
        }
    }

    public Dictionary<Tuple<int, int>, Tile> GetTilesByProperty(Func<Tile, bool> propertyCondition)
    {
        Dictionary<Tuple<int, int>, Tile> conditionedTiles = new Dictionary<Tuple<int, int>, Tile>();

        ForTiles((x, y, tile) =>
        {
            if (propertyCondition(tile)) // bomb
            {
                conditionedTiles.Add(new Tuple<int, int>(x, y), tile);
            }
        });

        return conditionedTiles;
    }

    public Tuple<int, int> _Tuple(int x, int y)
    {
        return new Tuple<int, int>(x, y);
    }

    private void InitializeGame()
    {
        // NOTE: not optimized cuz i want to add modding support in an easy to interact way

        // Place bombs randomly
        Random random = new Random();

        for (int i = 0; i < bombCount;)
        {
            int x = random.Next(0, gridSizeX);
            int y = random.Next(0, gridSizeY);

            Dictionary<Tuple<int, int>, Tile> bombs = GetTilesByProperty(tile => tile.Id == Tile.Bomb);

            if (!bombs.ContainsKey(_Tuple(x, y)))
            {
                tiles[x, y].Id = Tile.Bomb; // set the tile ID
                i++;
            }
        }
    }

    public void RedrawGame()
    {
        GameRenderer.Invalidate();
    }

    public void AddStripItem(ToolStripItemCollection parent, string text, Action click)
    {
        ToolStripMenuItem item = new ToolStripMenuItem() { Text = text };

        item.Click += delegate { click(); };

        parent.Add(item);
    }

    public void AddStripSplit(ToolStripItemCollection parent)
    {
        parent.Add(new ToolStripSeparator());
    }

    public void SetLevel(Level level)
    {
        bombCount = level.Bombs;
        gridSizeX = level.GridX;
        gridSizeY = level.GridY;

        ResetGame();
    }

    private void InitializeUI()
    {
        Text = "Minesweeper";
        DoubleBuffered = true;

        GameRenderer = new Panel() { Dock = DockStyle.Fill };
        Controls.Add(GameRenderer);

        GameStrip = new MenuStrip();
        Controls.Add(GameStrip);

        // file category
        {
            ToolStripMenuItem parent = new ToolStripMenuItem() { Text = "Game" };

            AddStripItem(parent.DropDownItems, "Reset", () => { ResetGame(); });

            AddStripSplit(parent.DropDownItems);
            AddStripItem(parent.DropDownItems, "Beginner", () => { SetLevel(MinesweeperLevels.Get().Beginner); });
            AddStripItem(parent.DropDownItems, "Intermediate", () => { SetLevel(MinesweeperLevels.Get().Intermediate); });
            AddStripItem(parent.DropDownItems, "Expert", () => { SetLevel(MinesweeperLevels.Get().Expert); });
            
            // Custom category
            {
                ToolStripMenuItem parent2 = new ToolStripMenuItem() { Text = "Custom.." };

                AddStripItem(parent2.DropDownItems, "Set", () => { ResetGame(); });

                parent.DropDownItems.Add(parent2);
            }

            AddStripSplit(parent.DropDownItems);
            AddStripItem(parent.DropDownItems, "Exit", () => { Application.Exit(); });

            GameStrip.Items.Add(parent);
        }

        // score/bombs left counter
        BombsLeftDisplay = new ToolStripMenuItem()
        {
            Text = "9",
            ForeColor = Color.Red,
            Alignment = ToolStripItemAlignment.Right
        };

        GameStrip.Items.Add(BombsLeftDisplay);

        GameRenderer.Paint += OnPaint;
        GameRenderer.MouseClick += OnMouseClick;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        ForTiles((x, y, tile) =>
        {
            Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
            string coordinates = $"{x + 1}{(char)('A' + y)}";

            if (tile.IsRevealed)
            {
                if (tile.Id == Tile.Bomb)
                {
                    g.FillRectangle(Brushes.Red, cellRect);
                    g.DrawImage(bombImage, cellRect);
                }
                else
                {
                    int adjacentBombs = CountAdjacentBombs(x, y);
                    if (adjacentBombs > 0)
                        g.DrawString(adjacentBombs.ToString(), Font, Brushes.Black, cellRect);
                }
            }
            else if (tile.IsFlagged)
            {
                g.FillRectangle(Brushes.Gray, cellRect);
                g.DrawImage(flagImage, cellRect);
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150)), cellRect);
                g.DrawString(coordinates, Font, Brushes.Black, cellRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            }

            ControlPaint.DrawBorder(g, cellRect, Color.Black, ButtonBorderStyle.Solid);
        });
    }

    private void OnMouseClick(object sender, MouseEventArgs e)
    {
        int x = e.X / cellSize;
        int y = e.Y / cellSize;

        if (x < 0 || y < 0)
            return;

        if (x > gridSizeX - 1 || y > gridSizeY - 1)
            return;

        Tile tile = tiles[x, y];

        if (e.Button == MouseButtons.Left)
        {
            if (tile.IsFlagged || tile.IsRevealed)
                return; // its flagged so we dont want to check if theirs a bomb or not

            if (tile.Id == Tile.Bomb)
            {
                tile.Reveal(); // render bomb where its meant to be then redraw frame
                RedrawGame();

                MessageBox.Show("Game Over!");
                ResetGame();
            }
            else
            {
                RevealEmptySquares(x, y);
                RedrawGame();
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            if (tile.IsRevealed)
                return; // dont redraw/toggle when its been revealed

            ToggleFlag(x, y);
            RedrawGame();
        }
    }

    private void RevealEmptySquares(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || tiles[x, y].IsRevealed)
            return;

        Tile tile = tiles[x, y];

        if (tile.IsFlagged)
            ToggleFlag(x, y);

        tile.Reveal();

        if (CountAdjacentBombs(x, y) == 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newX = x + i;
                    int newY = y + j;

                    RevealEmptySquares(newX, newY);
                }
            }
        }
    }

    private void ToggleFlag(int x, int y)
    {
        Tile tile = tiles[x, y];

        if (tile.IsRevealed)
            return; // dont flag spots that are already exposed

        tile.Flag(!tile.IsFlagged);

        bombsLeft += tile.IsFlagged ? -1 : 1;

        BombsLeftDisplay.Text = bombsLeft.ToString();
    }

    private int CountAdjacentBombs(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;

                if (newX >= 0 && newX < gridSizeX && newY >= 0 && newY < gridSizeY && tiles[newX, newY].Id == Tile.Bomb)
                    count++;
            }
        }
        return count;
    }
}