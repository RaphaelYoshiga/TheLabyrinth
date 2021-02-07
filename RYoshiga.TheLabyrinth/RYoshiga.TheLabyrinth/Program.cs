using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public struct Point
{
    public Point(int row, int column)
    {
        Row = row;

        Column = column;
    }

    public Point(Point current, Direction direction)
    {
        Column = current.Column;
        Row = current.Row;

        switch (direction)
        {
            case Direction.LEFT:
                Column--;
                break;
            case Direction.RIGHT:
                Column++;
                break;
            case Direction.DOWN:
                Row++;
                break;
            case Direction.UP:
                Row--;
                break;
        }
    }

    public int Column { get; }
    public int Row { get; }

    public Direction FindDirection(Point previous)
    {
        if (this.Row > previous.Row)
            return Direction.UP;

        if (this.Row < previous.Row)
            return Direction.DOWN;

        if (this.Column > previous.Column)
            return Direction.LEFT;

        return Direction.RIGHT;
    }
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int R = int.Parse(inputs[0]); // number of rows.
        int C = int.Parse(inputs[1]); // number of columns.
        int A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

        var map = new Map(R, C);
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int currentRow = int.Parse(inputs[0]); // row where Kirk is located.
            int currentColumn = int.Parse(inputs[1]); // column where Kirk is located.

            for (int i = 0; i < R; i++)
            {
                string row = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                map.SetRow(row, i);
            }

            Console.Error.WriteLine(map.ToString());
            Console.Error.WriteLine("END");

            var command = map.DecideWhereToGo(new Point(currentRow, currentColumn));
            Console.WriteLine(command);

        }
    }

  
}

public class Map
{
    private readonly HashSet<Point> _visited = new HashSet<Point>();
    private const char WALL = '#';
    private readonly string[] _rows;
    private bool _controlRoomFound;
    private readonly int _maxColumn;
    private readonly Stack<Point> _positions = new Stack<Point>();
    private Dictionary<Point, bool> _searchCache;

    public Map(int rows, int column)
    {
        _maxColumn = column;
        _rows = new string[rows];

        _positions = new Stack<Point>();
        _searchCache = new Dictionary<Point, bool>();
    }

    public void SetRow(string row, int i)
    {
        _rows[i] = row;
    }

    public string DecideWhereToGo(Point current)
    {
        SetFoundControlRoom(current);

        if (_controlRoomFound)
        {
            var tryPop = _positions.TryPop(out Point result);
            if (tryPop)
            {
                var direction = current.FindDirection(result);
                return FormatResponse(direction);
            }

            return "DONT KNOW";
        }

        _searchCache = new Dictionary<Point, bool>();

        var dfsResult = DFS(current, 0);

        _positions.Push(current);
        _visited.Add(current);

        return FormatResponse(dfsResult.Direction);
    }

    private int CalculateDiscoveredCells(Point current)
    {
        var range = new[] {
            new Point(current.Row + 1, current.Column),
            new Point(current.Row + 2, current.Column),

            new Point(current.Row - 1, current.Column),
            new Point(current.Row - 2, current.Column),

            new Point(current.Row, current.Column + 1),
            new Point(current.Row, current.Column + 2),

            new Point(current.Row, current.Column - 1),
            new Point(current.Row, current.Column - 2),



            new Point(current.Row + 1, current.Column + 1),
            new Point(current.Row + 1, current.Column + 2),
            new Point(current.Row + 2, current.Column + 1),
            new Point(current.Row + 2, current.Column + 2),


            new Point(current.Row + 1, current.Column - 1),
            new Point(current.Row + 1, current.Column - 2),
            new Point(current.Row + 2, current.Column - 1),
            new Point(current.Row + 2, current.Column - 2),


            new Point(current.Row - 1, current.Column + 1),
            new Point(current.Row - 1, current.Column + 2),
            new Point(current.Row - 2, current.Column + 1),
            new Point(current.Row - 2, current.Column + 2),


            new Point(current.Row - 1, current.Column - 1),
            new Point(current.Row - 1, current.Column - 2),
            new Point(current.Row - 2, current.Column - 1),
            new Point(current.Row - 2, current.Column - 2),

        };
        return range.Count(x => At(x.Row, x.Column) == '?');
    }

    private static string FormatResponse(Direction resultDirection)
    {
        return resultDirection.ToString().ToUpper();
    }

    private ScoreResult DFS(Point current, int score)
    {
        if (_searchCache.ContainsKey(current))
            return new ScoreResult(-1, Direction.UP);

        if (_visited.Contains(current))
            return new ScoreResult(-1, Direction.UP);

        _searchCache[current] = true;
        var charAt = At(current.Row, current.Column);
        if (charAt == WALL)
            return new ScoreResult(-1, Direction.UP);

        if (charAt == 'C')
            return new ScoreResult(int.MaxValue, Direction.UP);

        score += 1 + CalculateDiscoveredCells(current);
        if (charAt == '?')
        {
            return new ScoreResult(score, Direction.UP);
        }

        var results = new ScoreResult[4]
        {
            GetScore(current, score, Direction.LEFT),
            GetScore(current, score, Direction.RIGHT),
            GetScore(current, score, Direction.UP),
            GetScore(current, score, Direction.DOWN),
        };

        var scoreResult = results.OrderByDescending(x => x.Score).First();

        return new ScoreResult(scoreResult.Score, scoreResult.Direction);
    }

    private ScoreResult GetScore(Point current, int score, Direction direction)
    {
        var dfsScore = DFS(new Point(current, direction), score).Score;
        return new ScoreResult(dfsScore, direction);
    }

    private char At(int row, int column)
    {
        if (row <= 0 || row >= _rows.Length)
            return WALL;

        if (column <= 0 || column >= _maxColumn)
            return WALL;

        return _rows[row][column];
    }

    private void SetFoundControlRoom(Point currentPosition)
    {
        if (At(currentPosition.Row, currentPosition.Column) == 'C')
        {
            _controlRoomFound = true;
        }
    }

    public override string ToString()
    {
        return string.Join('\n', _rows);
    }
}

internal struct ScoreResult
{
    public int Score { get; }
    public Direction Direction { get; }

    public ScoreResult(int score, Direction direction)
    {
        Score = score;
        Direction = direction;
    }
}

public enum Direction
{
    LEFT,
    RIGHT,
    DOWN,
    UP,
}