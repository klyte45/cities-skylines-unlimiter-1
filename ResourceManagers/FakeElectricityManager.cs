using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.ResourceManagers
{
    public class FakeElectricityManager : SerializableDataExtensionBase
    {
        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                Cell[] electricityGrid = FakeElectricityManager.electricityGrid;
                int num = electricityGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int i = 0; i < num; i++)
                {
                    @byte.Write(electricityGrid[i].m_conductivity);
                }
                @byte.EndWrite();
                EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
                for (int j = 0; j < num; j++)
                {
                    if (electricityGrid[j].m_conductivity != 0)
                    {
                        @short.Write(electricityGrid[j].m_currentCharge);
                    }
                }
                @short.EndWrite();
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
                for (int k = 0; k < num; k++)
                {
                    if (electricityGrid[k].m_conductivity != 0)
                    {
                        uShort.Write(electricityGrid[k].m_extraCharge);
                    }
                }
                uShort.EndWrite();
                EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
                for (int l = 0; l < num; l++)
                {
                    if (electricityGrid[l].m_conductivity != 0)
                    {
                        uShort2.Write(electricityGrid[l].m_pulseGroup);
                    }
                }
                uShort2.EndWrite();
                EncodedArray.Bool @bool = EncodedArray.Bool.BeginWrite(s);
                for (int m = 0; m < num; m++)
                {
                    if (electricityGrid[m].m_conductivity != 0)
                    {
                        @bool.Write(electricityGrid[m].m_electrified);
                    }
                }
                @bool.EndWrite();
                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginWrite(s);
                for (int n = 0; n < num; n++)
                {
                    if (electricityGrid[n].m_conductivity != 0)
                    {
                        bool2.Write(electricityGrid[n].m_tmpElectrified);
                    }
                }
                bool2.EndWrite();

                s.WriteUInt16((uint)FakeElectricityManager.m_pulseGroupCount);
                for (int num2 = 0; num2 < FakeElectricityManager.m_pulseGroupCount; num2++)
                {
                    s.WriteUInt32(FakeElectricityManager.m_pulseGroups[num2].m_origCharge);
                    s.WriteUInt32(FakeElectricityManager.m_pulseGroups[num2].m_curCharge);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseGroups[num2].m_mergeIndex);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseGroups[num2].m_mergeCount);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseGroups[num2].m_x);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseGroups[num2].m_z);
                }
                int num3 = FakeElectricityManager.m_pulseUnitEnd - FakeElectricityManager.m_pulseUnitStart;
                if (num3 < 0)
                {
                    num3 += FakeElectricityManager.m_pulseUnits.Length;
                }
                s.WriteUInt16((uint)num3);
                int num4 = FakeElectricityManager.m_pulseUnitStart;
                while (num4 != FakeElectricityManager.m_pulseUnitEnd)
                {
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseUnits[num4].m_group);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseUnits[num4].m_node);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseUnits[num4].m_x);
                    s.WriteUInt16((uint)FakeElectricityManager.m_pulseUnits[num4].m_z);
                    if (++num4 >= FakeElectricityManager.m_pulseUnits.Length)
                    {
                        num4 = 0;
                    }
                }
                EncodedArray.UShort uShort3 = EncodedArray.UShort.BeginWrite(s);
                for (int num5 = 0; num5 < 32768; num5++)
                {
                    uShort3.Write(FakeElectricityManager.m_nodeGroups[num5]);
                }
                uShort3.EndWrite();

                s.WriteInt32(FakeElectricityManager.m_processedCells);
                s.WriteInt32(FakeElectricityManager.m_conductiveCells);
                s.WriteBool(FakeElectricityManager.m_canContinue);
            }

            public void Deserialize(DataSerializer s)
            {
                Cell[] electricityGrid = new Cell[GRID * GRID];
                int num = electricityGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                for (int i = 0; i < num; i++)
                {
                    electricityGrid[i].m_conductivity = @byte.Read();
                }
                @byte.EndRead();

                EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
                for (int l = 0; l < num; l++)
                {
                    if (electricityGrid[l].m_conductivity != 0)
                    {
                        electricityGrid[l].m_currentCharge = @short.Read();
                    }
                    else
                    {
                        electricityGrid[l].m_currentCharge = 0;
                    }
                }
                @short.EndRead();
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
                for (int m = 0; m < num; m++)
                {
                    if (electricityGrid[m].m_conductivity != 0)
                    {
                        electricityGrid[m].m_extraCharge = uShort.Read();
                    }
                    else
                    {
                        electricityGrid[m].m_extraCharge = 0;
                    }
                }
                uShort.EndRead();
                EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
                for (int n = 0; n < num; n++)
                {
                    if (electricityGrid[n].m_conductivity != 0)
                    {
                        electricityGrid[n].m_pulseGroup = uShort2.Read();
                    }
                    else
                    {
                        electricityGrid[n].m_pulseGroup = 65535;
                    }
                }
                uShort2.EndRead();

                EncodedArray.Bool @bool = EncodedArray.Bool.BeginRead(s);
                for (int num3 = 0; num3 < num; num3++)
                {
                    if (electricityGrid[num3].m_conductivity != 0 )
                    {
                        electricityGrid[num3].m_electrified = @bool.Read();
                    }
                    else
                    {
                        electricityGrid[num3].m_electrified = false;
                    }
                }
                @bool.EndRead();
                EncodedArray.Bool bool2 = EncodedArray.Bool.BeginRead(s);
                for (int num4 = 0; num4 < num; num4++)
                {
                    if (electricityGrid[num4].m_conductivity != 0 )
                    {
                        electricityGrid[num4].m_tmpElectrified = bool2.Read();
                    }
                    else
                    {
                        electricityGrid[num4].m_tmpElectrified = false;
                    }
                }
                bool2.EndRead();
                FakeElectricityManager.electricityGrid = electricityGrid;

                FakeElectricityManager.m_pulseGroups = new PulseGroup[1024];
                FakeElectricityManager.m_pulseGroupCount = (int)s.ReadUInt16();
                for (int num5 = 0; num5 < FakeElectricityManager.m_pulseGroupCount; num5++)
                {
                    FakeElectricityManager.m_pulseGroups[num5].m_origCharge = s.ReadUInt32();
                    FakeElectricityManager.m_pulseGroups[num5].m_curCharge = s.ReadUInt32();
                    FakeElectricityManager.m_pulseGroups[num5].m_mergeIndex = (ushort)s.ReadUInt16();
                    FakeElectricityManager.m_pulseGroups[num5].m_mergeCount = (ushort)s.ReadUInt16();
                    FakeElectricityManager.m_pulseGroups[num5].m_x = (byte)s.ReadUInt16();
                    FakeElectricityManager.m_pulseGroups[num5].m_z = (byte)s.ReadUInt16();
                }

                FakeElectricityManager.m_pulseUnits = new PulseUnit[32768];
                int num6 = (int)s.ReadUInt16();
                FakeElectricityManager.m_pulseUnitStart = 0;
                FakeElectricityManager.m_pulseUnitEnd = num6 % FakeElectricityManager.m_pulseUnits.Length;
                for (int num7 = 0; num7 < num6; num7++)
                {
                    FakeElectricityManager.m_pulseUnits[num7].m_group = (ushort)s.ReadUInt16();
                    FakeElectricityManager.m_pulseUnits[num7].m_node = (ushort)s.ReadUInt16();
                    FakeElectricityManager.m_pulseUnits[num7].m_x = (byte)s.ReadUInt16();
                    FakeElectricityManager.m_pulseUnits[num7].m_z = (byte)s.ReadUInt16();
                }


                FakeElectricityManager.m_nodeGroups = new ushort[32768];
                EncodedArray.UShort uShort4 = EncodedArray.UShort.BeginRead(s);
                for (int num8 = 0; num8 < 32768; num8++)
                {
                    FakeElectricityManager.m_nodeGroups[num8] = uShort4.Read();
                }
                uShort4.EndRead();
                
                FakeElectricityManager.m_processedCells = s.ReadInt32();
                FakeElectricityManager.m_conductiveCells = s.ReadInt32();
                FakeElectricityManager.m_canContinue = s.ReadBool();
            }

            public void AfterDeserialize(DataSerializer s)
            {                
                Singleton<LoadingManager>.instance.WaitUntilEssentialScenesLoaded();                
                ElectricityManager.instance.AreaModified(0, 0, GRID, GRID);                
            }
        }

        private const string id = "fakeEM";

        public override void OnSaveData()
        {
            var em = ElectricityManager.instance;
            var oldGrid = (IList)typeof(ElectricityManager).GetField("m_electricityGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(em);
            int oldGridSize = 256;
            int diff = (GRID - oldGridSize) / 2;
            var fields = Unlimiter.GetFieldsFromStruct(oldGrid[0],electricityGrid[0]);
            for (var i = 0; i < oldGridSize; i += 1)
            {
                for (var j = 0; j < oldGridSize; j += 1)
                {
                    var oldCellIndex = j * oldGridSize + i;
                    oldGrid[oldCellIndex] = Unlimiter.CopyStruct((object)oldGrid[oldCellIndex], electricityGrid[(j + diff) * GRID + (i + diff)], fields);
                }
            }

            Unlimiter.CopyStructArrayBack(m_pulseGroups, em, "m_pulseGroups");
            Unlimiter.CopyStructArrayBack(m_pulseUnits, em, "m_pulseUnits");
            Unlimiter.CopyArrayBack(m_nodeGroups, em, "m_nodeGroups");

            Unlimiter.SetPropertyValueBack(m_pulseGroupCount,em, "m_pulseGroupCount");
            Unlimiter.SetPropertyValueBack(m_pulseUnitEnd % m_pulseUnits.Length, em, "m_pulseUnitEnd");
            Unlimiter.SetPropertyValueBack(m_processedCells,em, "m_processedCells");
            Unlimiter.SetPropertyValueBack(m_conductiveCells,em, "m_conductiveCells");
            Unlimiter.SetPropertyValueBack(m_canContinue,em, "m_canContinue");

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 1u, new Data());
                var data = ms.ToArray();
                serializableDataManager.SaveData(id, data);
            }
        }

        public override void OnLoadData()
        {
            if (!serializableDataManager.EnumerateData().Contains(id))
            {
                return;
            }
            var data = serializableDataManager.LoadData(id);
            using (var ms = new MemoryStream(data))
            {
                var s = DataSerializer.Deserialize<Data>(ms, DataSerializer.Mode.Memory);
            }
        }

        public struct Cell
        {
            public short m_currentCharge;
            public ushort m_extraCharge;
            public ushort m_pulseGroup;
            public byte m_conductivity;
            public bool m_tmpElectrified;
            public bool m_electrified;

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3} {4} {5}", m_currentCharge.ToString(), m_extraCharge.ToString(), m_pulseGroup.ToString(), m_conductivity.ToString(), m_tmpElectrified.ToString(), m_electrified.ToString());
            }
        }
        public struct PulseGroup
        {
            public uint m_origCharge;
            public uint m_curCharge;
            public ushort m_mergeIndex;
            public ushort m_mergeCount;
            public ushort m_x;
            public ushort m_z;
            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3} {4} {5}", m_origCharge.ToString(), m_curCharge.ToString(), m_mergeIndex.ToString(), m_mergeCount.ToString(), m_x.ToString(), m_z.ToString());
            }
        }
        public struct PulseUnit
        {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3}", m_group.ToString(), m_node.ToString(), m_x.ToString(), m_z.ToString());
            }
        }

        public const int GRID = 462;
        public const int HALFGRID = 231;

        public static Cell[] electricityGrid;
        public static PulseGroup[] m_pulseGroups;
        public static PulseUnit[] m_pulseUnits;
        public static int m_pulseGroupCount;
        public static int m_pulseUnitStart;
        public static int m_pulseUnitEnd;
        public static int m_processedCells;
        public static int m_conductiveCells;
        public static bool m_canContinue;
        public static ushort[] m_nodeGroups;

        private static int m_modifiedX1;
        private static int m_modifiedZ1;
        private static int m_modifiedX2;
        private static int m_modifiedZ2;

        private static Texture2D m_electricityTexture;
        static FieldInfo m_refreshGrid;


        public static void Init()
        {
            var em = ElectricityManager.instance;
            if (electricityGrid == null)
            {
                electricityGrid = new Cell[GRID * GRID];

                var oldGrid = (IList)typeof(ElectricityManager).GetField("m_electricityGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(em);
                int oldGridSize = 256;
                int diff = (GRID - oldGridSize) / 2;
                var fields = Unlimiter.GetFieldsFromStruct(electricityGrid[0], oldGrid[0]);
                for (var i = 0; i < oldGridSize; i += 1)
                {
                    for (var j = 0; j < oldGridSize; j += 1)
                    {
                        electricityGrid[(j + diff) * GRID + (i + diff)] = (Cell)Unlimiter.CopyStruct(new Cell(), oldGrid[j * oldGridSize + i], fields);                        
                    }
                }
                m_pulseGroups = new PulseGroup[1024];
                Unlimiter.CopyStructArray(m_pulseGroups, em, "m_pulseGroups");

                m_pulseUnits = new PulseUnit[32768];
                Unlimiter.CopyStructArray(m_pulseUnits, em, "m_pulseUnits");

                m_nodeGroups = new ushort[32768];
                Unlimiter.CopyArray(m_nodeGroups, em, "m_nodeGroups");

                Unlimiter.SetPropertyValue(ref m_pulseGroupCount, em, "m_pulseGroupCount");
                m_pulseUnitStart = 0;
                Unlimiter.SetPropertyValue(ref m_pulseUnitEnd, em, "m_pulseUnitEnd");
                Unlimiter.SetPropertyValue(ref m_processedCells, em, "m_processedCells");
                Unlimiter.SetPropertyValue(ref m_conductiveCells, em, "m_conductiveCells");
                Unlimiter.SetPropertyValue(ref m_canContinue, em, "m_canContinue");
            }

            m_modifiedX1 = 0;
            m_modifiedZ1 = 0;
            m_modifiedX2 = GRID - 1;
            m_modifiedZ2 = GRID - 1;
            m_refreshGrid = typeof(ElectricityManager).GetField("m_refreshGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            m_electricityTexture = new Texture2D(GRID, GRID, TextureFormat.RGBA32, false, true);
            m_electricityTexture.filterMode = FilterMode.Point;
            m_electricityTexture.wrapMode = TextureWrapMode.Clamp;
            Shader.SetGlobalTexture("_ElectricityTexture", m_electricityTexture);
        }


        internal static void OnDestroy()
        {
            if (m_electricityTexture != null)
            {
                UnityEngine.Object.Destroy(m_electricityTexture);
                m_electricityTexture = null;
            }
            electricityGrid = null;
        }

        [ReplaceMethod]
        private static void UpdateElectricityMapping(ElectricityManager em)
        {
            Vector4 vec;
            vec.z = 1 / (38.25f * GRID);
            vec.x = 0.5f;
            vec.y = 0.5f;
            vec.w = 0.00390625f;
            Shader.SetGlobalVector("_ElectricityMapping", vec);
        }

        [ReplaceMethod]
        private void UpdateTexture()
        {
            while (!Monitor.TryEnter(electricityGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
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
                Monitor.Exit(electricityGrid);
            }
            for (int i = modifiedZ; i <= modifiedZ2; i++)
            {
                for (int j = modifiedX; j <= modifiedX2; j++)
                {
                    Cell cell = electricityGrid[i * GRID + j];
                    Color color;
                    if (cell.m_conductivity >= 64 && j != 0 && i != 0 && j != GRID - 1 && i != GRID - 1)
                    {
                        color.r = Mathf.Min(1f, 0.7f + (float)(cell.m_conductivity - 64) * 0.00208333344f);
                        color.g = 0f;
                        color.b = 0f;
                        color.a = 0f;
                    }
                    else
                    {
                        color.r = 0f;
                        color.g = 0f;
                        color.b = 0f;
                        color.a = 0f;
                    }
                    m_electricityTexture.SetPixel(j, i, color);
                }
            }
            m_electricityTexture.Apply();
        }

        [ReplaceMethod]
        public void AreaModified(int minX, int minZ, int maxX, int maxZ)
        {
            while (!Monitor.TryEnter(electricityGrid, SimulationManager.SYNCHRONIZE_TIMEOUT))
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
                Monitor.Exit(electricityGrid);
            }
        }

        [ReplaceMethod]
        public int TryDumpElectricity(Vector3 pos, int rate, int max)
        {
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            if (max > 15000)
            {
                int num3 = (max + 14999) / 15000;
                int num4 = Mathf.RoundToInt(Mathf.Sqrt((float)num3));
                int num5 = num3 / num4;
                int num6 = Mathf.Max(0, num - (num4 >> 1));
                int num7 = Mathf.Max(0, num2 - (num5 >> 1));
                int num8 = Mathf.Min(GRID - 1, num + (num4 >> 1));
                int num9 = Mathf.Min(GRID - 1, num2 + (num5 >> 1));
                num3 = (num8 - num6 + 1) * (num9 - num7 + 1);
                rate /= num3;
                max /= num3;
                int num10 = 0;
                for (int i = num7; i <= num9; i++)
                {
                    for (int j = num6; j <= num8; j++)
                    {
                        num10 += TryDumpElectricity(j, i, rate, max);
                    }
                }
                return num10;
            }
            return TryDumpElectricity(num, num2, rate, max);
        }

        private int TryDumpElectricity(int x, int z, int rate, int max)
        {
            int num = z * GRID + x;
            Cell cell = electricityGrid[num];
            if (cell.m_extraCharge != 0)
            {
                int num2 = Mathf.Min(rate, (int)cell.m_extraCharge);
                cell.m_currentCharge += (short)num2;
                cell.m_extraCharge -= (ushort)num2;
                rate -= num2;
            }
            rate = Mathf.Min(Mathf.Min(rate, max), (int)(32767 - cell.m_currentCharge));
            cell.m_currentCharge += (short)rate;
            electricityGrid[num] = cell;
            return rate;
        }

        [ReplaceMethod]
        public int TryFetchElectricity(Vector3 pos, int rate, int max)
        {
            if (max == 0)
            {
                return 0;
            }
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            int num3 = num2 * GRID + num;
            Cell cell = electricityGrid[num3];
            if (cell.m_electrified)
            {
                rate = Mathf.Min(Mathf.Min(rate, max), 32768 + (int)cell.m_currentCharge);
                cell.m_currentCharge -= (short)rate;
            }
            else
            {
                rate = 0;
            }
            electricityGrid[num3] = cell;
            return rate;
        }

        [ReplaceMethod]
        public void CheckElectricity(Vector3 pos, out bool electricity)
        {
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            int num3 = num2 * GRID + num;
            electricity = electricityGrid[num3].m_electrified;
        }

        [ReplaceMethod]
        public bool CheckConductivity(Vector3 pos)
        {
            int num = Mathf.Clamp((int)(pos.x / 38.25f + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)(pos.z / 38.25f + HALFGRID), 0, GRID - 1);
            int num3 = num2 * GRID + num;
            int conductivity = (int)electricityGrid[num3].m_conductivity;
            if (conductivity >= 1)
            {
                if (conductivity < 64)
                {
                    bool flag = true;
                    bool flag2 = true;
                    if (num > 0 && electricityGrid[num3 - 1].m_conductivity >= 64)
                    {
                        flag = false;
                    }
                    if (num < GRID - 1 && electricityGrid[num3 + 1].m_conductivity >= 64)
                    {
                        flag = false;
                    }
                    if (num2 > 0 && electricityGrid[num3 - GRID].m_conductivity >= 64)
                    {
                        flag2 = false;
                    }
                    if (num2 < GRID - 1 && electricityGrid[num3 + GRID].m_conductivity >= 64)
                    {
                        flag2 = false;
                    }
                    if (flag || flag2)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private ushort GetRootGroup(ushort group)
        {
            for (ushort mergeIndex = m_pulseGroups[(int)group].m_mergeIndex; mergeIndex != 65535; mergeIndex = m_pulseGroups[(int)group].m_mergeIndex)
            {
                group = mergeIndex;
            }
            return group;
        }

        private void MergeGroups(ushort root, ushort merged)
        {
            PulseGroup pulseGroup = m_pulseGroups[(int)root];
            PulseGroup pulseGroup2 = m_pulseGroups[(int)merged];
            pulseGroup.m_origCharge += pulseGroup2.m_origCharge;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < m_pulseGroupCount; i++)
                {
                    if (m_pulseGroups[i].m_mergeIndex == merged)
                    {
                        m_pulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origCharge -= m_pulseGroups[i].m_origCharge;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curCharge += pulseGroup2.m_curCharge;
            pulseGroup2.m_curCharge = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            m_pulseGroups[(int)root] = pulseGroup;
            m_pulseGroups[(int)merged] = pulseGroup2;
        }

        private void ConductToCell(ref Cell cell, ushort group, int x, int z, int limit)
        {
            if ((int)cell.m_conductivity >= limit)
            {
                if (cell.m_conductivity < 64)
                {
                    bool flag = true;
                    bool flag2 = true;
                    int num = z * GRID + x;
                    if (x > 0 && electricityGrid[num - 1].m_conductivity >= 64)
                    {
                        flag = false;
                    }
                    if (x < GRID - 1 && electricityGrid[num + 1].m_conductivity >= 64)
                    {
                        flag = false;
                    }
                    if (z > 0 && electricityGrid[num - GRID].m_conductivity >= 64)
                    {
                        flag2 = false;
                    }
                    if (z < GRID - 1 && electricityGrid[num + GRID].m_conductivity >= 64)
                    {
                        flag2 = false;
                    }
                    if (flag || flag2)
                    {
                        return;
                    }
                }
                if (cell.m_pulseGroup == 65535)
                {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = 0;
                    pulseUnit.m_x = (ushort)x;
                    pulseUnit.m_z = (ushort)z;
                    m_pulseUnits[m_pulseUnitEnd] = pulseUnit;
                    if (++m_pulseUnitEnd == m_pulseUnits.Length)
                    {
                        m_pulseUnitEnd = 0;
                    }
                    cell.m_pulseGroup = group;
                    m_canContinue = true;
                }
                else
                {
                    ushort rootGroup = GetRootGroup(cell.m_pulseGroup);
                    if (rootGroup != group)
                    {
                        MergeGroups(group, rootGroup);
                        cell.m_pulseGroup = group;
                        m_canContinue = true;
                    }
                }
            }
        }

        private void ConductToCells(ushort group, float worldX, float worldZ)
        {
            int num = (int)(worldX / 38.25f + HALFGRID);
            int num2 = (int)(worldZ / 38.25f + HALFGRID);
            if (num >= 0 && num < GRID && num2 >= 0 && num2 < GRID)
            {
                int num3 = num2 * GRID + num;
                ConductToCell(ref electricityGrid[num3], group, num, num2, 1);
            }
        }

        private void ConductToNode(ushort nodeIndex, ref NetNode node, ushort group, float minX, float minZ, float maxX, float maxZ)
        {
            if (node.m_position.x >= minX && node.m_position.z >= minZ && node.m_position.x <= maxX && node.m_position.z <= maxZ)
            {
                NetInfo info = node.Info;
                if (info.m_class.m_service == ItemClass.Service.Electricity)
                {
                    if (m_nodeGroups[(int)nodeIndex] == 65535)
                    {
                        PulseUnit pulseUnit;
                        pulseUnit.m_group = group;
                        pulseUnit.m_node = nodeIndex;
                        pulseUnit.m_x = 0;
                        pulseUnit.m_z = 0;
                        m_pulseUnits[m_pulseUnitEnd] = pulseUnit;
                        if (++m_pulseUnitEnd == m_pulseUnits.Length)
                        {
                            m_pulseUnitEnd = 0;
                        }
                        m_nodeGroups[(int)nodeIndex] = group;
                        m_canContinue = true;
                    }
                    else
                    {
                        ushort rootGroup = GetRootGroup(m_nodeGroups[(int)nodeIndex]);
                        if (rootGroup != group)
                        {
                            MergeGroups(group, rootGroup);
                            m_nodeGroups[(int)nodeIndex] = group;
                            m_canContinue = true;
                        }
                    }
                }
            }
        }

        private void ConductToNodes(ushort group, int cellX, int cellZ)
        {
            float num = ((float)cellX - HALFGRID) * 38.25f;
            float num2 = ((float)cellZ - HALFGRID) * 38.25f;
            float num3 = num + 38.25f;
            float num4 = num2 + 38.25f;
            int num5 = Mathf.Max((int)(num / 64f + 135f), 0);
            int num6 = Mathf.Max((int)(num2 / 64f + 135f), 0);
            int num7 = Mathf.Min((int)(num3 / 64f + 135f), 269);
            int num8 = Mathf.Min((int)(num4 / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = num6; i <= num8; i++)
            {
                for (int j = num5; j <= num7; j++)
                {
                    ushort num9 = instance.m_nodeGrid[i * 270 + j];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        ConductToNode(num9, ref instance.m_nodes.m_buffer[(int)num9], group, num, num2, num3, num4);
                        num9 = instance.m_nodes.m_buffer[(int)num9].m_nextGridNode;
                        if (++num10 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateNodeElectricity(int nodeID, int value)
        {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            NetManager instance = Singleton<NetManager>.instance;
            bool flag = false;
            ushort building = instance.m_nodes.m_buffer[nodeID].m_building;
            if (building != 0)
            {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                if ((int)instance2.m_buildings.m_buffer[(int)building].m_electricityBuffer != value)
                {
                    instance2.m_buildings.m_buffer[(int)building].m_electricityBuffer = (ushort)value;
                    flag = (currentMode == InfoManager.InfoMode.Electricity);
                }
                if (flag)
                {
                    instance2.UpdateBuildingColors(building);
                }
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
                int num = (int)(currentFrameIndex & 255);
                if (num < 128)
                {
                    if (num == 0)
                    {
                        m_pulseGroupCount = 0;
                        m_pulseUnitStart = 0;
                        m_pulseUnitEnd = 0;
                        m_processedCells = 0;
                        m_conductiveCells = 0;
                        m_canContinue = true;
                    }
                    NetManager instance = Singleton<NetManager>.instance;
                    int num2 = num * 32768 >> 7;
                    int num3 = ((num + 1) * 32768 >> 7) - 1;
                    for (int i = num2; i <= num3; i++)
                    {
                        if (instance.m_nodes.m_buffer[i].m_flags != NetNode.Flags.None)
                        {
                            NetInfo info = instance.m_nodes.m_buffer[i].Info;
                            if (info.m_class.m_service == ItemClass.Service.Electricity)
                            {
                                UpdateNodeElectricity(i, (m_nodeGroups[i] == 65535) ? 0 : 1);
                                m_conductiveCells++;
                            }
                        }
                        m_nodeGroups[i] = 65535;
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
                                Cell cell = electricityGrid[num6];
                                if (cell.m_currentCharge > 0)
                                {
                                    if (m_pulseGroupCount < 1024)
                                    {
                                        PulseGroup pulseGroup;
                                        pulseGroup.m_origCharge = (uint)cell.m_currentCharge;
                                        pulseGroup.m_curCharge = (uint)cell.m_currentCharge;
                                        pulseGroup.m_mergeCount = 0;
                                        pulseGroup.m_mergeIndex = 65535;
                                        pulseGroup.m_x = (ushort)k;
                                        pulseGroup.m_z = (ushort)j;
                                        PulseUnit pulseUnit;
                                        pulseUnit.m_group = (ushort)m_pulseGroupCount;
                                        pulseUnit.m_node = 0;
                                        pulseUnit.m_x = (ushort)k;
                                        pulseUnit.m_z = (ushort)j;
                                        cell.m_pulseGroup = (ushort)m_pulseGroupCount;
                                        m_pulseGroups[m_pulseGroupCount++] = pulseGroup;
                                        m_pulseUnits[m_pulseUnitEnd] = pulseUnit;
                                        if (++m_pulseUnitEnd == m_pulseUnits.Length)
                                        {
                                            m_pulseUnitEnd = 0;
                                        }
                                    }
                                    else
                                    {
                                        cell.m_pulseGroup = 65535;
                                    }
                                    cell.m_currentCharge = 0;
                                    m_conductiveCells++;
                                }
                                else
                                {
                                    cell.m_pulseGroup = 65535;
                                    if (cell.m_conductivity >= 64)
                                    {
                                        m_conductiveCells++;
                                    }
                                }
                                if (cell.m_tmpElectrified != cell.m_electrified)
                                {
                                    cell.m_electrified = cell.m_tmpElectrified;
                                }
                                cell.m_tmpElectrified = (cell.m_pulseGroup != 65535);
                                electricityGrid[num6] = cell;
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
                        int pulseUnitEnd = m_pulseUnitEnd;
                        while (m_pulseUnitStart != pulseUnitEnd)
                        {
                            PulseUnit pulseUnit2 = m_pulseUnits[m_pulseUnitStart];
                            if (++m_pulseUnitStart == m_pulseUnits.Length)
                            {
                                m_pulseUnitStart = 0;
                            }
                            pulseUnit2.m_group = GetRootGroup(pulseUnit2.m_group);
                            uint num8 = m_pulseGroups[(int)pulseUnit2.m_group].m_curCharge;
                            if (pulseUnit2.m_node == 0)
                            {
                                int num9 = (int)pulseUnit2.m_z * GRID + (int)pulseUnit2.m_x;
                                Cell cell2 = electricityGrid[num9];
                                if (cell2.m_conductivity != 0 && !cell2.m_tmpElectrified && num8 != 0u)
                                {
                                    int num10 = Mathf.Clamp((int)(-(int)cell2.m_currentCharge), 0, (int)num8);
                                    num8 -= (uint)num10;
                                    cell2.m_currentCharge += (short)num10;
                                    if (cell2.m_currentCharge == 0)
                                    {
                                        cell2.m_tmpElectrified = true;
                                    }
                                    electricityGrid[num9] = cell2;
                                    m_pulseGroups[(int)pulseUnit2.m_group].m_curCharge = num8;
                                }
                                if (num8 != 0u)
                                {
                                    int limit = (cell2.m_conductivity < 64) ? 64 : 1;
                                    m_processedCells++;
                                    if (pulseUnit2.m_z > 0)
                                    {
                                        ConductToCell(ref electricityGrid[num9 - GRID], pulseUnit2.m_group, (int)pulseUnit2.m_x, (int)(pulseUnit2.m_z - 1), limit);
                                    }
                                    if (pulseUnit2.m_x > 0)
                                    {
                                        ConductToCell(ref electricityGrid[num9 - 1], pulseUnit2.m_group, (int)(pulseUnit2.m_x - 1), (int)pulseUnit2.m_z, limit);
                                    }
                                    if (pulseUnit2.m_z < GRID - 1)
                                    {
                                        ConductToCell(ref electricityGrid[num9 + GRID], pulseUnit2.m_group, (int)pulseUnit2.m_x, (int)(pulseUnit2.m_z + 1), limit);
                                    }
                                    if (pulseUnit2.m_x < GRID - 1)
                                    {
                                        ConductToCell(ref electricityGrid[num9 + 1], pulseUnit2.m_group, (int)(pulseUnit2.m_x + 1), (int)pulseUnit2.m_z, limit);
                                    }
                                    ConductToNodes(pulseUnit2.m_group, (int)pulseUnit2.m_x, (int)pulseUnit2.m_z);
                                }
                                else
                                {
                                    m_pulseUnits[m_pulseUnitEnd] = pulseUnit2;
                                    if (++m_pulseUnitEnd == m_pulseUnits.Length)
                                    {
                                        m_pulseUnitEnd = 0;
                                    }
                                }
                            }
                            else if (num8 != 0u)
                            {
                                m_processedCells++;
                                NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)pulseUnit2.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u))
                                {
                                    ConductToCells(pulseUnit2.m_group, netNode.m_position.x, netNode.m_position.z);
                                    for (int l = 0; l < 8; l++)
                                    {
                                        ushort segment = netNode.GetSegment(l);
                                        if (segment != 0)
                                        {
                                            ushort startNode = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_startNode;
                                            ushort endNode = Singleton<NetManager>.instance.m_segments.m_buffer[(int)segment].m_endNode;
                                            ushort num11 = (startNode != pulseUnit2.m_node) ? startNode : endNode;
                                            ConductToNode(num11, ref Singleton<NetManager>.instance.m_nodes.m_buffer[(int)num11], pulseUnit2.m_group, -100000f, -100000f, 100000f, 100000f);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                m_pulseUnits[m_pulseUnitEnd] = pulseUnit2;
                                if (++m_pulseUnitEnd == m_pulseUnits.Length)
                                {
                                    m_pulseUnitEnd = 0;
                                }
                            }
                        }
                    }
                    if (num == 255)
                    {
                        for (int m = 0; m < m_pulseGroupCount; m++)
                        {
                            PulseGroup pulseGroup2 = m_pulseGroups[m];
                            if (pulseGroup2.m_mergeIndex != 65535)
                            {
                                PulseGroup pulseGroup3 = m_pulseGroups[(int)pulseGroup2.m_mergeIndex];
                                pulseGroup2.m_curCharge = (uint)((ulong)pulseGroup3.m_curCharge * (ulong)pulseGroup2.m_origCharge / (ulong)pulseGroup3.m_origCharge);
                                pulseGroup3.m_curCharge -= pulseGroup2.m_curCharge;
                                pulseGroup3.m_origCharge -= pulseGroup2.m_origCharge;
                                m_pulseGroups[(int)pulseGroup2.m_mergeIndex] = pulseGroup3;
                                m_pulseGroups[m] = pulseGroup2;
                            }
                        }
                        for (int n = 0; n < m_pulseGroupCount; n++)
                        {
                            PulseGroup pulseGroup4 = m_pulseGroups[n];
                            if (pulseGroup4.m_curCharge != 0u)
                            {
                                int num12 = (int)pulseGroup4.m_z * GRID + (int)pulseGroup4.m_x;
                                Cell cell3 = electricityGrid[num12];
                                if (cell3.m_conductivity != 0)
                                {
                                    cell3.m_extraCharge += (ushort)Mathf.Min((int)pulseGroup4.m_curCharge, (int)(32767 - cell3.m_extraCharge));
                                }
                                electricityGrid[num12] = cell3;
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
                    electricityGrid[num5].m_conductivity = 0;
                    num5++;
                }
            }
            int num6 = Mathf.Max((int)((((float)num - HALFGRID) * 38.25f - 96f) / 64f + 135f), 0);
            int num7 = Mathf.Max((int)((((float)num2 - HALFGRID) * 38.25f - 96f) / 64f + 135f), 0);
            int num8 = Mathf.Min((int)((((float)num3 - HALFGRID + 1f) * 38.25f + 96f) / 64f + 135f), 269);
            int num9 = Mathf.Min((int)((((float)num4 - HALFGRID + 1f) * 38.25f + 96f) / 64f + 135f), 269);
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int k = num7; k <= num9; k++)
            {
                for (int l = num6; l <= num8; l++)
                {
                    ushort num10 = buildingGrid[k * 270 + l];
                    int num11 = 0;
                    while (num10 != 0)
                    {
                        Building.Flags flags = buildings.m_buffer[(int)num10].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                        {
                            BuildingInfo buildingInfo;
                            int num12;
                            int num13;
                            buildings.m_buffer[(int)num10].GetInfoWidthLength(out buildingInfo, out num12, out num13);
                            if (buildingInfo != null)
                            {
                                float num14 = buildingInfo.m_buildingAI.ElectricityGridRadius();
                                if (num14 > 0.1f)
                                {
                                    Vector3 position = buildings.m_buffer[(int)num10].m_position;
                                    float angle = buildings.m_buffer[(int)num10].m_angle;
                                    Vector3 vector = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                                    Vector3 vector2 = new Vector3(vector.z, 0f, -vector.x);
                                    Vector3 vector3 = position - (float)(num12 * 4) * vector - (float)(num13 * 4) * vector2;
                                    Vector3 vector4 = position + (float)(num12 * 4) * vector - (float)(num13 * 4) * vector2;
                                    Vector3 vector5 = position + (float)(num12 * 4) * vector + (float)(num13 * 4) * vector2;
                                    Vector3 vector6 = position - (float)(num12 * 4) * vector + (float)(num13 * 4) * vector2;
                                    minX = Mathf.Min(Mathf.Min(vector3.x, vector4.x), Mathf.Min(vector5.x, vector6.x)) - num14;
                                    maxX = Mathf.Max(Mathf.Max(vector3.x, vector4.x), Mathf.Max(vector5.x, vector6.x)) + num14;
                                    minZ = Mathf.Min(Mathf.Min(vector3.z, vector4.z), Mathf.Min(vector5.z, vector6.z)) - num14;
                                    maxZ = Mathf.Max(Mathf.Max(vector3.z, vector4.z), Mathf.Max(vector5.z, vector6.z)) + num14;
                                    int num15 = Mathf.Max(num, (int)(minX / 38.25f + HALFGRID));
                                    int num16 = Mathf.Min(num3, (int)(maxX / 38.25f + HALFGRID));
                                    int num17 = Mathf.Max(num2, (int)(minZ / 38.25f + HALFGRID));
                                    int num18 = Mathf.Min(num4, (int)(maxZ / 38.25f + HALFGRID));
                                    for (int m = num17; m <= num18; m++)
                                    {
                                        for (int n = num15; n <= num16; n++)
                                        {
                                            Vector3 a;
                                            a.x = ((float)n + 0.5f - HALFGRID) * 38.25f;
                                            a.y = position.y;
                                            a.z = ((float)m + 0.5f - HALFGRID) * 38.25f;
                                            float num19 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector, a - position)) - (float)(num12 * 4));
                                            float num20 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector2, a - position)) - (float)(num13 * 4));
                                            float num21 = Mathf.Sqrt(num19 * num19 + num20 * num20);
                                            if (num21 < num14 + 19.125f)
                                            {
                                                float num22 = (num14 - num21) * 0.0130718956f + 0.25f;
                                                int num23 = Mathf.Min(255, Mathf.RoundToInt(num22 * 255f));
                                                int num24 = m * GRID + n;
                                                if (num23 > (int)electricityGrid[num24].m_conductivity)
                                                {
                                                    electricityGrid[num24].m_conductivity = (byte)num23;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        num10 = buildings.m_buffer[(int)num10].m_nextGridBuilding;
                        if (++num11 >= BuildingManager.MAX_BUILDING_COUNT)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int num25 = num2; num25 <= num4; num25++)
            {
                int num26 = num25 * GRID + num;
                for (int num27 = num; num27 <= num3; num27++)
                {
                    Cell cell = electricityGrid[num26];
                    if (cell.m_conductivity == 0)
                    {
                        cell.m_currentCharge = 0;
                        cell.m_extraCharge = 0;
                        cell.m_pulseGroup = 65535;
                        cell.m_tmpElectrified = false;
                        cell.m_electrified = false;
                        electricityGrid[num26] = cell;
                    }
                    num26++;
                }
            }
            AreaModified(num, num2, num3, num4);
        }



        [ReplaceMethod]
        public void UpdateData(SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("ElectricityManager.UpdateData");

            if ((bool)m_refreshGrid.GetValue(ElectricityManager.instance))
            {
                UpdateGrid(-100000f, -100000f, 100000f, 100000f);
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

    }
}
