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
    private readonly int _maxColumn;
    private readonly Stack<Point> _positions = new Stack<Point>();
    private Dictionary<Point, bool> _searchCache;
    private GoingBackHome _goingBackHome;
    private Point? _startingPoint;

    public Map(int rows, int column)
    {
        _maxColumn = column;
        _rows = new string[rows];

        _positions = new Stack<Point>();
        _searchCache = new Dictionary<Point, bool>();
    }

    public int Rows => _rows.Length;
    public int Columns => _maxColumn;

    public void SetRow(string row, int i)
    {
        _rows[i] = row;
    }

    public string DecideWhereToGo(Point current)
    {
        if (_startingPoint == null)
            _startingPoint = current;

        SetFoundControlRoom(current);

        if (_goingBackHome != null)
        {
            return _goingBackHome.Do(current);

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

    public static string FormatResponse(Direction resultDirection)
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

    public char At(int row, int column)
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
            _goingBackHome = new GoingBackHome(this, currentPosition);
            _goingBackHome.FillDynamicProgramming(_startingPoint.Value);
        }
    }

    public override string ToString()
    {
        return string.Join('\n', _rows);
    }
}

public class GoingBackHome
{
    private readonly int?[,] _dynamicProgramming;
    private readonly Point _returnPoint;
    private Map _map;
    private Point _exit;

    public GoingBackHome(Map map, Point currentPosition)
    {
        _map = map;
        _returnPoint = currentPosition;
        _dynamicProgramming = new int?[map.Rows, map.Columns];

        _dynamicProgramming[_returnPoint.Row, _returnPoint.Column] = int.MaxValue;
    }

    public void FillDynamicProgramming(Point exit)
    {
        _exit = exit;
        _dynamicProgramming[exit.Row, exit.Column] = 0;
        Bfs(exit, 0);
    }

    private void Bfs(Point point, int steps)
    {
        if (IsOutsideRange(point))
            return;

        var existingValue = _dynamicProgramming[point.Row, point.Column];
        if (existingValue > 0 && steps > existingValue || steps > 0 && point.Equals(_exit))
        {
            return;
        }

        var charAt = _map.At(point.Row, point.Column);
        if (charAt == '?' || charAt == '#' || charAt == 'C')
        {
            _dynamicProgramming[point.Row, point.Column] = int.MaxValue;
            return;
        }
        if (charAt == 'T' && steps > 0)
        {
            _dynamicProgramming[point.Row, point.Column] = 0;
            return;
        }
        
        SetDp(point, steps);

        steps++;

        Bfs(new Point(point, Direction.UP), steps);
        Bfs(new Point(point, Direction.DOWN), steps);
        Bfs(new Point(point, Direction.RIGHT), steps);
        Bfs(new Point(point, Direction.LEFT), steps);
    }

    private void SetDp(Point point, int steps)
    {
        _dynamicProgramming[point.Row, point.Column] = steps;
    }

    private bool IsOutsideRange(Point point)
    {
        var row = point.Row;
        if (row < 0 || row >= _map.Rows)
            return true;

        var column = point.Column;
        return column < 0 || column >= _map.Columns;
    }

    public string Do(Point current)
    {
        var results = new[]
        {
            new ScoreResult(DynamicAt(new Point(current, Direction.DOWN)) ?? int.MaxValue, Direction.DOWN),
            new ScoreResult(DynamicAt(new Point(current, Direction.UP)) ?? int.MaxValue, Direction.UP),
            new ScoreResult(DynamicAt(new Point(current, Direction.LEFT)) ?? int.MaxValue, Direction.LEFT),
            new ScoreResult(DynamicAt(new Point(current, Direction.RIGHT)) ?? int.MaxValue, Direction.RIGHT),
        };

        var direction = results.OrderBy(x => x.Score).First().Direction;
        return Map.FormatResponse(direction);
    }

    private int? DynamicAt(Point point)
    {
        if (IsOutsideRange(point))
            return null;

        return _dynamicProgramming[point.Row, point.Column];
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