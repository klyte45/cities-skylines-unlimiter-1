using ColossalFramework;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Trees
{
    internal class RotatingTreeInstance
    {
        private static System.Random random = new System.Random();

        [ReplaceMethod]
        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, TreeInfo info, Vector3 position, float scale, float brightness)
        {
            if (!info.m_prefabInitialized)
                return;
            if (cameraInfo == null || (UnityEngine.Object)info.m_lodMesh1 == (UnityEngine.Object)null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance))
            {
                TreeManager instance = Singleton<TreeManager>.instance;
                MaterialPropertyBlock properties = instance.m_materialBlock;
                Matrix4x4 matrix = new Matrix4x4();
                matrix.SetTRS(position, Quaternion.Euler(0, info.m_instanceID.RawData % 360, 0), new Vector3(scale, scale, scale));
                Color color = info.m_defaultColor * brightness;
                color.a = Singleton<WindManager>.instance.GetWindSpeed(position);
                properties.Clear();
                properties.AddColor(instance.ID_Color, color);
                ++instance.m_drawCallData.m_defaultCalls;

                Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, (Camera)null, 0, properties);
            }
            else
            {
                position.y += info.m_generatedInfo.m_center.y * (scale - 1f);
                Color color = info.m_defaultColor * brightness;
                color.a = Singleton<WindManager>.instance.GetWindSpeed(position);
                info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, scale);
                info.m_lodColors[info.m_lodCount] = color;
                info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                if (++info.m_lodCount != info.m_lodLocations.Length)
                    return;
                TreeInstance.RenderLod(cameraInfo, info);
            }
        }
    }
}
