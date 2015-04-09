//using System;
//using UnityEngine;
//using Unlimiter.Attributes;

//namespace Unlimiter.Terrain
//{
//    internal class FakeTerrainManager
//    {
//        [ReplaceMethod]
//        public static TerrainManager.ZoneCell GetZoneCell(TerrainManager t, int x, int z)
//        {
//            try
//            {
//                int num1 = Mathf.Min(x / 480, 8);
//                int num2 = Mathf.Min(z / 480, 8);

//                int num3 = t.m_patches[num2 * 9 + num1].m_simDetailIndex;
//                if (num3 != 0)
//                {
//                    int num4 = x - num1 * 480;
//                    int num5 = z - num2 * 480;
//                    return t.m_detailZones[(num3 - 1) * 480 * 480 + num5 * 480 + num4];
//                }
//            }
//            catch (IndexOutOfRangeException)
//            {
//            }

//            return new TerrainManager.ZoneCell()
//            {
//                m_zone = (byte)15
//            };
//        }

//        [ReplaceMethod]
//        public static TerrainManager.SurfaceCell GetSurfaceCell(TerrainManager t, int x, int z)
//        {
//            try
//            {
//                int num1 = Mathf.Min(x / 480, 8);
//                int num2 = Mathf.Min(z / 480, 8);
//                int index = num2 * 9 + num1;
//                int num3 = t.m_patches[index].m_simDetailIndex;
//                if (num3 == 0)
//                    return t.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
//                int num4 = (num3 - 1) * 480 * 480;
//                int num5 = x - num1 * 480;
//                int num6 = z - num2 * 480;
//                if (num5 == 0 && num1 != 0 && t.m_patches[index - 1].m_simDetailIndex == 0 || num6 == 0 && num2 != 0 && t.m_patches[index - 9].m_simDetailIndex == 0)
//                {
//                    TerrainManager.SurfaceCell surfaceCell = t.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
//                    surfaceCell.m_clipped = t.m_detailSurface[num4 + num6 * 480 + num5].m_clipped;
//                    return surfaceCell;
//                }
//                if ((num5 != 479 || num1 == 8 || t.m_patches[index + 1].m_simDetailIndex != 0) && (num6 != 479 || num2 == 8 || t.m_patches[index + 9].m_simDetailIndex != 0))
//                    return t.m_detailSurface[num4 + num6 * 480 + num5];
//                TerrainManager.SurfaceCell surfaceCell1 = t.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
//                surfaceCell1.m_clipped = t.m_detailSurface[num4 + num6 * 480 + num5].m_clipped;
//                return surfaceCell1;
//            }
//            catch (IndexOutOfRangeException)
//            {
//                Debug.LogFormat("Trying to get surface cell {0}, {1}", x, z);
//                TerrainManager.SurfaceCell cell = new TerrainManager.SurfaceCell();
//                cell.m_clipped = 1;
//                return cell;
//            }
//        }
//    }
//}
