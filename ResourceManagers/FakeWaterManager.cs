using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.ResourceManagers
{
    class FakeWaterManager
    {
        public struct Cell
        {
            public short m_currentWaterPressure;
            public short m_currentSewagePressure;
            public ushort m_waterPulseGroup;
            public ushort m_sewagePulseGroup;
            public ushort m_closestPipeSegment;
            public byte m_conductivity;
            public byte m_pollution;
            public bool m_tmpHasWater;
            public bool m_tmpHasSewage;
            public bool m_hasWater;
            public bool m_hasSewage;
        }
        private struct PulseGroup
        {
            public uint m_origPressure;
            public uint m_curPressure;
            public ushort m_mergeIndex;
            public ushort m_mergeCount;
            public ushort m_node;
        }
        private struct PulseUnit
        {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
        }
        public struct Node
        {
            public ushort m_waterPulseGroup;
            public ushort m_sewagePulseGroup;
            public ushort m_curWaterPressure;
            public ushort m_curSewagePressure;
            public ushort m_extraWaterPressure;
            public ushort m_extraSewagePressure;
            public byte m_pollution;
        }

        public const int GRID = 462;
        public const int HALFGRID = 231;

        private static int m_processedCells;
        private static int m_conductiveCells;
        private static bool m_canContinue;
        private static int m_modifiedX1 = 0;
        private static int m_modifiedZ1 = 0;
        private static int m_modifiedX2 = GRID - 1;
        private static int m_modifiedZ2 = GRID - 1;

        public static Node[] m_nodeData = new Node[32768];
        private static Cell[] m_waterGrid = new Cell[GRID * GRID];
        private static PulseGroup[] m_waterPulseGroups = new PulseGroup[1024];
        private static PulseGroup[] m_sewagePulseGroups = new PulseGroup[1024];
        private static PulseUnit[] m_waterPulseUnits = new PulseUnit[32768];
        private static PulseUnit[] m_sewagePulseUnits = new PulseUnit[32768];
        private static Texture2D m_waterTexture;
        private static int m_waterPulseGroupCount;
        private static int m_waterPulseUnitStart;
        private static int m_waterPulseUnitEnd;
        private static int m_sewagePulseGroupCount;
        private static int m_sewagePulseUnitStart;
        private static int m_sewagePulseUnitEnd;
        

        static FieldInfo m_refreshGrid;
        static FieldInfo undergroundCamera;

        public static void Init()
        {
            m_refreshGrid = typeof(WaterManager).GetField("m_refreshGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            undergroundCamera = typeof(WaterManager).GetField("m_undergroundCamera", BindingFlags.NonPublic | BindingFlags.Instance);

            m_waterTexture = new Texture2D(GRID, GRID, TextureFormat.RGBA32, false, true);
            m_waterTexture.filterMode = FilterMode.Point;
            m_waterTexture.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture("_ElectricityTexture", m_waterTexture);
            UpdateWaterMapping(WaterManager.instance);
        }
        
        internal static void OnDestroy()
        {
            if (m_waterTexture != null)
            {
                UnityEngine.Object.Destroy(m_waterTexture);
                m_waterTexture = null;
            }
        }

        [ReplaceMethod]
        private static void UpdateWaterMapping(WaterManager wm)
        {
            var cam = (Camera)undergroundCamera.GetValue(WaterManager.instance);
            if (cam != null)
            {
                if (WaterManager.instance.WaterMapVisible)
                {
                    cam.cullingMask |= 1 << LayerMask.NameToLayer("WaterPipes");
                }
                else
                {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer("WaterPipes"));
                }
            }
            Vector4 vec;
            vec.z = 1 / (38.25f * GRID);
            vec.x = 0.5f;
            vec.y = 0.5f;
            vec.w = 0.00390625f;
            Shader.SetGlobalVector("_WaterMapping", vec);
        }

        [ReplaceMethod]
        private void UpdateTexture()
        {
            while (!Monitor.TryEnter(m_waterGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            int modifiedX;
            int modifiedZ;
            int modifiedX2;
            int modifiedZ2;
            try
            {
                modifiedX = m_modifiedX1;
                modifiedZ = m_modifiedZ1;
                modifiedX2 = m_modifiedX2;
                modifiedZ2 = m_modifiedZ2;
                m_modifiedX1 = 10000;
                m_modifiedZ1 = 10000;
                m_modifiedX2 = -10000;
                m_modifiedZ2 = -10000;
            }
            finally
            {
                Monitor.Exit(m_waterGrid);
            }

            NetManager instance = Singleton<NetManager>.instance;
            for (int i = modifiedZ; i <= modifiedZ2; i++)
            {
                for (int j = modifiedX; j <= modifiedX2; j++)
                {
                    Cell cell = m_waterGrid[i * GRID + j];
                    Color color;
                    if (cell.m_closestPipeSegment != 0 && j != 0 && i != 0 && j != GRID - 1 && i != GRID - 1)
                    {
                        ushort startNode = instance.m_segments.m_buffer[(int)cell.m_closestPipeSegment].m_startNode;
                        ushort endNode = instance.m_segments.m_buffer[(int)cell.m_closestPipeSegment].m_endNode;
                        Vector3 position = instance.m_nodes.m_buffer[(int)startNode].m_position;
                        Vector3 position2 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                        int num = Mathf.RoundToInt(position.x * 0.418300658f) + 2048;
                        int num2 = Mathf.RoundToInt(position.z * 0.418300658f) + 2048;
                        int num3 = Mathf.RoundToInt(position2.x * 0.418300658f) + 2048;
                        int num4 = Mathf.RoundToInt(position2.z * 0.418300658f) + 2048;
                        color.r = (float)(j * 16 + 8 - num + 128) * 0.003921569f;
                        color.g = (float)(i * 16 + 8 - num2 + 128) * 0.003921569f;
                        color.b = (float)(j * 16 + 8 - num3 + 128) * 0.003921569f;
                        color.a = (float)(i * 16 + 8 - num4 + 128) * 0.003921569f;
                    }
                    else
                    {
                        color.r = 0f;
                        color.g = 0f;
                        color.b = 0f;
                        color.a = 0f;
                    }
                    m_waterTexture.SetPixel(j, i, color);
                }
            }
            m_waterTexture.Apply();
        }

        [ReplaceMethod]
        public void AreaModified(int minX, int minZ, int maxX, int maxZ)
        {
            while (!Monitor.TryEnter(m_waterGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                m_modifiedX1 = Mathf.Min(m_modifiedX1, minX);
                m_modifiedZ1 = Mathf.Min(m_modifiedZ1, minZ);
                m_modifiedX2 = Mathf.Max(m_modifiedX2, maxX);
                m_modifiedZ2 = Mathf.Max(m_modifiedZ2, maxZ);
            }
            finally
            {
                Monitor.Exit(m_waterGrid);
            }
        }

        [ReplaceMethod]
        public int TryDumpWater(ushort node, int rate, int max, byte waterPollution)
        {
            if (node == 0)
            {
                return 0;
            }
            int num = Mathf.Min(rate, 32767);
            Node node2 = m_nodeData[(int)node];
            if (node2.m_extraWaterPressure != 0)
            {
                int num2 = Mathf.Min(num - (int)node2.m_curWaterPressure, (int)node2.m_extraWaterPressure);
                if (num2 > 0)
                {
                    node2.m_curWaterPressure += (ushort)num2;
                    node2.m_extraWaterPressure -= (ushort)num2;
                    rate -= num2;
                }
            }
            rate = Mathf.Max(0, Mathf.Min(Mathf.Min(rate, max), num - (int)node2.m_curWaterPressure));
            node2.m_curWaterPressure += (ushort)rate;
            if (rate != 0)
            {
                node2.m_pollution = waterPollution;
            }
            m_nodeData[(int)node] = node2;
            return rate;
        }

        [ReplaceMethod]
        public int TryDumpSewage(Vector3 pos, int rate, int max)
        {
            if (max == 0)
            {
                return 0;
            }
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID ), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            int result = 0;
            if (TryDumpSewageImpl(pos, num, num2, rate, max, ref result))
            {
                return result;
            }
            if (m_waterGrid[num2 * GRID + num].m_conductivity == 0)
            {
                return 0;
            }
            float num3 = ((float)num + 0.5f - HALFGRID) * 38.25f;
            float num4 = ((float)num2 + 0.5f - HALFGRID) * 38.25f;
            if (pos.z > num4 && num2 < GRID - 1)
            {
                if (TryDumpSewageImpl(pos, num, num2 + 1, rate, max, ref result))
                {
                    return result;
                }
            }
            else if (pos.z < num4 && num2 > 0 && TryDumpSewageImpl(pos, num, num2 - 1, rate, max, ref result))
            {
                return result;
            }
            if (pos.x > num3 && num < GRID - 1)
            {
                if (TryDumpSewageImpl(pos, num + 1, num2, rate, max, ref result))
                {
                    return result;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (TryDumpSewageImpl(pos, num + 1, num2 + 1, rate, max, ref result))
                    {
                        return result;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && TryDumpSewageImpl(pos, num + 1, num2 - 1, rate, max, ref result))
                {
                    return result;
                }
            }
            else if (pos.x < num3 && num > 0)
            {
                if (TryDumpSewageImpl(pos, num - 1, num2, rate, max, ref result))
                {
                    return result;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (TryDumpSewageImpl(pos, num - 1, num2 + 1, rate, max, ref result))
                    {
                        return result;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && TryDumpSewageImpl(pos, num - 1, num2 - 1, rate, max, ref result))
                {
                    return result;
                }
            }
            return 0;
        }

        private bool TryDumpSewageImpl(Vector3 pos, int x, int z, int rate, int max, ref int result)
        {
            int num = z * GRID + x;
            Cell cell = m_waterGrid[num];
            if (cell.m_hasSewage)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_endNode;
                Segment2 segment;
                segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)startNode].m_position);
                segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)endNode].m_position);
                float num2;
                if ((double)segment.DistanceSqr(VectorUtils.XZ(pos), out num2) < 9025.0)
                {
                    rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentSewagePressure);
                    cell.m_currentSewagePressure -= (short)rate;
                    m_waterGrid[num] = cell;
                    result = rate;
                    return true;
                }
            }
            return false;
        }

        [ReplaceMethod]
        public int TryFetchWater(Vector3 pos, int rate, int max, ref byte waterPollution)
        {
            if (max == 0)
            {
                return 0;
            }
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            int result = 0;
            if (TryFetchWaterImpl(pos, num, num2, rate, max, ref result, ref waterPollution))
            {
                return result;
            }
            if (m_waterGrid[num2 * GRID + num].m_conductivity == 0)
            {
                return 0;
            }
            float num3 = ((float)num + 0.5f - HALFGRID) * 38.25f;
            float num4 = ((float)num2 + 0.5f - HALFGRID) * 38.25f;
            if (pos.z > num4 && num2 < GRID - 1)
            {
                if (TryFetchWaterImpl(pos, num, num2 + 1, rate, max, ref result, ref waterPollution))
                {
                    return result;
                }
            }
            else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(pos, num, num2 - 1, rate, max, ref result, ref waterPollution))
            {
                return result;
            }
            if (pos.x > num3 && num < GRID - 1)
            {
                if (TryFetchWaterImpl(pos, num + 1, num2, rate, max, ref result, ref waterPollution))
                {
                    return result;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (TryFetchWaterImpl(pos, num + 1, num2 + 1, rate, max, ref result, ref waterPollution))
                    {
                        return result;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(pos, num + 1, num2 - 1, rate, max, ref result, ref waterPollution))
                {
                    return result;
                }
            }
            else if (pos.x < num3 && num > 0)
            {
                if (TryFetchWaterImpl(pos, num - 1, num2, rate, max, ref result, ref waterPollution))
                {
                    return result;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (TryFetchWaterImpl(pos, num - 1, num2 + 1, rate, max, ref result, ref waterPollution))
                    {
                        return result;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(pos, num - 1, num2 - 1, rate, max, ref result, ref waterPollution))
                {
                    return result;
                }
            }
            return 0;
        }

        private bool TryFetchWaterImpl(Vector3 pos, int x, int z, int rate, int max, ref int result, ref byte waterPollution)
        {
            int num = z * GRID + x;
            Cell cell = m_waterGrid[num];
            if (cell.m_hasWater)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_endNode;
                Segment2 segment;
                segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)startNode].m_position);
                segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)endNode].m_position);
                float num2;
                if ((double)segment.DistanceSqr(VectorUtils.XZ(pos), out num2) < 9025.0)
                {
                    rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentWaterPressure);
                    cell.m_currentWaterPressure -= (short)rate;
                    waterPollution = cell.m_pollution;
                    m_waterGrid[num] = cell;
                    result = rate;
                    return true;
                }
            }
            return false;
        }

        [ReplaceMethod]
        public int TryFetchSewage(ushort node, int rate, int max)
        {
            if (node == 0)
            {
                return 0;
            }
            int num = Mathf.Min(rate, 32767);
            Node node2 = m_nodeData[(int)node];
            if (node2.m_extraSewagePressure != 0)
            {
                int num2 = Mathf.Min(num - (int)node2.m_curSewagePressure, (int)node2.m_extraSewagePressure);
                if (num2 > 0)
                {
                    node2.m_curSewagePressure += (ushort)num2;
                    node2.m_extraSewagePressure -= (ushort)num2;
                    rate -= num2;
                }
            }
            rate = Mathf.Max(0, Mathf.Min(Mathf.Min(rate, max), num - (int)node2.m_curSewagePressure));
            node2.m_curSewagePressure += (ushort)rate;
            m_nodeData[(int)node] = node2;
            return rate;
        }

        [ReplaceMethod]
        public void CheckWater(Vector3 pos, out bool water, out bool sewage, out byte waterPollution)
        {
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            if (CheckWaterImpl(pos, num, num2, out water, out sewage, out waterPollution))
            {
                return;
            }
            if (m_waterGrid[num2 * GRID + num].m_conductivity == 0)
            {
                return;
            }
            float num3 = ((float)num + 0.5f - HALFGRID) * 38.25f;
            float num4 = ((float)num2 + 0.5f - HALFGRID) * 38.25f;
            if (pos.z > num4 && num2 < GRID - 1)
            {
                if (CheckWaterImpl(pos, num, num2 + 1, out water, out sewage, out waterPollution))
                {
                    return;
                }
            }
            else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(pos, num, num2 - 1, out water, out sewage, out waterPollution))
            {
                return;
            }
            if (pos.x > num3 && num < GRID - 1)
            {
                if (CheckWaterImpl(pos, num + 1, num2, out water, out sewage, out waterPollution))
                {
                    return;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (CheckWaterImpl(pos, num + 1, num2 + 1, out water, out sewage, out waterPollution))
                    {
                        return;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(pos, num + 1, num2 - 1, out water, out sewage, out waterPollution))
                {
                    return;
                }
            }
            else if (pos.x < num3 && num > 0)
            {
                if (CheckWaterImpl(pos, num - 1, num2, out water, out sewage, out waterPollution))
                {
                    return;
                }
                if (pos.z > num4 && num2 < GRID - 1)
                {
                    if (CheckWaterImpl(pos, num - 1, num2 + 1, out water, out sewage, out waterPollution))
                    {
                        return;
                    }
                }
                else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(pos, num - 1, num2 - 1, out water, out sewage, out waterPollution))
                {
                    return;
                }
            }
        }

        private bool CheckWaterImpl(Vector3 pos, int x, int z, out bool water, out bool sewage, out byte waterPollution)
        {
            int num = z * GRID + x;
            Cell cell = m_waterGrid[num];
            if (cell.m_hasWater || cell.m_hasSewage)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[(int)closestPipeSegment].m_endNode;
                Segment2 segment;
                segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)startNode].m_position);
                segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)endNode].m_position);
                float num2;
                if ((double)segment.DistanceSqr(VectorUtils.XZ(pos), out num2) < 9025.0)
                {
                    water = cell.m_hasWater;
                    sewage = cell.m_hasSewage;
                    if (water)
                    {
                        waterPollution = m_waterGrid[num].m_pollution;
                    }
                    else
                    {
                        waterPollution = 0;
                    }
                    return true;
                }
            }
            water = false;
            sewage = false;
            waterPollution = 0;
            return false;
        }

        private ushort GetRootWaterGroup(ushort group)
        {
            for (ushort mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private ushort GetRootSewageGroup(ushort group)
        {
            for (ushort mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private void MergeWaterGroups(ushort root, ushort merged)
        {
            PulseGroup pulseGroup = m_waterPulseGroups[(int)root];
            PulseGroup pulseGroup2 = m_waterPulseGroups[(int)merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            m_nodeData[(int)pulseGroup.m_node].m_pollution = (byte)(m_nodeData[(int)pulseGroup.m_node].m_pollution + m_nodeData[(int)pulseGroup2.m_node].m_pollution + 1 >> 1);
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_waterPulseGroupCount; i++)
                {
                    if (m_waterPulseGroups[i].m_mergeIndex == merged)
                    {
                        m_waterPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_waterPulseGroups[i].m_origPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            m_waterPulseGroups[(int)root] = pulseGroup;
            m_waterPulseGroups[(int)merged] = pulseGroup2;
        }

        private void MergeSewageGroups(ushort root, ushort merged)
        {
            PulseGroup pulseGroup = m_sewagePulseGroups[(int)root];
            PulseGroup pulseGroup2 = m_sewagePulseGroups[(int)merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_sewagePulseGroupCount; i++)
                {
                    if (m_sewagePulseGroups[i].m_mergeIndex == merged)
                    {
                        m_sewagePulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_sewagePulseGroups[i].m_origPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            m_sewagePulseGroups[(int)root] = pulseGroup;
            m_sewagePulseGroups[(int)merged] = pulseGroup2;
        }

        private void ConductWaterToCell(ref Cell cell, ushort group, int x, int z)
        {
            if (cell.m_conductivity >= 96 && cell.m_waterPulseGroup == 65535)
            {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                {
                    m_waterPulseUnitEnd = 0;
                }
                cell.m_waterPulseGroup = group;
                m_canContinue = true;
            }
        }

        private void ConductSewageToCell(ref Cell cell, ushort group, int x, int z)
        {
            if (cell.m_conductivity >= 96 && cell.m_sewagePulseGroup == 65535)
            {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                {
                    m_sewagePulseUnitEnd = 0;
                }
                cell.m_sewagePulseGroup = group;
                m_canContinue = true;
            }
        }

        private void ConductWaterToCells(ushort group, float worldX, float worldZ, float radius)
        {
            int num = Mathf.Max((int)((worldX - radius) / 38.25f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / 38.25f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / 38.25f + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((worldZ + radius) / 38.25f + HALFGRID), GRID - 1);
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                float num6 = ((float)i + 0.5f - HALFGRID) * 38.25f - worldZ;
                for (int j = num; j <= num3; j++)
                {
                    float num7 = ((float)j + 0.5f - HALFGRID) * 38.25f - worldX;
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        int num8 = i * GRID + j;
                        ConductWaterToCell(ref m_waterGrid[num8], group, j, i);
                    }
                }
            }
        }

        private void ConductSewageToCells(ushort group, float worldX, float worldZ, float radius)
        {
            int num = Mathf.Max((int)((worldX - radius) / 38.25f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / 38.25f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / 38.25f + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((worldZ + radius) / 38.25f + HALFGRID), GRID - 1);
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                float num6 = ((float)i + 0.5f - HALFGRID) * 38.25f - worldZ;
                for (int j = num; j <= num3; j++)
                {
                    float num7 = ((float)j + 0.5f - HALFGRID) * 38.25f - worldX;
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        int num8 = i * GRID + j;
                        ConductSewageToCell(ref m_waterGrid[num8], group, j, i);
                    }
                }
            }
        }

        private void ConductWaterToNode(ushort nodeIndex, ref NetNode node, ushort group)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water)
            {
                if (m_nodeData[(int)nodeIndex].m_waterPulseGroup == 65535)
                {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
                    m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                    if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                    {
                        m_waterPulseUnitEnd = 0;
                    }
                    m_nodeData[(int)nodeIndex].m_waterPulseGroup = group;
                    m_canContinue = true;
                }
                else
                {
                    ushort rootWaterGroup = GetRootWaterGroup(m_nodeData[(int)nodeIndex].m_waterPulseGroup);
                    if (rootWaterGroup != group)
                    {
                        MergeWaterGroups(group, rootWaterGroup);
                        m_nodeData[(int)nodeIndex].m_waterPulseGroup = group;
                        m_canContinue = true;
                    }
                }
            }
        }

        private void ConductSewageToNode(ushort nodeIndex, ref NetNode node, ushort group)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water)
            {
                if (m_nodeData[(int)nodeIndex].m_sewagePulseGroup == 65535)
                {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
                    m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                    if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                    {
                        m_sewagePulseUnitEnd = 0;
                    }
                    m_nodeData[(int)nodeIndex].m_sewagePulseGroup = group;
                    m_canContinue = true;
                }
                else
                {
                    ushort rootSewageGroup = GetRootSewageGroup(m_nodeData[(int)nodeIndex].m_sewagePulseGroup);
                    if (rootSewageGroup != group)
                    {
                        MergeSewageGroups(group, rootSewageGroup);
                        m_nodeData[(int)nodeIndex].m_sewagePulseGroup = group;
                        m_canContinue = true;
                    }
                }
            }
        }

        private void UpdateNodeWater(int nodeID, int water, int sewage)
        {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            NetManager instance = Singleton<NetManager>.instance;
            bool flag = false;
            ushort building = instance.m_nodes.m_buffer[nodeID].m_building;
            if (building != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                if ((int)instance2.m_buildings.m_buffer[(int)building].m_waterBuffer != water)
                {
                    instance2.m_buildings.m_buffer[(int)building].m_waterBuffer = (ushort)water;
                    flag = (currentMode == InfoManager.InfoMode.Water);
                }
                if ((int)instance2.m_buildings.m_buffer[(int)building].m_sewageBuffer != sewage)
                {
                    instance2.m_buildings.m_buffer[(int)building].m_sewageBuffer = (ushort)sewage;
                    flag = (currentMode == InfoManager.InfoMode.Water);
                }
                if (flag)
                {
                    instance2.UpdateBuildingColors(building);
                }
            }
            NetNode.Flags flags = instance.m_nodes.m_buffer[nodeID].m_flags;
            NetNode.Flags flags2 = flags & ~(NetNode.Flags.Water | NetNode.Flags.Sewage);
            if (water != 0)
            {
                flags2 |= NetNode.Flags.Water;
            }
            if (sewage != 0)
            {
                flags2 |= NetNode.Flags.Sewage;
            }
            if (flags2 != flags)
            {
                instance.m_nodes.m_buffer[nodeID].m_flags = flags2;
                flag = (currentMode == InfoManager.InfoMode.Water);
            }
            if (flag)
            {
                instance.UpdateNodeColors((ushort)nodeID);
                for (int i = 0; i < 8; i++)
                {
                    ushort segment = instance.m_nodes.m_buffer[nodeID].GetSegment(i);
                    if (segment != 0)
                    {
                        instance.UpdateSegmentColors(segment);
                    }
                }
            }
        }

        [ReplaceMethod]
        protected void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num = (int)(currentFrameIndex & 511u);
                if (num < HALFGRID)
                {
                    if (num < 128)
                    {
                        if (num == 0)
                        {
                            m_waterPulseGroupCount = 0;
                            m_waterPulseUnitStart = 0;
                            m_waterPulseUnitEnd = 0;
                            m_sewagePulseGroupCount = 0;
                            m_sewagePulseUnitStart = 0;
                            m_sewagePulseUnitEnd = 0;
                            m_processedCells = 0;
                            m_conductiveCells = 0;
                            m_canContinue = true;
                        }
                        NetManager instance = Singleton<NetManager>.instance;
                        int num2 = num * 32768 >> 7;
                        int num3 = ((num + 1) * 32768 >> 7) - 1;
                        for (int i = num2; i <= num3; i++)
                        {
                            Node node = m_nodeData[i];
                            NetNode.Flags flags = instance.m_nodes.m_buffer[i].m_flags;
                            if (flags != NetNode.Flags.None)
                            {
                                NetInfo info = instance.m_nodes.m_buffer[i].Info;
                                if (info.m_class.m_service == ItemClass.Service.Water)
                                {
                                    int water = (node.m_waterPulseGroup == 65535) ? 0 : 1;
                                    int sewage = (node.m_sewagePulseGroup == 65535) ? 0 : 1;
                                    UpdateNodeWater(i, water, sewage);
                                    m_conductiveCells += 2;
                                    node.m_waterPulseGroup = 65535;
                                    node.m_sewagePulseGroup = 65535;
                                    if (node.m_curWaterPressure != 0 && m_waterPulseGroupCount < 1024)
                                    {
                                        PulseGroup pulseGroup;
                                        pulseGroup.m_origPressure = (uint)node.m_curWaterPressure;
                                        pulseGroup.m_curPressure = (uint)node.m_curWaterPressure;
                                        pulseGroup.m_mergeCount = 0;
                                        pulseGroup.m_mergeIndex = 65535;
                                        pulseGroup.m_node = (ushort)i;
                                        PulseUnit pulseUnit;
                                        pulseUnit.m_group = (ushort)m_waterPulseGroupCount;
                                        pulseUnit.m_node = (ushort)i;
                                        pulseUnit.m_x = 0;
                                        pulseUnit.m_z = 0;
                                        node.m_waterPulseGroup = (ushort)m_waterPulseGroupCount;
                                        m_waterPulseGroups[m_waterPulseGroupCount++] = pulseGroup;
                                        m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                                        if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                        {
                                            m_waterPulseUnitEnd = 0;
                                        }
                                    }
                                    if (node.m_curSewagePressure != 0 && m_sewagePulseGroupCount < 1024)
                                    {
                                        PulseGroup pulseGroup2;
                                        pulseGroup2.m_origPressure = (uint)node.m_curSewagePressure;
                                        pulseGroup2.m_curPressure = (uint)node.m_curSewagePressure;
                                        pulseGroup2.m_mergeCount = 0;
                                        pulseGroup2.m_mergeIndex = 65535;
                                        pulseGroup2.m_node = (ushort)i;
                                        PulseUnit pulseUnit2;
                                        pulseUnit2.m_group = (ushort)m_sewagePulseGroupCount;
                                        pulseUnit2.m_node = (ushort)i;
                                        pulseUnit2.m_x = 0;
                                        pulseUnit2.m_z = 0;
                                        node.m_sewagePulseGroup = (ushort)m_sewagePulseGroupCount;
                                        m_sewagePulseGroups[m_sewagePulseGroupCount++] = pulseGroup2;
                                        m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit2;
                                        if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                        {
                                            m_sewagePulseUnitEnd = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    node.m_waterPulseGroup = 65535;
                                    node.m_sewagePulseGroup = 65535;
                                    node.m_extraWaterPressure = 0;
                                    node.m_extraSewagePressure = 0;
                                }
                            }
                            else
                            {
                                node.m_waterPulseGroup = 65535;
                                node.m_sewagePulseGroup = 65535;
                                node.m_extraWaterPressure = 0;
                                node.m_extraSewagePressure = 0;
                            }
                            node.m_curWaterPressure = 0;
                            node.m_curSewagePressure = 0;
                            m_nodeData[i] = node;
                        }
                    }

                    int num4 = num * 2;
                    for (int j = num4; j < num4 + 2; j++)
                    {
                        int num6 = j * GRID;
                        for (int k = 0; k < GRID; k++)
                        {
                            Cell cell = m_waterGrid[num6];
                            cell.m_waterPulseGroup = 65535;
                            cell.m_sewagePulseGroup = 65535;
                            if (cell.m_conductivity >= 96)
                            {
                                m_conductiveCells += 2;
                            }
                            if (cell.m_tmpHasWater != cell.m_hasWater)
                            {
                                cell.m_hasWater = cell.m_tmpHasWater;
                            }
                            if (cell.m_tmpHasSewage != cell.m_hasSewage)
                            {
                                cell.m_hasSewage = cell.m_tmpHasSewage;
                            }
                            cell.m_tmpHasWater = false;
                            cell.m_tmpHasSewage = false;
                            m_waterGrid[num6] = cell;
                            num6++;
                        }
                    }
                }
                else
                {
                    int num7 = (num - HALFGRID - 1) * m_conductiveCells >> 7;
                    if (num == GRID - 1)
                    {
                        num7 = 1000000000;
                    }
                    while (m_canContinue && m_processedCells < num7)
                    {
                        m_canContinue = false;
                        int waterPulseUnitEnd = m_waterPulseUnitEnd;
                        int sewagePulseUnitEnd = m_sewagePulseUnitEnd;
                        while (m_waterPulseUnitStart != waterPulseUnitEnd)
                        {
                            PulseUnit pulseUnit3 = m_waterPulseUnits[m_waterPulseUnitStart];
                            if (++m_waterPulseUnitStart == m_waterPulseUnits.Length)
                            {
                                m_waterPulseUnitStart = 0;
                            }
                            pulseUnit3.m_group = GetRootWaterGroup(pulseUnit3.m_group);
                            uint num8 = m_waterPulseGroups[(int)pulseUnit3.m_group].m_curPressure;
                            if (pulseUnit3.m_node == 0)
                            {
                                int num9 = (int)pulseUnit3.m_z * GRID + (int)pulseUnit3.m_x;
                                Cell cell2 = m_waterGrid[num9];
                                if (cell2.m_conductivity != 0 && !cell2.m_tmpHasWater && num8 != 0u)
                                {
                                    int num10 = Mathf.Clamp((int)(-(int)cell2.m_currentWaterPressure), 0, (int)num8);
                                    num8 -= (uint)num10;
                                    cell2.m_currentWaterPressure += (short)num10;
                                    if (cell2.m_currentWaterPressure >= 0)
                                    {
                                        cell2.m_tmpHasWater = true;
                                        cell2.m_pollution = m_nodeData[(int)m_waterPulseGroups[(int)pulseUnit3.m_group].m_node].m_pollution;
                                    }
                                    m_waterGrid[num9] = cell2;
                                    m_waterPulseGroups[(int)pulseUnit3.m_group].m_curPressure = num8;
                                }
                                if (num8 != 0u)
                                {
                                    m_processedCells++;
                                }
                                else
                                {
                                    m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit3;
                                    if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                    {
                                        m_waterPulseUnitEnd = 0;
                                    }
                                }
                            }
                            else if (num8 != 0u)
                            {
                                m_processedCells++;
                                NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit3.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u))
                                {
                                    byte pollution = m_nodeData[(int)m_waterPulseGroups[(int)pulseUnit3.m_group].m_node].m_pollution;
                                    m_nodeData[(int)pulseUnit3.m_node].m_pollution = pollution;
                                    if (netNode.m_building != 0)
                                    {
                                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)netNode.m_building].m_waterPollution = pollution;
                                    }
                                    ConductWaterToCells(pulseUnit3.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                    for (int l = 0; l < 8; l++)
                                    {
                                        ushort segment = netNode.GetSegment(l);
                                        if (segment != 0)
                                        {
                                            ushort startNode = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_startNode;
                                            ushort endNode = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_endNode;
                                            ushort num11 = (startNode != pulseUnit3.m_node) ? startNode : endNode;
                                            ConductWaterToNode(num11, ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)num11], pulseUnit3.m_group);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit3;
                                if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                {
                                    m_waterPulseUnitEnd = 0;
                                }
                            }
                        }
                        while (m_sewagePulseUnitStart != sewagePulseUnitEnd)
                        {
                            PulseUnit pulseUnit4 = m_sewagePulseUnits[m_sewagePulseUnitStart];
                            if (++m_sewagePulseUnitStart == m_sewagePulseUnits.Length)
                            {
                                m_sewagePulseUnitStart = 0;
                            }
                            pulseUnit4.m_group = GetRootSewageGroup(pulseUnit4.m_group);
                            uint num12 = m_sewagePulseGroups[(int)pulseUnit4.m_group].m_curPressure;
                            if (pulseUnit4.m_node == 0)
                            {
                                int num13 = (int)pulseUnit4.m_z * GRID + (int)pulseUnit4.m_x;
                                Cell cell3 = m_waterGrid[num13];
                                if (cell3.m_conductivity != 0 && !cell3.m_tmpHasSewage && num12 != 0u)
                                {
                                    int num14 = Mathf.Clamp((int)(-(int)cell3.m_currentSewagePressure), 0, (int)num12);
                                    num12 -= (uint)num14;
                                    cell3.m_currentSewagePressure += (short)num14;
                                    if (cell3.m_currentSewagePressure >= 0)
                                    {
                                        cell3.m_tmpHasSewage = true;
                                    }
                                    m_waterGrid[num13] = cell3;
                                    m_sewagePulseGroups[(int)pulseUnit4.m_group].m_curPressure = num12;
                                }
                                if (num12 != 0u)
                                {
                                    m_processedCells++;
                                }
                                else
                                {
                                    m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit4;
                                    if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                    {
                                        m_sewagePulseUnitEnd = 0;
                                    }
                                }
                            }
                            else if (num12 != 0u)
                            {
                                m_processedCells++;
                                NetNode netNode2 = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit4.m_node];
                                if (netNode2.m_flags != NetNode.Flags.None && netNode2.m_buildIndex < (currentFrameIndex & 4294967168u))
                                {
                                    ConductSewageToCells(pulseUnit4.m_group, netNode2.m_position.x, netNode2.m_position.z, 100f);
                                    for (int m = 0; m < 8; m++)
                                    {
                                        ushort segment2 = netNode2.GetSegment(m);
                                        if (segment2 != 0)
                                        {
                                            ushort startNode2 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment2].m_startNode;
                                            ushort endNode2 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment2].m_endNode;
                                            ushort num15 = (startNode2 != pulseUnit4.m_node) ? startNode2 : endNode2;
                                            ConductSewageToNode(num15, ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)num15], pulseUnit4.m_group);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit4;
                                if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                {
                                    m_sewagePulseUnitEnd = 0;
                                }
                            }
                        }
                    }
                    if (num == GRID - 1)
                    {
                        for (int n = 0; n < m_waterPulseGroupCount; n++)
                        {
                            PulseGroup pulseGroup3 = m_waterPulseGroups[n];
                            if (pulseGroup3.m_mergeIndex != 65535)
                            {
                                PulseGroup pulseGroup4 = m_waterPulseGroups[(int)pulseGroup3.m_mergeIndex];
                                pulseGroup3.m_curPressure = (uint)((ulong)pulseGroup4.m_curPressure * (ulong)pulseGroup3.m_origPressure / (ulong)pulseGroup4.m_origPressure);
                                pulseGroup4.m_curPressure -= pulseGroup3.m_curPressure;
                                pulseGroup4.m_origPressure -= pulseGroup3.m_origPressure;
                                m_waterPulseGroups[(int)pulseGroup3.m_mergeIndex] = pulseGroup4;
                                m_waterPulseGroups[n] = pulseGroup3;
                            }
                        }
                        for (int num16 = 0; num16 < m_waterPulseGroupCount; num16++)
                        {
                            PulseGroup pulseGroup5 = m_waterPulseGroups[num16];
                            if (pulseGroup5.m_curPressure != 0u)
                            {
                                Node node2 = m_nodeData[(int)pulseGroup5.m_node];
                                node2.m_extraWaterPressure += (ushort)Mathf.Min((int)pulseGroup5.m_curPressure, (int)(32767 - node2.m_extraWaterPressure));
                                m_nodeData[(int)pulseGroup5.m_node] = node2;
                            }
                        }
                        for (int num17 = 0; num17 < m_sewagePulseGroupCount; num17++)
                        {
                            PulseGroup pulseGroup6 = m_sewagePulseGroups[num17];
                            if (pulseGroup6.m_mergeIndex != 65535)
                            {
                                PulseGroup pulseGroup7 = m_sewagePulseGroups[(int)pulseGroup6.m_mergeIndex];
                                pulseGroup6.m_curPressure = (uint)((ulong)pulseGroup7.m_curPressure * (ulong)pulseGroup6.m_origPressure / (ulong)pulseGroup7.m_origPressure);
                                pulseGroup7.m_curPressure -= pulseGroup6.m_curPressure;
                                pulseGroup7.m_origPressure -= pulseGroup6.m_origPressure;
                                m_sewagePulseGroups[(int)pulseGroup6.m_mergeIndex] = pulseGroup7;
                                m_sewagePulseGroups[num17] = pulseGroup6;
                            }
                        }
                        for (int num18 = 0; num18 < m_sewagePulseGroupCount; num18++)
                        {
                            PulseGroup pulseGroup8 = m_sewagePulseGroups[num18];
                            if (pulseGroup8.m_curPressure != 0u)
                            {
                                Node node3 = m_nodeData[(int)pulseGroup8.m_node];
                                node3.m_extraSewagePressure += (ushort)Mathf.Min((int)pulseGroup8.m_curPressure, (int)(32767 - node3.m_extraSewagePressure));
                                m_nodeData[(int)pulseGroup8.m_node] = node3;
                            }
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public void UpdateGrid(float minX, float minZ, float maxX, float maxZ)
        {
            int num = Mathf.Max((int)(minX / 38.25f + HALFGRID), 0);
            int num2 = Mathf.Max((int)(minZ / 38.25f + HALFGRID), 0);
            int num3 = Mathf.Min((int)(maxX / 38.25f + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)(maxZ / 38.25f + HALFGRID), GRID - 1);
            for (int i = num2; i <= num4; i++)
            {
                int num5 = i * GRID + num;
                for (int j = num; j <= num3; j++)
                {
                    m_waterGrid[num5].m_conductivity = 0;
                    m_waterGrid[num5].m_closestPipeSegment = 0;
                    num5++;
                }
            }
            float num6 = ((float)num - HALFGRID) * 38.25f - 100f;
            float num7 = ((float)num2 - HALFGRID) * 38.25f - 100f;
            float num8 = ((float)num3 - HALFGRID + 1f) * 38.25f + 100f;
            float num9 = ((float)num4 - HALFGRID + 1f) * 38.25f + 100f;
            int num10 = Mathf.Max((int)(num6 / 64f + 135f), 0);
            int num11 = Mathf.Max((int)(num7 / 64f + 135f), 0);
            int num12 = Mathf.Min((int)(num8 / 64f + 135f), 269);
            int num13 = Mathf.Min((int)(num9 / 64f + 135f), 269);
            float num14 = 100f;
            Array16<NetNode> nodes = Singleton<NetManager>.instance.m_nodes;
            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
            for (int k = num11; k <= num13; k++)
            {
                for (int l = num10; l <= num12; l++)
                {
                    ushort num15 = segmentGrid[k * 270 + l];
                    int num16 = 0;
                    while (num15 != 0)
                    {
                        NetSegment.Flags flags = segments.m_buffer[(int)num15].m_flags;
                        if ((flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created)
                        {
                            NetInfo info = segments.m_buffer[(int)num15].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water)
                            {
                                ushort startNode = segments.m_buffer[(int)num15].m_startNode;
                                ushort endNode = segments.m_buffer[(int)num15].m_endNode;
                                Vector2 a = VectorUtils.XZ(nodes.m_buffer[(int)startNode].m_position);
                                Vector2 b = VectorUtils.XZ(nodes.m_buffer[(int)endNode].m_position);
                                float num17 = Mathf.Max(Mathf.Max(num6 - a.x, num7 - a.y), Mathf.Max(a.x - num8, a.y - num9));
                                float num18 = Mathf.Max(Mathf.Max(num6 - b.x, num7 - b.y), Mathf.Max(b.x - num8, b.y - num9));
                                if (num17 < 0f || num18 < 0f)
                                {
                                    int num19 = Mathf.Max((int)((Mathf.Min(a.x, b.x) - num14) / 38.25f + HALFGRID), num);
                                    int num20 = Mathf.Max((int)((Mathf.Min(a.y, b.y) - num14) / 38.25f + HALFGRID), num2);
                                    int num21 = Mathf.Min((int)((Mathf.Max(a.x, b.x) + num14) / 38.25f + HALFGRID), num3);
                                    int num22 = Mathf.Min((int)((Mathf.Max(a.y, b.y) + num14) / 38.25f + HALFGRID), num4);
                                    for (int m = num20; m <= num22; m++)
                                    {
                                        int num23 = m * GRID + num19;
                                        float y = ((float)m + 0.5f - HALFGRID) * 38.25f;
                                        for (int n = num19; n <= num21; n++)
                                        {
                                            float x = ((float)n + 0.5f - HALFGRID) * 38.25f;
                                            float num25;
                                            float num24 = Segment2.DistanceSqr(a, b, new Vector2(x, y), out num25);
                                            num24 = Mathf.Sqrt(num24);
                                            if (num24 < num14 + 19.125f)
                                            {
                                                float num26 = (num14 - num24) * 0.0130718956f + 0.25f;
                                                int num27 = Mathf.Min(255, Mathf.RoundToInt(num26 * 255f));
                                                if (num27 > (int)m_waterGrid[num23].m_conductivity)
                                                {
                                                    m_waterGrid[num23].m_conductivity = (byte)num27;
                                                    m_waterGrid[num23].m_closestPipeSegment = num15;
                                                }
                                            }
                                            num23++;
                                        }
                                    }
                                }
                            }
                        }
                        num15 = segments.m_buffer[(int)num15].m_nextGridSegment;
                        if (++num16 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int num28 = num2; num28 <= num4; num28++)
            {
                int num29 = num28 * GRID + num;
                for (int num30 = num; num30 <= num3; num30++)
                {
                    Cell cell = m_waterGrid[num29];
                    if (cell.m_conductivity == 0)
                    {
                        cell.m_currentWaterPressure = 0;
                        cell.m_currentSewagePressure = 0;
                        cell.m_waterPulseGroup = 65535;
                        cell.m_sewagePulseGroup = 65535;
                        cell.m_tmpHasWater = false;
                        cell.m_tmpHasSewage = false;
                        cell.m_hasWater = false;
                        cell.m_hasSewage = false;
                        cell.m_pollution = 0;
                        m_waterGrid[num29] = cell;
                    }
                    num29++;
                }
            }
            AreaModified(num, num2, num3, num4);
        }
    }
}
