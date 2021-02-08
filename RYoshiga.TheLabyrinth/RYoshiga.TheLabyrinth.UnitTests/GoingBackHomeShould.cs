using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace RYoshiga.TheLabyrinth.UnitTests
{
    public class GoingBackHomeShould
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

            var goingBackHome = new GoingBackHome(map, new Point(1, 1));
            goingBackHome.FillDynamicProgramming(new Point(3, 3));

            goingBackHome.Do(new Point(1, 1)).ShouldBe("DOWN");
            goingBackHome.Do(new Point(2, 1)).ShouldBe("DOWN");
            goingBackHome.Do(new Point(3, 1)).ShouldBe("RIGHT");
            goingBackHome.Do(new Point(3, 2)).ShouldBe("RIGHT");
        }
    }
}
