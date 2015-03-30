using Unlimiter.Attributes;

namespace Unlimiter.Zones
{
    [Fixme]
    internal class FakeBuildingTool
    {
#if false
        private static void SimulationStep(BuildingTool b)
        {
            BuildingInfo info;
            int relocating;
            GetPrefabInfo(b, out info, out relocating);
            if (info == null)
                return;
            ulong[] collidingSegments;
            ulong[] collidingBuildings;

            ToolController m_toolController = (ToolController) typeof(ToolBase).GetField("m_toolController", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(b);
            m_toolController.BeginColliding(out collidingSegments, out collidingBuildings);
            try
            {
                ToolBase.RaycastOutput output;
                bool m_mouseRayValid = (bool)b.GetType().GetField("m_mouseRayValid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(b);
                if (m_mouseRayValid && RayCast(new ToolBase.RaycastInput(b.m_mouseRay, b.m_mouseRayLength), out output))
                {
                    Vector3 vector3_1 = output.m_hitPos;
                    float num1 = b.m_mouseAngle;
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
                        int num6 = Mathf.Max((int)(((double)num2 - 46.0) / 64.0 + 75.0), 0);
                        int num7 = Mathf.Max((int)(((double)num3 - 46.0) / 64.0 + 75.0), 0);
                        int num8 = Mathf.Min((int)(((double)num4 + 46.0) / 64.0 + 75.0), 149);
                        int num9 = Mathf.Min((int)(((double)num5 + 46.0) / 64.0 + 75.0), 149);
                        for (int index1 = num7; index1 <= num9; ++index1)
                        {
                            for (int index2 = num6; index2 <= num8; ++index2)
                            {
                                ushort block = instance.m_zoneGrid[index1 * 150 + index2];
                                int num10 = 0;
                                while ((int)block != 0)
                                {
                                    Vector3 vector3_2 = instance.m_blocks.m_buffer[(int)block].m_position;
                                    if ((double)Mathf.Max(Mathf.Max(num2 - 46f - vector3_2.x, num3 - 46f - vector3_2.z), Mathf.Max((float)((double)vector3_2.x - (double)num4 - 46.0), (float)((double)vector3_2.z - (double)num5 - 46.0))) < 0.0)
                                        b.FindClosestZone(info, block, output.m_hitPos, ref minD, ref min2, ref vector3_1, ref num1);
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
                                ToolBase.ToolErrors toolErrors2 = b.CheckSpace(info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                        ToolBase.ToolErrors toolErrors2 = b.CheckSpace(info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                        ToolBase.ToolErrors toolErrors2 = b.CheckSpace(info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                toolErrors1 = b.CheckSpace(info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                toolErrors1 = b.CheckSpace(info, relocating, vector3_1, minY2, num2 + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)vector3_1.y - (double)waterHeight > 128.0)
                                    toolErrors1 |= ToolBase.ToolErrors.HeightTooHigh;
                                vector3_1.y = num2;
                            }
                        }
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnSurface)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = b.CheckSpace(info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                        vector3_1.y = buildingY;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = b.CheckSpace(info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                            toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                        vector3_1.y = buildingY;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(b.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = b.m_angle * (Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        toolErrors1 = b.CheckSpace(info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                    b.m_mousePosition = vector3_1;
                    b.m_mouseAngle = num1;
                    b.m_connectionSegment = connectionSegment;
                    b.m_productionRate = productionRate;
                    b.m_constructionCost = constructionCost;
                    b.m_placementErrors = toolErrors3;
                }
                else
                {
                    b.m_placementErrors = ToolBase.ToolErrors.RaycastFailed;
                    b.m_connectionSegment = new Segment3();
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
            bool res = typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, p);
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
#endif
    }
}
