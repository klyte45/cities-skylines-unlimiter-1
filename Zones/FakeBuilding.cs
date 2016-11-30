using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using EightyOne.Redirection;

namespace EightyOne.Zones
{

    [TargetType(typeof(Building))]
    internal struct FakeBuilding
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        [RedirectReverse(true)]
        private static void CheckZoning(ref Building b, ItemClass.Zone zone1, ItemClass.Zone zone2, ref uint validCells, ref bool secondary, ref ZoneBlock block)
        {
            UnityEngine.Debug.LogError("FakeBuilding - Failed to redirect reverse CheckZoning()");
        }

        [RedirectMethod]
        public static bool CheckZoning(ref Building b, ItemClass.Zone zone1, ItemClass.Zone zone2, bool allowCollapsed)
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
            //begin mod
            int num1 = Mathf.Max((int)(((double)vector3_5.x - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)vector3_5.z - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)vector3_6.x + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            int num4 = Mathf.Min((int)(((double)vector3_6.z + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
            //end mod
            bool secondary = false;
            uint validCells = 0;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    //begin mod
                    ushort num5 = instance.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                    //end mod
                    int num6 = 0;
                    while ((int)num5 != 0)
                    {
                        if (allowCollapsed || ((int)instance.m_blocks.m_buffer[(int)num5].m_flags & 4) == 0)
                        {
                            Vector3 vector3_7 = instance.m_blocks.m_buffer[(int)num5].m_position;
                            if ((double)Mathf.Max(Mathf.Max(vector3_5.x - 46f - vector3_7.x, vector3_5.z - 46f - vector3_7.z), Mathf.Max((float)((double)vector3_7.x - (double)vector3_6.x - 46.0), (float)((double)vector3_7.z - (double)vector3_6.z - 46.0))) < 0.0)
                                CheckZoning(ref b, zone1, zone2, ref validCells, ref secondary, ref instance.m_blocks.m_buffer[(int)num5]);
                        }
                        num5 = instance.m_blocks.m_buffer[(int)num5].m_nextGridBlock;
                        if (++num6 >= 49152)
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
            if (!secondary ? zone1 == ItemClass.Zone.CommercialHigh || zone1 == ItemClass.Zone.ResidentialHigh : zone2 == ItemClass.Zone.CommercialHigh || zone2 == ItemClass.Zone.ResidentialHigh)
                b.m_flags |= Building.Flags.HighDensity;
            else
                b.m_flags &= ~Building.Flags.HighDensity;
            return true;
        }


    }
}
