﻿Renderer renderer = new Renderer();
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

        if (levelSize == LevelSize.Small)
        {
            Rooms[0, 1] = new PitRoom();
            Rooms[1, 1] = new MaelstromRoom();
            Rooms[3, 1] = new AmarokRoom();
        }
        else if (levelSize == LevelSize.Medium)
        {
            (Rooms[1, 3], Rooms[2, 4]) = (new PitRoom(), new PitRoom());
            Rooms[3, 3] = new MaelstromRoom();
            (Rooms[0, 2], Rooms[4, 1]) = (new PitRoom(), new AmarokRoom());
        }
        else if (levelSize == LevelSize.Large)
        {
            (Rooms[2, 6], Rooms[0, 2], Rooms[3, 4], Rooms[6, 4]) = (new PitRoom(), new PitRoom(), new PitRoom(), new PitRoom());
            (Rooms[2, 1], Rooms[5, 3]) = (new MaelstromRoom(), new MaelstromRoom());
            (Rooms[2, 0], Rooms[4, 2], Rooms[5, 6]) = (new AmarokRoom(), new AmarokRoom(), new AmarokRoom());
        }
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
            _renderer.PrintRoundIntro(Player.Row, Player.Col, Player.Arrows);

            if (HasWon())
            {
                _renderer.PrintWinMessage();
                break;
            }

            _renderer.PrintRoomDescription(Rooms[Player.Row, Player.Col]);

            if (HasLost())
            {
                _renderer.PrintLostMessage();
                break;
            }

            if (HasEncounteredMaelstrom())
            {
                HandleMaelstromEncounter();
            }

            Sense();

            AskForPlayerAction();
        }
    }

    private void HandleMaelstromEncounter()
    {
        Player.ExecuteAction(this, new MoveNorth());
        Player.ExecuteAction(this, new MoveEast());
        Player.ExecuteAction(this, new MoveEast());
        _renderer.PrintMovedByMaelstrom();
        _renderer.PrintRoundIntro(Player.Row, Player.Col, Player.Arrows);
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

    private void Sense()
    {
        // North
        if (Player.Row < Rows - 1)
            SenseRoom(Rooms[Player.Row + 1, Player.Col]);
        // North East
        if (Player.Row < Rows - 1 && Player.Col < Cols - 1)
            SenseRoom(Rooms[Player.Row + 1, Player.Col + 1]);
        // East
        if (Player.Col < Cols - 1)
            SenseRoom(Rooms[Player.Row, Player.Col + 1]);
        // South East
        if (Player.Row > 0 && Player.Col < Cols - 1)
            SenseRoom(Rooms[Player.Row - 1, Player.Col + 1]);
        // South
        if (Player.Row > 0)
            SenseRoom(Rooms[Player.Row - 1, Player.Col]);
        // South West
        if (Player.Row > 0 && Player.Col > 0)
            SenseRoom(Rooms[Player.Row - 1, Player.Col - 1]);
        // West
        if (Player.Row > 0 && Player.Col > 0)
            SenseRoom(Rooms[Player.Row, Player.Col - 1]);
        // North West
        if (Player.Row < Rows - 1 && Player.Col > 0)
            SenseRoom(Rooms[Player.Row + 1, Player.Col - 1]);
    }

    private void SenseRoom(IRoom room)
    {
        switch (room.RoomType)
        {
            case RoomType.Pit:
                _renderer.PrintSensePit();
                break;
            case RoomType.Maelstrom:
                _renderer.PrintSenseMaelstrom();
                break;
            case RoomType.Amarok:
                _renderer.PrintSenseAmarok();
                break;
        }
    }

    private bool HasWon()
    {
        FountainOfObjectsRoom? room = Rooms[_rowOfFountain, _colOfFountain] as FountainOfObjectsRoom;
        EntranceRoom? playerRoom = Rooms[Player.Row, Player.Col] as EntranceRoom;
        return room != null && room.IsEnabled && playerRoom != null;
    }

    private bool HasLost() => Rooms[Player.Row, Player.Col] switch
    {
        PitRoom => true,
        AmarokRoom => true,
        _ => false,
    };

    private bool HasEncounteredMaelstrom() => Rooms[Player.Row, Player.Col] switch
    {
        MaelstromRoom => true,
        _ => false,
    };
}

public class Player
{
    public int Row { get; set; } = 0;
    public int Col { get; set; } = 0;

    public int Arrows { get; set; } = 5;

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
                    "shoot nord" => new ShootNorth(),
                    "shoot east" => new ShootEast(),
                    "shoot south" => new ShootSouth(),
                    "shoot west" => new ShootWest(),
                    "help" => new Help(_renderer),
                    _ => throw new InvalidInputException("Invalid input. Please enter 'move (north, east, south, west)', 'enable fountain' or 'shoot (north, east, south, west)'.")
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

public class Help : IAction
{
    private Renderer _renderer;
    
    public Help (Renderer renderer)
    {
        _renderer = renderer;
    }   

    public bool Execute(Game game)
    {
        _renderer.PrintHelpMessage();
        return true;
    }
}

public interface IRoom
{
    public RoomType RoomType { get; }
}


public class EmptyRoom : IRoom
{
    public RoomType RoomType { get; } = RoomType.Empty;

    public override string ToString()
    {
        return "The room appears to be empty.";
    }
}

public class FountainOfObjectsRoom : IRoom
{
    public RoomType RoomType { get; } = RoomType.FountainOfObjects;

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
    public RoomType RoomType { get; } = RoomType.Entrance;

