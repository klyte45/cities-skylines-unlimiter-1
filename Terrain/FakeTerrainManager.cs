using EightyOne.Areas;
using UnityEngine;
using EightyOne.Redirection;

namespace EightyOne.Terrain
{

    [TargetType(typeof(TerrainManager))]
    public class FakeTerrainManager : TerrainManager
    {
        public static void Init()
        {
        }

        [RedirectMethod]
        public float GetUnlockableTerrainFlatness()
        {
            float num1 = 0.0f;
            //begin mod
            for (int index1 = 0; index1 < FakeGameAreaManager.GRID; ++index1)
            {
                for (int index2 = 0; index2 < FakeGameAreaManager.GRID; ++index2)
                    num1 += this.GetTileFlatness(index2, index1);
            }
            return num1 / (FakeGameAreaManager.GRID * FakeGameAreaManager.GRID);
            //end mod
        }

        [RedirectMethod]
        public float GetTileFlatness(int x, int z)
        {
            //begin mod
            return this.m_patches[z * 9 + x].m_flatness;
            //end mod
        }

        [RedirectMethod]
        [IgnoreIfSurfacePainterEnabled]
        public TerrainManager.SurfaceCell GetSurfaceCell(int x, int z)
        {
            //begin mod
            int num1 = Mathf.Clamp(x / 480, 0, 8);
            int num2 = Mathf.Clamp(z / 480, 0, 8);
            //end mod
            int index = num2 * 9 + num1;
            int num3 = this.m_patches[index].m_simDetailIndex;
            if (num3 == 0)
                return this.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
            int num4 = (num3 - 1) * 480 * 480;
            int num5 = x - num1 * 480;
            int num6 = z - num2 * 480;
            //begin mod
            var maxLimit = this.m_detailSurface.Length - 1;
            //end mod
            if (num5 == 0 && num1 != 0 && this.m_patches[index - 1].m_simDetailIndex == 0 || num6 == 0 && num2 != 0 && this.m_patches[index - 9].m_simDetailIndex == 0)
            {
                TerrainManager.SurfaceCell surfaceCell = this.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
                //begin mod
                surfaceCell.m_clipped = this.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5, 0, maxLimit)].m_clipped;
                //end mod
                return surfaceCell;
            }
            if ((num5 != 479 || num1 == 8 || this.m_patches[index + 1].m_simDetailIndex != 0) && (num6 != 479 || num2 == 8 || this.m_patches[index + 9].m_simDetailIndex != 0))
                //begin mod
                return this.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5, 0, maxLimit)];
                //end mod
            TerrainManager.SurfaceCell surfaceCell1 = this.SampleRawSurface((float)x * 0.25f, (float)z * 0.25f);
            //begin mod
            surfaceCell1.m_clipped = this.m_detailSurface[Mathf.Clamp(num4 + num6 * 480 + num5, 0, maxLimit)].m_clipped;
            //end mod
            return surfaceCell1;
        }
    }
}
