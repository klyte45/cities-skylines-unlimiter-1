using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using System.Threading;
using ColossalFramework.Steamworks;
using ColossalFramework.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Areas
{
    [TargetType(typeof(GameAreaManager))]
    public class FakeGameAreaManager : GameAreaManager
    {

        public const int GRID = 9;
        public const float HALFGRID = 4.5f;

        public static int[] areaGrid;
        private static FieldInfo _AreasUpdatedField = typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _AreaTex = typeof(GameAreaManager).GetField("m_areaTex", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void UnlockAll()
        {
            instance.StartCoroutine(UnlockAllCoroutine());
        }

        private static IEnumerator UnlockAllCoroutine()
        {
            Debug.Log("Unlock all coroutine");

            var set = GetUnlockables();
            while (set.Length > 0)
            {
                var counter = set.Count();
                foreach (var keyValuePair in set)
                {
                    SimulationManager.instance.AddAction(() =>
                    {
                        if (instance.IsUnlocked(keyValuePair.Key, keyValuePair.Value))
                        {
                            return;
                        }
                        var areaIndex = FakeGameAreaManager.GetTileIndex(keyValuePair.Key, keyValuePair.Value); //for some reason this method can't be detoured
                        instance.UnlockArea(areaIndex);
                        Interlocked.Decrement(ref counter);
                    });
                    yield return null;
                }
                while (counter > 0)
                {
                    yield return new WaitForEndOfFrame();
                }
                set = GetUnlockables();
                yield return null;
            }


            //                        var i1 = i;
            //                        var j1 = j;
            //                        if (instance.CanUnlock(i1, j1))
            //                        {
            //                            SimulationManager.instance.AddAction(() =>
            //                            {
            //                                if (instance.IsUnlocked(i1, j1))
            //                                {
            //                                    return;
            //                                }
            //                                var areaIndex = FakeGameAreaManager.GetTileIndex(i1, j1);
            //                                //for some reason this method can't be detoured
            //                                instance.UnlockArea(areaIndex);
            //                            });
            //                        }
            //                        yield return null;
        }

        private static KeyValuePair<int, int>[] GetUnlockables()
        {
            var set = new HashSet<KeyValuePair<int, int>>();
            for (var i = 0; i < GRID; ++i)
            {
                for (var j = 0; j < GRID; ++j)
                {
                    if (!instance.IsUnlocked(i, j))
                    {
                        continue;
                    }
                    if (!instance.IsUnlocked(i + 1, j) && instance.CanUnlock(i + 1, j))
                    {
                        set.Add(new KeyValuePair<int, int>(i + 1, j));
                    }
                    if (!instance.IsUnlocked(i - 1, j) && instance.CanUnlock(i - 1, j))
                    {
                        set.Add(new KeyValuePair<int, int>(i - 1, j));
                    }
                    if (!instance.IsUnlocked(i, j + 1) && instance.CanUnlock(i, j + 1))
                    {
                        set.Add(new KeyValuePair<int, int>(i, j + 1));
                    }
                    if (!instance.IsUnlocked(i, j - 1) && instance.CanUnlock(i, j - 1))
                    {
                        set.Add(new KeyValuePair<int, int>(i, j - 1));
                    }
                }
            }
            return set.ToArray();
        }

        public static void Init()
        {
            var areaTex = new Texture2D(FakeGameAreaManagerUI.AREA_TEX_SIZE, FakeGameAreaManagerUI.AREA_TEX_SIZE, TextureFormat.ARGB32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _AreaTex.SetValue(GameAreaManager.instance, areaTex);


            GameAreaManager.instance.m_maxAreaCount = GRID * GRID + 1;
            if (areaGrid == null)
            {
                areaGrid = new int[GRID * GRID];
                for (var i = 0; i < 5; ++i)
                {
                    for (var j = 0; j < 5; ++j)
                    {
                        areaGrid[(i + 2) * GRID + (j + 2)] = GameAreaManager.instance.m_areaGrid[i * 5 + j];
                    }
                }
                _AreasUpdatedField.SetValue(GameAreaManager.instance, true);
            }

            GameAreaManager.instance.m_areaCount = 0;
            for (int z2 = 0; z2 < GRID; ++z2)
            {
                for (int x2 = 0; x2 < GRID; ++x2)
                {
                    if (GameAreaManager.instance.GetArea(x2, z2) > 0)
                    {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2, z2);

                        float minX = (float)(((double)x2 - HALFGRID) * 1920.0);
                        float maxX = (float)(((double)(x2 + 1) - HALFGRID) * 1920.0);
                        float minZ = (float)(((double)z2 - HALFGRID) * 1920.0);
                        float maxZ = (float)(((double)(z2 + 1) - HALFGRID) * 1920.0);
                        Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                        GameAreaManager.instance.m_areaCount += 1;
                    }
                }
            }

            SimulationManager.instance.AddAction(() => GameObject.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => GameObject.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
        }

        public static void OnDestroy()
        {
            areaGrid = null;
        }

        [ReplaceMethod]
        public int get_MaxAreaCount()
        {
            if (this.m_maxAreaCount == 0)
                //begin mod
                this.m_maxAreaCount = GRID * GRID;
            //end mod
            return this.m_maxAreaCount;
        }

        [ReplaceMethod]
        public new Vector3 GetAreaPositionSmooth(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || x >= GRID || z >= GRID)
            //end mod
            {
                return Vector3.zero;
            }
            Vector3 vector;
            //begin mod
            vector.x = ((float)x - HALFGRID + 0.5f) * 1920f;
            //end mod
            vector.y = 0f;
            //begin mod
            vector.z = ((float)z - HALFGRID + 0.5f) * 1920f;
            //end mod
            vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, true, 0f);
            return vector;
        }

        [ReplaceMethod]
        public new bool CanUnlock(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || (x >= GRID || z >= GRID) || (this.m_areaCount >= this.MaxAreaCount || !Singleton<UnlockManager>.instance.Unlocked(this.m_areaCount) || areaGrid[z * GRID + x] != 0))
                return false;
            //end mod
            bool result = this.IsUnlocked(x, z - 1) || this.IsUnlocked(x - 1, z) || this.IsUnlocked(x + 1, z) || this.IsUnlocked(x, z + 1);
            //begin mod
            //end mod
            return result;
        }

        [ReplaceMethod]
        public new int GetArea(int x, int z)
        {
            //begin mod
            if (x >= 0 && z >= 0 && (x < GRID && z < GRID))
                return areaGrid[z * GRID + x];
            int num = 0;
            return x < -num || z < -num || (x >= GRID + num || z >= GRID + num) ? -2 : -1;
            //end mod
        }

        [ReplaceMethod]
        public new bool IsUnlocked(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || (x >= GRID || z >= GRID))
                return false;
            return areaGrid[z * GRID + x] != 0;
            //end mod
        }

        [ReplaceMethod]
        public new int GetAreaIndex(Vector3 p)
        {
            //begin mod
            int num1 = Mathf.FloorToInt((float)((double)p.x / 1920.0 + HALFGRID));
            int num2 = Mathf.FloorToInt((float)((double)p.z / 1920.0 + HALFGRID));
            if (num1 < 0 || num2 < 0 || (num1 >= GRID || num2 >= GRID))
                return -1;
            return num2 * GRID + num1;
            //end mod
        }

        //For some reason this method can't be detoured
        public static void GetTileXZ(int tile, out int x, out int z)
        {
            //begin mod
            x = tile % GRID;
            z = tile / GRID;
            //end mod
        }

        //For some reason this method can't be detoured
        public static int GetTileIndex(int x, int z)
        {
            //begin mod
            return z * GRID + x;
            //end mod
        }

        [ReplaceMethod]
        public new bool UnlockArea(int index)
        {
            typeof(GameAreaManager).GetField("m_unlocking", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true);
            try
            {
                //begin mod
                int x = index % GRID;
                int z = index / GRID;
                //end mod
                if (this.CanUnlock(x, z))
                {
                    this.m_areaNotUnlocked.Deactivate();
                    CODebugBase<LogChannel>.Log(LogChannel.Core, "Unlocking new area");
                    areaGrid[index] = ++this.m_areaCount;
                    _AreasUpdatedField.SetValue(this, true);
                    //begin mod
                    if (this.m_areaCount == 9)
                        //end mod
                        ThreadHelper.dispatcher.Dispatch((System.Action)(() =>
                        {
                            if (Steam.achievements["SIMulatedCity"].achieved)
                                return;
                            Steam.achievements["SIMulatedCity"].Unlock();
                        }));
                    //begin mod
                    float minX = (float)(((double)x - HALFGRID) * 1920.0);
                    float maxX = (float)(((double)(x + 1) - HALFGRID) * 1920.0);
                    float minZ = (float)(((double)z - HALFGRID) * 1920.0);
                    float maxZ = (float)(((double)(z + 1) - HALFGRID) * 1920.0);
                    //end mod
                    Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);

                    if (Singleton<TerrainManager>.instance.SetDetailedPatch(x, z))
                    {
                        Singleton<MessageManager>.instance.TryCreateMessage(this.m_properties.m_unlockMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
                        //begin mod
                        //end mod
                        return true;

                    }
                    --this.m_areaCount;
                    areaGrid[index] = 0;

                    _AreasUpdatedField.SetValue(this, true);
                }
                return false;
            }
            finally
            {
                typeof(GameAreaManager).GetField("m_unlocking", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, false);
            }
        }

        private float CalculateBuildableArea(int tileX, int tileZ)
        {
            uint num;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            Singleton<NaturalResourceManager>.instance.GetTileResources(tileX - 2, tileZ - 2, out num, out num2, out num3, out num4, out num5);
            float tileFlatness = Singleton<TerrainManager>.instance.GetTileFlatness(tileX - 2, tileZ - 2);
            float num6 = 3686400f;
            float num7 = 1139.0625f;
            float num8 = num6 / num7 * 255f;
            return tileFlatness * (1f - num5 / num8);
        }

        [ReplaceMethod]
        public new void UpdateData(SimulationManager.UpdateMode mode)
        {
            var g = GameAreaManager.instance;
            if (mode == SimulationManager.UpdateMode.NewGame || mode == SimulationManager.UpdateMode.NewAsset || mode == SimulationManager.UpdateMode.LoadAsset)
            {
                g.m_areaCount = 0;
                int num = (int)g.GetType().GetField("m_startTile", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(g);
                for (int index1 = 0; index1 < GRID; ++index1)
                {
                    for (int index2 = 0; index2 < GRID; ++index2)
                    {
                        int index3 = index1 * GRID + index2;
                        areaGrid[index3] = index3 != num ? 0 : ++g.m_areaCount;
                    }
                }
            }
            int x1;
            int z1;
            g.GetStartTile(out x1, out z1);
            if (mode != SimulationManager.UpdateMode.LoadGame || (float)g.GetType().GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(g) < 0.0)
            {
                float m_buildableArea0 = 0.0f;
                float m_buildableArea1 = 0.0f;
                float m_buildableArea2 = 0.0f;
                float m_buildableArea3 = 0.0f;
                float num1 = 0.0f;
                float num2 = 0.0f;
                float num3 = 0.0f;
                float num4 = 0.0f;
                for (int tileZ = 0; tileZ < GRID; tileZ += 1)
                {
                    for (int tileX = 0; tileX < GRID; tileX += 1)
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

                g.GetType().GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(g, m_buildableArea0);
                g.GetType().GetField("m_buildableArea1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(g, m_buildableArea1);
                g.GetType().GetField("m_buildableArea2", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(g, m_buildableArea2);
                g.GetType().GetField("m_buildableArea3", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(g, m_buildableArea3);
            }

            for (int z2 = 0; z2 < GRID; ++z2)
            {
                for (int x2 = 0; x2 < GRID; ++x2)
                {
                    if (g.GetArea(x2, z2) > 0)
                    {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2, z2);

                        float minX = (float)(((double)x2 - HALFGRID) * 1920.0);
                        float maxX = (float)(((double)(x2 + 1) - HALFGRID) * 1920.0);
                        float minZ = (float)(((double)z2 - HALFGRID) * 1920.0);
                        float maxZ = (float)(((double)(z2 + 1) - HALFGRID) * 1920.0);
                        Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                    }
                }
            }
            if (mode == SimulationManager.UpdateMode.NewGame || g.m_areaNotUnlocked == null)
                g.m_areaNotUnlocked = new GenericGuide();
        }

        [ReplaceMethod]
        public new bool ClampPoint(ref Vector3 position)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
                return true;
            }
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
                return true;
            }
            //begin mod
            int x = Mathf.FloorToInt((float)((double)position.x / 1920.0 + HALFGRID));
            int z = Mathf.FloorToInt((float)((double)position.z / 1920.0 + HALFGRID));
            //end mod
            if (this.GetArea(x, z) > 0)
                return true;
            Rect rect1 = new Rect();
            //begin mod
            rect1.xMin = -1920 * HALFGRID;
            rect1.yMin = -1920 * HALFGRID;
            rect1.xMax = 1920 * HALFGRID;
            rect1.yMax = 1920 * HALFGRID;
            //end mod
            float num3 = 1000000f;
            for (int index1 = -1; index1 <= 1; ++index1)
            {
                for (int index2 = -1; index2 <= 1; ++index2)
                {
                    if (this.GetArea(x + index2, z + index1) > 0)
                    {
                        Rect rect2 = new Rect();
                        //begin mod
                        rect2.xMin = (float)(((double)(x + index2) - HALFGRID) * 1920.0);
                        rect2.yMin = (float)(((double)(z + index1) - HALFGRID) * 1920.0);
                        //end mod
                        rect2.xMax = rect2.xMin + 1920f;
                        rect2.yMax = rect2.yMin + 1920f;
                        float num1 = Mathf.Max(Mathf.Max(position.x - rect2.xMax, rect2.xMin - position.x), Mathf.Max(position.z - rect2.yMax, rect2.yMin - position.z));
                        if ((double)num1 < (double)num3)
                        {
                            rect1 = rect2;
                            num3 = num1;
                        }
                    }
                }
            }
            if ((double)position.x < (double)rect1.xMin)
                position.x = rect1.xMin;
            if ((double)position.x > (double)rect1.xMax)
                position.x = rect1.xMax;
            if ((double)position.z < (double)rect1.yMin)
                position.z = rect1.yMin;
            if ((double)position.z > (double)rect1.yMax)
                position.z = rect1.yMax;
            return (double)num3 != 1000000.0;

        }


        [ReplaceMethod]
        public new bool PointOutOfArea(Vector3 p)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
            }
            else
            {
                bool flag = (availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None;
                //begin mod
                int area = this.GetArea(Mathf.FloorToInt((float)((double)p.x / 1920.0 + HALFGRID)), Mathf.FloorToInt((float)((double)p.z / 1920.0 + HALFGRID)));
                //end mod
                if (area == -2 || !flag && area <= 0)
                    return true;
            }
            return false;
        }


        [ReplaceMethod]
        public new bool QuadOutOfArea(Quad2 quad)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
            }
            else
            {
                bool flag = (availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None;
                Vector2 vector2_1 = quad.Min();
                Vector2 vector2_2 = quad.Max();
                //begin mod
                int num1 = Mathf.FloorToInt((float)(((double)vector2_1.x - 8.0) / 1920.0 + HALFGRID));
                int num2 = Mathf.FloorToInt((float)(((double)vector2_1.y - 8.0) / 1920.0 + HALFGRID));
                int num3 = Mathf.FloorToInt((float)(((double)vector2_2.x + 8.0) / 1920.0 + HALFGRID));
                int num4 = Mathf.FloorToInt((float)(((double)vector2_2.y + 8.0) / 1920.0 + HALFGRID));
                //end mod
                for (int z = num2; z <= num4; ++z)
                {
                    for (int x = num1; x <= num3; ++x)
                    {
                        int area = this.GetArea(x, z);
                        if (area == -2 || !flag && area <= 0)
                        {
                            //begin mod
                            if (quad.Intersect(new Quad2()
                            {
                                a = new Vector2((float)(((double)x - 2.5) * 1920.0 - 8.0), (float)(((double)z - HALFGRID) * 1920.0 - 8.0)),
                                b = new Vector2((float)(((double)x - 2.5) * 1920.0 - 8.0), (float)(((double)z - HALFGRID + 1.0) * 1920.0 + 8.0)),
                                c = new Vector2((float)(((double)x - 2.5 + 1.0) * 1920.0 + 8.0), (float)(((double)z - HALFGRID + 1.0) * 1920.0 + 8.0)),
                                d = new Vector2((float)(((double)x - 2.5 + 1.0) * 1920.0 + 8.0), (float)(((double)z - HALFGRID) * 1920.0 - 8.0))
                            }))
                                //end mod
                                return true;
                        }
                    }
                }
            }
            return false;
        }


    }
}
