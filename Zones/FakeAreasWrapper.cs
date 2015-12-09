using EightyOne.Areas;
using EightyOne.Attributes;

namespace EightyOne.Zones
{
    [TargetType(typeof(AreasWrapper))]
    public class FakeAreasWrapper
    {
        [ReplaceMethod]
        public void set_maxAreaCount(int value)
        {
            //begin mod
            int num = FakeGameAreaManager.GRID * FakeGameAreaManager.GRID;
            if (GameAreaManager.instance.m_maxAreaCount == num)
                return;
            //end mod
            GameAreaManager.instance.m_maxAreaCount = num;
        }

        [ReplaceMethod]
        public void OnCanUnlockArea(int x, int z, ref bool result)
        {
            //begin mod
            //end mod
        }
    }
}