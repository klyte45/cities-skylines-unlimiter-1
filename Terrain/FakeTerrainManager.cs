using ColossalFramework.Math;
using System;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Terrain
{
    [TargetType(typeof(TerrainManager))]
    public class FakeTerrainManager
    {
        public static void Init()
        {
        //    var tm = TerrainManager.instance;
        //    tm.m_detailHeights = new ushort[231361 * 81];
        //    tm.m_detailSurface = new TerrainManager.SurfaceCell[230400 * 81];
        //    tm.m_detailZones = new TerrainManager.ZoneCell[230400 * 81];
        }

        [ReplaceMethod]
        public float GetTileFlatness(int x, int z)
        {
            TerrainPatch terrainPatch = TerrainManager.instance.m_patches[z * 9 + x];
            return terrainPatch.m_flatness;
        }

        [ReplaceMethod]
        public TerrainManager.SurfaceCell GetSurfaceCell(int x, int z)
        {
            var tm = TerrainManager.instance;
            int num = Mathf.Clamp(x / 480,0,8);
            int num2 = Mathf.Clamp(z / 480,0, 8);
            int num3 = num2 * 9 + num;
            int simDetailIndex = tm.m_patches[num3].m_simDetailIndex;
            if (simDetailIndex == 0)
            {
                return tm.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
            }
            int num4 = (simDetailIndex - 1) * 480 * 480;
            int num5 = x - num * 480;
            int num6 = z - num2 * 480;

            var maxLimit = tm.m_detailSurface.Length - 1;
            if ((num5 == 0 && num != 0 && tm.m_patches[num3 - 1].m_simDetailIndex == 0) || (num6 == 0 && num2 != 0 && tm.m_patches[num3 - 9].m_simDetailIndex == 0))
            {
                TerrainManager.SurfaceCell result = tm.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
                result.m_clipped = tm.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5,0,maxLimit)].m_clipped;
                return result;
            }
            if ((num5 == 479 && num != 8 && tm.m_patches[num3 + 1].m_simDetailIndex == 0) || (num6 == 479 && num2 != 8 && tm.m_patches[num3 + 9].m_simDetailIndex == 0))
            {
                TerrainManager.SurfaceCell result2 = tm.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
                result2.m_clipped = tm.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5,0,maxLimit)].m_clipped;
                return result2;
            }
            return tm.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5, 0, maxLimit)];
        }
    }
}
