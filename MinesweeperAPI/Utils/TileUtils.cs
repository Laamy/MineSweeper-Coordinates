using System.Collections.Generic;
using System;
using System.Windows.Forms;

public class TileUtils
{
    public static int CountAdjacentBombs(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;

                if (newX >= 0 && newX < Game.GridX && newY >= 0 && newY < Game.GridY && Game.Tiles[newX, newY].Id == Tile.Bomb)
                    count++;
            }
        }
        return count;
    }

    public static void ForTiles(Action<int, int, Tile> action)
    {
        for (int x = 0; x < Game.GridX; x++)
        {
            for (int y = 0; y < Game.GridY; y++)
            {
                action(x, y, Game.Tiles[x, y]);
            }
        }
    }

    public static Dictionary<Tuple<int, int>, Tile> GetTilesByProperty(Func<Tile, bool> propertyCondition)
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

    public static Tuple<int, int> Tuple(int x, int y)
    {
        return new Tuple<int, int>(x, y);
    }

    public static void RevealEmptySquares(ToolStripMenuItem display, int x, int y)
    {
        if (x < 0 || x >= Game.GridX || y < 0 || y >= Game.GridY || Game.Tiles[x, y].IsRevealed)
            return;

        Tile tile = Game.Tiles[x, y];

        if (tile.IsFlagged)
            TileUtils.ToggleFlag(display, x, y);

        tile.Reveal();

        if (TileUtils.CountAdjacentBombs(x, y) == 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newX = x + i;
                    int newY = y + j;

                    RevealEmptySquares(display, newX, newY);
                }
            }
        }
    }

    public static void ToggleFlag(ToolStripMenuItem display, int x, int y)
    {
        Tile tile = Game.Tiles[x, y];

        if (tile.IsRevealed)
            return; // dont flag spots that are already exposed

        tile.Flag(!tile.IsFlagged);

        Game.BombsLeft += tile.IsFlagged ? -1 : 1;

        display.Text = Game.BombsLeft.ToString();
    }
}