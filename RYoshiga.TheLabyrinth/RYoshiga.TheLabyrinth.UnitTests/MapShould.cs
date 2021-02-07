using System;
using Shouldly;
using Xunit;

namespace RYoshiga.TheLabyrinth.UnitTests
{
    public class MapShould
    {
        [Fact]
        public void ScanProperly()
        {
            var map = new Map(10, 10);

            map.SetRow("#####?????", 0);
            map.SetRow("#....?????", 1);
            map.SetRow("#....?????", 2);
            map.SetRow("#....?????", 3);
            map.SetRow("#....?????", 4);
            map.SetRow("#....?????", 5);
            map.SetRow("??????????", 6);
            map.SetRow("??????????", 7);
            map.SetRow("??????????", 8);
            map.SetRow("??????????", 9);

            var decideWhereToGo = map.DecideWhereToGo(new Point(1, 1));
            
            decideWhereToGo.ShouldBe("RIGHT");
        }
    }
}
