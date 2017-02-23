using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using EightyOne.IgnoreAttributes;
using EightyOne.Redirection;

//TODO(earalov): review this class
namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(WaterManager))]
    public class FakeWaterManager
    {

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateNodeWater(WaterManager manager, int nodeID, int water, int sewage, int heating)
        {
            UnityEngine.Debug.Log($"{manager}-{nodeID}-{water}-{sewage}-{heating}");
            UnityEngine.Debug.Log("AAAA");
            UnityEngine.Debug.Log("BBBB");
            UnityEngine.Debug.Log("CCCC");
            UnityEngine.Debug.Log("DDDD");
        }

        internal struct PulseUnit
        {
            public ushort m_group;
            public ushort m_node;
            //begin mid
            public ushort m_x;
            public ushort m_z;
            //end mod
        }

        public const int GRID = 462;
        public const int HALFGRID = 231;

        internal static int m_processedCells;
        internal static int m_conductiveCells;
        internal static bool m_canContinue;
        private static int m_modifiedX1;
        private static int m_modifiedZ1;
        private static int m_modifiedX2;
        private static int m_modifiedZ2;

        public static WaterManager.Node[] m_nodeData;
        internal static WaterManager.Cell[] m_waterGrid;
        internal static WaterManager.PulseGroup[] m_waterPulseGroups;
        internal static WaterManager.PulseGroup[] m_sewagePulseGroups;
        internal static WaterManager.PulseGroup[] m_heatingPulseGroups;
        internal static PulseUnit[] m_waterPulseUnits;
        internal static PulseUnit[] m_sewagePulseUnits;
        internal static PulseUnit[] m_heatingPulseUnits;
        private static Texture2D m_waterTexture;
        internal static int m_waterPulseGroupCount;
        private static int m_waterPulseUnitStart;
        internal static int m_waterPulseUnitEnd;
        internal static int m_sewagePulseGroupCount;
        private static int m_sewagePulseUnitStart;
        internal static int m_sewagePulseUnitEnd;
        internal static int m_heatingPulseGroupCount;
        private static int m_heatingPulseUnitStart;
        internal static int m_heatingPulseUnitEnd;

        static FieldInfo undergroundCamera = typeof(WaterManager).GetField("m_undergroundCamera", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Init()
        {
            var wm = Singleton<WaterManager>.instance;
            m_nodeData = wm.m_nodeData;
            if (m_waterGrid == null)
            {
                m_waterGrid = new WaterManager.Cell[GRID * GRID];
                var oldGrid = (IList)typeof(WaterManager).GetField("m_waterGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wm);
                int oldGridSize = 256;
                int diff = (GRID - oldGridSize) / 2;
                var fields = Util.GetFieldsFromStruct(m_waterGrid[0], oldGrid[0]);
                for (var i = 0; i < oldGridSize; i += 1)
                {
                    for (var j = 0; j < oldGridSize; j += 1)
                    {
                        m_waterGrid[(j + diff) * GRID + (i + diff)] = (WaterManager.Cell)Util.CopyStruct(new WaterManager.Cell(), oldGrid[j * oldGridSize + i], fields);
                    }
                }


                m_waterPulseGroups = new WaterManager.PulseGroup[1024];
                Util.CopyStructArray(m_waterPulseGroups, wm, "m_waterPulseGroups");
                m_sewagePulseGroups = new WaterManager.PulseGroup[1024];
                Util.CopyStructArray(m_sewagePulseGroups, wm, "m_sewagePulseGroups");
                m_heatingPulseGroups = new WaterManager.PulseGroup[1024];
                Util.CopyStructArray(m_heatingPulseGroups, wm, "m_heatingPulseGroups");


                m_waterPulseUnits = new PulseUnit[32768];
                Util.CopyStructArray(m_waterPulseUnits, wm, "m_waterPulseUnits");
                m_sewagePulseUnits = new PulseUnit[32768];
                Util.CopyStructArray(m_sewagePulseUnits, wm, "m_sewagePulseUnits");
                m_heatingPulseUnits = new PulseUnit[32768];
                Util.CopyStructArray(m_heatingPulseUnits, wm, "m_heatingPulseUnits");


                Util.SetPropertyValue(ref m_waterPulseGroupCount, wm, "m_waterPulseGroupCount");
                Util.SetPropertyValue(ref m_sewagePulseGroupCount, wm, "m_sewagePulseGroupCount");
                Util.SetPropertyValue(ref m_heatingPulseGroupCount, wm, "m_heatingPulseGroupCount");

                m_waterPulseUnitStart = 0;
                m_sewagePulseUnitStart = 0;
                m_heatingPulseUnitStart = 0;

                Util.SetPropertyValue(ref m_waterPulseUnitEnd, wm, "m_waterPulseUnitEnd");
                Util.SetPropertyValue(ref m_sewagePulseUnitEnd, wm, "m_sewagePulseUnitEnd");
                Util.SetPropertyValue(ref m_heatingPulseUnitEnd, wm, "m_heatingPulseUnitEnd");

                Util.SetPropertyValue(ref m_processedCells, wm, "m_processedCells");
                Util.SetPropertyValue(ref m_conductiveCells, wm, "m_conductiveCells");
                Util.SetPropertyValue(ref m_canContinue, wm, "m_canContinue");
            }
            m_modifiedX1 = 0;
            m_modifiedZ1 = 0;
            m_modifiedX2 = GRID - 1;
            m_modifiedZ2 = GRID - 1;

            m_waterTexture = new Texture2D(GRID, GRID, TextureFormat.RGBA32, false, true);
            m_waterTexture.filterMode = FilterMode.Point;
            m_waterTexture.wrapMode = TextureWrapMode.Clamp;

            Shader.SetGlobalTexture("_WaterTexture", m_waterTexture);
            SimulationManager.instance.AddAction(() =>
            {
                wm.UpdateGrid(-100000f, -100000f, 100000f, 100000f);
                UpdateWaterMapping(WaterManager.instance);
            });

        }

        internal static void OnDestroy()
        {
            if (m_waterTexture != null)
            {
                UnityEngine.Object.Destroy(m_waterTexture);
                m_waterTexture = null;
            }
            m_waterGrid = null;
        }

        [RedirectMethod]
        private static void UpdateWaterMapping(WaterManager wm)
        {
            var cam = (Camera)undergroundCamera.GetValue(WaterManager.instance);
            if (cam != null)
            {
                if (WaterManager.instance.WaterMapVisible || WaterManager.instance.HeatingMapVisible)
                {
                    cam.cullingMask |= 1 << LayerMask.NameToLayer("WaterPipes");
                }
                else
                {
                    cam.cullingMask &= ~(1 << LayerMask.NameToLayer("WaterPipes"));
                }
            }
            Vector4 vec;
            //begin mod
            vec.z = 1 / (38.25f * GRID);
            //end mod
            vec.x = 0.5f;
            vec.y = 0.5f;
            //begin mod
            vec.w = 1.0f / GRID;
            //end mod
            Shader.SetGlobalVector("_WaterMapping", vec);
        }

        [RedirectMethod]
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
                    WaterManager.Cell cell = m_waterGrid[i * GRID + j];
                    Color color;
                    ushort num5 = !WaterManager.instance.WaterMapVisible ? cell.m_closestPipeSegment2 : cell.m_closestPipeSegment;
                    if (num5 != 0 && j != 0 && i != 0 && j != GRID - 1 && i != GRID - 1)
                    {
                        ushort startNode = instance.m_segments.m_buffer[(int)num5].m_startNode;
                        ushort endNode = instance.m_segments.m_buffer[(int)num5].m_endNode;
                        Vector3 position = instance.m_nodes.m_buffer[(int)startNode].m_position;
                        Vector3 position2 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                        float offset = 16;
                        float halfOffset = 8;
                        float mult = offset / 38.25f;
                        var makePositive = GRID * offset / 2;

                        var num = position.x * mult + makePositive;
                        var num2 = position.z * mult + makePositive;
                        var num3 = position2.x * mult + makePositive;
                        var num4 = position2.z * mult + makePositive;

                        var min = 0f;
                        var max = 1.0f;
                        color.r = (float)Mathf.Clamp((j * offset + halfOffset - num + 128) / 255, 0, 1);
                        if (color.r > max || color.r < min)
                        {
                            color.r = 0;
                        }
                        color.g = (float)Mathf.Clamp((i * offset + halfOffset - num2 + 128) / 255, 0, 1);
                        if (color.g > max || color.g < min)
                        {
                            color.g = 0;
                        }
                        color.b = (float)Mathf.Clamp((j * offset + halfOffset - num3 + 128) / 255, 0, 1);
                        if (color.b > max || color.b < min)
                        {
                            color.b = 0;
                        }
                        color.a = (float)Mathf.Clamp((i * offset + halfOffset - num4 + 128) / 255, 0, 1);
                        if (color.a > max || color.a < min)
                        {
                            color.a = 0;
                        }
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

        [RedirectMethod]
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

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
        public int TryDumpSewage(Vector3 pos, int rate, int max)
        {
            if (max == 0)
            {
                return 0;
            }
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
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

        [RedirectMethod]
        private bool TryDumpSewageImpl(Vector3 pos, int x, int z, int rate, int max, ref int result)
        {
            //begin mod
            int index = z * GRID + x;
            //end mod
            WaterManager.Cell cell = m_waterGrid[index];
            if (cell.m_hasSewage)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num1 = cell.m_closestPipeSegment;
                ushort num2 = instance.m_segments.m_buffer[(int)num1].m_startNode;
                ushort num3 = instance.m_segments.m_buffer[(int)num1].m_endNode;
                if ((instance.m_nodes.m_buffer[(int)num2].m_flags & instance.m_nodes.m_buffer[(int)num3].m_flags & NetNode.Flags.Sewage) != NetNode.Flags.None)
                {
                    Segment2 segment2;
                    segment2.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num2].m_position);
                    segment2.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num3].m_position);
                    float u;
                    if ((double)segment2.DistanceSqr(VectorUtils.XZ(pos), out u) < 9025.0)
                    {
                        rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentSewagePressure);
                        cell.m_currentSewagePressure -= (short)rate;
                        m_waterGrid[index] = cell;
                        result = rate;
                        return true;
                    }
                }
            }
            return false;
        }

        [RedirectMethod] //no changes. Just references m_nodeData
        public int TryDumpSewage(ushort node, int rate, int max)
        {
            if ((int)node == 0)
                return 0;
            WaterManager.Node node1 = m_nodeData[(int)node];
            int num1 = Mathf.Min(rate, (int)short.MaxValue);
            int num2 = Mathf.Min(Mathf.Min(rate, max), (int)node1.m_extraSewagePressure);
            max -= num2;
            rate = Mathf.Max(0, Mathf.Min(Mathf.Min(rate, max), num1 - (int)node1.m_collectSewagePressure));
            node1.m_collectSewagePressure += (ushort)rate;
            node1.m_extraSewagePressure -= (ushort)num2;
            m_nodeData[(int)node] = node1;
            return num2;
        }

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
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

        [RedirectMethod]
        private bool TryFetchWaterImpl(Vector3 pos, int x, int z, int rate, int max, ref int result, ref byte waterPollution)
        {
            //begin mod
            int index = z * GRID + x;
            //end mod
            WaterManager.Cell cell = m_waterGrid[index];
            if (cell.m_hasWater)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num1 = cell.m_closestPipeSegment;
                ushort num2 = instance.m_segments.m_buffer[(int)num1].m_startNode;
                ushort num3 = instance.m_segments.m_buffer[(int)num1].m_endNode;
                if ((instance.m_nodes.m_buffer[(int)num2].m_flags & instance.m_nodes.m_buffer[(int)num3].m_flags & NetNode.Flags.Water) != NetNode.Flags.None)
                {
                    Segment2 segment2;
                    segment2.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num2].m_position);
                    segment2.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num3].m_position);
                    float u;
                    if ((double)segment2.DistanceSqr(VectorUtils.XZ(pos), out u) < 9025.0)
                    {
                        rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentWaterPressure);
                        cell.m_currentWaterPressure -= (short)rate;
                        waterPollution = cell.m_pollution;
                        m_waterGrid[index] = cell;
                        result = rate;
                        return true;
                    }
                }
            }
            return false;
        }

        [RedirectMethod] //no changes. Just references m_nodeData
        public int TryFetchWater(ushort node, int rate, int max, ref byte waterPollution)
        {
            if ((int)node == 0)
                return 0;
            WaterManager.Node node1 = m_nodeData[(int)node];
            int num1 = Mathf.Min(rate, (int)short.MaxValue);
            int num2 = Mathf.Min(Mathf.Min(rate, max), (int)node1.m_extraWaterPressure);
            max -= num2;
            rate = Mathf.Max(0, Mathf.Min(Mathf.Min(rate, max), num1 - (int)node1.m_collectWaterPressure));
            node1.m_collectWaterPressure += (ushort)rate;
            node1.m_extraWaterPressure -= (ushort)num2;
            if (num2 != 0)
                waterPollution = node1.m_pollution;
            m_nodeData[(int)node] = node1;
            return num2;
        }

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
        public int TryFetchHeating(Vector3 pos, int rate, int max, out bool connected)
        {
            connected = false;
            if (max == 0)
                return 0;
            //begin mod
            int x = Mathf.Clamp((int)((double)pos.x / 38.25 + HALFGRID), 0, GRID - 1);
            int z = Mathf.Clamp((int)((double)pos.z / 38.25 + HALFGRID), 0, GRID - 1);
            //end mod
            int result = 0;
            if (TryFetchHeatingImpl(pos, x, z, rate, max, ref result, ref connected))
                return result;
            //begin mod
            if ((int)m_waterGrid[z * GRID + x].m_conductivity2 == 0)
                //end mod
                return 0;
            //begin mod
            float num1 = (float)(((double)x + 0.5 - HALFGRID) * 38.25);
            float num2 = (float)(((double)z + 0.5 - HALFGRID) * 38.25);
            if ((double)pos.z > (double)num2 && z < GRID - 1)
            //end mod
            {
                if (TryFetchHeatingImpl(pos, x, z + 1, rate, max, ref result, ref connected))
                    return result;
            }
            else if ((double)pos.z < (double)num2 && z > 0 && TryFetchHeatingImpl(pos, x, z - 1, rate, max, ref result, ref connected))
                return result;
            //begin mod
            if ((double)pos.x > (double)num1 && x < GRID - 1)
            //end mod
            {
                if (TryFetchHeatingImpl(pos, x + 1, z, rate, max, ref result, ref connected))
                    return result;
                //begin mod
                if ((double)pos.z > (double)num2 && z < GRID - 1)
                //end mod
                {
                    if (TryFetchHeatingImpl(pos, x + 1, z + 1, rate, max, ref result, ref connected))
                        return result;
                }
                else if ((double)pos.z < (double)num2 && z > 0 && TryFetchHeatingImpl(pos, x + 1, z - 1, rate, max, ref result, ref connected))
                    return result;
            }
            else if ((double)pos.x < (double)num1 && x > 0)
            {
                if (TryFetchHeatingImpl(pos, x - 1, z, rate, max, ref result, ref connected))
                    return result;
                //begin mod
                if ((double)pos.z > (double)num2 && z < GRID - 1)
                //end mod
                {
                    if (TryFetchHeatingImpl(pos, x - 1, z + 1, rate, max, ref result, ref connected))
                        return result;
                }
                else if ((double)pos.z < (double)num2 && z > 0 && TryFetchHeatingImpl(pos, x - 1, z - 1, rate, max, ref result, ref connected))
                    return result;
            }
            return 0;
        }

        [RedirectMethod]
        private bool TryFetchHeatingImpl(Vector3 pos, int x, int z, int rate, int max, ref int result, ref bool connected)
        {
            //begin mod
            int index = z * GRID + x;
            //end mod
            WaterManager.Cell cell = m_waterGrid[index];
            if (cell.m_hasHeating)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num1 = cell.m_closestPipeSegment2;
                ushort num2 = instance.m_segments.m_buffer[(int)num1].m_startNode;
                ushort num3 = instance.m_segments.m_buffer[(int)num1].m_endNode;
                if ((instance.m_nodes.m_buffer[(int)num2].m_flags & instance.m_nodes.m_buffer[(int)num3].m_flags & NetNode.Flags.Heating) != NetNode.Flags.None)
                {
                    Segment2 segment2;
                    segment2.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num2].m_position);
                    segment2.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num3].m_position);
                    float u;
                    if ((double)segment2.DistanceSqr(VectorUtils.XZ(pos), out u) >= 9025.0)
                        return false;
                    rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentHeatingPressure);
                    cell.m_currentHeatingPressure -= (short)rate;
                    m_waterGrid[index] = cell;
                    result = rate;
                    connected = true;
                    return true;
                }
            }
            if ((int)cell.m_closestPipeSegment2 != 0 && (int)cell.m_conductivity2 >= 96)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num1 = cell.m_closestPipeSegment2;
                ushort num2 = instance.m_segments.m_buffer[(int)num1].m_startNode;
                ushort num3 = instance.m_segments.m_buffer[(int)num1].m_endNode;
                Segment2 segment2;
                segment2.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num2].m_position);
                segment2.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num3].m_position);
                float u;
                if ((double)segment2.DistanceSqr(VectorUtils.XZ(pos), out u) < 9025.0)
                    connected = true;
            }
            return false;
        }

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
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

        [RedirectMethod]
        private bool CheckWaterImpl(Vector3 pos, int x, int z, out bool water, out bool sewage, out byte waterPollution)
        {
            int num = z * GRID + x;
            WaterManager.Cell cell = m_waterGrid[num];
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

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
        public void CheckHeating(Vector3 pos, out bool heating)
        {
            //begin mod
            int x = Mathf.Clamp((int)((double)pos.x / 38.25 + HALFGRID), 0, GRID - 1);
            int z = Mathf.Clamp((int)((double)pos.z / 38.25 + HALFGRID), 0, GRID - 1);
            if (CheckHeatingImpl(pos, x, z, out heating) || (int)m_waterGrid[z * GRID + x].m_conductivity2 == 0)
                return;
            float num1 = (float)(((double)x + 0.5 - HALFGRID) * 38.25);
            float num2 = (float)(((double)z + 0.5 - HALFGRID) * 38.25);
            if ((double)pos.z > (double)num2 && z < GRID - 1)
            //end mod
            {
                if (CheckHeatingImpl(pos, x, z + 1, out heating))
                    return;
            }
            else if ((double)pos.z < (double)num2 && z > 0 && CheckHeatingImpl(pos, x, z - 1, out heating))
                return;
            //begin mod
            if ((double)pos.x > (double)num1 && x < GRID - 1)
            //end mod
            {
                if (CheckHeatingImpl(pos, x + 1, z, out heating))
                    return;
                //begin mod
                if ((double)pos.z > (double)num2 && z < GRID - 1)
                //end mod
                {
                    if (CheckHeatingImpl(pos, x + 1, z + 1, out heating))
                        ;
                }
                else if ((double)pos.z >= (double)num2 || z <= 0 || !CheckHeatingImpl(pos, x + 1, z - 1, out heating))
                    ;
            }
            else
            {
                if ((double)pos.x >= (double)num1 || x <= 0 || CheckHeatingImpl(pos, x - 1, z, out heating))
                    return;
                //begin mod
                if ((double)pos.z > (double)num2 && z < GRID - 1)
                //end mod
                {
                    if (CheckHeatingImpl(pos, x - 1, z + 1, out heating))
                        ;
                }
                else if ((double)pos.z >= (double)num2 || z <= 0 || !CheckHeatingImpl(pos, x - 1, z - 1, out heating))
                    ;
            }
        }

        [RedirectMethod]
        private bool CheckHeatingImpl(Vector3 pos, int x, int z, out bool heating)
        {
            //begin mod
            WaterManager.Cell cell = m_waterGrid[z * GRID + x];
            //end mod
            if (cell.m_hasHeating)
            {
                NetManager instance = Singleton<NetManager>.instance;
                ushort num1 = cell.m_closestPipeSegment2;
                ushort num2 = instance.m_segments.m_buffer[(int)num1].m_startNode;
                ushort num3 = instance.m_segments.m_buffer[(int)num1].m_endNode;
                if ((instance.m_nodes.m_buffer[(int)num2].m_flags & instance.m_nodes.m_buffer[(int)num3].m_flags & NetNode.Flags.Heating) != NetNode.Flags.None)
                {
                    Segment2 segment2;
                    segment2.a = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num2].m_position);
                    segment2.b = VectorUtils.XZ(instance.m_nodes.m_buffer[(int)num3].m_position);
                    float u;
                    if ((double)segment2.DistanceSqr(VectorUtils.XZ(pos), out u) < 9025.0)
                    {
                        heating = true;
                        return true;
                    }
                }
            }
            heating = false;
            return false;
        }

        [RedirectMethod]
        private static ushort GetRootWaterGroup(WaterManager wm, ushort group)
        {
            for (ushort mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        [RedirectMethod]
        private static ushort GetRootSewageGroup(WaterManager wm, ushort group)
        {
            for (ushort mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        [RedirectMethod]
        private static ushort GetRootHeatingGroup(WaterManager wm, ushort group)
        {
            for (ushort index = m_heatingPulseGroups[(int)group].m_mergeIndex; (int)index != (int)ushort.MaxValue; index = m_heatingPulseGroups[(int)group].m_mergeIndex)
                group = index;
            return group;
        }

        [RedirectMethod]
        private static void MergeWaterGroups(WaterManager wm, ushort root, ushort merged)
        {
            WaterManager.PulseGroup pulseGroup = m_waterPulseGroups[(int)root];
            WaterManager.PulseGroup pulseGroup2 = m_waterPulseGroups[(int)merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if ((int)pulseGroup2.m_origPressure != 0)
                m_nodeData[(int)pulseGroup.m_node].m_pollution = (byte)((int)m_nodeData[(int)pulseGroup.m_node].m_pollution + (int)m_nodeData[(int)pulseGroup2.m_node].m_pollution + 1 >> 1);
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_waterPulseGroupCount; i++)
                {
                    if (m_waterPulseGroups[i].m_mergeIndex == merged)
                    {
                        m_waterPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_waterPulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= m_waterPulseGroups[i].m_collectPressure;
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

        [RedirectMethod]
        private static void MergeSewageGroups(WaterManager wm, ushort root, ushort merged)
        {
            WaterManager.PulseGroup pulseGroup = m_sewagePulseGroups[(int)root];
            WaterManager.PulseGroup pulseGroup2 = m_sewagePulseGroups[(int)merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_sewagePulseGroupCount; i++)
                {
                    if (m_sewagePulseGroups[i].m_mergeIndex == merged)
                    {
                        m_sewagePulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_sewagePulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= m_sewagePulseGroups[i].m_collectPressure;
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

        //no change
        [RedirectMethod]
        private static void MergeHeatingGroups(WaterManager wm, ushort root, ushort merged)
        {
            WaterManager.PulseGroup pulseGroup1 = m_heatingPulseGroups[(int)root];
            WaterManager.PulseGroup pulseGroup2 = m_heatingPulseGroups[(int)merged];
            pulseGroup1.m_origPressure += pulseGroup2.m_origPressure;
            if ((int)pulseGroup2.m_mergeCount != 0)
            {
                for (int index = 0; index < m_heatingPulseGroupCount; ++index)
                {
                    if ((int)m_heatingPulseGroups[index].m_mergeIndex == (int)merged)
                    {
                        m_heatingPulseGroups[index].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= m_heatingPulseGroups[index].m_origPressure;
                    }
                }
                pulseGroup1.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = (ushort)0;
            }
            pulseGroup1.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0U;
            ++pulseGroup1.m_mergeCount;
            pulseGroup2.m_mergeIndex = root;
            m_heatingPulseGroups[(int)root] = pulseGroup1;
            m_heatingPulseGroups[(int)merged] = pulseGroup2;
        }

        [RedirectMethod]
        private static void ConductWaterToCell(WaterManager wm, ref WaterManager.Cell cell, ushort group, int x, int z)
        {
            if (cell.m_conductivity >= 96 && cell.m_waterPulseGroup == 65535)
            {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                //begin mod
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                //end mod
                m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                {
                    m_waterPulseUnitEnd = 0;
                }
                cell.m_waterPulseGroup = group;
                m_canContinue = true;
            }
        }

        [RedirectMethod]
        private static void ConductSewageToCell(WaterManager wm, ref WaterManager.Cell cell, ushort group, int x, int z)
        {
            if (cell.m_conductivity >= 96 && cell.m_sewagePulseGroup == 65535)
            {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                //begin mod
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                //end mod
                m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                {
                    m_sewagePulseUnitEnd = 0;
                }
                cell.m_sewagePulseGroup = group;
                m_canContinue = true;
            }
        }

        [RedirectMethod]
        private static void ConductHeatingToCell(WaterManager wm, ref WaterManager.Cell cell, ushort group, int x, int z)
        {
            if ((int)cell.m_conductivity2 < 96 || (int)cell.m_heatingPulseGroup != (int)ushort.MaxValue)
                return;
            PulseUnit pulseUnit;
            pulseUnit.m_group = group;
            pulseUnit.m_node = (ushort)0;
            //begin mod
            pulseUnit.m_x = (ushort)x;
            pulseUnit.m_z = (ushort)z;
            //end mod
            m_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
            if (++m_heatingPulseUnitEnd == m_heatingPulseUnits.Length)
                m_heatingPulseUnitEnd = 0;
            cell.m_heatingPulseGroup = group;
            m_canContinue = true;
        }

        [RedirectMethod]
        private static void ConductWaterToCells(WaterManager wm, ushort group, float worldX, float worldZ, float radius)
        {
            //begin mod
            int num = Mathf.Max((int)((worldX - radius) / 38.25f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / 38.25f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / 38.25f + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((worldZ + radius) / 38.25f + HALFGRID), GRID - 1);
            //end mod
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                //begin mod
                float num6 = ((float)i + 0.5f - HALFGRID) * 38.25f - worldZ;
                //end mod
                for (int j = num; j <= num3; j++)
                {
                    //begin mod
                    float num7 = ((float)j + 0.5f - HALFGRID) * 38.25f - worldX;
                    //end mod
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        //begin mod
                        int num8 = i * GRID + j;
                        //end mod
                        ConductWaterToCell(wm, ref m_waterGrid[num8], group, j, i);
                    }
                }
            }            
        }

        [RedirectMethod]
        private static void ConductSewageToCells(WaterManager wm, ushort group, float worldX, float worldZ, float radius)
        {
            //begin mod
            int num = Mathf.Max((int)((worldX - radius) / 38.25f + HALFGRID), 0);
            int num2 = Mathf.Max((int)((worldZ - radius) / 38.25f + HALFGRID), 0);
            int num3 = Mathf.Min((int)((worldX + radius) / 38.25f + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((worldZ + radius) / 38.25f + HALFGRID), GRID - 1);
            //end mod
            float num5 = radius + 19.125f;
            num5 *= num5;
            for (int i = num2; i <= num4; i++)
            {
                //begin mod
                float num6 = ((float)i + 0.5f - HALFGRID) * 38.25f - worldZ;
                //end mod
                for (int j = num; j <= num3; j++)
                {
                    //begin mod
                    float num7 = ((float)j + 0.5f - HALFGRID) * 38.25f - worldX;
                    //end mod
                    if (num7 * num7 + num6 * num6 < num5)
                    {
                        //begin mod
                        int num8 = i * GRID + j;
                        //end mod
                        ConductSewageToCell(wm, ref m_waterGrid[num8], group, j, i);
                    }
                }
            }
        }

        [RedirectMethod]
        private static void ConductHeatingToCells(WaterManager wm, ushort group, float worldX, float worldZ, float radius)
        {
            //begin mod
            int num1 = Mathf.Max((int)(((double)worldX - (double)radius) / 38.25 + HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)worldZ - (double)radius) / 38.25 + HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)worldX + (double)radius) / 38.25 + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)(((double)worldZ + (double)radius) / 38.25 + HALFGRID), GRID - 1);
            //end mod
            float num5 = radius + 19.125f;
            float num6 = num5 * num5;
            for (int z = num2; z <= num4; ++z)
            {
                //begin mod
                float num7 = (float)(((double)z + 0.5 - HALFGRID) * 38.25) - worldZ;
                //end mod
                for (int x = num1; x <= num3; ++x)
                {
                    //begin mod
                    float num8 = (float)(((double)x + 0.5 - HALFGRID) * 38.25) - worldX;
                    //end mod
                    if ((double)num8 * (double)num8 + (double)num7 * (double)num7 < (double)num6)
                        //begin mod
                        ConductHeatingToCell(wm, ref m_waterGrid[z * GRID + x], group, x, z);
                    //end mod
                }
            }
        }

        [RedirectMethod]
        private static void ConductWaterToNode(WaterManager wm, ushort nodeIndex, ref NetNode node, ushort group)
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
                    ushort rootWaterGroup = GetRootWaterGroup(wm, m_nodeData[(int)nodeIndex].m_waterPulseGroup);
                    if (rootWaterGroup == group)
                    {
                        return;
                    }
                    MergeWaterGroups(wm, group, rootWaterGroup);
                    if ((int)m_waterPulseGroups[(int)rootWaterGroup].m_origPressure == 0)
                    {
                        FakeWaterManager.PulseUnit pulseUnit;
                        pulseUnit.m_group = group;
                        pulseUnit.m_node = nodeIndex;
                        pulseUnit.m_x = (byte)0;
                        pulseUnit.m_z = (byte)0;
                        m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                        if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                            m_waterPulseUnitEnd = 0;
                    }
                    m_nodeData[(int)nodeIndex].m_waterPulseGroup = group;
                    m_canContinue = true;
                }
            }
        }

        [RedirectMethod]
        private static void ConductSewageToNode(WaterManager wm, ushort nodeIndex, ref NetNode node, ushort group)
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
                    ushort rootSewageGroup = GetRootSewageGroup(wm, m_nodeData[(int)nodeIndex].m_sewagePulseGroup);
                    if (rootSewageGroup == group)
                    {
                        return;
                    }
                    MergeSewageGroups(wm, group, rootSewageGroup);
                    if ((int)m_sewagePulseGroups[(int)rootSewageGroup].m_origPressure == 0)
                    {
                        FakeWaterManager.PulseUnit pulseUnit;
                        pulseUnit.m_group = group;
                        pulseUnit.m_node = nodeIndex;
                        pulseUnit.m_x = (byte)0;
                        pulseUnit.m_z = (byte)0;
                        m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                        if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                            m_sewagePulseUnitEnd = 0;
                    }
                    m_nodeData[(int)nodeIndex].m_sewagePulseGroup = group;
                    m_canContinue = true;
                }
            }
        }

        //no changes
        [RedirectMethod]
        private static void ConductHeatingToNode(WaterManager wm, ushort nodeIndex, ref NetNode node, ushort group)
        {
            NetInfo info = node.Info;
            if (info.m_class.m_service != ItemClass.Service.Water || info.m_class.m_level != ItemClass.Level.Level2)
                return;
            if ((int)m_nodeData[(int)nodeIndex].m_heatingPulseGroup == (int)ushort.MaxValue)
            {
                FakeWaterManager.PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = (byte)0;
                pulseUnit.m_z = (byte)0;
                m_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                if (++m_heatingPulseUnitEnd == m_heatingPulseUnits.Length)
                    m_heatingPulseUnitEnd = 0;
                m_nodeData[(int)nodeIndex].m_heatingPulseGroup = group;
                m_canContinue = true;
            }
            else
            {
                ushort rootHeatingGroup = GetRootHeatingGroup(wm, m_nodeData[(int)nodeIndex].m_heatingPulseGroup);
                if ((int)rootHeatingGroup == (int)group)
                    return;
                MergeHeatingGroups(wm, group, rootHeatingGroup);
                m_nodeData[(int)nodeIndex].m_heatingPulseGroup = group;
                m_canContinue = true;
            }
        }

        [RedirectMethod]
        protected static void SimulationStepImpl(WaterManager wm, int subStep)
        {
            if (subStep == 0 || subStep == 1000)
                return;

            uint num1 = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num2 = (int)num1 & (int)byte.MaxValue;
            if (num2 < 128)
            {
                if (num2 == 0)
                {
                    m_waterPulseGroupCount = 0;
                    m_waterPulseUnitStart = 0;
                    m_waterPulseUnitEnd = 0;
                    m_sewagePulseGroupCount = 0;
                    m_sewagePulseUnitStart = 0;
                    m_sewagePulseUnitEnd = 0;
                    m_heatingPulseGroupCount = 0;
                    m_heatingPulseUnitStart = 0;
                    m_heatingPulseUnitEnd = 0;
                    m_processedCells = 0;
                    m_conductiveCells = 0;
                    m_canContinue = true;
                }
                NetManager instance = Singleton<NetManager>.instance;
                int num3 = num2 * 32768 >> 7;
                int num4 = ((num2 + 1) * 32768 >> 7) - 1;
                for (int nodeID = num3; nodeID <= num4; ++nodeID)
                {
                    WaterManager.Node node = m_nodeData[nodeID];
                    if (instance.m_nodes.m_buffer[nodeID].m_flags != NetNode.Flags.None)
                    {
                        NetInfo info = instance.m_nodes.m_buffer[nodeID].Info;
                        if (info.m_class.m_service == ItemClass.Service.Water &&
                            info.m_class.m_level <= ItemClass.Level.Level2)
                        {
                            int water = (int)node.m_waterPulseGroup == (int)ushort.MaxValue ? 0 : 1;
                            int sewage = (int)node.m_sewagePulseGroup == (int)ushort.MaxValue ? 0 : 1;
                            int heating = (int)node.m_heatingPulseGroup == (int)ushort.MaxValue ? 0 : 1;
                            UpdateNodeWater(wm, nodeID, water, sewage, heating);
                            m_conductiveCells += 2;
                            node.m_waterPulseGroup = ushort.MaxValue;
                            node.m_sewagePulseGroup = ushort.MaxValue;
                            node.m_heatingPulseGroup = ushort.MaxValue;
                            if (((int)node.m_curWaterPressure != 0 || (int)node.m_collectWaterPressure != 0) && m_waterPulseGroupCount < 1024)
                            {
                                WaterManager.PulseGroup pulseGroup;
                                pulseGroup.m_origPressure = (uint)node.m_curWaterPressure;
                                pulseGroup.m_curPressure = (uint)node.m_curWaterPressure;
                                pulseGroup.m_collectPressure = (uint)node.m_collectWaterPressure;
                                pulseGroup.m_mergeCount = (ushort)0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_node = (ushort)nodeID;
                                node.m_waterPulseGroup = (ushort)m_waterPulseGroupCount;
                                m_waterPulseGroups[m_waterPulseGroupCount++] = pulseGroup;
                                if ((int)pulseGroup.m_origPressure != 0)
                                {
                                    PulseUnit pulseUnit;
                                    pulseUnit.m_group = (ushort)(m_waterPulseGroupCount - 1);
                                    pulseUnit.m_node = (ushort)nodeID;
                                    pulseUnit.m_x = (byte)0;
                                    pulseUnit.m_z = (byte)0;
                                    m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                                    if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                        m_waterPulseUnitEnd = 0;
                                }
                            }
                            if (((int)node.m_curSewagePressure != 0 || (int)node.m_collectSewagePressure != 0) && m_sewagePulseGroupCount < 1024)
                            {
                                WaterManager.PulseGroup pulseGroup;
                                pulseGroup.m_origPressure = (uint)node.m_curSewagePressure;
                                pulseGroup.m_curPressure = (uint)node.m_curSewagePressure;
                                pulseGroup.m_collectPressure = (uint)node.m_collectSewagePressure;
                                pulseGroup.m_mergeCount = (ushort)0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_node = (ushort)nodeID;
                                node.m_sewagePulseGroup = (ushort)m_sewagePulseGroupCount;
                                m_sewagePulseGroups[m_sewagePulseGroupCount++] = pulseGroup;
                                if ((int)pulseGroup.m_origPressure != 0)
                                {
                                    FakeWaterManager.PulseUnit pulseUnit;
                                    pulseUnit.m_group = (ushort)(m_sewagePulseGroupCount - 1);
                                    pulseUnit.m_node = (ushort)nodeID;
                                    pulseUnit.m_x = (byte)0;
                                    pulseUnit.m_z = (byte)0;
                                    m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                                    if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                        m_sewagePulseUnitEnd = 0;
                                }
                            }
                            if ((int)node.m_curHeatingPressure != 0 && m_heatingPulseGroupCount < 1024)
                            {
                                WaterManager.PulseGroup pulseGroup;
                                pulseGroup.m_origPressure = (uint)node.m_curHeatingPressure;
                                pulseGroup.m_curPressure = (uint)node.m_curHeatingPressure;
                                pulseGroup.m_collectPressure = 0U;
                                pulseGroup.m_mergeCount = (ushort)0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_node = (ushort)nodeID;
                                FakeWaterManager.PulseUnit pulseUnit;
                                pulseUnit.m_group = (ushort)m_heatingPulseGroupCount;
                                pulseUnit.m_node = (ushort)nodeID;
                                pulseUnit.m_x = (byte)0;
                                pulseUnit.m_z = (byte)0;
                                node.m_heatingPulseGroup = (ushort)m_heatingPulseGroupCount;
                                m_heatingPulseGroups[m_heatingPulseGroupCount++] = pulseGroup;
                                m_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                                if (++m_heatingPulseUnitEnd == m_heatingPulseUnits.Length)
                                    m_heatingPulseUnitEnd = 0;
                            }
                        }
                        else
                        {
                            node.m_waterPulseGroup = ushort.MaxValue;
                            node.m_sewagePulseGroup = ushort.MaxValue;
                            node.m_heatingPulseGroup = ushort.MaxValue;
                            node.m_extraWaterPressure = (ushort)0;
                            node.m_extraSewagePressure = (ushort)0;
                            node.m_extraHeatingPressure = (ushort)0;
                        }
                    }
                    else
                    {
                        node.m_waterPulseGroup = ushort.MaxValue;
                        node.m_sewagePulseGroup = ushort.MaxValue;
                        node.m_heatingPulseGroup = ushort.MaxValue;
                        node.m_extraWaterPressure = (ushort)0;
                        node.m_extraSewagePressure = (ushort)0;
                        node.m_extraHeatingPressure = (ushort)0;
                    }
                    node.m_curWaterPressure = (ushort)0;
                    node.m_curSewagePressure = (ushort)0;
                    node.m_curHeatingPressure = (ushort)0;
                    node.m_collectWaterPressure = (ushort)0;
                    node.m_collectSewagePressure = (ushort)0;
                    m_nodeData[nodeID] = node;
                }
                //TODO(earalov): review why this works
                //begin mod
                int num5 = num2 * 4;
                if (num5 < GRID)
                {
                    int num6 = Math.Min(GRID - 1, num5 + 3);
                    //end mod
                    for (int index1 = num5; index1 <= num6; ++index1)
                    {
                        //begin mod
                        int index2 = index1 * GRID;
                        for (int index3 = 0; index3 < GRID; ++index3)
                        {
                            //end mod
                            WaterManager.Cell cell = m_waterGrid[index2];
                            cell.m_waterPulseGroup = ushort.MaxValue;
                            cell.m_sewagePulseGroup = ushort.MaxValue;
                            cell.m_heatingPulseGroup = ushort.MaxValue;
                            if ((int)cell.m_conductivity >= 96)
                                m_conductiveCells += 2;
                            if (cell.m_tmpHasWater != cell.m_hasWater)
                                cell.m_hasWater = cell.m_tmpHasWater;
                            if (cell.m_tmpHasSewage != cell.m_hasSewage)
                                cell.m_hasSewage = cell.m_tmpHasSewage;
                            if (cell.m_tmpHasHeating != cell.m_hasHeating)
                                cell.m_hasHeating = cell.m_tmpHasHeating;
                            cell.m_tmpHasWater = false;
                            cell.m_tmpHasSewage = false;
                            cell.m_tmpHasHeating = false;
                            m_waterGrid[index2] = cell;
                            ++index2;
                        }
                    }
                    //begin mod
                }
                //end mod
            }
            else
            {
                int num3 = (num2 - (int)sbyte.MaxValue) * m_conductiveCells >> 7;
                if (num2 == (int)byte.MaxValue)
                    num3 = 1000000000;
                while (m_canContinue && m_processedCells < num3)
                {
                    m_canContinue = false;
                    int num4 = m_waterPulseUnitEnd;
                    int num5 = m_sewagePulseUnitEnd;
                    int num6 = m_heatingPulseUnitEnd;
                    while (m_waterPulseUnitStart != num4)
                    {
                        FakeWaterManager.PulseUnit pulseUnit = m_waterPulseUnits[m_waterPulseUnitStart];
                        if (++m_waterPulseUnitStart == m_waterPulseUnits.Length)
                            m_waterPulseUnitStart = 0;
                        pulseUnit.m_group = GetRootWaterGroup(wm, pulseUnit.m_group);
                        uint num7 = m_waterPulseGroups[(int)pulseUnit.m_group].m_curPressure;
                        if ((int)pulseUnit.m_node == 0)
                        {
                            //begin mod
                            int index = (int)pulseUnit.m_z * GRID + (int)pulseUnit.m_x;
                            //end mod
                            WaterManager.Cell cell = m_waterGrid[index];
                            if ((int)cell.m_conductivity != 0 && !cell.m_tmpHasWater && (int)num7 != 0)
                            {
                                int num8 = Mathf.Clamp((int)-cell.m_currentWaterPressure, 0, (int)num7);
                                num7 -= (uint)num8;
                                cell.m_currentWaterPressure += (short)num8;
                                if ((int)cell.m_currentWaterPressure >= 0)
                                {
                                    cell.m_tmpHasWater = true;
                                    cell.m_pollution =
                                        m_nodeData[(int)m_waterPulseGroups[(int)pulseUnit.m_group].m_node].m_pollution;
                                }
                                m_waterGrid[index] = cell;
                                m_waterPulseGroups[(int)pulseUnit.m_group].m_curPressure = num7;
                            }
                            if ((int)num7 != 0)
                            {
                                ++m_processedCells;
                            }
                            else
                            {
                                m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                                if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                    m_waterPulseUnitEnd = 0;
                            }
                        }
                        else if ((int)num7 != 0)
                        {
                            ++m_processedCells;
                            NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit.m_node];
                            if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (num1 & 4294967168u))
                            {
                                byte num8 =
                                    m_nodeData[(int)m_waterPulseGroups[(int)pulseUnit.m_group].m_node].m_pollution;
                                m_nodeData[(int)pulseUnit.m_node].m_pollution = num8;
                                if ((int)netNode.m_building != 0)
                                    Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)netNode.m_building]
                                        .m_waterPollution = num8;
                                ConductWaterToCells(wm, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                for (int index = 0; index < 8; ++index)
                                {
                                    ushort segment = netNode.GetSegment(index);
                                    if ((int)segment != 0)
                                    {
                                        ushort num9 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_startNode;
                                        ushort num10 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_endNode;
                                        ushort nodeIndex = (int)num9 != (int)pulseUnit.m_node ? num9 : num10;
                                        ConductWaterToNode(wm, nodeIndex, ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)nodeIndex],
                                            pulseUnit.m_group);
                                    }
                                }
                            }
                        }
                        else
                        {
                            m_waterPulseUnits[m_waterPulseUnitEnd] = pulseUnit;
                            if (++m_waterPulseUnitEnd == m_waterPulseUnits.Length)
                                m_waterPulseUnitEnd = 0;
                        }
                    }
                    while (m_sewagePulseUnitStart != num5)
                    {
                        FakeWaterManager.PulseUnit pulseUnit = m_sewagePulseUnits[m_sewagePulseUnitStart];
                        if (++m_sewagePulseUnitStart == m_sewagePulseUnits.Length)
                            m_sewagePulseUnitStart = 0;
                        pulseUnit.m_group = GetRootSewageGroup(wm, pulseUnit.m_group);
                        uint num7 = m_sewagePulseGroups[(int)pulseUnit.m_group].m_curPressure;
                        if ((int)pulseUnit.m_node == 0)
                        {
                            //begin mod
                            int index = (int)pulseUnit.m_z * GRID + (int)pulseUnit.m_x;
                            //end mod
                            WaterManager.Cell cell = m_waterGrid[index];
                            if ((int)cell.m_conductivity != 0 && !cell.m_tmpHasSewage && (int)num7 != 0)
                            {
                                int num8 = Mathf.Clamp((int)-cell.m_currentSewagePressure, 0, (int)num7);
                                num7 -= (uint)num8;
                                cell.m_currentSewagePressure += (short)num8;
                                if ((int)cell.m_currentSewagePressure >= 0)
                                    cell.m_tmpHasSewage = true;
                                m_waterGrid[index] = cell;
                                m_sewagePulseGroups[(int)pulseUnit.m_group].m_curPressure = num7;
                            }
                            if ((int)num7 != 0)
                            {
                                ++m_processedCells;
                            }
                            else
                            {
                                m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                                if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                    m_sewagePulseUnitEnd = 0;
                            }
                        }
                        else if ((int)num7 != 0)
                        {
                            ++m_processedCells;
                            NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit.m_node];
                            if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (num1 & 4294967168u))
                            {
                                ConductSewageToCells(wm, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                for (int index = 0; index < 8; ++index)
                                {
                                    ushort segment = netNode.GetSegment(index);
                                    if ((int)segment != 0)
                                    {
                                        ushort num8 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_startNode;
                                        ushort num9 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_endNode;
                                        ushort nodeIndex = (int)num8 != (int)pulseUnit.m_node ? num8 : num9;
                                        ConductSewageToNode(wm, nodeIndex, ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)nodeIndex],
                                            pulseUnit.m_group);
                                    }
                                }
                            }
                        }
                        else
                        {
                            m_sewagePulseUnits[m_sewagePulseUnitEnd] = pulseUnit;
                            if (++m_sewagePulseUnitEnd == m_sewagePulseUnits.Length)
                                m_sewagePulseUnitEnd = 0;
                        }
                    }
                    while (m_heatingPulseUnitStart != num6)
                    {
                        FakeWaterManager.PulseUnit pulseUnit = m_heatingPulseUnits[m_heatingPulseUnitStart];
                        if (++m_heatingPulseUnitStart == m_heatingPulseUnits.Length)
                            m_heatingPulseUnitStart = 0;
                        pulseUnit.m_group = GetRootHeatingGroup(wm, pulseUnit.m_group);
                        uint num7 = m_heatingPulseGroups[(int)pulseUnit.m_group].m_curPressure;
                        if ((int)pulseUnit.m_node == 0)
                        {
                            //begin mod
                            int index = (int)pulseUnit.m_z * GRID + (int)pulseUnit.m_x;
                            //end mod
                            WaterManager.Cell cell = m_waterGrid[index];
                            if ((int)cell.m_conductivity2 != 0 && !cell.m_tmpHasHeating && (int)num7 != 0)
                            {
                                int num8 = Mathf.Clamp((int)-cell.m_currentHeatingPressure, 0, (int)num7);
                                num7 -= (uint)num8;
                                cell.m_currentHeatingPressure += (short)num8;
                                if ((int)cell.m_currentHeatingPressure >= 0)
                                    cell.m_tmpHasHeating = true;
                                m_waterGrid[index] = cell;
                                m_heatingPulseGroups[(int)pulseUnit.m_group].m_curPressure = num7;
                            }
                            if ((int)num7 != 0)
                            {
                                ++m_processedCells;
                            }
                            else
                            {
                                m_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                                if (++m_heatingPulseUnitEnd == m_heatingPulseUnits.Length)
                                    m_heatingPulseUnitEnd = 0;
                            }
                        }
                        else if ((int)num7 != 0)
                        {
                            ++m_processedCells;
                            NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit.m_node];
                            if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (num1 & 4294967168u))
                            {
                                ConductHeatingToCells(wm, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z,
                                    100f);
                                for (int index = 0; index < 8; ++index)
                                {
                                    ushort segment = netNode.GetSegment(index);
                                    if ((int)segment != 0)
                                    {
                                        NetInfo info = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].Info;
                                        if (info.m_class.m_service == ItemClass.Service.Water &&
                                            info.m_class.m_level == ItemClass.Level.Level2)
                                        {
                                            ushort num8 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_startNode;
                                            ushort num9 = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_endNode;
                                            ushort nodeIndex = (int)num8 != (int)pulseUnit.m_node ? num8 : num9;
                                            ConductHeatingToNode(wm, nodeIndex,
                                                ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)nodeIndex], pulseUnit.m_group);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            m_heatingPulseUnits[m_heatingPulseUnitEnd] = pulseUnit;
                            if (++m_heatingPulseUnitEnd == m_heatingPulseUnits.Length)
                                m_heatingPulseUnitEnd = 0;
                        }
                    }
                }
                if (num2 != (int)byte.MaxValue)
                    return;
                for (int index = 0; index < m_waterPulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup1 = m_waterPulseGroups[index];
                    if ((int)pulseGroup1.m_mergeIndex != (int)ushort.MaxValue && (int)pulseGroup1.m_collectPressure != 0)
                    {
                        WaterManager.PulseGroup pulseGroup2 = m_waterPulseGroups[(int)pulseGroup1.m_mergeIndex];
                        pulseGroup1.m_curPressure = (uint)((ulong)pulseGroup2.m_curPressure * (ulong)pulseGroup1.m_collectPressure / (ulong)pulseGroup2.m_collectPressure);
                        if (pulseGroup1.m_collectPressure < pulseGroup1.m_curPressure)
                            pulseGroup1.m_curPressure = pulseGroup1.m_collectPressure;
                        pulseGroup2.m_curPressure -= pulseGroup1.m_curPressure;
                        pulseGroup2.m_collectPressure -= pulseGroup1.m_collectPressure;
                        m_waterPulseGroups[(int)pulseGroup1.m_mergeIndex] = pulseGroup2;
                        m_waterPulseGroups[index] = pulseGroup1;
                    }
                }
                for (int index = 0; index < m_waterPulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup1 = m_waterPulseGroups[index];
                    if ((int)pulseGroup1.m_mergeIndex != (int)ushort.MaxValue && (int)pulseGroup1.m_collectPressure == 0)
                    {
                        WaterManager.PulseGroup pulseGroup2 = m_waterPulseGroups[(int)pulseGroup1.m_mergeIndex];
                        uint num4 = pulseGroup2.m_curPressure;
                        uint num5 = pulseGroup2.m_collectPressure < num4 ? num4 - pulseGroup2.m_collectPressure : 0U;
                        pulseGroup1.m_curPressure = (uint)((ulong)num5 * (ulong)pulseGroup1.m_origPressure / (ulong)pulseGroup2.m_origPressure);
                        pulseGroup2.m_curPressure -= pulseGroup1.m_curPressure;
                        pulseGroup2.m_origPressure -= pulseGroup1.m_origPressure;
                        m_waterPulseGroups[(int)pulseGroup1.m_mergeIndex] = pulseGroup2;
                        m_waterPulseGroups[index] = pulseGroup1;
                    }
                }
                for (int index = 0; index < m_waterPulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup = m_waterPulseGroups[index];
                    if ((int)pulseGroup.m_curPressure != 0)
                    {
                        WaterManager.Node node = m_nodeData[(int)pulseGroup.m_node];
                        node.m_extraWaterPressure +=
                            (ushort)
                                Mathf.Min((int)pulseGroup.m_curPressure,
                                    (int)short.MaxValue - (int)node.m_extraWaterPressure);
                        m_nodeData[(int)pulseGroup.m_node] = node;
                    }
                }
                for (int index = 0; index < m_sewagePulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup1 = m_sewagePulseGroups[index];
                    if ((int)pulseGroup1.m_mergeIndex != (int)ushort.MaxValue && (int)pulseGroup1.m_collectPressure != 0)
                    {
                        WaterManager.PulseGroup pulseGroup2 = m_sewagePulseGroups[(int)pulseGroup1.m_mergeIndex];
                        pulseGroup1.m_curPressure = (uint)((ulong)pulseGroup2.m_curPressure * (ulong)pulseGroup1.m_collectPressure / (ulong)pulseGroup2.m_collectPressure);
                        if (pulseGroup1.m_collectPressure < pulseGroup1.m_curPressure)
                            pulseGroup1.m_curPressure = pulseGroup1.m_collectPressure;
                        pulseGroup2.m_curPressure -= pulseGroup1.m_curPressure;
                        pulseGroup2.m_collectPressure -= pulseGroup1.m_collectPressure;
                        m_sewagePulseGroups[(int)pulseGroup1.m_mergeIndex] = pulseGroup2;
                        m_sewagePulseGroups[index] = pulseGroup1;
                    }
                }
                for (int index = 0; index < m_sewagePulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup1 = m_sewagePulseGroups[index];
                    if ((int)pulseGroup1.m_mergeIndex != (int)ushort.MaxValue && (int)pulseGroup1.m_collectPressure == 0)
                    {
                        WaterManager.PulseGroup pulseGroup2 = m_sewagePulseGroups[(int)pulseGroup1.m_mergeIndex];
                        uint num4 = pulseGroup2.m_curPressure;
                        uint num5 = pulseGroup2.m_collectPressure < num4 ? num4 - pulseGroup2.m_collectPressure : 0U;
                        pulseGroup1.m_curPressure = (uint) ((ulong)pulseGroup2.m_curPressure * (ulong)pulseGroup1.m_origPressure / (ulong)pulseGroup2.m_origPressure);
                        pulseGroup2.m_curPressure -= pulseGroup1.m_curPressure;
                        pulseGroup2.m_origPressure -= pulseGroup1.m_origPressure;
                        m_sewagePulseGroups[(int)pulseGroup1.m_mergeIndex] = pulseGroup2;
                        m_sewagePulseGroups[index] = pulseGroup1;
                    }
                }
                for (int index = 0; index < m_sewagePulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup = m_sewagePulseGroups[index];
                    if ((int)pulseGroup.m_curPressure != 0)
                    {
                        WaterManager.Node node = m_nodeData[(int)pulseGroup.m_node];
                        node.m_extraSewagePressure +=
                            (ushort)
                                Mathf.Min((int)pulseGroup.m_curPressure,
                                    (int)short.MaxValue - (int)node.m_extraSewagePressure);
                        m_nodeData[(int)pulseGroup.m_node] = node;
                    }
                }
                for (int index = 0; index < m_heatingPulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup1 = m_heatingPulseGroups[index];
                    if ((int)pulseGroup1.m_mergeIndex != (int)ushort.MaxValue)
                    {
                        WaterManager.PulseGroup pulseGroup2 = m_heatingPulseGroups[(int)pulseGroup1.m_mergeIndex];
                        pulseGroup1.m_curPressure =
                            (uint)
                                ((ulong)pulseGroup2.m_curPressure * (ulong)pulseGroup1.m_origPressure /
                                 (ulong)pulseGroup2.m_origPressure);
                        pulseGroup2.m_curPressure -= pulseGroup1.m_curPressure;
                        pulseGroup2.m_origPressure -= pulseGroup1.m_origPressure;
                        m_heatingPulseGroups[(int)pulseGroup1.m_mergeIndex] = pulseGroup2;
                        m_heatingPulseGroups[index] = pulseGroup1;
                    }
                }
                for (int index = 0; index < m_heatingPulseGroupCount; ++index)
                {
                    WaterManager.PulseGroup pulseGroup = m_heatingPulseGroups[index];
                    if ((int)pulseGroup.m_curPressure != 0)
                    {
                        WaterManager.Node node = m_nodeData[(int)pulseGroup.m_node];
                        node.m_extraHeatingPressure +=
                            (ushort)
                                Mathf.Min((int)pulseGroup.m_curPressure,
                                    (int)short.MaxValue - (int)node.m_extraHeatingPressure);
                        m_nodeData[(int)pulseGroup.m_node] = node;
                    }
                }
            }
        }

        [RedirectMethod]
        public void UpdateGrid(float minX, float minZ, float maxX, float maxZ)
        {
            //begin mod
            int num1 = Mathf.Max((int)((double)minX / 38.25 + HALFGRID), 0);
            int num2 = Mathf.Max((int)((double)minZ / 38.25 + HALFGRID), 0);
            int num3 = Mathf.Min((int)((double)maxX / 38.25 + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((double)maxZ / 38.25 + HALFGRID), GRID - 1);
            //end mod
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                //begin mod
                int index2 = index1 * GRID + num1;
                //end mod
                for (int index3 = num1; index3 <= num3; ++index3)
                {
                    m_waterGrid[index2].m_conductivity = (byte)0;
                    m_waterGrid[index2].m_conductivity2 = (byte)0;
                    m_waterGrid[index2].m_closestPipeSegment = (ushort)0;
                    m_waterGrid[index2].m_closestPipeSegment2 = (ushort)0;
                    ++index2;
                }
            }
            //begin mod
            float num5 = (float)(((double)num1 - HALFGRID) * 38.25 - 100.0);
            float num6 = (float)(((double)num2 - HALFGRID) * 38.25 - 100.0);
            float num7 = (float)(((double)num3 - HALFGRID + 1.0) * 38.25 + 100.0);
            float num8 = (float)(((double)num4 - HALFGRID + 1.0) * 38.25 + 100.0);
            //end mod
            int num9 = Mathf.Max((int)((double)num5 / 64.0 + 135.0), 0);
            int num10 = Mathf.Max((int)((double)num6 / 64.0 + 135.0), 0);
            int num11 = Mathf.Min((int)((double)num7 / 64.0 + 135.0), 269);
            int num12 = Mathf.Min((int)((double)num8 / 64.0 + 135.0), 269);
            float num13 = 100f;
            Array16<NetNode> array16_1 = Singleton<NetManager>.instance.m_nodes;
            Array16<NetSegment> array16_2 = Singleton<NetManager>.instance.m_segments;
            ushort[] numArray = Singleton<NetManager>.instance.m_segmentGrid;
            for (int index1 = num10; index1 <= num12; ++index1)
            {
                for (int index2 = num9; index2 <= num11; ++index2)
                {
                    ushort num14 = numArray[index1 * 270 + index2];
                    int num15 = 0;
                    while ((int)num14 != 0)
                    {
                        if ((array16_2.m_buffer[(int)num14].m_flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created)
                        {
                            NetInfo info = array16_2.m_buffer[(int)num14].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2)
                            {
                                ushort num16 = array16_2.m_buffer[(int)num14].m_startNode;
                                ushort num17 = array16_2.m_buffer[(int)num14].m_endNode;
                                Vector2 a = VectorUtils.XZ(array16_1.m_buffer[(int)num16].m_position);
                                Vector2 b = VectorUtils.XZ(array16_1.m_buffer[(int)num17].m_position);
                                if ((double)Mathf.Max(Mathf.Max(num5 - a.x, num6 - a.y), Mathf.Max(a.x - num7, a.y - num8)) < 0.0 || (double)Mathf.Max(Mathf.Max(num5 - b.x, num6 - b.y), Mathf.Max(b.x - num7, b.y - num8)) < 0.0)
                                {
                                    //begin mod
                                    int num18 = Mathf.Max((int)(((double)Mathf.Min(a.x, b.x) - (double)num13) / 38.25 + HALFGRID), num1);
                                    int num19 = Mathf.Max((int)(((double)Mathf.Min(a.y, b.y) - (double)num13) / 38.25 + HALFGRID), num2);
                                    int num20 = Mathf.Min((int)(((double)Mathf.Max(a.x, b.x) + (double)num13) / 38.25 + HALFGRID), num3);
                                    int num21 = Mathf.Min((int)(((double)Mathf.Max(a.y, b.y) + (double)num13) / 38.25 + HALFGRID), num4);
                                    //end mod
                                    for (int index3 = num19; index3 <= num21; ++index3)
                                    {
                                        //begin mod
                                        int index4 = index3 * GRID + num18;
                                        float y = (float)(((double)index3 + 0.5 - HALFGRID) * 38.25);
                                        //end mod
                                        for (int index5 = num18; index5 <= num20; ++index5)
                                        {
                                            //begin mod
                                            float x = (float)(((double)index5 + 0.5 - HALFGRID) * 38.25);
                                            //end mood
                                            float u;
                                            float num22 = Mathf.Sqrt(Segment2.DistanceSqr(a, b, new Vector2(x, y), out u));
                                            if ((double)num22 < (double)num13 + 19.125)
                                            {
                                                int num23 = Mathf.Min((int)byte.MaxValue, Mathf.RoundToInt((float)(((double)num13 - (double)num22) * 0.0130718955770135 + 0.25) * (float)byte.MaxValue));
                                                if (num23 > (int)m_waterGrid[index4].m_conductivity)
                                                {
                                                    m_waterGrid[index4].m_conductivity = (byte)num23;
                                                    m_waterGrid[index4].m_closestPipeSegment = num14;
                                                }
                                                if (info.m_class.m_level == ItemClass.Level.Level2 && num23 > (int)m_waterGrid[index4].m_conductivity2)
                                                {
                                                    m_waterGrid[index4].m_conductivity2 = (byte)num23;
                                                    m_waterGrid[index4].m_closestPipeSegment2 = num14;
                                                }
                                            }
                                            ++index4;
                                        }
                                    }
                                }
                            }
                        }
                        num14 = array16_2.m_buffer[(int)num14].m_nextGridSegment;
                        if (++num15 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                //begin mod
                int index2 = index1 * GRID + num1;
                //end mod
                for (int index3 = num1; index3 <= num3; ++index3)
                {
                    WaterManager.Cell cell = m_waterGrid[index2];
                    if ((int)cell.m_conductivity == 0)
                    {
                        cell.m_currentWaterPressure = (short)0;
                        cell.m_currentSewagePressure = (short)0;
                        cell.m_currentHeatingPressure = (short)0;
                        cell.m_waterPulseGroup = ushort.MaxValue;
                        cell.m_sewagePulseGroup = ushort.MaxValue;
                        cell.m_heatingPulseGroup = ushort.MaxValue;
                        cell.m_tmpHasWater = false;
                        cell.m_tmpHasSewage = false;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasWater = false;
                        cell.m_hasSewage = false;
                        cell.m_hasHeating = false;
                        cell.m_pollution = (byte)0;
                        m_waterGrid[index2] = cell;
                    }
                    else if ((int)cell.m_conductivity2 == 0)
                    {
                        cell.m_currentHeatingPressure = (short)0;
                        cell.m_heatingPulseGroup = ushort.MaxValue;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasHeating = false;
                        m_waterGrid[index2] = cell;
                    }
                    ++index2;
                }
            }
            this.AreaModified(num1, num2, num3, num4);
        }


        public class Data : IDataContainer
        {
            //no major changes
            public void Serialize(DataSerializer s)
            {
                //begin mod
                //end mod
                WaterManager.Cell[] cellArray = m_waterGrid;
                int length = cellArray.Length;
                EncodedArray.Byte byte1 = EncodedArray.Byte.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                    byte1.Write(cellArray[index].m_conductivity);
                byte1.EndWrite();
                EncodedArray.Byte byte2 = EncodedArray.Byte.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                    byte2.Write(cellArray[index].m_conductivity2);
                byte2.EndWrite();
                EncodedArray.Short short1 = EncodedArray.Short.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        short1.Write(cellArray[index].m_currentWaterPressure);
                }
                short1.EndWrite();
                EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        short2.Write(cellArray[index].m_currentSewagePressure);
                }
                short2.EndWrite();
                EncodedArray.Short short3 = EncodedArray.Short.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity2 != 0)
                        short3.Write(cellArray[index].m_currentHeatingPressure);
                }
                short3.EndWrite();
                EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        ushort1.Write(cellArray[index].m_waterPulseGroup);
                }
                ushort1.EndWrite();
                EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        ushort2.Write(cellArray[index].m_sewagePulseGroup);
                }
                ushort2.EndWrite();
                EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity2 != 0)
                        ushort3.Write(cellArray[index].m_heatingPulseGroup);
                }
                ushort3.EndWrite();
                EncodedArray.UShort ushort4 = EncodedArray.UShort.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        ushort4.Write(cellArray[index].m_closestPipeSegment);
                }
                ushort4.EndWrite();
                EncodedArray.UShort ushort5 = EncodedArray.UShort.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity2 != 0)
                        ushort5.Write(cellArray[index].m_closestPipeSegment2);
                }
                ushort5.EndWrite();
                EncodedArray.Bool bool1 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        bool1.Write(cellArray[index].m_hasWater);
                }
                bool1.EndWrite();
                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        bool2.Write(cellArray[index].m_hasSewage);
                }
                bool2.EndWrite();
                EncodedArray.Bool bool3 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity2 != 0)
                        bool3.Write(cellArray[index].m_hasHeating);
                }
                bool3.EndWrite();
                EncodedArray.Bool bool4 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        bool4.Write(cellArray[index].m_tmpHasWater);
                }
                bool4.EndWrite();
                EncodedArray.Bool bool5 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        bool5.Write(cellArray[index].m_tmpHasSewage);
                }
                bool5.EndWrite();
                EncodedArray.Bool bool6 = EncodedArray.Bool.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity2 != 0)
                        bool6.Write(cellArray[index].m_tmpHasHeating);
                }
                bool6.EndWrite();
                EncodedArray.Byte byte3 = EncodedArray.Byte.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                {
                    if ((int)cellArray[index].m_conductivity != 0)
                        byte3.Write(cellArray[index].m_pollution);
                }
                byte3.EndWrite();
                s.WriteUInt16((uint)m_waterPulseGroupCount);
                for (int index = 0; index < m_waterPulseGroupCount; ++index)
                {
                    s.WriteUInt32(m_waterPulseGroups[index].m_origPressure);
                    s.WriteUInt32(m_waterPulseGroups[index].m_curPressure);
                    s.WriteUInt32(m_waterPulseGroups[index].m_collectPressure);
                    s.WriteUInt16((uint)m_waterPulseGroups[index].m_mergeIndex);
                    s.WriteUInt16((uint)m_waterPulseGroups[index].m_mergeCount);
                    s.WriteUInt16((uint)m_waterPulseGroups[index].m_node);
                }
                s.WriteUInt16((uint)m_sewagePulseGroupCount);
                for (int index = 0; index < m_sewagePulseGroupCount; ++index)
                {
                    s.WriteUInt32(m_sewagePulseGroups[index].m_origPressure);
                    s.WriteUInt32(m_sewagePulseGroups[index].m_curPressure);
                    s.WriteUInt32(m_sewagePulseGroups[index].m_collectPressure);
                    s.WriteUInt16((uint)m_sewagePulseGroups[index].m_mergeIndex);
                    s.WriteUInt16((uint)m_sewagePulseGroups[index].m_mergeCount);
                    s.WriteUInt16((uint)m_sewagePulseGroups[index].m_node);
                }
                s.WriteUInt16((uint)m_heatingPulseGroupCount);
                for (int index = 0; index < m_heatingPulseGroupCount; ++index)
                {
                    s.WriteUInt32(m_heatingPulseGroups[index].m_origPressure);
                    s.WriteUInt32(m_heatingPulseGroups[index].m_curPressure);
                    s.WriteUInt16((uint)m_heatingPulseGroups[index].m_mergeIndex);
                    s.WriteUInt16((uint)m_heatingPulseGroups[index].m_mergeCount);
                    s.WriteUInt16((uint)m_heatingPulseGroups[index].m_node);
                }
                int num1 = m_waterPulseUnitEnd - m_waterPulseUnitStart;
                if (num1 < 0)
                    num1 += m_waterPulseUnits.Length;
                s.WriteUInt16((uint)num1);
                int index1 = m_waterPulseUnitStart;
                while (index1 != m_waterPulseUnitEnd)
                {
                    s.WriteUInt16((uint)m_waterPulseUnits[index1].m_group);
                    s.WriteUInt16((uint)m_waterPulseUnits[index1].m_node);
                    //begin mod
                    s.WriteUInt16((uint)m_waterPulseUnits[index1].m_x);
                    s.WriteUInt16((uint)m_waterPulseUnits[index1].m_z);
                    //end mod
                    if (++index1 >= m_waterPulseUnits.Length)
                        index1 = 0;
                }
                int num2 = m_sewagePulseUnitEnd - m_sewagePulseUnitStart;
                if (num2 < 0)
                    num2 += m_sewagePulseUnits.Length;
                s.WriteUInt16((uint)num2);
                int index2 = m_sewagePulseUnitStart;
                while (index2 != m_sewagePulseUnitEnd)
                {
                    s.WriteUInt16((uint)m_sewagePulseUnits[index2].m_group);
                    s.WriteUInt16((uint)m_sewagePulseUnits[index2].m_node);
                    //begin mod
                    s.WriteUInt16((uint)m_sewagePulseUnits[index2].m_x);
                    s.WriteUInt16((uint)m_sewagePulseUnits[index2].m_z);
                    //end mod
                    if (++index2 >= m_sewagePulseUnits.Length)
                        index2 = 0;
                }
                int num3 = m_heatingPulseUnitEnd - m_heatingPulseUnitStart;
                if (num3 < 0)
                    num3 += m_heatingPulseUnits.Length;
                s.WriteUInt16((uint)num3);
                int index3 = m_heatingPulseUnitStart;
                while (index3 != m_heatingPulseUnitEnd)
                {
                    s.WriteUInt16((uint)m_heatingPulseUnits[index3].m_group);
                    s.WriteUInt16((uint)m_heatingPulseUnits[index3].m_node);
                    //begin mod
                    s.WriteUInt16((uint)m_heatingPulseUnits[index3].m_x);
                    s.WriteUInt16((uint)m_heatingPulseUnits[index3].m_z);
                    //end mod
                    if (++index3 >= m_heatingPulseUnits.Length)
                        index3 = 0;
                }
                //begin mod
                //end mod
                s.WriteInt32(m_processedCells);
                s.WriteInt32(m_conductiveCells);
                s.WriteBool(m_canContinue);
                //begin mod
                //end mod
            }

            public void Deserialize(DataSerializer s)
            {
                m_waterGrid = null;
                //begin mod
                if (s.version < 241U) //1.3.0
                {
                    return;
                }
                m_waterGrid = new WaterManager.Cell[GRID * GRID];
                m_waterPulseGroups = new WaterManager.PulseGroup[1024];
                m_sewagePulseGroups = new WaterManager.PulseGroup[1024];
                m_heatingPulseGroups = new WaterManager.PulseGroup[1024];
                m_waterPulseUnits = new PulseUnit[32768];
                m_sewagePulseUnits = new PulseUnit[32768];
                m_heatingPulseUnits = new PulseUnit[32768];
                WaterManager.Cell[] cellArray = m_waterGrid;
                //end mod
                int length = cellArray.Length;
                EncodedArray.Byte byte1 = EncodedArray.Byte.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_conductivity = byte1.Read();
                byte1.EndRead();
                if (s.version >= 227U)
                {
                    EncodedArray.Byte byte2 = EncodedArray.Byte.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_conductivity2 = byte2.Read();
                    byte2.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_conductivity2 = (byte)0;
                }
                EncodedArray.Short short1 = EncodedArray.Short.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_currentWaterPressure = (int)cellArray[index].m_conductivity == 0 ? (short)0 : short1.Read();
                short1.EndRead();
                EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_currentSewagePressure = (int)cellArray[index].m_conductivity == 0 ? (short)0 : short2.Read();
                short2.EndRead();
                if (s.version >= 227U)
                {
                    EncodedArray.Short short3 = EncodedArray.Short.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_currentHeatingPressure = (int)cellArray[index].m_conductivity2 == 0 ? (short)0 : short3.Read();
                    short3.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_currentHeatingPressure = (short)0;
                }
                EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_waterPulseGroup = (int)cellArray[index].m_conductivity == 0 ? ushort.MaxValue : ushort1.Read();
                ushort1.EndRead();
                EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_sewagePulseGroup = (int)cellArray[index].m_conductivity == 0 ? ushort.MaxValue : ushort2.Read();
                ushort2.EndRead();
                if (s.version >= 227U)
                {
                    EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_heatingPulseGroup = (int)cellArray[index].m_conductivity2 == 0 ? ushort.MaxValue : ushort3.Read();
                    ushort3.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_heatingPulseGroup = ushort.MaxValue;
                }
                if (s.version >= 73U)
                {
                    EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_closestPipeSegment = (int)cellArray[index].m_conductivity == 0 ? (ushort)0 : ushort3.Read();
                    ushort3.EndRead();
                    //begin mod
                }
                //end mod
                if (s.version >= 227U)
                {
                    EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_closestPipeSegment2 = (int)cellArray[index].m_conductivity2 == 0 ? (ushort)0 : ushort3.Read();
                    ushort3.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_closestPipeSegment2 = (ushort)0;
                }
                EncodedArray.Bool bool1 = EncodedArray.Bool.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_hasWater = (int)cellArray[index].m_conductivity != 0 && bool1.Read();
                bool1.EndRead();
                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_hasSewage = (int)cellArray[index].m_conductivity != 0 && bool2.Read();
                bool2.EndRead();
                if (s.version >= 227U)
                {
                    EncodedArray.Bool bool3 = EncodedArray.Bool.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_hasHeating = (int)cellArray[index].m_conductivity2 != 0 && bool3.Read();
                    bool3.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_hasHeating = false;
                }
                EncodedArray.Bool bool4 = EncodedArray.Bool.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_tmpHasWater = (int)cellArray[index].m_conductivity != 0 && bool4.Read();
                bool4.EndRead();
                EncodedArray.Bool bool5 = EncodedArray.Bool.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    cellArray[index].m_tmpHasSewage = (int)cellArray[index].m_conductivity != 0 && bool5.Read();
                bool5.EndRead();
                if (s.version >= 227U)
                {
                    EncodedArray.Bool bool3 = EncodedArray.Bool.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_tmpHasHeating = (int)cellArray[index].m_conductivity2 != 0 && bool3.Read();
                    bool3.EndRead();
                }
                else
                {
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_tmpHasHeating = false;
                }
                if (s.version >= 23U)
                {
                    EncodedArray.Byte byte2 = EncodedArray.Byte.BeginRead(s);
                    for (int index = 0; index < length; ++index)
                        cellArray[index].m_pollution = (int)cellArray[index].m_conductivity == 0 ? (byte)0 : byte2.Read();
                    byte2.EndRead();
                }
                m_waterPulseGroupCount = (int)s.ReadUInt16();
                for (int index = 0; index < m_waterPulseGroupCount; ++index)
                {
                    m_waterPulseGroups[index].m_origPressure = s.ReadUInt32();
                    m_waterPulseGroups[index].m_curPressure = s.ReadUInt32();
                    m_waterPulseGroups[index].m_collectPressure = s.version < 270U ? 0U : s.ReadUInt32();
                    m_waterPulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                    m_waterPulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                    m_waterPulseGroups[index].m_node = (ushort)s.ReadUInt16();
                }
                m_sewagePulseGroupCount = (int)s.ReadUInt16();
                for (int index = 0; index < m_sewagePulseGroupCount; ++index)
                {
                    m_sewagePulseGroups[index].m_origPressure = s.ReadUInt32();
                    m_sewagePulseGroups[index].m_curPressure = s.ReadUInt32();
                    m_sewagePulseGroups[index].m_collectPressure = s.version < 306U ? 0U : s.ReadUInt32();
                    m_sewagePulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                    m_sewagePulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                    m_sewagePulseGroups[index].m_node = (ushort)s.ReadUInt16();
                }
                if (s.version >= 227U)
                {
                    m_heatingPulseGroupCount = (int)s.ReadUInt16();
                    for (int index = 0; index < m_heatingPulseGroupCount; ++index)
                    {
                        m_heatingPulseGroups[index].m_origPressure = s.ReadUInt32();
                        m_heatingPulseGroups[index].m_curPressure = s.ReadUInt32();
                        m_heatingPulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                        m_heatingPulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                        m_heatingPulseGroups[index].m_node = (ushort)s.ReadUInt16();
                    }
                }
                else
                    m_heatingPulseGroupCount = 0;
                int num1 = (int)s.ReadUInt16();
                m_waterPulseUnitStart = 0;
                m_waterPulseUnitEnd = num1 % m_waterPulseUnits.Length;
                for (int index = 0; index < num1; ++index)
                {
                    m_waterPulseUnits[index].m_group = (ushort)s.ReadUInt16();
                    m_waterPulseUnits[index].m_node = (ushort)s.ReadUInt16();
                    //begin mod
                    m_waterPulseUnits[index].m_x = (ushort)s.ReadUInt16();
                    m_waterPulseUnits[index].m_z = (ushort)s.ReadUInt16();
                    //end mod
                }
                int num2 = (int)s.ReadUInt16();
                m_sewagePulseUnitStart = 0;
                m_sewagePulseUnitEnd = num2 % m_sewagePulseUnits.Length;
                for (int index = 0; index < num2; ++index)
                {
                    m_sewagePulseUnits[index].m_group = (ushort)s.ReadUInt16();
                    m_sewagePulseUnits[index].m_node = (ushort)s.ReadUInt16();
                    //begin mod
                    m_sewagePulseUnits[index].m_x = (ushort)s.ReadUInt16();
                    m_sewagePulseUnits[index].m_z = (ushort)s.ReadUInt16();
                    //end mod
                }
                if (s.version >= 227U)
                {
                    int num3 = (int)s.ReadUInt16();
                    m_heatingPulseUnitStart = 0;
                    m_heatingPulseUnitEnd = num3 % m_heatingPulseUnits.Length;
                    for (int index = 0; index < num3; ++index)
                    {
                        m_heatingPulseUnits[index].m_group = (ushort)s.ReadUInt16();
                        m_heatingPulseUnits[index].m_node = (ushort)s.ReadUInt16();
                        //begin mod
                        m_heatingPulseUnits[index].m_x = (ushort)s.ReadUInt16();
                        m_heatingPulseUnits[index].m_z = (ushort)s.ReadUInt16();
                        //end mod
                    }
                }
                else
                {
                    m_heatingPulseUnitStart = 0;
                    m_heatingPulseUnitEnd = 0;
                }
                //begin mod
                //end mod
                m_processedCells = s.ReadInt32();
                m_conductiveCells = s.ReadInt32();
                m_canContinue = s.ReadBool();
                //begin mod
                //end mod
            }

            public void AfterDeserialize(DataSerializer s)
            {
                //begin mod
                Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
                WaterManager.instance.AreaModified(0, 0, GRID - 1, GRID - 1);
                //end mod
            }
        }
    }
}
