using System;
using System.Drawing;

public class NoBombPowerup : Plugin
{
    public static int BombPowerups = 1;

    public const int NoBombs = 2;

    public override void Initialize()
    {
        PluginTunnel.Log("Loaded plugin " + Name + " on API version " + API.Version);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override void OnGameStart()
    {
        Random random = new Random();

        for (int i = 0; i < BombPowerups;)
        {
            int x = random.Next(0, Game.GridX);
            int y = random.Next(0, Game.GridY);

            var bombs = TileUtils.GetTilesByProperty(tile => tile.Id == Tile.Bomb);
            var nobombs = TileUtils.GetTilesByProperty(tile => tile.Id == NoBombs);

            if (!bombs.ContainsKey(TileUtils.Tuple(x, y)) && !nobombs.ContainsKey(TileUtils.Tuple(x, y)))
            {
                Game.Tiles[x, y].Id = NoBombs; // set the tile ID
                //Game.Tiles[x, y].IsRevealed = true;
                i++;
            }
        }
    }

    public override void OnTileClicked(Tuple<int, int> clicked, Tile _tile)
    {
        if (_tile.Id == NoBombs)
        {
            _tile.Id = Tile.Empty; // remove powerup

            TileUtils.GetFirst(tile => tile.Id == Tile.Bomb).Id = Tile.Empty; // remove bomb
            Game.BombsLeft--;

            PluginTunnel.RedrawGame();
        }
    }

    public override void OnTileRender(Graphics g, int x, int y, Tile tile, ref bool cancel)
    {
        cancel = true;

        Font Font = PluginTunnel.GetWin().Font;

        Rectangle cellRect = new Rectangle(x * Game.CellSize, y * Game.CellSize, Game.CellSize, Game.CellSize);
        string coordinates = (x + 1).ToString() + (char)('A' + y);

        if (tile.IsRevealed)
        {
            if (tile.Id == NoBombs)
            {
                g.FillRectangle(Brushes.Blue, cellRect);
            }
            else Tile.DrawRevealed(g, Font, tile, TileUtils.Tuple(x, y), cellRect); // draw original callbacks
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
}