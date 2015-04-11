using ColossalFramework;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Areas
{
    internal class FakeNetManager
    {
        private static int[] m_tileNodesCount;

        public static void Init()
        {
            m_tileNodesCount = new int[17 * FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
            var nodes = NetManager.instance.m_nodes.m_buffer;
            for (int n = 1; n < nodes.Length; n++)
            {
                if (nodes[n].m_flags != NetNode.Flags.None)
                {
                    NetInfo info = nodes[n].Info;
                    if (info != null)
                    {
                        info.m_netAI.NodeLoaded((ushort)n, ref nodes[n]);
                    }
                }
            }
        }

        [ReplaceMethod]
        public void AddTileNode(Vector3 position, ItemClass.Service service, ItemClass.SubService subService)
        {
            if (service > ItemClass.Service.Office)
            {
                int num = Singleton<GameAreaManager>.instance.GetAreaIndex(position);
                if (num != -1)
                {
                    num *= 17;
                    if (subService != ItemClass.SubService.None)
                    {
                        num += subService - ItemClass.SubService.IndustrialOre - 1 + 12;
                    }
                    else
                    {
                        num += service - ItemClass.Service.Office - 1;
                    }
                    m_tileNodesCount[num] += 1 ;
                }
            }
        }

        [ReplaceMethod]
        public int GetTileNodeCount(int x, int z, ItemClass.Service service, ItemClass.SubService subService)
        {
            int num = Singleton<GameAreaManager>.instance.GetTileIndex(x, z);
            if (num != -1)
            {
                num *= 17;
                if (subService != ItemClass.SubService.None)
                {
                    num += subService - ItemClass.SubService.IndustrialOre - 1 + 12;
                }
                else
                {
                    num += service - ItemClass.Service.Office - 1;
                }
                return m_tileNodesCount[num];
            }
            return 0;
        }
    }
}
