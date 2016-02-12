using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using EightyOne.Redirection;

//TODO(earalov): review this class
namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(WaterManager))]
    public class FakeWaterManager
    {

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateNodeWater(WaterManager manager, int nodeID, int water, int sewage)
        {
            UnityEngine.Debug.Log($"{manager}-{nodeID}-{water}-{sewage}");
        }

        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                Cell[] waterGrid = FakeWaterManager.m_waterGrid;
                int num = waterGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int i = 0; i < num; i++)
                {
                    @byte.Write(waterGrid[i].m_conductivity);
                }
                @byte.EndWrite();
                EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
                for (int j = 0; j < num; j++)
                {
                    if (waterGrid[j].m_conductivity != 0)
                    {
                        @short.Write(waterGrid[j].m_currentWaterPressure);
                    }
                }
                @short.EndWrite();
                EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
                for (int k = 0; k < num; k++)
                {
                    if (waterGrid[k].m_conductivity != 0)
                    {
                        short2.Write(waterGrid[k].m_currentSewagePressure);
                    }
                }
                short2.EndWrite();
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
                for (int l = 0; l < num; l++)
                {
                    if (waterGrid[l].m_conductivity != 0)
                    {
                        uShort.Write(waterGrid[l].m_waterPulseGroup);
                    }
                }
                uShort.EndWrite();
                EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
                for (int m = 0; m < num; m++)
                {
                    if (waterGrid[m].m_conductivity != 0)
                    {
                        uShort2.Write(waterGrid[m].m_sewagePulseGroup);
                    }
                }
                uShort2.EndWrite();
                EncodedArray.UShort uShort3 = EncodedArray.UShort.BeginWrite(s);
                for (int n = 0; n < num; n++)
                {
                    if (waterGrid[n].m_conductivity != 0)
                    {
                        uShort3.Write(waterGrid[n].m_closestPipeSegment);
                    }
                }
                uShort3.EndWrite();
                EncodedArray.Bool @bool = EncodedArray.Bool.BeginWrite(s);
                for (int num2 = 0; num2 < num; num2++)
                {
                    if (waterGrid[num2].m_conductivity != 0)
                    {
                        @bool.Write(waterGrid[num2].m_hasWater);
                    }
                }
                @bool.EndWrite();
                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginWrite(s);
                for (int num3 = 0; num3 < num; num3++)
                {
                    if (waterGrid[num3].m_conductivity != 0)
                    {
                        bool2.Write(waterGrid[num3].m_hasSewage);
                    }
                }
                bool2.EndWrite();
                EncodedArray.Bool bool3 = EncodedArray.Bool.BeginWrite(s);
                for (int num4 = 0; num4 < num; num4++)
                {
                    if (waterGrid[num4].m_conductivity != 0)
                    {
                        bool3.Write(waterGrid[num4].m_tmpHasWater);
                    }
                }
                bool3.EndWrite();
                EncodedArray.Bool bool4 = EncodedArray.Bool.BeginWrite(s);
                for (int num5 = 0; num5 < num; num5++)
                {
                    if (waterGrid[num5].m_conductivity != 0)
                    {
                        bool4.Write(waterGrid[num5].m_tmpHasSewage);
                    }
                }
                bool4.EndWrite();
                EncodedArray.Byte byte2 = EncodedArray.Byte.BeginWrite(s);
                for (int num6 = 0; num6 < num; num6++)
                {
                    if (waterGrid[num6].m_conductivity != 0)
                    {
                        byte2.Write(waterGrid[num6].m_pollution);
                    }
                }
                byte2.EndWrite();

                s.WriteUInt16((uint)FakeWaterManager.m_waterPulseGroupCount);
                for (int num7 = 0; num7 < FakeWaterManager.m_waterPulseGroupCount; num7++)
                {
                    s.WriteUInt32(FakeWaterManager.m_waterPulseGroups[num7].m_origPressure);
                    s.WriteUInt32(FakeWaterManager.m_waterPulseGroups[num7].m_curPressure);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseGroups[num7].m_mergeIndex);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseGroups[num7].m_mergeCount);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseGroups[num7].m_node);
                }
                s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseGroupCount);
                for (int num8 = 0; num8 < FakeWaterManager.m_sewagePulseGroupCount; num8++)
                {
                    s.WriteUInt32(FakeWaterManager.m_sewagePulseGroups[num8].m_origPressure);
                    s.WriteUInt32(FakeWaterManager.m_sewagePulseGroups[num8].m_curPressure);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseGroups[num8].m_mergeIndex);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseGroups[num8].m_mergeCount);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseGroups[num8].m_node);
                }
                int num9 = FakeWaterManager.m_waterPulseUnitEnd - FakeWaterManager.m_waterPulseUnitStart;
                if (num9 < 0)
                {
                    num9 += FakeWaterManager.m_waterPulseUnits.Length;
                }
                s.WriteUInt16((uint)num9);
                int num10 = FakeWaterManager.m_waterPulseUnitStart;
                while (num10 != FakeWaterManager.m_waterPulseUnitEnd)
                {
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseUnits[num10].m_group);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseUnits[num10].m_node);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseUnits[num10].m_x);
                    s.WriteUInt16((uint)FakeWaterManager.m_waterPulseUnits[num10].m_z);
                    if (++num10 >= FakeWaterManager.m_waterPulseUnits.Length)
                    {
                        num10 = 0;
                    }
                }
                int num11 = FakeWaterManager.m_sewagePulseUnitEnd - FakeWaterManager.m_sewagePulseUnitStart;
                if (num11 < 0)
                {
                    num11 += FakeWaterManager.m_sewagePulseUnits.Length;
                }
                s.WriteUInt16((uint)num11);
                int num12 = FakeWaterManager.m_sewagePulseUnitStart;
                while (num12 != FakeWaterManager.m_sewagePulseUnitEnd)
                {
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseUnits[num12].m_group);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseUnits[num12].m_node);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseUnits[num12].m_x);
                    s.WriteUInt16((uint)FakeWaterManager.m_sewagePulseUnits[num12].m_z);
                    if (++num12 >= FakeWaterManager.m_sewagePulseUnits.Length)
                    {
                        num12 = 0;
                    }
                }
                EncodedArray.UShort uShort4 = EncodedArray.UShort.BeginWrite(s);
                for (int num13 = 0; num13 < 32768; num13++)
                {
                    uShort4.Write(FakeWaterManager.m_nodeData[num13].m_waterPulseGroup);
                }
                uShort4.EndWrite();
                EncodedArray.UShort uShort5 = EncodedArray.UShort.BeginWrite(s);
                for (int num14 = 0; num14 < 32768; num14++)
                {
                    uShort5.Write(FakeWaterManager.m_nodeData[num14].m_curWaterPressure);
                }
                uShort5.EndWrite();
                EncodedArray.UShort uShort6 = EncodedArray.UShort.BeginWrite(s);
                for (int num15 = 0; num15 < 32768; num15++)
                {
                    uShort6.Write(FakeWaterManager.m_nodeData[num15].m_extraWaterPressure);
                }
                uShort6.EndWrite();
                EncodedArray.UShort uShort7 = EncodedArray.UShort.BeginWrite(s);
                for (int num16 = 0; num16 < 32768; num16++)
                {
                    uShort7.Write(FakeWaterManager.m_nodeData[num16].m_sewagePulseGroup);
                }
                uShort7.EndWrite();
                EncodedArray.UShort uShort8 = EncodedArray.UShort.BeginWrite(s);
                for (int num17 = 0; num17 < 32768; num17++)
                {
                    uShort8.Write(FakeWaterManager.m_nodeData[num17].m_curSewagePressure);
                }
                uShort8.EndWrite();
                EncodedArray.UShort uShort9 = EncodedArray.UShort.BeginWrite(s);
                for (int num18 = 0; num18 < 32768; num18++)
                {
                    uShort9.Write(FakeWaterManager.m_nodeData[num18].m_extraSewagePressure);
                }
                uShort9.EndWrite();
                EncodedArray.Byte byte3 = EncodedArray.Byte.BeginWrite(s);
                for (int num19 = 0; num19 < 32768; num19++)
                {
                    byte3.Write(FakeWaterManager.m_nodeData[num19].m_pollution);
                }
                byte3.EndWrite();
                s.WriteInt32(FakeWaterManager.m_processedCells);
                s.WriteInt32(FakeWaterManager.m_conductiveCells);
                s.WriteBool(FakeWaterManager.m_canContinue);
            }

            public void Deserialize(DataSerializer s)
            {
                Cell[] waterGrid = new Cell[GRID * GRID];
                int num = waterGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                for (int i = 0; i < num; i++)
                {
                    waterGrid[i].m_conductivity = @byte.Read();
                }
                @byte.EndRead();
                EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
                for (int j = 0; j < num; j++)
                {
                    if (waterGrid[j].m_conductivity != 0)
                    {
                        waterGrid[j].m_currentWaterPressure = @short.Read();
                    }
                    else
                    {
                        waterGrid[j].m_currentWaterPressure = 0;
                    }
                }
                @short.EndRead();
                EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
                for (int k = 0; k < num; k++)
                {
                    if (waterGrid[k].m_conductivity != 0)
                    {
                        waterGrid[k].m_currentSewagePressure = short2.Read();
                    }
                    else
                    {
                        waterGrid[k].m_currentSewagePressure = 0;
                    }
                }
                short2.EndRead();
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
                for (int l = 0; l < num; l++)
                {
                    if (waterGrid[l].m_conductivity != 0)
                    {
                        waterGrid[l].m_waterPulseGroup = uShort.Read();
                    }
                    else
                    {
                        waterGrid[l].m_waterPulseGroup = 65535;
                    }
                }
                uShort.EndRead();
                EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
                for (int m = 0; m < num; m++)
                {
                    if (waterGrid[m].m_conductivity != 0)
                    {
                        waterGrid[m].m_sewagePulseGroup = uShort2.Read();
                    }
                    else
                    {
                        waterGrid[m].m_sewagePulseGroup = 65535;
                    }
                }
                uShort2.EndRead();
                EncodedArray.UShort uShort3 = EncodedArray.UShort.BeginRead(s);
                for (int n = 0; n < num; n++)
                {
                    if (waterGrid[n].m_conductivity != 0)
                    {
                        waterGrid[n].m_closestPipeSegment = uShort3.Read();
                    }
                    else
                    {
                        waterGrid[n].m_closestPipeSegment = 0;
                    }
                }
                uShort3.EndRead();

                EncodedArray.Bool @bool = EncodedArray.Bool.BeginRead(s);
                for (int num2 = 0; num2 < num; num2++)
                {
                    if (waterGrid[num2].m_conductivity != 0)
                    {
                        waterGrid[num2].m_hasWater = @bool.Read();
                    }
                    else
                    {
                        waterGrid[num2].m_hasWater = false;
                    }
                }
                @bool.EndRead();

                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginRead(s);
                for (int num3 = 0; num3 < num; num3++)
                {
                    if (waterGrid[num3].m_conductivity != 0)
                    {
                        waterGrid[num3].m_hasSewage = bool2.Read();
                    }
                    else
                    {
                        waterGrid[num3].m_hasSewage = false;
                    }
                }
                bool2.EndRead();

                EncodedArray.Bool bool3 = EncodedArray.Bool.BeginRead(s);
                for (int num4 = 0; num4 < num; num4++)
                {
                    if (waterGrid[num4].m_conductivity != 0)
                    {
                        waterGrid[num4].m_tmpHasWater = bool3.Read();
                    }
                    else
                    {
                        waterGrid[num4].m_tmpHasWater = false;
                    }
                }
                bool3.EndRead();

                EncodedArray.Bool bool4 = EncodedArray.Bool.BeginRead(s);
                for (int num5 = 0; num5 < num; num5++)
                {
                    if (waterGrid[num5].m_conductivity != 0)
                    {
                        waterGrid[num5].m_tmpHasSewage = bool4.Read();
                    }
                    else
                    {
                        waterGrid[num5].m_tmpHasSewage = false;
                    }
                }
                bool4.EndRead();

                EncodedArray.Byte byte2 = EncodedArray.Byte.BeginRead(s);
                for (int num6 = 0; num6 < num; num6++)
                {
                    if (waterGrid[num6].m_conductivity != 0)
                    {
                        waterGrid[num6].m_pollution = byte2.Read();
                    }
                    else
                    {
                        waterGrid[num6].m_pollution = 0;
                    }
                }
                byte2.EndRead();
                FakeWaterManager.m_waterGrid = waterGrid;
                FakeWaterManager.m_nodeData = new Node[32768];
                FakeWaterManager.m_waterPulseGroups = new PulseGroup[1024];
                FakeWaterManager.m_sewagePulseGroups = new PulseGroup[1024];
                FakeWaterManager.m_waterPulseUnits = new PulseUnit[32768];
                FakeWaterManager.m_sewagePulseUnits = new PulseUnit[32768];

                FakeWaterManager.m_waterPulseGroupCount = (int)s.ReadUInt16();
                for (int num7 = 0; num7 < FakeWaterManager.m_waterPulseGroupCount; num7++)
                {
                    FakeWaterManager.m_waterPulseGroups[num7].m_origPressure = s.ReadUInt32();
                    FakeWaterManager.m_waterPulseGroups[num7].m_curPressure = s.ReadUInt32();
                    FakeWaterManager.m_waterPulseGroups[num7].m_mergeIndex = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_waterPulseGroups[num7].m_mergeCount = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_waterPulseGroups[num7].m_node = (ushort)s.ReadUInt16();
                }
                FakeWaterManager.m_sewagePulseGroupCount = (int)s.ReadUInt16();
                for (int num8 = 0; num8 < FakeWaterManager.m_sewagePulseGroupCount; num8++)
                {
                    FakeWaterManager.m_sewagePulseGroups[num8].m_origPressure = s.ReadUInt32();
                    FakeWaterManager.m_sewagePulseGroups[num8].m_curPressure = s.ReadUInt32();
                    FakeWaterManager.m_sewagePulseGroups[num8].m_mergeIndex = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_sewagePulseGroups[num8].m_mergeCount = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_sewagePulseGroups[num8].m_node = (ushort)s.ReadUInt16();
                }
                int num9 = (int)s.ReadUInt16();
                FakeWaterManager.m_waterPulseUnitStart = 0;
                FakeWaterManager.m_waterPulseUnitEnd = num9 % FakeWaterManager.m_waterPulseUnits.Length;
                for (int num10 = 0; num10 < num9; num10++)
                {
                    FakeWaterManager.m_waterPulseUnits[num10].m_group = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_waterPulseUnits[num10].m_node = (ushort)s.ReadUInt16();
                    //begin mod
                    FakeWaterManager.m_waterPulseUnits[num10].m_x = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_waterPulseUnits[num10].m_z = (ushort)s.ReadUInt16();
                    //end mod
                }
                int num11 = (int)s.ReadUInt16();
                FakeWaterManager.m_sewagePulseUnitStart = 0;
                FakeWaterManager.m_sewagePulseUnitEnd = num11 % FakeWaterManager.m_sewagePulseUnits.Length;
                for (int num12 = 0; num12 < num11; num12++)
                {
                    FakeWaterManager.m_sewagePulseUnits[num12].m_group = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_sewagePulseUnits[num12].m_node = (ushort)s.ReadUInt16();
                    //begin mod
                    FakeWaterManager.m_sewagePulseUnits[num12].m_x = (ushort)s.ReadUInt16();
                    FakeWaterManager.m_sewagePulseUnits[num12].m_z = (ushort)s.ReadUInt16();
                    //end mod
                }
                int num13 = 32768;
                EncodedArray.UShort uShort4 = EncodedArray.UShort.BeginRead(s);
                for (int num15 = 0; num15 < num13; num15++)
                {
                    FakeWaterManager.m_nodeData[num15].m_waterPulseGroup = uShort4.Read();
                }
                uShort4.EndRead();
                EncodedArray.UShort uShort5 = EncodedArray.UShort.BeginRead(s);
                for (int num16 = 0; num16 < num13; num16++)
                {
                    FakeWaterManager.m_nodeData[num16].m_curWaterPressure = uShort5.Read();
                }
                uShort5.EndRead();
                EncodedArray.UShort uShort6 = EncodedArray.UShort.BeginRead(s);
                for (int num17 = 0; num17 < num13; num17++)
                {
                    FakeWaterManager.m_nodeData[num17].m_extraWaterPressure = uShort6.Read();
                }
                uShort6.EndRead();
                EncodedArray.UShort uShort7 = EncodedArray.UShort.BeginRead(s);
                for (int num18 = 0; num18 < num13; num18++)
                {
                    FakeWaterManager.m_nodeData[num18].m_sewagePulseGroup = uShort7.Read();
                }
                uShort7.EndRead();
                EncodedArray.UShort uShort8 = EncodedArray.UShort.BeginRead(s);
                for (int num19 = 0; num19 < num13; num19++)
                {
                    FakeWaterManager.m_nodeData[num19].m_curSewagePressure = uShort8.Read();
                }
                uShort8.EndRead();
                EncodedArray.UShort uShort9 = EncodedArray.UShort.BeginRead(s);
                for (int num20 = 0; num20 < num13; num20++)
                {
                    FakeWaterManager.m_nodeData[num20].m_extraSewagePressure = uShort9.Read();
                }
                uShort9.EndRead();

                EncodedArray.Byte byte3 = EncodedArray.Byte.BeginRead(s);
                for (int num21 = 0; num21 < num13; num21++)
                {
                    FakeWaterManager.m_nodeData[num21].m_pollution = byte3.Read();
                }
                byte3.EndRead();

                FakeWaterManager.m_processedCells = s.ReadInt32();
                FakeWaterManager.m_conductiveCells = s.ReadInt32();
                FakeWaterManager.m_canContinue = s.ReadBool();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();
                WaterManager.instance.AreaModified(0, 0, GRID - 1, GRID - 1);
            }
        }

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

        internal struct PulseGroup
        {
            public uint m_origPressure;
            public uint m_curPressure;
            public ushort m_mergeIndex;
            public ushort m_mergeCount;
            public ushort m_node;
        }

        internal struct PulseUnit
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

        internal static int m_processedCells;
        internal static int m_conductiveCells;
        internal static bool m_canContinue;
        private static int m_modifiedX1;
        private static int m_modifiedZ1;
        private static int m_modifiedX2;
        private static int m_modifiedZ2;

        public static Node[] m_nodeData;
        internal static Cell[] m_waterGrid;
        internal static PulseGroup[] m_waterPulseGroups;
        internal static PulseGroup[] m_sewagePulseGroups;
        internal static PulseUnit[] m_waterPulseUnits;
        internal static PulseUnit[] m_sewagePulseUnits;
        private static Texture2D m_waterTexture;
        internal static int m_waterPulseGroupCount;
        private static int m_waterPulseUnitStart;
        internal static int m_waterPulseUnitEnd;
        internal static int m_sewagePulseGroupCount;
        private static int m_sewagePulseUnitStart;
        internal static int m_sewagePulseUnitEnd;

        static FieldInfo m_refreshGrid;
        static FieldInfo undergroundCamera;

        public static void Init()
        {
            var wm = WaterManager.instance;
            if (m_waterGrid == null)
            {
                m_waterGrid = new Cell[GRID * GRID];
                var oldGrid = (IList)typeof(WaterManager).GetField("m_waterGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wm);
                int oldGridSize = 256;
                int diff = (GRID - oldGridSize) / 2;
                var fields = Util.GetFieldsFromStruct(m_waterGrid[0], oldGrid[0]);
                for (var i = 0; i < oldGridSize; i += 1)
                {
                    for (var j = 0; j < oldGridSize; j += 1)
                    {
                        m_waterGrid[(j + diff) * GRID + (i + diff)] = (Cell)Util.CopyStruct(new Cell(), oldGrid[j * oldGridSize + i], fields);
                    }
                }

                m_nodeData = new Node[32768];
                Util.CopyStructArray(m_nodeData, wm, "m_nodeData");

                m_waterPulseGroups = new PulseGroup[1024];
                Util.CopyStructArray(m_waterPulseGroups, wm, "m_waterPulseGroups");
                m_sewagePulseGroups = new PulseGroup[1024];
                Util.CopyStructArray(m_sewagePulseGroups, wm, "m_sewagePulseGroups");

                m_waterPulseUnits = new PulseUnit[32768];
                Util.CopyStructArray(m_waterPulseUnits, wm, "m_waterPulseUnits");
                m_sewagePulseUnits = new PulseUnit[32768];
                Util.CopyStructArray(m_sewagePulseUnits, wm, "m_sewagePulseUnits");


                Util.SetPropertyValue(ref m_waterPulseGroupCount,wm, "m_waterPulseGroupCount");
                Util.SetPropertyValue(ref m_sewagePulseGroupCount,wm, "m_sewagePulseGroupCount");

                m_waterPulseUnitStart = 0;
                m_sewagePulseUnitStart = 0;

                Util.SetPropertyValue(ref m_waterPulseUnitEnd, wm, "m_waterPulseUnitEnd");
                Util.SetPropertyValue(ref m_sewagePulseUnitEnd, wm, "m_sewagePulseUnitEnd");

                Util.SetPropertyValue(ref m_processedCells, wm, "m_processedCells");
                Util.SetPropertyValue(ref m_conductiveCells, wm, "m_conductiveCells");
                Util.SetPropertyValue(ref m_canContinue, wm, "m_canContinue");
            }
            m_modifiedX1 = 0;
            m_modifiedZ1 = 0;
            m_modifiedX2 = GRID - 1;
            m_modifiedZ2 = GRID - 1;

            m_refreshGrid = typeof(WaterManager).GetField("m_refreshGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            undergroundCamera = typeof(WaterManager).GetField("m_undergroundCamera", BindingFlags.NonPublic | BindingFlags.Instance);

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
        }

        [RedirectMethod]
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
                    Cell cell = m_waterGrid[i * GRID + j];
                    Color color;
                    if (cell.m_closestPipeSegment != 0 && j != 0 && i != 0 && j != GRID - 1 && i != GRID - 1)
                    {
                        ushort startNode = instance.m_segments.m_buffer[(int)cell.m_closestPipeSegment].m_startNode;
                        ushort endNode = instance.m_segments.m_buffer[(int)cell.m_closestPipeSegment].m_endNode;
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

        [RedirectMethod]
        [IgnoreIfRemoveNeedForPipesEnabled]
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

        [RedirectMethod]
        private ushort GetRootWaterGroup(ushort group)
        {
            for (ushort mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_waterPulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        [RedirectMethod]
        private ushort GetRootSewageGroup(ushort group)
        {
            for (ushort mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_sewagePulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        [RedirectMethod]
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

        [RedirectMethod]
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

        //this method can't be reditected since it uses different Cell class in parameters
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

        //this method can't be reditected since it uses different Cell class in parameters
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

        [RedirectMethod]
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

        [RedirectMethod]
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

        [RedirectMethod]
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

        [RedirectMethod]
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

        [RedirectMethod]
        protected void SimulationStepImpl(int subStep)
        {
            if (subStep == 0 || subStep == 1000)
            {
                return;
            }
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num = (int)(currentFrameIndex % 256);
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
                            UpdateNodeWater(WaterManager.instance, i, water, sewage);
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


                int num4 = num * 4;
                if (num4 < GRID)
                {
                    int num5 = Math.Min(GRID, num4 + 4);
                    for (int j = num4; j < num5; j++)
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
            }
            else
            {
                int num7 = (num - 127) * m_conductiveCells >> 7;
                if (num == 255)
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
                if (num == 255)
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

        [RedirectMethod]
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
