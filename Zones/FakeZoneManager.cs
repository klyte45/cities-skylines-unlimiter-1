using ColossalFramework;
using System.Threading;
using ColossalFramework.Math;
using UnityEngine;
using EightyOne.Redirection;

namespace EightyOne.Zones
{
    [TargetType(typeof(ZoneManager))]
    internal class FakeZoneManager : ZoneManager
    {
        public const int GRIDSIZE = 270;
        public const int HALFGRID = 135;

        public static void Init()
        {
            var oldGrid = instance.m_zoneGrid;
            while (!Monitor.TryEnter(oldGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {

            }
            try
            {
                instance.m_zoneGrid = new ushort[GRIDSIZE * GRIDSIZE];
                for (var i = 1; i < instance.m_blocks.m_buffer.Length; ++i)
                {
                    instance.m_blocks.m_buffer[i].m_nextGridBlock = 0;
                    if (instance.m_blocks.m_buffer[i].m_flags != 0)
                    {
                        InitializeBlock(instance, (ushort)i, ref instance.m_blocks.m_buffer[i]);
                    }
                }
            }
            finally
            {
                Monitor.Exit(oldGrid);
            }
        }

        [RedirectMethod]
        private static void InitializeBlock(ZoneManager zm, ushort block, ref ZoneBlock data)
        {
            //begin mod
            int num = Mathf.Clamp((int)(data.m_position.x / 64.0 + HALFGRID), 0, GRIDSIZE - 1);
            int index = Mathf.Clamp((int)(data.m_position.z / 64.0 + HALFGRID), 0, GRIDSIZE - 1) * GRIDSIZE + num;
            //end mod
            while (!Monitor.TryEnter(zm.m_zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {

            }
            try
            {
                zm.m_blocks.m_buffer[block].m_nextGridBlock = zm.m_zoneGrid[index];
                zm.m_zoneGrid[index] = block;
            }
            finally
            {
                Monitor.Exit(zm.m_zoneGrid);
            }

        }

        [RedirectMethod]
        private void ReleaseBlockImplementation(ushort block, ref ZoneBlock data)
        {
            if ((int)data.m_flags == 0)
                return;
            data.m_flags |= 2U;
            this.m_cachedBlocks.Add(data);
            int rowCount = data.RowCount;
            float f = data.m_angle;
            Vector2 vector2_1 = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * 8f;
            Vector2 vector2_2 = new Vector2(vector2_1.y, -vector2_1.x);
            Vector2 vector2_3 = VectorUtils.XZ(data.m_position);
            this.UpdateBlocks(new Quad2()
            {
                a = vector2_3 - 4f * vector2_1 - 4f * vector2_2,
                b = vector2_3 + 0.0f * vector2_1 - 4f * vector2_2,
                c = vector2_3 + 0.0f * vector2_1 + (float)(rowCount - 4) * vector2_2,
                d = vector2_3 - 4f * vector2_1 + (float)(rowCount - 4) * vector2_2
            });
            data.m_flags = 0U;
            //begin mod
            int num1 = Mathf.Clamp((int)((double)data.m_position.x / 64.0 + HALFGRID), 0, GRIDSIZE - 1);
            int index = Mathf.Clamp((int)((double)data.m_position.z / 64.0 + HALFGRID), 0, GRIDSIZE - 1) * GRIDSIZE + num1;
            //end mod
            do
                ;
            while (!Monitor.TryEnter((object)this.m_zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT));
            try
            {
                ushort num2 = (ushort)0;
                ushort num3 = this.m_zoneGrid[index];
                int num4 = 0;
                while ((int)num3 != 0)
                {
                    if ((int)num3 == (int)block)
                    {
                        if ((int)num2 == 0)
                        {
                            this.m_zoneGrid[index] = data.m_nextGridBlock;
                            break;
                        }
                        this.m_blocks.m_buffer[(int)num2].m_nextGridBlock = data.m_nextGridBlock;
                        break;
                    }
                    num2 = num3;
                    num3 = this.m_blocks.m_buffer[(int)num3].m_nextGridBlock;
                    if (++num4 > 49152)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                        break;
                    }
                }
                data.m_nextGridBlock = (ushort)0;
            }
            finally
            {
                Monitor.Exit((object)this.m_zoneGrid);
            }
            this.m_blocks.ReleaseItem(block);
            this.m_blockCount = (int)this.m_blocks.ItemCount() - 1;
        }

        [RedirectMethod]
        public void UpdateBlocks(Quad2 quad)
        {
            Vector2 vector2_1 = quad.Min();
            Vector2 vector2_2 = quad.Max();
            //begin mod
            int num1 = Mathf.Max((int)(((double)vector2_1.x - 46.0) / 64.0 + HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)vector2_1.y - 46.0) / 64.0 + HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)vector2_2.x + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            int num4 = Mathf.Min((int)(((double)vector2_2.y + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            //end mod
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    //begin mod
                    ushort num5 = this.m_zoneGrid[index1 * GRIDSIZE + index2];
                    //end mod
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 v = this.m_blocks.m_buffer[(int)num5].m_position;
                        if ((double)Mathf.Max(Mathf.Max(vector2_1.x - 46f - v.x, vector2_1.y - 46f - v.z), Mathf.Max((float)((double)v.x - (double)vector2_2.x - 46.0), (float)((double)v.z - (double)vector2_2.y - 46.0))) < 0.0 && ((int)this.m_blocks.m_buffer[(int)num5].m_flags & 3) == 1)
                        {
                            int rowCount = this.m_blocks.m_buffer[(int)num5].RowCount;
                            float f = this.m_blocks.m_buffer[(int)num5].m_angle;
                            Vector2 vector2_3 = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * 8f;
                            Vector2 vector2_4 = new Vector2(vector2_3.y, -vector2_3.x);
                            Vector2 vector2_5 = VectorUtils.XZ(v);
                            if (quad.Intersect(new Quad2()
                            {
                                a = vector2_5 - 4f * vector2_3 - 4f * vector2_4,
                                b = vector2_5 + 0.0f * vector2_3 - 4f * vector2_4,
                                c = vector2_5 + 0.0f * vector2_3 + (float)(rowCount - 4) * vector2_4,
                                d = vector2_5 - 4f * vector2_3 + (float)(rowCount - 4) * vector2_4
                            }))
                            {
                                this.m_updatedBlocks[(int)num5 >> 6] |= (ulong)(1L << (int)num5);
                                this.m_blocksUpdated = true;
                            }
                        }
                        num5 = this.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }


        [RedirectMethod]
        public void TerrainUpdated(TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            float minX = zoneArea.m_min.x;
            float minZ = zoneArea.m_min.z;
            float maxX = zoneArea.m_max.x;
            float maxZ = zoneArea.m_max.z;
            //begin mod
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / 64.0 + HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / 64.0 + HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            //end mod
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    //begin mod
                    ushort blockID = this.m_zoneGrid[index1 * GRIDSIZE + index2];
                    //end mod
                    int num5 = 0;
                    while ((int)blockID != 0)
                    {
                        Vector3 vector3 = this.m_blocks.m_buffer[(int)blockID].m_position;
                        if ((double)Mathf.Max(Mathf.Max(minX - 46f - vector3.x, minZ - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)maxX - 46.0), (float)((double)vector3.z - (double)maxZ - 46.0))) < 0.0)
                            this.m_blocks.m_buffer[(int)blockID].ZonesUpdated(blockID, minX, minZ, maxX, maxZ);
                        blockID = this.m_blocks.m_buffer[(int)blockID].m_nextGridBlock;
                        if (++num5 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [RedirectMethod]
        public bool CheckSpace(Vector3 position, float angle, int width, int length, out int offset)
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
            //begin mod
            int num6 = Mathf.Max((int)(((double)num2 - 46.0) / 64.0 + HALFGRID), 0);
            int num7 = Mathf.Max((int)(((double)num3 - 46.0) / 64.0 + HALFGRID), 0);
            int num8 = Mathf.Min((int)(((double)num4 + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            int num9 = Mathf.Min((int)(((double)num5 + 46.0) / 64.0 + HALFGRID), GRIDSIZE - 1);
            //end mod
            for (int index1 = num7; index1 <= num9; ++index1)
            {
                for (int index2 = num6; index2 <= num8; ++index2)
                {
                    //begin mod
                    ushort block = this.m_zoneGrid[index1 * GRIDSIZE + index2];
                    //end mod
                    int num10 = 0;
                    while ((int)block != 0)
                    {
                        Vector3 vector3 = this.m_blocks.m_buffer[(int)block].m_position;
                        if ((double)Mathf.Max(Mathf.Max(num2 - 46f - vector3.x, num3 - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)num4 - 46.0), (float)((double)vector3.z - (double)num5 - 46.0))) < 0.0)
                            this.CheckSpace(block, position, angle, width, length, ref space1, ref space2, ref space3, ref space4);
                        block = this.m_blocks.m_buffer[(int)block].m_nextGridBlock;
                        if (++num10 >= 49152)
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
