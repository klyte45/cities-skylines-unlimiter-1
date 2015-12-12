using System.Reflection;
using ColossalFramework;

namespace EightyOne.Areas
{
    public class FakeGameAreaManagerInit
    {
        private static FieldInfo _areasUpdatedField = typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance);

        //based on GameAreaManager's method of the same name
        public static void UpdateData()
        {
            var instance = GameAreaManager.instance;
            instance.m_maxAreaCount = FakeGameAreaManager.GRID * FakeGameAreaManager.GRID;
            if (FakeGameAreaManager.Data._loadedGrid == null)
            {
                var areaGrid = new int[FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
                for (var i = 0; i < GameAreaManager.AREAGRID_RESOLUTION; ++i)
                {
                    for (var j = 0; j < GameAreaManager.AREAGRID_RESOLUTION; ++j)
                    {
                        areaGrid[(i + 2) * FakeGameAreaManager.GRID + (j + 2)] =
                            GameAreaManager.instance.m_areaGrid[i * GameAreaManager.AREAGRID_RESOLUTION + j];
                    }
                }
                GameAreaManager.instance.m_areaGrid = areaGrid;
            }
            else
            {
                GameAreaManager.instance.m_areaGrid = FakeGameAreaManager.Data._loadedGrid;
            }
            FakeGameAreaManager.Data._loadedGrid = null;
            _areasUpdatedField.SetValue(GameAreaManager.instance, true);
            instance.m_areaCount = 0;
            for (var i = 0; i < FakeGameAreaManager.GRID; ++i)
            {
                for (var j = 0; j < FakeGameAreaManager.GRID; ++j)
                {
                    if (GameAreaManager.instance.m_areaGrid[i * FakeGameAreaManager.GRID + j] > 0)
                    {
                        instance.m_areaCount++;
                    }
                }
            }

            TerrainManager.instance.m_detailPatchCount = 0;
            for (int index1 = 0; index1 < TerrainManager.PATCH_RESOLUTION; ++index1)
            {
                for (int index2 = 0; index2 < TerrainManager.PATCH_RESOLUTION; ++index2)
                    TerrainManager.instance.m_patches[index1 * TerrainManager.PATCH_RESOLUTION + index2].m_simDetailIndex = 0;
            }
            for (int z2 = 0; z2 < FakeGameAreaManager.GRID; ++z2)
            {
                for (int x2 = 0; x2 < FakeGameAreaManager.GRID; ++x2)
                {
                    if (instance.GetArea(x2, z2) > 0)
                    {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2, z2);
                        float minX = (float)(((double)x2 - FakeGameAreaManager.HALFGRID) * 1920.0);
                        float maxX = (float)(((double)(x2 + 1) - FakeGameAreaManager.HALFGRID) * 1920.0);
                        float minZ = (float)(((double)z2 - FakeGameAreaManager.HALFGRID) * 1920.0);
                        float maxZ = (float)(((double)(z2 + 1) - FakeGameAreaManager.HALFGRID) * 1920.0);
                        Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                    }
                }
            }
        }
    }
}