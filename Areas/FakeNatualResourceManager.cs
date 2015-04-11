using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EightyOne.Attributes;

namespace EightyOne.Areas
{
    class FakeNatualResourceManager
    {
        [ReplaceMethod]
        public void GetTileResources(int x, int z, out uint ore, out uint oil, out uint forest, out uint fertility, out uint water)
        {
            ore = 0u;
            oil = 0u;
            forest = 0u;
            fertility = 0u;
            water = 0u;
            GetTileResourcesImpl(x, z, ref ore, ref oil, ref forest, ref fertility, ref water);
        }

        private void GetTileResourcesImpl(int x, int z, ref uint ore, ref uint oil, ref uint forest, ref uint fertility, ref uint water)
        {
            int num = z * 9 + x;
            GetTileResourcesImpl(ref NaturalResourceManager.instance.m_areaResources[num], ref ore, ref oil, ref forest, ref fertility, ref water);
        }
        private void GetTileResourcesImpl(ref NaturalResourceManager.AreaCell cell, ref uint ore, ref uint oil, ref uint forest, ref uint fertility, ref uint water)
        {
            ore += cell.m_finalOre;
            oil += cell.m_finalOil;
            forest += cell.m_finalForest;
            fertility += cell.m_finalFertility;
            water += cell.m_finalWater;
        }
    }
}
