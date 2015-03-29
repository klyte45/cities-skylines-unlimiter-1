using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unlimiter.Areas;

namespace Unlimiter.Zones
{
    /// <summary>
    /// Building is a struct. Calls to this may not work with Replacing the method handle.
    /// </summary>
    class FakeBuilding
    {
        internal static bool CheckZoning(Building b, ItemClass.Zone zone)
        {
            int width = b.Width;
            int length = b.Length;
            Vector3 vector3_1 = new Vector3(Mathf.Cos(b.m_angle), 0.0f, Mathf.Sin(b.m_angle));
            Vector3 vector3_2 = new Vector3(vector3_1.z, 0.0f, -vector3_1.x);
            Vector3 vector3_3 = vector3_1 * ((float)width * 4f);
            Vector3 vector3_4 = vector3_2 * ((float)length * 4f);
            Quad3 quad3 = new Quad3();
            quad3.a = b.m_position - vector3_3 - vector3_4;
            quad3.b = b.m_position + vector3_3 - vector3_4;
            quad3.c = b.m_position + vector3_3 + vector3_4;
            quad3.d = b.m_position - vector3_3 + vector3_4;
            Vector3 vector3_5 = quad3.Min();
            Vector3 vector3_6 = quad3.Max();
            int num1 = Mathf.Max((int)(((double)vector3_5.x - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
            int num2 = Mathf.Max((int)(((double)vector3_5.z - 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), 0);
            int num3 = Mathf.Min((int)(((double)vector3_6.x + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)(((double)vector3_6.z + 46.0) / FakeZoneManager.ZONEGRID_CELL_SIZE + FakeZoneManager.UNKNOWN_FLOAT_75), FakeZoneManager.ZONEGRID_RESOLUTION - 1);
            uint validCells = 0U;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort num5 = instance.m_zoneGrid[index1 * FakeZoneManager.ZONEGRID_RESOLUTION + index2];
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        Vector3 vector3_7 = instance.m_blocks.m_buffer[(int)num5].m_position;
                        if ((double)Mathf.Max(Mathf.Max(vector3_5.x - 46f - vector3_7.x, vector3_5.z - 46f - vector3_7.z), Mathf.Max((float)((double)vector3_7.x - (double)vector3_6.x - 46.0), (float)((double)vector3_7.z - (double)vector3_6.z - 46.0))) < 0.0)
                            CallCheckZoning(b, zone, ref validCells, ref instance.m_blocks.m_buffer[num5]);
                        num5 = instance.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int index1 = 0; index1 < length; ++index1)
            {
                for (int index2 = 0; index2 < width; ++index2)
                {
                    if (((int)validCells & 1 << (index1 << 3) + index2) == 0)
                        return false;
                }
            }
            return true;
        }

        private static void CallCheckZoning(Building b, ItemClass.Zone zone, ref uint validCells, ref ZoneBlock block)
        {
            var p = new object[] { zone, validCells, block };
            b.GetType().GetMethod("CheckZoning", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new Type[]{typeof(ItemClass.Zone), typeof(uint).MakeByRefType(), typeof(ZoneBlock).MakeByRefType()}, null).Invoke(b, p);
            validCells = (uint)p[1];
            block = (ZoneBlock)p[2];
        }
    }
}
