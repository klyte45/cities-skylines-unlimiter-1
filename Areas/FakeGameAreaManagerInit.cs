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
            if (FakeGameAreaManager.areaGrid == null)
            {
                FakeGameAreaManager.areaGrid = new int[FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
                for (var i = 0; i < 5; ++i)
                {
                    for (var j = 0; j < 5; ++j)
                    {
                        FakeGameAreaManager.areaGrid[(i + 2) * FakeGameAreaManager.GRID + (j + 2)] = GameAreaManager.instance.m_areaGrid[i * 5 + j];
                    }
                }
                _areasUpdatedField.SetValue(GameAreaManager.instance, true);
            }
            instance.m_areaCount = 0;
            for (var i = 0; i < FakeGameAreaManager.GRID; ++i)
            {
                for (var j = 0; j < FakeGameAreaManager.GRID; ++j)
                {
                    if (FakeGameAreaManager.areaGrid[i * FakeGameAreaManager.GRID + j] > 0)
                    {
                        instance.m_areaCount++;
                    }
                }
            }

            TerrainManager.instance.m_detailPatchCount = 0;
            for (int index1 = 0; index1 < 9; ++index1)
            {
                for (int index2 = 0; index2 < 9; ++index2)
                    TerrainManager.instance.m_patches[index1 * 9 + index2].m_simDetailIndex = 0;
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