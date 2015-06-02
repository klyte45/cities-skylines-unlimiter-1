using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Zones
{
    internal class FakeBuildingTool
    {

        static MethodInfo CheckSpace;
        static FieldInfo mouseRayValid;
        static FieldInfo mouseAngle;
        static FieldInfo mouseRay;
        static FieldInfo mouseRayLength;
        static FieldInfo mousePosition;
        static FieldInfo connectionSegment;
        static FieldInfo productionRate;
        static FieldInfo constructionCost;
        static FieldInfo placementErrors;
        static FieldInfo cachedAngle;
        static FieldInfo elevation;


        private static void Init(BuildingTool b)
        {
            CheckSpace = b.GetType().GetMethod("CheckSpace", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseRayValid = b.GetType().GetField("m_mouseRayValid", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseAngle = b.GetType().GetField("m_mouseAngle", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseRay = b.GetType().GetField("m_mouseRay", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseRayLength = b.GetType().GetField("m_mouseRayLength", BindingFlags.NonPublic | BindingFlags.Instance);

            mousePosition = b.GetType().GetField("m_mousePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            
            connectionSegment = b.GetType().GetField("m_connectionSegment", BindingFlags.NonPublic | BindingFlags.Instance);
            productionRate = b.GetType().GetField("m_productionRate", BindingFlags.NonPublic | BindingFlags.Instance);
            constructionCost = b.GetType().GetField("m_constructionCost", BindingFlags.NonPublic | BindingFlags.Instance);
            placementErrors = b.GetType().GetField("m_placementErrors", BindingFlags.NonPublic | BindingFlags.Instance);
            cachedAngle = b.GetType().GetField("m_cachedAngle", BindingFlags.NonPublic | BindingFlags.Instance);
            elevation = b.GetType().GetField("m_elevation", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [ReplaceMethod]
        public static void SimulationStep(BuildingTool b)
        {
            if (mouseRay == null)
            {
                Init(b);
            }

            BuildingInfo buildingInfo;
            int num;
            GetPrefabInfo(b,out buildingInfo, out num);
            if (buildingInfo == null)
            {
                return;
            }
            ulong[] collidingSegmentBuffer;
            ulong[] collidingBuildingBuffer;

            ToolController m_toolController = ToolManager.instance.m_properties;
            if (m_toolController == null)
            {
                return;
            }

            m_toolController.BeginColliding(out collidingSegmentBuffer, out collidingBuildingBuffer);
            try
            {
                Ray m_mouseRay = (Ray)mouseRay.GetValue(b);
                float m_mouseRayLength = (float)mouseRayLength.GetValue(b);

                ToolBase.RaycastInput input = new ToolBase.RaycastInput(m_mouseRay, m_mouseRayLength);
                ToolBase.RaycastOutput raycastOutput;

                bool m_mouseRayValid = (bool)mouseRayValid.GetValue(b);
                if (m_mouseRayValid && RayCast(input, out raycastOutput))
                {
                    Vector3 vector = raycastOutput.m_hitPos;

                    float m_mouseAngle = (float)mouseAngle.GetValue(b);
                    float num2 = m_mouseAngle;
                    bool flag = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
                    float num3 = 0f;
                    ToolBase.ToolErrors toolErrors;
                    if (buildingInfo.m_placementMode == BuildingInfo.PlacementMode.Roadside)
                    {
                        toolErrors = ToolBase.ToolErrors.GridNotFound;
                        float num4 = raycastOutput.m_hitPos.x - 8f;
                        float num5 = raycastOutput.m_hitPos.z - 8f;
                        float num6 = raycastOutput.m_hitPos.x + 8f;
                        float num7 = raycastOutput.m_hitPos.z + 8f;
                        ZoneManager instance = Singleton<ZoneManager>.instance;
                        float num8 = 8f;
                        float num9 = 1000000f;
                        int num10 = Mathf.Max((int)((num4 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
                        int num11 = Mathf.Max((int)((num5 - 46f) / 64f + FakeZoneManager.HALFGRID), 0);
                        int num12 = Mathf.Min((int)((num6 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        int num13 = Mathf.Min((int)((num7 + 46f) / 64f + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        for (int i = num11; i <= num13; i++)
                        {
                            for (int j = num10; j <= num12; j++)
                            {
                                ushort num14 = FakeZoneManager.zoneGrid[i * FakeZoneManager.GRIDSIZE + j];
                                int num15 = 0;
                                while (num14 != 0)
                                {
                                    Vector3 position = instance.m_blocks.m_buffer[(int)num14].m_position;
                                    float num16 = Mathf.Max(Mathf.Max(num4 - 46f - position.x, num5 - 46f - position.z), Mathf.Max(position.x - num6 - 46f, position.z - num7 - 46f));
                                    if (num16 < 0f)
                                    {
                                        FindClosestZone(buildingInfo, num14, raycastOutput.m_hitPos, ref num8, ref num9, ref vector, ref num2);
                                    }
                                    num14 = instance.m_blocks.m_buffer[(int)num14].m_nextGridBlock;
                                    if (++num15 >= 32768)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }
                        if (num8 < 8f)
                        {
                            int num17;
                            if (Singleton<ZoneManager>.instance.CheckSpace(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, out num17))
                            {
                                float num18;
                                float num19;
                                float num20;
                                Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num18, out num19, out num20);
                                ToolBase.ToolErrors toolErrors2 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, num18, num20 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                                if (num19 - num18 > buildingInfo.m_maxHeightOffset)
                                {
                                    toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                }
                                if (toolErrors2 == ToolBase.ToolErrors.None)
                                {
                                    vector.y = num20;
                                }
                                toolErrors = toolErrors2;
                            }
                            else if (num17 < 0)
                            {
                                Vector3 a = new Vector3(Mathf.Cos(num2), 0f, Mathf.Sin(num2)) * 8f;
                                int num21 = buildingInfo.m_cellWidth >> 1;
                                for (int k = 1; k <= num21; k++)
                                {
                                    Vector3 vector2 = vector - a * (float)k;
                                    if (Singleton<ZoneManager>.instance.CheckSpace(vector2, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, out num17))
                                    {
                                        float num22;
                                        float num23;
                                        float num24;
                                        Building.SampleBuildingHeight(vector2, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num22, out num23, out num24);
                                        ToolBase.ToolErrors toolErrors3 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector2, num22, num24 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                                        if (num23 - num22 > buildingInfo.m_maxHeightOffset)
                                        {
                                            toolErrors3 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        }
                                        if (toolErrors3 == ToolBase.ToolErrors.None)
                                        {
                                            vector2.y = num24;
                                            vector = vector2;
                                        }
                                        toolErrors = toolErrors3;
                                        break;
                                    }
                                }
                            }
                            else if (num17 > 0)
                            {
                                Vector3 a2 = new Vector3(Mathf.Cos(num2), 0f, Mathf.Sin(num2)) * 8f;
                                int num25 = buildingInfo.m_cellWidth >> 1;
                                for (int l = 1; l <= num25; l++)
                                {
                                    Vector3 vector3 = vector + a2 * (float)l;
                                    if (Singleton<ZoneManager>.instance.CheckSpace(vector3, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, out num17))
                                    {
                                        float num26;
                                        float num27;
                                        float num28;
                                        Building.SampleBuildingHeight(vector3, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num26, out num27, out num28);
                                        ToolBase.ToolErrors toolErrors4 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector3, num26, num28 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                                        if (num27 - num26 > buildingInfo.m_maxHeightOffset)
                                        {
                                            toolErrors4 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        }
                                        if (toolErrors4 == ToolBase.ToolErrors.None)
                                        {
                                            vector3.y = num28;
                                            vector = vector3;
                                        }
                                        toolErrors = toolErrors4;
                                        break;
                                    }
                                }
                            }
                            if (toolErrors != ToolBase.ToolErrors.None)
                            {
                                float num29;
                                float num30;
                                float num31;
                                Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num29, out num30, out num31);
                                m_toolController.ResetColliding();
                                toolErrors = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, num29, num31 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                                if (num30 - num29 > buildingInfo.m_maxHeightOffset)
                                {
                                    toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                                }
                                vector.y = num31;
                            }
                        }
                    }
                    else if (buildingInfo.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
                    {
                        toolErrors = ToolBase.ToolErrors.ShoreNotFound;
                        Vector3 vector4;
                        Vector3 vector5;
                        if (Singleton<TerrainManager>.instance.GetShorePos(vector, 50f, out vector4, out vector5, out num3))
                        {
                            vector = vector4;
                            if (Singleton<TerrainManager>.instance.GetShorePos(vector, 50f, out vector4, out vector5, out num3))
                            {
                                vector = vector4;
                                num2 = Mathf.Atan2(vector5.x, -vector5.z);
                                float num32;
                                float num33;
                                float num34;
                                Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num32, out num33, out num34);
                                num32 = Mathf.Min(num3, num32);
                                num34 = Mathf.Max(vector.y, num34);
                                toolErrors = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, num32, num34 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                                if (vector.y - num3 > 128f)
                                {
                                    toolErrors |= ToolBase.ToolErrors.HeightTooHigh;
                                }
                                vector.y = num34;
                            }
                        }
                    }
                    else if (buildingInfo.m_placementMode == BuildingInfo.PlacementMode.OnSurface)
                    {
                        Quaternion rotation = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector -= rotation * buildingInfo.m_centerOffset;
                        num2 = b.m_angle * 0.0174532924f;
                        float minY;
                        float num35;
                        float num36;
                        Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out minY, out num35, out num36);
                        toolErrors = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, minY, num36 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                        vector.y = num36;
                    }
                    else if (buildingInfo.m_placementMode == BuildingInfo.PlacementMode.OnGround)
                    {
                        Quaternion rotation2 = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector -= rotation2 * buildingInfo.m_centerOffset;
                        num2 = b.m_angle * 0.0174532924f;
                        float num37;
                        float num38;
                        float num39;
                        Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out num37, out num38, out num39);
                        toolErrors = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, num37, num39 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                        if (num38 - num37 > buildingInfo.m_maxHeightOffset)
                        {
                            toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                        }
                        vector.y = num39;
                    }
                    else if (buildingInfo.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        Quaternion rotation3 = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector -= rotation3 * buildingInfo.m_centerOffset;
                        num2 = b.m_angle * 0.0174532924f;
                        float minY2;
                        float num40;
                        float num41;
                        Building.SampleBuildingHeight(vector, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out minY2, out num40, out num41);
                        toolErrors = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { buildingInfo, num, vector, minY2, num41 + buildingInfo.m_size.y, num2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer });
                        vector.y = num41;
                    }
                    else
                    {
                        toolErrors = ToolBase.ToolErrors.Pending;
                    }
                    Segment3 connectionSegment = default(Segment3);
                    float elevation = GetElevation(b, buildingInfo);
                    int productionRate;
                    int num42;
                    toolErrors |= buildingInfo.m_buildingAI.CheckBuildPosition((ushort)num, ref vector, ref num2, num3, elevation, ref connectionSegment, out productionRate, out num42);
                    if (buildingInfo.m_subBuildings != null && buildingInfo.m_subBuildings.Length != 0)
                    {
                        Matrix4x4 matrix4x = default(Matrix4x4);
                        matrix4x.SetTRS(vector, Quaternion.AngleAxis(num2 * 57.29578f, Vector3.down), Vector3.one);
                        for (int m = 0; m < buildingInfo.m_subBuildings.Length; m++)
                        {
                            BuildingInfo buildingInfo2 = buildingInfo.m_subBuildings[m].m_buildingInfo;
                            Vector3 vector6 = matrix4x.MultiplyPoint(buildingInfo.m_subBuildings[m].m_position);
                            float num43 = buildingInfo.m_subBuildings[m].m_angle * 0.0174532924f + (float)cachedAngle.GetValue(b);
                            Segment3 segment = default(Segment3);
                            int num44;
                            int num45;
                            toolErrors |= buildingInfo2.m_buildingAI.CheckBuildPosition((ushort)num, ref vector6, ref num43, num3, elevation, ref segment, out num44, out num45);
                            num42 += num45;
                        }
                    }
                    if (flag && Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, num42) != num42)
                    {
                        toolErrors |= ToolBase.ToolErrors.NotEnoughMoney;
                    }
                    if (!Singleton<BuildingManager>.instance.CheckLimits())
                    {
                        toolErrors |= ToolBase.ToolErrors.TooManyObjects;
                    }

                    FakeBuildingTool.mousePosition.SetValue(b, vector);
                    FakeBuildingTool.mouseAngle.SetValue(b, num2);
                    FakeBuildingTool.connectionSegment.SetValue(b, connectionSegment);
                    FakeBuildingTool.productionRate.SetValue(b, productionRate);
                    FakeBuildingTool.constructionCost.SetValue(b, num42);
                    FakeBuildingTool.placementErrors.SetValue(b, toolErrors);
                }
                else
                {
                    FakeBuildingTool.placementErrors.SetValue(b, ToolBase.ToolErrors.RaycastFailed);
                    FakeBuildingTool.connectionSegment.SetValue(b, new Segment3());

                }
            }
            finally
            {
                m_toolController.EndColliding();
            }
        }

        private static float GetElevation(BuildingTool b,BuildingInfo info)
        {
            if (info == null)
            {
                return 0f;
            }
            int num;
            int num2;
            info.m_buildingAI.GetElevationLimits(out num, out num2);
            if (num == num2)
            {
                return 0f;
            }

            float m_elevation = (float)elevation.GetValue(b);
            return (float)Mathf.Clamp(m_elevation, num, num2) * 12f;
        }

        private static bool RayCast(ToolBase.RaycastInput raycastInput, out ToolBase.RaycastOutput output)
        {
            object[] p = new object[]{raycastInput, null};
            bool res = (bool)typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, p);
            output = (ToolBase.RaycastOutput) p[1];
            return res;
        }

        private static void GetPrefabInfo(BuildingTool b, out BuildingInfo info, out int relocating)
        {
            info = b.m_prefab;
            relocating = b.m_relocate;
            if (info != null)
            {
                relocating = 0;
                return;
            }
            if (relocating != 0)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                if ((instance.m_buildings.m_buffer[relocating].m_flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                {
                    info = instance.m_buildings.m_buffer[relocating].Info;
                    return;
                }
            }
            info = null;
            relocating = 0;
        }

        private static void FindClosestZone(BuildingInfo info, ushort block, Vector3 refPos, ref float minD, ref float min2, ref Vector3 minPos, ref float minAngle)
        {
            if (block == 0)
            {
                return;
            }
            ZoneBlock zoneBlock = Singleton<ZoneManager>.instance.m_blocks.m_buffer[(int)block];
            if (Mathf.Abs(zoneBlock.m_position.x - refPos.x) >= 52f || Mathf.Abs(zoneBlock.m_position.z - refPos.z) >= 52f)
            {
                return;
            }
            int rowCount = zoneBlock.RowCount;
            Vector3 vector = new Vector3(Mathf.Cos(zoneBlock.m_angle), 0f, Mathf.Sin(zoneBlock.m_angle)) * 8f;
            Vector3 a = new Vector3(vector.z, 0f, -vector.x);
            for (int i = 0; i < rowCount; i++)
            {
                Vector3 b = ((float)i - 3.5f) * a;
                int num = 0;
                while ((long)num < 4L)
                {
                    if ((zoneBlock.m_valid & 1uL << (i << 3 | num)) != 0uL)
                    {
                        Vector3 b2 = ((float)num - 3.5f) * vector;
                        Vector3 a2 = zoneBlock.m_position + b2 + b;
                        float num2 = Mathf.Sqrt((a2.x - refPos.x) * (a2.x - refPos.x) + (a2.z - refPos.z) * (a2.z - refPos.z));
                        float num3 = Vector3.Dot(vector, refPos - zoneBlock.m_position);
                        if (num2 <= minD - 0.2f || (num2 < minD + 0.2f && num3 < min2))
                        {
                            minD = num2;
                            min2 = num3;
                            if ((info.m_cellWidth & 1) == 0)
                            {
                                Vector3 vector2 = a2 + a * 0.5f;
                                Vector3 vector3 = a2 - a * 0.5f;
                                float num4 = (vector2.x - refPos.x) * (vector2.x - refPos.x) + (vector2.z - refPos.z) * (vector2.z - refPos.z);
                                float num5 = (vector3.x - refPos.x) * (vector3.x - refPos.x) + (vector3.z - refPos.z) * (vector3.z - refPos.z);
                                if (num4 < num5)
                                {
                                    minPos = zoneBlock.m_position + ((float)info.m_cellLength * 0.5f - 4f) * vector + ((float)i - 3f) * a;
                                }
                                else
                                {
                                    minPos = zoneBlock.m_position + ((float)info.m_cellLength * 0.5f - 4f) * vector + ((float)i - 4f) * a;
                                }
                            }
                            else
                            {
                                minPos = zoneBlock.m_position + ((float)info.m_cellLength * 0.5f - 4f) * vector + ((float)i - 3.5f) * a;
                            }
                            minPos.y = refPos.y;
                            minAngle = zoneBlock.m_angle + 1.57079637f;
                        }
                    }
                    num++;
                }
            }
        }
    }
}
