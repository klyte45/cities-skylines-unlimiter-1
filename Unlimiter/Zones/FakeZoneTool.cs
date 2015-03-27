using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter.Zones
{
#if false
    internal class FakeZoneTool
    {
        private static void ApplyBrush(ZoneTool z)
        {
            float brushRadius = z.m_brushSize * 0.5f;
            Vector3 position = (Vector3)z.GetType().GetField("m_mousePosition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(z);
            float num1 = position.x - brushRadius;
            float num2 = position.z - brushRadius;
            float num3 = position.x + brushRadius;
            float num4 = position.z + brushRadius;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            int num5 = Mathf.Max((int)(((double)num1 - 46.0) / 64.0 + 75.0), 0);
            int num6 = Mathf.Max((int)(((double)num2 - 46.0) / 64.0 + 75.0), 0);
            int num7 = Mathf.Min((int)(((double)num3 + 46.0) / 64.0 + 75.0), 149);
            int num8 = Mathf.Min((int)(((double)num4 + 46.0) / 64.0 + 75.0), 149);
            for (int index1 = num6; index1 <= num8; ++index1)
            {
                for (int index2 = num5; index2 <= num7; ++index2)
                {
                    ushort blockIndex = instance.m_zoneGrid[index1 * 150 + index2];
                    int num9 = 0;
                    while ((int)blockIndex != 0)
                    {
                        Vector3 vector3 = instance.m_blocks.m_buffer[(int)blockIndex].m_position;
                        if ((double)Mathf.Max(Mathf.Max(num1 - 46f - vector3.x, num2 - 46f - vector3.z), Mathf.Max((float)((double)vector3.x - (double)num3 - 46.0), (float)((double)vector3.z - (double)num4 - 46.0))) < 0.0)
                            ApplyBrush(z, blockIndex, ref instance.m_blocks.m_buffer[(int)blockIndex], position, brushRadius);
                        blockIndex = instance.m_blocks.m_buffer[(int)blockIndex].m_nextGridBlock;
                        if (++num9 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        private static void ApplyBrush(ZoneTool z, ushort blockIndex, ref ZoneBlock data, Vector3 position, float brushRadius)
        {
            
  private bool m_zoning;
  private bool m_dezoning;

            Vector3 vector3_1 = data.m_position - position;
            if ((double)Mathf.Abs(vector3_1.x) > 46.0 + (double)brushRadius || (double)Mathf.Abs(vector3_1.z) > 46.0 + (double)brushRadius)
                return;
            int num = (int)((data.m_flags & 65280U) >> 8);
            Vector3 vector3_2 = new Vector3(Mathf.Cos(data.m_angle), 0.0f, Mathf.Sin(data.m_angle)) * 8f;
            Vector3 vector3_3 = new Vector3(vector3_2.z, 0.0f, -vector3_2.x);
            bool flag = false;
            for (int z = 0; z < num; ++z)
            {
                Vector3 vector3_4 = ((float)z - 3.5f) * vector3_3;
                for (int x = 0; x < 4; ++x)
                {
                    Vector3 vector3_5 = ((float)x - 3.5f) * vector3_2;
                    Vector3 vector3_6 = vector3_1 + vector3_5 + vector3_4;
                    if ((double)vector3_6.x * (double)vector3_6.x + (double)vector3_6.z * (double)vector3_6.z <= (double)brushRadius * (double)brushRadius)
                    {
                        if (z.m_zoning)
                        {
                            if ((z.m_zone == ItemClass.Zone.Unzoned || data.GetZone(x, z) == ItemClass.Zone.Unzoned) && data.SetZone(x, z, z.m_zone))
                                flag = true;
                        }
                        else if (z.m_dezoning && data.SetZone(x, z, ItemClass.Zone.Unzoned))
                            flag = true;
                    }
                }
            }
            if (!flag)
                return;
            data.RefreshZoning(blockIndex);
            if (!z.m_zoning)
                return;
            z.GetType().GetMethod("UsedZone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(z, new object[] { z.m_zone });
        }
    }
#endif
}
