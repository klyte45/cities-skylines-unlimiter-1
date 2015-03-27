using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Unlimiter.Areas
{
    internal class FakeZoneManager
    {
        private class Helper
        {
            internal static void EnsureInit()
            {
                if (ZoneManager.instance.m_zoneGrid.Length != ZONEGRID_RESOLUTION * ZONEGRID_RESOLUTION)
                {
                    var grid = new ushort[ZONEGRID_RESOLUTION * ZONEGRID_RESOLUTION];

                    int diff = ZONEGRID_RESOLUTION - DEFAULT_ZONEGRID_RESOLUTION;
                    for (int z = 0; z < DEFAULT_ZONEGRID_RESOLUTION; ++z)
                    {
                        for (int x = 0; x < DEFAULT_ZONEGRID_RESOLUTION; ++x)
                        {
                            grid[(z + diff) * ZONEGRID_RESOLUTION + (x + diff)] = ZoneManager.instance.m_zoneGrid[z * DEFAULT_ZONEGRID_RESOLUTION + x];
                        }
                    }

                    ZoneManager.instance.m_zoneGrid = grid;
                }
            }
        }

        // We basically want to increase the grid resolution.
        // 
        internal const int DEFAULT_ZONEGRID_RESOLUTION = 150;
        internal const int ZONEGRID_RESOLUTION = 210;
        internal const float ZONEGRID_CELL_SIZE = 64f;
        internal const float UNKNOWN_FLOAT_75 = 75.0f;

        private static void InitializeBlock(ZoneManager z, ushort block, ref ZoneBlock data)
        {
            Helper.EnsureInit();
            int num = Mathf.Clamp((int)((double)data.m_position.x / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0, ZONEGRID_RESOLUTION - 1);
            int index = Mathf.Clamp((int)((double)data.m_position.z / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0, ZONEGRID_RESOLUTION - 1) * ZONEGRID_RESOLUTION + num;
            do
            { }
            while (!Monitor.TryEnter((object)z.m_zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                z.m_blocks.m_buffer[(int)block].m_nextGridBlock = z.m_zoneGrid[index];
                z.m_zoneGrid[index] = block;
            }
            finally
            {
                Monitor.Exit((object)z.m_zoneGrid);
            }
        }

        private static void ReleaseBlockImplementation(ZoneManager z, ushort block, ref ZoneBlock data)
        {
            if ((int)data.m_flags == 0)
                return;
            data.m_flags |= 2U;
            z.m_cachedBlocks.Add(data);
            data.m_flags = 0U;
            int num1 = Mathf.Clamp((int)((double)data.m_position.x / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0, ZONEGRID_RESOLUTION - 1);
            int index = Mathf.Clamp((int)((double)data.m_position.z / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0, ZONEGRID_RESOLUTION - 1) * ZONEGRID_RESOLUTION + num1;
            do
            { }
            while (!Monitor.TryEnter((object)z.m_zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                ushort num2 = (ushort)0;
                ushort num3 = z.m_zoneGrid[index];
                int num4 = 0;
                while ((int)num3 != 0)
                {
                    if ((int)num3 == (int)block)
                    {
                        if ((int)num2 == 0)
                        {
                            z.m_zoneGrid[index] = data.m_nextGridBlock;
                            break;
                        }
                        z.m_blocks.m_buffer[(int)num2].m_nextGridBlock = data.m_nextGridBlock;
                        break;
                    }
                    num2 = num3;
                    num3 = z.m_blocks.m_buffer[(int)num3].m_nextGridBlock;
                    if (++num4 > 32768)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
                data.m_nextGridBlock = (ushort)0;
            }
            finally
            {
                Monitor.Exit((object)z.m_zoneGrid);
            }
            z.m_blocks.ReleaseItem(block);
            z.m_blockCount = (int)z.m_blocks.ItemCount() - 1;
        }

        private static void UpdateBlocks(ZoneManager z, float minX, float minZ, float maxX, float maxZ)
        {
            // Where the hell does this 46 come from
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num5 = z.m_zoneGrid[index1 * ZONEGRID_RESOLUTION + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 vector3 = z.m_blocks.m_buffer[(int)num5].m_position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 46f - vector3.x, minZ - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)maxX - 46.0), (float)((double)vector3.z - (double)maxZ - 46.0))) < 0.0)
                        {
                            z.m_updatedBlocks[(int)num5 >> 6] |= (ulong)(1L << (int)num5);
                            z.m_blocksUpdated = true;
                        }
                        num5 = z.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private static void TerrainUpdated(ZoneManager z, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            float minX = zoneArea.m_min.x;
            float minZ = zoneArea.m_min.z;
            float maxX = zoneArea.m_max.x;
            float maxZ = zoneArea.m_max.z;
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort blockID = z.m_zoneGrid[index1 * ZONEGRID_RESOLUTION + index2];
                    int num5 = 0;
                    while ((int)blockID != 0)
                    {
                        Vector3 vector3 = z.m_blocks.m_buffer[(int)blockID].m_position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 46f - vector3.x, minZ - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)maxX - 46.0), (float)((double)vector3.z - (double)maxZ - 46.0))) < 0.0)
                            z.m_blocks.m_buffer[(int)blockID].ZonesUpdated(blockID, minX, minZ, maxX, maxZ);
                        blockID = z.m_blocks.m_buffer[(int)blockID].m_nextGridBlock;
                        if (++num5 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        public static bool CheckSpace(ZoneManager z, Vector3 position, float angle, int width, int length, out int offset)
        {
            float num1 = Mathf.Min(72f, (float)(width + length) * 4f) + 6f;
            float num2 = position.x - num1;
            float num3 = position.z - num1;
            float num4 = position.x + num1;
            float num5 = position.z + num1;
            ulong space1 = 0UL;
            ulong space2 = 0UL;
            ulong space3 = 0UL;
            ulong space4 = 0UL;
            int num6 = Mathf.Max((int)(((double)num2 - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num7 = Mathf.Max((int)(((double)num3 - 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), 0);
            int num8 = Mathf.Min((int)(((double)num4 + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            int num9 = Mathf.Min((int)(((double)num5 + 46.0) / ZONEGRID_CELL_SIZE + UNKNOWN_FLOAT_75), ZONEGRID_RESOLUTION - 1);
            for (int index1 = num7; index1 <= num9; ++index1)
            {
                for (int index2 = num6; index2 <= num8; ++index2)
                {
                    ushort block = z.m_zoneGrid[index1 * ZONEGRID_RESOLUTION + index2];
                    int num10 = 0;
                    while ((int)block != 0)
                    {
                        Vector3 vector3 = z.m_blocks.m_buffer[(int)block].m_position;
                        if ((double)Mathf.Max(Mathf.Max(num2 - 46f - vector3.x, num3 - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)num4 - 46.0), (float)((double)vector3.z - (double)num5 - 46.0))) < 0.0)
                            z.CheckSpace(block, position, angle, width, length, ref space1, ref space2, ref space3, ref space4);
                        block = z.m_blocks.m_buffer[(int)block].m_nextGridBlock;
                        if (++num10 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            bool flag1 = true;
            bool flag2 = false;
            bool flag3 = false;
            for (int index1 = 0; index1 < length; ++index1)
            {
                for (int index2 = 0; index2 < width; ++index2)
                {
                    if (!(index1 >= 4 ? (index1 >= 8 ? (index1 >= 12 ? ((long)space4 & 1L << (index1 - 12 << 4 | index2)) != 0L : ((long)space3 & 1L << (index1 - 8 << 4 | index2)) != 0L) : ((long)space2 & 1L << (index1 - 4 << 4 | index2)) != 0L) : ((long)space1 & 1L << (index1 << 4 | index2)) != 0L))
                    {
                        flag1 = false;
                        if (index2 < width >> 1)
                            flag2 = true;
                        if (index2 >= width + 1 >> 1)
                            flag3 = true;
                    }
                }
            }
            offset = flag2 != flag3 ? (!flag2 ? (!flag3 ? 0 : -1) : 1) : 0;
            return flag1;
        }
    }
}
