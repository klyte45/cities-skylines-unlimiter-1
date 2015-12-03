using ColossalFramework;
using ColossalFramework.IO;
using System;
using System.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Zones
{
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
                    int num2 = num1 * 48;
                    int num3 = (num1 + 1) * 48 - 1;
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

    }
}
