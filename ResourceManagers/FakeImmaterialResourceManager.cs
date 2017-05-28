using System;
using System.Reflection;
using ColossalFramework;
using System.Threading;
using ColossalFramework.Math;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

//TODO(earalov): review this class
namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(ImmaterialResourceManager))]
    class FakeImmaterialResourceManager
    {
        public const int GRID = 450;
        public const int HALFGRID = 225;

        private static ushort[] m_localFinalResources;
        private static ushort[] m_localTempResources;
        private static Texture2D m_resourceTexture;
        private static int[] m_globalFinalResources;
        private static int[] m_globalTempResources;
        private static int[] m_totalFinalResources;
        private static int[] m_totalTempResources;
        private static long[] m_totalTempResourcesMul;
        private static int[] m_modifiedX1;
        private static int[] m_modifiedX2;
        private static bool m_modified;
        private static int[] m_tempCircleMinX;
        private static int[] m_tempCircleMaxX;
        private static float[] m_tempSectorSlopes;
        private static float[] m_tempSectorDistances;

        private static FieldInfo _buildingThemesPositionField;
        private static readonly string BUILDING_THEMES_MOD_TYPE = "BuildingThemesMod";
        private static readonly string BUILDING_THEMES_POSITION_FIELD = "position";


        public static void Init()
        {
            _buildingThemesPositionField = null;
            if (Util.IsModActive(Mod.BUILDING_THEMES_MOD))
            {
                var buildingThemesType = Util.FindType(BUILDING_THEMES_MOD_TYPE);
                if (buildingThemesType != null)
                {
                    _buildingThemesPositionField = buildingThemesType.GetField(BUILDING_THEMES_POSITION_FIELD,
                        BindingFlags.Public | BindingFlags.Static);
                }
            }

            m_localFinalResources = new ushort[GRID * GRID * 24];
            m_localTempResources = new ushort[GRID * GRID * 24];
            m_globalFinalResources = (int[])typeof(ImmaterialResourceManager).GetField("m_globalFinalResources", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ImmaterialResourceManager.instance);
            m_globalTempResources = (int[])typeof(ImmaterialResourceManager).GetField("m_globalTempResources", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ImmaterialResourceManager.instance);
            m_totalFinalResources = (int[])typeof(ImmaterialResourceManager).GetField("m_totalFinalResources", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ImmaterialResourceManager.instance);
            m_totalTempResources = (int[])typeof(ImmaterialResourceManager).GetField("m_totalTempResources", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ImmaterialResourceManager.instance);
            m_totalTempResourcesMul = (long[])typeof(ImmaterialResourceManager).GetField("m_totalTempResourcesMul", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ImmaterialResourceManager.instance);
            m_modifiedX1 = new int[GRID];
            m_modifiedX2 = new int[GRID];
            m_tempCircleMinX = new int[GRID];
            m_tempCircleMaxX = new int[GRID];
            m_tempSectorSlopes = new float[GRID];
            m_tempSectorDistances = new float[GRID];


            for (int index = 0; index < GRID; ++index)
            {
                m_modifiedX1[index] = 0;
                m_modifiedX2[index] = GRID - 1;
            }
            m_modified = true;
            m_resourceTexture = new Texture2D(GRID, GRID, TextureFormat.Alpha8, false, true);
            m_resourceTexture.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture("_ImmaterialResources", m_resourceTexture);
            ImmaterialResourceManager.instance.AreaModified(0, 0, GRID, GRID);
        }

        [RedirectReverse(true)]
        private static void CalculateTotalResources(int[] buffer, long[] bufferMul, int[] target)
        {
            UnityEngine.Debug.Log($"{buffer}+{bufferMul}+{target}");
        }

        [RedirectReverse(true)]
        private static void AddResource(ImmaterialResourceManager manager, ref ushort buffer, int rate)
        {
            UnityEngine.Debug.Log($"{manager}-{buffer}-{rate}");
        }

        public static void OnDestroy()
        {
            if (m_resourceTexture != null)
            {
                UnityEngine.Object.Destroy(m_resourceTexture);
                m_resourceTexture = null;
            }
        }

        [RedirectMethod]
        private void UpdateResourceMapping()
        {
            Vector4 vec;
            vec.z = 1 / (38.4f * GRID);
            vec.x = 0.5f;
            vec.y = 0.5f;
            vec.w = 0.25f;
            Shader.SetGlobalVector("_ImmaterialResourceMapping", vec);
            if (ImmaterialResourceManager.instance.ResourceMapVisible != ImmaterialResourceManager.Resource.None)
            {
                AreaModified(0, 0, GRID, GRID);
                UpdateTexture();
                Shader.EnableKeyword("RESOURCE_OVERLAY_IMMATERIAL");
                Shader.SetGlobalColor("_ImmaterialResourceColor", ImmaterialResourceManager.instance.m_properties.m_resourceColors[(int)ImmaterialResourceManager.instance.ResourceMapVisible].linear);
            }
            else
            {
                Shader.DisableKeyword("RESOURCE_OVERLAY_IMMATERIAL");
            }
        }

        [RedirectMethod]
        private void LateUpdate()
        {
            //begin mod
            if (!Singleton<LoadingManager>.instance.m_loadingComplete || ImmaterialResourceManager.instance.ResourceMapVisible == ImmaterialResourceManager.Resource.None || !m_modified)
                return;
            m_modified = false;
            //end mod
            this.UpdateTexture();
        }

        [RedirectMethod]
        private void UpdateTexture()
        {
            //begin mod
            for (int index = 0; index < GRID; ++index)
            {
                if (m_modifiedX2[index] >= m_modifiedX1[index])
                {
                    do
                        ;
                    while (!Monitor.TryEnter((object)m_localFinalResources, SimulationManager.SYNCHRONIZE_TIMEOUT));
                    int num1;
                    int num2;
                    try
                    {
                        num1 = m_modifiedX1[index];
                        num2 = m_modifiedX2[index];
                        m_modifiedX1[index] = 10000;
                        m_modifiedX2[index] = -10000;
                    }
                    finally
                    {
                        Monitor.Exit((object)m_localFinalResources);
                    }
                    //end mod

                    for (int x = num1; x <= num2; ++x)
                    {
                        int amount = 0;
                        this.AddLocalResource(x - 1, index - 1, 5, ref amount);
                        this.AddLocalResource(x, index - 1, 7, ref amount);
                        this.AddLocalResource(x + 1, index - 1, 5, ref amount);
                        this.AddLocalResource(x - 1, index, 7, ref amount);
                        this.AddLocalResource(x, index, 14, ref amount);
                        this.AddLocalResource(x + 1, index, 7, ref amount);
                        this.AddLocalResource(x - 1, index + 1, 5, ref amount);
                        this.AddLocalResource(x, index + 1, 7, ref amount);
                        this.AddLocalResource(x + 1, index + 1, 5, ref amount);
                        float num3 = Mathf.Clamp01(Mathf.Sqrt((float)amount * 0.0001612903f));
                        Color color;
                        color.r = num3;
                        color.g = num3;
                        color.b = num3;
                        color.a = num3;
                        //begin mod
                        m_resourceTexture.SetPixel(x, index, color);
                        //end mod
                    }
                }
            }
            //begin mod
            m_resourceTexture.Apply();
            //end mod
        }

        [RedirectMethod]
        private void AddLocalResource(int x, int z, int multiplier, ref int amount)
        {
            x = Mathf.Clamp(x, 0, GRID - 1);
            z = Mathf.Clamp(z, 0, GRID - 1);
            int num = (int)((z * GRID + x) * 24 + ImmaterialResourceManager.instance.ResourceMapVisible);
            amount += (int)m_localFinalResources[num] * multiplier;
        }

        [RedirectMethod]
        public void AreaModified(int minX, int minZ, int maxX, int maxZ)
        {
            minX = Mathf.Max(0, minX - 1);
            minZ = Mathf.Max(0, minZ - 1);
            maxX = Mathf.Min(GRID - 1, maxX + 1);
            maxZ = Mathf.Min(GRID - 1, maxZ + 1);
            while (!Monitor.TryEnter(m_localFinalResources, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                for (int i = minZ; i <= maxZ; i++)
                {
                    m_modifiedX1[i] = Mathf.Min(m_modifiedX1[i], minX);
                    m_modifiedX2[i] = Mathf.Max(m_modifiedX2[i], maxX);
                }
                m_modified = true;
            }
            finally
            {
                Monitor.Exit(m_localFinalResources);
            }
        }

        [RedirectMethod]
        public int AddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 position, float radius)
        {
            //begin mod
            if (_buildingThemesPositionField != null)
            {
                if (resource == ImmaterialResourceManager.Resource.Abandonment)
                {
                    _buildingThemesPositionField.SetValue(null, position);
                }
            }
            //end mod

            if (rate == 0)
            {
                return 0;
            }
            float num = Mathf.Max(0f, Mathf.Min(radius, 19.2f));
            float num2 = Mathf.Max(38.4f, radius + 19.2f);
            int num3 = Mathf.Max((int)((position.x - radius) / 38.4f + HALFGRID), 2);
            int num4 = Mathf.Max((int)((position.z - radius) / 38.4f + HALFGRID), 2);
            int num5 = Mathf.Min((int)((position.x + radius) / 38.4f + HALFGRID), GRID - 3);
            int num6 = Mathf.Min((int)((position.z + radius) / 38.4f + HALFGRID), GRID - 3);
            for (int i = num4; i <= num6; i++)
            {
                float num7 = ((float)i - HALFGRID + 0.5f) * 38.4f - position.z;
                for (int j = num3; j <= num5; j++)
                {
                    float num8 = ((float)j - HALFGRID + 0.5f) * 38.4f - position.x;
                    float num9 = num7 * num7 + num8 * num8;
                    if (num9 < num2 * num2)
                    {
                        int num10 = rate;
                        if (num9 > num * num)
                        {
                            float num11 = Mathf.Clamp01((num2 - Mathf.Sqrt(num9)) / (num2 - num));
                            num10 = Mathf.RoundToInt((float)num10 * num11);
                        }
                        int num12 = (int)((i * GRID + j) * 24 + resource);
                        AddResource(ImmaterialResourceManager.instance, ref m_localTempResources[num12], num10);
                    }
                }
            }
            return rate;
        }

        [RedirectMethod]
        public int AddObstructedResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 position, float radius)
        {
            if (rate == 0)
                return 0;
            ushort[] finalHeights = Singleton<TerrainManager>.instance.FinalHeights;
            //begin mod
            for (int index = 0; index < GRID; ++index)
            {
                //end mod
                m_tempSectorSlopes[index] = -100f;
                m_tempSectorDistances[index] = 0.0f;
            }
            float num1 = radius * 0.5f;
            float num2 = Mathf.Max(38.4f, radius + 19.2f);
            int num3 = (int)resource;
            //begin mod
            int num4 = Mathf.Clamp((int)((double)position.x / 38.4000015258789 + HALFGRID), 2, GRID - 3);
            //end mod
            int num5 = num4;
            //begin mod
            int num6 = Mathf.Clamp((int)((double)position.z / 38.4000015258789 + HALFGRID), 2, GRID - 3);
            //end mod
            int num7 = num6;
            //begin mod
            float num8 = position.x - (float)(((double)num4 - HALFGRID + 0.5) * 38.4000015258789);
            float num9 = position.z - (float)(((double)num6 - HALFGRID + 0.5) * 38.4000015258789);
            //end mod
            if ((double)num8 > 9.60000038146973)
                //begin mod
                num5 = Mathf.Min(num5 + 1, GRID - 3);
                //end mod
            else if ((double)num8 < -9.60000038146973)
                num4 = Mathf.Max(num4 - 1, 2);
            if ((double)num9 > 9.60000038146973)
                //begin mod
                num7 = Mathf.Min(num7 + 1, GRID - 3);
                //end mod
            else if ((double)num9 < -9.60000038146973)
                num6 = Mathf.Max(num6 - 1, 2);
            int num10 = num6;
            int num11 = num7;
            int num12 = num10 + 1;
            int num13 = num11 - 1;
            int num14 = 0;
            bool flag;
            do
            {
                flag = false;
                float num15 = (float)(38.4000015258789 * (0.75 + (double)num14++));
                float num16 = num15 * num15;
                for (int index1 = num10; index1 <= num11; ++index1)
                {
                    int index2 = index1 > num6 ? index1 : num6 - index1 + num10;
                    //begin mod
                    float num17 = (float)(((double)index2 - HALFGRID + 0.5) * 38.4000015258789);
                    //end mod
                    float y = num17 - position.z;
                    int num18 = Mathf.Clamp(Mathf.RoundToInt((float)((double)num17 / 16.0 + 540.0)), 0, 1080);
                    int a1 = num4;
                    int a2 = num5;
                    if (index2 >= num12 && index2 <= num13)
                    {
                        a1 = Mathf.Min(a1, m_tempCircleMinX[index2] - 1);
                        a2 = Mathf.Max(a2, m_tempCircleMaxX[index2] + 1);
                    }
                    for (int index3 = a1; index3 >= 2; a1 = index3--)
                    {
                        //begin mod
                        float num19 = (float)(((double)index3 - HALFGRID + 0.5) * 38.4000015258789);
                        //end mod
                        float x = num19 - position.x;
                        int num20 = Mathf.Clamp(Mathf.RoundToInt((float)((double)num19 / 16.0 + 540.0)), 0, 1080);
                        float f1 = (float)((double)y * (double)y + (double)x * (double)x);
                        if ((a1 == index3 || (double)f1 < (double)num16) && (double)f1 < (double)num2 * (double)num2)
                        {
                            float b1 = Mathf.Sqrt(f1);
                            float num21 = (float)rate;
                            float num22 = (1f / 64f * (float)finalHeights[num18 * 1081 + num20] - position.y) / Mathf.Max(1f, b1);
                            float num23 = Mathf.Atan2(y, x) * 40.74366f;
                            float num24 = Mathf.Min((float)(38.4000015258789 / (double)Mathf.Max(1f, b1) * 20.3718318939209), 64f); //TODO: 64: halfgrid/2 ??
                            int num25 = Mathf.RoundToInt(num23 - num24) & (int)byte.MaxValue;
                            int num26 = Mathf.RoundToInt(num23 + num24) & (int)byte.MaxValue;
                            float num27 = 0.0f;
                            float b2 = 0.0f;
                            int index4 = num25;
                            while (true)
                            {
                                float num28 = m_tempSectorSlopes[index4];
                                float num29 = m_tempSectorDistances[index4];
                                float num30 = Mathf.Clamp(b1 - num29, 1f, 38.4f);
                                num27 += (num28 - num22) * num30;
                                b2 += num30;
                                if ((double)num22 > (double)num28)
                                {
                                    m_tempSectorSlopes[index4] = num22;
                                    m_tempSectorDistances[index4] = b1;
                                }
                                if (index4 != num26)
                                    index4 = index4 + 1 & (int)byte.MaxValue;
                                else
                                    break;
                            }
                            float num31 = num27 / Mathf.Max(1f, b2);
                            float f2 = num21 * (float)(1.5 / (double)Mathf.Max(1f, (float)((double)num31 * 20.0 + 2.625)) - 0.5);
                            if ((double)f2 > 0.0)
                            {
                                if ((double)b1 > (double)num1)
                                    f2 *= Mathf.Clamp01((float)(((double)num2 - (double)b1) / ((double)num2 - (double)num1)));
                                //begin mod
                                this.AddResource(ref m_localTempResources[(index2 * GRID + index3) * 24 + num3], Mathf.RoundToInt(f2));
                                //end mod
                            }
                            flag = true;
                        }
                        if ((double)f1 >= (double)num16)
                            break;
                    }
                    //begin mod
                    for (int index3 = a2; index3 <= GRID - 3; a2 = index3++)
                    {
                        float num19 = (float)(((double)index3 - HALFGRID + 0.5) * 38.4000015258789);
                        //end mod
                        float x = num19 - position.x;
                        int num20 = Mathf.Clamp(Mathf.RoundToInt((float)((double)num19 / 16.0 + 540.0)), 0, 1080);
                        float f1 = (float)((double)y * (double)y + (double)x * (double)x);
                        if ((a2 == index3 || (double)f1 < (double)num16) && (index3 != num4 && (double)f1 < (double)num2 * (double)num2))
                        {
                            float b1 = Mathf.Sqrt(f1);
                            float num21 = (float)rate;
                            float num22 = (1f / 64f * (float)finalHeights[num18 * 1081 + num20] - position.y) / Mathf.Max(1f, b1);
                            float num23 = Mathf.Atan2(y, x) * 40.74366f;
                            float num24 = Mathf.Min((float)(38.4000015258789 / (double)Mathf.Max(1f, b1) * 20.3718318939209), 64f); //TODO: 64: halfgrid/2 ??
                            int num25 = Mathf.RoundToInt(num23 - num24) & (int)byte.MaxValue;
                            int num26 = Mathf.RoundToInt(num23 + num24) & (int)byte.MaxValue;
                            float num27 = 0.0f;
                            float b2 = 0.0f;
                            int index4 = num25;
                            while (true)
                            {
                                float num28 = m_tempSectorSlopes[index4];
                                float num29 = m_tempSectorDistances[index4];
                                float num30 = Mathf.Clamp(b1 - num29, 1f, 38.4f);
                                num27 += (num28 - num22) * num30;
                                b2 += num30;
                                if ((double)num22 > (double)num28)
                                {
                                    m_tempSectorSlopes[index4] = num22;
                                    m_tempSectorDistances[index4] = b1;
                                }
                                if (index4 != num26)
                                    index4 = index4 + 1 & (int)byte.MaxValue;
                                else
                                    break;
                            }
                            float num31 = num27 / Mathf.Max(1f, b2);
                            float f2 = num21 * (float)(1.5 / (double)Mathf.Max(1f, (float)((double)num31 * 20.0 + 2.625)) - 0.5);
                            if ((double)f2 > 0.0)
                            {
                                if ((double)b1 > (double)num1)
                                    f2 *= Mathf.Clamp01((float)(((double)num2 - (double)b1) / ((double)num2 - (double)num1)));
                                //begin mod
                                this.AddResource(ref m_localTempResources[(index2 * GRID + index3) * 24 + num3], Mathf.RoundToInt(f2));
                                //end mod
                            }
                            flag = true;
                        }
                        if ((double)f1 >= (double)num16)
                            break;
                    }
                    m_tempCircleMinX[index2] = a1;
                    m_tempCircleMaxX[index2] = a2;
                }
                num12 = num10;
                num13 = num11;
                if (num10 > 2)
                    --num10;
                //begin mod
                if (num11 < GRID - 3)
                    //end mod
                    ++num11;
            }
            while (flag);
            return rate;
        }

        [RedirectMethod] //No changes here. This method is most likely inlined. It's added here just for compilation sake
        private void AddResource(ref ushort buffer, int rate)
        {
            buffer = (ushort)Mathf.Min((int)buffer + rate, (int)ushort.MaxValue);
        }

        [RedirectMethod]
        private static bool CalculateLocalResources(int x, int z, ushort[] buffer, int[] global, ushort[] target, int index)
        {
            int resourceRate1 = (int)buffer[index] + global[0];
            int resourceRate2 = (int)buffer[index + 1] + global[1];
            int resourceRate3 = (int)buffer[index + 2] + global[2];
            int resourceRate4 = (int)buffer[index + 3] + global[3];
            int resourceRate5 = (int)buffer[index + 4] + global[4];
            int resourceRate6 = (int)buffer[index + 5] + global[5];
            int resourceRate7 = (int)buffer[index + 6] + global[6];
            int resourceRate8 = (int)buffer[index + 7] + global[7];
            int resourceRate9 = (int)buffer[index + 8] + global[8];
            int num1 = (int)buffer[index + 9] + global[9];
            int num2 = (int)buffer[index + 10] + global[10];
            int num3 = (int)buffer[index + 11] + global[11];
            int a = (int)buffer[index + 12] + global[12];
            int resourceRate10 = (int)buffer[index + 13] + global[13];
            int num4 = (int)buffer[index + 14] + global[14];
            int num5 = (int)buffer[index + 15] + global[15];
            int num6 = (int)buffer[index + 16] + global[16];
            int num7 = (int)buffer[index + 17] + global[17];
            int resourceRate11 = (int)buffer[index + 18] + global[18];
            int resourceRate12 = (int)buffer[index + 19] + global[19];
            int resourceRate13 = (int)buffer[index + 20] + global[20];
            int resourceRate14 = (int)buffer[index + 21];
            int num8 = (int)buffer[index + 22] + global[22];
            int resourceRate15 = (int)buffer[index + 23] + global[23];
            //begin mod
            Rect area = new Rect((float)(((double)x - HALFGRID - 1.5) * 38.4f), (float)(((double)z - HALFGRID - 1.5) * 38.4f), 153.6f, 153.6f);
            //end mod
            float groundPollution;
            float waterProximity;
            Singleton<NaturalResourceManager>.instance.AveragePollutionAndWater(area, out groundPollution, out waterProximity);
            int num9 = (int)((double)groundPollution * 100.0);
            int num10 = (int)((double)waterProximity * 100.0);
            if (num10 > 33 && num10 < 99)
            {
                //begin mod
                area = new Rect((float)(((double)x - HALFGRID + 0.25) * 38.4000015258789), (float)(((double)z - HALFGRID + 0.25) * 38.4000015258789), 19.2f, 19.2f);
                //end mod
                Singleton<NaturalResourceManager>.instance.AveragePollutionAndWater(area, out groundPollution, out waterProximity);
                num10 = Mathf.Max(Mathf.Min(num10, (int)((double)waterProximity * 100.0)), 33);
            }
            int resourceRate16 = num7 * 2 / (resourceRate2 + 50);
            int resourceRate17;
            int resourceRate18;
            int resourceRate19;
            if (a == 0)
            {
                resourceRate17 = 0;
                resourceRate18 = 50;
                resourceRate19 = 50;
            }
            else
            {
                resourceRate17 = num1 / a;
                resourceRate18 = num2 / a;
                resourceRate19 = num3 / a;
                num6 += Mathf.Min(a, 10) * 10;
            }
            //begin mod
            int num11 = ((Singleton<GameAreaManager>.instance.PointOutOfArea(VectorUtils.X_Y(area.center)) ? 1 : 0) | (x <= 1 || x >= GRID - 2 || z <= 1 ? 1 : (z >= GRID - 2 ? 1 : 0))) == 0 ? (num4 + ImmaterialResourceManager.CalculateResourceEffect(resourceRate1, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate3, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate2, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate4, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate5, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate6, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate7, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate8, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate12, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate10, 100, 500, 100, 200) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate19, 60, 100, 0, 50) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate18, 60, 100, 0, 50) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate13, 50, 100, 20, 25) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate15, 50, 100, 20, 25) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate14, 100, 1000, 0, 25) - ImmaterialResourceManager.CalculateResourceEffect(100 - resourceRate19, 60, 100, 0, 50) - ImmaterialResourceManager.CalculateResourceEffect(100 - resourceRate18, 60, 100, 0, 50) - ImmaterialResourceManager.CalculateResourceEffect(num9, 50, (int)byte.MaxValue, 50, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate9, 10, 100, 0, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate17, 10, 100, 0, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate16, 50, 100, 10, 50) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate11, 15, 50, 100, 200) + (ImmaterialResourceManager.CalculateResourceEffect(num10, 33, 67, 300, 0) * Mathf.Max(0, 32 - num9) >> 5)) / 10 : 0;
            //end mod
            int num12 = Mathf.Clamp(resourceRate1, 0, (int)ushort.MaxValue);
            int num13 = Mathf.Clamp(resourceRate2, 0, (int)ushort.MaxValue);
            int num14 = Mathf.Clamp(resourceRate3, 0, (int)ushort.MaxValue);
            int num15 = Mathf.Clamp(resourceRate4, 0, (int)ushort.MaxValue);
            int num16 = Mathf.Clamp(resourceRate5, 0, (int)ushort.MaxValue);
            int num17 = Mathf.Clamp(resourceRate6, 0, (int)ushort.MaxValue);
            int num18 = Mathf.Clamp(resourceRate7, 0, (int)ushort.MaxValue);
            int num19 = Mathf.Clamp(resourceRate8, 0, (int)ushort.MaxValue);
            int num20 = Mathf.Clamp(resourceRate9, 0, (int)ushort.MaxValue);
            int num21 = Mathf.Clamp(resourceRate17, 0, (int)ushort.MaxValue);
            int num22 = Mathf.Clamp(resourceRate18, 0, (int)ushort.MaxValue);
            int num23 = Mathf.Clamp(resourceRate19, 0, (int)ushort.MaxValue);
            int num24 = Mathf.Clamp(a, 0, (int)ushort.MaxValue);
            int num25 = Mathf.Clamp(resourceRate10, 0, (int)ushort.MaxValue);
            int landvalue = Mathf.Clamp(num11, 0, (int)ushort.MaxValue);
            int num26 = Mathf.Clamp(num5, 0, (int)ushort.MaxValue);
            int coverage = Mathf.Clamp(num6, 0, (int)ushort.MaxValue);
            int num27 = Mathf.Clamp(resourceRate16, 0, (int)ushort.MaxValue);
            int num28 = Mathf.Clamp(resourceRate11, 0, (int)ushort.MaxValue);
            int num29 = Mathf.Clamp(resourceRate12, 0, (int)ushort.MaxValue);
            int num30 = Mathf.Clamp(resourceRate13, 0, (int)ushort.MaxValue);
            int num31 = Mathf.Clamp(resourceRate14, 0, (int)ushort.MaxValue);
            int num32 = Mathf.Clamp(num8, 0, (int)ushort.MaxValue);
            int num33 = Mathf.Clamp(resourceRate15, 0, (int)ushort.MaxValue);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            //begin mod
            byte district = instance.GetDistrict(x * FakeDistrictManager.GRID / GRID, z * FakeDistrictManager.GRID / GRID);
            //end mod
            instance.m_districts.m_buffer[(int)district].AddGroundData(landvalue, num9, coverage);
            bool flag = false;
            if (num12 != (int)target[index])
            {
                target[index] = (ushort)num12;
                flag = true;
            }
            if (num13 != (int)target[index + 1])
            {
                target[index + 1] = (ushort)num13;
                flag = true;
            }
            if (num14 != (int)target[index + 2])
            {
                target[index + 2] = (ushort)num14;
                flag = true;
            }
            if (num15 != (int)target[index + 3])
            {
                target[index + 3] = (ushort)num15;
                flag = true;
            }
            if (num16 != (int)target[index + 4])
            {
                target[index + 4] = (ushort)num16;
                flag = true;
            }
            if (num17 != (int)target[index + 5])
            {
                target[index + 5] = (ushort)num17;
                flag = true;
            }
            if (num18 != (int)target[index + 6])
            {
                target[index + 6] = (ushort)num18;
                flag = true;
            }
            if (num19 != (int)target[index + 7])
            {
                target[index + 7] = (ushort)num19;
                flag = true;
            }
            if (num20 != (int)target[index + 8])
            {
                target[index + 8] = (ushort)num20;
                flag = true;
            }
            if (num21 != (int)target[index + 9])
            {
                target[index + 9] = (ushort)num21;
                flag = true;
            }
            if (num22 != (int)target[index + 10])
            {
                target[index + 10] = (ushort)num22;
                flag = true;
            }
            if (num23 != (int)target[index + 11])
            {
                target[index + 11] = (ushort)num23;
                flag = true;
            }
            if (num24 != (int)target[index + 12])
            {
                target[index + 12] = (ushort)num24;
                flag = true;
            }
            if (num25 != (int)target[index + 13])
            {
                target[index + 13] = (ushort)num25;
                flag = true;
            }
            if (landvalue != (int)target[index + 14])
            {
                target[index + 14] = (ushort)landvalue;
                flag = true;
            }
            if (num26 != (int)target[index + 15])
            {
                target[index + 15] = (ushort)num26;
                flag = true;
            }
            if (coverage != (int)target[index + 16])
            {
                target[index + 16] = (ushort)coverage;
                flag = true;
            }
            if (num27 != (int)target[index + 17])
            {
                target[index + 17] = (ushort)num27;
                flag = true;
            }
            if (num28 != (int)target[index + 18])
            {
                target[index + 18] = (ushort)num28;
                flag = true;
            }
            if (num29 != (int)target[index + 19])
            {
                target[index + 19] = (ushort)num29;
                flag = true;
            }
            if (num30 != (int)target[index + 20])
            {
                target[index + 20] = (ushort)num30;
                flag = true;
            }
            if (num31 != (int)target[index + 21])
            {
                target[index + 21] = (ushort)num31;
                flag = true;
            }
            if (num32 != (int)target[index + 22])
            {
                target[index + 22] = (ushort)num32;
                flag = true;
            }
            if (num33 != (int)target[index + 23])
            {
                target[index + 23] = (ushort)num33;
                flag = true;
            }
            return flag;
        }

        [RedirectMethod]
        public void CheckResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local, out int total)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID + num) * 24 + resource);
            local = (int)m_localFinalResources[num3];
            total = m_totalFinalResources[(int)resource];
        }

        [RedirectMethod]
        public void CheckLocalResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID + num) * 24 + resource);
            local = (int)m_localFinalResources[num3];
        }

        [RedirectMethod]
        public void CheckLocalResources(Vector3 position, out ushort[] resources, out int index)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            index = (num2 * GRID + num) * 24;
            resources = m_localFinalResources;
        }

        [RedirectMethod]
        protected void SimulationStepImpl(int subStep)
        {
            if (subStep == 0 || subStep == 1000)
                return;
            //all managers refresh at that frequency. We must not change that value!
            int num1 = (int)Singleton<SimulationManager>.instance.m_currentFrameIndex & (int)byte.MaxValue;
            //begin mod
            int num2 = num1 * 2; //two lines at single time. 256*2 = 512 > 450
            int minX = -1;
            int maxX = -1;
            int minZ = -1;
            int maxZ = -1;

            for (int z = num2; z <= num2 + 1; z++)
            {
                if (z > GRID - 1)
                {
                    continue;
                }
                int num3 = 0;
                int num4 = GRID - 1;
                for (int x = num3; x <= num4; ++x)
                {
                    int index1 = (z * GRID + x) * 24;
                    if (CalculateLocalResources(x, z, m_localTempResources, m_globalFinalResources, m_localFinalResources, index1))
                    {
                        minX = minX == -1 ? x : Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minZ = minZ == -1 ? z : Math.Min(minZ, z);
                        maxZ = Math.Max(maxZ, z);
                    }
                    int num5 = (int)m_localFinalResources[index1 + 16];
                    for (int index2 = 0; index2 < 24; ++index2)
                    {
                        int num6 = (int)m_localFinalResources[index1 + index2];
                        m_totalTempResources[index2] += num6;
                        m_totalTempResourcesMul[index2] += (long) (num6 * num5);
                        m_localTempResources[index1 + index2] = (ushort)0;
                    }
                }
            }
            //end mod
            if (num1 == byte.MaxValue) //all managers refresh at that frequency. We must not change that value!
            {
                //begin mod
                CalculateTotalResources(m_totalTempResources, m_totalTempResourcesMul, m_totalFinalResources);
                //end mod
                StatisticBase statisticBase = Singleton<StatisticsManager>.instance.Acquire<StatisticArray>(StatisticType.ImmaterialResource);
                for (int index = 0; index < 24; ++index)
                {
                    //begin mod
                    m_globalFinalResources[index] = m_globalTempResources[index];
                    m_globalTempResources[index] = 0;
                    m_totalTempResources[index] = 0;
                    m_totalTempResourcesMul[index] = 0;
                    statisticBase.Acquire<StatisticInt32>(index, 24).Set(m_totalFinalResources[index]);
                    //end mod
                }
            }
            if (minX == -1)
                return;
            //begin mod
            this.AreaModified(minX, minZ, maxX, maxZ);
            //end mod
        }
    }
}
