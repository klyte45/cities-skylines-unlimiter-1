using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter
{
    public static class FakeBuildingDecoration
    {
        public static void ClearDecorations()
        {
            NetManager instance1 = Singleton<NetManager>.instance;
            for (int index = 1; index < 32768; ++index)
            {
                if (instance1.m_segments.m_buffer[index].m_flags != NetSegment.Flags.None)
                    instance1.ReleaseSegment((ushort)index, true);
            }
            for (int index = 1; index < 32768; ++index)
            {
                if (instance1.m_nodes.m_buffer[index].m_flags != NetNode.Flags.None)
                    instance1.ReleaseNode((ushort)index);
            }
            PropManager instance2 = Singleton<PropManager>.instance;
            for (int index = 1; index < 65536; ++index)
            {
                if ((int)instance2.m_props.m_buffer[index].m_flags != 0)
                    instance2.ReleaseProp((ushort)index);
            }
            TreeManager instance3 = Singleton<TreeManager>.instance;
            for (int index = 1; index < Mod.MAX_TREE_COUNT; ++index)
            {
                if ((int)instance3.m_trees.m_buffer[index].m_flags != 0)
                    instance3.ReleaseTree((uint)index);
            }
        }

        public static void SaveProps(BuildingInfo info, ushort buildingID, ref Building data)
        {
            FastList<BuildingInfo.Prop> fastList = new FastList<BuildingInfo.Prop>();
            Vector3 pos = data.m_position;
            Quaternion q = Quaternion.AngleAxis(data.m_angle * 57.29578f, Vector3.down);
            Matrix4x4 matrix4x4 = new Matrix4x4();
            matrix4x4.SetTRS(pos, q, Vector3.one);
            matrix4x4 = matrix4x4.inverse;
            PropManager instance1 = Singleton<PropManager>.instance;
            for (int index = 0; index < 65536; ++index)
            {
                if (((int)instance1.m_props.m_buffer[index].m_flags & 67) == 1)
                {
                    BuildingInfo.Prop prop = new BuildingInfo.Prop();
                    prop.m_prop = instance1.m_props.m_buffer[index].Info;
                    prop.m_finalProp = prop.m_prop;
                    prop.m_position = matrix4x4.MultiplyPoint(instance1.m_props.m_buffer[index].Position);
                    prop.m_radAngle = instance1.m_props.m_buffer[index].Angle - data.m_angle;
                    prop.m_angle = 57.29578f * prop.m_radAngle;
                    prop.m_fixedHeight = instance1.m_props.m_buffer[index].FixedHeight;
                    prop.m_probability = 100;
                    fastList.Add(prop);
                }
            }
            TreeManager instance2 = Singleton<TreeManager>.instance;
            for (int index = 0; index < Mod.MAX_TREE_COUNT; ++index)
            {
                if (((int)instance2.m_trees.m_buffer[index].m_flags & 3) == 1 && instance2.m_trees.m_buffer[index].GrowState != 0)
                {
                    BuildingInfo.Prop prop = new BuildingInfo.Prop();
                    prop.m_tree = instance2.m_trees.m_buffer[index].Info;
                    prop.m_finalTree = prop.m_tree;
                    prop.m_position = matrix4x4.MultiplyPoint(instance2.m_trees.m_buffer[index].Position);
                    prop.m_fixedHeight = instance2.m_trees.m_buffer[index].FixedHeight;
                    prop.m_probability = 100;
                    fastList.Add(prop);
                }
            }
            info.m_props = fastList.ToArray();
        }
    }
}
