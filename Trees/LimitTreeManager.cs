using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Trees
{
    internal static class LimitTreeManager
    {
        internal static class Helper
        {
            internal static int TreeLimit
            {
                get
                {
                    if (!UseModifiedTreeCap)
                        return Mod.DEFAULT_TREE_COUNT;

                    return Mod.MOD_TREE_SCALE * Mod.DEFAULT_TREE_COUNT;
                }
            }

            internal static bool UseModifiedTreeCap
            {
                get
                {
                    if (!Mod.IsEnabled)
                        return false;

                    var mode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
                    return mode == SimulationManager.UpdateMode.LoadGame || mode == SimulationManager.UpdateMode.NewGame;
                }
            }

            internal static void EnsureInit()
            {
                Debug.LogFormat("[TreeLimit] This mod is {0}. Tree limit is {0}.", Mod.IsEnabled ? "enabled" : "disabled", UseModifiedTreeCap ? "enabled" : "disabled");
                if (!UseModifiedTreeCap)
                    return;

                if (TreeManager.instance.m_trees.m_buffer.Length != TreeLimit)
                {
                    Debug.LogFormat("[TreeLimit] Scaling up TreeManager");

                    TreeManager.instance.m_trees = new Array32<TreeInstance>((uint)TreeLimit);
                    TreeManager.instance.m_updatedTrees = new ulong[4096 * Mod.MOD_TREE_SCALE];

                    uint num;
                    TreeManager.instance.m_trees.CreateItem(out num);
                }
            }
        }

        [ReplaceMethod]
        public static void EndRenderingImpl(TreeManager tm, RenderManager.CameraInfo cameraInfo)
        {
            if (Input.GetKeyDown(KeyCode.F5) && Event.current.control)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("TreeLimit: TreeCount={0}, TreeLimit={1}, CanPlaceMoreTrees={2}", tm.m_treeCount, Helper.TreeLimit, tm.CheckLimits()));

                var ta = tm.m_trees;
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("TreeLimit: ArraySize={0}, ItemCount={1}, UnusedCount={2}", ta.m_size, ta.ItemCount(), ta.GetType().GetField("m_unusedCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(ta)));
            }

            FastList<RenderGroup> fastList = Singleton<RenderManager>.instance.m_renderedGroups;
            for (int index1 = 0; index1 < fastList.m_size; ++index1)
            {
                RenderGroup renderGroup = fastList.m_buffer[index1];
                if ((renderGroup.m_instanceMask & 1 << tm.m_treeLayer) != 0)
                {
                    int num1 = renderGroup.m_x * 540 / 45;
                    int num2 = renderGroup.m_z * 540 / 45;
                    int num3 = (renderGroup.m_x + 1) * 540 / 45 - 1;
                    int num4 = (renderGroup.m_z + 1) * 540 / 45 - 1;
                    for (int index2 = num2; index2 <= num4; ++index2)
                    {
                        for (int index3 = num1; index3 <= num3; ++index3)
                        {
                            uint treeID = tm.m_treeGrid[index2 * 540 + index3];
                            int num5 = 0;
                            while ((int)treeID != 0)
                            {
                                tm.m_trees.m_buffer[treeID].RenderInstance(cameraInfo, treeID, renderGroup.m_instanceMask);
                                treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                                if (++num5 >= Helper.TreeLimit)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            int num = PrefabCollection<TreeInfo>.PrefabCount();
            for (int index = 0; index < num; ++index)
            {
                TreeInfo prefab = PrefabCollection<TreeInfo>.GetPrefab((uint)index);
                if (prefab != null && prefab.m_lodCount != 0)
                    TreeInstance.RenderLod(cameraInfo, prefab);
            }
        }

        [ReplaceMethod]
        public static float SampleSmoothHeight(TreeManager tm, Vector3 worldPos)
        {
            float num1 = 0.0f;
            int num2 = Mathf.Max((int)(((double)worldPos.x - 32.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Max((int)(((double)worldPos.z - 32.0) / 32.0 + 270.0), 0);
            int num4 = Mathf.Min((int)(((double)worldPos.x + 32.0) / 32.0 + 270.0), 539);
            int num5 = Mathf.Min((int)(((double)worldPos.z + 32.0) / 32.0 + 270.0), 539);
            for (int index1 = num3; index1 <= num5; ++index1)
            {
                for (int index2 = num2; index2 <= num4; ++index2)
                {
                    uint num6 = tm.m_treeGrid[index1 * 540 + index2];
                    int num7 = 0;
                    while ((int)num6 != 0)
                    {
                        if (tm.m_trees.m_buffer[num6].GrowState != 0)
                        {
                            Vector3 position = tm.m_trees.m_buffer[num6].Position;
                            Vector3 vector3 = worldPos - position;
                            float num8 = 1024f;
                            float num9 = (float)((double)vector3.x * (double)vector3.x + (double)vector3.z * (double)vector3.z);
                            if ((double)num9 < (double)num8)
                            {
                                TreeInfo info = tm.m_trees.m_buffer[num6].Info;
                                float num10 = MathUtils.SmoothClamp01(1f - Mathf.Sqrt(num9 / num8));
                                float num11 = position.y + info.m_generatedInfo.m_size.y * 1.25f * num10;
                                if ((double)num11 > (double)num1)
                                    num1 = num11;
                            }
                        }
                        num6 = tm.m_trees.m_buffer[num6].m_nextGridTree;
                        if (++num7 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return num1;
        }

        [ReplaceMethod]
        public static bool CheckLimits(TreeManager tm)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
                if (tm.m_treeCount >= 250000)
                    return false;
            }
            else if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                if (tm.m_treeCount + Singleton<PropManager>.instance.m_propCount >= 64)
                    return false;
            }
            else if (tm.m_treeCount >= Helper.TreeLimit - 5)//262139
                return false;
            return true;
        }

        private static void ReleaseTreeImplementation(TreeManager tm, uint tree, ref TreeInstance data)
        {
            if ((int)data.m_flags == 0)
                return;
            Singleton<InstanceManager>.instance.ReleaseInstance(new InstanceID()
            {
                Tree = tree
            });
            data.m_flags |= (ushort)2;
            data.UpdateTree(tree);
            data.m_flags = (ushort)0;
            int num1;
            int num2;
            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor)
            {
                num1 = Mathf.Clamp(((int)data.m_posX / 16 + 32768) * 540 / 65536, 0, 539);
                num2 = Mathf.Clamp(((int)data.m_posZ / 16 + 32768) * 540 / 65536, 0, 539);
            }
            else
            {
                num1 = Mathf.Clamp(((int)data.m_posX + 32768) * 540 / 65536, 0, 539);
                num2 = Mathf.Clamp(((int)data.m_posZ + 32768) * 540 / 65536, 0, 539);
            }
            int index = num2 * 540 + num1;
            do
            { }
            while (!Monitor.TryEnter(tm.m_treeGrid, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                uint num3 = 0U;
                uint num4 = tm.m_treeGrid[index];
                int num5 = 0;
                while ((int)num4 != 0)
                {
                    if ((int)num4 == (int)tree)
                    {
                        if ((int)num3 == 0)
                        {
                            tm.m_treeGrid[index] = data.m_nextGridTree;
                            break;
                        }
                        tm.m_trees.m_buffer[num3].m_nextGridTree = data.m_nextGridTree;
                        break;
                    }
                    num3 = num4;
                    num4 = tm.m_trees.m_buffer[num4].m_nextGridTree;
                    if (++num5 > Helper.TreeLimit)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
                data.m_nextGridTree = 0U;
            }
            finally
            {
                Monitor.Exit((object)tm.m_treeGrid);
            }

            try
            {
                tm.m_trees.ReleaseItem(tree);
            }
            catch (Exception e)
            {
                Debug.LogFormat("[TreeLimit] Releasing {0} {1} {2} {3}", tree, tm.m_trees.m_size, Helper.TreeLimit, Helper.UseModifiedTreeCap);
                Debug.LogException(e);
            }

            // UpdateTree
            UpdateTree(tm, tree);

            tm.m_treeCount = (int)tm.m_trees.ItemCount() - 1;
            Singleton<RenderManager>.instance.UpdateGroup(num1 * 45 / 540, num2 * 45 / 540, tm.m_treeLayer);
        }

        [ReplaceMethod]
        public static void UpdateTree(TreeManager tm, uint tree)
        {
            // This requires us to use a bigger tree grid.
            tm.m_updatedTrees[(tree >> 6)] |= (ulong)(1L << (int)tree);
            tm.m_treesUpdated = true;
        }

        [ReplaceMethod]
        public static void UpdateTrees(TreeManager tm, float minX, float minZ, float maxX, float maxZ)
        {
            int num1 = Mathf.Max((int)(((double)minX - 8.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 8.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 8.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)maxZ + 8.0) / 32.0 + 270.0), 539);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint num5 = tm.m_treeGrid[index1 * 540 + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 position = tm.m_trees.m_buffer[num5].Position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max((float)((double)position.x - (double)maxX - 8.0), (float)((double)position.z - (double)maxZ - 8.0))) < 0.0)
                        {
                            tm.m_updatedTrees[(num5 >> 6)] |= (ulong)(1L << (int)num5);
                            tm.m_treesUpdated = true;
                        }
                        num5 = tm.m_trees.m_buffer[num5].m_nextGridTree;
                        if (++num6 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public static bool OverlapQuad(TreeManager tm, Quad2 quad, float minY, float maxY, int layer, uint ignoreTree)
        {
            Vector2 vector2_1 = quad.Min();
            Vector2 vector2_2 = quad.Max();
            int num1 = Mathf.Max((int)(((double)vector2_1.x - 8.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)vector2_1.y - 8.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)vector2_2.x + 8.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)vector2_2.y + 8.0) / 32.0 + 270.0), 539);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)treeID != 0)
                    {
                        Vector3 position = tm.m_trees.m_buffer[treeID].Position;
                        if ((double)Mathf.Max(Mathf.Max(vector2_1.x - 8f - position.x, vector2_1.y - 8f - position.z), Mathf.Max((float)((double)position.x - (double)vector2_2.x - 8.0), (float)((double)position.z - (double)vector2_2.y - 8.0))) < 0.0 && tm.m_trees.m_buffer[treeID].OverlapQuad(treeID, quad, minY, maxY))
                            return true;
                        treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return false;
        }

        [ReplaceMethod]
        public static bool RayCast(TreeManager tm, Segment3 ray, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Layer itemLayers, TreeInstance.Flags ignoreFlags, out Vector3 hit, out uint treeIndex)
        {
            Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds))
            {
                Vector3 vector3_1 = ray.b - ray.a;
                int num1 = (int)((double)ray.a.x / 32.0 + 270.0);
                int num2 = (int)((double)ray.a.z / 32.0 + 270.0);
                int num3 = (int)((double)ray.b.x / 32.0 + 270.0);
                int num4 = (int)((double)ray.b.z / 32.0 + 270.0);
                float num5 = Mathf.Abs(vector3_1.x);
                float num6 = Mathf.Abs(vector3_1.z);
                int num7;
                int num8;
                if ((double)num5 >= (double)num6)
                {
                    num7 = (double)vector3_1.x <= 0.0 ? -1 : 1;
                    num8 = 0;
                    if ((double)num5 != 0.0)
                        vector3_1 *= 32f / num5;
                }
                else
                {
                    num7 = 0;
                    num8 = (double)vector3_1.z <= 0.0 ? -1 : 1;
                    if ((double)num6 != 0.0)
                        vector3_1 *= 32f / num6;
                }
                float t1 = 2f;
                float num9 = 10000f;
                treeIndex = 0U;
                Vector3 vector3_2 = ray.a;
                Vector3 vector3_3 = ray.a;
                int a1 = num1;
                int a2 = num2;
                do
                {
                    Vector3 vector3_4 = vector3_3 + vector3_1;
                    int num10;
                    int num11;
                    int num12;
                    int num13;
                    if (num7 != 0)
                    {
                        num10 = a1 == num1 && num7 > 0 || a1 == num3 && num7 < 0 ? Mathf.Max((int)(((double)vector3_4.x - 72.0) / 32.0 + 270.0), 0) : Mathf.Max(a1, 0);
                        num11 = a1 == num1 && num7 < 0 || a1 == num3 && num7 > 0 ? Mathf.Min((int)(((double)vector3_4.x + 72.0) / 32.0 + 270.0), 539) : Mathf.Min(a1, 539);
                        num12 = Mathf.Max((int)(((double)Mathf.Min(vector3_2.z, vector3_4.z) - 72.0) / 32.0 + 270.0), 0);
                        num13 = Mathf.Min((int)(((double)Mathf.Max(vector3_2.z, vector3_4.z) + 72.0) / 32.0 + 270.0), 539);
                    }
                    else
                    {
                        num12 = a2 == num2 && num8 > 0 || a2 == num4 && num8 < 0 ? Mathf.Max((int)(((double)vector3_4.z - 72.0) / 32.0 + 270.0), 0) : Mathf.Max(a2, 0);
                        num13 = a2 == num2 && num8 < 0 || a2 == num4 && num8 > 0 ? Mathf.Min((int)(((double)vector3_4.z + 72.0) / 32.0 + 270.0), 539) : Mathf.Min(a2, 539);
                        num10 = Mathf.Max((int)(((double)Mathf.Min(vector3_2.x, vector3_4.x) - 72.0) / 32.0 + 270.0), 0);
                        num11 = Mathf.Min((int)(((double)Mathf.Max(vector3_2.x, vector3_4.x) + 72.0) / 32.0 + 270.0), 539);
                    }
                    for (int index1 = num12; index1 <= num13; ++index1)
                    {
                        for (int index2 = num10; index2 <= num11; ++index2)
                        {
                            uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                            int num14 = 0;
                            while ((int)treeID != 0)
                            {
                                if (((TreeInstance.Flags)tm.m_trees.m_buffer[treeID].m_flags & ignoreFlags) == TreeInstance.Flags.None && (double)ray.DistanceSqr(tm.m_trees.m_buffer[treeID].Position) < 2500.0)
                                {
                                    TreeInfo info = tm.m_trees.m_buffer[treeID].Info;
                                    float t2;
                                    float targetSqr;
                                    if ((service == ItemClass.Service.None || info.m_class.m_service == service) && (subService == ItemClass.SubService.None || info.m_class.m_subService == subService) && ((itemLayers == ItemClass.Layer.None || (info.m_class.m_layer & itemLayers) != ItemClass.Layer.None) && tm.m_trees.m_buffer[treeID].RayCast(treeID, ray, out t2, out targetSqr)) && ((double)t2 < (double)t1 - 9.99999974737875E-05 || (double)t2 < (double)t1 + 9.99999974737875E-05 && (double)targetSqr < (double)num9))
                                    {
                                        t1 = t2;
                                        num9 = targetSqr;
                                        treeIndex = treeID;
                                    }
                                }
                                treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                                if (++num14 > Helper.TreeLimit)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                    vector3_2 = vector3_3;
                    vector3_3 = vector3_4;
                    a1 += num7;
                    a2 += num8;
                }
                while ((a1 <= num3 || num7 <= 0) && (a1 >= num3 || num7 >= 0) && ((a2 <= num4 || num8 <= 0) && (a2 >= num4 || num8 >= 0)));
                if ((double)t1 != 2.0)
                {
                    hit = ray.Position(t1);
                    return true;
                }
            }
            hit = Vector3.zero;
            treeIndex = 0U;
            return false;
        }

        [ReplaceMethod]
        public static void TerrainUpdated(TreeManager tm, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            float minX = surfaceArea.m_min.x;
            float minZ = surfaceArea.m_min.z;
            float maxX = surfaceArea.m_max.x;
            float maxZ = surfaceArea.m_max.z;
            int num1 = Mathf.Max((int)(((double)minX - 8.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 8.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 8.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)maxZ + 8.0) / 32.0 + 270.0), 539);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)treeID != 0)
                    {
                        Vector3 position = tm.m_trees.m_buffer[treeID].Position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max((float)((double)position.x - (double)maxX - 8.0), (float)((double)position.z - (double)maxZ - 8.0))) < 0.0)
                            tm.m_trees.m_buffer[treeID].TerrainUpdated(treeID, minX, minZ, maxX, maxZ);
                        treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public static void AfterTerrainUpdate(TreeManager tm, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            float minX = heightArea.m_min.x;
            float minZ = heightArea.m_min.z;
            float maxX = heightArea.m_max.x;
            float maxZ = heightArea.m_max.z;
            int num1 = Mathf.Max((int)(((double)minX - 8.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 8.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 8.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)maxZ + 8.0) / 32.0 + 270.0), 539);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)treeID != 0)
                    {
                        Vector3 position = tm.m_trees.m_buffer[treeID].Position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max((float)((double)position.x - (double)maxX - 8.0), (float)((double)position.z - (double)maxZ - 8.0))) < 0.0)
                            tm.m_trees.m_buffer[treeID].AfterTerrainUpdated(treeID, minX, minZ, maxX, maxZ);
                        treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public static void CalculateAreaHeight(TreeManager tm, float minX, float minZ, float maxX, float maxZ, out int num, out float min, out float avg, out float max)
        {
            int num1 = Mathf.Max((int)(((double)minX - 8.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 8.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 8.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)maxZ + 8.0) / 32.0 + 270.0), 539);
            num = 0;
            min = 1024f;
            avg = 0.0f;
            max = 0.0f;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint _seed = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)_seed != 0)
                    {
                        Vector3 position = tm.m_trees.m_buffer[_seed].Position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max((float)((double)position.x - (double)maxX - 8.0), (float)((double)position.z - (double)maxZ - 8.0))) < 0.0)
                        {
                            TreeInfo info = tm.m_trees.m_buffer[_seed].Info;
                            if (info != null)
                            {
                                Randomizer randomizer = new Randomizer(_seed);
                                float num6 = info.m_minScale + (float)((double)randomizer.Int32(10000U) * ((double)info.m_maxScale - (double)info.m_minScale) * 9.99999974737875E-05);
                                float num7 = position.y + (float)((double)info.m_generatedInfo.m_size.y * (double)num6 * 2.0);
                                if ((double)num7 < (double)min)
                                    min = num7;
                                avg = avg + num7;
                                if ((double)num7 > (double)max)
                                    max = num7;
                                num = num + 1;
                            }
                        }
                        _seed = tm.m_trees.m_buffer[_seed].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            if ((double)avg == 0.0)
                return;
            avg = avg / (float)num;
        }

        [ReplaceMethod]
        public static bool CalculateGroupData(TreeManager tm, int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            bool flag = false;
            if (layer != tm.m_treeLayer)
                return flag;
            int num1 = groupX * 540 / 45;
            int num2 = groupZ * 540 / 45;
            int num3 = (groupX + 1) * 540 / 45 - 1;
            int num4 = (groupZ + 1) * 540 / 45 - 1;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)treeID != 0)
                    {
                        if (tm.m_trees.m_buffer[treeID].CalculateGroupData(treeID, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays))
                            flag = true;
                        treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        [ReplaceMethod]
        public static void PopulateGroupData(TreeManager tm, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            if (layer != tm.m_treeLayer)
                return;
            int num1 = groupX * 540 / 45;
            int num2 = groupZ * 540 / 45;
            int num3 = (groupX + 1) * 540 / 45 - 1;
            int num4 = (groupZ + 1) * 540 / 45 - 1;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    uint treeID = tm.m_treeGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)treeID != 0)
                    {
                        tm.m_trees.m_buffer[treeID].PopulateGroupData(treeID, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                        treeID = tm.m_trees.m_buffer[treeID].m_nextGridTree;
                        if (++num5 >= Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public static void UpdateData(TreeManager tm, SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("TreeManager.UpdateData");
            // base.UpdateData(mode);
            Helper.EnsureInit();
            for (int index = 1; index < Helper.TreeLimit; ++index)
            {
                if ((int)tm.m_trees.m_buffer[index].m_flags != 0 && tm.m_trees.m_buffer[index].Info == null)
                    tm.ReleaseTree((uint)index);
            }
            int num = PrefabCollection<TreeInfo>.PrefabCount();
            int tiles = 1;
            while (tiles * tiles < num)
                ++tiles;
            for (int index = 0; index < num; ++index)
            {
                TreeInfo prefab = PrefabCollection<TreeInfo>.GetPrefab((uint)index);
                if (prefab != null)
                    prefab.SetRenderParameters(index, tiles);
            }
            ColossalFramework.Threading.ThreadHelper.dispatcher.Dispatch((Action)(() =>
            {
                tm.GetType().GetField("m_lastShadowRotation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(tm, new Quaternion());
                tm.GetType().GetField("m_lastCameraRotation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(tm, new Quaternion());
            }));
            tm.m_infoCount = num;
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        [ReplaceMethod]
        public static void InitializeTree(TreeManager tm, uint tree, ref TreeInstance data, bool assetEditor)
        {
            int num1;
            int num2;
            if (assetEditor)
            {
                num1 = Mathf.Clamp(((int)data.m_posX / 16 + 32768) * 540 / 65536, 0, 539);
                num2 = Mathf.Clamp(((int)data.m_posZ / 16 + 32768) * 540 / 65536, 0, 539);
            }
            else
            {
                num1 = Mathf.Clamp(((int)data.m_posX + 32768) * 540 / 65536, 0, 539);
                num2 = Mathf.Clamp(((int)data.m_posZ + 32768) * 540 / 65536, 0, 539);
            }
            int index = num2 * 540 + num1;
            do
            { }
            while (!Monitor.TryEnter((object)tm.m_treeGrid, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                tm.m_trees.m_buffer[tree].m_nextGridTree = tm.m_treeGrid[index];
                tm.m_treeGrid[index] = tree;
            }
            finally
            {
                Monitor.Exit((object)tm.m_treeGrid);
            }
        }

        internal class Data
        {
            [ReplaceMethod]
            public static void Serialize(TreeManager.Data data, DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "TreeManager");
                TreeInstance[] treeInstanceArray = Singleton<TreeManager>.instance.m_trees.m_buffer;
                int length = Mod.DEFAULT_TREE_COUNT; //treeInstanceArray.Length;
                EncodedArray.UShort @ushort = EncodedArray.UShort.BeginWrite(s);
                for (int index = 1; index < length; ++index)
                    @ushort.Write(treeInstanceArray[index].m_flags);
                @ushort.EndWrite();
                try
                {
                    PrefabCollection<TreeInfo>.BeginSerialize(s);
                    for (int index = 1; index < length; ++index)
                    {
                        if ((int)treeInstanceArray[index].m_flags != 0)
                            PrefabCollection<TreeInfo>.Serialize((uint)treeInstanceArray[index].m_infoIndex);
                    }
                }
                finally
                {
                    PrefabCollection<TreeInfo>.EndSerialize(s);
                }
                EncodedArray.Short short1 = EncodedArray.Short.BeginWrite(s);
                for (int index = 1; index < length; ++index)
                {
                    if ((int)treeInstanceArray[index].m_flags != 0)
                        short1.Write(treeInstanceArray[index].m_posX);
                }
                short1.EndWrite();
                EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
                for (int index = 1; index < length; ++index)
                {
                    if ((int)treeInstanceArray[index].m_flags != 0)
                        short2.Write(treeInstanceArray[index].m_posZ);
                }
                short2.EndWrite();

                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "TreeManager");
            }

            [ReplaceMethod]
            public static void Deserialize(TreeManager.Data data, DataSerializer s)
            {
                Helper.EnsureInit();

                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "TreeManager");
                TreeManager instance = Singleton<TreeManager>.instance;
                TreeInstance[] treeInstanceArray = instance.m_trees.m_buffer;
                uint[] numArray = instance.m_treeGrid;
                int length1 = Mod.DEFAULT_TREE_COUNT;// treeInstanceArray.Length;
                int length2 = numArray.Length;
                instance.m_trees.ClearUnused();
                SimulationManager.UpdateMode updateMode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
                bool assetEditor = updateMode == SimulationManager.UpdateMode.NewAsset || updateMode == SimulationManager.UpdateMode.LoadAsset;
                for (int index = 0; index < length2; ++index)
                    numArray[index] = 0U;
                EncodedArray.UShort @ushort = EncodedArray.UShort.BeginRead(s);
                for (int index = 1; index < length1; ++index)
                    treeInstanceArray[index].m_flags = @ushort.Read();
                @ushort.EndRead();
                PrefabCollection<TreeInfo>.BeginDeserialize(s);
                for (int index = 1; index < length1; ++index)
                {
                    if ((int)treeInstanceArray[index].m_flags != 0)
                        treeInstanceArray[index].m_infoIndex = (ushort)PrefabCollection<TreeInfo>.Deserialize();
                }
                PrefabCollection<TreeInfo>.EndDeserialize(s);
                EncodedArray.Short short1 = EncodedArray.Short.BeginRead(s);
                for (int index = 1; index < length1; ++index)
                    treeInstanceArray[index].m_posX = (int)treeInstanceArray[index].m_flags == 0 ? (short)0 : short1.Read();
                short1.EndRead();
                EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
                for (int index = 1; index < length1; ++index)
                    treeInstanceArray[index].m_posZ = (int)treeInstanceArray[index].m_flags == 0 ? (short)0 : short2.Read();
                short2.EndRead();
                for (int index = 1; index < length1; ++index)
                {
                    treeInstanceArray[index].m_nextGridTree = 0U;
                    treeInstanceArray[index].m_posY = (ushort)0;
                }

                // needed to ensure enough free spaces are available
                if (Helper.UseModifiedTreeCap)
                {
                    CustomSerializer.Deserialize();
                }

                // I feel like this is called in some other way; at least it was usually bugging out when the number of total loaded trees exceeded the natural cap.
                // This was only noticable after reloading saves/creating new games often enough.
                // This would cause m_trees.ReleaseItem() to exceed array buffers and throw an exception.
                instance.m_trees.ClearUnused();
                for (int index = 0; index < Helper.TreeLimit; ++index)
                {
                    if ((int)treeInstanceArray[index].m_flags != 0)
                        LimitTreeManager.InitializeTree(instance, (uint)index, ref treeInstanceArray[index], assetEditor);
                    else
                        instance.m_trees.ReleaseItem((uint)index);
                }

                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "TreeManager");
            }
        }

        internal static class CustomSerializer
        {
            [ReplaceMethod]
            public static void Serialize()
            {
                if (!Helper.UseModifiedTreeCap)
                    return;

                TreeManager instance = Singleton<TreeManager>.instance;
                TreeInstance[] treeInstanceArray = instance.m_trees.m_buffer;

                List<ushort> data = new List<ushort>();

                // Version
                data.Add(1);

                int serialized = 0;
                for (int i = Mod.DEFAULT_TREE_COUNT; i < Helper.TreeLimit; ++i)
                {
                    TreeInstance element = treeInstanceArray[i];
                    data.Add(element.m_flags);
                    if (element.m_flags != 0)
                    {
                        data.Add(element.m_infoIndex);
                        data.Add((ushort)element.m_posX);
                        data.Add((ushort)element.m_posZ);
                        ++serialized;
                    }
                }

                // FIXME is it possible to loose trees here? CheckLimits() uses (LIMIT-5) by default
                Debug.LogFormat("[TreeLimit] used {0} of {1} extra trees, size in savegame: {2} bytes", serialized, Helper.TreeLimit - Mod.DEFAULT_TREE_COUNT, data.Count * 2);
                SimulationManager.instance.m_serializableDataStorage["mabako/unlimiter"] = data.SelectMany(v => BitConverter.GetBytes(v)).ToArray();
            }

            [ReplaceMethod]
            public static bool Deserialize()
            {
                Helper.EnsureInit();
                if (!Helper.UseModifiedTreeCap)
                    return false;

                byte[] bytes = null;
                if (SimulationManager.instance.m_serializableDataStorage.TryGetValue("mabako/unlimiter", out bytes))
                {
                    Debug.LogFormat("[TreeLimit] we have {0} bytes of extra trees", bytes.Length);
                    if (bytes.Length < 2 || bytes.Length % 2 != 0)
                    {
                        Debug.Log("[TreeLimit] Invalid chunk size");
                        return false;
                    }

                    // just the required things
                    TreeManager instance = Singleton<TreeManager>.instance;
                    TreeInstance[] treeInstanceArray = instance.m_trees.m_buffer;

                    // we basically need shorts for everything
                    ushort[] shorts = new ushort[bytes.Length / 2];
                    Buffer.BlockCopy(bytes, 0, shorts, 0, bytes.Length);

                    uint pos = 0;
                    ushort version = shorts[pos++];
                    if (version != 1)
                    {
                        Debug.LogFormat("[TreeLimit] Wrong version ({0}|{1}|{2},{3}).", shorts[0], version, bytes[0], bytes[1]);
                        return false;
                    }

                    int loaded = 0;
                    for (int index = Mod.DEFAULT_TREE_COUNT; index < Helper.TreeLimit; ++index)
                    {
                        uint savedPos = pos;
                        try
                        {
                            treeInstanceArray[index].m_flags = shorts[pos++];
                            if (treeInstanceArray[index].m_flags != 0)
                            {
                                treeInstanceArray[index].m_infoIndex = shorts[pos++];
                                treeInstanceArray[index].m_posX = (short)shorts[pos++];
                                treeInstanceArray[index].m_posY = 0;
                                treeInstanceArray[index].m_posZ = (short)shorts[pos++];

                                ++loaded;
                            }

                            if (pos == shorts.Length)
                                break;
                        }
                        catch (Exception e)
                        {
                            Debug.LogFormat("[TreeLimit]While fetching tree {0} in pos {1} of {2}", index, savedPos, shorts.Length);
                            Debug.LogException(e);
                            throw e;
                        }
                    }
                    Debug.LogFormat("[TreeLimit] Loaded {0} additional trees (out of {1} possible)", loaded, Helper.TreeLimit - Mod.DEFAULT_TREE_COUNT);
                    return true;
                }
                else
                {
                    Debug.Log("[TreeLimit] no extra data saved with this savegame.");
                    return false;
                }
            }
        }
    }
}
