using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter
{
    internal static class LimitTreeTool
    {
        private static void ApplyBrush(TreeTool tt)
        {
            Randomizer r = (Randomizer)tt.GetType().GetField("m_randomizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);
            ToolController m_toolController = (ToolController)tt.GetType().GetField("m_toolController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);
            Vector3 vector3_1 = (Vector3)tt.GetType().GetField("m_mousePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);
            bool m_mouseLeftDown = (bool)tt.GetType().GetField("m_mouseLeftDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);
            bool m_mouseRightDown = (bool)tt.GetType().GetField("m_mouseRightDown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);
            TreeInfo m_treeInfo = (TreeInfo)tt.GetType().GetField("m_treeInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(tt);

            float[] brushData = m_toolController.BrushData;
            float num1 = tt.m_brushSize * 0.5f;
            float num2 = 32f;
            int num3 = 540;
            TreeInstance[] treeInstanceArray = Singleton<TreeManager>.instance.m_trees.m_buffer;
            uint[] numArray = Singleton<TreeManager>.instance.m_treeGrid;
            float num4 = tt.m_strength;
            int num5 = Mathf.Max((int)(((double)vector3_1.x - (double)num1) / (double)num2 + (double)num3 * 0.5), 0);
            int num6 = Mathf.Max((int)(((double)vector3_1.z - (double)num1) / (double)num2 + (double)num3 * 0.5), 0);
            int num7 = Mathf.Min((int)(((double)vector3_1.x + (double)num1) / (double)num2 + (double)num3 * 0.5), num3 - 1);
            int num8 = Mathf.Min((int)(((double)vector3_1.z + (double)num1) / (double)num2 + (double)num3 * 0.5), num3 - 1);
            for (int index1 = num6; index1 <= num8; ++index1)
            {
                float f1 = (float)((((double)index1 - (double)num3 * 0.5 + 0.5) * (double)num2 - (double)vector3_1.z + (double)num1) / (double)tt.m_brushSize * 64.0 - 0.5);
                int num9 = Mathf.Clamp(Mathf.FloorToInt(f1), 0, 63);
                int num10 = Mathf.Clamp(Mathf.CeilToInt(f1), 0, 63);
                for (int index2 = num5; index2 <= num7; ++index2)
                {
                    float f2 = (float)((((double)index2 - (double)num3 * 0.5 + 0.5) * (double)num2 - (double)vector3_1.x + (double)num1) / (double)tt.m_brushSize * 64.0 - 0.5);
                    int num11 = Mathf.Clamp(Mathf.FloorToInt(f2), 0, 63);
                    int num12 = Mathf.Clamp(Mathf.CeilToInt(f2), 0, 63);
                    float num13 = brushData[num9 * 64 + num11];
                    float num14 = brushData[num9 * 64 + num12];
                    float num15 = brushData[num10 * 64 + num11];
                    float num16 = brushData[num10 * 64 + num12];
                    float num17 = num13 + (float)(((double)num14 - (double)num13) * ((double)f2 - (double)num11));
                    float num18 = num15 + (float)(((double)num16 - (double)num15) * ((double)f2 - (double)num11));
                    float num19 = num17 + (float)(((double)num18 - (double)num17) * ((double)f1 - (double)num9));
                    int num20 = (int)((double)num4 * ((double)num19 * 1.20000004768372 - 0.200000002980232) * 10000.0);
                    if (m_mouseLeftDown && tt.m_prefab != null)
                    {
                        if (r.Int32(10000U) < num20)
                        {
                            TreeInfo info = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) == ItemClass.Availability.None ? tt.m_prefab.GetVariation(ref r) : tt.m_prefab;
                            Vector3 vector3_2;
                            vector3_2.x = ((float)index2 - (float)num3 * 0.5f) * num2;
                            vector3_2.z = ((float)index1 - (float)num3 * 0.5f) * num2;
                            vector3_2.x += (float)(((double)r.Int32(10000U) + 0.5) * ((double)num2 / 10000.0));
                            vector3_2.z += (float)(((double)r.Int32(10000U) + 0.5) * ((double)num2 / 10000.0));
                            vector3_2.y = 0.0f;
                            float slopeX;
                            float slopeZ;
                            vector3_2.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector3_2, out slopeX, out slopeZ);
                            if ((double)Mathf.Max(Mathf.Abs(slopeX), Mathf.Abs(slopeZ)) < (double)r.Int32(10000U) * 4.99999987368938E-05)
                            {
                                float num21 = m_treeInfo.m_generatedInfo.m_size.y * (m_treeInfo.m_minScale + (float)((double)new Randomizer(Singleton<TreeManager>.instance.m_trees.NextFreeItem(ref r)).Int32(10000U) * ((double)m_treeInfo.m_maxScale - (double)m_treeInfo.m_minScale) * 9.99999974737875E-05));
                                float num22 = 4.5f;
                                Vector2 position = VectorUtils.XZ(vector3_2);
                                Quad2 quad1 = new Quad2();
                                quad1.a = position + new Vector2(-num22, -num22);
                                quad1.b = position + new Vector2(-num22, num22);
                                quad1.c = position + new Vector2(num22, num22);
                                quad1.d = position + new Vector2(num22, -num22);
                                Quad2 quad2 = new Quad2();
                                quad2.a = position + new Vector2(-8f, -8f);
                                quad2.b = position + new Vector2(-8f, 8f);
                                quad2.c = position + new Vector2(8f, 8f);
                                quad2.d = position + new Vector2(8f, -8f);
                                float minY = vector3_1.y;
                                float maxY = vector3_1.y + num21;
                                uint tree;
                                if (Singleton<PropManager>.instance.OverlapQuad(quad1, minY, maxY, 0, (ushort)0) || Singleton<TreeManager>.instance.OverlapQuad(quad2, minY, maxY, 0, 0U) || (Singleton<NetManager>.instance.OverlapQuad(quad1, minY, maxY, info.m_class.m_layer, (ushort)0, (ushort)0, (ushort)0) || Singleton<BuildingManager>.instance.OverlapQuad(quad1, minY, maxY, info.m_class.m_layer, (ushort)0, (ushort)0, (ushort)0)) || (Singleton<TerrainManager>.instance.HasWater(position) || Singleton<GameAreaManager>.instance.QuadOutOfArea(quad1) || !Singleton<TreeManager>.instance.CreateTree(out tree, ref r, info, vector3_2, false)))
                                { }
                            }
                        }
                    }
                    else if (m_mouseRightDown || tt.m_prefab == null)
                    {
                        uint tree = numArray[index1 * num3 + index2];
                        int num21 = 0;
                        while ((int)tree != 0)
                        {
                            uint num22 = treeInstanceArray[tree].m_nextGridTree;
                            if (r.Int32(10000U) < num20)
                                Singleton<TreeManager>.instance.ReleaseTree(tree);
                            tree = num22;
                            if (++num21 >= Mod.MAX_TREE_COUNT)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
