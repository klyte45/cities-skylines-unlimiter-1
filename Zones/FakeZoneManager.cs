using ColossalFramework;
using ColossalFramework.IO;
using System;
using System.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Zones
{
    [TargetType(typeof(ZoneManager))]
    internal class FakeZoneManager
    {        
        public const int GRIDSIZE = 270;
        public const int HALFGRID = 135;

        public static ushort[] zoneGrid;

        public static void Init()
        {
            zoneGrid = new ushort[GRIDSIZE * GRIDSIZE];

            var zm = ZoneManager.instance;
            for(var i  = 0;  i < zm.m_blocks.m_buffer.Length; i +=1)
            {
                if (zm.m_blocks.m_buffer[i].m_flags != 0)
                {
                    InitializeBlock(zm, (ushort)i, ref zm.m_blocks.m_buffer[i]);
                }
            }
        }

        [ReplaceMethod]
        private static void InitializeBlock(ZoneManager zm, ushort block, ref ZoneBlock data)
        {
            int num = Mathf.Clamp((int)(data.m_position.x / 64f + HALFGRID), 0, GRIDSIZE - 1);
            int num2 = Mathf.Clamp((int)(data.m_position.z / 64f + HALFGRID), 0, GRIDSIZE - 1);
            int num3 = num2 * GRIDSIZE + num;
            while (!Monitor.TryEnter(zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                ZoneManager.instance.m_blocks.m_buffer[(int)block].m_nextGridBlock = zoneGrid[num3];
                zoneGrid[num3] = block;
            }
            finally
            {
                Monitor.Exit(zoneGrid);
            }
        }

        [ReplaceMethod]
        private void ReleaseBlockImplementation(ushort block, ref ZoneBlock data)
        {
            var zm = ZoneManager.instance;
            if (data.m_flags != 0u)
            {
                data.m_flags |= 2u;
                zm.m_cachedBlocks.Add(data);
                data.m_flags = 0u;
                int num = Mathf.Clamp((int)(data.m_position.x / 64f + HALFGRID), 0, GRIDSIZE - 1);
                int num2 = Mathf.Clamp((int)(data.m_position.z / 64f + HALFGRID), 0, GRIDSIZE - 1);
                int num3 = num2 * GRIDSIZE + num;
                while (!Monitor.TryEnter(zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }
                try
                {
                    ushort num4 = 0;
                    ushort num5 = zoneGrid[num3];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        if (num5 == block)
                        {
                            if (num4 == 0)
                            {
                                zoneGrid[num3] = data.m_nextGridBlock;
                            }
                            else
                            {
                                zm.m_blocks.m_buffer[(int)num4].m_nextGridBlock = data.m_nextGridBlock;
                            }
                            break;
                        }
                        num4 = num5;
                        num5 = zm.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 > ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                    data.m_nextGridBlock = 0;
                }
                finally
                {
                    Monitor.Exit(zoneGrid);
                }
                zm.m_blocks.ReleaseItem(block);
                zm.m_blockCount = (int)(zm.m_blocks.ItemCount() - 1u);
            }
        }

        [ReplaceMethod]
        public void UpdateBlocks(float minX, float minZ, float maxX, float maxZ)
        {
            var zm = ZoneManager.instance;
            int num = Mathf.Max((int)((minX - 46f) / 64f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((minZ - 46f) / 64f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((maxX + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            int num4 = Mathf.Min((int)((maxZ + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = zoneGrid[i * GRIDSIZE + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        Vector3 position = zm.m_blocks.m_buffer[(int)num5].m_position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 46f - position.x, minZ - 46f - position.z), Mathf.Max(position.x - maxX - 46f, position.z - maxZ - 46f));
                        if (num7 < 0f)
                        {
                            zm.m_updatedBlocks[num5 >> 6] |= 1uL << (int)num5;
                            zm.m_blocksUpdated = true;
                        }
                        num5 = zm.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public void TerrainUpdated(TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            var zm = ZoneManager.instance;
            float x = zoneArea.m_min.x;
            float z = zoneArea.m_min.z;
            float x2 = zoneArea.m_max.x;
            float z2 = zoneArea.m_max.z;
            int num = Mathf.Max((int)((x - 46f) / 64f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((z - 46f) / 64f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((x2 + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            int num4 = Mathf.Min((int)((z2 + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = zoneGrid[i * GRIDSIZE + j];
                    int num6 = 0;
                    while (num5 != 0)
                    {
                        Vector3 position = zm.m_blocks.m_buffer[(int)num5].m_position;
                        float num7 = Mathf.Max(Mathf.Max(x - 46f - position.x, z - 46f - position.z), Mathf.Max(position.x - x2 - 46f, position.z - z2 - 46f));
                        if (num7 < 0f)
                        {
                            zm.m_blocks.m_buffer[(int)num5].ZonesUpdated(num5, x, z, x2, z2);
                        }
                        num5 = zm.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public bool CheckSpace(Vector3 position, float angle, int width, int length, out int offset)
        {
            var zm = ZoneManager.instance;
            float num = Mathf.Min(72f, (float)(width + length) * 4f) + 6f;
            float num2 = position.x - num;
            float num3 = position.z - num;
            float num4 = position.x + num;
            float num5 = position.z + num;
            ulong num6 = 0uL;
            ulong num7 = 0uL;
            ulong num8 = 0uL;
            ulong num9 = 0uL;
            int num10 = Mathf.Max((int)((num2 - 46f) / 64f + HALFGRID), 0);
            int num11 = Mathf.Max((int)((num3 - 46f) / 64f + HALFGRID), 0);
            int num12 = Mathf.Min((int)((num4 + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            int num13 = Mathf.Min((int)((num5 + 46f) / 64f + HALFGRID), GRIDSIZE - 1);
            for (int i = num11; i <= num13; i++)
            {
                for (int j = num10; j <= num12; j++)
                {
                    ushort num14 = zoneGrid[i * GRIDSIZE + j];
                    int num15 = 0;
                    while (num14 != 0)
                    {
                        Vector3 position2 = zm.m_blocks.m_buffer[(int)num14].m_position;
                        float num16 = Mathf.Max(Mathf.Max(num2 - 46f - position2.x, num3 - 46f - position2.z), Mathf.Max(position2.x - num4 - 46f, position2.z - num5 - 46f));
                        if (num16 < 0f)
                        {
                            zm.CheckSpace(num14, position, angle, width, length, ref num6, ref num7, ref num8, ref num9);
                        }
                        num14 = zm.m_blocks.m_buffer[(int)num14].m_nextGridBlock;
                        if (++num15 >= ZoneManager.MAX_BLOCK_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            bool result = true;
            bool flag = false;
            bool flag2 = false;
            for (int k = 0; k < length; k++)
            {
                for (int l = 0; l < width; l++)
                {
                    bool flag3;
                    if (k < 4)
                    {
                        flag3 = ((num6 & 1uL << (k << 4 | l)) != 0uL);
                    }
                    else if (k < 8)
                    {
                        flag3 = ((num7 & 1uL << (k - 4 << 4 | l)) != 0uL);
                    }
                    else if (k < 12)
                    {
                        flag3 = ((num8 & 1uL << (k - 8 << 4 | l)) != 0uL);
                    }
                    else
                    {
                        flag3 = ((num9 & 1uL << (k - 12 << 4 | l)) != 0uL);
                    }
                    if (!flag3)
                    {
                        result = false;
                        if (l < width >> 1)
                        {
                            flag = true;
                        }
                        if (l >= width + 1 >> 1)
                        {
                            flag2 = true;
                        }
                    }
                }
            }
            if (flag == flag2)
            {
                offset = 0;
            }
            else if (flag)
            {
                offset = 1;
            }
            else if (flag2)
            {
                offset = -1;
            }
            else
            {
                offset = 0;
            }
            return result;
        }
    }
}
