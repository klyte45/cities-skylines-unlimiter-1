using System;
using System.Reflection;
using ColossalFramework;
using System.Threading;
using ColossalFramework.Math;
using UnityEngine;
using EightyOne.Redirection;

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
        private static int[] m_totalTempResourcesMul;
        private static int[] m_modifiedX1;
        private static int[] m_modifiedX2;
        private static bool m_modified;
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

            m_localFinalResources = new ushort[GRID * GRID * 20];
            m_localTempResources = new ushort[GRID * GRID * 20];
            m_globalFinalResources = new int[20];
            m_globalTempResources = new int[20];
            m_totalFinalResources = new int[20];
            m_totalTempResources = new int[20];
            m_totalTempResourcesMul = new int[20];
            m_modifiedX1 = new int[GRID];
            m_modifiedX2 = new int[GRID];
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
            int num = (int)((z * GRID + x) * 20 + ImmaterialResourceManager.instance.ResourceMapVisible);
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

        //no changes
        private void AddResource(ref ushort buffer, int rate)
        {
            buffer = (ushort)Mathf.Min((int)buffer + rate, (int)ushort.MaxValue);
        }

        //no changes
        private void AddResource(ref int buffer, int rate)
        {
            buffer = Mathf.Min(buffer + rate, int.MaxValue);
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
                        int num12 = (int)((i * GRID + j) * 20 + resource);
                        AddResource(ref m_localTempResources[num12], num10);
                    }
                }
            }
            return rate;
        }

        [RedirectMethod]
        public int AddResource(ImmaterialResourceManager.Resource resource, int rate)
        {
            if (rate == 0)
                return 0;
            //begin mod
            this.AddResource(ref m_globalTempResources[(int)resource], rate);
            //end mod
            return rate;
        }

        [RedirectMethod]
        public static int CalculateResourceEffect(int resourceRate, int middleRate, int maxRate, int middleEffect, int maxEffect)
        {
            if (resourceRate <= 0)
            {
                return 0;
            }
            if (resourceRate < middleRate)
            {
                return middleEffect * resourceRate / middleRate;
            }
            if (resourceRate < maxRate)
            {
                return middleEffect + (maxEffect - middleEffect) * (resourceRate - middleRate) / (maxRate - middleRate);
            }
            return maxEffect;
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
            //begin mod
            Rect area = new Rect((float)(((double)x - HALFGRID - 1.5) * 38.4f), (float)(((double)z - HALFGRID - 1.5) * 38.4f), 153.6f, 153.6f);
            //end mod
            float groundPollution;
            float waterProximity;
            Singleton<NaturalResourceManager>.instance.AveragePollutionAndWater(area, out groundPollution, out waterProximity);
            int num8 = (int)((double)groundPollution * 100.0);
            int resourceRate13 = (int)((double)waterProximity * 100.0);
            int resourceRate14 = num7 * 2 / (resourceRate2 + 50);
            int resourceRate15;
            int resourceRate16;
            int resourceRate17;
            if (a == 0)
            {
                resourceRate15 = 0;
                resourceRate16 = 50;
                resourceRate17 = 50;
            }
            else
            {
                resourceRate15 = num1 / a;
                resourceRate16 = num2 / a;
                resourceRate17 = num3 / a;
                num6 += Mathf.Min(a, 10) * 10;
            }
            //begin mod
            int num9 = ((Singleton<GameAreaManager>.instance.PointOutOfArea(VectorUtils.X_Y(area.center)) ? 1 : 0) | (x <= 1 || x >= GRID - 1 || z <= 1 ? 1 : (z >= GRID - 1 ? 1 : 0))) == 0 ? (num4 + ImmaterialResourceManager.CalculateResourceEffect(resourceRate1, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate3, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate2, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate4, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate5, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate6, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate7, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate8, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate12, 100, 500, 50, 100) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate10, 100, 500, 100, 200) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate17, 60, 100, 0, 50) + ImmaterialResourceManager.CalculateResourceEffect(resourceRate16, 60, 100, 0, 50) - ImmaterialResourceManager.CalculateResourceEffect(100 - resourceRate17, 60, 100, 0, 50) - ImmaterialResourceManager.CalculateResourceEffect(100 - resourceRate16, 60, 100, 0, 50) - ImmaterialResourceManager.CalculateResourceEffect(num8, 50, (int)byte.MaxValue, 50, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate9, 10, 100, 0, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate15, 10, 100, 0, 100) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate14, 50, 100, 10, 50) - ImmaterialResourceManager.CalculateResourceEffect(resourceRate11, 15, 50, 100, 200) + (ImmaterialResourceManager.CalculateResourceEffect(resourceRate13, 33, 67, 300, 0) * Mathf.Max(0, 32 - num8) >> 5)) / 10 : 0;
            //end mod
            int num10 = Mathf.Clamp(resourceRate1, 0, (int)ushort.MaxValue);
            int num11 = Mathf.Clamp(resourceRate2, 0, (int)ushort.MaxValue);
            int num12 = Mathf.Clamp(resourceRate3, 0, (int)ushort.MaxValue);
            int num13 = Mathf.Clamp(resourceRate4, 0, (int)ushort.MaxValue);
            int num14 = Mathf.Clamp(resourceRate5, 0, (int)ushort.MaxValue);
            int num15 = Mathf.Clamp(resourceRate6, 0, (int)ushort.MaxValue);
            int num16 = Mathf.Clamp(resourceRate7, 0, (int)ushort.MaxValue);
            int num17 = Mathf.Clamp(resourceRate8, 0, (int)ushort.MaxValue);
            int num18 = Mathf.Clamp(resourceRate9, 0, (int)ushort.MaxValue);
            int num19 = Mathf.Clamp(resourceRate15, 0, (int)ushort.MaxValue);
            int num20 = Mathf.Clamp(resourceRate16, 0, (int)ushort.MaxValue);
            int num21 = Mathf.Clamp(resourceRate17, 0, (int)ushort.MaxValue);
            int num22 = Mathf.Clamp(a, 0, (int)ushort.MaxValue);
            int num23 = Mathf.Clamp(resourceRate10, 0, (int)ushort.MaxValue);
            int landvalue = Mathf.Clamp(num9, 0, (int)ushort.MaxValue);
            int num24 = Mathf.Clamp(num5, 0, (int)ushort.MaxValue);
            int coverage = Mathf.Clamp(num6, 0, (int)ushort.MaxValue);
            int num25 = Mathf.Clamp(resourceRate14, 0, (int)ushort.MaxValue);
            int num26 = Mathf.Clamp(resourceRate11, 0, (int)ushort.MaxValue);
            int num27 = Mathf.Clamp(resourceRate12, 0, (int)ushort.MaxValue);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            //begin mod
            byte district = instance.GetDistrict(x * FakeDistrictManager.GRID / GRID, z * FakeDistrictManager.GRID / GRID);
            //end mod
            instance.m_districts.m_buffer[(int)district].AddGroundData(landvalue, num8, coverage);
            bool flag = false;
            if (num10 != (int)target[index])
            {
                target[index] = (ushort)num10;
                flag = true;
            }
            if (num11 != (int)target[index + 1])
            {
                target[index + 1] = (ushort)num11;
                flag = true;
            }
            if (num12 != (int)target[index + 2])
            {
                target[index + 2] = (ushort)num12;
                flag = true;
            }
            if (num13 != (int)target[index + 3])
            {
                target[index + 3] = (ushort)num13;
                flag = true;
            }
            if (num14 != (int)target[index + 4])
            {
                target[index + 4] = (ushort)num14;
                flag = true;
            }
            if (num15 != (int)target[index + 5])
            {
                target[index + 5] = (ushort)num15;
                flag = true;
            }
            if (num16 != (int)target[index + 6])
            {
                target[index + 6] = (ushort)num16;
                flag = true;
            }
            if (num17 != (int)target[index + 7])
            {
                target[index + 7] = (ushort)num17;
                flag = true;
            }
            if (num18 != (int)target[index + 8])
            {
                target[index + 8] = (ushort)num18;
                flag = true;
            }
            if (num19 != (int)target[index + 9])
            {
                target[index + 9] = (ushort)num19;
                flag = true;
            }
            if (num20 != (int)target[index + 10])
            {
                target[index + 10] = (ushort)num20;
                flag = true;
            }
            if (num21 != (int)target[index + 11])
            {
                target[index + 11] = (ushort)num21;
                flag = true;
            }
            if (num22 != (int)target[index + 12])
            {
                target[index + 12] = (ushort)num22;
                flag = true;
            }
            if (num23 != (int)target[index + 13])
            {
                target[index + 13] = (ushort)num23;
                flag = true;
            }
            if (landvalue != (int)target[index + 14])
            {
                target[index + 14] = (ushort)landvalue;
                flag = true;
            }
            if (num24 != (int)target[index + 15])
            {
                target[index + 15] = (ushort)num24;
                flag = true;
            }
            if (coverage != (int)target[index + 16])
            {
                target[index + 16] = (ushort)coverage;
                flag = true;
            }
            if (num25 != (int)target[index + 17])
            {
                target[index + 17] = (ushort)num25;
                flag = true;
            }
            if (num26 != (int)target[index + 18])
            {
                target[index + 18] = (ushort)num26;
                flag = true;
            }
            if (num27 != (int)target[index + 19])
            {
                target[index + 19] = (ushort)num27;
                flag = true;
            }
            return flag;
        }

        //no changes
        private static void CalculateTotalResources(int[] buffer, int[] bufferMul, int[] target)
        {
            int num1 = buffer[0];
            int num2 = buffer[1];
            int num3 = buffer[2];
            int num4 = buffer[3];
            int num5 = buffer[4];
            int num6 = buffer[5];
            int num7 = buffer[6];
            int num8 = buffer[7];
            int num9 = buffer[8];
            int num10 = buffer[9];
            int num11 = buffer[10];
            int num12 = buffer[11];
            int num13 = buffer[12];
            int num14 = buffer[13];
            int num15 = buffer[14];
            int num16 = buffer[15];
            int num17 = buffer[16];
            int num18 = buffer[17];
            int num19 = buffer[18];
            int num20 = buffer[19];
            int num21 = bufferMul[0];
            int num22 = bufferMul[1];
            int num23 = bufferMul[2];
            int num24 = bufferMul[3];
            int num25 = bufferMul[4];
            int num26 = bufferMul[5];
            int num27 = bufferMul[6];
            int num28 = bufferMul[7];
            int num29 = bufferMul[8];
            int num30 = bufferMul[9];
            int num31 = bufferMul[10];
            int num32 = bufferMul[11];
            int num33 = bufferMul[12];
            int num34 = bufferMul[13];
            int num35 = bufferMul[14];
            int num36 = bufferMul[15];
            int num37 = bufferMul[17];
            int num38 = bufferMul[18];
            int num39 = bufferMul[19];
            int num40;
            int num41;
            int num42;
            int num43;
            int num44;
            int num45;
            int num46;
            int num47;
            int num48;
            int num49;
            int num50;
            int num51;
            int num52;
            int num53;
            int num54;
            int num55;
            int num56;
            int num57;
            if (num17 != 0)
            {
                num40 = num21 / num17;
                num41 = num22 / num17;
                num42 = num23 / num17;
                num43 = num24 / num17;
                num44 = num25 / num17;
                num45 = num26 / num17;
                num46 = num27 / num17;
                num47 = num28 / num17;
                num48 = num30 / num17;
                num49 = num31 / num17;
                num50 = num32 / num17;
                num51 = num33 / num17;
                num52 = num34 / num17;
                num53 = num35 / num17;
                num16 = num36 / num17;
                num54 = num29 / num17;
                num55 = num37 / num17;
                num56 = num38 / num17;
                num57 = num39 / num17;
            }
            else
            {
                num40 = 0;
                num41 = 0;
                num42 = 0;
                num43 = 0;
                num44 = 0;
                num45 = 0;
                num46 = 0;
                num47 = 0;
                num48 = 0;
                num49 = 50;
                num50 = 50;
                num51 = 0;
                num52 = 0;
                num53 = 0;
                num54 = 0;
                num55 = 0;
                num56 = 0;
                num57 = 0;
            }
            int num58 = num16 + num53;
            int num59 = Mathf.Clamp(num40, 0, int.MaxValue);
            int num60 = Mathf.Clamp(num41, 0, int.MaxValue);
            int num61 = Mathf.Clamp(num42, 0, int.MaxValue);
            int num62 = Mathf.Clamp(num43, 0, int.MaxValue);
            int num63 = Mathf.Clamp(num44, 0, int.MaxValue);
            int num64 = Mathf.Clamp(num45, 0, int.MaxValue);
            int num65 = Mathf.Clamp(num46, 0, int.MaxValue);
            int num66 = Mathf.Clamp(num47, 0, int.MaxValue);
            int num67 = Mathf.Clamp(num54, 0, int.MaxValue);
            int num68 = Mathf.Clamp(num48, 0, int.MaxValue);
            int num69 = Mathf.Clamp(num49, 0, int.MaxValue);
            int num70 = Mathf.Clamp(num50, 0, int.MaxValue);
            int num71 = Mathf.Clamp(num51, 0, int.MaxValue);
            int num72 = Mathf.Clamp(num52, 0, int.MaxValue);
            int num73 = Mathf.Clamp(num53, 0, int.MaxValue);
            int num74 = Mathf.Clamp(num58, 0, int.MaxValue);
            int num75 = Mathf.Clamp(num17, 0, int.MaxValue);
            int num76 = Mathf.Clamp(num55, 0, int.MaxValue);
            int num77 = Mathf.Clamp(num56, 0, int.MaxValue);
            int num78 = Mathf.Clamp(num57, 0, int.MaxValue);
            target[0] = num59;
            target[2] = num61;
            target[1] = num60;
            target[3] = num62;
            target[4] = num63;
            target[5] = num64;
            target[6] = num65;
            target[7] = num66;
            target[8] = num67;
            target[9] = num68;
            target[10] = num69;
            target[11] = num70;
            target[12] = num71;
            target[13] = num72;
            target[14] = num73;
            target[15] = num74;
            target[16] = num75;
            target[17] = num76;
            target[18] = num77;
            target[19] = num78;
        }

        [RedirectMethod]
        public void CheckResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local, out int total)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID + num) * 20 + resource);
            local = (int)m_localFinalResources[num3];
            total = m_totalFinalResources[(int)resource];
        }

        [RedirectMethod]
        public void CheckLocalResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID + num) * 20 + resource);
            local = (int)m_localFinalResources[num3];
        }

        [RedirectMethod]
        public void CheckLocalResources(Vector3 position, out ushort[] resources, out int index)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            index = (num2 * GRID + num) * 20;
            resources = m_localFinalResources;
        }

        [RedirectMethod]
        public void CheckTotalResource(ImmaterialResourceManager.Resource resource, out int total)
        {
            total = m_totalFinalResources[(int)resource];
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
                    int index1 = (z * GRID + x) * 20;
                    if (CalculateLocalResources(x, z, m_localTempResources, m_globalFinalResources, m_localFinalResources, index1))
                    {
                        minX = minX == -1 ? x : Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        minZ = minZ == -1 ? z : Math.Min(minZ, z);
                        maxZ = Math.Max(maxZ, z);
                    }
                    int num5 = (int)m_localFinalResources[index1 + 16];
                    for (int index2 = 0; index2 < 20; ++index2)
                    {
                        int num6 = (int)m_localFinalResources[index1 + index2];
                        m_totalTempResources[index2] += num6;
                        m_totalTempResourcesMul[index2] += num6 * num5;
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
                for (int index = 0; index < 20; ++index)
                {
                    //begin mod
                    m_globalFinalResources[index] = m_globalTempResources[index];
                    m_globalTempResources[index] = 0;
                    m_totalTempResources[index] = 0;
                    m_totalTempResourcesMul[index] = 0;
                    statisticBase.Acquire<StatisticInt32>(index, 20).Set(m_totalFinalResources[index]);
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
