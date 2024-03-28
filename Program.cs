using System.Runtime.CompilerServices;

Game game = new Game();
game.Run();

public class Game
{
    public IRoom[,] Rooms { get; }
    public Player Player { get; } = new Player();
    public int Rows { get; } = 4;
    public int Cols { get; } = 4;

    public Game()
    {
        Rooms = new IRoom[,]
        {
            {new EntranceRoom(), new EmptyRoom(), new FountainOfObjectsRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
            {new EmptyRoom(), new EmptyRoom(), new EmptyRoom(), new EmptyRoom() },
        };
    }

    public void Run()
    {
        PrintIntro();
        
        while (true)
        {
            Console.WriteLine("----------------------------------------------------------------------------------");
            Console.WriteLine($"You are in the room at (Row={Player.Row} Column={Player.Col})");
                        
            if (HasWon())
            {
                ColoredText.PrintNarrativeText("The Fountain of Objects has been reactivated, and you have escaped with your life!");
                ColoredText.PrintNarrativeText("You win!");
                break;
            }

            ColoredText.PrintRoomText(Rooms[Player.Row, Player.Col]);
            
            try
            {
                IAction action = Player.askForAction();
                if (!Player.executeAction(this, action))
                    ColoredText.PrintErrorText("Cannot do this action here.");
            }
            catch             {
                ColoredText.PrintErrorText("Invalid input.");
            }
        }
    }

    private void PrintIntro()
    {
        ColoredText.PrintNarrativeText("Welome to the The Fountain of Objects!");
        ColoredText.PrintNarrativeText("Unnatural darkness pervades the caverns, preventing both natural and human-made light. You must navigate the caverns in the dark.");
        ColoredText.PrintNarrativeText("To escape you must find and enable the Fountain of Objects. But beware, dangers may lurk in every room.");
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

    public IAction askForAction()
    {
        ColoredText.PrintPromptText("What do you want to do? ");
        IAction action;
        string? input = Console.ReadLine();
        ColoredText.Reset();
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

    public override string ToString()
    {
        return $"X: {Row} Y: {Col}";
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

public class ColoredText
{

    public static void Reset()
    {
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    public static void PrintRoomText(IRoom room)
    {
        Type roomType = room.GetType();
        Console.ForegroundColor = roomType switch
        {
            _ when roomType == typeof(FountainOfObjectsRoom) => ConsoleColor.Blue,
            _ when roomType == typeof(EntranceRoom) => ConsoleColor.Yellow,
            _ => ConsoleColor.White,
        };
        Console.WriteLine(room);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintNarrativeText(string text)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintPromptText(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
    }

    public static void PrintErrorText (string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }
}