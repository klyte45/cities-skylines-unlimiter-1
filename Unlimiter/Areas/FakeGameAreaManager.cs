using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter.Areas
{
    internal class FakeGameAreaManager
    {
        private static class Helper
        {
            internal static void EnsureInit()
            {
                if(GameAreaManager.instance.m_areaGrid.Length != GRID * GRID)
                {
                    int[] areas = new int[GRID * GRID];
                    for (int z = 0; z < DEFAULT_GRID; z++)
                    {
                        for (int x = 0; x < DEFAULT_GRID; ++x)
                        {
                            areas[(z + GRID_DIFF) * GRID + (x + GRID_DIFF)] = GameAreaManager.instance.m_areaGrid[z * DEFAULT_GRID + x];
                            Debug.LogFormat("{0} {1} -> {2}, {3}; {4}", z, x, (z + GRID_DIFF) * GRID + (x + GRID_DIFF), z * DEFAULT_GRID + x, GameAreaManager.instance.m_areaGrid[z * DEFAULT_GRID + x]);
                        }
                    }
                    GameAreaManager.instance.m_areaGrid = areas;
                }
            }
        }

        public const int DEFAULT_GRID = 5;
        public const int GRID_DIFF = 2;
        public const int GRID = DEFAULT_GRID + 2 * GRID_DIFF;


        private static Vector3 GetAreaPositionSmooth(GameAreaManager g, int x, int z)
        {
            //GameAreaInfoPanel gameAreaInfoPanel = UIView.library.Hide<GameAreaInfoPanel>("GameAreaInfoPanel");
            //if (gameAreaInfoPanel == null)
            //   return Vector3.zero;

            int tile = z * DEFAULT_GRID + x;
            GetTileXZ(g, tile, out x, out z);
            // we already know this is literally only used in one place
            // Debug.LogFormat("FFF {0} | {1}, {2}", gameAreaInfoPanel.GetType().GetField("m_AreaIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(gameAreaInfoPanel), x, z);
            if (x < -GRID_DIFF || z < -GRID_DIFF || (x >= DEFAULT_GRID + GRID_DIFF || z >= DEFAULT_GRID + GRID_DIFF))
                return Vector3.zero;
            Vector3 worldPos;
            worldPos.x = (float)(((double)x - 2.5 + 0.5) * 1920.0);
            worldPos.y = 0.0f;
            worldPos.z = (float)(((double)z - 2.5 + 0.5) * 1920.0);
            worldPos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(worldPos, true, 0.0f);
            return worldPos;
        }

        private static bool CanUnlock(GameAreaManager g, int x, int z)
        {
            if (x < -GRID_DIFF || z < -GRID_DIFF || (x >= DEFAULT_GRID + GRID_DIFF || z >= DEFAULT_GRID + GRID_DIFF) || (g.m_areaCount >= g.MaxAreaCount || g.m_areaGrid[(z + GRID_DIFF) * GRID + (x + GRID_DIFF)] != 0))
                return false;
            bool result = g.IsUnlocked(x, z - 1) || g.IsUnlocked(x - 1, z) || g.IsUnlocked(x + 1, z) || g.IsUnlocked(x, z + 1);
            g.m_AreasWrapper.OnCanUnlockArea(x, z, ref result);
            return result;
        }



        private static void BeginOverlayImpl(GameAreaManager g, RenderManager.CameraInfo cameraInfo)
        {
            float m_borderAlpha = (float)g.GetType().GetField("m_borderAlpha", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            float m_areaAlpha = (float)g.GetType().GetField("m_areaAlpha", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_borderMaterial = (Material)g.GetType().GetField("m_borderMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_areaMaterial = (Material)g.GetType().GetField("m_areaMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_decorationMaterial = (Material)g.GetType().GetField("m_decorationMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Mesh m_borderMesh = (Mesh)g.GetType().GetField("m_borderMesh", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            
            int ID_Color = (int) g.GetType().GetField("ID_Color", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            int ID_AreaMapping = (int) g.GetType().GetField("ID_AreaMapping", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            int ID_DecorationArea = (int)g.GetType().GetField("ID_DecorationArea", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            int ID_DecorationAlpha = (int)g.GetType().GetField("ID_DecorationAlpha", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Texture2D m_areaTex = (Texture2D)g.GetType().GetField("m_areaTex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);

            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
#if false
                if ((double)m_borderAlpha < 1.0 / 1000.0)
                    return;
                PrefabInfo prefabInfo = Singleton<ToolManager>.instance.m_properties.m_editPrefabInfo;
                if (!((UnityEngine.Object)prefabInfo != (UnityEngine.Object)null))
                    return;
                Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(17280f, 1224f, 17280f));
                int width;
                int length;
                float offset;
                prefabInfo.GetDecorationArea(out width, out length, out offset);
                bool negX;
                bool posX;
                bool negZ;
                bool posZ;
                prefabInfo.GetDecorationDirections(out negX, out posX, out negZ, out posZ);
                float z = (float)width * 4f;
                float num = (float)length * 4f;
                m_decorationMaterial.SetVector(ID_DecorationArea, new Vector4(-z, offset - num, z, offset + num));
                m_decorationMaterial.SetFloat(ID_DecorationAlpha, m_borderAlpha);
                ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, m_decorationMaterial, 0, bounds);
                if (!posZ)
                    return;
                Color color = new Color(1f, 1f, 1f, 0.25f);
                Quad3 quad;
                quad.a = new Vector3(-10f, 60f, (float)((double)num + 16.0 + 10.0));
                quad.b = new Vector3(-10f, 60f, (float)((double)num + 16.0 - 10.0));
                quad.c = new Vector3(10f, 60f, (float)((double)num + 16.0 - 10.0));
                quad.d = new Vector3(10f, 60f, (float)((double)num + 16.0 + 10.0));
                ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, g.m_properties.m_directionArrow, color, quad, 50f, 70f, false, true);
#endif
            }
            else if ((availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
#if false
                if ((double)m_borderAlpha >= 1.0 / 1000.0 && (UnityEngine.Object)m_borderMaterial != (UnityEngine.Object)null)
                {
                    Quaternion rotation = Quaternion.AngleAxis(90f, Vector3.up);
                    int x1;
                    int z1;
                    g.GetStartTile(out x1, out z1);
                    Color color1 = new Color(1f, 1f, 1f, m_borderAlpha);
                    Color color2 = new Color(1f, 1f, 1f, m_borderAlpha);
                    Color color3 = new Color(1f, 1f, 1f, 0.25f * m_borderAlpha);
                    for (int z2 = 0; z2 <= 5; ++z2)
                    {
                        for (int x2 = 0; x2 <= 5; ++x2)
                        {
                            bool flag1 = g.GetArea(x2, z2) > 0;
                            bool flag2 = g.GetArea(x2, z2 - 1) > 0;
                            bool flag3 = g.GetArea(x2 - 1, z2) > 0;
                            if (x2 != 5)
                            {
                                Vector3 vector3 = new Vector3((float)(((double)x2 - 2.5 + 0.5) * 1920.0), 0.0f, (float)(((double)z2 - 2.5) * 1920.0));
                                Vector3 size = new Vector3(1920f, 1024f, 100f);
                                Bounds bounds = new Bounds(vector3 + new Vector3(0.0f, size.y * 0.5f, 0.0f), size);
                                if (cameraInfo.Intersect(bounds))
                                {
                                    Singleton<TerrainManager>.instance.SetWaterMaterialProperties(vector3, m_borderMaterial);
                                    if (z2 >= z1 && z2 <= z1 + 1 && x2 == x1 || flag1 != flag2)
                                        m_borderMaterial.SetColor(ID_Color, color1);
                                    else if (z2 == 0 || z2 == 5)
                                        m_borderMaterial.SetColor(ID_Color, color2);
                                    else
                                        m_borderMaterial.SetColor(ID_Color, color3);
                                    if (m_borderMaterial.SetPass(0))
                                    {
                                        ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                                        Graphics.DrawMeshNow(m_borderMesh, vector3, rotation);
                                    }
                                }
                            }
                            if (z2 != 5)
                            {
                                Vector3 vector3 = new Vector3((float)(((double)x2 - 2.5) * 1920.0), 0.0f, (float)(((double)z2 - 2.5 + 0.5) * 1920.0));
                                Vector3 size = new Vector3(100f, 1024f, 1920f);
                                Bounds bounds = new Bounds(vector3 + new Vector3(0.0f, size.y * 0.5f, 0.0f), size);
                                if (cameraInfo.Intersect(bounds))
                                {
                                    Singleton<TerrainManager>.instance.SetWaterMaterialProperties(vector3, m_borderMaterial);
                                    if (x2 >= x1 && x2 <= x1 + 1 && z2 == z1 || flag1 != flag3)
                                        m_borderMaterial.SetColor(ID_Color, color1);
                                    else if (x2 == 0 || x2 == 5)
                                        m_borderMaterial.SetColor(ID_Color, color2);
                                    else
                                        m_borderMaterial.SetColor(ID_Color, color3);
                                    if (m_borderMaterial.SetPass(0))
                                    {
                                        ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                                        Graphics.DrawMeshNow(m_borderMesh, vector3, Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                }
                if ((double)m_areaAlpha < 1.0 / 1000.0 || !((UnityEngine.Object)m_areaMaterial != (UnityEngine.Object)null))
                    return;
                Vector4 vector;
                vector.z = 6.510417E-05f;
                vector.x = 7.0f / 16.0f;
                vector.y = 7.0f / 16.0f;
                vector.w = 0.125f;
                m_areaMaterial.mainTexture = (Texture)m_areaTex;
                m_areaMaterial.SetColor(ID_Color, new Color(1f, 1f, 1f, m_areaAlpha));
                m_areaMaterial.SetVector(ID_AreaMapping, vector);
                Bounds bounds1 = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(9600f, 1024f, 9600f));
                bounds1.size = bounds1.size + new Vector3(100f, 1f, 100f);
                ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, m_areaMaterial, 0, bounds1);
#endif
            }
            else
            {
                if ((double)m_borderAlpha >= 1.0 / 1000.0 && (UnityEngine.Object)m_borderMaterial != (UnityEngine.Object)null)
                {
                    Quaternion rotation = Quaternion.AngleAxis(90f, Vector3.up);
                    Color color = Color.white;
                    ToolController toolController = Singleton<ToolManager>.instance.m_properties;
                    if ((UnityEngine.Object)toolController != (UnityEngine.Object)null && (toolController.CurrentTool.GetErrors() & ToolBase.ToolErrors.OutOfArea) != ToolBase.ToolErrors.None)
                        color = Color.red;
                    color.a = m_borderAlpha;
                    for (int z = -GRID_DIFF; z <= DEFAULT_GRID + GRID_DIFF; ++z)
                    {
                        for (int x = -GRID_DIFF; x <= DEFAULT_GRID + GRID_DIFF; ++x)
                        {
                            bool flag1 = g.GetArea(x, z) > 0;
                            bool flag2 = g.GetArea(x, z - 1) > 0;
                            bool flag3 = g.GetArea(x - 1, z) > 0;
                            if (flag1 != flag2)
                            {
                                Vector3 vector3 = new Vector3((float)(((double)x - 2.5 + 0.5) * 1920.0), 0.0f, (float)(((double)z - 2.5) * 1920.0));
                                Vector3 size = new Vector3(1920f, 1024f, 100f);
                                Bounds bounds = new Bounds(vector3 + new Vector3(0.0f, size.y * 0.5f, 0.0f), size);
                                if (cameraInfo.Intersect(bounds))
                                {
                                    Singleton<TerrainManager>.instance.SetWaterMaterialProperties(vector3, m_borderMaterial);
                                    m_borderMaterial.SetColor(ID_Color, color);
                                    if (m_borderMaterial.SetPass(0))
                                    {
                                        ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                                        Graphics.DrawMeshNow(m_borderMesh, vector3, rotation);
                                    }
                                }
                            }
                            if (flag1 != flag3)
                            {
                                Vector3 vector3 = new Vector3((float)(((double)x - 2.5) * 1920.0), 0.0f, (float)(((double)z - 2.5 + 0.5) * 1920.0));
                                Vector3 size = new Vector3(100f, 1024f, 1920f);
                                Bounds bounds = new Bounds(vector3 + new Vector3(0.0f, size.y * 0.5f, 0.0f), size);
                                if (cameraInfo.Intersect(bounds))
                                {
                                    Singleton<TerrainManager>.instance.SetWaterMaterialProperties(vector3, m_borderMaterial);
                                    m_borderMaterial.SetColor(ID_Color, color);
                                    if (m_borderMaterial.SetPass(0))
                                    {
                                        ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                                        Graphics.DrawMeshNow(m_borderMesh, vector3, Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                }
                if (m_areaAlpha < 1.0 / 1000.0 || !((UnityEngine.Object)m_areaMaterial != (UnityEngine.Object)null))
                    return;
                Vector4 vector;
                vector.z = 6.510417E-05f;
                vector.x = 7.0f / 16.0f;
                vector.y = 7.0f / 16.0f;
                vector.w = 0.125f;
                m_areaMaterial.mainTexture = (Texture)m_areaTex;
                m_areaMaterial.SetColor(ID_Color, new Color(1f, 1f, 1f, m_areaAlpha));
                m_areaMaterial.SetVector(ID_AreaMapping, vector);
                Bounds freeBounds = GetFreeBounds(g);
                freeBounds.size = freeBounds.size + new Vector3(100f, 1f, 100f);
                ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, m_areaMaterial, 0, freeBounds);
            }
        }

        private static Bounds GetFreeBounds(GameAreaManager g)
        {
            Vector3 zero1 = Vector3.zero;
            Vector3 zero2 = Vector3.zero;
            for (int z = 0; z < DEFAULT_GRID; ++z)
            {
                for (int x = 0; x < DEFAULT_GRID; ++x)
                {
                    if (g.IsUnlocked(x, z))
                    {
                        zero1.x = Mathf.Min(zero1.x, (float)(((x - 1) - 2.5) * 1920.0));
                        zero2.x = Mathf.Max(zero2.x, (float)(((x + 2) - 2.5) * 1920.0));
                        zero1.z = Mathf.Min(zero1.z, (float)(((z - 1) - 2.5) * 1920.0));
                        zero2.z = Mathf.Max(zero2.z, (float)(((z + 2) - 2.5) * 1920.0));
                        zero2.y = Mathf.Max(zero2.y, 1024f);
                    }
                }
            }

            //new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(9600f, 1024f, 9600f))

            Bounds bounds = new Bounds();
            bounds.SetMinMax(zero1, zero2);
            return bounds;
        }

        private static int GetArea(GameAreaManager g, int x, int z)
        {
            if (x >= -GRID_DIFF && z >= -GRID_DIFF && (x < DEFAULT_GRID - GRID_DIFF && z < DEFAULT_GRID - GRID_DIFF))
                return g.m_areaGrid[(z + GRID_DIFF) * GRID + (x + GRID_DIFF)];
            return -2;
        }

        private static bool IsUnlocked(GameAreaManager g, int x, int z)
        {
            if (x < -GRID_DIFF || z < -GRID_DIFF || (x >= DEFAULT_GRID + GRID_DIFF || z >= DEFAULT_GRID + GRID_DIFF))
                return false;
            return g.m_areaGrid[(z + GRID_DIFF) * GRID + (x + GRID_DIFF)] != 0;
        }

        private static int GetAreaIndex(GameAreaManager g, Vector3 p)
        {
            int x = Mathf.FloorToInt((float)((double)p.x / 1920.0 + 2.5));
            int z = Mathf.FloorToInt((float)((double)p.z / 1920.0 + 2.5));
            if (x < -GRID_DIFF || z < -GRID_DIFF || (x >= DEFAULT_GRID + GRID_DIFF || z >= DEFAULT_GRID + GRID_DIFF))
                return -1;

            return (z + GRID_DIFF) * GRID + (x + GRID_DIFF);
        }

        internal static void GetTileXZ(GameAreaManager g, int tile, out int x, out int z)
        {
            x = (tile % GRID) - GRID_DIFF;
            z = (tile / GRID) - GRID_DIFF;
        }

        private static int GetTileIndex(GameAreaManager g, int x, int z)
        {
            return (z + GRID_DIFF) * GRID + (x + GRID_DIFF);
        }

        public static int CalculateTilePrice(GameAreaManager g, int tile)
        {
            return 0;
        }

        // TODO: Fix calls to this
        private static bool UnlockArea(GameAreaManager g, int index)
        {
            g.GetType().GetField("m_unlocking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);
            try
            {
                int x = (index % GRID) - GRID_DIFF;
                int z = (index / GRID) - GRID_DIFF;
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("unlock {0} {1}", x, z));
                if (g.CanUnlock(x, z))
                {
                    g.m_areaNotUnlocked.Deactivate();
                    CODebugBase<LogChannel>.Log(LogChannel.Core, "Unlocking new area");
                    g.m_areaGrid[index] = ++g.m_areaCount;
                    g.GetType().GetField("m_areasUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);

                    float minX = (float)(((double)x - 2.5f) * 1920.0);
                    float maxX = (float)(((double)(x + 1) - 2.5f) * 1920.0);
                    float minZ = (float)(((double)z - 2.5f) * 1920.0);
                    float maxZ = (float)(((double)(z + 1) - 2.5f) * 1920.0);
                    Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                    int num = 2;
                    if (Singleton<TerrainManager>.instance.SetDetailedPatch(x + num, z + num))
                    {
                        Singleton<MessageManager>.instance.TryCreateMessage(g.m_properties.m_unlockMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
                        g.m_AreasWrapper.OnUnlockArea(x, z);
                        return true;
                    }
                    --g.m_areaCount;
                    g.m_areaGrid[index] = 0;

                    DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, "Areas updated failed");
                    g.GetType().GetField("m_areasUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);
                }
                else
                {
                    DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, "Can not unlock.");
                }
                return false;
            }
            finally
            {
                g.GetType().GetField("m_unlocking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, false);
            }
        }

        private static void UpdateData(GameAreaManager g, SimulationManager.UpdateMode mode)
        {
            Helper.EnsureInit();
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("GameAreaManager.UpdateData");
            //base.UpdateData(mode);
            if (mode == SimulationManager.UpdateMode.NewGame || mode == SimulationManager.UpdateMode.NewAsset || mode == SimulationManager.UpdateMode.LoadAsset)
            {
                g.m_areaCount = 0;
                int num = (int)g.GetType().GetField("m_startTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(g);
                for (int index1 = 0; index1 < GRID; ++index1)
                {
                    for (int index2 = 0; index2 < GRID; ++index2)
                    {
                        int index3 = index1 * GRID + index2;
                        g.m_areaGrid[index3] = index3 != num ? 0 : ++g.m_areaCount;
                    }
                }
            }
            int x1;
            int z1;
            g.GetStartTile(out x1, out z1);
            if (mode != SimulationManager.UpdateMode.LoadGame || (float)g.GetType().GetField("m_buildableArea0", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g) < 0.0)
            {
                float m_buildableArea0 = 0.0f;
                float m_buildableArea1 = 0.0f;
                float m_buildableArea2 = 0.0f;
                float m_buildableArea3 = 0.0f;
                float num1 = 0.0f;
                float num2 = 0.0f;
                float num3 = 0.0f;
                float num4 = 0.0f;
                for (int tileZ = -GRID_DIFF; tileZ < DEFAULT_GRID + GRID_DIFF; ++tileZ)
                {
                    for (int tileX = -GRID_DIFF; tileX < DEFAULT_GRID + GRID_DIFF; ++tileX)
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

                g.GetType().GetField("m_buildableArea0", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea0);
                g.GetType().GetField("m_buildableArea1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea1);
                g.GetType().GetField("m_buildableArea2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea2);
                g.GetType().GetField("m_buildableArea3", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea3);
            }
            int num5 = 0;// 2;
            for (int z2 = 0; z2 < GRID; ++z2)
            {
                for (int x2 = 0; x2 < GRID; ++x2)
                {
                    if (g.GetArea(x2 - GRID_DIFF, z2 - GRID_DIFF) > 0)
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2 + num5, z2 + num5);
                }
            }
            if (mode == SimulationManager.UpdateMode.NewGame || g.m_areaNotUnlocked == null)
                g.m_areaNotUnlocked = new GenericGuide();
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

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
        private static void UpdateAreaTexture(GameAreaManager g)
        {
            Texture2D m_areaTex = (Texture2D)g.GetType().GetField("m_areaTex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            g.GetType().GetField("m_areasUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, false);
            int num1 = 1;

            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
#if false
                int startX;
                int startZ;
                g.GetStartTile(out startX, out startZ);
                for (int y = 0; y <= 8; ++y)
                {
                    for (int x2 = 0; x2 <= 8; ++x2)
                    {
                        int num2 = x2 - num1;
                        int num3 = y - num1;
                        bool flag1 = num2 == startX && num3 == startZ;
                        bool flag2 = !flag1 && num2 >= 0 && (num3 >= 0 && num2 < 5) && num3 < 5;
                        Color color;
                        color.r = !flag1 ? 0.0f : 1f;
                        color.g = !flag2 ? 0.0f : 1f;
                        color.b = !flag2 || m_highlightAreaIndex != num3 * 5 + num2 ? 0.0f : 1f;
                        color.a = 1f;
                        m_areaTex.SetPixel(x2, y, color);
                    }
                }
#endif
            }
            else
            {
                for (int _z = 0; _z <= 8; ++_z)
                {
                    for (int _x = 0; _x <= 8; ++_x)
                    {
                        int x = _x - num1;
                        int z = _z - num1;
                        bool flag1 = x >= 0 && z >= 0 && x < DEFAULT_GRID && z < DEFAULT_GRID && g.IsUnlocked(x, z);
                        bool flag2 = x >= 0 && z >= 0 && x < DEFAULT_GRID && z < DEFAULT_GRID && g.CanUnlock(x, z);
                        Color color;
                        color.r = !flag1 ? 0.0f : 1f;
                        color.g = !flag2 ? 0.0f : 1f;
                        color.b = g.HighlightAreaIndex != (z + GRID_DIFF) * GRID + (x + GRID_DIFF) ? 0.0f : (!flag2 ? (!flag1 ? 0.0f : 0.5f) : 0.5f);
                        color.a = 1f;
                        m_areaTex.SetPixel(_x, _z, color);
                    }
                }
            }
            m_areaTex.Apply(false);
        }
    }
}
