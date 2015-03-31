using System;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Terrain
{
    internal class FakeTerrainManager
    {
        [ReplaceMethod]
        public static TerrainManager.ZoneCell GetZoneCell(TerrainManager t, int x, int z)
        {
            try
            {
                int num1 = Mathf.Min(x / 480, 8);
                int num2 = Mathf.Min(z / 480, 8);

                int num3 = t.m_patches[num2 * 9 + num1].m_simDetailIndex;
                if (num3 != 0)
                {
                    int num4 = x - num1 * 480;
                    int num5 = z - num2 * 480;
                    return t.m_detailZones[(num3 - 1) * 480 * 480 + num5 * 480 + num4];
                }
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogWarningFormat("Trying to Get Zone Cell {0}, {1}", x, z);
            }

            return new TerrainManager.ZoneCell()
            {
                m_zone = (byte)15
            };
        }
    }
}
