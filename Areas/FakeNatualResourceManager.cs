using EightyOne.Redirection;

namespace EightyOne.Areas
{
    [TargetType(typeof(NaturalResourceManager))]
    class FakeNatualResourceManager
    {
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
            this.GetTileResourcesImpl(ref NaturalResourceManager.instance.m_areaResources[z * FakeGameAreaManager.GRID + x], ref ore, ref oil, ref forest, ref fertility, ref water);
            //end mod
        }

        //no changes
        private void GetTileResourcesImpl(ref NaturalResourceManager.AreaCell cell, ref uint ore, ref uint oil, ref uint forest, ref uint fertility, ref uint water)
        {
            ore = ore + cell.m_finalOre;
            oil = oil + cell.m_finalOil;
            forest = forest + cell.m_finalForest;
            fertility = fertility + cell.m_finalFertility;
            water = water + cell.m_finalWater;
        }
    }
}
