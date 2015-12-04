using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(DistrictManager))]
    public class FakeDistrictManager : SerializableDataExtensionBase
    {
        public class Data : IDataContainer
        {

            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
                DistrictManager instance = Singleton<DistrictManager>.instance;
                District[] buffer = instance.m_districts.m_buffer;
                DistrictManager.Cell[] districtGrid = FakeDistrictManager.m_districtGrid;
                int num = districtGrid.Length;
                for (int i = 0; i < num; i++)
                {
                    DistrictManager.Cell cell = districtGrid[i];
                    District[] expr_60_cp_0 = buffer;
                    byte expr_60_cp_1 = cell.m_district1;
                    expr_60_cp_0[(int)expr_60_cp_1].m_totalAlpha = expr_60_cp_0[(int)expr_60_cp_1].m_totalAlpha + (uint)cell.m_alpha1;
                    District[] expr_80_cp_0 = buffer;
                    byte expr_80_cp_1 = cell.m_district2;
                    expr_80_cp_0[(int)expr_80_cp_1].m_totalAlpha = expr_80_cp_0[(int)expr_80_cp_1].m_totalAlpha + (uint)cell.m_alpha2;
                    District[] expr_A0_cp_0 = buffer;
                    byte expr_A0_cp_1 = cell.m_district3;
                    expr_A0_cp_0[(int)expr_A0_cp_1].m_totalAlpha = expr_A0_cp_0[(int)expr_A0_cp_1].m_totalAlpha + (uint)cell.m_alpha3;
                    District[] expr_C0_cp_0 = buffer;
                    byte expr_C0_cp_1 = cell.m_district4;
                    expr_C0_cp_0[(int)expr_C0_cp_1].m_totalAlpha = expr_C0_cp_0[(int)expr_C0_cp_1].m_totalAlpha + (uint)cell.m_alpha4;
                }
                instance.m_districtCount = (int)(instance.m_districts.ItemCount() - 1u);
                instance.AreaModified(0, 0, 511, 511, true);
                instance.NamesModified();
            }

            public void Deserialize(DataSerializer s)
            {
                var districtGrid = new DistrictManager.Cell[GRID * GRID];
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                int num2 = districtGrid.Length;
                for (int num21 = 0; num21 < num2; num21++)
                {
                    districtGrid[num21].m_district1 = @byte.Read();
                }
                for (int num22 = 0; num22 < num2; num22++)
                {
                    districtGrid[num22].m_district2 = @byte.Read();
                }
                for (int num23 = 0; num23 < num2; num23++)
                {
                    districtGrid[num23].m_district3 = @byte.Read();
                }
                for (int num24 = 0; num24 < num2; num24++)
                {
                    districtGrid[num24].m_district4 = @byte.Read();
                }
                for (int num25 = 0; num25 < num2; num25++)
                {
                    districtGrid[num25].m_alpha1 = @byte.Read();
                }
                for (int num26 = 0; num26 < num2; num26++)
                {
                    districtGrid[num26].m_alpha2 = @byte.Read();
                }
                for (int num27 = 0; num27 < num2; num27++)
                {
                    districtGrid[num27].m_alpha3 = @byte.Read();
                }
                for (int num28 = 0; num28 < num2; num28++)
                {
                    districtGrid[num28].m_alpha4 = @byte.Read();
                }
                @byte.EndRead();

                FakeDistrictManager.m_districtGrid = districtGrid;
            }

            public void Serialize(DataSerializer s)
            {
                var districtGrid = FakeDistrictManager.m_districtGrid;
                int num2 = districtGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int num19 = 0; num19 < num2; num19++)
                {
                    @byte.Write(districtGrid[num19].m_district1);
                }
                for (int num20 = 0; num20 < num2; num20++)
                {
                    @byte.Write(districtGrid[num20].m_district2);
                }
                for (int num21 = 0; num21 < num2; num21++)
                {
                    @byte.Write(districtGrid[num21].m_district3);
                }
                for (int num22 = 0; num22 < num2; num22++)
                {
                    @byte.Write(districtGrid[num22].m_district4);
                }
                for (int num23 = 0; num23 < num2; num23++)
                {
                    @byte.Write(districtGrid[num23].m_alpha1);
                }
                for (int num24 = 0; num24 < num2; num24++)
                {
                    @byte.Write(districtGrid[num24].m_alpha2);
                }
                for (int num25 = 0; num25 < num2; num25++)
                {
                    @byte.Write(districtGrid[num25].m_alpha3);
                }
                for (int num26 = 0; num26 < num2; num26++)
                {
                    @byte.Write(districtGrid[num26].m_alpha4);
                }
                @byte.EndWrite();                
            }
        }

        private const string id = "fakeDM";

        public override void OnSaveData()
        {
            var oldGrid = DistrictManager.instance.m_districtGrid;
            int oldGridSize = 512;
            int diff = (GRID - oldGridSize) / 2;
            for (var i = 0; i < oldGridSize; i += 1)
            {
                for (var j = 0; j < oldGridSize; j += 1)
                {
                    oldGrid[j * oldGridSize + i] = m_districtGrid[(j + diff) * GRID + (i + diff)];
                }
            }

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory,1u,new Data());
                var data = ms.ToArray();
                serializableDataManager.SaveData(id, data);   
            }
        }

        public override void OnLoadData()
        {
            if (!serializableDataManager.EnumerateData().Contains(id)){
                return;
            }
            var data = serializableDataManager.LoadData(id);
            using (var ms = new MemoryStream(data)){
                var s = DataSerializer.Deserialize<Data>(ms,DataSerializer.Mode.Memory);
           }
        }

        public static int GRID = 900;
        public static int HALFGRID = 450;
        public static DistrictManager.Cell[] m_districtGrid;
        private static Color32[] m_colorBuffer;
        private static int[] m_distanceBuffer;
        private static int[] m_indexBuffer;

        private static Texture2D m_districtTexture1;
        private static Texture2D m_districtTexture2;

        private static FieldInfo m_modifiedX1;
        private static FieldInfo m_modifiedZ1;
        private static FieldInfo m_modifiedX2;
        private static FieldInfo m_modifiedZ2;
        private static FieldInfo m_fullUpdate;
        private static FieldInfo m_modifyLock;
        private static FieldInfo m_namesModified;
        private static FieldInfo m_areaMaterial;

        private static int ID_Districts1;
        private static int ID_Districts2;
        private static int ID_DistrictMapping;
        private static int ID_Highlight1;
        private static int ID_Highlight2;

        private static TempDistrictData[] m_tempData;

        private struct TempDistrictData
        {
            public int m_averageX;
            public int m_averageZ;
            public int m_bestScore;
            public int m_divider;
            public int m_bestLocation;
        }

        public static void OnDestroy()
        {
            if (m_districtTexture1 != null)
            {
                UnityEngine.Object.Destroy(m_districtTexture1);
                m_districtTexture1 = null;
            }
            if (m_districtTexture2 != null)
            {
                UnityEngine.Object.Destroy(m_districtTexture2);
                m_districtTexture2 = null;
            }
            m_districtGrid = null;
        }

        public static void Init()
        {
            if (m_districtGrid == null)
            {
                m_districtGrid = new DistrictManager.Cell[GRID * GRID];
                for (int i = 0; i < m_districtGrid.Length; i++)
                {
                    m_districtGrid[i].m_district1 = 0;
                    m_districtGrid[i].m_district2 = 1;
                    m_districtGrid[i].m_district3 = 2;
                    m_districtGrid[i].m_district4 = 3;
                    m_districtGrid[i].m_alpha1 = 255;
                    m_districtGrid[i].m_alpha2 = 0;
                    m_districtGrid[i].m_alpha3 = 0;
                    m_districtGrid[i].m_alpha4 = 0;
                }

                var oldGrid = DistrictManager.instance.m_districtGrid;
                int oldGridSize = 512;
                int diff = (GRID - oldGridSize) / 2;
                for (var i = 0; i < oldGridSize; i += 1)
                {
                    for (var j = 0; j < oldGridSize; j += 1)
                    {
                        m_districtGrid[(j + diff) * GRID + (i + diff)] = oldGrid[j * oldGridSize + i];
                    }
                }                
            }
            m_colorBuffer = new Color32[GRID * GRID];
            m_distanceBuffer = new int[HALFGRID * HALFGRID];
            m_indexBuffer = new int[HALFGRID * HALFGRID];
            m_tempData = new TempDistrictData[128];

            var dm = typeof(DistrictManager);
            m_modifiedX1 = dm.GetField("m_modifiedX1",BindingFlags.Instance| BindingFlags.NonPublic);
            m_modifiedZ1 = dm.GetField("m_modifiedZ1", BindingFlags.Instance | BindingFlags.NonPublic);
            m_modifiedX2 = dm.GetField("m_modifiedX2", BindingFlags.Instance | BindingFlags.NonPublic);
            m_modifiedZ2 = dm.GetField("m_modifiedZ2", BindingFlags.Instance | BindingFlags.NonPublic);
            m_fullUpdate = dm.GetField("m_fullUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            m_modifyLock = dm.GetField("m_modifyLock", BindingFlags.Instance | BindingFlags.NonPublic);
            m_namesModified = dm.GetField("m_namesModified", BindingFlags.Instance | BindingFlags.NonPublic);
            m_areaMaterial = dm.GetField("m_areaMaterial", BindingFlags.Instance | BindingFlags.NonPublic);

            m_districtTexture1 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            m_districtTexture2 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            m_districtTexture1.wrapMode = TextureWrapMode.Clamp;
            m_districtTexture2.wrapMode = TextureWrapMode.Clamp;
            ID_Districts1 = Shader.PropertyToID("_Districts1");
            ID_Districts2 = Shader.PropertyToID("_Districts2");
            ID_DistrictMapping = Shader.PropertyToID("_DistrictMapping");
            ID_Highlight1 = Shader.PropertyToID("_Highlight1");
            ID_Highlight2 = Shader.PropertyToID("_Highlight2");
        }

        [ReplaceMethod]
        protected void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            var dm = DistrictManager.instance;
            var areaMaterial = (Material)m_areaMaterial.GetValue(dm);
            if ((dm.DistrictsVisible || dm.DistrictsInfoVisible) && areaMaterial != null)
            {
                areaMaterial.SetTexture(ID_Districts1, m_districtTexture1);
                areaMaterial.SetTexture(ID_Districts2, m_districtTexture2);
                Vector4 vector;
                vector.z = 1 / (19.2f * GRID);
                vector.x = 0.5f;
                vector.y = 0.5f;
                vector.w = ((dm.HighlightDistrict <= 0) ? 0f : 1f);
                areaMaterial.SetVector(ID_DistrictMapping, vector);
                Color32 c = new Color32(128, 128, 128, 128);
                Color32 c2 = new Color32(128, 128, 128, 128);
                AddDistrictColor1((byte)Mathf.Max(0, dm.HighlightDistrict), 255, ref c);
                AddDistrictColor2((byte)Mathf.Max(0, dm.HighlightDistrict), DistrictPolicies.Policies.None, 255, true, ref c2);
                areaMaterial.SetColor(ID_Highlight1, c);
                areaMaterial.SetColor(ID_Highlight2, c2);

                if (dm.HighlightPolicy != DistrictPolicies.Policies.None)
                {
                    areaMaterial.EnableKeyword("POLICYTOOL_ON");
                }
                else
                {
                    areaMaterial.DisableKeyword("POLICYTOOL_ON");
                }
                Vector3 center = new Vector3(0f, 512f, 0f);
                Vector3 a = new Vector3(19.2f * GRID, 1024f, 19.2f * GRID);
                Bounds bounds = new Bounds(center, a + Vector3.one);
                DistrictManager expr_1C2_cp_0 = Singleton<DistrictManager>.instance;
                expr_1C2_cp_0.m_drawCallData.m_overlayCalls = expr_1C2_cp_0.m_drawCallData.m_overlayCalls + 1;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, areaMaterial, 0, bounds);
            }
        }

        [ReplaceMethod]
        private void UpdateTexture()
        {            
            var dm = DistrictManager.instance;
            var modLock = m_modifyLock.GetValue(dm);
            while (!Monitor.TryEnter(modLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            int num;
            int num2;
            int num3;
            int num4;
            bool fullUpdate;
            try
            {
                num = (int)m_modifiedX1.GetValue(dm);
                num2 = (int)m_modifiedZ1.GetValue(dm);
                num3 = (int)m_modifiedX2.GetValue(dm);
                num4 = (int)m_modifiedZ2.GetValue(dm);
                fullUpdate = (bool)m_fullUpdate.GetValue(dm);
                m_modifiedX1.SetValue(dm,10000);
                m_modifiedZ1.SetValue(dm, 10000);


                m_modifiedX2.SetValue(dm, -10000);
                m_modifiedZ2.SetValue(dm, -10000);
                m_fullUpdate.SetValue(dm, false);
            }
            finally
            {
                Monitor.Exit(modLock);
            }
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            int num5 = Mathf.RoundToInt(99.99999f);
            int num6 = (5 * num5 >> 1) - HALFGRID;
            if ((num3 - num + 1) * (num4 - num2 + 1) > 65536)
            {
                num = 1;
                num2 = 1;
                num3 = GRID -2;
                num4 = GRID -2;
                if (fullUpdate)
                {
                    for (int i = num2; i <= num4; i++)
                    {
                        for (int j = num; j <= num3; j++)
                        {
                            int num7 = i * GRID + j;
                            DistrictManager.Cell cell = m_districtGrid[num7];
                            Color32 color = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(cell.m_district1, cell.m_alpha1, ref color);
                            AddDistrictColor1(cell.m_district2, cell.m_alpha2, ref color);
                            AddDistrictColor1(cell.m_district3, cell.m_alpha3, ref color);
                            AddDistrictColor1(cell.m_district4, cell.m_alpha4, ref color);
                            m_colorBuffer[num7] = color;
                        }
                    }
                    m_districtTexture1.SetPixels32(m_colorBuffer);
                    m_districtTexture1.Apply();
                }
                for (int k = num2; k <= num4; k++)
                {
                    for (int l = num; l <= num3; l++)
                    {
                        int num8 = k * GRID + l;
                        DistrictManager.Cell cell2 = m_districtGrid[num8];
                        bool inArea = false;
                        int num9 = (l + num6) / num5;
                        int num10 = (k + num6) / num5;
                        if (num9 >= 0 && num9 < 5 && num10 >= 0 && num10 < 5)
                        {
                            inArea = (areaGrid[num10 * 5 + num9] != 0);
                        }
                        Color32 color2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(cell2.m_district1, dm.HighlightPolicy, cell2.m_alpha1, inArea, ref color2);
                        AddDistrictColor2(cell2.m_district2, dm.HighlightPolicy, cell2.m_alpha2, inArea, ref color2);
                        AddDistrictColor2(cell2.m_district3, dm.HighlightPolicy, cell2.m_alpha3, inArea, ref color2);
                        AddDistrictColor2(cell2.m_district4, dm.HighlightPolicy, cell2.m_alpha4, inArea, ref color2);
                        m_colorBuffer[num8] = color2;
                    }
                }
                m_districtTexture2.SetPixels32(m_colorBuffer);
                m_districtTexture2.Apply();
            }
            else
            {
                num = Mathf.Max(1, num);
                num2 = Mathf.Max(1, num2);
                num3 = Mathf.Min(GRID -2, num3);
                num4 = Mathf.Min(GRID -2, num4);
                for (int m = num2; m <= num4; m++)
                {
                    for (int n = num; n <= num3; n++)
                    {
                        int num11 = m * GRID + n;
                        DistrictManager.Cell cell3 = m_districtGrid[num11];
                        if (fullUpdate)
                        {
                            Color32 c = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(cell3.m_district1, cell3.m_alpha1, ref c);
                            AddDistrictColor1(cell3.m_district2, cell3.m_alpha2, ref c);
                            AddDistrictColor1(cell3.m_district3, cell3.m_alpha3, ref c);
                            AddDistrictColor1(cell3.m_district4, cell3.m_alpha4, ref c);
                            m_districtTexture1.SetPixel(n, m, c);
                        }
                        bool inArea2 = false;
                        int num12 = (n + num6) / num5;
                        int num13 = (m + num6) / num5;
                        if (num12 >= 0 && num12 < 5 && num13 >= 0 && num13 < 5)
                        {
                            inArea2 = (areaGrid[num13 * 5 + num12] != 0);
                        }
                        Color32 c2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(cell3.m_district1, dm.HighlightPolicy, cell3.m_alpha1, inArea2, ref c2);
                        AddDistrictColor2(cell3.m_district2, dm.HighlightPolicy, cell3.m_alpha2, inArea2, ref c2);
                        AddDistrictColor2(cell3.m_district3, dm.HighlightPolicy, cell3.m_alpha3, inArea2, ref c2);
                        AddDistrictColor2(cell3.m_district4, dm.HighlightPolicy, cell3.m_alpha4, inArea2, ref c2);
                        m_districtTexture2.SetPixel(n, m, c2);
                    }
                }
                if (fullUpdate)
                {
                    m_districtTexture1.Apply();
                }
                m_districtTexture2.Apply();
            }
        }
        private void AddDistrictColor1(byte district, byte alpha, ref Color32 color1)
        {
            if ((district & 1) != 0)
            {
                color1.r = (byte)Mathf.Max((int)color1.r, (int)alpha);
            }
            else
            {
                color1.r = (byte)Mathf.Min((int)color1.r, (int)(255 - alpha));
            }
            if ((district & 2) != 0)
            {
                color1.g = (byte)Mathf.Max((int)color1.g, (int)alpha);
            }
            else
            {
                color1.g = (byte)Mathf.Min((int)color1.g, (int)(255 - alpha));
            }
            if ((district & 4) != 0)
            {
                color1.b = (byte)Mathf.Max((int)color1.b, (int)alpha);
            }
            else
            {
                color1.b = (byte)Mathf.Min((int)color1.b, (int)(255 - alpha));
            }
            if ((district & 8) != 0)
            {
                color1.a = (byte)Mathf.Max((int)color1.a, (int)alpha);
            }
            else
            {
                color1.a = (byte)Mathf.Min((int)color1.a, (int)(255 - alpha));
            }
        }
        private void AddDistrictColor2(byte district, DistrictPolicies.Policies policy, byte alpha, bool inArea, ref Color32 color2)
        {
            if ((district & 16) != 0)
            {
                color2.r = (byte)Mathf.Max((int)color2.r, (int)alpha);
            }
            else
            {
                color2.r = (byte)Mathf.Min((int)color2.r, (int)(255 - alpha));
            }
            if ((district & 32) != 0)
            {
                color2.g = (byte)Mathf.Max((int)color2.g, (int)alpha);
            }
            else
            {
                color2.g = (byte)Mathf.Min((int)color2.g, (int)(255 - alpha));
            }
            if ((district & 64) != 0)
            {
                color2.b = (byte)Mathf.Max((int)color2.b, (int)alpha);
            }
            else
            {
                color2.b = (byte)Mathf.Min((int)color2.b, (int)(255 - alpha));
            }
            if (policy != DistrictPolicies.Policies.None)
            {
                if (DistrictManager.instance.m_districts.m_buffer[(int)district].IsPolicySet(policy) && (inArea || district != 0))
                {
                    color2.a = (byte)Mathf.Max((int)color2.a, (int)alpha);
                }
                else
                {
                    color2.a = (byte)Mathf.Min((int)color2.a, (int)(255 - alpha));
                }
            }
            else
            {
                color2.a = (byte)Mathf.Min((int)color2.a, (int)(255 - alpha));
            }
        }

        [ReplaceMethod]
        public void NamesModified()
        {
            var dm = DistrictManager.instance;
            int num = m_distanceBuffer.Length;
            for (int i = 0; i < num; i++)
            {
                m_distanceBuffer[i] = 0;
            }
            for (int j = 0; j < 128; j++)
            {
                m_tempData[j] = default(TempDistrictData);
            }
            int num2 = 2;
            int num3 = GRID * 2;
            int num4 = 0;
            int num5 = 0;
            for (int k = 0; k < HALFGRID; k++)
            {
                for (int l = 0; l < HALFGRID; l++)
                {
                    int num6 = k * num3 + l * num2;
                    byte district = m_districtGrid[num6].m_district1;
                    if (district != 0 && (l == 0 || k == 0 || l == HALFGRID-1 || k == HALFGRID-1 || m_districtGrid[num6 - num3].m_district1 != district || m_districtGrid[num6 - num2].m_district1 != district || m_districtGrid[num6 + num2].m_district1 != district || m_districtGrid[num6 + num3].m_district1 != district))
                    {
                        int num7 = k * HALFGRID + l;
                        m_distanceBuffer[num7] = 1;
                        m_indexBuffer[num5] = num7;
                        num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                        m_tempData[(int)district].m_averageX = m_tempData[(int)district].m_averageX + l;                        
                        m_tempData[(int)district].m_averageZ = m_tempData[(int)district].m_averageZ + k;                        
                        m_tempData[(int)district].m_divider = m_tempData[(int)district].m_divider + 1;
                    }
                }
            }
            for (int m = 0; m < 128; m++)
            {
                int divider = m_tempData[m].m_divider;
                if (divider != 0)
                {
                    m_tempData[m].m_averageX = (m_tempData[m].m_averageX + divider / 2 ) / divider;
                    m_tempData[m].m_averageZ = (m_tempData[m].m_averageZ + divider / 2 ) / divider;
                }
            }
            while (num4 != num5)
            {
                int num8 = m_indexBuffer[num4];
                num4 = ((num4 + 1) % (HALFGRID * HALFGRID));
                int num9 = num8 % HALFGRID;
                int num10 = num8 / HALFGRID;
                int num11 = num10 * num3 + num9 * num2;
                byte district2 = m_districtGrid[num11].m_district1;
                int num12 = num9 - m_tempData[(int)district2].m_averageX;
                int num13 = num10 - m_tempData[(int)district2].m_averageZ;
                int num14 = (GRID * GRID) - (GRID * GRID/ 2) / m_distanceBuffer[num8] - num12 * num12 - num13 * num13;
                if (num14 > m_tempData[(int)district2].m_bestScore)
                {
                    m_tempData[(int)district2].m_bestScore = num14;
                    m_tempData[(int)district2].m_bestLocation = num8;
                }
                int num15 = num8 - 1;
                if (num9 > 0 && m_distanceBuffer[num15] == 0 && m_districtGrid[num11 - num2].m_district1 == district2)
                {
                    m_distanceBuffer[num15] = (m_distanceBuffer[num8] + 1);
                    m_indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 + 1;
                if (num9 < HALFGRID-1 && m_distanceBuffer[num15] == 0 && m_districtGrid[num11 + num2].m_district1 == district2)
                {
                    m_distanceBuffer[num15] = (m_distanceBuffer[num8] + 1);
                    m_indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 - HALFGRID;
                if (num10 > 0 && m_distanceBuffer[num15] == 0 && m_districtGrid[num11 - num3].m_district1 == district2)
                {
                    m_distanceBuffer[num15] = (m_distanceBuffer[num8] + 1);
                    m_indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 + HALFGRID;
                if (num10 < HALFGRID-1 && m_distanceBuffer[num15] == 0 && m_districtGrid[num11 + num3].m_district1 == district2)
                {
                    m_distanceBuffer[num15] = (m_distanceBuffer[num8] + 1);
                    m_indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
            }
            for (int n = 0; n < 128; n++)
            {
                int bestLocation = m_tempData[n].m_bestLocation;
                Vector3 vector;
                vector.x = 19.2f * (float)(bestLocation % HALFGRID) * 2f - 19.2f * HALFGRID;
                vector.y = 0f;
                vector.z = 19.2f * (float)(bestLocation / HALFGRID) * 2f - 19.2f * HALFGRID;
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, false, 0f);
                dm.m_districts.m_buffer[n].m_nameLocation = vector;
            }
            m_namesModified.SetValue(dm,true);
        }

        [ReplaceMethod]
        public void GetDistrictArea(byte district, out int minX, out int minZ, out int maxX, out int maxZ)
        {
            minX = 10000;
            minZ = 10000;
            maxX = -10000;
            maxZ = -10000;
            for (int i = 0; i < GRID; i++)
            {
                for (int j = 0; j < GRID; j++)
                {
                    int num = i * GRID + j;
                    DistrictManager.Cell cell = m_districtGrid[num];
                    if (cell.m_alpha1 != 0 && cell.m_district1 == district)
                    {
                        if (j < minX)
                        {
                            minX = j;
                        }
                        if (i < minZ)
                        {
                            minZ = i;
                        }
                        if (j > maxX)
                        {
                            maxX = j;
                        }
                        if (i > maxZ)
                        {
                            maxZ = i;
                        }
                    }
                    else if (cell.m_alpha2 != 0 && cell.m_district2 == district)
                    {
                        if (j < minX)
                        {
                            minX = j;
                        }
                        if (i < minZ)
                        {
                            minZ = i;
                        }
                        if (j > maxX)
                        {
                            maxX = j;
                        }
                        if (i > maxZ)
                        {
                            maxZ = i;
                        }
                    }
                    else if (cell.m_alpha3 != 0 && cell.m_district3 == district)
                    {
                        if (j < minX)
                        {
                            minX = j;
                        }
                        if (i < minZ)
                        {
                            minZ = i;
                        }
                        if (j > maxX)
                        {
                            maxX = j;
                        }
                        if (i > maxZ)
                        {
                            maxZ = i;
                        }
                    }
                    else if (cell.m_alpha4 != 0 && cell.m_district4 == district)
                    {
                        if (j < minX)
                        {
                            minX = j;
                        }
                        if (i < minZ)
                        {
                            minZ = i;
                        }
                        if (j > maxX)
                        {
                            maxX = j;
                        }
                        if (i > maxZ)
                        {
                            maxZ = i;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public byte GetDistrict(int x, int z)
        {
            int num = z * GRID + x;
            return m_districtGrid[num].m_district1;
        }

        [ReplaceMethod]
        public byte GetDistrict(Vector3 worldPos)
        {
            int num = Mathf.Clamp((int)(worldPos.x / 19.2f + HALFGRID), 0, GRID -1);
            int num2 = Mathf.Clamp((int)(worldPos.z / 19.2f + HALFGRID), 0, GRID -1);
            int num3 = num2 * GRID + num;
            return m_districtGrid[num3].m_district1;
        }

        [ReplaceMethod]
        public byte SampleDistrict(Vector3 worldPos)
        {
            int num = Mathf.RoundToInt(worldPos.x * 13.333333f + (HALFGRID * HALFGRID) - HALFGRID);
            int num2 = Mathf.RoundToInt(worldPos.z * 13.333333f + (HALFGRID * HALFGRID) - HALFGRID);

            int num3 = Mathf.Clamp((int)(worldPos.x / 19.2f + HALFGRID), 0, GRID - 1);
            int num4 = Mathf.Clamp((int)(worldPos.z / 19.2f + HALFGRID), 0, GRID - 1);
            int num5 = Mathf.Min(num3 + 1, GRID -1);
            int num6 = Mathf.Min(num4 + 1, GRID -1);
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            SetBitAlphas(m_districtGrid[num4 * GRID + num3], (255 - (num & 255)) * (255 - (num2 & 255)), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(m_districtGrid[num4 * GRID + num5], (num & 255) * (255 - (num2 & 255)), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(m_districtGrid[num6 * GRID + num3], (255 - (num & 255)) * (num2 & 255), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(m_districtGrid[num6 * GRID + num5], (num & 255) * (num2 & 255), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            byte b = 0;
            if (num7 > 0)
            {
                b |= 1;
            }
            if (num8 > 0)
            {
                b |= 2;
            }
            if (num9 > 0)
            {
                b |= 4;
            }
            if (num10 > 0)
            {
                b |= 8;
            }
            if (num11 > 0)
            {
                b |= 16;
            }
            if (num12 > 0)
            {
                b |= 32;
            }
            if (num13 > 0)
            {
                b |= 64;
            }
            return b;
        }

        private void SetBitAlphas(DistrictManager.Cell cell, int alpha, ref int b1, ref int b2, ref int b3, ref int b4, ref int b5, ref int b6, ref int b7)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            SetBitAlphas((int)cell.m_district1, (int)cell.m_alpha1, ref num, ref num2, ref num3, ref num4, ref num5, ref num6, ref num7);
            SetBitAlphas((int)cell.m_district2, (int)cell.m_alpha2, ref num, ref num2, ref num3, ref num4, ref num5, ref num6, ref num7);
            SetBitAlphas((int)cell.m_district3, (int)cell.m_alpha3, ref num, ref num2, ref num3, ref num4, ref num5, ref num6, ref num7);
            SetBitAlphas((int)cell.m_district4, (int)cell.m_alpha4, ref num, ref num2, ref num3, ref num4, ref num5, ref num6, ref num7);
            b1 += num * alpha;
            b2 += num2 * alpha;
            b3 += num3 * alpha;
            b4 += num4 * alpha;
            b5 += num5 * alpha;
            b6 += num6 * alpha;
            b7 += num7 * alpha;
        }

        private void SetBitAlphas(int district, int alpha, ref int b1, ref int b2, ref int b3, ref int b4, ref int b5, ref int b6, ref int b7)
        {
            if ((district & 1) != 0)
            {
                b1 = Mathf.Max(b1, alpha - 128);
            }
            else
            {
                b1 = Mathf.Min(b1, 128 - alpha);
            }
            if ((district & 2) != 0)
            {
                b2 = Mathf.Max(b2, alpha - 128);
            }
            else
            {
                b2 = Mathf.Min(b2, 128 - alpha);
            }
            if ((district & 4) != 0)
            {
                b3 = Mathf.Max(b3, alpha - 128);
            }
            else
            {
                b3 = Mathf.Min(b3, 128 - alpha);
            }
            if ((district & 8) != 0)
            {
                b4 = Mathf.Max(b4, alpha - 128);
            }
            else
            {
                b4 = Mathf.Min(b4, 128 - alpha);
            }
            if ((district & 16) != 0)
            {
                b5 = Mathf.Max(b5, alpha - 128);
            }
            else
            {
                b5 = Mathf.Min(b5, 128 - alpha);
            }
            if ((district & 32) != 0)
            {
                b6 = Mathf.Max(b6, alpha - 128);
            }
            else
            {
                b6 = Mathf.Min(b6, 128 - alpha);
            }
            if ((district & 64) != 0)
            {
                b7 = Mathf.Max(b7, alpha - 128);
            }
            else
            {
                b7 = Mathf.Min(b7, 128 - alpha);
            }
        }

        [ReplaceMethod]
        public void ModifyCell(int x, int z, DistrictManager.Cell cell)
        {
            var dm = DistrictManager.instance;
            if (cell.m_alpha2 > cell.m_alpha1)
            {
                Exchange(ref cell.m_alpha1, ref cell.m_alpha2, ref cell.m_district1, ref cell.m_district2);
            }
            if (cell.m_alpha3 > cell.m_alpha1)
            {
                Exchange(ref cell.m_alpha1, ref cell.m_alpha3, ref cell.m_district1, ref cell.m_district3);
            }
            if (cell.m_alpha4 > cell.m_alpha1)
            {
                Exchange(ref cell.m_alpha1, ref cell.m_alpha4, ref cell.m_district1, ref cell.m_district4);
            }
            int num = z * GRID + x;
            DistrictManager.Cell cell2 = m_districtGrid[num];
            m_districtGrid[num] = cell;
            District[] expr_E4_cp_0 = dm.m_districts.m_buffer;
            byte expr_E4_cp_1 = cell.m_district1;
            expr_E4_cp_0[(int)expr_E4_cp_1].m_totalAlpha = expr_E4_cp_0[(int)expr_E4_cp_1].m_totalAlpha + (uint)cell.m_alpha1;
            District[] expr_10E_cp_0 = dm.m_districts.m_buffer;
            byte expr_10E_cp_1 = cell.m_district2;
            expr_10E_cp_0[(int)expr_10E_cp_1].m_totalAlpha = expr_10E_cp_0[(int)expr_10E_cp_1].m_totalAlpha + (uint)cell.m_alpha2;
            District[] expr_138_cp_0 = dm.m_districts.m_buffer;
            byte expr_138_cp_1 = cell.m_district3;
            expr_138_cp_0[(int)expr_138_cp_1].m_totalAlpha = expr_138_cp_0[(int)expr_138_cp_1].m_totalAlpha + (uint)cell.m_alpha3;
            District[] expr_162_cp_0 = dm.m_districts.m_buffer;
            byte expr_162_cp_1 = cell.m_district4;
            expr_162_cp_0[(int)expr_162_cp_1].m_totalAlpha = expr_162_cp_0[(int)expr_162_cp_1].m_totalAlpha + (uint)cell.m_alpha4;
            EraseDistrict(cell2.m_district1, ref dm.m_districts.m_buffer[(int)cell2.m_district1], (uint)cell2.m_alpha1);
            EraseDistrict(cell2.m_district2, ref dm.m_districts.m_buffer[(int)cell2.m_district2], (uint)cell2.m_alpha2);
            EraseDistrict(cell2.m_district3, ref dm.m_districts.m_buffer[(int)cell2.m_district3], (uint)cell2.m_alpha3);
            EraseDistrict(cell2.m_district4, ref dm.m_districts.m_buffer[(int)cell2.m_district4], (uint)cell2.m_alpha4);
        }

        private void Exchange(ref byte alpha1, ref byte alpha2, ref byte district1, ref byte district2)
        {
            byte b = alpha2;
            byte b2 = district2;
            alpha2 = alpha1;
            district2 = district1;
            alpha1 = b;
            district1 = b2;
        }

        private void EraseDistrict(byte district, ref District data, uint amount)
        {
            var dm = DistrictManager.instance;
            if (amount >= data.m_totalAlpha)
            {
                if (district == 0)
                {
                    data.m_totalAlpha = 0u;
                }
                else
                {
                    ReleaseDistrictImplementation(district, ref dm.m_districts.m_buffer[(int)district]);
                }
            }
            else
            {
                data.m_totalAlpha -= amount;
            }
        }

        private void ReleaseDistrictImplementation(byte district, ref District data)
        {
            var dm = DistrictManager.instance;
            if (data.m_flags != District.Flags.None)
            {
                InstanceID id = default(InstanceID);
                id.District = district;
                Singleton<InstanceManager>.instance.ReleaseInstance(id);
                data.m_flags = District.Flags.None;
                data.m_totalAlpha = 0u;
                dm.m_districts.ReleaseItem(district);
                dm.m_districtCount = (int)(dm.m_districts.ItemCount() - 1u);
            }
        }
    }
}
