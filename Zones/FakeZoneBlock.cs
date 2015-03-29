using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Zones
{
    /// <summary>
    /// None of these methods are exported, as ZoneBlock is a struct.
    /// </summary>
    [Fixme]
    internal class FakeZoneBlock
    {
        public static void CalculateBlock2(ref ZoneBlock z, ushort blockID)
        {
            if (((int)z.m_flags & 3) != 1)
                return;
            int rowCount = z.RowCount;
            Vector2 vector2_1 = new Vector2(Mathf.Cos(z.m_angle), Mathf.Sin(z.m_angle)) * 8f;
            Vector2 vector2_2 = new Vector2(vector2_1.y, -vector2_1.x);
            Vector2 vector2_3 = VectorUtils.XZ(z.m_position);
            Vector2 vector2_4 = vector2_3 - 4f * vector2_1 - 4f * vector2_2;
            Vector2 vector2_5 = vector2_3 + 0.0f * vector2_1 - 4f * vector2_2;
            Vector2 vector2_6 = vector2_3 + 0.0f * vector2_1 + (float)(rowCount - 4) * vector2_2;
            Vector2 vector2_7 = vector2_3 - 4f * vector2_1 + (float)(rowCount - 4) * vector2_2;
            float minX = Mathf.Min(Mathf.Min(vector2_4.x, vector2_5.x), Mathf.Min(vector2_6.x, vector2_7.x));
            float minZ = Mathf.Min(Mathf.Min(vector2_4.y, vector2_5.y), Mathf.Min(vector2_6.y, vector2_7.y));
            float maxX = Mathf.Max(Mathf.Max(vector2_4.x, vector2_5.x), Mathf.Max(vector2_6.x, vector2_7.x));
            float maxZ = Mathf.Max(Mathf.Max(vector2_4.y, vector2_5.y), Mathf.Max(vector2_6.y, vector2_7.y));
            ulong valid = z.m_valid;
            ulong shared = 0UL;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            for (int index = 0; index < instance.m_cachedBlocks.m_size; ++index)
                CalculateImplementation2(z, blockID, ref instance.m_cachedBlocks.m_buffer[index], ref valid, ref shared, minX, minZ, maxX, maxZ);
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num5 = instance.m_zoneGrid[index1 * FakeZoneManager.ZONEGRID_RESOLUTION + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 vector3 = instance.m_blocks.m_buffer[(int)num5].m_position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 46f - vector3.x, minZ - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)maxX - 46.0), (float)((double)vector3.z - (double)maxZ - 46.0))) < 0.0 && (int)num5 != (int)blockID)
                            CalculateImplementation2(z, blockID, ref instance.m_blocks.m_buffer[(int)num5], ref valid, ref shared, minX, minZ, maxX, maxZ);
                        num5 = instance.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            ulong num7 = 144680345676153346UL;
            for (int index = 0; index < 7; ++index)
            {
                valid = (ulong)((long)valid & ~(long)num7 | (long)valid & (long)valid << 1 & (long)num7);
                num7 <<= 1;
            }
            z.m_valid = valid;
            z.m_shared = shared;
        }

        [Fixme("Make this more lightweight, ie. only a thin wrapper.")]
        private static void CalculateImplementation2(ZoneBlock z, ushort blockID, ref ZoneBlock other, ref ulong valid, ref ulong shared, float minX, float minZ, float maxX, float maxZ)
        {
            if (((int)other.m_flags & 1) == 0 || (double)Mathf.Abs(other.m_position.x - z.m_position.x) >= 92.0 || (double)Mathf.Abs(other.m_position.z - z.m_position.z) >= 92.0)
                return;
            bool flag1 = ((int)other.m_flags & 2) != 0;
            int rowCount1 = z.RowCount;
            int rowCount2 = other.RowCount;
            Vector2 vector2_1 = new Vector2(Mathf.Cos(z.m_angle), Mathf.Sin(z.m_angle)) * 8f;
            Vector2 vector2_2 = new Vector2(vector2_1.y, -vector2_1.x);
            Vector2 vector2_3 = new Vector2(Mathf.Cos(other.m_angle), Mathf.Sin(other.m_angle)) * 8f;
            Vector2 vector2_4 = new Vector2(vector2_3.y, -vector2_3.x);
            Vector2 vector2_5 = VectorUtils.XZ(other.m_position);
            Quad2 quad = new Quad2();
            quad.a = vector2_5 - 4f * vector2_3 - 4f * vector2_4;
            quad.b = vector2_5 + 0.0f * vector2_3 - 4f * vector2_4;
            quad.c = vector2_5 + 0.0f * vector2_3 + (float)(rowCount2 - 4) * vector2_4;
            quad.d = vector2_5 - 4f * vector2_3 + (float)(rowCount2 - 4) * vector2_4;
            Vector2 vector2_6 = quad.Min();
            Vector2 vector2_7 = quad.Max();
            if ((double)vector2_6.x > (double)maxX || (double)vector2_6.y > (double)maxZ || ((double)minX > (double)vector2_7.x || (double)minZ > (double)vector2_7.y))
                return;
            Vector2 vector2_8 = VectorUtils.XZ(z.m_position);
            Quad2 quad2_1 = new Quad2();
            quad2_1.a = vector2_8 - 4f * vector2_1 - 4f * vector2_2;
            quad2_1.b = vector2_8 + 0.0f * vector2_1 - 4f * vector2_2;
            quad2_1.c = vector2_8 + 0.0f * vector2_1 + (float)(rowCount1 - 4) * vector2_2;
            quad2_1.d = vector2_8 - 4f * vector2_1 + (float)(rowCount1 - 4) * vector2_2;
            if (!quad2_1.Intersect(quad))
                return;
            for (int z1 = 0; z1 < rowCount1; ++z1)
            {
                Vector2 vector2_9 = ((float)z1 - 3.99f) * vector2_2;
                Vector2 vector2_10 = ((float)z1 - 3.01f) * vector2_2;
                quad2_1.a = vector2_8 - 4f * vector2_1 + vector2_9;
                quad2_1.b = vector2_8 + 0.0f * vector2_1 + vector2_9;
                quad2_1.c = vector2_8 + 0.0f * vector2_1 + vector2_10;
                quad2_1.d = vector2_8 - 4f * vector2_1 + vector2_10;
                if (quad2_1.Intersect(quad))
                {
                    for (int x1 = 0; (long)x1 < 4L && ((long)valid & 1L << (z1 << 3 | x1)) != 0L; ++x1)
                    {
                        Vector2 vector2_11 = ((float)x1 - 3.99f) * vector2_1;
                        Vector2 vector2_12 = ((float)x1 - 3.01f) * vector2_1;
                        Vector2 p = vector2_8 + (vector2_12 + vector2_11 + vector2_10 + vector2_9) * 0.5f;
                        if (Quad2.Intersect(quad.a - vector2_3 - vector2_4, quad.b + vector2_3 - vector2_4, quad.c + vector2_3 + vector2_4, quad.d - vector2_3 + vector2_4, p))
                        {
                            Quad2 quad2_2 = new Quad2();
                            quad2_2.a = vector2_8 + vector2_11 + vector2_9;
                            quad2_2.b = vector2_8 + vector2_12 + vector2_9;
                            quad2_2.c = vector2_8 + vector2_12 + vector2_10;
                            quad2_2.d = vector2_8 + vector2_11 + vector2_10;
                            bool flag2 = true;
                            bool flag3 = false;
                            for (int z2 = 0; z2 < rowCount2 && flag2; ++z2)
                            {
                                Vector2 vector2_13 = ((float)z2 - 3.99f) * vector2_4;
                                Vector2 vector2_14 = ((float)z2 - 3.01f) * vector2_4;
                                for (int x2 = 0; (long)x2 < 4L && flag2; ++x2)
                                {
                                    if (((long)other.m_valid & ~(long)other.m_shared & 1L << (z2 << 3 | x2)) != 0L)
                                    {
                                        Vector2 vector2_15 = ((float)x2 - 3.99f) * vector2_3;
                                        Vector2 vector2_16 = ((float)x2 - 3.01f) * vector2_3;
                                        float num1 = Vector2.SqrMagnitude(vector2_5 + (vector2_16 + vector2_15 + vector2_14 + vector2_13) * 0.5f - p);
                                        if ((double)num1 < 144.0)
                                        {
                                            if (!flag1)
                                            {
                                                float f = Mathf.Abs(other.m_angle - z.m_angle) * 0.6366197f;
                                                float num2 = f - Mathf.Floor(f);
                                                if ((double)num1 < 0.00999999977648258 && ((double)num2 < 0.00999999977648258 || (double)num2 > 0.990000009536743))
                                                {
                                                    if (x1 < x2 || x1 == x2 && z.m_buildIndex < other.m_buildIndex)
                                                        other.m_shared |= (ulong)(1L << (z2 << 3 | x2));
                                                    else
                                                        flag3 = true;
                                                }
                                                else if (quad2_2.Intersect(new Quad2()
                                                {
                                                    a = vector2_5 + vector2_15 + vector2_13,
                                                    b = vector2_5 + vector2_16 + vector2_13,
                                                    c = vector2_5 + vector2_16 + vector2_14,
                                                    d = vector2_5 + vector2_15 + vector2_14
                                                }))
                                                {
                                                    if (x2 >= 4 && x1 >= 4 || x2 < 4 && x1 < 4)
                                                    {
                                                        if (x2 >= 2 && x1 >= 2 || x2 < 2 && x1 < 2)
                                                        {
                                                            if (z.m_buildIndex < other.m_buildIndex)
                                                                other.m_valid &= (ulong)~(1L << (z2 << 3 | x2));
                                                            else
                                                                flag2 = false;
                                                        }
                                                        else if (x2 < 2)
                                                            flag2 = false;
                                                        else
                                                            other.m_valid &= (ulong)~(1L << (z2 << 3 | x2));
                                                    }
                                                    else if (x2 < 4)
                                                        flag2 = false;
                                                    else
                                                        other.m_valid &= (ulong)~(1L << (z2 << 3 | x2));
                                                }
                                            }
                                            if ((double)num1 < 36.0 && x1 < 4 && x2 < 4)
                                            {
                                                ItemClass.Zone zone1 = z.GetZone(x1, z1);
                                                ItemClass.Zone zone2 = other.GetZone(x2, z2);
                                                if (zone1 == ItemClass.Zone.Unzoned)
                                                    z.SetZone(x1, z1, zone2);
                                                else if (zone2 == ItemClass.Zone.Unzoned && !flag1)
                                                    other.SetZone(x2, z2, zone1);
                                            }
                                        }
                                    }
                                }
                            }
                            if (!flag2)
                            {
                                valid = valid & (ulong)~(1L << (z1 << 3 | x1));
                                break;
                            }
                            if (flag3)
                                shared = shared | (ulong)(1L << (z1 << 3 | x1));
                        }
                    }
                }
            }
        }

        public static void SimulationStep(ref ZoneBlock b, ushort blockID)
        {
                ZoneManager instance1 = Singleton<ZoneManager>.instance;
                int rowCount = b.RowCount;
                Vector2 xDir = new Vector2(Mathf.Cos(b.m_angle), Mathf.Sin(b.m_angle)) * 8f;
                Vector2 zDir = new Vector2(xDir.y, -xDir.x);
                ulong num1 = b.m_valid & (ulong)~((long)b.m_occupied1 | (long)b.m_occupied2);
                int z = 0;
                ItemClass.Zone zone = ItemClass.Zone.Unzoned;
                for (int index = 0; index < 4 && zone == ItemClass.Zone.Unzoned; ++index)
                {
                    z = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)rowCount);
                    if (((long)num1 & 1L << (z << 3)) != 0L)
                        zone = b.GetZone(0, z);
                }
                DistrictManager instance2 = Singleton<DistrictManager>.instance;
                byte district = instance2.GetDistrict(b.m_position);
                int num2;
                switch (zone)
                {
                    case ItemClass.Zone.ResidentialLow:
                        num2 = instance1.m_actualResidentialDemand + instance2.m_districts.m_buffer[(int)district].CalculateResidentialLowDemandOffset();
                        break;
                    case ItemClass.Zone.ResidentialHigh:
                        num2 = instance1.m_actualResidentialDemand + instance2.m_districts.m_buffer[(int)district].CalculateResidentialHighDemandOffset();
                        break;
                    case ItemClass.Zone.CommercialLow:
                        num2 = instance1.m_actualCommercialDemand + instance2.m_districts.m_buffer[(int)district].CalculateCommercialLowDemandOffset();
                        break;
                    case ItemClass.Zone.CommercialHigh:
                        num2 = instance1.m_actualCommercialDemand + instance2.m_districts.m_buffer[(int)district].CalculateCommercialHighDemandOffset();
                        break;
                    case ItemClass.Zone.Industrial:
                        num2 = instance1.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[(int)district].CalculateIndustrialDemandOffset();
                        break;
                    case ItemClass.Zone.Office:
                        num2 = instance1.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[(int)district].CalculateOfficeDemandOffset();
                        break;
                    default:
                        return;
                }
                Vector2 vector2_1 = VectorUtils.XZ(b.m_position);
                Vector2 vector2_2 = vector2_1 - 3.5f * xDir + ((float)z - 3.5f) * zDir;
                int[] xBuffer = instance1.m_tmpXBuffer;
                for (int index = 0; index < 13; ++index)
                    xBuffer[index] = 0;
                Quad2 quad = new Quad2();
                quad.a = vector2_1 - 4f * xDir + ((float)z - 10f) * zDir;
                quad.b = vector2_1 + 3f * xDir + ((float)z - 10f) * zDir;
                quad.c = vector2_1 + 3f * xDir + ((float)z + 2f) * zDir;
                quad.d = vector2_1 - 4f * xDir + ((float)z + 2f) * zDir;
                Vector2 vector2_3 = quad.Min();
                Vector2 vector2_4 = quad.Max();
                int num3 = Mathf.Max((int)(((double)vector2_3.x - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
                int num4 = Mathf.Max((int)(((double)vector2_3.y - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
                int num5 = Mathf.Min((int)(((double)vector2_4.x + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
                int num6 = Mathf.Min((int)(((double)vector2_4.y + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
                for (int index1 = num4; index1 <= num6; ++index1)
                {
                    for (int index2 = num3; index2 <= num5; ++index2)
                    {
                        ushort num7 = instance1.m_zoneGrid[index1 * FakeZoneManager.ZONEGRID_RESOLUTION + index2];
                        int num8 = 0;
                        while ((int)num7 != 0)
                        {
                            Vector3 vector3 = instance1.m_blocks.m_buffer[(int)num7].m_position;
                            if ((double)Mathf.Max(Mathf.Max(vector2_3.x - 46f - vector3.x, vector2_3.y - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)vector2_4.x - 46.0), (float)((double)vector3.z - (double)vector2_4.y - 46.0))) < 0.0)
                            {
                                CheckBlock(b, ref instance1.m_blocks.m_buffer[(int)num7], xBuffer, zone, vector2_2, xDir, zDir, quad);
                            }
                            num7 = instance1.m_blocks.m_buffer[(int)num7].m_nextGridBlock;
                            if (++num8 >= 32768)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
                for (int index = 0; index < 13; ++index)
                {
                    uint num7 = (uint)xBuffer[index];
                    int num8 = 0;
                    bool flag = false;
                    while (((int)num7 & 1) != 0)
                    {
                        ++num8;
                        flag = ((int)num7 & 65536) != 0;
                        num7 >>= 1;
                    }
                    if (num8 == 5 || num8 == 6)
                    {
                        if (flag)
                            num8 -= Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) + 2;
                        else
                            num8 = 4;
                    }
                    else if (num8 == 7)
                        num8 = 4;
                    xBuffer[index] = num8;
                }
                int num9 = xBuffer[6];
                if (num9 == 0)
                    return;
                bool flag1 = IsGoodPlace(b, vector2_2);
                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(100U) >= num2)
                {
                    if (!flag1)
                        return;
                    instance1.m_goodAreaFound[(int)zone] = (short)1024;
                }
                else if (!flag1 && (int)instance1.m_goodAreaFound[(int)zone] > -1024)
                {
                    if ((int)instance1.m_goodAreaFound[(int)zone] != 0)
                        return;
                    instance1.m_goodAreaFound[(int)zone] = (short)-1;
                }
                else
                {
                    int num7 = 6;
                    int num8 = 6;
                    bool flag2 = true;
                    while (true)
                    {
                        if (flag2)
                        {
                            while (num7 != 0 && xBuffer[num7 - 1] == num9)
                                --num7;
                            while (num8 != 12 && xBuffer[num8 + 1] == num9)
                                ++num8;
                        }
                        else
                        {
                            while (num7 != 0 && xBuffer[num7 - 1] >= num9)
                                --num7;
                            while (num8 != 12 && xBuffer[num8 + 1] >= num9)
                                ++num8;
                        }
                        int num10 = num7;
                        int num11 = num8;
                        while (num10 != 0 && xBuffer[num10 - 1] >= 2)
                            --num10;
                        while (num11 != 12 && xBuffer[num11 + 1] >= 2)
                            ++num11;
                        bool flag3 = num10 != 0 && num10 == num7 - 1;
                        bool flag4 = num11 != 12 && num11 == num8 + 1;
                        if (flag3 && flag4)
                        {
                            if (num8 - num7 <= 2)
                            {
                                if (num9 <= 2)
                                {
                                    if (!flag2)
                                        goto label_88;
                                }
                                else
                                    --num9;
                            }
                            else
                                break;
                        }
                        else if (flag3)
                        {
                            if (num8 - num7 <= 1)
                            {
                                if (num9 <= 2)
                                {
                                    if (!flag2)
                                        goto label_88;
                                }
                                else
                                    --num9;
                            }
                            else
                                goto label_73;
                        }
                        else if (flag4)
                        {
                            if (num8 - num7 <= 1)
                            {
                                if (num9 <= 2)
                                {
                                    if (!flag2)
                                        goto label_88;
                                }
                                else
                                    --num9;
                            }
                            else
                                goto label_79;
                        }
                        else if (num7 == num8)
                        {
                            if (num9 <= 2)
                            {
                                if (!flag2)
                                    goto label_88;
                            }
                            else
                                --num9;
                        }
                        else
                            goto label_88;
                        flag2 = false;
                    }
                    ++num7;
                    --num8;
                    goto label_88;
                label_73:
                    ++num7;
                    goto label_88;
                label_79:
                    --num8;
                label_88:
                    int num12;
                    int num13;
                    if (num9 == 1 && num8 - num7 >= 1)
                    {
                        num7 += Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)(num8 - num7));
                        num8 = num7 + 1;
                        num12 = num7 + Singleton<SimulationManager>.instance.m_randomizer.Int32(2U);
                        num13 = num12;
                    }
                    else
                    {
                        do
                        {
                            num12 = num7;
                            num13 = num8;
                            if (num8 - num7 == 2)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                    --num13;
                                else
                                    ++num12;
                            }
                            else if (num8 - num7 == 3)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                    num13 -= 2;
                                else
                                    num12 += 2;
                            }
                            else if (num8 - num7 == 4)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                {
                                    num8 -= 2;
                                    num13 -= 3;
                                }
                                else
                                {
                                    num7 += 2;
                                    num12 += 3;
                                }
                            }
                            else if (num8 - num7 == 5)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                {
                                    num8 -= 3;
                                    num13 -= 2;
                                }
                                else
                                {
                                    num7 += 3;
                                    num12 += 2;
                                }
                            }
                            else if (num8 - num7 >= 6)
                            {
                                if (num7 == 0 || num8 == 12)
                                {
                                    if (num7 == 0)
                                    {
                                        num7 = 3;
                                        num12 = 2;
                                    }
                                    if (num8 == 12)
                                    {
                                        num8 = 9;
                                        num13 = 10;
                                    }
                                }
                                else if (Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0)
                                {
                                    num8 = num7 + 3;
                                    num13 = num12 + 2;
                                }
                                else
                                {
                                    num7 = num8 - 3;
                                    num12 = num13 - 2;
                                }
                            }
                        }
                        while (num8 - num7 > 3 || num13 - num12 > 3);
                    }
                    int num14 = 4;
                    for (int index = num7; index <= num8; ++index)
                        num14 = Mathf.Min(num14, xBuffer[index]);
                    int a = 4;
                    for (int index = num12; index <= num13; ++index)
                        a = Mathf.Min(a, xBuffer[index]);
                    ItemClass.SubService subService = ItemClass.SubService.None;
                    ItemClass.Level level = ItemClass.Level.Level1;
                    Vector3 vector3 = b.m_position + VectorUtils.X_Y((float)((double)num14 * 0.5 - 4.0) * xDir + (float)((double)(num7 + num8 + 1) * 0.5 + (double)z - 10.0) * zDir);
                    ItemClass.Service service;
                    switch (zone)
                    {
                        case ItemClass.Zone.ResidentialLow:
                            service = ItemClass.Service.Residential;
                            subService = ItemClass.SubService.ResidentialLow;
                            break;
                        case ItemClass.Zone.ResidentialHigh:
                            service = ItemClass.Service.Residential;
                            subService = ItemClass.SubService.ResidentialHigh;
                            break;
                        case ItemClass.Zone.CommercialLow:
                            service = ItemClass.Service.Commercial;
                            subService = ItemClass.SubService.CommercialLow;
                            break;
                        case ItemClass.Zone.CommercialHigh:
                            service = ItemClass.Service.Commercial;
                            subService = ItemClass.SubService.CommercialHigh;
                            break;
                        case ItemClass.Zone.Industrial:
                            service = ItemClass.Service.Industrial;
                            ZoneBlock.GetIndustryType(vector3, out subService, out level);
                            break;
                        case ItemClass.Zone.Office:
                            service = ItemClass.Service.Office;
                            subService = ItemClass.SubService.None;
                            break;
                        default:
                            return;
                    }
                    BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, num8 - num7 + 1, num14);
                    if (randomBuildingInfo == null)
                    {
                        int num10 = num12;
                        int num11 = num13;
                        num14 = a;
                        vector3 = b.m_position + VectorUtils.X_Y((float)((double)num14 * 0.5 - 4.0) * xDir + (float)((double)(num10 + num11 + 1) * 0.5 + (double)z - 10.0) * zDir);
                        if (zone == ItemClass.Zone.Industrial)
                            ZoneBlock.GetIndustryType(vector3, out subService, out level);
                        randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, service, subService, level, num11 - num10 + 1, num14);
                        if (randomBuildingInfo == null)
                            return;
                    }
                    if ((double)Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(vector3)) > (double)vector3.y)
                        return;
                    ushort building;
                    if (Singleton<BuildingManager>.instance.CreateBuilding(out building, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, vector3, b.m_angle + 1.570796f, num14, Singleton<SimulationManager>.instance.m_currentBuildIndex))
                    {
                        ++Singleton<SimulationManager>.instance.m_currentBuildIndex;
                        switch (service)
                        {
                            case ItemClass.Service.Residential:
                                instance1.m_actualResidentialDemand = Mathf.Max(0, instance1.m_actualResidentialDemand - 5);
                                break;
                            case ItemClass.Service.Commercial:
                                instance1.m_actualCommercialDemand = Mathf.Max(0, instance1.m_actualCommercialDemand - 5);
                                break;
                            case ItemClass.Service.Industrial:
                                instance1.m_actualWorkplaceDemand = Mathf.Max(0, instance1.m_actualWorkplaceDemand - 5);
                                break;
                            case ItemClass.Service.Office:
                                instance1.m_actualWorkplaceDemand = Mathf.Max(0, instance1.m_actualWorkplaceDemand - 5);
                                break;
                        }
                    }
                    instance1.m_goodAreaFound[(int)zone] = (short)1024;
                }
            
        }

        [Fixme("Make this a lightweight wrapper")]
        internal static bool IsGoodPlace(ZoneBlock x, Vector2 position)
        {
            int num1 = Mathf.Max((int)(((double)position.x - 104.0) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)position.y - 104.0) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)position.x + 104.0) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)position.y + 104.0) / 64.0 + 135.0), 269);
            Array16<Building> array16 = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] numArray = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num5 = numArray[index1 * 270 + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        if ((array16.m_buffer[(int)num5].m_flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                        {
                            BuildingInfo info;
                            int width;
                            int length;
                            array16.m_buffer[(int)num5].GetInfoWidthLength(out info, out width, out length);
                            if (info != null)
                            {
                                float b = info.m_buildingAI.ElectricityGridRadius();
                                if ((double)b > 0.100000001490116 || info.m_class.m_service == ItemClass.Service.Electricity)
                                {
                                    Vector2 vector2 = VectorUtils.XZ(array16.m_buffer[(int)num5].m_position);
                                    float num7 = Mathf.Max(8f, b) + 32f;
                                    if ((double)Vector2.SqrMagnitude(position - vector2) < (double)num7 * (double)num7)
                                        return true;
                                }
                            }
                        }
                        num5 = array16.m_buffer[(int)num5].m_nextGridBuilding;
                        if (++num6 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return false;
        }

        [Fixme("Make this a lightweight wrapper")]
        internal static void CheckBlock(ZoneBlock b, ref ZoneBlock other, int[] xBuffer, ItemClass.Zone zone, Vector2 startPos, Vector2 xDir, Vector2 zDir, Quad2 quad)
        {
            float f1 = Mathf.Abs(other.m_angle - b.m_angle) * 0.6366197f;
            float num1 = f1 - Mathf.Floor(f1);
            if ((double)num1 >= 0.00999999977648258 && (double)num1 <= 0.990000009536743)
                return;
            int rowCount = other.RowCount;
            Vector2 vector2_1 = new Vector2(Mathf.Cos(other.m_angle), Mathf.Sin(other.m_angle)) * 8f;
            Vector2 vector2_2 = new Vector2(vector2_1.y, -vector2_1.x);
            ulong num2 = other.m_valid & (ulong)~((long)other.m_occupied1 | (long)other.m_occupied2);
            Vector2 vector2_3 = VectorUtils.XZ(other.m_position);
            if (!quad.Intersect(new Quad2()
            {
                a = vector2_3 - 4f * vector2_1 - 4f * vector2_2,
                b = vector2_3 - 4f * vector2_2,
                c = vector2_3 + (float)(rowCount - 4) * vector2_2,
                d = vector2_3 - 4f * vector2_1 + (float)(rowCount - 4) * vector2_2
            }))
                return;
            for (int z = 0; z < rowCount; ++z)
            {
                Vector2 vector2_4 = ((float)z - 3.5f) * vector2_2;
                for (int x = 0; x < 4; ++x)
                {
                    if (((long)num2 & 1L << (z << 3 | x)) != 0L && other.GetZone(x, z) == zone)
                    {
                        Vector2 vector2_5 = ((float)x - 3.5f) * vector2_1;
                        Vector2 vector2_6 = vector2_3 + vector2_5 + vector2_4 - startPos;
                        float f2 = (float)(((double)vector2_6.x * (double)xDir.x + (double)vector2_6.y * (double)xDir.y) * (1.0 / 64.0));
                        float f3 = (float)(((double)vector2_6.x * (double)zDir.x + (double)vector2_6.y * (double)zDir.y) * (1.0 / 64.0));
                        int num3 = Mathf.RoundToInt(f2);
                        int num4 = Mathf.RoundToInt(f3);
                        if (num3 >= 0 && num3 <= 6 && (num4 >= -6 && num4 <= 6) && ((double)Mathf.Abs(f2 - (float)num3) < 0.0125000001862645 && (double)Mathf.Abs(f3 - (float)num4) < 0.0125000001862645 && (x == 0 || num3 != 0)))
                        {
                            xBuffer[num4 + 6] |= 1 << num3;
                            if (x == 0)
                                xBuffer[num4 + 6] |= 1 << num3 + 16;
                        }
                    }
                }
            }
        }
    }
}
