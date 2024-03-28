Renderer renderer = new Renderer();
Game game = new Game(renderer, new PlayerInput(renderer));
game.Run();

public class Game
{
    private readonly Renderer _renderer;
    private readonly PlayerInput _playerInput;
    private int _rowOfFountain;
    private int _colOfFountain;

    public IRoom[,] Rooms { get; private set; }
    public Player Player { get; } = new Player();
    public int Rows { get; private set; }
    public int Cols { get; private set; }

    public Game(Renderer renderer, PlayerInput playerInput)
    {
        _renderer = renderer;
        _playerInput = playerInput;
    }

    private void CreateRoom(LevelSize levelSize)
    {
        (Rows, Cols, _rowOfFountain, _colOfFountain) = levelSize switch
        {
            LevelSize.Small  => (4, 4, 0, 2),
            LevelSize.Medium => (6, 6, 1, 4),
            LevelSize.Large  => (8, 8, 3, 6),
            _ => throw new ArgumentOutOfRangeException(nameof(levelSize))
        };
        
        Rooms = new IRoom[Rows, Cols];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
                Rooms[i, j] = new EmptyRoom();
        }
        Rooms[0, 0] = new EntranceRoom();
        Rooms[_rowOfFountain, _colOfFountain] = new FountainOfObjectsRoom();
    }

    public void Run()
    {
        _renderer.PrintGameIntro();
        CreateRoom(AskForLevelSize());
        GameLoop();
    }

    private void GameLoop()
    {
        while (true)
        {
            _renderer.PrintRoundIntro(Player.Row, Player.Col);

            if (HasWon())
            {
                _renderer.PrintWinMessage();
                break;
            }

            _renderer.PrintRoomDescription(Rooms[Player.Row, Player.Col]);
            AskForPlayerAction();
        }
    }

    
    private void AskForPlayerAction()
    {
        IAction action = _playerInput.AskForAction();
        if (!Player.ExecuteAction(this, action))
            _renderer.PrintError("Cannot do this action here.");
    
    }

    private LevelSize AskForLevelSize()
    {
        return _playerInput.AskForLevelSize();
    }

    private bool HasWon()
    {
        FountainOfObjectsRoom? room = Rooms[_rowOfFountain, _colOfFountain] as FountainOfObjectsRoom;
        EntranceRoom? playerRoom = Rooms[Player.Row, Player.Col] as EntranceRoom;
        if (room != null && room.IsEnabled && playerRoom != null )
            return true;
        return false;
    }
}

public class Player
{
    public int Row { get; set; } = 0;
    public int Col { get; set; } = 0;

    public bool ExecuteAction (Game game, IAction action)
    {
        return action.Execute(game);
    }
}

public class PlayerInput
{
    private readonly Renderer _renderer;

    public PlayerInput(Renderer renderer)
    {
        _renderer = renderer;
    }

    public IAction AskForAction()
    {        
        while (true)
        {
            try
            {
                _renderer.PrintPromptText("What do you want to do? ");
                string input = Console.ReadLine() ?? throw new InvalidInputException("Invalid input. Input is null.");
                _renderer.ResetColor();

                return input switch
                {
                    "move north" => new MoveNorth(),
                    "move east" => new MoveEast(),
                    "move south" => new MoveSouth(),
                    "move west" => new MoveWest(),
                    "enable fountain" => new EnableFountain(),
                    _ => throw new InvalidInputException("Invalid input. Please enter 'move north', 'move east', 'move south', 'move west' or 'enable fountain'.")
                };
            }
            catch (InvalidInputException e)
            {
                _renderer.PrintError(e.Message);
            }
        }
    }

    public LevelSize AskForLevelSize()
    {
        while (true)
        {
            try
            {
                _renderer.PrintPromptText("Do you want to play a small, medium or large game? ");
                string input = Console.ReadLine() ?? throw new InvalidInputException("Invald input. Input is null");
                _renderer.ResetColor();

                return input switch
                {
                    "small" => LevelSize.Small,
                    "medium" => LevelSize.Medium,
                    "large" => LevelSize.Large,
                    _ => throw new InvalidInputException("Invalid input. Please enter 'small', 'medium', or 'large'.")
                };
            }
            catch (InvalidInputException e)
            {
                _renderer.PrintError(e.Message);
            }
        }
    }
}

public interface IAction
{
    public bool Execute(Game game);
}

public class MoveNorth : IAction
{
    public bool Execute(Game game)
    {
        if (game.Player.Row < game.Rows - 1)
        {
            game.Player.Row += 1;
            return true;
        }
        return false;
    }
}

public class MoveEast : IAction
{
    public bool Execute(Game game)
    {
        if (game.Player.Col < game.Cols - 1)
        {
            game.Player.Col += 1;
            return true;
        }
        return false;
    }
}

public class MoveSouth : IAction
{
    public bool Execute(Game game)
    {
        if (game.Player.Row > 0)
        {
            game.Player.Row -= 1;
            return true;
        }
        return false;
    }
}

public class MoveWest : IAction
{
    public bool Execute(Game game)
    {
        if (game.Player.Col > 0)
        {
            game.Player.Col -= 1;
            return true;
        }
        return false;
    }
}

public class EnableFountain : IAction
{
    public bool Execute(Game game)
    {
        FountainOfObjectsRoom? room = game.Rooms[game.Player.Row, game.Player.Col] as FountainOfObjectsRoom;
        if (room != null)
        {
            room.IsEnabled = true;
            return true;
        }
        return false;
    }
}

public interface IRoom
{
    
}

public class EmptyRoom : IRoom
{
    public override string ToString()
    {
        return "You do not sense anything. The room appears to be empty.";
    }
}

public class FountainOfObjectsRoom : IRoom
{
    public bool IsEnabled = false;

    public override string ToString()
    {
        if (IsEnabled)
            return "You hear the rushing waters from the Fountain of Objects.It has been reactivated!";
        else
            return "You hear water dripping in this room. The Fountain of Objects is here!";
    }
}

public class EntranceRoom : IRoom
{
    public override string ToString()
    {
        return "You see light coming from the cavern entrance.";
    }
}

public class Renderer
{
    public void PrintGameIntro()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Welome to the The Fountain of Objects!");
        Console.WriteLine("Unnatural darkness pervades the caverns, preventing both natural and human-made light. You must navigate the caverns in the dark.");
        Console.WriteLine("To escape you must find and enable the Fountain of Objects. But beware, dangers may lurk in every room.");
        ResetColor();
    }

    public void PrintRoundIntro(int playerRow, int playerCol)
    {
        Console.WriteLine("----------------------------------------------------------------------------------");
        Console.WriteLine($"You are in the room at (Row={playerRow} Column={playerCol})");
    }

    public void PrintWinMessage()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("The Fountain of Objects has been reactivated, and you have escaped with your life!");
        Console.WriteLine("You win!");
        ResetColor();
    }

    public void PrintRoomDescription(IRoom room)
    {
        Console.ForegroundColor = room switch
        {
            FountainOfObjectsRoom => ConsoleColor.Blue,
            EntranceRoom => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        Console.WriteLine(room);
        ResetColor();
    }

    public void PrintError (string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        ResetColor();
    }

    public void PrintPromptText(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
    }

    public void ResetColor()
    {
        Console.ForegroundColor = ConsoleColor.White;
    }
}

public enum LevelSize
{
    Small,
    Medium,
    Large
}

public class InvalidInputException : Exception
{
    public InvalidInputException(string message) : base(message) { }
}