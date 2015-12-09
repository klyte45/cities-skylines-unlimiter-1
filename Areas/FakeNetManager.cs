using ColossalFramework;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Areas
{
    [TargetType(typeof(NetManager))]
    internal class FakeNetManager
    {
        private static int[] m_tileNodesCount;

        private static void Init()
        {
            m_tileNodesCount = new int[20 * FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
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
            int publicServiceIndex = ItemClass.GetPublicServiceIndex(service);
            if (publicServiceIndex == -1)
                return;
            int areaIndex = Singleton<GameAreaManager>.instance.GetAreaIndex(position);
            if (areaIndex == -1)
                return;
            int num = areaIndex * 20;
            int publicSubServiceIndex = ItemClass.GetPublicSubServiceIndex(subService);
            //begin mod
            ++m_tileNodesCount[publicSubServiceIndex == -1 ? num + publicServiceIndex : num + (publicSubServiceIndex + 12)];
            //end mod
        }

        [ReplaceMethod]
        public int GetTileNodeCount(int x, int z, ItemClass.Service service, ItemClass.SubService subService)
        {
            int publicServiceIndex = ItemClass.GetPublicServiceIndex(service);
            if (publicServiceIndex != -1)
            {
                //begin mod
                int tileIndex = FakeGameAreaManager.GetTileIndex(x, z); //for some reason that method can't be detoured
                //end mod
                if (tileIndex != -1)
                {
                    int num = tileIndex * 20;
                    int publicSubServiceIndex = ItemClass.GetPublicSubServiceIndex(subService);
                    //begin mod
                    if (m_tileNodesCount == null)
                    {
                        Init();
                    }
                    return m_tileNodesCount[publicSubServiceIndex == -1 ? num + publicServiceIndex : num + (publicSubServiceIndex + 12)];
                    //end mod
                }
            }
            return 0;
        }
    }
}
