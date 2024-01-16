class Level
{
    public int Bombs;
    public int GridX;
    public int GridY;
}

class MinesweeperLevels
{
    public Level Beginner;
    public Level Intermediate;
    public Level Expert;

    private static MinesweeperLevels _instance;

    public static MinesweeperLevels Get()
    {
        if (_instance != null)
            return _instance;

        _instance = new MinesweeperLevels()
        {
            Beginner = new Level()
            {
                Bombs = 9,
                GridX = 10,
                GridY = 10
            },
            Intermediate = new Level()
            {
                Bombs = 40,
                GridX = 15,
                GridY = 13
            },
            Expert = new Level()
            {
                Bombs = 99,
                GridX = 30,
                GridY = 16
            }
        };

        return _instance;
    }
}