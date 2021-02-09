using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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
    private ScanResult[,] _scanDp;
    private readonly Radar _radar;
    private SimplePathFinder _beforeArrivingFinder;
    private int _lowesPossibleScore;
    private ScanResult _bestPlaceToGo;
    private HashSet<Point> _queried;
    private Point _controlRoom;

    public Map(int rows, int column)
    {
        _maxColumn = column;
        _rows = new string[rows];


        _positions = new Stack<Point>();
        _radar = new Radar(this);


        _lowesPossibleScore = -9999;
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

        if (_simplePathFinder != null)
        {
            Console.Error.WriteLine($"Simple path finder return");
            return _simplePathFinder.Do(current);
        }

        try
        {
            if (_beforeArrivingFinder != null)
            {
                return FindPath(current, _controlRoom);
            }

            var controlRoomRow = _rows.FirstOrDefault(x => x.Contains("C"));
            if (controlRoomRow != null)
            {
                Console.Error.WriteLine($"Trying to find path");

                var rowIndex = Array.IndexOf(_rows, controlRoomRow);
                var charIndex = controlRoomRow.IndexOf('C');
                _controlRoom = new Point(rowIndex, charIndex);
                return FindPath(current, _controlRoom);
            }
        }
        catch (NoExistingPath e)
        {
            Console.Error.WriteLine("No path yet");
        }

        _scanDp = new ScanResult[Rows, Columns];
        _bestPlaceToGo = null;

        Bfs(current);

        _positions.Push(current);
        _visited.Add(current);

        return ChooseBasedOnTheDp(current);
    }

    private string FindPath(Point current, Point controlRoom)
    {
        _beforeArrivingFinder = new SimplePathFinder(this, current, 'C');
        _beforeArrivingFinder.FillDynamicProgramming(controlRoom);

        return _beforeArrivingFinder.Do(current);
    }

    public string ChooseBasedOnTheDp(Point current)
    {
        Console.Error.WriteLine($"This was the best place to go: {_bestPlaceToGo.Point.Row}, {_bestPlaceToGo.Point.Column}");
        var stepPoint = new StepPoint(_bestPlaceToGo.Steps, _bestPlaceToGo.Point);
        while (stepPoint.Steps > 1)
        {
            var results = new[]
            {
                DynamicAt(new Point(stepPoint.Point, Direction.DOWN)),
                DynamicAt(new Point(stepPoint.Point, Direction.UP)),
                DynamicAt(new Point(stepPoint.Point, Direction.LEFT)),
                DynamicAt(new Point(stepPoint.Point, Direction.RIGHT)),
            };

            stepPoint = results.OrderBy(p => p.Steps).ThenByDescending(x => x.Point.Row).First();
        }

        Console.Error.WriteLine($"Trying to go to: {stepPoint.Point.Row}, {stepPoint.Point.Column}");
        return FormatResponse(current.FindDirection(stepPoint.Point));
    }

    private StepPoint DynamicAt(Point point)
    {
        if (IsOutsideRange(point))
        {
            return new StepPoint(int.MaxValue, point);
        }

        var scanResult = _scanDp[point.Row, point.Column];
        if (scanResult == null)
            return new StepPoint(int.MaxValue, point);

        return new StepPoint(scanResult.Steps, point);
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

    private void Bfs(Point startingPoint)
    {
        var searchQueue = new Queue<PointStep>();
        _queried = new HashSet<Point>();

        searchQueue.Enqueue(new PointStep(startingPoint, 0));

        while (searchQueue.TryDequeue(out var pointStep))
        {
            var point = pointStep.Point;
            var steps = pointStep.Step;

            if (IsOutsideRange(point))
                continue;

            if (steps >= 20)
                break;

            var charAt = At(point);
            if (charAt == WALL)
            {
                _scanDp[point.Row, point.Column] = new ScanResult(int.MaxValue, -1000);
                continue;
            }

            if (steps > 0 && (charAt == '?'))
            {
                _scanDp[point.Row, point.Column] = new ScanResult(steps, _lowesPossibleScore);
                continue;
            }

            var discoveredCells = _radar.CalculateDiscoveredCells(point);
            var scanResult = new ScanResult(steps, discoveredCells, point);
            if (_bestPlaceToGo == null || scanResult.LearnStepScore > _bestPlaceToGo.LearnStepScore)
            {
                _bestPlaceToGo = scanResult;
            }

            _scanDp[point.Row, point.Column] = scanResult;

            steps++;

            SafeEnqueue(searchQueue, steps, new Point(point, Direction.UP));
            SafeEnqueue(searchQueue, steps, new Point(point, Direction.DOWN));
            SafeEnqueue(searchQueue, steps, new Point(point, Direction.LEFT));
            SafeEnqueue(searchQueue, steps, new Point(point, Direction.RIGHT));
        }
    }

    private void SafeEnqueue(Queue<PointStep> searchQueue, int steps, Point newPoint)
    {
        if (_queried.Contains(newPoint))
        {
            return;
        }

        _queried.Add(newPoint);

        searchQueue.Enqueue(new PointStep(newPoint, steps));
    }

    private int At(Point current)
    {
        return At(current.Row, current.Column);
    }

    public char At(int row, int column)
    {
        if (row < 0 || row >= _rows.Length)
            return WALL;

        if (column < 0 || column >= _maxColumn)
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

public class StepPoint
{
    public int Steps { get; }
    public Point Point { get; }

    public StepPoint(int steps, Point point)
    {
        Steps = steps;
        Point = point;
    }
}

internal class ScanResult
{
    public int Steps { get; }
    public int LearnedSquares { get; }
    public Point Point { get; }
    public decimal LearnStepScore => Steps == 0 ? decimal.MinValue : LearnedSquares / (Steps * 0.26m);

    public ScanResult(int steps, int learnedSquares)
    {
        Steps = steps;
        LearnedSquares = learnedSquares;
    }

    public ScanResult(in int steps, in int discoveredCells, Point point)
    {
        Steps = steps;
        LearnedSquares = discoveredCells;
        Point = point;
    }
}

public class SimplePathFinder
{
    private readonly int?[,] _dynamicProgramming;
    private readonly Point _startingPoint;
    private readonly Map _map;
    private Point _targetPoint;
    private readonly char _target;
    private HashSet<Point> _queried;

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
        Bfs(targetPoint, 0);
    }

    private void Bfs(Point startingPoint, int steps)
    {
        var searchQueue = new Queue<PointStep>();
        searchQueue.Enqueue(new PointStep(startingPoint, steps));
        _queried = new HashSet<Point>();


        while (searchQueue.TryDequeue(out var pointStep))
        {
            var point = pointStep.Point;
            if (_map.IsOutsideRange(point))
                continue;

            var existingValue = _dynamicProgramming[point.Row, point.Column];
            if ((steps > 0 && existingValue.HasValue && steps > existingValue) ||
                (steps > 0 && point.Equals(_targetPoint)) ||
                existingValue.HasValue && existingValue == int.MaxValue)
            {
                continue;
            }

            var charAt = _map.At(point.Row, point.Column);
            if (charAt == _target && steps > 0)
            {
                SetDp(point, 0);
                continue;
            }

            if (steps > 0 && (charAt == '?' || charAt == '#' || charAt == 'C'))
            {
                SetDp(point, int.MaxValue);
                continue;
            }

            steps++;

            SetDp(point, steps);

            SafeEnqueue(steps, searchQueue, new Point(point, Direction.UP));
            SafeEnqueue(steps, searchQueue, new Point(point, Direction.DOWN));
            SafeEnqueue(steps, searchQueue, new Point(point, Direction.LEFT));
            SafeEnqueue(steps, searchQueue, new Point(point, Direction.RIGHT));
        }
    }

    private void SafeEnqueue(int steps, Queue<PointStep> searchQueue, Point newPoint)
    {
        if (_queried.Contains(newPoint))
            return;
        
        _queried.Add(newPoint);
        searchQueue.Enqueue(new PointStep(newPoint, steps));
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

        var lowestSteps = results.OrderBy(x => x.Score).First();

        if (lowestSteps.Score > 1000000)
            throw new NoExistingPath();

        var direction = lowestSteps.Direction;
        return Map.FormatResponse(direction);
    }

    private int? DynamicAt(Point point)
    {
        return _map.IsOutsideRange(point) ? null : _dynamicProgramming[point.Row, point.Column];
    }
}

public class NoExistingPath : Exception
{
}

internal class PointStep
{
    public PointStep(Point point, int step)
    {
        Point = point;
        Step = step;
    }

    public Point Point { get; set; }
    public int Step { get; set; }
}

public class Radar
{
    private const char QuestionMark = '?';
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
        return range.Count(x => _map.At(x.Row, x.Column) == QuestionMark);
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
        if (Row > previous.Row)
            return Direction.UP;

        if (Row < previous.Row)
            return Direction.DOWN;

        if (Column > previous.Column)
            return Direction.LEFT;

        return Direction.RIGHT;
    }
}