using System.Reflection;
using ColossalFramework;
using UnityEngine;

namespace EightyOne.Areas
{
    public class FakeGameAreaManagerInit
    {
        private static FieldInfo _startTileField = typeof(GameAreaManager).GetField("m_startTile", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _areasUpdatedField = typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _fieldInfo1 = typeof(GameAreaManager).GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _fieldInfo2 = typeof(GameAreaManager).GetField("m_buildableArea1", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _fieldInfo3 = typeof(GameAreaManager).GetField("m_buildableArea2", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _fieldInfo4 = typeof(GameAreaManager).GetField("m_buildableArea3", BindingFlags.Instance | BindingFlags.NonPublic);

        //losely based on GameAreaManager's method of the same name
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

            int x1;
            int z1;
            GetStartTile(out x1, out z1);
            float m_buildableArea0 = 0.0f;
            float m_buildableArea1 = 0.0f;
            float m_buildableArea2 = 0.0f;
            float m_buildableArea3 = 0.0f;
            float num1 = 0.0f;
            float num2 = 0.0f;
            float num3 = 0.0f;
            float num4 = 0.0f;
            for (int tileZ = 0; tileZ < FakeGameAreaManager.GRID; tileZ += 1)
            {
                for (int tileX = 0; tileX < FakeGameAreaManager.GRID; tileX += 1)
                {
                    switch (Mathf.Abs(tileX - x1) + Mathf.Abs(tileZ - z1))
                    {
                        case 0:
                            m_buildableArea0 += CalculateBuildableArea(tileX, tileZ);
                            ++num1;
                            break;

                        case 1:
                            m_buildableArea1 += CalculateBuildableArea(tileX, tileZ);
                            ++num2;
                            break;

                        case 2:
                            m_buildableArea2 += CalculateBuildableArea(tileX, tileZ);
                            ++num3;
                            break;

                        case 3:
                            m_buildableArea3 += CalculateBuildableArea(tileX, tileZ);
                            ++num4;
                            break;
                    }
                }
            }
            if ((double)num1 != 0.0)
                m_buildableArea0 /= num1;
            if ((double)num2 != 0.0)
                m_buildableArea1 /= num2;
            if ((double)num3 != 0.0)
                m_buildableArea2 /= num3;
            if ((double)num4 != 0.0)
                m_buildableArea3 /= num4;

            _fieldInfo1.SetValue(instance, m_buildableArea0);
            _fieldInfo2.SetValue(instance, m_buildableArea1);
            _fieldInfo3.SetValue(instance, m_buildableArea2);
            _fieldInfo4.SetValue(instance, m_buildableArea3);


            //TODO(earalov): this must be set somehow because without it unlocking mechanism breaks. But right now it causes terrain distortion
            //TerrainManager.instance.m_detailPatchCount  = 0;
            for (int z2 = 0; z2 < FakeGameAreaManager.GRID; ++z2)
            {
                for (int x2 = 0; x2 < FakeGameAreaManager.GRID; ++x2)
                {
                    if (x1 == x2 && z1 == z2)
                    {
                        continue;
                    }
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

        private static void GetStartTile(out int x, out int z)
        {
            Debug.Log("GetStartTile");
            int num = (int)_startTileField.GetValue(GameAreaManager.instance);
            //begin mod
            x = num % 5 + 2;
            z = num / 5 + 2;
            //end mod
        }

        //no changes
        private static float CalculateBuildableArea(int tileX, int tileZ)
        {
            uint ore;
            uint oil;
            uint forest;
            uint fertility;
            uint water;
            Singleton<NaturalResourceManager>.instance.GetTileResources(tileX, tileZ, out ore, out oil, out forest, out fertility, out water);
            float tileFlatness = Singleton<TerrainManager>.instance.GetTileFlatness(tileX, tileZ);
            float num = (float)(3686400.0 / (18225.0 / 16.0) * (double)byte.MaxValue);
            return tileFlatness * (float)(1.0 - (double)water / (double)num);
        }
    }
}