using ColossalFramework;
using ColossalFramework.IO;
using System.Threading;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Zones
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

                    int diff = EXTENSION;
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

        internal const int EXTENSION = 60;
        internal const int ZONEGRID_RESOLUTION = EXTENSION * 2 + DEFAULT_ZONEGRID_RESOLUTION;
        internal const float ZONEGRID_CELL_SIZE = 64f;
        internal const float HALF_ZONEGRID_RESOLUTION = (float)ZONEGRID_RESOLUTION / 2f;

        [ReplaceMethod]
        public static void InitializeBlock(ZoneManager z, ushort block, ref ZoneBlock data)
        {
            int num = Mathf.Clamp((int)((double)data.m_position.x / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0, ZONEGRID_RESOLUTION - 1);
            int index = Mathf.Clamp((int)((double)data.m_position.z / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0, ZONEGRID_RESOLUTION - 1) * ZONEGRID_RESOLUTION + num;
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

        [ReplaceMethod]
        public static void ReleaseBlockImplementation(ZoneManager z, ushort block, ref ZoneBlock data)
        {
            if ((int)data.m_flags == 0)
                return;
            data.m_flags |= 2U;
            z.m_cachedBlocks.Add(data);
            data.m_flags = 0U;
            int num1 = Mathf.Clamp((int)((double)data.m_position.x / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0, ZONEGRID_RESOLUTION - 1);
            int index = Mathf.Clamp((int)((double)data.m_position.z / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0, ZONEGRID_RESOLUTION - 1) * ZONEGRID_RESOLUTION + num1;
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

        [ReplaceMethod]
        public static void UpdateBlocks(ZoneManager z, float minX, float minZ, float maxX, float maxZ)
        {
            // Where the hell does this 46 come from
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
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

        [ReplaceMethod]
        public static void TerrainUpdated(ZoneManager z, TerrainArea heightArea, TerrainArea surfaceArea, TerrainArea zoneArea)
        {
            float minX = zoneArea.m_min.x;
            float minZ = zoneArea.m_min.z;
            float maxX = zoneArea.m_max.x;
            float maxZ = zoneArea.m_max.z;
            int num1 = Mathf.Max((int)(((double)minX - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num2 = Mathf.Max((int)(((double)minZ - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num3 = Mathf.Min((int)(((double)maxX + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)maxZ + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
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

        [ReplaceMethod]
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
            int num6 = Mathf.Max((int)(((double)num2 - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num7 = Mathf.Max((int)(((double)num3 - 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), 0);
            int num8 = Mathf.Min((int)(((double)num4 + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
            int num9 = Mathf.Min((int)(((double)num5 + 46.0) / ZONEGRID_CELL_SIZE + HALF_ZONEGRID_RESOLUTION), ZONEGRID_RESOLUTION - 1);
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

        [ReplaceMethod]
        public static void SimulationStepImpl(ZoneManager z, int subStep)
        {
            if (z.m_blocksUpdated)
            {
                int length = z.m_updatedBlocks.Length;
                for (int index1 = 0; index1 < length; ++index1)
                {
                    ulong num = z.m_updatedBlocks[index1];
                    if ((long)num != 0L)
                    {
                        for (int index2 = 0; index2 < 64; ++index2)
                        {
                            if (((long)num & 1L << index2) != 0L)
                            {
                                ushort blockID = (ushort)(index1 << 6 | index2);
                                z.m_blocks.m_buffer[(int)blockID].CalculateBlock1(blockID);
                            }
                        }
                    }
                }
                for (int index1 = 0; index1 < length; ++index1)
                {
                    ulong num = z.m_updatedBlocks[index1];
                    if ((long)num != 0L)
                    {
                        for (int index2 = 0; index2 < 64; ++index2)
                        {
                            if (((long)num & 1L << index2) != 0L)
                            {
                                ushort blockID = (ushort)(index1 << 6 | index2);
                                FakeZoneBlock.CalculateBlock2(ref z.m_blocks.m_buffer[(int)blockID], blockID);
                            }
                        }
                    }
                }
                for (int index1 = 0; index1 < length; ++index1)
                {
                    ulong num = z.m_updatedBlocks[index1];
                    if ((long)num != 0L)
                    {
                        for (int index2 = 0; index2 < 64; ++index2)
                        {
                            if (((long)num & 1L << index2) != 0L)
                            {
                                ushort blockID = (ushort)(index1 << 6 | index2);
                                z.m_blocks.m_buffer[(int)blockID].CalculateBlock3(blockID);
                            }
                        }
                    }
                }
                z.m_blocksUpdated = false;
                for (int index1 = 0; index1 < length; ++index1)
                {
                    ulong num = z.m_updatedBlocks[index1];
                    if ((long)num != 0L)
                    {
                        z.m_updatedBlocks[index1] = 0UL;
                        for (int index2 = 0; index2 < 64; ++index2)
                        {
                            if (((long)num & 1L << index2) != 0L)
                            {
                                ushort blockID = (ushort)(index1 << 6 | index2);
                                z.m_blocks.m_buffer[(int)blockID].UpdateBlock(blockID);
                            }
                        }
                    }
                }
                for (int index = 0; index < z.m_cachedBlocks.m_size; ++index)
                    z.m_cachedBlocks.m_buffer[index].UpdateBlock((ushort)0);
                z.m_cachedBlocks.Clear();
            }
            GuideController guideController = Singleton<GuideManager>.instance.m_properties;
            if (subStep != 0)
            {
                SimulationManager instance1 = Singleton<SimulationManager>.instance;
                if ((int)instance1.m_currentBuildIndex == (int)z.m_lastBuildIndex)
                {
                    int num1 = (int)instance1.m_currentFrameIndex & 1023;
                    int num2 = num1 * 32;
                    int num3 = (num1 + 1) * 32 - 1;
                    for (int index = num2; index <= num3; ++index)
                    {
                        if (((int)z.m_blocks.m_buffer[index].m_flags & 1) != 0)
                        {
                            FakeZoneBlock.SimulationStep(ref z.m_blocks.m_buffer[index], (ushort)index);
                            if ((int)instance1.m_currentBuildIndex != (int)z.m_lastBuildIndex)
                                break;
                        }
                    }
                    for (int index = 0; index < 8; ++index)
                    {
                        int num4 = (int)z.m_goodAreaFound[index];
                        if (num4 != 0 && num4 > -1024)
                            z.m_goodAreaFound[index] = (short)(num4 - 1);
                    }
                }
                if (((int)instance1.m_currentFrameIndex & 7) == 0)
                    z.m_lastBuildIndex = instance1.m_currentBuildIndex;
                if (((int)instance1.m_currentFrameIndex & (int)byte.MaxValue) == 0)
                {
                    if (z.m_fullDemand)
                    {
                        z.m_actualResidentialDemand = 100;
                        z.m_actualCommercialDemand = 100;
                        z.m_actualWorkplaceDemand = 100;
                        z.m_residentialDemand = 100;
                        z.m_commercialDemand = 100;
                        z.m_workplaceDemand = 100;
                    }
                    else
                    {
                        DistrictManager instance2 = Singleton<DistrictManager>.instance;
                        var p = new object[] { instance2.m_districts.m_buffer[0] };
                        int target1 = (int)typeof(ZoneManager).GetMethod("CalculateResidentialDemand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(z, p);
                        int target2 = (int)typeof(ZoneManager).GetMethod("CalculateCommercialDemand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(z, p);
                        int target3 = (int)typeof(ZoneManager).GetMethod("CalculateWorkplaceDemand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(z, p);
                        UpdateDemand(z, ref z.m_actualResidentialDemand, target1);
                        UpdateDemand(z, ref z.m_actualCommercialDemand, target2);
                        UpdateDemand(z, ref z.m_actualWorkplaceDemand, target3);
                        int target4 = z.m_actualResidentialDemand + instance2.m_districts.m_buffer[0].CalculateResidentialDemandOffset();
                        int target5 = z.m_actualCommercialDemand + instance2.m_districts.m_buffer[0].CalculateCommercialDemandOffset();
                        int target6 = z.m_actualWorkplaceDemand + instance2.m_districts.m_buffer[0].CalculateWorkplaceDemandOffset();
                        UpdateDemand(z, ref z.m_residentialDemand, target4);
                        UpdateDemand(z, ref z.m_commercialDemand, target5);
                        UpdateDemand(z, ref z.m_workplaceDemand, target6);
                    }
                }
            }
            if (subStep <= 1 && guideController != null)
            {
                switch ((int)Singleton<SimulationManager>.instance.m_currentTickIndex & 1023)
                {
                    case 100:
                        if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Zone.ResidentialLow))
                        {
                            if (z.m_residentialDemand > 80)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                                {
                                    z.m_zoneDemandResidential.Activate(guideController.m_generalDemand, ItemClass.Zone.ResidentialLow);
                                    break;
                                }
                                z.m_zoneDemandResidential.Activate(guideController.m_zoningDemand, ItemClass.Zone.ResidentialLow);
                                break;
                            }
                            if (z.m_residentialDemand < 80)
                            {
                                z.m_zoneDemandResidential.Deactivate();
                                break;
                            }
                            break;
                        }
                        break;

                    case 200:
                        if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Zone.CommercialLow))
                        {
                            if (z.m_commercialDemand > 80)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                                {
                                    z.m_zoneDemandCommercial.Activate(guideController.m_generalDemand, ItemClass.Zone.ResidentialLow);
                                    break;
                                }
                                z.m_zoneDemandCommercial.Activate(guideController.m_zoningDemand, ItemClass.Zone.CommercialLow);
                                break;
                            }
                            if (z.m_commercialDemand < 80)
                            {
                                z.m_zoneDemandCommercial.Deactivate();
                                break;
                            }
                            break;
                        }
                        break;

                    case 300:
                        if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Zone.Industrial))
                        {
                            if (z.m_workplaceDemand > 80)
                            {
                                if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
                                {
                                    z.m_zoneDemandWorkplace.Activate(guideController.m_generalDemand, ItemClass.Zone.ResidentialLow);
                                    break;
                                }
                                z.m_zoneDemandWorkplace.Activate(guideController.m_zoningDemand, ItemClass.Zone.Industrial);
                                break;
                            }
                            if (z.m_workplaceDemand < 80)
                            {
                                z.m_zoneDemandWorkplace.Deactivate();
                                break;
                            }
                            break;
                        }
                        break;

                    case 400:
                        if (Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.Zoning))
                        {
                            z.m_zonesNotUsed.Activate(guideController.m_zoningNotUsed1);
                            break;
                        }
                        break;
                }
            }
            if (subStep > 1 || guideController == null)
                return;
            int num5 = (int)Singleton<SimulationManager>.instance.m_currentTickIndex & 1023;
            int num6 = num5 * 8 >> 10;
            int num7 = ((num5 + 1) * 8 >> 10) - 1;
            for (int index = num6; index <= num7; ++index)
            {
                if (Singleton<UnlockManager>.instance.Unlocked((ItemClass.Zone)index))
                    z.m_zoneNotUsed[index].Activate(guideController.m_zoningNotUsed2, (ItemClass.Zone)index);
            }
        }

        private static void UpdateDemand(ZoneManager z, ref int demand, int target)
        {
            int lastDemand = demand;
            int nextDemand = demand;
            if (target > lastDemand)
                nextDemand = Mathf.Min(lastDemand + 2, target);
            else if (target < demand)
                nextDemand = Mathf.Max(lastDemand - 2, target);
            z.m_DemandWrapper.OnUpdateDemand(lastDemand, ref nextDemand, target);
            demand = nextDemand;
        }

        internal class Data
        {
            [ReplaceMethod]
            public static void Deserialize(ZoneManager.Data d, DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "ZoneManager");
                ZoneManager instance = Singleton<ZoneManager>.instance;
                ZoneBlock[] zoneBlockArray = instance.m_blocks.m_buffer;
                ushort[] numArray = instance.m_zoneGrid;
                int length1 = zoneBlockArray.Length;
                int length2 = numArray.Length;
                instance.m_blocks.ClearUnused();

                Helper.EnsureInit();

                if (s.version >= 205U)
                {
                    EncodedArray.UInt @uint = EncodedArray.UInt.BeginRead(s);
                    for (int index = 1; index < length1; ++index)
                        zoneBlockArray[index].m_flags = @uint.Read();
                    @uint.EndRead();
                }
                else
                {
                    EncodedArray.UInt @uint = EncodedArray.UInt.BeginRead(s);
                    for (int index = 1; index < 16384; ++index)
                        zoneBlockArray[index].m_flags = @uint.Read();
                    for (int index = 16384; index < length1; ++index)
                        zoneBlockArray[index].m_flags = 0U;
                    @uint.EndRead();
                }
                for (int index = 1; index < length1; ++index)
                {
                    zoneBlockArray[index].m_nextGridBlock = (ushort)0;
                    if ((int)zoneBlockArray[index].m_flags != 0)
                    {
                        zoneBlockArray[index].m_buildIndex = s.ReadUInt32();
                        zoneBlockArray[index].m_position = s.ReadVector3();
                        zoneBlockArray[index].m_angle = s.ReadFloat();
                        zoneBlockArray[index].m_valid = s.ReadULong64();
                        zoneBlockArray[index].m_shared = s.ReadULong64();
                        zoneBlockArray[index].m_occupied1 = s.ReadULong64();
                        zoneBlockArray[index].m_occupied2 = s.version < 138U ? 0UL : s.ReadULong64();
                        if (s.version >= 4U)
                        {
                            zoneBlockArray[index].m_zone1 = s.ReadULong64();
                            zoneBlockArray[index].m_zone2 = s.ReadULong64();
                        }
                        else
                        {
                            int num1 = (int)s.ReadUInt32();
                            int num2 = (int)s.ReadUInt32();
                            zoneBlockArray[index].m_zone1 = 0UL;
                            zoneBlockArray[index].m_zone2 = 0UL;
                        }
                        InitializeBlock(instance, (ushort)index, ref zoneBlockArray[index]);
                    }
                    else
                    {
                        zoneBlockArray[index].m_buildIndex = 0U;
                        zoneBlockArray[index].m_position = Vector3.zero;
                        zoneBlockArray[index].m_angle = 0.0f;
                        zoneBlockArray[index].m_valid = 0UL;
                        zoneBlockArray[index].m_shared = 0UL;
                        zoneBlockArray[index].m_occupied1 = 0UL;
                        zoneBlockArray[index].m_occupied2 = 0UL;
                        zoneBlockArray[index].m_zone1 = 0UL;
                        zoneBlockArray[index].m_zone2 = 0UL;
                        instance.m_blocks.ReleaseItem((ushort)index);
                    }
                }
                if (s.version >= 74U)
                {
                    instance.m_residentialDemand = s.ReadInt8();
                    instance.m_commercialDemand = s.ReadInt8();
                    instance.m_workplaceDemand = s.ReadInt8();
                }
                else
                {
                    instance.m_residentialDemand = 50;
                    instance.m_commercialDemand = 50;
                    instance.m_workplaceDemand = 50;
                }
                if (s.version >= 168U)
                {
                    instance.m_actualResidentialDemand = s.ReadInt8();
                    instance.m_actualCommercialDemand = s.ReadInt8();
                    instance.m_actualWorkplaceDemand = s.ReadInt8();
                }
                else
                {
                    instance.m_actualResidentialDemand = instance.m_residentialDemand;
                    instance.m_actualCommercialDemand = instance.m_commercialDemand;
                    instance.m_actualWorkplaceDemand = instance.m_workplaceDemand;
                }
                if (s.version >= 194U)
                {
                    for (int index = 0; index < 8; ++index)
                        instance.m_goodAreaFound[index] = (short)s.ReadInt16();
                }
                else
                {
                    for (int index = 0; index < 8; ++index)
                        instance.m_goodAreaFound[index] = (short)0;
                }
                if (s.version >= 82U)
                {
                    for (int index = 0; index < 8; ++index)
                        instance.m_zoneNotUsed[index] = s.ReadObject<ZoneTypeGuide>();
                    instance.m_zoneDemandResidential = s.ReadObject<ZoneTypeGuide>();
                    instance.m_zoneDemandCommercial = s.ReadObject<ZoneTypeGuide>();
                    instance.m_zoneDemandWorkplace = s.ReadObject<ZoneTypeGuide>();
                }
                else
                {
                    for (int index = 0; index < 8; ++index)
                        instance.m_zoneNotUsed[index] = (ZoneTypeGuide)null;
                    instance.m_zoneDemandResidential = (ZoneTypeGuide)null;
                    instance.m_zoneDemandCommercial = (ZoneTypeGuide)null;
                    instance.m_zoneDemandWorkplace = (ZoneTypeGuide)null;
                }
                instance.m_optionsNotUsed = s.version < 134U ? (GenericGuide)null : s.ReadObject<GenericGuide>();
                instance.m_zonesNotUsed = s.version < 160U ? (GenericGuide)null : s.ReadObject<GenericGuide>();
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "ZoneManager");
            }
        }
    }
}