    public override string ToString()
    {
        return "You see light coming from the cavern entrance.";
    }
}

public class PitRoom : IRoom
{
    public RoomType RoomType { get; } = RoomType.Pit;

    public override string ToString()
    {
        return "You fall into a pit and you die.";
    }
}

public class AmarokRoom : IRoom
{
    public RoomType RoomType { get; } = RoomType.Amarok;

    public override string ToString()
    {
        return "You encounter an amarok and you die.";
    }
}

public class MaelstromRoom : IRoom
{
    public RoomType RoomType { get; } = RoomType.Maelstrom;

    public override string ToString()
    {
        return "You encounter the maelstrom - a sentient, malevolent wind.";
    }
}

public class Renderer
{
    public void PrintGameIntro()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("You enter the Cavern of Objects, a maze of rooms filled with dangerous pits in search of the Fountain of Objects.");
        Console.WriteLine("Light is visible only in the entrance, and no other light is seen anywhere in the caverns.");
        Console.WriteLine("You must navigate the Caverns with your other senses.");
        Console.WriteLine("Look out for pits. You will feel a breeze if a pit is in an adjacent room. If you enter a room with a pit, you will die.");
        Console.WriteLine("Maelstroms are violent forces of sentient wind. Entering a room with one could transport you to any other location in the caverns. You will be able to hear their growling and groaning in nearby rooms.");
        Console.WriteLine("Amaroks roam the caverns. Encountering one is certain death, but you can smell their rotten stench in nearby rooms.");
        Console.WriteLine("You carry with you a bow and a quiver of arrows. You can use them to shoot monsters in the caverns but be warned: you have a limited supply.");
        Console.WriteLine("Find the Fountain of Objects, activate it, and return to the entrance.");
        ResetColor();
    }

    public void PrintRoundIntro(int playerRow, int playerCol, int arrows)
    {
        Console.WriteLine("----------------------------------------------------------------------------------");
        Console.WriteLine($"You are in the room at (Row={playerRow} Column={playerCol}). You have {arrows} arrows.");
    }

    public void PrintWinMessage()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("The Fountain of Objects has been reactivated, and you have escaped with your life!");
        Console.WriteLine("You win!");
        ResetColor();
    }

    public void PrintLostMessage()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("You lost!");
        ResetColor();
    }

    public void PrintRoomDescription(IRoom room)
    {
        Console.ForegroundColor = room switch
        {
            FountainOfObjectsRoom => ConsoleColor.Blue,
            EntranceRoom => ConsoleColor.Yellow,
            PitRoom => ConsoleColor.DarkYellow,
            MaelstromRoom => ConsoleColor.DarkYellow,
            AmarokRoom => ConsoleColor.DarkYellow,
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

    public void PrintSensePit()
    {
        Console.WriteLine("You feel a draft. There is a pit in a nearby room.");
    }

    public void PrintSenseMaelstrom()
    {
        Console.WriteLine("You hear the growling and groaning of a maelstrom nearby.");
    }

    public void PrintSenseAmarok()
    {
        Console.WriteLine("You can smell the rotten stench of an amarok in a nearby room.");
    }

    public void PrintMovedByMaelstrom()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("The malestrom moves you to another room.");
        ResetColor();
    }

    public void PrintHelpMessage()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Help: Available commands:");
        Console.WriteLine("- move north, move south, move east, move west - moves one room in the given direction");
        Console.WriteLine("- shoot north, shoot south, shoot east, shoot west - shoots in the given direction)");
        Console.WriteLine("- enable fountain - enables the Fountain of Objects");
        Console.WriteLine("- help (displays this message)");
        ResetColor();
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

public enum RoomType
{
    Empty,
    Entrance,
    FountainOfObjects,
    Pit,
    Maelstrom,
    Amarok
}

abstract public class Shoot : IAction
{

    public bool Execute(Game game)
    {
        if (game.Player.Arrows > 0)
            return DoShot(game);
        return false;   
    }

    protected abstract bool DoShot(Game game);

    protected bool hasTarget(IRoom room) => room is MaelstromRoom || room is AmarokRoom;

    protected void ShootAtOffset(Game game, int rowOffset, int colOffset)
    {
        game.Player.Arrows--;
        IRoom room = game.Rooms[game.Player.Row + rowOffset, game.Player.Col + colOffset];
        if (hasTarget(room))
            game.Rooms[game.Player.Row + rowOffset, game.Player.Col + colOffset] = new EmptyRoom();
    }
}

public class ShootNorth : Shoot
{
    protected override bool DoShot(Game game)
    {
        if (game.Player.Row < game.Rows - 1)
        {
            ShootAtOffset(game, 1, 0);
            return true;
        }
        return false;
    }
}

public class ShootSouth : Shoot
{
    protected override bool DoShot(Game game)
    {
        if (game.Player.Row > 0)
        {
            ShootAtOffset(game, -1, 0);
            return true;
        }
        return false;
    }
}

public class ShootEast : Shoot
{
    protected override bool DoShot(Game game)
    {
        if (game.Player.Col < game.Cols - 1)
        {
            ShootAtOffset(game, 0, 1);
            return true;
        }
        return false;
    }
}

public class ShootWest : Shoot
{
    protected override bool DoShot(Game game)
    {
        if (game.Player.Col > 0)
        {
            ShootAtOffset(game, 0, -1);
            return true;
        }
        return false;
    }
}

public class InvalidInputException : Exception
{
    public InvalidInputException(string message) : base(message) { }
}