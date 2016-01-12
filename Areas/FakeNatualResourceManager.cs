using ColossalFramework;
using EightyOne.Redirection;

namespace EightyOne.Areas
{
    [TargetType(typeof(NaturalResourceManager))]
    class FakeNatualResourceManager
    {
        [RedirectReverse]
        private static void GetTileResourcesImpl(NaturalResourceManager manager, ref NaturalResourceManager.AreaCell cell, ref uint ore, ref uint oil, ref uint forest, ref uint fertility, ref uint water)
        {
            UnityEngine.Debug.Log($"{manager}+{cell}+{ore}+{oil}+{forest}+{fertility}+{water}");
        }

        [RedirectMethod]
        public void GetTileResources(int x, int z, out uint ore, out uint oil, out uint forest, out uint fertility, out uint water)
        {
            //begin mod
            int num = 0;
            //end mod
            ore = 0U;
            oil = 0U;
            forest = 0U;
            fertility = 0U;
            water = 0U;
            this.GetTileResourcesImpl(x + num, z + num, ref ore, ref oil, ref forest, ref fertility, ref water);
        }

        [RedirectMethod]
        private void GetTileResourcesImpl(int x, int z, ref uint ore, ref uint oil, ref uint forest, ref uint fertility, ref uint water)
        {
            //begin mod
            GetTileResourcesImpl(NaturalResourceManager.instance, ref NaturalResourceManager.instance.m_areaResources[z * FakeGameAreaManager.GRID + x], ref ore, ref oil, ref forest, ref fertility, ref water);
            //end mod
        }

        [RedirectMethod]
        public void CalculateUnlockedResources(out uint ore, out uint oil, out uint forest, out uint fertility, out uint water)
        {
            ore = 0U;
            oil = 0U;
            forest = 0U;
            fertility = 0U;
            water = 0U;
            GameAreaManager instance = Singleton<GameAreaManager>.instance;
            //begin mod
            int num = 0;
            for (int z = 0; z < FakeGameAreaManager.GRID; ++z)
            {
                for (int x = 0; x < FakeGameAreaManager.GRID; ++x)
                {
                    //end mod
                    if (instance.IsUnlocked(x, z))
                        this.GetTileResourcesImpl(x + num, z + num, ref ore, ref oil, ref forest, ref fertility, ref water);
                }
            }
        }

        [RedirectMethod]
        public void CalculateUnlockableResources(out uint ore, out uint oil, out uint forest, out uint fertility, out uint water)
        {
            ore = 0U;
            oil = 0U;
            forest = 0U;
            fertility = 0U;
            water = 0U;
            //begin mod
            int num = 0;
            for (int index1 = 0; index1 < FakeGameAreaManager.GRID; ++index1)
            {
                for (int index2 = 0; index2 < FakeGameAreaManager.GRID; ++index2)
                    //end mod
                    this.GetTileResourcesImpl(index2 + num, index1 + num, ref ore, ref oil, ref forest, ref fertility, ref water);
            }
        }
    }
}
