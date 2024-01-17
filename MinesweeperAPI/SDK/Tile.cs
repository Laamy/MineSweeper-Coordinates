public class Tile
{
    public int Id { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }

    public void Reveal() => IsRevealed = true;
    public void Flag(bool value) => IsFlagged = value;

    public const int Bomb = 1; // ID's
    public const int Empty = 1;
}