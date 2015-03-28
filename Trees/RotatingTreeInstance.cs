using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter.Trees
{
    internal class RotatingTreeInstance
    {
        private static System.Random random = new System.Random();
        private static ulong wut;

        private static void RenderInstance(RenderManager.CameraInfo cameraInfo, TreeInfo info, Vector3 position, float scale, float brightness)
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

        private static void RenderLod(RenderManager.CameraInfo cameraInfo, TreeInfo info)
        {
            TreeManager instance = Singleton<TreeManager>.instance;
            MaterialPropertyBlock properties = instance.m_materialBlock;
            properties.Clear();
            for (int index = 0; index < info.m_lodCount; ++index)
            {
                properties.AddVector(instance.ID_TreeLocation[index], info.m_lodLocations[index]);
                properties.AddColor(instance.ID_TreeColor[index], info.m_lodColors[index]);
            }
            Mesh mesh;
            int num;
            if (info.m_lodCount <= 1)
            {
                mesh = info.m_lodMesh1;
                num = 1;
            }
            else if (info.m_lodCount <= 4)
            {
                mesh = info.m_lodMesh4;
                num = 4;
            }
            else if (info.m_lodCount <= 8)
            {
                mesh = info.m_lodMesh8;
                num = 8;
            }
            else
            {
                mesh = info.m_lodMesh16;
                num = 16;
            }
            for (int index = info.m_lodCount; index < num; ++index)
            {
                properties.AddVector(instance.ID_TreeLocation[index], (Vector4)(cameraInfo.m_forward * -100000f));
                properties.AddColor(instance.ID_TreeColor[index], Color.black);
            }
            Bounds bounds = new Bounds();
            bounds.SetMinMax(info.m_lodMin - new Vector3(100f, 100f, 100f), info.m_lodMax + new Vector3(100f, 100f, 100f));
            mesh.bounds = bounds;
            info.m_lodMin = new Vector3(100000f, 100000f, 100000f);
            info.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
            ++instance.m_drawCallData.m_lodCalls;
            instance.m_drawCallData.m_batchedCalls += info.m_lodCount - 1;

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetTRS(Vector3.zero, Quaternion.Euler(0, ++wut % 360, 0), Vector3.one); // this -clearly- doesn't work
            Graphics.DrawMesh(mesh, matrix, info.m_lodMaterial, info.m_prefabDataLayer, (Camera)null, 0, properties);
            info.m_lodCount = 0;
        }
    }
}
