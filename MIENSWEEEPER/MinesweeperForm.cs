using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

class MinesweeperGame : Form
{
    public static MinesweeperGame Instance;

    private Panel GameRenderer;
    private MenuStrip GameStrip;

    private ToolStripMenuItem BombsLeftDisplay;

    private Level CustomLevel = new Level() { Bombs = 9, GridX = 10, GridY = 10 };

    public MinesweeperGame()
    {
        Instance = this;

        InitializeUI();
        ResetGame();

        PluginManager.InitPlugins();
    }

    public void ResetGame()
    {
        Size newWinSize = new Size(
            (int)((Game.GridX + 0.5f) * Game.CellSize),
            (int)(((Game.GridY + 1.2f) * Game.CellSize) + GameStrip.Height)
        );

        MinimumSize = newWinSize;
        MaximumSize = newWinSize;

        Game.Tiles = new Tile[Game.GridX, Game.GridY];

        for (int x = 0; x < Game.GridX; x++)
        {
            for (int y = 0; y < Game.GridY; y++)
            {
                Game.Tiles[x, y] = new Tile();
            }
        }

        Game.BombsLeft = Game.BombCount;
        BombsLeftDisplay.Text = Game.BombsLeft.ToString();

        InitializeGame();
        RedrawGame();

        PluginManager.Foreach(plugin => plugin.OnGameStart());
    }

    private void InitializeGame()
    {
        // Place bombs randomly
        Random random = new Random();

        for (int i = 0; i < Game.BombCount;)
        {
            int x = random.Next(0, Game.GridX);
            int y = random.Next(0, Game.GridY);

            Dictionary<Tuple<int, int>, Tile> bombs = TileUtils.GetTilesByProperty(tile => tile.Id == Tile.Bomb);

            if (!bombs.ContainsKey(TileUtils.Tuple(x, y)))
            {
                Game.Tiles[x, y].Id = Tile.Bomb; // set the tile ID
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
        Game.BombCount = level.Bombs;
        Game.GridX = level.GridX;
        Game.GridY = level.GridY;

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

        TileUtils.ForTiles((x, y, tile) =>
        {
            bool cancel = false;

            PluginManager.Foreach(plugin => plugin.OnTileRender(g, x, y, tile, ref cancel));

            if (!cancel)
            {
                Rectangle cellRect = new Rectangle(x * Game.CellSize, y * Game.CellSize, Game.CellSize, Game.CellSize);
                string coordinates = $"{x + 1}{(char)('A' + y)}";

                if (tile.IsRevealed)
                {
                    Tile.DrawRevealed(g, Font, tile, TileUtils.Tuple(x, y), cellRect);
                }
                else if (tile.IsFlagged)
                {
                    Tile.DrawFlag(g, cellRect);
                }
                else
                {
                    Tile.DrawHidden(g, Font, cellRect, coordinates);
                }

                Tile.DrawBorder(g, cellRect);
            }
        });
    }

    private void OnMouseClick(object sender, MouseEventArgs e)
    {
        int x = e.X / Game.CellSize;
        int y = e.Y / Game.CellSize;

        if (x < 0 || y < 0)
            return;

        if (x > Game.GridX - 1 || y > Game.GridY - 1)
            return;

        Tile tile = Game.Tiles[x, y];

        if (e.Button == MouseButtons.Left)
        {
            PluginManager.Foreach(plugin => plugin.OnTileClicked(new Tuple<int, int>(x, y), tile));

            if (tile.IsFlagged || tile.IsRevealed)
                return; // its flagged so we dont want to check if theirs a bomb or not

            if (tile.Id == Tile.Bomb)
            {
                tile.Reveal(); // render bomb where its meant to be then redraw frame
                RedrawGame();

                PluginManager.Foreach(plugin => plugin.OnGameEnd());

                MessageBox.Show("Game Over!");

                ResetGame();
            }
            else
            {
                TileUtils.RevealEmptySquares(BombsLeftDisplay, x, y);
                RedrawGame();
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            PluginManager.Foreach(plugin => plugin.OnTileRightClicked(new Tuple<int, int>(x, y), tile));

            if (tile.IsRevealed)
                return; // dont redraw/toggle when its been revealed

            TileUtils.ToggleFlag(BombsLeftDisplay, x, y);
            RedrawGame();
        }
    }
}