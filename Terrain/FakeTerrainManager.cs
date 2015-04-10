using ColossalFramework.Math;
using System;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Terrain
{
    public class FakeTerrainManager
    {
        public static void Init()
        {
            var tm = TerrainManager.instance;
            tm.m_detailHeights = new ushort[231361 * 81];
            tm.m_detailSurface = new TerrainManager.SurfaceCell[230400 * 81];
            tm.m_detailZones = new TerrainManager.ZoneCell[230400 * 81];
        }
    }
}
