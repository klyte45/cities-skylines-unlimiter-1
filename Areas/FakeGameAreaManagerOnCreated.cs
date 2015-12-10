using System.Reflection;
using ColossalFramework;
using EightyOne.Attributes;
using UnityEngine;

namespace EightyOne.Areas
{
    //TODO(earalov): this is a dead code. The detoured method never gets called
    [TargetType(typeof(GameAreaManager))]
    public class FakeGameAreaManagerOnCreated : GameAreaManager
    {

        //TODO(earalov): review method
        [ReplaceMethod]
        public new void UpdateData(SimulationManager.UpdateMode mode)
        {
            UnityEngine.Debug.Log("UpdateData was called");
            if (mode == SimulationManager.UpdateMode.NewGame || mode == SimulationManager.UpdateMode.NewAsset || mode == SimulationManager.UpdateMode.LoadAsset)
            {
                this.m_areaCount = 0;
                int num = (int)this.GetType().GetField("m_startTile", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
                for (int index1 = 0; index1 < FakeGameAreaManager.GRID; ++index1)
                {
                    for (int index2 = 0; index2 < FakeGameAreaManager.GRID; ++index2)
                    {
                        int index3 = index1 * FakeGameAreaManager.GRID + index2;
                        FakeGameAreaManager.areaGrid[index3] = index3 != num ? 0 : ++this.m_areaCount;
                    }
                }
            }
            int x1;
            int z1;
            this.GetStartTile(out x1, out z1);
            if (mode != SimulationManager.UpdateMode.LoadGame || (float)this.GetType().GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) < 0.0)
            {
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

                this.GetType().GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, m_buildableArea0);
                this.GetType().GetField("m_buildableArea1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, m_buildableArea1);
                this.GetType().GetField("m_buildableArea2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, m_buildableArea2);
                this.GetType().GetField("m_buildableArea3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, m_buildableArea3);
            }

            for (int z2 = 0; z2 < FakeGameAreaManager.GRID; ++z2)
            {
                for (int x2 = 0; x2 < FakeGameAreaManager.GRID; ++x2)
                {
                    if (this.GetArea(x2, z2) > 0)
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
            if (mode == SimulationManager.UpdateMode.NewGame || this.m_areaNotUnlocked == null)
                this.m_areaNotUnlocked = new GenericGuide();
        }

        //no changes
        private float CalculateBuildableArea(int tileX, int tileZ)
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