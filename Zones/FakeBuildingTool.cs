using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Zones
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
        }

        [ReplaceMethod]
        private static void SimulationStep(BuildingTool b)
        {
            if (CheckSpace == null)
            {
                Init(b);
            }

            BuildingInfo info;
            int relocating;
            GetPrefabInfo(b, out info, out relocating);
            if (info == null)
                return;
            ulong[] collidingSegments;
            ulong[] collidingBuildings;

            ToolController m_toolController = ToolManager.instance.m_properties;
            m_toolController.BeginColliding(out collidingSegments, out collidingBuildings);
            try
            {
                ToolBase.RaycastOutput output;
                bool m_mouseRayValid = (bool)mouseRayValid.GetValue(b);
                float m_mouseAngle = (float)mouseAngle.GetValue(b);
                Ray m_mouseRay = (Ray)mouseRay.GetValue(b);
                float m_mouseRayLength = (float)mouseRayLength.GetValue(b);
 
                if (m_mouseRayValid && RayCast(new ToolBase.RaycastInput(m_mouseRay, m_mouseRayLength), out output))
                {
                    Vector3 vector3_1 = output.m_hitPos;
                    float num1 = m_mouseAngle;
                    bool flag = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
                    float waterHeight = 0.0f;
                    ToolBase.ToolErrors toolErrors1;
                    if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside)
                    {
                        toolErrors1 = ToolBase.ToolErrors.GridNotFound;
                        float num2 = output.m_hitPos.x - 8f;
                        float num3 = output.m_hitPos.z - 8f;
                        float num4 = output.m_hitPos.x + 8f;
                        float num5 = output.m_hitPos.z + 8f;
                        ZoneManager instance = Singleton<ZoneManager>.instance;
                        float minD = 8f;
                        float min2 = 1000000f;
                        int num6 = Mathf.Max((int)(((double)num2 - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
                        int num7 = Mathf.Max((int)(((double)num3 - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
                        int num8 = Mathf.Min((int)(((double)num4 + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        int num9 = Mathf.Min((int)(((double)num5 + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        for (int index1 = num7; index1 <= num9; ++index1)
                        {
                            for (int index2 = num6; index2 <= num8; ++index2)
                            {
                                ushort block = FakeZoneManager.zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                                int num10 = 0;
                                while ((int)block != 0)
                                {
                                    Vector3 vector3_2 = instance.m_blocks.m_buffer[(int)block].m_position;
                                    if ((double)Mathf.Max(Mathf.Max(num2 - 46f - vector3_2.x, num3 - 46f - vector3_2.z), Mathf.Max((float)((double)vector3_2.x - (double)num4 - 46.0), (float)((double)vector3_2.z - (double)num5 - 46.0))) < 0.0)
                                        FindClosestZone(info, block, output.m_hitPos, ref minD, ref min2, ref vector3_1, ref num1);
                                    block = instance.m_blocks.m_buffer[(int)block].m_nextGridBlock;
                                    if (++num10 >= 32768)
                                    {
                                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                                        break;
                                    }
                                }
                            }
                        }
                        if ((double)minD < 8.0)
                        {
                            int offset;
                            if (Singleton<ZoneManager>.instance.CheckSpace(vector3_1, num1, info.m_cellWidth, info.m_cellLength, out offset))
                            {
                                float minY;
                                float maxY;
                                float buildingY;
                                Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);


                                ToolBase.ToolErrors toolErrors2 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings });
                                if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                    toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                if (toolErrors2 == ToolBase.ToolErrors.None)
                                    vector3_1.y = buildingY;
                                toolErrors1 = toolErrors2;
                            }
                            else if (offset < 0)
                            {
                                Vector3 vector3_2 = new Vector3(Mathf.Cos(num1), 0.0f, Mathf.Sin(num1)) * 8f;
                                int num10 = info.m_cellWidth >> 1;
                                for (int index = 1; index <= num10; ++index)
                                {
                                    Vector3 vector3_3 = vector3_1 - vector3_2 * (float)index;
                                    if (Singleton<ZoneManager>.instance.CheckSpace(vector3_3, num1, info.m_cellWidth, info.m_cellLength, out offset))
                                    {
                                        float minY;
                                        float maxY;
                                        float buildingY;
                                        Building.SampleBuildingHeight(vector3_3, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                                        ToolBase.ToolErrors toolErrors2 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] {info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                            toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        if (toolErrors2 == ToolBase.ToolErrors.None)
                                        {
                                            vector3_3.y = buildingY;
                                            vector3_1 = vector3_3;
                                        }
                                        toolErrors1 = toolErrors2;
                                        break;
                                    }
                                }
                            }
                            else if (offset > 0)
                            {
                                Vector3 vector3_2 = new Vector3(Mathf.Cos(num1), 0.0f, Mathf.Sin(num1)) * 8f;
                                int num10 = info.m_cellWidth >> 1;
                                for (int index = 1; index <= num10; ++index)
                                {
                                    Vector3 vector3_3 = vector3_1 + vector3_2 * (float)index;
                                    if (Singleton<ZoneManager>.instance.CheckSpace(vector3_3, num1, info.m_cellWidth, info.m_cellLength, out offset))
                                    {
                                        float minY;
                                        float maxY;
                                        float buildingY;
                                        Building.SampleBuildingHeight(vector3_3, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                                        ToolBase.ToolErrors toolErrors2 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings });
                                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                            toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        if (toolErrors2 == ToolBase.ToolErrors.None)
                                        {
                                            vector3_3.y = buildingY;
                                            vector3_1 = vector3_3;
                                        }
                                        toolErrors1 = toolErrors2;
                                        break;
                                    }
                                }
                            }
                            if (toolErrors1 != ToolBase.ToolErrors.None)
                            {
                                float minY;
                                float maxY;
                                float buildingY;
                                Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                                m_toolController.ResetColliding();
                                toolErrors1 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                                if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                    toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                                vector3_1.y = buildingY;
                            }
                        }
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
                    {
                        toolErrors1 = ToolBase.ToolErrors.ShoreNotFound;
                        Vector3 position;
                        Vector3 direction;
                        if (Singleton<TerrainManager>.instance.GetShorePos(vector3_1, 50f, out position, out direction, out waterHeight))
                        {
                            vector3_1 = position;
                            if (Singleton<TerrainManager>.instance.GetShorePos(vector3_1, 50f, out position, out direction, out waterHeight))
                            {
                                vector3_1 = position;
                                num1 = Mathf.Atan2(direction.x, -direction.z);
                                float minY1;
                                float maxY;
                                float buildingY;
                                Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY1, out maxY, out buildingY);
                                float minY2 = Mathf.Min(waterHeight, minY1);
                                float num2 = Mathf.Max(vector3_1.y, buildingY);
                                toolErrors1 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY2, num2 + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                                if ((double)vector3_1.y - (double)waterHeight > 128.0)
                                    toolErrors1 |= ToolBase.ToolErrors.HeightTooHigh;
                                vector3_1.y = num2;
                            }
                        }
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnSurface)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                        vector3_1.y = buildingY;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                            toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                        vector3_1.y = buildingY;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = (ToolBase.ToolErrors)CheckSpace.Invoke(b, new object[] { info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings});
                        vector3_1.y = buildingY;
                    }
                    else
                        toolErrors1 = ToolBase.ToolErrors.Pending;
                    Segment3 connectionSegment = new Segment3();
                    int productionRate;
                    int constructionCost;
                    ToolBase.ToolErrors toolErrors3 = toolErrors1 | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, ref connectionSegment, out productionRate, out constructionCost);
                    if (flag && Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, constructionCost) != constructionCost)
                        toolErrors3 |= ToolBase.ToolErrors.NotEnoughMoney;
                    if (!Singleton<BuildingManager>.instance.CheckLimits())
                        toolErrors3 |= ToolBase.ToolErrors.TooManyObjects;

                    mousePosition.SetValue(b, vector3_1);
                    mouseAngle.SetValue(b, num1);
                    FakeBuildingTool.connectionSegment.SetValue(b, connectionSegment);
                    FakeBuildingTool.productionRate.SetValue(b, productionRate);
                    FakeBuildingTool.constructionCost.SetValue(b, constructionCost);
                    placementErrors.SetValue(b, toolErrors3);
                }
                else
                {
                    placementErrors.SetValue(b, ToolBase.ToolErrors.RaycastFailed);
                    FakeBuildingTool.connectionSegment.SetValue(b, new Segment3());                    
                }
            }
            finally
            {
                m_toolController.EndColliding();
            }
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
            }
            else
            {
                if (relocating != 0)
                {
                    BuildingManager instance = Singleton<BuildingManager>.instance;
                    if ((instance.m_buildings.m_buffer[relocating].m_flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                    {
                        info = instance.m_buildings.m_buffer[relocating].Info;
                        return;
                    }
                }
                info = (BuildingInfo)null;
                relocating = 0;
            }
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
