using ColossalFramework;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

namespace EightyOne.Areas
{
    [TargetType(typeof(NetManager))]
    internal class FakeNetManager
    {
        private static int[] m_tileNodesCount;
        public static bool DontUpdateNodeFlags;

        private static void Init()
        {
            DontUpdateNodeFlags = false;
            m_tileNodesCount = new int[20 * FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
            var nodes = NetManager.instance.m_nodes.m_buffer;
            for (int n = 1; n < nodes.Length; n++)
            {
                if (nodes[n].m_flags != NetNode.Flags.None)
                {
                    NetInfo info = nodes[n].Info;
                    if (info != null)
                    {
                        info.m_netAI.NodeLoaded((ushort)n, ref nodes[n], 0);
                    }
                }
            }
        }

        [RedirectMethod]
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

        [RedirectMethod]
        public int GetTileNodeCount(int x, int z, ItemClass.Service service, ItemClass.SubService subService)
        {
            int publicServiceIndex = ItemClass.GetPublicServiceIndex(service);
            if (publicServiceIndex != -1)
            {
                //begin mod
                int tileIndex = FakeGameAreaManager.GetTileIndex(x, z); //This method gets inlined and can't be detoured
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
                    var index0 = publicSubServiceIndex == -1 ? num + publicServiceIndex : num + (publicSubServiceIndex + 12);
                    return m_tileNodesCount[index0];
                    //end mod
                }
            }
            return 0;
        }

        [RedirectMethod]
        public void UpdateNodeFlags(ushort node)
        {
            if (DontUpdateNodeFlags)
            {
                return;
            }
            if (NetManager.instance.m_nodes.m_buffer[(int)node].m_flags == NetNode.Flags.None)
                return;
            NetInfo info = NetManager.instance.m_nodes.m_buffer[(int)node].Info;
            if (info == null)
                return;
            info.m_netAI.UpdateNodeFlags(node, ref NetManager.instance.m_nodes.m_buffer[(int)node]);
        }
    }
}
