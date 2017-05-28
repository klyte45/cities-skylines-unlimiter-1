using EightyOne.RedirectionFramework.Attributes;

namespace EightyOne.Areas
{
    [TargetType(typeof(AreasWrapper))]
    public class FakeAreasWrapper
    {
        [RedirectMethod]
        public void set_maxAreaCount(int value)
        {
            //begin mod
            GameAreaManager.instance.m_maxAreaCount = FakeGameAreaManager.GRID * FakeGameAreaManager.GRID;
            //end mod
        }

        [RedirectMethod]
        public void OnCanUnlockArea(int x, int z, ref bool result)
        {
            //begin mod
            //end mod
        }
    }
}