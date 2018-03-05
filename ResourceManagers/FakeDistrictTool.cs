using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using EightyOne.RedirectionFramework.Attributes;
using EightyOne.Terrain;
using UnityEngine;

namespace EightyOne.ResourceManagers
{
    [TargetType(typeof(DistrictTool))]
    class FakeDistrictTool : DistrictTool
    {
        private static FieldInfo m_lastPaintPosition;
        private static FieldInfo m_mousePosition;

        public static void Init()
        {
            m_lastPaintPosition = typeof(DistrictTool).GetField("m_lastPaintPosition", BindingFlags.NonPublic | BindingFlags.Instance);
            m_mousePosition = typeof(DistrictTool).GetField("m_mousePosition", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [RedirectReverse(true)]
        private static int GetAlpha(ref DistrictManager.Cell cell, byte district)
        {
            UnityEngine.Debug.Log($"{cell}-{district}");
            return 0;
        }

        [RedirectReverse(true)]
        private static void Normalize(ref DistrictManager.Cell cell, int ignoreIndex)
        {
            UnityEngine.Debug.Log($"{cell}-{ignoreIndex}");
        }

        [RedirectMethod]
        public static void ApplyBrush(DistrictTool.Layer layer, byte district, float brushRadius, Vector3 startPosition, Vector3 endPosition)
        {
            GameAreaManager instance = Singleton<GameAreaManager>.instance;
            //begin mod
            DistrictManager.Cell[] cellArray = layer != DistrictTool.Layer.Districts ? FakeDistrictManager.parkGrid : FakeDistrictManager.districtGrid;
            //end mod
            float num1 = 19.2f;
            //begin mod
            int num2 = FakeDistrictManager.GRID;
            //end mod
            if ((double)startPosition.x < -50000.0)
                startPosition = endPosition;
            startPosition.y = 0.0f;
            endPosition.y = 0.0f;
            Vector3 vector3_1 = Vector3.Min(startPosition, endPosition);
            Vector3 vector3_2 = Vector3.Max(startPosition, endPosition);
            int num3 = Mathf.Max((int)(((double)vector3_1.x - (double)brushRadius) / (double)num1 + (double)num2 * 0.5), 0);
            int num4 = Mathf.Max((int)(((double)vector3_1.z - (double)brushRadius) / (double)num1 + (double)num2 * 0.5), 0);
            int num5 = Mathf.Min((int)(((double)vector3_2.x + (double)brushRadius) / (double)num1 + (double)num2 * 0.5), num2 - 1);
            int num6 = Mathf.Min((int)(((double)vector3_2.z + (double)brushRadius) / (double)num1 + (double)num2 * 0.5), num2 - 1);
            int num7 = num2;
            int num8 = -1;
            int num9 = num2;
            int num10 = -1;
            for (int index1 = num4; index1 <= num6; ++index1)
            {
                for (int index2 = num3; index2 <= num5; ++index2)
                {
                    Vector3 vector3_3 = new Vector3((float)((double)index2 - (double)num2 * 0.5 + 0.5) * num1, 0.0f, (float)((double)index1 - (double)num2 * 0.5 + 0.5) * num1);
                    Vector3 position = vector3_3;
                    if (instance.ClampPoint(ref position))
                    {
                        float a = (float)((double)Mathf.Sqrt(Segment3.DistanceSqr(startPosition, endPosition, vector3_3)) - (double)brushRadius + (double)num1 * 2.0);
                        float b = Vector3.Distance(position, vector3_3);
                        float num11 = Mathf.Clamp01((float)(-(double)(Mathf.Max(a, b) - num1 * 2f) / ((double)num1 * 2.0)));
                        if ((double)num11 != 0.0)
                        {
                            int min = Mathf.Clamp((int)(256.0 * (double)num11), 0, (int)byte.MaxValue);
                            if ((double)b <= 1.0 || (int)district == 0 ? SetDistrictAlpha(index2, index1, layer, district, min, (int)byte.MaxValue) : ForceDistrictAlpha(index2, index1, layer, district, min, (int)byte.MaxValue))
                            {
                                num7 = Mathf.Min(num7, index2);
                                num8 = Mathf.Max(num8, index2);
                                num9 = Mathf.Min(num9, index1);
                                num10 = Mathf.Max(num10, index1);
                            }
                        }
                    }
                }
            }
            int num12 = num7;
            int num13 = num8;
            int num14 = num9;
            int num15 = num10;
            int num16 = 0;
            bool flag1;
            do
            {
                int num11 = Mathf.Max(num7 - 1, 0);
                int num17 = Mathf.Min(num8 + 1, num2 - 1);
                int num18 = Mathf.Max(num9 - 1, 0);
                int num19 = Mathf.Min(num10 + 1, num2 - 1);
                num7 = num2;
                num8 = -1;
                num9 = num2;
                num10 = -1;
                flag1 = false;
                for (int index1 = num18; index1 <= num19; ++index1)
                {
                    for (int index2 = num11; index2 <= num17; ++index2)
                    {
                        DistrictManager.Cell cell = cellArray[index1 * num2 + index2];
                        bool flag2 = false;
                        bool flag3 = false;
                        if ((int)cell.m_alpha1 != 0)
                        {
                            bool flag4 = (int)cell.m_district1 == (int)district;
                            int min;
                            int max;
                            CheckNeighbourCells(index2, index1, layer, cell.m_district1, out min, out max);
                            if (!flag4 && Mathf.Min((int)cell.m_alpha1 + 120, (int)byte.MaxValue) > max)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district1, 0, Mathf.Max(0, max - 120));
                            else if (flag4 && Mathf.Max((int)cell.m_alpha1 - 120, 0) < min)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district1, Mathf.Min((int)byte.MaxValue, min + 120), (int)byte.MaxValue);
                            if (flag4)
                                flag2 = true;
                        }
                        if ((int)cell.m_alpha2 != 0)
                        {
                            bool flag4 = (int)cell.m_district2 == (int)district;
                            int min;
                            int max;
                            CheckNeighbourCells(index2, index1, layer, cell.m_district2, out min, out max);
                            if (!flag4 && Mathf.Min((int)cell.m_alpha2 + 120, (int)byte.MaxValue) > max)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district2, 0, Mathf.Max(0, max - 120));
                            else if (flag4 && Mathf.Max((int)cell.m_alpha2 - 120, 0) < min)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district2, Mathf.Min((int)byte.MaxValue, min + 120), (int)byte.MaxValue);
                            if (flag4)
                                flag2 = true;
                        }
                        if ((int)cell.m_alpha3 != 0)
                        {
                            bool flag4 = (int)cell.m_district3 == (int)district;
                            int min;
                            int max;
                            CheckNeighbourCells(index2, index1, layer, cell.m_district3, out min, out max);
                            if (!flag4 && Mathf.Min((int)cell.m_alpha3 + 120, (int)byte.MaxValue) > max)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district3, 0, Mathf.Max(0, max - 120));
                            else if (flag4 && Mathf.Max((int)cell.m_alpha3 - 120, 0) < min)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district3, Mathf.Min((int)byte.MaxValue, min + 120), (int)byte.MaxValue);
                            if (flag4)
                                flag2 = true;
                        }
                        if ((int)cell.m_alpha4 != 0)
                        {
                            bool flag4 = (int)cell.m_district4 == (int)district;
                            int min;
                            int max;
                            CheckNeighbourCells(index2, index1, layer, cell.m_district4, out min, out max);
                            if (!flag4 && Mathf.Min((int)cell.m_alpha4 + 120, (int)byte.MaxValue) > max)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district4, 0, Mathf.Max(0, max - 120));
                            else if (flag4 && Mathf.Max((int)cell.m_alpha4 - 120, 0) < min)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, cell.m_district4, Mathf.Min((int)byte.MaxValue, min + 120), (int)byte.MaxValue);
                            if (flag4)
                                flag2 = true;
                        }
                        if (!flag2)
                        {
                            int min;
                            int max;
                            CheckNeighbourCells(index2, index1, layer, district, out min, out max);
                            if (0 < min)
                                flag3 = flag3 || SetDistrictAlpha(index2, index1, layer, district, Mathf.Min((int)byte.MaxValue, min + 120), (int)byte.MaxValue);
                        }
                        if (flag3)
                        {
                            num7 = Mathf.Min(num7, index2);
                            num8 = Mathf.Max(num8, index2);
                            num9 = Mathf.Min(num9, index1);
                            num10 = Mathf.Max(num10, index1);
                            flag1 = true;
                        }
                    }
                }
                num12 = Mathf.Min(num12, num7);
                num13 = Mathf.Max(num13, num8);
                num14 = Mathf.Min(num14, num9);
                num15 = Mathf.Max(num15, num10);
            }
            while (++num16 < 10 && flag1);
            if (layer == DistrictTool.Layer.Districts)
            {
                Singleton<DistrictManager>.instance.AreaModified(num12, num14, num13, num15, true);
                Singleton<DistrictManager>.instance.m_districtsNotUsed.Disable();
            }
            else
                Singleton<DistrictManager>.instance.ParksAreaModified(num12, num14, num13, num15, true);
        }

        [RedirectMethod]
        private static bool ForceDistrictAlpha(int x, int z, DistrictTool.Layer layer, byte district, int min, int max)
        {
            //begin mod
            int num1 = FakeDistrictManager.GRID;
            DistrictManager.Cell cell1 = (layer != DistrictTool.Layer.Districts ? FakeDistrictManager.parkGrid : FakeDistrictManager.districtGrid)[z * num1 + x];
            //end mod
            int num2 = Mathf.Clamp(GetAlpha(ref cell1, district), min, max);
            DistrictManager.Cell cell2 = new DistrictManager.Cell();
            cell2.m_district1 = district;
            cell2.m_district2 = (byte)0;
            cell2.m_alpha1 = (byte)num2;
            cell2.m_alpha2 = (byte)((int)byte.MaxValue - num2);
            if ((int)cell2.m_alpha1 == (int)cell1.m_alpha1 && (int)cell2.m_alpha2 == (int)cell1.m_alpha2 && ((int)cell2.m_alpha3 == (int)cell1.m_alpha3 && (int)cell2.m_alpha4 == (int)cell1.m_alpha4) && ((int)cell2.m_district1 == (int)cell1.m_district1 && (int)cell2.m_district2 == (int)cell1.m_district2 && ((int)cell2.m_district3 == (int)cell1.m_district3 && (int)cell2.m_district4 == (int)cell1.m_district4)))
                return false;
            if (layer == DistrictTool.Layer.Districts)
                Singleton<DistrictManager>.instance.ModifyCell(x, z, cell2);
            else
                Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell2);
            return true;
        }

        [RedirectMethod]
        private static bool SetDistrictAlpha(int x, int z, DistrictTool.Layer layer, byte district, int min, int max)
        {
            //begin mod
            int num1 = FakeDistrictManager.GRID;
            DistrictManager.Cell cell = (layer != DistrictTool.Layer.Districts ? FakeDistrictManager.parkGrid : FakeDistrictManager.districtGrid)[z * num1 + x];
            //end mod
            if ((int)cell.m_district1 == (int)district)
            {
                int num2 = Mathf.Clamp((int)cell.m_alpha1, min, max);
                if (num2 != (int)cell.m_alpha1)
                {
                    cell.m_alpha1 = (byte)num2;
                    Normalize(ref cell, 1);
                    if (layer == DistrictTool.Layer.Districts)
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    else
                        Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                    return true;
                }
            }
            else if ((int)cell.m_district2 == (int)district)
            {
                int num2 = Mathf.Clamp((int)cell.m_alpha2, min, max);
                if (num2 != (int)cell.m_alpha2)
                {
                    cell.m_alpha2 = (byte)num2;
                    Normalize(ref cell, 2);
                    if (layer == DistrictTool.Layer.Districts)
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    else
                        Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                    return true;
                }
            }
            else if ((int)cell.m_district3 == (int)district)
            {
                int num2 = Mathf.Clamp((int)cell.m_alpha3, min, max);
                if (num2 != (int)cell.m_alpha3)
                {
                    cell.m_alpha3 = (byte)num2;
                    Normalize(ref cell, 3);
                    if (layer == DistrictTool.Layer.Districts)
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    else
                        Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                    return true;
                }
            }
            else if ((int)cell.m_district4 == (int)district)
            {
                int num2 = Mathf.Clamp((int)cell.m_alpha4, min, max);
                if (num2 != (int)cell.m_alpha4)
                {
                    cell.m_alpha4 = (byte)num2;
                    Normalize(ref cell, 4);
                    if (layer == DistrictTool.Layer.Districts)
                        Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                    else
                        Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                    return true;
                }
            }
            else if (min > 0)
            {
                int num2 = 256;
                int num3 = -1;
                if ((int)cell.m_alpha1 < num2)
                {
                    num2 = (int)cell.m_alpha1;
                    num3 = 1;
                }
                if ((int)cell.m_alpha2 < num2)
                {
                    num2 = (int)cell.m_alpha2;
                    num3 = 2;
                }
                if ((int)cell.m_alpha3 < num2)
                {
                    num2 = (int)cell.m_alpha3;
                    num3 = 3;
                }
                if ((int)cell.m_alpha4 < num2)
                {
                    num2 = (int)cell.m_alpha4;
                    num3 = 4;
                }
                if (num2 <= min)
                {
                    if (num3 == 1)
                    {
                        cell.m_district1 = district;
                        cell.m_alpha1 = (byte)min;
                        Normalize(ref cell, 1);
                        if (layer == DistrictTool.Layer.Districts)
                            Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        else
                            Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                        return true;
                    }
                    if (num3 == 2)
                    {
                        cell.m_district2 = district;
                        cell.m_alpha2 = (byte)min;
                        Normalize(ref cell, 2);
                        if (layer == DistrictTool.Layer.Districts)
                            Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        else
                            Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                        return true;
                    }
                    if (num3 == 3)
                    {
                        cell.m_district3 = district;
                        cell.m_alpha3 = (byte)min;
                        Normalize(ref cell, 3);
                        if (layer == DistrictTool.Layer.Districts)
                            Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        else
                            Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                        return true;
                    }
                    if (num3 == 4)
                    {
                        cell.m_district4 = district;
                        cell.m_alpha4 = (byte)min;
                        Normalize(ref cell, 4);
                        if (layer == DistrictTool.Layer.Districts)
                            Singleton<DistrictManager>.instance.ModifyCell(x, z, cell);
                        else
                            Singleton<DistrictManager>.instance.ModifyParkCell(x, z, cell);
                        return true;
                    }
                }
            }
            return false;
        }

        [RedirectMethod]
        private static void CheckNeighbourCells(int x, int z, DistrictTool.Layer layer, byte district, out int min, out int max)
        {
            min = (int)byte.MaxValue;
            max = 0;
            //begin mod
            int num = FakeDistrictManager.GRID;
            DistrictManager.Cell[] cellArray = layer != DistrictTool.Layer.Districts ? FakeDistrictManager.parkGrid : FakeDistrictManager.districtGrid;
            //end mod
            if (z > 0)
            {
                if (x > 0)
                {
                    DistrictManager.Cell cell = cellArray[(z - 1) * num + x - 1];
                    int alpha = GetAlpha(ref cell, district);
                    min = Mathf.Min(min, alpha);
                    max = Mathf.Max(max, alpha);
                }
                DistrictManager.Cell cell1 = cellArray[(z - 1) * num + x];
                int alpha1 = GetAlpha(ref cell1, district);
                min = Mathf.Min(min, alpha1);
                max = Mathf.Max(max, alpha1);
                if (x < num - 1)
                {
                    DistrictManager.Cell cell2 = cellArray[(z - 1) * num + x + 1];
                    int alpha2 = GetAlpha(ref cell2, district);
                    min = Mathf.Min(min, alpha2);
                    max = Mathf.Max(max, alpha2);
                }
            }
            if (x > 0)
            {
                DistrictManager.Cell cell = cellArray[z * num + x - 1];
                int alpha = GetAlpha(ref cell, district);
                min = Mathf.Min(min, alpha);
                max = Mathf.Max(max, alpha);
            }
            if (x < num - 1)
            {
                DistrictManager.Cell cell = cellArray[z * num + x + 1];
                int alpha = GetAlpha(ref cell, district);
                min = Mathf.Min(min, alpha);
                max = Mathf.Max(max, alpha);
            }
            if (z >= num - 1)
                return;
            if (x > 0)
            {
                DistrictManager.Cell cell = cellArray[(z + 1) * num + x - 1];
                int alpha =GetAlpha(ref cell, district);
                min = Mathf.Min(min, alpha);
                max = Mathf.Max(max, alpha);
            }
            DistrictManager.Cell cell3 = cellArray[(z + 1) * num + x];
            int alpha3 = GetAlpha(ref cell3, district);
            min = Mathf.Min(min, alpha3);
            max = Mathf.Max(max, alpha3);
            if (x >= num - 1)
                return;
            DistrictManager.Cell cell4 = cellArray[(z + 1) * num + x + 1];
            int alpha4 = GetAlpha(ref cell4, district);
            min = Mathf.Min(min, alpha4);
            max = Mathf.Max(max, alpha4);
        }
    }
}
