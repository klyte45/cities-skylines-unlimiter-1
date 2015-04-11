using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using ICities;
using System.IO;
using System.Reflection;
using UnityEngine;
using EightyOne.Attributes;
using System.Linq;

namespace EightyOne.Areas
{
    public class FakeGameAreaManager : SerializableDataExtensionBase
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                int num = areaGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int i = 0; i < num; i++)
                {
                    @byte.Write((byte)areaGrid[i]);
                }
                @byte.EndWrite();
            }

            public void Deserialize(DataSerializer s)
            {
                areaGrid = new int[GRID * GRID];
                int num = areaGrid.Length;

                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                for (int i = 0; i < num; i++)
                {
                    areaGrid[i] = (int)@byte.Read();
                }
                @byte.EndRead();              
            }

            public void AfterDeserialize(DataSerializer s)
            {                
                typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameAreaManager.instance,true);                
            }
        }

        private const string id = "fakeGAM";

        public override void OnSaveData()
        {
            var currentCount = 0;
            for (var i = 0; i < 5; i += 1)
            {
                for (var j = 0; j < 5; j += 1)
                {
                    var grid = areaGrid[(j + 2) * GRID + (i + 2)];
                    GameAreaManager.instance.m_areaGrid[j * 5 + i] = grid;
                    if (grid != 0)
                    {
                        currentCount += 1;
                    }
                }
            }
            GameAreaManager.instance.m_areaCount = currentCount;            
            typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameAreaManager.instance, true);
            GameAreaManager.instance.m_maxAreaCount = 25;

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 1u, new Data());
                var data = ms.ToArray();
                serializableDataManager.SaveData(id, data);
            }
        }

        public override void OnLoadData()
        {
            if (!serializableDataManager.EnumerateData().Contains(id))
            {
                return;
            }
            var data = serializableDataManager.LoadData(id);
            using (var ms = new MemoryStream(data))
            {
                var s = DataSerializer.Deserialize<Data>(ms, DataSerializer.Mode.Memory);
            }
        }
        
        public const int GRID = 9;
        public const float HALFGRID = 4.5f;
        public const int TEXSIZE = 10;

        public static int[] areaGrid;
        private static Texture2D m_areaTex;
        private static FieldInfo m_areasUpdated;

        public static void Init()
        {
            GameAreaManager.instance.m_maxAreaCount = 82;
            if (areaGrid == null)
            {
                areaGrid = new int[GRID * GRID];
                for (var i = 0; i < 5; i += 1)
                {
                    for (var j = 0; j < 5; j += 1)
                    {
                        areaGrid[(j + 2) * GRID +  (i + 2)] = GameAreaManager.instance.m_areaGrid[j * 5 + i];
                    }
                }
                typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameAreaManager.instance, true);        
            }

            GameAreaManager.instance.m_areaCount = 0;
            for (int z2 = 0; z2 < GRID; ++z2)
            {
                for (int x2 = 0; x2 < GRID; ++x2)
                {
                    if (GameAreaManager.instance.GetArea(x2, z2) > 0)
                    {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2, z2);

                        float minX = (float)(((double)x2 - 4.5f) * 1920.0);
                        float maxX = (float)(((double)(x2 + 1) - 4.5f) * 1920.0);
                        float minZ = (float)(((double)z2 - 4.5f) * 1920.0);
                        float maxZ = (float)(((double)(z2 + 1) - 4.5f) * 1920.0);
                        Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                        GameAreaManager.instance.m_areaCount += 1;
                    }
                }
            }


            m_areaTex = new Texture2D(TEXSIZE, TEXSIZE, TextureFormat.ARGB32, false, true);
            m_areaTex.filterMode = FilterMode.Point;
            m_areaTex.wrapMode = TextureWrapMode.Clamp;

            m_areasUpdated = typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
            SimulationManager.instance.AddAction(() => GameObject.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => GameObject.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
        }

        public static void OnDestroy(){
            areaGrid = null;
            if (m_areaTex != null)
            {
                UnityEngine.Object.Destroy(m_areaTex);
                m_areaTex = null;
            }
        }

        public static void UnlockAll()
        {
            for (int i = 0; i < 9; i += 1)
            {
                for (int j = 0; j < 9; j += 1)
                {
                    GameAreaManager.instance.UnlockArea(j * GRID + i);
                }
            }
        }

        [ReplaceMethod]
        private void UpdateAreaMapping()
        {
            var gm = GameAreaManager.instance;
            var camController = (CameraController)typeof(GameAreaManager).GetField("m_cameraController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(gm);
            if (gm.AreasVisible)
            {
                this.UpdateAreaTexture();
            }
            if (camController != null)
            {
                if (gm.AreasVisible)
                {
                    Vector3 vector = camController.transform.TransformDirection(Vector3.right);
                    ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                    Bounds freeBounds;
                    if ((mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
                    {
                        freeBounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(9600f, 1024f, 9600f));
                    }
                    else
                    {
                        freeBounds = this.GetFreeBounds();
                    }
                    Vector3 center = freeBounds.center;
                    if (Mathf.Abs(vector.x) >= Mathf.Abs(vector.z))
                    {
                        if (vector.x > 0f)
                        {
                            center.z -= freeBounds.size.z * 0.02f + 50f;
                        }
                        else
                        {
                            center.z += freeBounds.size.z * 0.02f + 50f;
                        }
                        camController.SetOverrideModeOn(center, new Vector2((vector.x <= 0f) ? 180f : 0f, 80f), freeBounds.size.z);
                    }
                    else
                    {
                        if (vector.z > 0f)
                        {
                            center.x += freeBounds.size.x * 0.02f + 50f;
                        }
                        else
                        {
                            center.x -= freeBounds.size.x * 0.02f + 50f;
                        }
                        camController.SetOverrideModeOn(center, new Vector2((vector.z <= 0f) ? 90f : -90f, 80f), freeBounds.size.x);
                    }
                }
                else
                {
                    camController.SetOverrideModeOff();
                }
            }
        }

        [ReplaceMethod]
        private void UpdateAreaTexture()
        {
            var g = GameAreaManager.instance;
            m_areasUpdated.SetValue(g, false);

            for (int _z = 0; _z < TEXSIZE; ++_z)
            {
                for (int _x = 0; _x < TEXSIZE; ++_x)
                {
                    int num = 1;
                    var x = _x - num;
                    var z = _z - num;

                    bool flag1 = g.IsUnlocked(x,z);
                    bool flag2 = g.CanUnlock(x,z);
                    Color color;
                    color.r = !flag1 ? 0.0f : 1f;
                    color.g = !flag2 ? 0.0f : 1f;
                    if (g.HighlightAreaIndex == z * GRID + x)
                    {
                        if (flag1)
                        {
                            color.b = 0.5f;
                        }
                        else if (flag2)
                        {
                            color.b = 0.5f;
                        }
                        else
                        {
                            color.b = 0f;
                        }
                    }
                    else
                    {
                        color.b = 0f;
                    }
                    color.a = 1f;
                    m_areaTex.SetPixel(_x, _z, color);
                }
            }

            m_areaTex.Apply(false);
        }

        [ReplaceMethod]
        public Vector3 GetAreaPositionSmooth(int x, int z)
        {
            if (x < 0 || z < 0 || x >= GRID || z >= GRID)
            {
                return Vector3.zero;
            }
            Vector3 vector;
            vector.x = ((float)x - HALFGRID + 0.5f) * 1920f;
            vector.y = 0f;
            vector.z = ((float)z - HALFGRID + 0.5f) * 1920f;
            vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, true, 0f);
            return vector;
        }

        [ReplaceMethod]
        public bool CanUnlock(int x, int z)
        {
            var instance = GameAreaManager.instance;
            if (x < 0 || z < 0 || x >= GRID || z >= GRID)
            {
                return false;
            }
            if (areaGrid[z * GRID + x] != 0)
            {
                return false;
            }
            return true;
        }

        [ReplaceMethod]
        private Bounds GetFreeBounds()
        {
            Vector3 zero = Vector3.zero;
            Vector3 zero2 = Vector3.zero;
            for (int i = 0; i < GRID; i++)
            {
                for (int j = 0; j < GRID; j++)
                {
                    if (IsUnlocked(j, i))
                    {
                        zero.x = Mathf.Min(zero.x, ((float)(j - 1) - HALFGRID) * 1920f);
                        zero2.x = Mathf.Max(zero2.x, ((float)(j + 2) - HALFGRID) * 1920f);
                        zero.z = Mathf.Min(zero.z, ((float)(i - 1) - HALFGRID) * 1920f);
                        zero2.z = Mathf.Max(zero2.z, ((float)(i + 2) - HALFGRID) * 1920f);
                        zero2.y = Mathf.Max(zero2.y, 1024f);
                    }
                }
            }
            Bounds result = default(Bounds);
            result.SetMinMax(zero, zero2);
            return result;
        }

        [ReplaceMethod]
        public int GetArea(int x, int z)
        {
            if (x >= 0 && z >= 0 && x < GRID && z < GRID)
            {
                return areaGrid[z * GRID + x];
            }
            return -2;
        }

        [ReplaceMethod]
        public bool IsUnlocked(int x, int z)
        {
            return x >= 0 && z >= 0 && x < GRID && z < GRID && areaGrid[z * GRID + x] != 0;
        }

        [ReplaceMethod]
        public int GetAreaIndex(Vector3 p)
        {
            int num = Mathf.FloorToInt(p.x / 1920f + HALFGRID);
            int num2 = Mathf.FloorToInt(p.z / 1920f + HALFGRID);
            if (num < 0 || num2 < 0 || num >= GRID || num2 >= GRID)
            {
                return -1;
            }            
            return num2 * GRID + num;
        }

        [ReplaceMethod]
        public static void GetTileXZ(GameAreaManager gm,  int tile, out int x, out int z)
        {
            x = tile % GRID;
            z = tile / GRID;            
        }

        [ReplaceMethod]
        static int GetTileIndex(GameAreaManager g,int x, int z)
        {
            return z * GRID + x;
        }

        [ReplaceMethod]
        public bool PointOutOfArea(Vector3 p)
        {
            int x = Mathf.FloorToInt(p.x / 1920f + HALFGRID);
            int z = Mathf.FloorToInt(p.z / 1920f + HALFGRID);
                int area = GetArea(x, z);
                if (area == -2)
                {
                    return true;
                }
                return false;
        }

        [ReplaceMethod]
        public bool UnlockArea( int index)
        {
            var g = GameAreaManager.instance;
            g.GetType().GetField("m_unlocking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);
            try
            {
                int x = index % GRID;
                int z = index / GRID;
                if (g.CanUnlock(x, z))
                {
                    g.m_areaNotUnlocked.Deactivate();
                    areaGrid[index] = ++g.m_areaCount;
                    g.GetType().GetField("m_areasUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);

                    float minX = (float)(((double)x - 4.5f) * 1920.0);
                    float maxX = (float)(((double)(x + 1) - 4.5f) * 1920.0);
                    float minZ = (float)(((double)z - 4.5f) * 1920.0);
                    float maxZ = (float)(((double)(z + 1) - 4.5f) * 1920.0);
                    Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                    
                    if (Singleton<TerrainManager>.instance.SetDetailedPatch(x, z))
                    {
                        Singleton<MessageManager>.instance.TryCreateMessage(g.m_properties.m_unlockMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
                        g.m_AreasWrapper.OnUnlockArea(x, z);
                        return true;

                    }
                    Debug.Log("Failde to unlock: " + x.ToString() + " " + z.ToString());
                    --g.m_areaCount;
                    areaGrid[index] = 0;

                    g.GetType().GetField("m_areasUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, true);
                }
                return false;
            }
            finally
            {
                g.GetType().GetField("m_unlocking", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, false);
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
        public void UpdateData(SimulationManager.UpdateMode mode)
        {
            var g = GameAreaManager.instance;
            if (mode == SimulationManager.UpdateMode.NewGame || mode == SimulationManager.UpdateMode.NewAsset || mode == SimulationManager.UpdateMode.LoadAsset)
            {
                g.m_areaCount = 0;
                int num = (int)g.GetType().GetField("m_startTile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(g);
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

                g.GetType().GetField("m_buildableArea0", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea0);
                g.GetType().GetField("m_buildableArea1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea1);
                g.GetType().GetField("m_buildableArea2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea2);
                g.GetType().GetField("m_buildableArea3", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(g, m_buildableArea3);
            }

            for (int z2 = 0; z2 < GRID; ++z2)
            {
                for (int x2 = 0; x2 < GRID; ++x2)
                {
                    if (g.GetArea(x2, z2) > 0)
                    {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(x2, z2);

                        float minX = (float)(((double)x2 - 4.5f) * 1920.0);
                        float maxX = (float)(((double)(x2 + 1) - 4.5f) * 1920.0);
                        float minZ = (float)(((double)z2 - 4.5f) * 1920.0);
                        float maxZ = (float)(((double)(z2 + 1) - 4.5f) * 1920.0);
                        Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);
                    }
                }
            }
            if (mode == SimulationManager.UpdateMode.NewGame || g.m_areaNotUnlocked == null)
                g.m_areaNotUnlocked = new GenericGuide();
        }

        [ReplaceMethod]
        protected void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            var g = GameAreaManager.instance;
            float m_borderAlpha = (float)g.GetType().GetField("m_borderAlpha", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            float m_areaAlpha = (float)g.GetType().GetField("m_areaAlpha", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_borderMaterial = (Material)g.GetType().GetField("m_borderMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_areaMaterial = (Material)g.GetType().GetField("m_areaMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Material m_decorationMaterial = (Material)g.GetType().GetField("m_decorationMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            Mesh m_borderMesh = (Mesh)g.GetType().GetField("m_borderMesh", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);

            int ID_Color = (int)g.GetType().GetField("ID_Color", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);
            int ID_AreaMapping = (int)g.GetType().GetField("ID_AreaMapping", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(g);


            if ((double)m_borderAlpha >= 1.0 / 1000.0 && (UnityEngine.Object)m_borderMaterial != (UnityEngine.Object)null)
            {
                Quaternion rotation = Quaternion.AngleAxis(90f, Vector3.up);
                Color color = Color.white;
                ToolController toolController = Singleton<ToolManager>.instance.m_properties;
                if ((UnityEngine.Object)toolController != (UnityEngine.Object)null && (toolController.CurrentTool.GetErrors() & ToolBase.ToolErrors.OutOfArea) != ToolBase.ToolErrors.None)
                    color = Color.red;
                color.a = m_borderAlpha;
                for (int z = 0; z < GRID; z +=1)
                {
                    for (int x = 0; x < GRID; x +=1)
                    {
                        bool flag1 = g.GetArea(x, z) > 0;
                        bool flag2 = g.GetArea(x, z - 1) > 0;
                        bool flag3 = g.GetArea(x - 1, z) > 0;
                        if (flag1 != flag2)
                        {
                            Vector3 vector3 = new Vector3((float)(((double)x - 4.5 + 0.5) * 1920.0), 0.0f, (float)(((double)z - 4.5) * 1920.0));
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
                            Vector3 vector3 = new Vector3((float)(((double)x - 4.5) * 1920.0), 0.0f, (float)(((double)z - 4.5 + 0.5) * 1920.0));
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
            vector.z = 1 / (float)(1920f * TEXSIZE); //6.510417E-05f;
            vector.x = (GRID + 2) / (float)(TEXSIZE * 2); // 0.5f;
            vector.y = (GRID + 2)/ (float)(TEXSIZE * 2); // 0.5f;
            vector.w = 0.125f;
            m_areaMaterial.mainTexture = (Texture)m_areaTex;
            m_areaMaterial.SetColor(ID_Color, new Color(1f, 1f, 1f, m_areaAlpha));
            m_areaMaterial.SetVector(ID_AreaMapping, vector);
            Bounds freeBounds = GetFreeBounds();
            freeBounds.size = freeBounds.size + new Vector3(100f, 1f, 100f);
            ++Singleton<GameAreaManager>.instance.m_drawCallData.m_overlayCalls;
            Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, m_areaMaterial, 0, freeBounds);
        }


        [ReplaceMethod]
        public bool ClampPoint(ref Vector3 position)
        {            
                int num7 = Mathf.FloorToInt(position.x / 1920f + 4.5f);
                int num8 = Mathf.FloorToInt(position.z / 1920f + 4.5f);
                if (GetArea(num7, num8) > 0)
                {
                    return true;
                }
                Rect rect = default(Rect);
                rect.xMin = -8640;
                rect.yMin = -8640;
                rect.xMax = 8640;
                rect.yMax = 8640;
                float num9 = 1000000f;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (GetArea(num7 + j, num8 + i) > 0)
                        {
                            Rect rect2 = default(Rect);
                            rect2.xMin = ((float)(num7 + j) - 4.5f) * 1920f;
                            rect2.yMin = ((float)(num8 + i) - 4.5f) * 1920f;
                            rect2.xMax = rect2.xMin + 1920f;
                            rect2.yMax = rect2.yMin + 1920f;
                            float num10 = Mathf.Max(Mathf.Max(position.x - rect2.xMax, rect2.xMin - position.x), Mathf.Max(position.z - rect2.yMax, rect2.yMin - position.z));
                            if (num10 < num9)
                            {
                                rect = rect2;
                                num9 = num10;
                            }
                        }
                    }
                }
                if (position.x < rect.xMin)
                {
                    position.x = rect.xMin;
                }
                if (position.x > rect.xMax)
                {
                    position.x = rect.xMax;
                }
                if (position.z < rect.yMin)
                {
                    position.z = rect.yMin;
                }
                if (position.z > rect.yMax)
                {
                    position.z = rect.yMax;
                }
                return num9 != 1000000f;
            
        }

        [ReplaceMethod]
        public bool QuadOutOfArea(Quad2 quad)
        {
                Vector2 vector = quad.Min();
                Vector2 vector2 = quad.Max();
                int num6 = Mathf.FloorToInt((vector.x - 8f) / 1920f + 4.5f);
                int num7 = Mathf.FloorToInt((vector.y - 8f) / 1920f + 4.5f);
                int num8 = Mathf.FloorToInt((vector2.x + 8f) / 1920f + 4.5f);
                int num9 = Mathf.FloorToInt((vector2.y + 8f) / 1920f + 4.5f);
                for (int i = num7; i <= num9; i++)
                {
                    for (int j = num6; j <= num8; j++)
                    {
                        int area = GetArea(j, i);
                        if ((area == -2 ) && quad.Intersect(new Quad2
                        {
                            a = new Vector2(((float)j - 4.5f) * 1920f - 8f, ((float)i - 4.5f) * 1920f - 8f),
                            b = new Vector2(((float)j - 4.5f) * 1920f - 8f, ((float)i - 4.5f + 1f) * 1920f + 8f),
                            c = new Vector2(((float)j - 4.5f + 1f) * 1920f + 8f, ((float)i - 4.5f + 1f) * 1920f + 8f),
                            d = new Vector2(((float)j - 4.5f + 1f) * 1920f + 8f, ((float)i - 4.5f) * 1920f - 8f)
                        }))
                        {
                            return true;
                        }
                    }
                }            
            return false;
        }


    }
}
