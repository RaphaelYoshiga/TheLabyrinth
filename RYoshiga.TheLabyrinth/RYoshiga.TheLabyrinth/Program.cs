using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Point
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }
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



        var map = new Map(R);
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int KR = int.Parse(inputs[0]); // row where Kirk is located.
            int KC = int.Parse(inputs[1]); // column where Kirk is located.

            for (int i = 0; i < R; i++)
            {
                string row = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                map.SetRow(row, i);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(map.DecideWhereToGo(new Point(KR, KC))); // Kirk's next move (UP DOWN LEFT or RIGHT).

        }
    }

    class Map
    {
        private readonly string[] _rows;
        private bool controlRoomFound = false;
        public Map(int rows)
        {
            _rows = new string[rows];
        }

        public void SetRow(string row, int i)
        {
            _rows[i] = row;
        }

        public string DecideWhereToGo(Point currentPosition)
        {
            if (_rows[currentPosition.X][currentPosition.Y] == 'C')
            {
                controlRoomFound = true;
            }

            if (controlRoomFound)
                return "LEFT";

            return "RIGHT";
        }
    }
}