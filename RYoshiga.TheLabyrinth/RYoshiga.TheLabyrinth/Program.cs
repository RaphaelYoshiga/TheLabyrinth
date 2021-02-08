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
    private SimplePathFinder _simplePathFinder;
    private Point? _startingPoint;
    private readonly ScanResult[,] _scanDp;
    private readonly Radar _radar;
    private SimplePathFinder _beforeArrivingFinder;

    public Map(int rows, int column)
    {
        _maxColumn = column;
        _rows = new string[rows];

        _scanDp = new ScanResult[Rows, Columns];

        _positions = new Stack<Point>();
        _searchCache = new Dictionary<Point, bool>();
        _radar = new Radar(this);
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

        SetArrivedAtControlRoom(current);

        if (_beforeArrivingFinder != null && _simplePathFinder == null)
        {
            return _beforeArrivingFinder.Do(current);
        }

        if (_simplePathFinder != null)
        {
            Console.Error.WriteLine($"Simple path finder return");
            return _simplePathFinder.Do(current);
        }

        var controlRoomRow = _rows.FirstOrDefault(x => x.Contains("C"));
        if (controlRoomRow != null)
        {
            Console.Error.WriteLine($"Trying to find path");

            var rowIndex = Array.IndexOf(_rows, controlRoomRow);
            var charIndex = controlRoomRow.IndexOf('C');
            var controlRoom = new Point(rowIndex, charIndex);
            return FindPath(current, controlRoom);


        }

        _searchCache = new Dictionary<Point, bool>();

        Dfs(current, 0);

        _positions.Push(current);
        _visited.Add(current);


        return Do(current);
    }

    private string FindPath(Point current, Point controlRoom)
    {
        _beforeArrivingFinder = new SimplePathFinder(this, current, 'C');
        _beforeArrivingFinder.FillDynamicProgramming(controlRoom);

        return _beforeArrivingFinder.Do(current);
    }

    public string Do(Point current)
    {
        var results = new[]
        {
            new ScoreResult(DynamicAt(new Point(current, Direction.DOWN)) , Direction.DOWN),
            new ScoreResult(DynamicAt(new Point(current, Direction.UP)), Direction.UP),
            new ScoreResult(DynamicAt(new Point(current, Direction.LEFT)), Direction.LEFT),
            new ScoreResult(DynamicAt(new Point(current, Direction.RIGHT)), Direction.RIGHT),
        };

        var direction = results.OrderByDescending(x => x.Score).First().Direction;
        return FormatResponse(direction);
    }

    private int DynamicAt(Point point)
    {
        if (IsOutsideRange(point))
            return int.MinValue;

        var score = _scanDp[point.Row, point.Column].LearnStepRatio;
        return score;
    }

    public static string FormatResponse(Direction resultDirection)
    {
        return resultDirection.ToString().ToUpper();
    }


    public bool IsOutsideRange(Point point)
    {
        var row = point.Row;
        if (row < 0 || row >= Rows)
            return true;

        var column = point.Column;
        return column < 0 || column >= Columns;
    }

    private void Dfs(Point point, int steps)
    {
        if (IsOutsideRange(point))
            return;

        var scanResult = _scanDp[point.Row, point.Column];
        if (scanResult != null && steps > scanResult.Steps)
            return;

        var charAt = At(point);
        if (charAt == WALL)
        {
            _scanDp[point.Row, point.Column] = new ScanResult(steps, int.MinValue);
            return;
        }

        if (charAt == 'C')
        {
            _scanDp[point.Row, point.Column] = new ScanResult(steps, int.MaxValue);
            return;
        }

        if (charAt == '?')
        {
            _scanDp[point.Row, point.Column] = new ScanResult(steps, int.MaxValue);
            return;
        }

        _scanDp[point.Row, point.Column] = new ScanResult(steps, _radar.CalculateDiscoveredCells(point));

        steps++;

        GetScore(point, steps, Direction.LEFT);
        GetScore(point, steps, Direction.RIGHT);
        GetScore(point, steps, Direction.UP);
        GetScore(point, steps, Direction.DOWN);
    }

    private int At(Point current)
    {
        return At(current.Row, current.Column);
    }

    private void GetScore(Point current, int score, Direction direction)
    {
        Dfs(new Point(current, direction), score);
    }

    public char At(int row, int column)
    {
        if (row <= 0 || row >= _rows.Length)
            return WALL;

        if (column <= 0 || column >= _maxColumn)
            return WALL;

        return _rows[row][column];
    }

    private void SetArrivedAtControlRoom(Point currentPosition)
    {
        if (At(currentPosition.Row, currentPosition.Column) == 'C')
        {
            _simplePathFinder = new SimplePathFinder(this, currentPosition, 'T');
            _simplePathFinder.FillDynamicProgramming(_startingPoint.Value);
        }
    }

    public override string ToString()
    {
        return string.Join('\n', _rows);
    }
}

internal class ScanResult
{
    public int Steps { get; }
    public int LearnedSquares { get; }
    public int LearnStepRatio => Steps == 0 ? int.MinValue : LearnedSquares / Steps;

    public ScanResult(int steps, int learnedSquares)
    {
        Steps = steps;
        LearnedSquares = learnedSquares;
    }
}

public class SimplePathFinder
{
    private readonly int?[,] _dynamicProgramming;
    private readonly Point _startingPoint;
    private readonly Map _map;
    private Point _targetPoint;
    private readonly char _target;

    public SimplePathFinder(Map map, Point currentPosition, char target)
    {
        _map = map;
        _startingPoint = currentPosition;
        _dynamicProgramming = new int?[map.Rows, map.Columns];
        _target = target;

        _dynamicProgramming[_startingPoint.Row, _startingPoint.Column] = int.MaxValue;
    }

    public void FillDynamicProgramming(Point targetPoint)
    {
        _targetPoint = targetPoint;
        _dynamicProgramming[targetPoint.Row, targetPoint.Column] = 0;
        Dfs(targetPoint, 0);
    }

    private void Dfs(Point point, int steps)
    {
        if (_map.IsOutsideRange(point))
            return;

        var existingValue = _dynamicProgramming[point.Row, point.Column];
        if ((existingValue.HasValue && steps > existingValue) ||
            (steps > 0 && point.Equals(_targetPoint)) ||
            existingValue.HasValue && existingValue == int.MaxValue)
        {
            return;
        }

        var charAt = _map.At(point.Row, point.Column);
        if (charAt == _target && steps > 0)
        {
            SetDp(point, 0);
            return;
        }

        if (steps > 0 && (charAt == '?' || charAt == '#' || charAt == 'C'))
        {
            SetDp(point, int.MaxValue);
            return;
        }
        else
        {
            SetDp(point, steps);

            steps++;

            Dfs(new Point(point, Direction.UP), steps);
            Dfs(new Point(point, Direction.DOWN), steps);
            Dfs(new Point(point, Direction.RIGHT), steps);
            Dfs(new Point(point, Direction.LEFT), steps);
        }
    }

    private void SetDp(Point point, int steps)
    {
        _dynamicProgramming[point.Row, point.Column] = steps;
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
        if (_map.IsOutsideRange(point))
            return null;

        return _dynamicProgramming[point.Row, point.Column];
    }
}

public class Radar
{
    private Map _map;

    public Radar(Map map)
    {
        _map = map;
    }

    public int CalculateDiscoveredCells(Point current)
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
        return range.Count(x => _map.At(x.Row, x.Column) == '?');
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