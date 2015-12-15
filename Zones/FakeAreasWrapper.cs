using EightyOne.Areas;
using EightyOne.Redirection;

namespace EightyOne.Zones
{
    [TargetType(typeof(AreasWrapper))]
    public class FakeAreasWrapper
    {
        [RedirectMethod]
        public void set_maxAreaCount(int value)
        {
            //begin mod
            int num = FakeGameAreaManager.GRID * FakeGameAreaManager.GRID;
            if (GameAreaManager.instance.m_maxAreaCount == num)
                return;
            //end mod
            GameAreaManager.instance.m_maxAreaCount = num;
        }

        [RedirectMethod]
        public void OnCanUnlockArea(int x, int z, ref bool result)
        {
            //begin mod
            //end mod
        }
    }
}