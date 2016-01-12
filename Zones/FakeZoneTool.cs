using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using EightyOne.Redirection;

namespace EightyOne.Zones
{

    //TODO(earalov): review this class
    [TargetType(typeof(ZoneTool))]
    internal class FakeZoneTool
    {

        private static FastList<FillPos> m_fillPositions;
        public static void Init()
        {
            m_fillPositions = new FastList<FillPos>();
        }

        [RedirectReverse]
        private static void UsedZone(ZoneTool z, ItemClass.Zone zone)
        {
            UnityEngine.Debug.Log($"{z}-{zone}");
        }

        [RedirectReverse]
        private static bool ApplyZoning(ZoneTool z, ushort blockIndex, ref ZoneBlock data, Quad2 quad2)
        {
            UnityEngine.Debug.Log($"{z}-{blockIndex}-{data}-{quad2}");
            return false;
        }

        [RedirectReverse]
        private static bool ApplyFillBuffer(ZoneTool z, Vector3 position, Vector3 direction, float angle, ushort blockIndex, ref ZoneBlock block)
        {
            UnityEngine.Debug.Log($"{z}-{position}-{direction}-{angle}-{blockIndex}-{block}");
            return false;
        }

        [RedirectMethod]
        private static void ApplyBrush(ZoneTool z)
        {
            float num = z.m_brushSize * 0.5f;
            float brushRadius = z.m_brushSize * 0.5f;
            Vector3 mousePosition = (Vector3)z.GetType().GetField("m_mousePosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            float num2 = mousePosition.x - num;
            float num3 = mousePosition.z - num;
            float num4 = mousePosition.x + num;
            float num5 = mousePosition.z + num;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            int num6 = Mathf.Max((int)((num2 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num7 = Mathf.Max((int)((num3 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num8 = Mathf.Min((int)((num4 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num9 = Mathf.Min((int)((num5 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            for (int i = num7; i <= num9; i++)
            {
                for (int j = num6; j <= num8; j++)
                {
                    ushort num10 = instance.m_zoneGrid[i * FakeZoneManager.GRIDSIZE + j];
                    int num11 = 0;
                    while (num10 != 0)
                    {
                        Vector3 position = instance.m_blocks.m_buffer[(int)num10].m_position;
                        float num12 = Mathf.Max(Mathf.Max(num2 - 46f - position.x, num3 - 46f - position.z), Mathf.Max(position.x - num4 - 46f, position.z - num5 - 46f));
                        if (num12 < 0f)
                        {
                            ApplyBrush(z,num10,ref instance.m_blocks.m_buffer[(int)num10], mousePosition, num);                            
                        }
                        num10 = instance.m_blocks.m_buffer[(int)num10].m_nextGridBlock;
                        if (++num11 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private static void ApplyBrush(ZoneTool z,ushort blockIndex, ref ZoneBlock data, Vector3 position, float brushRadius)
        {
            Vector3 a = data.m_position - position;
            if (Mathf.Abs(a.x) > 46f + brushRadius || Mathf.Abs(a.z) > 46f + brushRadius)
            {
                return;
            }

            bool m_zoning = (bool)z.GetType().GetField("m_zoning", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            bool m_dezoning = (bool)z.GetType().GetField("m_dezoning", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            int num = (int)((data.m_flags & 65280u) >> 8);
            Vector3 a2 = new Vector3(Mathf.Cos(data.m_angle), 0f, Mathf.Sin(data.m_angle)) * 8f;
            Vector3 a3 = new Vector3(a2.z, 0f, -a2.x);
            bool flag = false;
            for (int i = 0; i < num; i++)
            {
                Vector3 b = ((float)i - 3.5f) * a3;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 b2 = ((float)j - 3.5f) * a2;
                    Vector3 vector = a + b2 + b;
                    float num2 = vector.x * vector.x + vector.z * vector.z;
                    if (num2 <= brushRadius * brushRadius)
                    {
                        if (m_zoning)
                        {
                            if ((z.m_zone == ItemClass.Zone.Unzoned || data.GetZone(j, i) == ItemClass.Zone.Unzoned) && data.SetZone(j, i, z.m_zone))
                            {
                                flag = true;
                            }
                        }
                        else if (m_dezoning && data.SetZone(j, i, ItemClass.Zone.Unzoned))
                        {
                            flag = true;
                        }
                    }
                }
            }
            if (!flag)
                return;
            data.RefreshZoning(blockIndex);
            if (!m_zoning)
                return;
            UsedZone(z, z.m_zone);
        }

        [RedirectMethod]
        private static void ApplyFill(ZoneTool z)
        {
            bool m_validPosition = (bool)z.GetType().GetField("m_validPosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            if (!m_validPosition)
                return;

            Vector3 position = (Vector3)z.GetType().GetField("m_mousePosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            Vector3 direction = (Vector3)z.GetType().GetField("m_mouseDirection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            float angle = Mathf.Atan2(-direction.x, direction.z);
            float num1 = position.x - 256f;
            float num2 = position.z - 256f;
            float num3 = position.x + 256f;
            float num4 = position.z + 256f;
            int num5 = Mathf.Max((int)((num1 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num6 = Mathf.Max((int)((num2 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num7 = Mathf.Min((int)((num3 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num8 = Mathf.Min((int)((num4 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            ZoneManager instance1 = Singleton<ZoneManager>.instance;
            bool flag = false;
            for (int index1 = num6; index1 <= num8; ++index1)
            {
                for (int index2 = num5; index2 <= num7; ++index2)
                {
                    ushort blockIndex = instance1.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                    int num9 = 0;
                    while ((int)blockIndex != 0)
                    {
                        Vector3 vector3 = instance1.m_blocks.m_buffer[(int)blockIndex].m_position;
                        if ((double)Mathf.Max(Mathf.Max(num1 - 46f - vector3.x, num2 - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)num3 - 46.0), (float)((double)vector3.z - (double)num4 - 46.0))) < 0.0 && ApplyFillBuffer(z, position, direction, angle, blockIndex, ref instance1.m_blocks.m_buffer[(int)blockIndex]))
                            flag = true;
                        blockIndex = instance1.m_blocks.m_buffer[(int)blockIndex].m_nextGridBlock;
                        if (++num9 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            if (!flag)
                return;

            bool m_zoning = (bool)z.GetType().GetField("m_validPosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            if (m_zoning)
                UsedZone(z, z.m_zone);
            EffectInfo effect = instance1.m_properties.m_fillEffect;
            if (effect == null)
                return;
            InstanceID instance2 = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(position, Vector3.up, 1f);
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance2, spawnArea, Vector3.zero, 0.0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
        }

        [RedirectMethod]
        private static void ApplyZoning(ZoneTool z)
        {
            Vector3 m_startPosition = (Vector3)z.GetType().GetField("m_startPosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            Vector3 m_mousePosition = (Vector3)z.GetType().GetField("m_mousePosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            Vector3 m_startDirection = (Vector3)z.GetType().GetField("m_startDirection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);

            Vector2 vector2_1 = VectorUtils.XZ(m_startPosition);
            Vector2 vector2_2 = VectorUtils.XZ(m_mousePosition);
            Vector2 vector2_3 = VectorUtils.XZ(m_startDirection);
            Vector2 vector2_4 = new Vector2(vector2_3.y, -vector2_3.x);
            float num1 = Mathf.Round((float)((((double)vector2_2.x - (double)vector2_1.x) * (double)vector2_3.x + ((double)vector2_2.y - (double)vector2_1.y) * (double)vector2_3.y) * 0.125)) * 8f;
            float num2 = Mathf.Round((float)((((double)vector2_2.x - (double)vector2_1.x) * (double)vector2_4.x + ((double)vector2_2.y - (double)vector2_1.y) * (double)vector2_4.y) * 0.125)) * 8f;
            float num3 = (double)num1 < 0.0 ? -4f : 4f;
            float num4 = (double)num2 < 0.0 ? -4f : 4f;
            Quad2 quad2 = new Quad2();
            quad2.a = vector2_1 - vector2_3 * num3 - vector2_4 * num4;
            quad2.b = vector2_1 - vector2_3 * num3 + vector2_4 * (num2 + num4);
            quad2.c = vector2_1 + vector2_3 * (num1 + num3) + vector2_4 * (num2 + num4);
            quad2.d = vector2_1 + vector2_3 * (num1 + num3) - vector2_4 * num4;
            if ((double)num3 == (double)num4)
            {
                Vector2 vector2_5 = quad2.b;
                quad2.b = quad2.d;
                quad2.d = vector2_5;
            }
            Vector2 vector2_6 = quad2.Min();
            Vector2 vector2_7 = quad2.Max();
            int num5 = Mathf.Max((int)((vector2_6.x - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num6 = Mathf.Max((int)((vector2_6.y - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
            int num7 = Mathf.Min((int)((vector2_7.x + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num8 = Mathf.Min((int)((vector2_7.y + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            var instance1 = ZoneManager.instance;
            bool flag = false;
            for (int index1 = num6; index1 <= num8; ++index1)
            {
                for (int index2 = num5; index2 <= num7; ++index2)
                {
                    ushort blockIndex = instance1.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                    int num9 = 0;
                    while ((int)blockIndex != 0)
                    {
                        Vector3 vector3 = instance1.m_blocks.m_buffer[(int)blockIndex].m_position;
                        if ((double)Mathf.Max(Mathf.Max(vector2_6.x - 46f - vector3.x, vector2_6.y - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)vector2_7.x - 46.0), (float)((double)vector3.z - (double)vector2_7.y - 46.0))) < 0.0 && ApplyZoning(z, blockIndex, ref instance1.m_blocks.m_buffer[(int)blockIndex], quad2))
                            flag = true;
                        blockIndex = instance1.m_blocks.m_buffer[(int)blockIndex].m_nextGridBlock;
                        if (++num9 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            if (!flag)
                return;
            bool m_zoning = (bool)z.GetType().GetField("m_validPosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            if (m_zoning)
                UsedZone(z, z.m_zone);
            EffectInfo effect = instance1.m_properties.m_fillEffect;
            if (effect == null)
                return;
            InstanceID instance2 = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea((Vector3)((vector2_1 + vector2_2) * 0.5f), Vector3.up, 1f);
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance2, spawnArea, Vector3.zero, 0.0f, 1f, Singleton<AudioManager>.instance.DefaultGroup);
        }

        private static void CalculateFillBuffer(ZoneTool zt, Vector3 position, Vector3 direction, float angle, ushort blockIndex, ref ZoneBlock block, ItemClass.Zone requiredZone, bool occupied1, bool occupied2)
        {
            var m_fillBuffer1 = (ulong[])typeof(ZoneTool).GetField("m_fillBuffer1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(zt);

            float f1 = Mathf.Abs(block.m_angle - angle) * 0.6366197f;
            float num1 = f1 - Mathf.Floor(f1);
            if ((double)num1 >= 0.00999999977648258 && (double)num1 <= 0.990000009536743)
                return;
            int rowCount = block.RowCount;
            Vector3 vector3_1 = new Vector3(Mathf.Cos(block.m_angle), 0.0f, Mathf.Sin(block.m_angle)) * 8f;
            Vector3 vector3_2 = new Vector3(vector3_1.z, 0.0f, -vector3_1.x);
            for (int z = 0; z < rowCount; ++z)
            {
                Vector3 vector3_3 = ((float)z - 3.5f) * vector3_2;
                for (int x = 0; x < 4; ++x)
                {
                    if (((long)block.m_valid & 1L << (z << 3 | x)) != 0L && block.GetZone(x, z) == requiredZone)
                    {
                        if (occupied1)
                        {
                            if (requiredZone == ItemClass.Zone.Unzoned && ((long)block.m_occupied1 & 1L << (z << 3 | x)) == 0L)
                                continue;
                        }
                        else if (occupied2)
                        {
                            if (requiredZone == ItemClass.Zone.Unzoned && ((long)block.m_occupied2 & 1L << (z << 3 | x)) == 0L)
                                continue;
                        }
                        else if ((((long)block.m_occupied1 | (long)block.m_occupied2) & 1L << (z << 3 | x)) != 0L)
                            continue;
                        Vector3 vector3_4 = ((float)x - 3.5f) * vector3_1;
                        Vector3 vector3_5 = block.m_position + vector3_4 + vector3_3 - position;
                        float f2 = (float)(((double)vector3_5.x * (double)direction.x + (double)vector3_5.z * (double)direction.z) * 0.125 + 32.0);
                        float f3 = (float)(((double)vector3_5.x * (double)direction.z - (double)vector3_5.z * (double)direction.x) * 0.125 + 32.0);
                        int num2 = Mathf.RoundToInt(f2);
                        int index = Mathf.RoundToInt(f3);
                        if (num2 >= 0 && num2 < 64 && (index >= 0 && index < 64) && ((double)Mathf.Abs(f2 - (float)num2) < 0.0125000001862645 && (double)Mathf.Abs(f3 - (float)index) < 0.0125000001862645))
                            m_fillBuffer1[index] |= (ulong)(1L << num2);
                    }
                }
            }
        }

        [RedirectMethod]
        private static bool CalculateFillBuffer(ZoneTool z, Vector3 position, Vector3 direction, ItemClass.Zone requiredZone, bool occupied1, bool occupied2)
        {
            var m_fillBuffer1 = (ulong[])typeof(ZoneTool).GetField("m_fillBuffer1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(z);

            for (int index = 0; index < 64; ++index)
                m_fillBuffer1[index] = 0UL;
            if (!occupied2)
            {
                float angle = Mathf.Atan2(-direction.x, direction.z);
                float num1 = position.x - 256f;
                float num2 = position.z - 256f;
                float num3 = position.x + 256f;
                float num4 = position.z + 256f;
                int num5 = Mathf.Max((int)((num1 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
                int num6 = Mathf.Max((int)((num2 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
                int num7 = Mathf.Min((int)((num3 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                int num8 = Mathf.Min((int)((num4 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                ZoneManager instance = Singleton<ZoneManager>.instance;
                for (int index1 = num6; index1 <= num8; ++index1)
                {
                    for (int index2 = num5; index2 <= num7; ++index2)
                    {
                        ushort blockIndex = instance.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                        int num9 = 0;
                        while ((int)blockIndex != 0)
                        {
                            Vector3 vector3 = instance.m_blocks.m_buffer[(int)blockIndex].m_position;
                            if ((double)Mathf.Max(Mathf.Max(num1 - 46f - vector3.x, num2 - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)num3 - 46.0), (float)((double)vector3.z - (double)num4 - 46.0))) < 0.0)
                                CalculateFillBuffer(z, position, direction, angle, blockIndex, ref instance.m_blocks.m_buffer[(int)blockIndex], requiredZone, occupied1, occupied2);
                            blockIndex = instance.m_blocks.m_buffer[(int)blockIndex].m_nextGridBlock;
                            if (++num9 >= ZoneManager.MAX_BLOCK_COUNT)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
            }
            if (((long)m_fillBuffer1[32] & 4294967296L) != 0L)
            {
                m_fillPositions.Clear();
                int num1 = 0;
                int num2 = 32;
                int num3 = 32;
                int num4 = 32;
                int num5 = 32;
                FillPos fillPos1;
                fillPos1.m_x = (byte)32;
                fillPos1.m_z = (byte)32;
                m_fillPositions.Add(fillPos1);
                m_fillBuffer1[32] &= 18446744069414584319UL;
                while (num1 < m_fillPositions.m_size)
                {
                    FillPos fillPos2 = m_fillPositions.m_buffer[num1++];
                    if ((int)fillPos2.m_z > 0)
                    {
                        FillPos fillPos3 = fillPos2;
                        --fillPos3.m_z;
                        if (((long)m_fillBuffer1[(int)fillPos3.m_z] & 1L << (int)fillPos3.m_x) != 0L)
                        {
                            m_fillPositions.Add(fillPos3);
                            m_fillBuffer1[(int)fillPos3.m_z] &= (ulong)~(1L << (int)fillPos3.m_x);
                            if ((int)fillPos3.m_z < num3)
                                num3 = (int)fillPos3.m_z;
                        }
                    }
                    if ((int)fillPos2.m_x > 0)
                    {
                        FillPos fillPos3 = fillPos2;
                        --fillPos3.m_x;
                        if (((long)m_fillBuffer1[(int)fillPos3.m_z] & 1L << (int)fillPos3.m_x) != 0L)
                        {
                            m_fillPositions.Add(fillPos3);
                            m_fillBuffer1[(int)fillPos3.m_z] &= (ulong)~(1L << (int)fillPos3.m_x);
                            if ((int)fillPos3.m_x < num2)
                                num2 = (int)fillPos3.m_x;
                        }
                    }
                    if ((int)fillPos2.m_x < 63)
                    {
                        FillPos fillPos3 = fillPos2;
                        ++fillPos3.m_x;
                        if (((long)m_fillBuffer1[(int)fillPos3.m_z] & 1L << (int)fillPos3.m_x) != 0L)
                        {
                            m_fillPositions.Add(fillPos3);
                            m_fillBuffer1[(int)fillPos3.m_z] &= (ulong)~(1L << (int)fillPos3.m_x);
                            if ((int)fillPos3.m_x > num4)
                                num4 = (int)fillPos3.m_x;
                        }
                    }
                    if ((int)fillPos2.m_z < 63)
                    {
                        FillPos fillPos3 = fillPos2;
                        ++fillPos3.m_z;
                        if (((long)m_fillBuffer1[(int)fillPos3.m_z] & 1L << (int)fillPos3.m_x) != 0L)
                        {
                            m_fillPositions.Add(fillPos3);
                            m_fillBuffer1[(int)fillPos3.m_z] &= (ulong)~(1L << (int)fillPos3.m_x);
                            if ((int)fillPos3.m_z > num5)
                                num5 = (int)fillPos3.m_z;
                        }
                    }
                }
                for (int index = 0; index < 64; ++index)
                    m_fillBuffer1[index] = 0UL;
                for (int index = 0; index < m_fillPositions.m_size; ++index)
                {
                    FillPos fillPos2 = m_fillPositions.m_buffer[index];
                    m_fillBuffer1[(int)fillPos2.m_z] |= (ulong)(1L << (int)fillPos2.m_x);
                }
                return true;
            }
            for (int index = 0; index < 64; ++index)
                m_fillBuffer1[index] = 0UL;
            return false;
        }

        private struct FillPos
        {
            public byte m_x;
            public byte m_z;
        }
    }
}
