using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.ResourceManagers
{
    class FakeImmaterialResourceManager
    {
        private static int GRID = 450;
        private static int HALFGRID = 225;
        private static ushort[] m_localFinalResources = new ushort[GRID * GRID * 20];
        private static ushort[] m_localTempResources = new ushort[GRID * GRID * 20];
        private static Texture2D m_resourceTexture;
        private static int[] m_globalFinalResources = new int[20];
        private static int[] m_globalTempResources = new int[20];
        private static int[] m_totalFinalResources = new int[20];
        private static int[] m_totalTempResources = new int[20];
        private static int[] m_totalTempResourcesMul = new int[20];
        private static int[] m_modifiedX1 = new int[GRID];
        private static int[] m_modifiedX2 = new int[GRID];
        private static bool m_modified = true;

        protected static void Init()
        {
            m_resourceTexture = new Texture2D(GRID, GRID, TextureFormat.Alpha8, false, true);
            m_resourceTexture.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture("_ImmaterialResources", m_resourceTexture);            
        }

        public static void OnDestroy()
        {
            if (m_resourceTexture != null)
            {
                UnityEngine.Object.Destroy(m_resourceTexture);
                m_resourceTexture = null;
            }
        }

        [ReplaceMethod]
        private void UpdateResourceMapping()
        {
            Vector4 vec;
            vec.z = 0.000101725258f;
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

        [ReplaceMethod]
        private void LateUpdate()
        {
            if (!Singleton<LoadingManager>.instance.m_loadingComplete)
            {
                return;
            }
            if (ImmaterialResourceManager.instance.ResourceMapVisible != ImmaterialResourceManager.Resource.None && m_modified)
            {
                m_modified = false;
                UpdateTexture();
            }
        }

        private void UpdateTexture()
        {
            for (int i = 0; i < GRID; i++)
            {
                if (m_modifiedX2[i] >= m_modifiedX1[i])
                {
                    while (!Monitor.TryEnter(m_localFinalResources, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    int num;
                    int num2;
                    try
                    {
                        num = m_modifiedX1[i];
                        num2 = m_modifiedX2[i];
                        m_modifiedX1[i] = 10000;
                        m_modifiedX2[i] = -10000;
                    }
                    finally
                    {
                        Monitor.Exit(m_localFinalResources);
                    }
                    for (int j = num; j <= num2; j++)
                    {
                        int num3 = 0;
                        AddLocalResource(j - 1, i - 1, 5, ref num3);
                        AddLocalResource(j, i - 1, 7, ref num3);
                        AddLocalResource(j + 1, i - 1, 5, ref num3);
                        AddLocalResource(j - 1, i, 7, ref num3);
                        AddLocalResource(j, i, 14, ref num3);
                        AddLocalResource(j + 1, i, 7, ref num3);
                        AddLocalResource(j - 1, i + 1, 5, ref num3);
                        AddLocalResource(j, i + 1, 7, ref num3);
                        AddLocalResource(j + 1, i + 1, 5, ref num3);
                        float num4 = Mathf.Clamp01(Mathf.Sqrt((float)num3 * 0.000161290329f));
                        Color color;
                        color.r = num4;
                        color.g = num4;
                        color.b = num4;
                        color.a = num4;
                        m_resourceTexture.SetPixel(j, i, color);
                    }
                }
            }
            m_resourceTexture.Apply();
        }

        private void AddLocalResource(int x, int z, int multiplier, ref int amount)
        {
            x = Mathf.Clamp(x, 0, GRID - 1);
            z = Mathf.Clamp(z, 0, GRID - 1);
            int num = (int)((z * GRID  + x) * 20 + ImmaterialResourceManager.instance.ResourceMapVisible);
            amount += (int)m_localFinalResources[num] * multiplier;
        }

        [ReplaceMethod]
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

        private void AddResource(ref ushort buffer, int rate)
        {
            buffer = (ushort)Mathf.Min((int)buffer + rate, 65535);
        }

        private void AddResource(ref int buffer, int rate)
        {
            buffer = Mathf.Min(buffer + rate, 2147483647);
        }

        [ReplaceMethod]
        public int AddResource(ImmaterialResourceManager.Resource resource, int rate, Vector3 position, float radius)
        {
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
                        int num12 = (int)((i * GRID  + j) * 20 + resource);
                        AddResource(ref m_localTempResources[num12], num10);
                    }
                }
            }
            return rate;
        }

        [ReplaceMethod]
        public int AddResource(ImmaterialResourceManager.Resource resource, int rate)
        {
            if (rate == 0)
            {
                return 0;
            }
            AddResource(ref m_globalTempResources[(int)resource], rate);
            return rate;
        }

        [ReplaceMethod]
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

        private static bool CalculateLocalResources(int x, int z, ushort[] buffer, int[] global, ushort[] target, int index)
        {
            int num = (int)buffer[index] + global[0];
            int num2 = (int)buffer[index + 1] + global[1];
            int num3 = (int)buffer[index + 2] + global[2];
            int num4 = (int)buffer[index + 3] + global[3];
            int num5 = (int)buffer[index + 4] + global[4];
            int num6 = (int)buffer[index + 5] + global[5];
            int num7 = (int)buffer[index + 6] + global[6];
            int num8 = (int)buffer[index + 7] + global[7];
            int num9 = (int)buffer[index + 8] + global[8];
            int num10 = (int)buffer[index + 9] + global[9];
            int num11 = (int)buffer[index + 10] + global[10];
            int num12 = (int)buffer[index + 11] + global[11];
            int num13 = (int)buffer[index + 12] + global[12];
            int num14 = (int)buffer[index + 13] + global[13];
            int num15 = (int)buffer[index + 14] + global[14];
            int num16 = (int)buffer[index + 15] + global[15];
            int num17 = (int)buffer[index + 16] + global[16];
            int num18 = (int)buffer[index + 17] + global[17];
            int num19 = (int)buffer[index + 18] + global[18];
            int num20 = (int)buffer[index + 19] + global[19];
            Vector3 pos;
            pos.x = ((float)x - HALFGRID + 0.5f) * 38.4f;
            pos.y = 0f;
            pos.z = ((float)z - HALFGRID + 0.5f) * 38.4f;
            byte b;
            Singleton<NaturalResourceManager>.instance.CheckPollution(pos, out b);
            int num21 = (int)b;
            num18 = num18 * 2 / (num2 + 50);
            if (num13 == 0)
            {
                num10 = 0;
                num11 = 50;
                num12 = 50;
            }
            else
            {
                num10 /= num13;
                num11 /= num13;
                num12 /= num13;
                num17 += Mathf.Min(num13, 10) * 10;
            }
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num3, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num2, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num4, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num5, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num6, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num7, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num8, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num20, 100, 500, 50, 100);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num14, 100, 500, 100, 200);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num12, 60, 100, 0, 50);
            num15 += ImmaterialResourceManager.CalculateResourceEffect(num11, 60, 100, 0, 50);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(100 - num12, 60, 100, 0, 50);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(100 - num11, 60, 100, 0, 50);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(num21, 50, 255, 50, 100);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(num9, 10, 100, 0, 100);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(num10, 10, 100, 0, 100);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(num18, 50, 100, 10, 50);
            num15 -= ImmaterialResourceManager.CalculateResourceEffect(num19, 15, 50, 100, 200);
            num15 = num15 * num17 / 1000;
            num = Mathf.Clamp(num, 0, 65535);
            num2 = Mathf.Clamp(num2, 0, 65535);
            num3 = Mathf.Clamp(num3, 0, 65535);
            num4 = Mathf.Clamp(num4, 0, 65535);
            num5 = Mathf.Clamp(num5, 0, 65535);
            num6 = Mathf.Clamp(num6, 0, 65535);
            num7 = Mathf.Clamp(num7, 0, 65535);
            num8 = Mathf.Clamp(num8, 0, 65535);
            num9 = Mathf.Clamp(num9, 0, 65535);
            num10 = Mathf.Clamp(num10, 0, 65535);
            num11 = Mathf.Clamp(num11, 0, 65535);
            num12 = Mathf.Clamp(num12, 0, 65535);
            num13 = Mathf.Clamp(num13, 0, 65535);
            num14 = Mathf.Clamp(num14, 0, 65535);
            num15 = Mathf.Clamp(num15, 0, 65535);
            num16 = Mathf.Clamp(num16, 0, 65535);
            num17 = Mathf.Clamp(num17, 0, 65535);
            num18 = Mathf.Clamp(num18, 0, 65535);
            num19 = Mathf.Clamp(num19, 0, 65535);
            num20 = Mathf.Clamp(num20, 0, 65535);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(x * 2, z * 2);
            instance.m_districts.m_buffer[(int)district].AddGroundData(num15, num21, num17);
            bool result = false;
            if (num != (int)target[index])
            {
                target[index] = (ushort)num;
                result = true;
            }
            if (num2 != (int)target[index + 1])
            {
                target[index + 1] = (ushort)num2;
                result = true;
            }
            if (num3 != (int)target[index + 2])
            {
                target[index + 2] = (ushort)num3;
                result = true;
            }
            if (num4 != (int)target[index + 3])
            {
                target[index + 3] = (ushort)num4;
                result = true;
            }
            if (num5 != (int)target[index + 4])
            {
                target[index + 4] = (ushort)num5;
                result = true;
            }
            if (num6 != (int)target[index + 5])
            {
                target[index + 5] = (ushort)num6;
                result = true;
            }
            if (num7 != (int)target[index + 6])
            {
                target[index + 6] = (ushort)num7;
                result = true;
            }
            if (num8 != (int)target[index + 7])
            {
                target[index + 7] = (ushort)num8;
                result = true;
            }
            if (num9 != (int)target[index + 8])
            {
                target[index + 8] = (ushort)num9;
                result = true;
            }
            if (num10 != (int)target[index + 9])
            {
                target[index + 9] = (ushort)num10;
                result = true;
            }
            if (num11 != (int)target[index + 10])
            {
                target[index + 10] = (ushort)num11;
                result = true;
            }
            if (num12 != (int)target[index + 11])
            {
                target[index + 11] = (ushort)num12;
                result = true;
            }
            if (num13 != (int)target[index + 12])
            {
                target[index + 12] = (ushort)num13;
                result = true;
            }
            if (num14 != (int)target[index + 13])
            {
                target[index + 13] = (ushort)num14;
                result = true;
            }
            if (num15 != (int)target[index + 14])
            {
                target[index + 14] = (ushort)num15;
                result = true;
            }
            if (num16 != (int)target[index + 15])
            {
                target[index + 15] = (ushort)num16;
                result = true;
            }
            if (num17 != (int)target[index + 16])
            {
                target[index + 16] = (ushort)num17;
                result = true;
            }
            if (num18 != (int)target[index + 17])
            {
                target[index + 17] = (ushort)num18;
                result = true;
            }
            if (num19 != (int)target[index + 18])
            {
                target[index + 18] = (ushort)num19;
                result = true;
            }
            if (num20 != (int)target[index + 19])
            {
                target[index + 19] = (ushort)num20;
                result = true;
            }
            return result;
        }

        private static void CalculateTotalResources(int[] buffer, int[] bufferMul, int[] target)
        {
            int num = buffer[0];
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
            if (num17 != 0)
            {
                num = num21 / num17;
                num2 = num22 / num17;
                num3 = num23 / num17;
                num4 = num24 / num17;
                num5 = num25 / num17;
                num6 = num26 / num17;
                num7 = num27 / num17;
                num8 = num28 / num17;
                num10 = num30 / num17;
                num11 = num31 / num17;
                num12 = num32 / num17;
                num13 = num33 / num17;
                num14 = num34 / num17;
                num15 = num35 / num17;
                num16 = num36 / num17;
                num9 = num29 / num17;
                num18 = num37 / num17;
                num19 = num38 / num17;
                num20 = num39 / num17;
            }
            else
            {
                num = 0;
                num2 = 0;
                num3 = 0;
                num4 = 0;
                num5 = 0;
                num6 = 0;
                num7 = 0;
                num8 = 0;
                num10 = 0;
                num11 = 50;
                num12 = 50;
                num13 = 0;
                num14 = 0;
                num15 = 0;
                num9 = 0;
                num18 = 0;
                num19 = 0;
                num20 = 0;
            }
            num16 += num15;
            num = Mathf.Clamp(num, 0, 2147483647);
            num2 = Mathf.Clamp(num2, 0, 2147483647);
            num3 = Mathf.Clamp(num3, 0, 2147483647);
            num4 = Mathf.Clamp(num4, 0, 2147483647);
            num5 = Mathf.Clamp(num5, 0, 2147483647);
            num6 = Mathf.Clamp(num6, 0, 2147483647);
            num7 = Mathf.Clamp(num7, 0, 2147483647);
            num8 = Mathf.Clamp(num8, 0, 2147483647);
            num9 = Mathf.Clamp(num9, 0, 2147483647);
            num10 = Mathf.Clamp(num10, 0, 2147483647);
            num11 = Mathf.Clamp(num11, 0, 2147483647);
            num12 = Mathf.Clamp(num12, 0, 2147483647);
            num13 = Mathf.Clamp(num13, 0, 2147483647);
            num14 = Mathf.Clamp(num14, 0, 2147483647);
            num15 = Mathf.Clamp(num15, 0, 2147483647);
            num16 = Mathf.Clamp(num16, 0, 2147483647);
            num17 = Mathf.Clamp(num17, 0, 2147483647);
            num18 = Mathf.Clamp(num18, 0, 2147483647);
            num19 = Mathf.Clamp(num19, 0, 2147483647);
            num20 = Mathf.Clamp(num20, 0, 2147483647);
            target[0] = num;
            target[2] = num3;
            target[1] = num2;
            target[3] = num4;
            target[4] = num5;
            target[5] = num6;
            target[6] = num7;
            target[7] = num8;
            target[8] = num9;
            target[9] = num10;
            target[10] = num11;
            target[11] = num12;
            target[12] = num13;
            target[13] = num14;
            target[14] = num15;
            target[15] = num16;
            target[16] = num17;
            target[17] = num18;
            target[18] = num19;
            target[19] = num20;
        }

        [ReplaceMethod]
        public void CheckResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local, out int total)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID  + num) * 20 + resource);
            local = (int)m_localFinalResources[num3];
            total = m_totalFinalResources[(int)resource];
        }

        [ReplaceMethod]
        public void CheckLocalResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            int num3 = (int)((num2 * GRID  + num) * 20 + resource);
            local = (int)m_localFinalResources[num3];
        }

        [ReplaceMethod]
        public void CheckLocalResources(Vector3 position, out ushort[] resources, out int index)
        {
            int num = Mathf.Clamp((int)(position.x / 38.4f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(position.z / 38.4f + HALFGRID), 0, GRID - 1);
            index = (num2 * GRID  + num) * 20;
            resources = m_localFinalResources;
        }

        [ReplaceMethod]
        public void CheckTotalResource(ImmaterialResourceManager.Resource resource, out int total)
        {
            total = m_totalFinalResources[(int)resource];
        }

        [ReplaceMethod]
        protected void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num = (int)(currentFrameIndex & 511u);
                if (num < GRID)
                {
                    int num2 = num;

                    int num5 = -1;
                    int maxX = -1;
                    for (int i = 0; i < GRID; i++)
                    {
                        int num6 = (num2 * GRID + i) * 20;
                        if (CalculateLocalResources(i, num2, m_localTempResources, m_globalFinalResources, m_localFinalResources, num6))
                        {
                            if (num5 == -1)
                            {
                                num5 = i;
                            }
                            maxX = i;
                        }
                        int num7 = (int)m_localFinalResources[num6 + 16];
                        for (int j = 0; j < 20; j++)
                        {
                            int num8 = (int)m_localFinalResources[num6 + j];
                            m_totalTempResources[j] += num8;
                            m_totalTempResourcesMul[j] += num8 * num7;
                            m_localTempResources[num6 + j] = 0;
                        }
                    }
                    if (num == GRID - 1)
                    {
                        CalculateTotalResources(m_totalTempResources, m_totalTempResourcesMul, m_totalFinalResources);
                        StatisticsManager instance = Singleton<StatisticsManager>.instance;
                        StatisticBase statisticBase = instance.Acquire<StatisticArray>(StatisticType.ImmaterialResource);
                        for (int k = 0; k < 20; k++)
                        {
                            m_globalFinalResources[k] = m_globalTempResources[k];
                            m_globalTempResources[k] = 0;
                            m_totalTempResources[k] = 0;
                            m_totalTempResourcesMul[k] = 0;
                            statisticBase.Acquire<StatisticInt32>(k, 20).Set(m_totalFinalResources[k]);
                        }
                    }
                    if (num5 != -1)
                    {
                        AreaModified(num5, num2, maxX, num2);
                    }
                }
            }
        }
    }
}
