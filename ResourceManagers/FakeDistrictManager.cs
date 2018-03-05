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

    //TODO(earalov): (de)serialization, park specific methods, maybe something else
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
        public static DistrictManager.Cell[] parkGrid;
        private static Color32[] colorBuffer;
        private static int[] distanceBuffer;
        private static int[] indexBuffer;

        private static Texture2D districtTexture1;
        private static Texture2D districtTexture2;

        private static Texture2D parkTexture1;
        private static Texture2D parkTexture2;

        private static FieldInfo districtsModifiedX1Field;
        private static FieldInfo districtsModifiedZ1Field;
        private static FieldInfo districtsModifiedX2Field;
        private static FieldInfo districtsModifiedZ2Field;
        private static FieldInfo fullDistrictsUpdateField;

        private static FieldInfo parksModifiedX1Field;
        private static FieldInfo parksModifiedZ1Field;
        private static FieldInfo parksModifiedX2Field;
        private static FieldInfo parksModifiedZ2Field;
        private static FieldInfo fullParksUpdateField;

        private static FieldInfo modifyLockField;
        private static FieldInfo namesModifiedField;
        private static FieldInfo areaMaterialField;
        private static FieldInfo highlightPolicyField;
        private static FieldInfo nameMeshField;
        private static FieldInfo iconMeshField;

        private static int ID_DistrictsA1;
        private static int ID_DistrictsA2;
        private static int ID_DistrictsB1;
        private static int ID_DistrictsB2;
        private static int ID_DistrictMapping;
        private static int ID_Highlight1;
        private static int ID_Highlight2;
        private static int ID_EdgeColorA;
        private static int ID_AreaColorA;
        private static int ID_EdgeColorB;
        private static int ID_AreaColorB;

        private static TempDistrictData[] m_tempData;

        private struct TempDistrictData
        {
            public int m_averageX;
            public int m_averageZ;
            public int m_bestScore;
            //begin mod
            public int m_divider;
            public int m_bestLocation;
            //end mod
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
            if (parkTexture1 != null)
            {
                UnityEngine.Object.Destroy(parkTexture1);
                parkTexture1 = null;
            }
            if (parkTexture2 != null)
            {
                UnityEngine.Object.Destroy(parkTexture2);
                parkTexture2 = null;
            }
            districtGrid = null;
            parkGrid = null;
        }

        public static void Init()
        {
            //TODO(earalov): init park gird
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
                var oldGrid = DistrictManager.instance.m_districtGrid;
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

            districtsModifiedX1Field = typeof(DistrictManager).GetField("m_districtsModifiedX1", BindingFlags.Instance | BindingFlags.NonPublic);
            districtsModifiedZ1Field = typeof(DistrictManager).GetField("m_districtsModifiedZ1", BindingFlags.Instance | BindingFlags.NonPublic);
            districtsModifiedX2Field = typeof(DistrictManager).GetField("m_districtsModifiedX2", BindingFlags.Instance | BindingFlags.NonPublic);
            districtsModifiedZ2Field = typeof(DistrictManager).GetField("m_districtsModifiedZ2", BindingFlags.Instance | BindingFlags.NonPublic);
            fullDistrictsUpdateField = typeof(DistrictManager).GetField("m_fullDistrictsUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

            parksModifiedX1Field = typeof(DistrictManager).GetField("m_parksModifiedX1", BindingFlags.Instance | BindingFlags.NonPublic);
            parksModifiedZ1Field = typeof(DistrictManager).GetField("m_parksModifiedZ1", BindingFlags.Instance | BindingFlags.NonPublic);
            parksModifiedX2Field = typeof(DistrictManager).GetField("m_parksModifiedX2", BindingFlags.Instance | BindingFlags.NonPublic);
            parksModifiedZ2Field = typeof(DistrictManager).GetField("m_parksModifiedZ2", BindingFlags.Instance | BindingFlags.NonPublic);
            fullParksUpdateField = typeof(DistrictManager).GetField("m_fullParksUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            
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

            parkTexture1 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            parkTexture2 = new Texture2D(GRID, GRID, TextureFormat.ARGB32, false, true);
            parkTexture1.wrapMode = TextureWrapMode.Clamp;
            parkTexture2.wrapMode = TextureWrapMode.Clamp;

            ID_DistrictsA1 = Shader.PropertyToID("_DistrictsA1");
            ID_DistrictsA2 = Shader.PropertyToID("_DistrictsA2");
            ID_DistrictsB1 = Shader.PropertyToID("_DistrictsB1");
            ID_DistrictsB2 = Shader.PropertyToID("_DistrictsB2");
            ID_DistrictMapping = Shader.PropertyToID("_DistrictMapping");
            ID_Highlight1 = Shader.PropertyToID("_Highlight1");
            ID_Highlight2 = Shader.PropertyToID("_Highlight2");

            ID_EdgeColorA = Shader.PropertyToID("_EdgeColorA");
            ID_AreaColorA = Shader.PropertyToID("_AreaColorA");
            ID_EdgeColorB = Shader.PropertyToID("_EdgeColorB");
            ID_AreaColorB = Shader.PropertyToID("_AreaColorB");
            
            SimulationManager.instance.AddAction(() =>
            {
                nameMeshField.SetValue(DistrictManager.instance, null);
                iconMeshField.SetValue(DistrictManager.instance, null);
                districtsModifiedX1Field.SetValue(DistrictManager.instance, 0);
                districtsModifiedZ1Field.SetValue(DistrictManager.instance, 0);
                districtsModifiedX2Field.SetValue(DistrictManager.instance, GRID);
                districtsModifiedZ2Field.SetValue(DistrictManager.instance, GRID);
                fullDistrictsUpdateField.SetValue(DistrictManager.instance, true);
                parksModifiedX1Field.SetValue(DistrictManager.instance, 0);
                parksModifiedZ1Field.SetValue(DistrictManager.instance, 0);
                parksModifiedX2Field.SetValue(DistrictManager.instance, GRID);
                parksModifiedZ2Field.SetValue(DistrictManager.instance, GRID);
                fullParksUpdateField.SetValue(DistrictManager.instance, true);
                DistrictManager.instance.NamesModified();
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
        private static void Exchange(ref byte alpha1, ref byte alpha2, ref byte district1, ref byte district2)
        {
            UnityEngine.Debug.Log($"{alpha1}-{alpha2}-{district1}-{district2}");
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
            if (this.DistrictsVisible || this.DistrictsInfoVisible && !this.ParksVisible)
            {
                if (!((UnityEngine.Object)areaMaterial != (UnityEngine.Object)null))
                    return;
                areaMaterial.SetTexture(ID_DistrictsA1, (Texture)districtTexture1);
                areaMaterial.SetTexture(ID_DistrictsA2, (Texture)districtTexture2);
                areaMaterial.SetTexture(ID_DistrictsB1, (Texture)parkTexture1);
                areaMaterial.SetTexture(ID_DistrictsB2, (Texture)parkTexture2);
                bool flag = Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.None;
                areaMaterial.SetColor(ID_EdgeColorA, !flag ? this.m_properties.m_districtEdgeColor : this.m_properties.m_districtEdgeColorInfo);
                areaMaterial.SetColor(ID_AreaColorA, !flag ? this.m_properties.m_districtAreaColor : this.m_properties.m_districtAreaColorInfo);
                areaMaterial.SetColor(ID_EdgeColorB, !flag ? this.m_properties.m_parkEdgeColor : this.m_properties.m_parkEdgeColorInfo);
                areaMaterial.SetColor(ID_AreaColorB, !flag ? this.m_properties.m_parkAreaColor : this.m_properties.m_parkAreaColorInfo);
                Vector4 vector4;
                //begin mod
                vector4.z = 1 / (19.2f * GRID);
                //end mod
                vector4.x = 0.5f;
                vector4.y = 0.5f;
                vector4.w = this.HighlightDistrict <= 0 ? 0.0f : 1f;
                areaMaterial.SetVector(ID_DistrictMapping, vector4);
                Color32 color1 = new Color32((byte)128, (byte)128, (byte)128, (byte)128);
                Color32 color2 = new Color32((byte)128, (byte)128, (byte)128, (byte)128);
                AddDistrictColor1(this, (byte)Mathf.Max(0, this.HighlightDistrict), byte.MaxValue, ref color1);
                AddDistrictColor2(this, (byte)Mathf.Max(0, this.HighlightDistrict), DistrictPolicies.Policies.None, byte.MaxValue, true, ref color2);
                areaMaterial.SetColor(ID_Highlight1, (Color)color1);
                areaMaterial.SetColor(ID_Highlight2, (Color)color2);
                if (this.HighlightPolicy != DistrictPolicies.Policies.None)
                    areaMaterial.EnableKeyword("POLICYTOOL_ON");
                else
                    areaMaterial.DisableKeyword("POLICYTOOL_ON");
                //begin mod
                Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(19.2f * GRID, 1024f, 19.2f * GRID) + Vector3.one);
                //end mod
                ++Singleton<DistrictManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, areaMaterial, 0, bounds);
            }
            else
            {
                if (!this.ParksVisible && !this.ParksInfoVisible || !((UnityEngine.Object)areaMaterial != (UnityEngine.Object)null))
                    return;
                areaMaterial.SetTexture(ID_DistrictsA1, (Texture)parkTexture1);
                areaMaterial.SetTexture(ID_DistrictsA2, (Texture)parkTexture2);
                areaMaterial.SetTexture(ID_DistrictsB1, (Texture)districtTexture1);
                areaMaterial.SetTexture(ID_DistrictsB2, (Texture)districtTexture2);
                bool flag = Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.None;
                areaMaterial.SetColor(ID_EdgeColorA, !flag ? this.m_properties.m_parkEdgeColor : this.m_properties.m_parkEdgeColorInfo);
                areaMaterial.SetColor(ID_AreaColorA, !flag ? this.m_properties.m_parkAreaColor : this.m_properties.m_parkAreaColorInfo);
                areaMaterial.SetColor(ID_EdgeColorB, !flag ? this.m_properties.m_districtEdgeColor : this.m_properties.m_districtEdgeColorInfo);
                areaMaterial.SetColor(ID_AreaColorB, !flag ? this.m_properties.m_districtAreaColor : this.m_properties.m_districtAreaColorInfo);
                Vector4 vector4;
                //begin mod
                vector4.z = 1 / (19.2f * GRID);
                //end mod
                vector4.x = 0.5f;
                vector4.y = 0.5f;
                vector4.w = this.HighlightPark <= 0 ? 0.0f : 1f;
                areaMaterial.SetVector(ID_DistrictMapping, vector4);
                Color32 color1 = new Color32((byte)128, (byte)128, (byte)128, (byte)128);
                Color32 color2 = new Color32((byte)128, (byte)128, (byte)128, (byte)128);
                AddDistrictColor1(this, (byte)Mathf.Max(0, this.HighlightPark), byte.MaxValue, ref color1);
                AddDistrictColor2(this, (byte)Mathf.Max(0, this.HighlightPark), DistrictPolicies.Policies.None, byte.MaxValue, true, ref color2);
                areaMaterial.SetColor(ID_Highlight1, (Color)color1);
                areaMaterial.SetColor(ID_Highlight2, (Color)color2);
                areaMaterial.DisableKeyword("POLICYTOOL_ON");
                //begin mod
                Bounds bounds = new Bounds(new Vector3(0.0f, 512f, 0.0f), new Vector3(19.2f * GRID, 1024f, 19.2f * GRID) + Vector3.one);
                //end mod
                ++Singleton<DistrictManager>.instance.m_drawCallData.m_overlayCalls;
                Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, areaMaterial, 0, bounds);
            }
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
                PositionData<DistrictPolicies.Policies>[] orderedEnumData = Utils.GetOrderedEnumData<DistrictPolicies.Policies>();
                for (int district = 1; district < 128; ++district)
                {
                    if (this.m_districts.m_buffer[district].m_flags != District.Flags.None)
                    {
                        string text = this.GetDistrictName(district) + "\n";
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
                            using (UIFontRenderer renderer = this.m_properties.m_areaNameFont.ObtainRenderer())
                            {
                                UIDynamicFont.DynamicFontRenderer dynamicFontRenderer = renderer as UIDynamicFont.DynamicFontRenderer;
                                if (dynamicFontRenderer != null)
                                {
                                    dynamicFontRenderer.spriteAtlas = this.m_properties.m_areaIconAtlas;
                                    dynamicFontRenderer.spriteBuffer = uiRenderData;
                                }
                                float x1 = 450f;
                                renderer.defaultColor = new Color32((byte)0, (byte)0, byte.MaxValue, byte.MaxValue);
                                renderer.textScale = 2f;
                                renderer.pixelRatio = 1f;
                                renderer.processMarkup = true;
                                renderer.multiLine = true;
                                renderer.wordWrap = true;
                                renderer.textAlign = UIHorizontalAlignment.Center;
                                renderer.maxSize = new Vector2(x1, 900f);
                                renderer.shadow = false;
                                renderer.shadowColor = new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue);
                                renderer.shadowOffset = Vector2.one;
                                Vector2 vector2 = renderer.MeasureString(text);
                                float x2 = vector2.x;
                                if ((double)vector2.x > (double)x1)
                                {
                                    x2 = x1 + (float)(((double)vector2.x - (double)x1) * 0.5);
                                    x1 = vector2.x;
                                    renderer.maxSize = new Vector2(x1, 900f);
                                    vector2 = renderer.MeasureString(text);
                                }
                                this.m_districts.m_buffer[district].m_nameSize = vector2;
                                vertices2.Add(new Vector3(-x2, -vector2.y, 1f));
                                vertices2.Add(new Vector3(-x2, vector2.y, 1f));
                                vertices2.Add(new Vector3(x2, vector2.y, 1f));
                                vertices2.Add(new Vector3(x2, -vector2.y, 1f));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
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
                                renderer.vectorOffset = new Vector3(x1 * -0.5f, vector2.y * 0.5f, 0.0f);
                                renderer.Render(text, destination);
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
                for (int park = 1; park < 128; ++park)
                {
                    if (this.m_parks.m_buffer[park].m_flags != DistrictPark.Flags.None)
                    {
                        string empty = string.Empty;
                        for (int index = 1; (DistrictPark.ParkLevel)index <= this.m_parks.m_buffer[park].m_parkLevel; ++index)
                            empty += "<sprite ParkLevelStar>";
                        string text = empty + "\n" + this.GetParkName(park) + "\n";
                        for (int index = 0; index < orderedEnumData.Length; ++index)
                        {
                            if (this.IsParkPolicySet(orderedEnumData[index].enumValue, (byte)park))
                            {
                                string str = "IconPolicy" + orderedEnumData[index].enumName;
                                text = text + "<sprite " + str + "> ";
                            }
                        }
                        if (text != null)
                        {
                            int count1 = normals2.Count;
                            int count2 = normals1.Count;
                            using (UIFontRenderer renderer = this.m_properties.m_areaNameFont.ObtainRenderer())
                            {
                                UIDynamicFont.DynamicFontRenderer dynamicFontRenderer = renderer as UIDynamicFont.DynamicFontRenderer;
                                if (dynamicFontRenderer != null)
                                {
                                    dynamicFontRenderer.spriteAtlas = this.m_properties.m_areaIconAtlas;
                                    dynamicFontRenderer.spriteBuffer = uiRenderData;
                                }
                                float x1 = 450f;
                                renderer.defaultColor = new Color32((byte)0, byte.MaxValue, (byte)0, byte.MaxValue);
                                renderer.textScale = 1.6f;
                                renderer.pixelRatio = 1f;
                                renderer.processMarkup = true;
                                renderer.multiLine = true;
                                renderer.wordWrap = true;
                                renderer.textAlign = UIHorizontalAlignment.Center;
                                renderer.maxSize = new Vector2(x1, 900f);
                                renderer.shadow = false;
                                renderer.shadowColor = new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue);
                                renderer.shadowOffset = Vector2.one;
                                Vector2 vector2 = renderer.MeasureString(text);
                                float x2 = vector2.x;
                                if ((double)vector2.x > (double)x1)
                                {
                                    x2 = x1 + (float)(((double)vector2.x - (double)x1) * 0.5);
                                    x1 = vector2.x;
                                    renderer.maxSize = new Vector2(x1, 900f);
                                    vector2 = renderer.MeasureString(text);
                                }
                                this.m_parks.m_buffer[park].m_nameSize = vector2;
                                vertices2.Add(new Vector3(-x2, -vector2.y, 1f));
                                vertices2.Add(new Vector3(-x2, vector2.y, 1f));
                                vertices2.Add(new Vector3(x2, vector2.y, 1f));
                                vertices2.Add(new Vector3(x2, -vector2.y, 1f));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
                                colors2.Add(new Color32(byte.MaxValue, (byte)0, (byte)0, byte.MaxValue));
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
                                renderer.vectorOffset = new Vector3(x1 * -0.5f, vector2.y * 0.5f, 0.0f);
                                renderer.Render(text, destination);
                            }
                            int count3 = vertices2.Count;
                            int count4 = normals2.Count;
                            Vector3 nameLocation = this.m_parks.m_buffer[park].m_nameLocation;
                            for (int index = count1; index < count4; ++index)
                                normals2[index] = nameLocation;
                            for (int index = count4; index < count3; ++index)
                                normals2.Add(nameLocation);
                            int count5 = vertices1.Count;
                            int count6 = normals1.Count;
                            for (int index = count2; index < count6; ++index)
                                normals1[index] = nameLocation;
                            for (int index = count6; index < count5; ++index)
                                normals1.Add(nameLocation);
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
        private void UpdateDistrictTexture()
        {
            //begin mod
            var modifyLock = modifyLockField.GetValue(this);
            do
                ;
            while (!Monitor.TryEnter(modifyLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
            //end mod
            int districtsModifiedX1;
            int districtsModifiedZ1;
            int districtsModifiedX2;
            int districtsModifiedZ2;
            bool fullDistrictsUpdate;
            try
            {
                //begin mod
                districtsModifiedX1 = (int)districtsModifiedX1Field.GetValue(this);
                districtsModifiedZ1 = (int)districtsModifiedZ1Field.GetValue(this);
                districtsModifiedX2 = (int)districtsModifiedX2Field.GetValue(this);
                districtsModifiedZ2 = (int)districtsModifiedZ2Field.GetValue(this);
                fullDistrictsUpdate = (bool)fullDistrictsUpdateField.GetValue(this);
                districtsModifiedX1Field.SetValue(this, 10000);
                districtsModifiedZ1Field.SetValue(this, 10000);
                districtsModifiedX2Field.SetValue(this, -10000);
                districtsModifiedZ2Field.SetValue(this, -10000);
                fullDistrictsUpdateField.SetValue(this, false);
                //end mod
            }
            finally
            {
                Monitor.Exit(modifyLock);
            }
            //begin mod
            this.UpdateTexture(districtsModifiedX1, districtsModifiedZ1, districtsModifiedX2, districtsModifiedZ2, fullDistrictsUpdate, districtGrid, this.HighlightPolicy, districtTexture1, districtTexture2);
            //end mod
        }

        [RedirectMethod]
        private void UpdateParkTexture()
        {
            //begin mod
            var modifyLock = modifyLockField.GetValue(this);
            do
                ;
            while (!Monitor.TryEnter(modifyLock, SimulationManager.SYNCHRONIZE_TIMEOUT));
            //end mod
            int parksModifiedX1;
            int parksModifiedZ1;
            int parksModifiedX2;
            int parksModifiedZ2;
            bool fullParksUpdate;
            try
            {
                //begin mod
                parksModifiedX1 = (int)parksModifiedX1Field.GetValue(this);
                parksModifiedZ1 = (int)parksModifiedZ1Field.GetValue(this);
                parksModifiedX2 = (int)parksModifiedX2Field.GetValue(this);
                parksModifiedZ2 = (int)parksModifiedZ2Field.GetValue(this);
                fullParksUpdate = (bool)fullParksUpdateField.GetValue(this);
                parksModifiedX1Field.SetValue(this, 10000);
                parksModifiedZ1Field.SetValue(this, 10000);
                parksModifiedX2Field.SetValue(this, -10000);
                parksModifiedZ2Field.SetValue(this, -10000);
                fullParksUpdateField.SetValue(this, false);
                //end mod
            }
            finally
            {
                Monitor.Exit(modifyLock);
            }
            //begin mod
            this.UpdateTexture(parksModifiedX1, parksModifiedZ1, parksModifiedX2, parksModifiedZ2, fullParksUpdate, parkGrid, DistrictPolicies.Policies.None, parkTexture1, parkTexture2);
            //end mod
        }

        [RedirectMethod]
        private void UpdateTexture(int minX, int minZ, int maxX, int maxZ, bool fullUpdate, DistrictManager.Cell[] cells, DistrictPolicies.Policies highlightPolicy, Texture2D texture1, Texture2D texture2)
        {
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            int num5 = Mathf.RoundToInt(99.99999f);
            //begin mod
            int num6 = (FakeGameAreaManager.GRID * num5 >> 1) - HALFGRID;
            //end mod
            if ((maxX - minX + 1) * (maxZ - minZ + 1) > HALFGRID * HALFGRID)
            {
                minX = 1;
                minZ = 1;
                //begin mod
                maxX = GRID - 2;
                maxZ = GRID - 2;
                //end mod
                if (fullUpdate)
                {
                    for (int i = minZ; i <= maxZ; i++)
                    {
                        for (int j = minX; j <= maxX; j++)
                        {
                            //begin mod
                            int num7 = i * GRID + j;
                            DistrictManager.Cell cell = cells[num7];
                            //end mod
                            Color32 color = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(this, cell.m_district1, cell.m_alpha1, ref color);
                            AddDistrictColor1(this, cell.m_district2, cell.m_alpha2, ref color);
                            AddDistrictColor1(this, cell.m_district3, cell.m_alpha3, ref color);
                            AddDistrictColor1(this, cell.m_district4, cell.m_alpha4, ref color);
                            colorBuffer[num7] = color;
                        }
                    }
                    texture1.SetPixels32(colorBuffer);
                    texture1.Apply();
                }
                for (int k = minZ; k <= maxZ; k++)
                {
                    for (int l = maxX; l <= maxX; l++)
                    {
                        //begin mod
                        int num8 = k * GRID + l;
                        DistrictManager.Cell cell2 = cells[num8];
                        //end mod
                        bool inArea = false;
                        int num9 = (l + num6) / num5;
                        int num10 = (k + num6) / num5;
                        //begin mod
                        if (num9 >= 0 && num9 < FakeGameAreaManager.GRID && num10 >= 0 && num10 < FakeGameAreaManager.GRID)
                            inArea = (areaGrid[num10 * FakeGameAreaManager.GRID + num9] != 0);
                        //end mod
                        Color32 color2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(this, cell2.m_district1, highlightPolicy, cell2.m_alpha1, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district2, highlightPolicy, cell2.m_alpha2, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district3, highlightPolicy, cell2.m_alpha3, inArea, ref color2);
                        AddDistrictColor2(this, cell2.m_district4, highlightPolicy, cell2.m_alpha4, inArea, ref color2);
                        //begin mod
                        colorBuffer[num8] = color2;
                        //end mod
                    }
                }
                texture2.SetPixels32(colorBuffer);
                texture2.Apply();
            }
            else
            {
                minX = Mathf.Max(1, minX);
                minZ = Mathf.Max(1, minZ);
                //begin mod
                maxX = Mathf.Min(GRID - 2, maxX);
                maxZ = Mathf.Min(GRID - 2, maxZ);
                //end mod
                for (int m = minZ; m <= maxZ; m++)
                {
                    for (int n = minX; n <= maxX; n++)
                    {
                        //begin mod
                        int num11 = m * GRID + n;
                        DistrictManager.Cell cell3 = cells[num11];
                        //end mod
                        if (fullUpdate)
                        {
                            Color32 c = new Color32(128, 128, 128, 128);
                            AddDistrictColor1(this, cell3.m_district1, cell3.m_alpha1, ref c);
                            AddDistrictColor1(this, cell3.m_district2, cell3.m_alpha2, ref c);
                            AddDistrictColor1(this, cell3.m_district3, cell3.m_alpha3, ref c);
                            AddDistrictColor1(this, cell3.m_district4, cell3.m_alpha4, ref c);
                            texture1.SetPixel(n, m, c);
                        }
                        bool inArea2 = false;
                        int num12 = (n + num6) / num5;
                        int num13 = (m + num6) / num5;
                        //begin mod
                        if (num12 >= 0 && num12 < FakeGameAreaManager.GRID && num13 >= 0 && num13 < FakeGameAreaManager.GRID)
                            inArea2 = (areaGrid[num13 * FakeGameAreaManager.GRID + num12] != 0);
                        //end mod
                        Color32 c2 = new Color32(128, 128, 128, 128);
                        AddDistrictColor2(this, cell3.m_district1, highlightPolicy, cell3.m_alpha1, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district2, highlightPolicy, cell3.m_alpha2, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district3, highlightPolicy, cell3.m_alpha3, inArea2, ref c2);
                        AddDistrictColor2(this, cell3.m_district4, highlightPolicy, cell3.m_alpha4, inArea2, ref c2);
                        texture2.SetPixel(n, m, c2);
                    }
                }
                if (fullUpdate)
                    texture1.Apply();
                texture2.Apply();
            }
        }

        [RedirectMethod]
        public void NamesModified()
        {
            //begin mod
            this.NamesModified(districtGrid);
            //end mod
            for (int index = 0; index < 128; ++index)
            {
                //begin mod
                int bestLocation = m_tempData[index].m_bestLocation;
                //end mod
                Vector3 worldPos;
                //begin mod
                worldPos.x = (float)(19.2f * (double)((int)bestLocation % HALFGRID) * 2.0 - 19.2f * HALFGRID);
                worldPos.y = 0.0f;
                worldPos.z = (float)(19.2f * (double)((int)bestLocation / HALFGRID) * 2.0 - 19.2f * HALFGRID);
                //end mod
                worldPos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(worldPos, false, 0.0f);
                this.m_districts.m_buffer[index].m_nameLocation = worldPos;
            }
            //begin mod
            namesModifiedField.SetValue(this, true);
        }

        [RedirectMethod]
        public void ParkNamesModified()
        {
            //begin mod
            this.NamesModified(parkGrid);
            //end mod
            for (int index = 0; index < 128; ++index)
            {
                //begin mod
                int bestLocation = m_tempData[index].m_bestLocation;
                //end mod
                Vector3 worldPos;
                //begin mod
                worldPos.x = (float)(19.2f * (double)((int)bestLocation % HALFGRID) * 2.0 - 19.2f * HALFGRID);
                worldPos.y = 0.0f;
                worldPos.z = (float)(19.2f * (double)((int)bestLocation / HALFGRID) * 2.0 - 19.2f * HALFGRID);
                //end mod
                worldPos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(worldPos, false, 0.0f);
                this.m_parks.m_buffer[index].m_nameLocation = worldPos;
            }
            namesModifiedField.SetValue(this, true);
            //end mod
        }

        [RedirectMethod]
        private void NamesModified(DistrictManager.Cell[] grid)
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
                    byte district = grid[num6].m_district1;
                    if (district != 0 && (l == 0 || k == 0 || l == HALFGRID - 1 || k == HALFGRID - 1 || grid[num6 - num3].m_district1 != district || grid[num6 - num2].m_district1 != district || grid[num6 + num2].m_district1 != district || grid[num6 + num3].m_district1 != district))
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
                byte district2 = grid[num11].m_district1;
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
                if (num9 > 0 && distanceBuffer[num15] == 0 && grid[num11 - num2].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                //end mod
                num15 = num8 + 1;
                //begin mod
                if (num9 < HALFGRID - 1 && distanceBuffer[num15] == 0 && grid[num11 + num2].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 - HALFGRID;
                if (num10 > 0 && distanceBuffer[num15] == 0 && grid[num11 - num3].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                num15 = num8 + HALFGRID;
                if (num10 < HALFGRID - 1 && distanceBuffer[num15] == 0 && grid[num11 + num3].m_district1 == district2)
                {
                    distanceBuffer[num15] = (distanceBuffer[num8] + 1);
                    indexBuffer[num5] = num15;
                    num5 = ((num5 + 1) % (HALFGRID * HALFGRID));
                }
                //end mod
            }
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

        [RedirectMethod]
        public void GetParkArea(byte park, out int minX, out int minZ, out int maxX, out int maxZ)
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
                    DistrictManager.Cell cell = parkGrid[index1 * GRID + index2];
                    //end mod
                    if ((int)cell.m_alpha1 != 0 && (int)cell.m_district1 == (int)park)
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
                    else if ((int)cell.m_alpha2 != 0 && (int)cell.m_district2 == (int)park)
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
                    else if ((int)cell.m_alpha3 != 0 && (int)cell.m_district3 == (int)park)
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
                    else if ((int)cell.m_alpha4 != 0 && (int)cell.m_district4 == (int)park)
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
        public byte GetPark(Vector3 worldPos)
        {
            //begin mod
            int num = Mathf.Clamp((int)((double)worldPos.x / 19.2f + HALFGRID), 0, GRID - 1);
            return parkGrid[Mathf.Clamp((int)((double)worldPos.z / 19.2f + HALFGRID), 0, GRID - 1) * GRID + num].m_district1;
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
                FakeDistrictManager.Exchange(ref cell.m_alpha1, ref cell.m_alpha2, ref cell.m_district1, ref cell.m_district2);
            if ((int)cell.m_alpha3 > (int)cell.m_alpha1)
                FakeDistrictManager.Exchange(ref cell.m_alpha1, ref cell.m_alpha3, ref cell.m_district1, ref cell.m_district3);
            if ((int)cell.m_alpha4 > (int)cell.m_alpha1)
                FakeDistrictManager.Exchange(ref cell.m_alpha1, ref cell.m_alpha4, ref cell.m_district1, ref cell.m_district4);
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
