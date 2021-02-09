using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace RYoshiga.TheLabyrinth.UnitTests
{
    public class SimplePathFinderShould
    {
        [Fact]
        public void FillDynamicProgramming()
        {
            var map = new Map(5, 5);

            map.SetRow("#####", 0);
            map.SetRow("#C..#", 1);
            map.SetRow("#.###", 2);
            map.SetRow("#..T#", 3);
            map.SetRow("#####", 4);

            var goingBackHome = new SimplePathFinder(map, new Point(1, 1), 'T');
            goingBackHome.FillDynamicProgramming(new Point(3, 3));

            goingBackHome.Do(new Point(1, 1)).ShouldBe("DOWN");
            goingBackHome.Do(new Point(2, 1)).ShouldBe("DOWN");
            goingBackHome.Do(new Point(3, 1)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(3, 2)).ShouldBe("RIGHT");
        }

        [Fact]
        public void SimpleRowFinding()
        {
            var map = new Map(3, 7);

            map.SetRow("#######", 0);
            map.SetRow("#T...C#", 1);
            map.SetRow("#######", 2);

            var goingBackHome = new SimplePathFinder(map, new Point(1, 1), 'C');
            goingBackHome.FillDynamicProgramming(new Point(1, 5));

            goingBackHome.Do(new Point(1, 1)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 2)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 3)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 4)).ShouldBe("RIGHT");
        }

        [Fact]
        public void SimpleRowFinding2()
        {
            var map = new Map(4, 7);

            map.SetRow("#######", 0);
            map.SetRow("#T...C#", 1);
            map.SetRow("#.....#", 1);
            map.SetRow("#######", 2);

            var goingBackHome = new SimplePathFinder(map, new Point(1, 1), 'C');
            goingBackHome.FillDynamicProgramming(new Point(1, 5));

            goingBackHome.Do(new Point(1, 1)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 2)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 3)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(1, 4)).ShouldBe("RIGHT");
        }

        [Fact]
        public void SimpleRowFindingReturning()
        {
            var map = new Map(3, 7);

            map.SetRow("#######", 0);
            map.SetRow("#T...C#", 1);
            map.SetRow("#######", 2);

            var goingBackHome = new SimplePathFinder(map, new Point(1, 5), 'T');
            goingBackHome.FillDynamicProgramming(new Point(1, 1));

            goingBackHome.Do(new Point(1, 5)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(1, 4)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(1, 3)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(1, 2)).ShouldBe("LEFT");
        }


        [Fact]
        public void NotBeBuggy()
        {
            var map = new Map(5, "???############???????????????".Length);

            map.SetRow("???############???????????????", 0);
            map.SetRow("???############???????????????", 1);
            map.SetRow("???##T......C##???????????????", 2);
            map.SetRow("???##......???#???????????????", 3);
            map.SetRow("???##......???#???????????????", 4);
            map.SetRow("???############???????????????", 4);

            var goingBackHome = new SimplePathFinder(map, new Point(2, 12), 'T');
            goingBackHome.FillDynamicProgramming(new Point(2, 5));

            goingBackHome.Do(new Point(2, 12)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(2, 11)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(2, 10)).ShouldBe("LEFT");
            goingBackHome.Do(new Point(2, 9)).ShouldBe("LEFT");
        }


        [Fact]
        public void HandleMassiveMap()
        {
            var strings = new List<string>()
            {
                "#####?????????????????????????",
                "#T........................????",
                "##........................????",
                "#.........................????",
                "#.........................????",
                "#.........................????",
                "?.........................????",
                "?.........................????",
                "?.........................????",
                "?.........................????",
                "?............................?",
                "?............................?",
                "?............................?",
                "?...........................C?",
                "?############################?"
            };
            var map = new Map(strings.Count, strings.First().Length);

            for (int i = 0; i < strings.Count; i++)
            {
                map.SetRow(strings[i], i);
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();


            var controlRoomRow = strings.FirstOrDefault(x => x.Contains("C"));
            Console.Error.WriteLine($"Trying to find path");

            var rowIndex = strings.IndexOf(controlRoomRow);
            var charIndex = controlRoomRow.IndexOf('C');
            var controlRoom = new Point(rowIndex, charIndex);
            

            
            var goingBackHome = new SimplePathFinder(map, new Point(12, 26), 'C');
            goingBackHome.FillDynamicProgramming(controlRoom);

            goingBackHome.Do(new Point(2, 12)).ShouldBe("RIGHT");

            sw.Stop();
            Debug.WriteLine($"Time taken {sw.ElapsedMilliseconds}");
        }




        [Fact]
        public void HandleMassiveMapTwo()
        {
            var strings = new List<string>()
            {
                "#####?????????????????????????",
                "#C........................????",
                "##........................????",
                "#.........................????",
                "#.........................????",
                "#.........................????",
                "?.........................????",
                "?.........................????",
                "?.........................????",
                "?.........................????",
                "?............................?",
                "?............................?",
                "?............................?",
                "?...........................T?",
                "?############################?"
            };
            var map = new Map(strings.Count, strings.First().Length);

            for (int i = 0; i < strings.Count; i++)
            {
                map.SetRow(strings[i], i);
            }


            Stopwatch sw = new Stopwatch();
            sw.Start();


            Console.Error.WriteLine($"Trying to find path");
            var controlRoom = new Point(13, 28);

            var goingBackHome = new SimplePathFinder(map, new Point(1, 1), 'T');
            goingBackHome.FillDynamicProgramming(controlRoom);

            goingBackHome.Do(new Point(1, 1)).ShouldBe("RIGHT");

            sw.Stop();
            Debug.WriteLine($"Time taken {sw.ElapsedMilliseconds}");
        }



        [Fact]
        public void ThrowWhenNotFeasibleToFindExit()
        {
            var strings = new List<string>()
            {
                "???????????????????###########",
                "???#####????##.##??###########",
                "?...#.######.#...??.#......T##",
                "?##.#.######.#.######.########",
                "##...........#.######.########",
                "###.#.######......###.##??????",
                "#...#.###....#.##.###.##??????",
                "#.####?#######.##.....##??????",
                "#.....?##......#########??????",
                "###.##?????????####.....??????",
                "###.##????????????????????????",
                "?##...????????????????????????",
                "?#####????????????????????????",
                "?###C.????????????????????????",
                "??????????????????????????????",
            };
            var map = new Map(strings.Count, strings.First().Length);

            for (int i = 0; i < strings.Count; i++)
            {
                map.SetRow(strings[i], i);
            }

            var controlRoomRow = strings.FirstOrDefault(x => x.Contains("C"));
            Console.Error.WriteLine($"Trying to find path");

            var rowIndex = strings.IndexOf(controlRoomRow);
            var charIndex = controlRoomRow.IndexOf('C');
            var controlRoom = new Point(rowIndex, charIndex);


            var goingBackHome = new SimplePathFinder(map, new Point(11, 3), 'C');
            goingBackHome.FillDynamicProgramming(controlRoom);


            Assert.Throws<NoExistingPath>(() => goingBackHome.Do(new Point(11, 3)));

        }



        //???#####????##################
        //???#####????##.###############
        //?...#.######.#......#......T##
        //?##.#.######.#.######.########
        //##...........#.######.########
        //###.#.######......###.##??????
        //#...#.###....#.##.###.##??????
        //#.####?#######.##.....##??????
        //#.....?##......#########??????
        //###.###############.........??
        //###.#####......####.########.#
        //?##.......####............####
        //?#############.######.###.##.#
        //?###C..........###....###....#
        //????????????#####??###########
    }

    
}
