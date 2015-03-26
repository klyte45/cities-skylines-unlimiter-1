using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter.Trees
{
    internal static class LimitNaturalResourceManager
    {
        // FIXME m_naturalResources?
        private static void TreesModified(NaturalResourceManager nrm, Vector3 position)
        {
            int num1 = Mathf.Clamp((int)((double)position.x / 33.75 + 256.0), 0, 511);
            int num2 = Mathf.Clamp((int)((double)position.z / 33.75 + 256.0), 0, 511);
            float num3 = (float)(((double)num1 - 256.0) * 33.75);
            float num4 = (float)(((double)num2 - 256.0) * 33.75);
            float num5 = (float)(((double)(num1 + 1) - 256.0) * 33.75);
            float num6 = (float)(((double)(num2 + 1) - 256.0) * 33.75);
            int num7 = Mathf.Max((int)((double)num3 / 32.0 + 270.0), 0);
            int num8 = Mathf.Max((int)((double)num4 / 32.0 + 270.0), 0);
            int num9 = Mathf.Min((int)((double)num5 / 32.0 + 270.0), 539);
            int num10 = Mathf.Min((int)((double)num6 / 32.0 + 270.0), 539);
            TreeManager instance = Singleton<TreeManager>.instance;
            int num11 = 0;
            int num12 = 0;
            for (int index1 = num8; index1 <= num10; ++index1)
            {
                for (int index2 = num7; index2 <= num9; ++index2)
                {
                    uint num13 = instance.m_treeGrid[index1 * 540 + index2];
                    int num14 = 0;
                    while ((int)num13 != 0)
                    {
                        if (((int)instance.m_trees.m_buffer[num13].m_flags & 3) == 1)
                        {
                            Vector3 position1 = instance.m_trees.m_buffer[num13].Position;
                            if ((double)position1.x >= (double)num3 && (double)position1.z >= (double)num4 && ((double)position1.x <= (double)num5 && (double)position1.z <= (double)num6))
                            {
                                num11 += 15;
                                num12 += instance.m_trees.m_buffer[num13].GrowState;
                            }
                        }
                        num13 = instance.m_trees.m_buffer[num13].m_nextGridTree;
                        if (++num14 >= LimitTreeManager.Helper.TreeLimit)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            byte num15 = (byte)Mathf.Min(num11 * 4, (int)byte.MaxValue);
            byte num16 = (byte)Mathf.Min(num12 * 4, (int)byte.MaxValue);
            NaturalResourceManager.ResourceCell resourceCell = nrm.m_naturalResources[num2 * 512 + num1];
            if ((int)num15 == (int)resourceCell.m_forest && (int)num16 == (int)resourceCell.m_tree)
                return;
            bool flag = (int)num15 != (int)resourceCell.m_forest;
            resourceCell.m_forest = num15;
            resourceCell.m_tree = num16;
            nrm.m_naturalResources[num2 * 512 + num1] = resourceCell;
            if (!flag)
                return;
            nrm.AreaModified(num1, num2, num1, num2);
        }

#if false
        private static void GetTileResources(NaturalResourceManager nm, int x, int z, out uint ore, out uint oil, out uint forest, out uint fertility, out uint water)
        {
            //int num = 2;
            ore = 0U;
            oil = 0U;
            forest = 0U;
            fertility = 0U;
            water = 0U;
            Debug.LogFormat("NMR {0} {1}", x, z);
            //nm.GetTileResourcesImpl(x + num, z + num, ref ore, ref oil, ref forest, ref fertility, ref water);
        }
#endif
    }
}
