﻿using System;
using System.Drawing;
using System.Windows.Forms;

class MinesweeperGame : Form
{
    private const int cellSize = 30;
    private const int gridSizeX = 10;
    private const int gridSizeY = 10;
    private const int bombCount = 9;

    private bool[,] bombs;
    private bool[,] revealed;
    private bool[,] flagged;

    private Panel GameRenderer;
    private MenuStrip GameStrip;

    public MinesweeperGame()
    {
        InitializeUI();
        ResetGame();
    }

    public void ResetGame()
    {
        Size newWInSize = new Size(
            (gridSizeX * cellSize) + 15,
            (gridSizeY * cellSize) + GameStrip.Size.Height + cellSize + 8
        );

        MinimumSize = newWInSize;
        MaximumSize = newWInSize;

        bombs = new bool[gridSizeX, gridSizeY];
        revealed = new bool[gridSizeX, gridSizeY];
        flagged = new bool[gridSizeX, gridSizeY];

        Array.Clear(revealed, 0, revealed.Length);
        Array.Clear(flagged, 0, flagged.Length);
        Array.Clear(bombs, 0, bombs.Length);

        InitializeGame();
        RedrawGame();
    }

    private void InitializeGame()
    {
        // Place bombs randomly
        Random random = new Random();
        for (int i = 0; i < bombCount;)
        {
            int x = random.Next(0, gridSizeX);
            int y = random.Next(0, gridSizeY);

            if (!bombs[x, y])
            {
                bombs[x, y] = true;
                i++;
            }
        }
    }

    public void RedrawGame()
    {
        GameRenderer.Invalidate();
    }

    public void AddStripItem(string text, Action click)
    {
        ToolStripMenuItem item = new ToolStripMenuItem() { Text = "Reset" };

        item.Click += delegate { click(); };

        GameStrip.Items.Add(item);
    }

    private void InitializeUI()
    {
        Text = "Minesweeper";
        DoubleBuffered = true;

        GameRenderer = new Panel() { Dock = DockStyle.Fill };
        Controls.Add(GameRenderer);

        GameStrip = new MenuStrip();
        Controls.Add(GameStrip);

        AddStripItem("Reset", () => { ResetGame(); });

        GameRenderer.Paint += OnPaint;
        GameRenderer.MouseClick += OnMouseClick;
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Rectangle cellRect = new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize);
                string coordinates = $"{x + 1}{(char)('A' + y)}";

                if (revealed[x, y])
                {
                    if (bombs[x, y])
                        g.FillRectangle(Brushes.Red, cellRect);
                    else
                    {
                        int adjacentBombs = CountAdjacentBombs(x, y);
                        if (adjacentBombs > 0)
                            g.DrawString(adjacentBombs.ToString(), Font, Brushes.Black, cellRect);
                    }
                }
                else if (flagged[x, y])
                {
                    g.FillRectangle(Brushes.Black, cellRect);
                    g.DrawString(coordinates, Font, Brushes.White, cellRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }
                else
                {
                    g.FillRectangle(Brushes.Gray, cellRect);
                    g.DrawString(coordinates, Font, Brushes.Black, cellRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }

                ControlPaint.DrawBorder(g, cellRect, Color.Black, ButtonBorderStyle.Solid);
            }
        }
    }

    private void OnMouseClick(object sender, MouseEventArgs e)
    {
        int x = e.X / cellSize;
        int y = e.Y / cellSize;

        if (x < 0 || y < 0)
            return;

        if (x > gridSizeX - 1 || y > gridSizeY - 1)
            return;

        if (e.Button == MouseButtons.Left)
        {
            if (bombs[x, y])
            {
                revealed[x, y] = true; // render red square where the bomb was
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
            ToggleFlag(x, y);
            RedrawGame();
        }
    }

    private void RevealEmptySquares(int x, int y)
    {
        if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY || revealed[x, y])
            return;

        revealed[x, y] = true;

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
        flagged[x, y] = !flagged[x, y];
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

                if (newX >= 0 && newX < gridSizeX && newY >= 0 && newY < gridSizeY && bombs[newX, newY])
                    count++;
            }
        }
        return count;
    }
}