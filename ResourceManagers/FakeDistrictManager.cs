using ColossalFramework;
using ColossalFramework.IO;
using System.Reflection;
using System.Threading;
using ColossalFramework.UI;
using EightyOne.Areas;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(DistrictManager))]
    public class FakeDistrictManager : DistrictManager
    {

        public new class Data : IDataContainer
        {
            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
                DistrictManager instance = Singleton<DistrictManager>.instance;
                District[] buffer = instance.m_districts.m_buffer;
                DistrictManager.Cell[] districtGrid = FakeDistrictManager.districtGrid;
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

                FakeDistrictManager.districtGrid = districtGrid;
            }

            public void Serialize(DataSerializer s)
            {
                var districtGrid = FakeDistrictManager.districtGrid;
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

        public static int GRID = 900;
        public static int HALFGRID = 450;
        public static DistrictManager.Cell[] districtGrid;
        private static Color32[] colorBuffer;
        private static int[] distanceBuffer;
        private static int[] indexBuffer;

        private static Texture2D districtTexture1;
        private static Texture2D districtTexture2;

        private static FieldInfo modifiedX1Field;
        private static FieldInfo modifiedZ1Field;
        private static FieldInfo modifiedX2Field;
        private static FieldInfo modifiedZ2Field;
        private static FieldInfo fullUpdateField;
        private static FieldInfo modifyLockField;
        private static FieldInfo namesModifiedField;
        private static FieldInfo areaMaterialField;
        private static FieldInfo highlightPolicyField;
        private static FieldInfo nameMeshField;
        private static FieldInfo iconMeshField;

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
            if (districtTexture1 != null)
            {
                UnityEngine.Object.Destroy(districtTexture1);
                districtTexture1 = null;
            }
            if (districtTexture2 != null)
            {
                UnityEngine.Object.Destroy(districtTexture2);
                districtTexture2 = null;
            }
            districtGrid = null;
        }

        public static void Init()
        {
            var districtManager = DistrictManager.instance;
            if (districtGrid == null)
            {
                districtGrid = new DistrictManager.Cell[GRID * GRID];
                for (int i = 0; i < districtGrid.Length; i++)
                {
                    districtGrid[i].m_district1 = 0;
                    districtGrid[i].m_district2 = 1;
                    districtGrid[i].m_district3 = 2;
                    districtGrid[i].m_district4 = 3;
                    districtGrid[i].m_alpha1 = 255;
                    districtGrid[i].m_alpha2 = 0;
                    districtGrid[i].m_alpha3 = 0;
                    districtGrid[i].m_alpha4 = 0;
                }
                var oldGrid = districtManager.m_districtGrid;
                int diff = (GRID - DISTRICTGRID_RESOLUTION) / 2;
                for (var i = 0; i < DISTRICTGRID_RESOLUTION; i += 1)
                {
                    for (var j = 0; j < DISTRICTGRID_RESOLUTION; j += 1)
                    {
                        districtGrid[(j + diff) * GRID + (i + diff)] = oldGrid[j * DISTRICTGRID_RESOLUTION + i];
                    }
                }
            }
            colorBuffer = new Color32[GRID * GRID];
            distanceBuffer = new int[HALFGRID * HALFGRID];
            indexBuffer = new int[HALFGRID * HALFGRID];
            m_tempData = new TempDistrictData[128];

            modifiedX1Field = typeof(DistrictManager).GetField("m_modifiedX1", BindingFlags.Instance | BindingFlags.NonPublic);
            modifiedZ1Field = typeof(DistrictManager).GetField("m_modifiedZ1", BindingFlags.Instance | BindingFlags.NonPublic);
            modifiedX2Field = typeof(DistrictManager).GetField("m_modifiedX2", BindingFlags.Instance | BindingFlags.NonPublic);
            modifiedZ2Field = typeof(DistrictManager).GetField("m_modifiedZ2", BindingFlags.Instance | BindingFlags.NonPublic);
            fullUpdateField = typeof(DistrictManager).GetField("m_fullUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            modifyLockField = typeof(DistrictManager).GetField("m_modifyLock", BindingFlags.Instance | BindingFlags.NonPublic);
            namesModifiedField = typeof(DistrictManager).GetField("m_namesModified", BindingFlags.Instance | BindingFlags.NonPublic);
            areaMaterialField = typeof(DistrictManager).GetField("m_areaMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
            highlightPolicyField = typeof(DistrictManager).GetField("m_highlightPolicy", BindingFlags.Instance | BindingFlags.NonPublic);
            nameMeshField = typeof(DistrictManager).GetField("m_nameMesh", BindingFlags.Instance | BindingFlags.NonPublic);
            iconMeshField = typeof(DistrictManager).GetField("m_iconMesh", BindingFlags.Instance | BindingFlags.NonPublic);

            districtTexture1 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            districtTexture2 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            districtTexture1.wrapMode = TextureWrapMode.Clamp;
            districtTexture2.wrapMode = TextureWrapMode.Clamp;
            ID_Districts1 = Shader.PropertyToID("_Districts1");
            ID_Districts2 = Shader.PropertyToID("_Districts2");
            ID_DistrictMapping = Shader.PropertyToID("_DistrictMapping");
            ID_Highlight1 = Shader.PropertyToID("_Highlight1");
            ID_Highlight2 = Shader.PropertyToID("_Highlight2");

            SimulationManager.instance.AddAction(() =>
            {
                nameMeshField.SetValue(districtManager, null);
                iconMeshField.SetValue(districtManager, null);
                modifiedX1Field.SetValue(districtManager, 0);
                modifiedZ1Field.SetValue(districtManager, 0);
                modifiedX2Field.SetValue(districtManager, GRID);
                modifiedZ2Field.SetValue(districtManager, GRID);
                fullUpdateField.SetValue(districtManager, true);
                districtManager.NamesModified();
            });
        }

        [RedirectReverse(true)]
        private static void AddDistrictColor1(DistrictManager manager, byte district, byte alpha, ref Color32 color1)
        {
            UnityEngine.Debug.Log($"{manager}+{district}+{alpha}+{color1}");
        }

        [RedirectReverse(true)]
        private static void AddDistrictColor2(DistrictManager manager, byte district, DistrictPolicies.Policies policy, byte alpha, bool inArea, ref Color32 color2)
        {
            UnityEngine.Debug.Log($"{manager}-{district}-{policy}-{alpha}-{inArea}-{color2}");
        }

        [RedirectReverse(true)]
        private static void SetBitAlphas(DistrictManager manager, DistrictManager.Cell cell, int alpha, ref int b1, ref int b2, ref int b3, ref int b4, ref int b5, ref int b6, ref int b7)
        {
            UnityEngine.Debug.Log($"{manager}-{cell}-{alpha}-{b1}-{b2}-{b3}-{b4}-{b5}-{b6}-{b7}");
        }

        [RedirectReverse(true)]
        private static void Exchange(DistrictManager manager, ref byte alpha1, ref byte alpha2, ref byte district1, ref byte district2)
        {
            UnityEngine.Debug.Log($"{manager}-{alpha1}-{alpha2}-{district1}-{district2}");
        }

        [RedirectReverse(true)]
        private static void EraseDistrict(DistrictManager manager, byte district, ref District data, uint amount)
        {
            UnityEngine.Debug.Log($"{manager}-{district}-{data}-{amount}");
        }

        [RedirectMethod]
        public void set_HighlightPolicy(DistrictPolicies.Policies value)
        {
            if (value == (DistrictPolicies.Policies)highlightPolicyField.GetValue(this))
                return;
            highlightPolicyField.SetValue(this, value);
            //begin mod
            this.AreaModified(0, 0, GRID - 1, GRID - 1, false);
            //end mod
        }

        [RedirectMethod]
        protected override void BeginOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            var areaMaterial = (Material)areaMaterialField.GetValue(this);
            if ((!this.DistrictsVisible && !this.DistrictsInfoVisible) || areaMaterial == null)
            {
                return;
            }
            areaMaterial.SetTexture(ID_Districts1, districtTexture1);
            areaMaterial.SetTexture(ID_Districts2, districtTexture2);
            Vector4 vector;
            //begin mod
            vector.z = 1 / (19.2f * GRID);
            //end mod
            vector.x = 0.5f;
            vector.y = 0.5f;
            vector.w = ((this.HighlightDistrict <= 0) ? 0f : 1f);
            areaMaterial.SetVector(ID_DistrictMapping, vector);
            Color32 c = new Color32(128, 128, 128, 128);
            Color32 c2 = new Color32(128, 128, 128, 128);
            AddDistrictColor1(this, (byte)Mathf.Max(0, this.HighlightDistrict), 255, ref c);
            AddDistrictColor2(this, (byte)Mathf.Max(0, this.HighlightDistrict), DistrictPolicies.Policies.None, 255, true, ref c2);
            areaMaterial.SetColor(ID_Highlight1, c);
            areaMaterial.SetColor(ID_Highlight2, c2);
            if (this.HighlightPolicy != DistrictPolicies.Policies.None)
                areaMaterial.EnableKeyword("POLICYTOOL_ON");
            else
                areaMaterial.DisableKeyword("POLICYTOOL_ON");
            //begin mod
            Bounds bounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(19.2f * GRID, 1024f, 19.2f * GRID) + Vector3.one);
            //end mod
            ++Singleton<DistrictManager>.instance.m_drawCallData.m_overlayCalls;
            Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, areaMaterial, 0, bounds);
        }

        [RedirectMethod]
        private void UpdateNames()
        {
            UIFontManager.Invalidate(this.m_properties.m_areaNameFont);
            namesModifiedField.SetValue(this, false);
            UIRenderData destination = UIRenderData.Obtain();
            UIRenderData uiRenderData = UIRenderData.Obtain();
            try
            {
                destination.Clear();
                uiRenderData.Clear();
                PoolList<Vector3> vertices1 = uiRenderData.vertices;
                PoolList<Vector3> normals1 = uiRenderData.normals;
                PoolList<Color32> colors1 = uiRenderData.colors;
                PoolList<Vector2> uvs1 = uiRenderData.uvs;
                PoolList<int> triangles1 = uiRenderData.triangles;
                PoolList<Vector3> vertices2 = destination.vertices;
                PoolList<Vector3> normals2 = destination.normals;
                PoolList<Color32> colors2 = destination.colors;
                PoolList<Vector2> uvs2 = destination.uvs;
                PoolList<int> triangles2 = destination.triangles;
                for (int district = 1; district < 128; ++district)
                {
                    if (this.m_districts.m_buffer[district].m_flags != District.Flags.None)
                    {
                        string text = this.GetDistrictName(district) + "\n";
                        PositionData<DistrictPolicies.Policies>[] orderedEnumData = Utils.GetOrderedEnumData<DistrictPolicies.Policies>();
                        for (int index = 0; index < orderedEnumData.Length; ++index)
                        {
                            if (this.IsDistrictPolicySet(orderedEnumData[index].enumValue, (byte)district))
                            {
                                string str = "IconPolicy" + orderedEnumData[index].enumName;
                                text = text + "<sprite " + str + "> ";
                            }
                        }
                        if (text != null)
                        {
                            int count1 = normals2.Count;
                            int count2 = normals1.Count;
                            using (UIFontRenderer uiFontRenderer = this.m_properties.m_areaNameFont.ObtainRenderer())
                            {
                                UIDynamicFont.DynamicFontRenderer dynamicFontRenderer = uiFontRenderer as UIDynamicFont.DynamicFontRenderer;
                                if (dynamicFontRenderer != null)
                                {
                                    dynamicFontRenderer.spriteAtlas = this.m_properties.m_areaIconAtlas;
                                    dynamicFontRenderer.spriteBuffer = uiRenderData;
                                }
                                uiFontRenderer.defaultColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)64);
                                uiFontRenderer.textScale = 2f;
                                uiFontRenderer.pixelRatio = 1f;
                                uiFontRenderer.processMarkup = true;
                                uiFontRenderer.multiLine = true;
                                uiFontRenderer.wordWrap = true;
                                uiFontRenderer.textAlign = UIHorizontalAlignment.Center;
                                uiFontRenderer.maxSize = new Vector2(450f, 900f);
                                uiFontRenderer.shadow = false;
                                uiFontRenderer.shadowColor = (Color32)Color.black;
                                uiFontRenderer.shadowOffset = Vector2.one;
                                Vector2 vector2 = uiFontRenderer.MeasureString(text);
                                this.m_districts.m_buffer[district].m_nameSize = vector2;
                                vertices2.Add(new Vector3(-vector2.x, -vector2.y, 1f));
                                vertices2.Add(new Vector3(-vector2.x, vector2.y, 1f));
                                vertices2.Add(new Vector3(vector2.x, vector2.y, 1f));
                                vertices2.Add(new Vector3(vector2.x, -vector2.y, 1f));
                                colors2.Add(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32((byte)0, (byte)0, (byte)0, byte.MaxValue));
                                uvs2.Add(new Vector2(-1f, -1f));
                                uvs2.Add(new Vector2(-1f, 1f));
                                uvs2.Add(new Vector2(1f, 1f));
                                uvs2.Add(new Vector2(1f, -1f));
                                triangles2.Add(vertices2.Count - 4);
                                triangles2.Add(vertices2.Count - 3);
                                triangles2.Add(vertices2.Count - 1);
                                triangles2.Add(vertices2.Count - 1);
                                triangles2.Add(vertices2.Count - 3);
                                triangles2.Add(vertices2.Count - 2);
                                uiFontRenderer.vectorOffset = new Vector3(-225f, vector2.y * 0.5f, 0.0f);
                                uiFontRenderer.Render(text, destination);
                            }
                            int count3 = vertices2.Count;
                            int count4 = normals2.Count;
                            Vector3 vector3 = this.m_districts.m_buffer[district].m_nameLocation;
                            for (int index = count1; index < count4; ++index)
                                normals2[index] = vector3;
                            for (int index = count4; index < count3; ++index)
                                normals2.Add(vector3);
                            int count5 = vertices1.Count;
                            int count6 = normals1.Count;
                            for (int index = count2; index < count6; ++index)
                                normals1[index] = vector3;
                            for (int index = count6; index < count5; ++index)
                                normals1.Add(vector3);
                        }
                    }
                }
                if ((Mesh)nameMeshField.GetValue(this) == null)
                    nameMeshField.SetValue(this, new Mesh());
                var nameMesh = (Mesh) nameMeshField.GetValue(this);
                nameMesh.Clear();
                nameMesh.vertices = vertices2.ToArray();
                nameMesh.normals = normals2.ToArray();
                nameMesh.colors32 = colors2.ToArray();
                nameMesh.uv = uvs2.ToArray();
                nameMesh.triangles = triangles2.ToArray();
                //begin mod
                nameMesh.bounds = new Bounds(Vector3.zero, new Vector3(GRID * 19.2f, 1024f, GRID * 19.2f));
                //end mod
                if ((Mesh)iconMeshField.GetValue(this) == null)
                    iconMeshField.SetValue(this, new Mesh());
                var iconMesh = (Mesh)iconMeshField.GetValue(this);
                iconMesh.Clear();
                iconMesh.vertices = vertices1.ToArray();
                iconMesh.normals = normals1.ToArray();
                iconMesh.colors32 = colors1.ToArray();
                iconMesh.uv = uvs1.ToArray();
                iconMesh.triangles = triangles1.ToArray();
                //begin mod
                iconMesh.bounds = new Bounds(Vector3.zero, new Vector3(GRID * 19.2f, 1024f, GRID * 19.2f));
                //end mod
            }
            finally
            {
                destination.Release();
                uiRenderData.Release();
            }
        }

        [RedirectMethod]
        private void UpdateTexture()
        {
            var modifyLock = modifyLockField.GetValue(this);
            do
                ;
            while (!Monitor.TryEnter(modifyLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
            int num;
            int num2;
            int num3;
            int num4;
            bool fullUpdate;
            try
            {
                num = (int)modifiedX1Field.GetValue(this);
                num2 = (int)modifiedZ1Field.GetValue(this);
                num3 = (int)modifiedX2Field.GetValue(this);
                num4 = (int)modifiedZ2Field.GetValue(this);
                fullUpdate = (bool)fullUpdateField.GetValue(this);
                modifiedX1Field.SetValue(this, 10000);
                modifiedZ1Field.SetValue(this, 10000);
                modifiedX2Field.SetValue(this, -10000);
                modifiedZ2Field.SetValue(this, -10000);
                fullUpdateField.SetValue(this, false);
            }
            finally
            {
                Monitor.Exit(modifyLock);
            }
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            int num5 = Mathf.RoundToInt(99.99999f);
            //begin mod
            int num6 = (FakeGameAreaManager.GRID * num5 >> 1) - HALFGRID;
            //end mod
            if ((num3 - num + 1) * (num4 - num2 + 1) > HALFGRID * HALFGRID)
            {
                num = 1;
                num2 = 1;
                //begin mod
                num3 = GRID - 2;
                num4 = GRID - 2;
                //end mod
                if (fullUpdate)
                {
                    for (int i = num2; i <= num4; i++)
                    {
                        for (int j = num; j <= num3; j++)
                        {
                            //begin mod
                            int num7 = i * GRID + j;
                            DistrictManager.Cell cell = districtGrid[num7];
                            //end mod
                            Color32 color = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(this, cell.m_district1, cell.m_alpha1, ref color);
                            AddDistrictColor1(this, cell.m_district2, cell.m_alpha2, ref color);
                            AddDistrictColor1(this, cell.m_district3, cell.m_alpha3, ref color);
                            AddDistrictColor1(this, cell.m_district4, cell.m_alpha4, ref color);
                            colorBuffer[num7] = color;
                        }
                    }
                    //begin mod
                    districtTexture1.SetPixels32(colorBuffer);
                    districtTexture1.Apply();
                    //end mod
                }
                for (int k = num2; k <= num4; k++)
                {
                    for (int l = num; l <= num3; l++)
                    {
                        //begin mod
                        int num8 = k * GRID + l;
                        DistrictManager.Cell cell2 = districtGrid[num8];
                        //end mod
                        bool inArea = false;
                        int num9 = (l + num6) / num5;
                        int num10 = (k + num6) / num5;
                        //begin mod
                        if (num9 >= 0 && num9 < FakeGameAreaManager.GRID && num10 >= 0 && num10 < FakeGameAreaManager.GRID)
                            inArea = (areaGrid[num10 * FakeGameAreaManager.GRID + num9] != 0);
                        //end mod
                        Color32 color2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(this, cell2.m_district1, this.HighlightPolicy, cell2.m_alpha1, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district2, this.HighlightPolicy, cell2.m_alpha2, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district3, this.HighlightPolicy, cell2.m_alpha3, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district4, this.HighlightPolicy, cell2.m_alpha4, inArea, ref color2);
                        //begin mod
                        colorBuffer[num8] = color2;
                        //end mod
                    }
                }
                //begin mod
                districtTexture2.SetPixels32(colorBuffer);
                districtTexture2.Apply();
                //end mod
            }
            else
            {
                num = Mathf.Max(1, num);
                num2 = Mathf.Max(1, num2);
                //begin mod
                num3 = Mathf.Min(GRID - 2, num3);
                num4 = Mathf.Min(GRID - 2, num4);
                //end mod
                for (int m = num2; m <= num4; m++)
                {
                    for (int n = num; n <= num3; n++)
                    {
                        //begin mod
                        int num11 = m * GRID + n;
                        DistrictManager.Cell cell3 = districtGrid[num11];
                        //end mod
                        if (fullUpdate)
                        {
                            Color32 c = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(this, cell3.m_district1, cell3.m_alpha1, ref c);
                            AddDistrictColor1(this, cell3.m_district2, cell3.m_alpha2, ref c);
                            AddDistrictColor1(this, cell3.m_district3, cell3.m_alpha3, ref c);
                            AddDistrictColor1(this, cell3.m_district4, cell3.m_alpha4, ref c);
                            districtTexture1.SetPixel(n, m, c);
                        }
                        bool inArea2 = false;
                        int num12 = (n + num6) / num5;
                        int num13 = (m + num6) / num5;
                        //begin mod
                        if (num12 >= 0 && num12 < FakeGameAreaManager.GRID && num13 >= 0 && num13 < FakeGameAreaManager.GRID)
                            inArea2 = (areaGrid[num13 * FakeGameAreaManager.GRID + num12] != 0);
                        //end mod
                        Color32 c2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(this, cell3.m_district1, this.HighlightPolicy, cell3.m_alpha1, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district2, this.HighlightPolicy, cell3.m_alpha2, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district3, this.HighlightPolicy, cell3.m_alpha3, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district4, this.HighlightPolicy, cell3.m_alpha4, inArea2, ref c2);
                        //begin mod
                        districtTexture2.SetPixel(n, m, c2);
                        //end mod
                    }
                }
                //begin mod
                if (fullUpdate)
                    districtTexture1.Apply();
                districtTexture2.Apply();
                //end mod
            }
        }

        [RedirectMethod]
        public new void NamesModified()
        {
            //begin mod
            int num = distanceBuffer.Length;
            for (int i = 0; i < num; i++)
                distanceBuffer[i] = 0;
            //end mod
            for (int j = 0; j < 128; j++)
                //begin mod
                m_tempData[j] = default(TempDistrictData);
            //end mod
            int num2 = 2;
            //begin mod
            int num3 = GRID * 2;
            //end mod
            int num4 = 0;
            int num5 = 0;
            //begin mod
            for (int k = 0; k < HALFGRID; k++)
            {
                for (int l = 0; l < HALFGRID; l++)
                {
                    //end mod
                    int num6 = k * num3 + l * num2;
                    //begin mod
                    byte district = districtGrid[num6].m_district1;
                    if (district != 0 && (l == 0 || k == 0 || l == HALFGRID - 1 || k == HALFGRID - 1 || districtGrid[num6 - num3].m_district1 != district || districtGrid[num6 - num2].m_district1 != district || districtGrid[num6 + num2].m_district1 != district || districtGrid[num6 + num3].m_district1 != district))
                    {
                        int num7 = k * HALFGRID + l;
                        distanceBuffer[num7] = 1;
                        indexBuffer[num5] = num7;
                        num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                        //end mod
                        m_tempData[(int)district].m_averageX = m_tempData[(int)district].m_averageX + l;
                        m_tempData[(int)district].m_averageZ = m_tempData[(int)district].m_averageZ + k;
                        ++m_tempData[(int)district].m_divider;
                    }
                }
            }
            for (int m = 0; m < 128; m++)
            {
                int divider = m_tempData[m].m_divider;
                if (divider != 0)
                {
                    m_tempData[m].m_averageX = (m_tempData[m].m_averageX + divider >> 1) / divider;
                    m_tempData[m].m_averageZ = (m_tempData[m].m_averageZ + divider >> 1) / divider;
                }
            }
            while (num4 != num5)
            {
                int num8 = indexBuffer[num4];
                num4 = ((num4 + 1) % (HALFGRID * HALFGRID));
                int num9 = num8 % HALFGRID;
                int num10 = num8 / HALFGRID;
                int num11 = num10 * num3 + num9 * num2;
                //begin mod
                byte district2 = districtGrid[num11].m_district1;
                //end mod
                int num12 = num9 - m_tempData[(int)district2].m_averageX;
                int num13 = num10 - m_tempData[(int)district2].m_averageZ;
                //begin mod
                int num14 = (GRID * GRID) - (GRID * GRID / 2) / distanceBuffer[num8] - num12 * num12 - num13 * num13;
                //end mod
                if (num14 > m_tempData[(int)district2].m_bestScore)
                {
                    m_tempData[(int)district2].m_bestScore = num14;
                    m_tempData[(int)district2].m_bestLocation = num8;
                }
                int num15 = num8 - 1;
                //begin mod
                if (num9 > 0 && distanceBuffer[num15] == 0 && districtGrid[num11 - num2].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                //end mod
                num15 = num8 + 1;
                //begin mod
                if (num9 < HALFGRID - 1 && distanceBuffer[num15] == 0 && districtGrid[num11 + num2].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 - HALFGRID;
                if (num10 > 0 && distanceBuffer[num15] == 0 && districtGrid[num11 - num3].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 + HALFGRID;
                if (num10 < HALFGRID - 1 && distanceBuffer[num15] == 0 && districtGrid[num11 + num3].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                //end mod
            }
            for (int n = 0; n < 128; n++)
            {
                int bestLocation = m_tempData[n].m_bestLocation;
                Vector3 vector;
                //begin mod
                vector.x = 19.2f * (float)(bestLocation % HALFGRID) * 2f - 19.2f * HALFGRID;
                //end mod
                vector.y = 0f;
                //begin mod
                vector.z = 19.2f * (float)(bestLocation / HALFGRID) * 2f - 19.2f * HALFGRID;
                //end
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, false, 0f);
                this.m_districts.m_buffer[n].m_nameLocation = vector;
            }
            namesModifiedField.SetValue(this, true);
        }

        [RedirectMethod]
        public new void GetDistrictArea(byte district, out int minX, out int minZ, out int maxX, out int maxZ)
        {
            minX = 10000;
            minZ = 10000;
            maxX = -10000;
            maxZ = -10000;
            //begin mod
            for (int index1 = 0; index1 < GRID; ++index1)
            {
                for (int index2 = 0; index2 < GRID; ++index2)
                {
                    DistrictManager.Cell cell = districtGrid[index1 * GRID + index2];
                    //end mod
                    if ((int)cell.m_alpha1 != 0 && (int)cell.m_district1 == (int)district)
                    {
                        if (index2 < minX)
                            minX = index2;
                        if (index1 < minZ)
                            minZ = index1;
                        if (index2 > maxX)
                            maxX = index2;
                        if (index1 > maxZ)
                            maxZ = index1;
                    }
                    else if ((int)cell.m_alpha2 != 0 && (int)cell.m_district2 == (int)district)
                    {
                        if (index2 < minX)
                            minX = index2;
                        if (index1 < minZ)
                            minZ = index1;
                        if (index2 > maxX)
                            maxX = index2;
                        if (index1 > maxZ)
                            maxZ = index1;
                    }
                    else if ((int)cell.m_alpha3 != 0 && (int)cell.m_district3 == (int)district)
                    {
                        if (index2 < minX)
                            minX = index2;
                        if (index1 < minZ)
                            minZ = index1;
                        if (index2 > maxX)
                            maxX = index2;
                        if (index1 > maxZ)
                            maxZ = index1;
                    }
                    else if ((int)cell.m_alpha4 != 0 && (int)cell.m_district4 == (int)district)
                    {
                        if (index2 < minX)
                            minX = index2;
                        if (index1 < minZ)
                            minZ = index1;
                        if (index2 > maxX)
                            maxX = index2;
                        if (index1 > maxZ)
                            maxZ = index1;
                    }
                }
            }
        }

        //TODO(earalov): make sure this method doesn't get inlined
        [RedirectMethod]
        public new byte GetDistrict(int x, int z)
        {
            //begin mod
            return districtGrid[z * GRID + x].m_district1;
            //end mod
        }

        [RedirectMethod]
        public byte GetDistrict(Vector3 worldPos)
        {
            //begin mod
            int num = Mathf.Clamp((int)(worldPos.x / 19.2f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(worldPos.z / 19.2f + HALFGRID), 0, GRID - 1);
            int num3 = num2 * GRID + num;
            return districtGrid[num3].m_district1;
            //end mod
        }

        [RedirectMethod]
        public new byte SampleDistrict(Vector3 worldPos)
        {
            //begin mod
            int num = Mathf.RoundToInt(worldPos.x * 13.333333f + (HALFGRID * HALFGRID) - HALFGRID);
            int num2 = Mathf.RoundToInt(worldPos.z * 13.333333f + (HALFGRID * HALFGRID) - HALFGRID);
            int num3 = Mathf.Clamp((int)(worldPos.x / 19.2f + HALFGRID), 0, GRID - 1);
            int num4 = Mathf.Clamp((int)(worldPos.z / 19.2f + HALFGRID), 0, GRID - 1);
            int num5 = Mathf.Min(num3 + 1, GRID - 1);
            int num6 = Mathf.Min(num4 + 1, GRID - 1);
            //end mod
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            int num12 = 0;
            int num13 = 0;
            //begin mod
            SetBitAlphas(this, districtGrid[num4 * GRID + num3], (255 - (num & 255)) * (255 - (num2 & 255)), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(this, districtGrid[num4 * GRID + num5], (num & 255) * (255 - (num2 & 255)), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(this, districtGrid[num6 * GRID + num3], (255 - (num & 255)) * (num2 & 255), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            SetBitAlphas(this, districtGrid[num6 * GRID + num5], (num & 255) * (num2 & 255), ref num7, ref num8, ref num9, ref num10, ref num11, ref num12, ref num13);
            //end mod
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

        [RedirectMethod]
        public void ModifyCell(int x, int z, DistrictManager.Cell cell)
        {
            if ((int)cell.m_alpha2 > (int)cell.m_alpha1)
                Exchange(this, ref cell.m_alpha1, ref cell.m_alpha2, ref cell.m_district1, ref cell.m_district2);
            if ((int)cell.m_alpha3 > (int)cell.m_alpha1)
                Exchange(this, ref cell.m_alpha1, ref cell.m_alpha3, ref cell.m_district1, ref cell.m_district3);
            if ((int)cell.m_alpha4 > (int)cell.m_alpha1)
                Exchange(this, ref cell.m_alpha1, ref cell.m_alpha4, ref cell.m_district1, ref cell.m_district4);
            //begin mod
            int index = z * GRID + x;
            DistrictManager.Cell cell1 = districtGrid[index];
            districtGrid[index] = cell;
            //end mod
            this.m_districts.m_buffer[(int)cell.m_district1].m_totalAlpha += (uint)cell.m_alpha1;
            this.m_districts.m_buffer[(int)cell.m_district2].m_totalAlpha += (uint)cell.m_alpha2;
            this.m_districts.m_buffer[(int)cell.m_district3].m_totalAlpha += (uint)cell.m_alpha3;
            this.m_districts.m_buffer[(int)cell.m_district4].m_totalAlpha += (uint)cell.m_alpha4;
            EraseDistrict(this, cell1.m_district1, ref this.m_districts.m_buffer[(int)cell1.m_district1], (uint)cell1.m_alpha1);
            EraseDistrict(this, cell1.m_district2, ref this.m_districts.m_buffer[(int)cell1.m_district2], (uint)cell1.m_alpha2);
            EraseDistrict(this, cell1.m_district3, ref this.m_districts.m_buffer[(int)cell1.m_district3], (uint)cell1.m_alpha3);
            EraseDistrict(this, cell1.m_district4, ref this.m_districts.m_buffer[(int)cell1.m_district4], (uint)cell1.m_alpha4);
        }


    }
}
