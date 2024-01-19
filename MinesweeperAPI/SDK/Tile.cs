using System;
using System.Drawing;
using System.Windows.Forms;

public class Tile
{
    public int Id { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }

    public void Reveal() => IsRevealed = true;
    public void Flag(bool value) => IsFlagged = value;

    public const int Bomb = 1; // ID's
    public const int Empty = 0;

    public static Image flagImage = MinesweeperAPI.Properties.Resources.flag;
    public static Image bombImage = MinesweeperAPI.Properties.Resources.bomb;

    public static void DrawRevealed(Graphics g, Font Font, Tile tile, Tuple<int, int> pos, Rectangle cellRect)
    {
        if (tile.Id == Bomb)
        {
            g.FillRectangle(Brushes.Red, cellRect);
            g.DrawImage(bombImage, cellRect);
        }
        else
        {
            int adjacentBombs = TileUtils.CountAdjacentBombs(pos.Item1, pos.Item2);
            if (adjacentBombs > 0)
                g.DrawString(adjacentBombs.ToString(), Font, Brushes.Black, cellRect);
        }
    }

    public static void DrawFlag(Graphics g, Rectangle cellRect)
    {
        g.FillRectangle(Brushes.Gray, cellRect);
        g.DrawImage(flagImage, cellRect);
    }

    public static void DrawHidden(Graphics g, Font Font, Rectangle cellRect, string coordinates = "")
    {
        g.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150)), cellRect);
        g.DrawString(coordinates, Font, Brushes.Black, cellRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
    }

    public static void DrawBorder(Graphics g, Rectangle cellRect)
    {
        ControlPaint.DrawBorder(g, cellRect, Color.Black, ButtonBorderStyle.Solid);
    }
}