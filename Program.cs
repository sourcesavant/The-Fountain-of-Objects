Renderer renderer = new Renderer();
Game game = new Game(renderer, new PlayerInput(renderer));
game.Run();

public class Game
{
    private readonly Renderer _renderer;
    private readonly PlayerInput _playerInput;

    public IRoom[,] Rooms { get; }
    public Player Player { get; } = new Player();
    public int Rows { get; } = 4;
    public int Cols { get; } = 4;

    public Game(Renderer renderer, PlayerInput playerInput)
    {
        Rooms = new IRoom[,]
        {
            {new EntranceRoom(), new EmptyRoom(), new FountainOfObjectsRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
        };
        _renderer = renderer;
        _playerInput = playerInput;
    }

    public void Run()
    {
        _renderer.PrintGameIntro();
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
        try
        {
            IAction action = _playerInput.askForAction();
            if (!Player.executeAction(this, action))
                _renderer.PrintError("Cannot do this action here.");
        }
        catch
        {
            _renderer.PrintError("Invalid input.");
        }
    }

    private bool HasWon()
    {
        FountainOfObjectsRoom? room = Rooms[0, 2] as FountainOfObjectsRoom;
        if (room != null && Player.Row == 0 && Player.Col == 0 && room.IsEnabled)
            return true;
        return false;
    }
}

public class Player
{
    public int Row { get; set; } = 0;
    public int Col { get; set; } = 0;

    public bool executeAction (Game game, IAction action)
    {
        return action.execute(game);
    }
}

public class PlayerInput
{
    private readonly Renderer _renderer;

    public PlayerInput(Renderer renderer)
    {
        _renderer = renderer;
    }

    public IAction askForAction()
    {
        _renderer.PrintPromptText("What do you want to do? ");
        IAction action;
        string? input = Console.ReadLine();
        _renderer.ResetColor();
        if (input != null)
        {
            action = input switch
            {
                "move north" => new MoveNorth(),
                "move east" => new MoveEast(),
                "move south" => new MoveSouth(),
                "move west" => new MoveWest(),
                "enable fountain" => new EnableFountain(),
                _ => throw new Exception()
            };
            return action;
        }
        throw new Exception();
    }
}

public interface IAction
{
    public bool execute(Game game);
}

public class MoveNorth : IAction
{
    public bool execute(Game game)
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
    public bool execute(Game game)
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
    public bool execute(Game game)
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
    public bool execute(Game game)
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
    public bool execute(Game game)
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
        Type roomType = room.GetType();
        Console.ForegroundColor = roomType switch
        {
            _ when roomType == typeof(FountainOfObjectsRoom) => ConsoleColor.Blue,
            _ when roomType == typeof(EntranceRoom) => ConsoleColor.Yellow,
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

public class ColoredText
{

    public static void Reset()
    {
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    
    public static void PrintPromptText(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
    }
}