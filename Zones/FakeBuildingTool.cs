using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;
using EightyOne.Redirection;

namespace EightyOne.Zones
{
    [TargetType(typeof(BuildingTool))]
    internal class FakeBuildingTool : BuildingTool
    {
        private static FieldInfo mouseRayValidField;
        private static FieldInfo mouseAngleField;
        private static FieldInfo mouseRayField;
        private static FieldInfo mouseRayLengthField;
        private static FieldInfo mousePositionField;
        private static FieldInfo connectionSegmentField;
        private static FieldInfo productionRateField;
        private static FieldInfo constructionCostField;
        private static FieldInfo placementErrorsField;
        private static FieldInfo cachedAngleField;

        private static void Init(BuildingTool b)
        {
            mouseRayValidField = b.GetType().GetField("m_mouseRayValid", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseAngleField = b.GetType().GetField("m_mouseAngle", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseRayField = b.GetType().GetField("m_mouseRay", BindingFlags.NonPublic | BindingFlags.Instance);
            mouseRayLengthField = b.GetType().GetField("m_mouseRayLength", BindingFlags.NonPublic | BindingFlags.Instance);
            mousePositionField = b.GetType().GetField("m_mousePosition", BindingFlags.NonPublic | BindingFlags.Instance);
            connectionSegmentField = b.GetType().GetField("m_connectionSegment", BindingFlags.NonPublic | BindingFlags.Instance);
            productionRateField = b.GetType().GetField("m_productionRate", BindingFlags.NonPublic | BindingFlags.Instance);
            constructionCostField = b.GetType().GetField("m_constructionCost", BindingFlags.NonPublic | BindingFlags.Instance);
            placementErrorsField = b.GetType().GetField("m_placementErrors", BindingFlags.NonPublic | BindingFlags.Instance);
            cachedAngleField = b.GetType().GetField("m_cachedAngle", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [RedirectReverse]
        private static ToolBase.ToolErrors CheckSpace(BuildingTool b, BuildingInfo info, int relocating, Vector3 pos,
            float minY, float maxY, float angle, int width, int length, bool test, ulong[] collidingSegmentBuffer,
            ulong[] collidingBuildingBuffer)
        {
            UnityEngine.Debug.Log(
                $"{b}-{info}-{relocating}-{pos}-{minY}-{maxY}-{angle}-{width}-{length}-{test}-{collidingSegmentBuffer}-{collidingBuildingBuffer}");
            return ToolBase.ToolErrors.None;
        }

        [RedirectReverse]
        private static float GetElevation(BuildingTool b, BuildingInfo info)
        {
            UnityEngine.Debug.Log($"{b}-{info}");
            return 0.0f;
        }

        [RedirectReverse]
        private static void GetPrefabInfo(BuildingTool b, out BuildingInfo info, out int relocating)
        {
            info = null;
            relocating = 0;
            UnityEngine.Debug.Log($"{b}-{info}-{relocating}");
        }

        [RedirectReverse]
        private static void FindClosestZone(BuildingTool b, BuildingInfo info, ushort block, Vector3 refPos, ref float minD, ref float min2, ref Vector3 minPos, ref float minAngle)
        {
            UnityEngine.Debug.Log($"{b}-{info}-{block}-{refPos}-{minD}-{min2}-{minPos}-{minAngle}");
        }

        [RedirectMethod]
        public override void SimulationStep()
        {
            //begin mod
            if (mouseRayField == null)
            {
                Init(this);
            }
            //end mod

            BuildingInfo info;
            int relocating;
            GetPrefabInfo(this, out info, out relocating);
            if (info == null)
                return;
            ulong[] collidingSegments;
            ulong[] collidingBuildings;
            this.m_toolController.BeginColliding(out collidingSegments, out collidingBuildings);
            try
            {
                ToolBase.RaycastOutput output;
                if ((bool)mouseRayValidField.GetValue(this) && ToolBase.RayCast(new ToolBase.RaycastInput((Ray)mouseRayField.GetValue(this), (float)mouseRayLengthField.GetValue(this)), out output))
                {
                    Vector3 vector3_1 = output.m_hitPos;
                    float num1 = (float)mouseAngleField.GetValue(this);
                    bool flag = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
                    float waterHeight = 0.0f;
                    Segment3 connectionSegment1 = new Segment3();
                    float elevation = GetElevation(this, info);
                    int productionRate1;
                    int constructionCost1;
                    ToolBase.ToolErrors toolErrors1;
                    if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside)
                    {
                        ToolBase.ToolErrors toolErrors2 = ToolBase.ToolErrors.GridNotFound;
                        float num2 = output.m_hitPos.x - 8f;
                        float num3 = output.m_hitPos.z - 8f;
                        float num4 = output.m_hitPos.x + 8f;
                        float num5 = output.m_hitPos.z + 8f;
                        ZoneManager instance = Singleton<ZoneManager>.instance;
                        float minD = 8f;
                        float min2 = 1000000f;
                        //begin mod
                        int num6 = Mathf.Max((int)(((double)num2 - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
                        int num7 = Mathf.Max((int)(((double)num3 - 46.0) / 64.0 + FakeZoneManager.HALFGRID), 0);
                        int num8 = Mathf.Min((int)(((double)num4 + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        int num9 = Mathf.Min((int)(((double)num5 + 46.0) / 64.0 + FakeZoneManager.HALFGRID), FakeZoneManager.GRIDSIZE - 1);
                        //end mod
                        for (int index1 = num7; index1 <= num9; ++index1)
                        {
                            for (int index2 = num6; index2 <= num8; ++index2)
                            {
                                //begin mod
                                ushort block = instance.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                                //end mod
                                int num10 = 0;
                                while ((int)block != 0)
                                {
                                    Vector3 vector3_2 = instance.m_blocks.m_buffer[(int)block].m_position;
                                    if ((double)Mathf.Max(Mathf.Max(num2 - 46f - vector3_2.x, num3 - 46f - vector3_2.z), Mathf.Max((float)((double)vector3_2.x - (double)num4 - 46.0), (float)((double)vector3_2.z - (double)num5 - 46.0))) < 0.0)
                                        FindClosestZone(this, info, block, output.m_hitPos, ref minD, ref min2, ref vector3_1, ref num1);
                                    block = instance.m_blocks.m_buffer[(int)block].m_nextGridBlock;
                                    if (++num10 >= 49152)
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
                                ToolBase.ToolErrors toolErrors3 = CheckSpace(this, info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                    toolErrors3 |= ToolBase.ToolErrors.SlopeTooSteep;
                                if (toolErrors3 == ToolBase.ToolErrors.None)
                                    vector3_1.y = buildingY;
                                toolErrors2 = toolErrors3;
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
                                        ToolBase.ToolErrors toolErrors3 = CheckSpace(this, info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                            toolErrors3 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        if (toolErrors3 == ToolBase.ToolErrors.None)
                                        {
                                            vector3_3.y = buildingY;
                                            vector3_1 = vector3_3;
                                        }
                                        toolErrors2 = toolErrors3;
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
                                        ToolBase.ToolErrors toolErrors3 = CheckSpace(this, info, relocating, vector3_3, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                            toolErrors3 |= ToolBase.ToolErrors.SlopeTooSteep;
                                        if (toolErrors3 == ToolBase.ToolErrors.None)
                                        {
                                            vector3_3.y = buildingY;
                                            vector3_1 = vector3_3;
                                        }
                                        toolErrors2 = toolErrors3;
                                        break;
                                    }
                                }
                            }
                            if (toolErrors2 != ToolBase.ToolErrors.None)
                            {
                                float minY;
                                float maxY;
                                float buildingY;
                                Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                                this.m_toolController.ResetColliding();
                                toolErrors2 = CheckSpace(this, info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                    toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                vector3_1.y = buildingY;
                            }
                        }
                        toolErrors1 = toolErrors2 | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1);
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
                    {
                        ToolBase.ToolErrors toolErrors2 = ToolBase.ToolErrors.ShoreNotFound;
                        Vector3 position;
                        Vector3 direction;
                        if (Singleton<TerrainManager>.instance.GetShorePos(vector3_1, 50f, out position, out direction, out waterHeight))
                        {
                            vector3_1 = position;
                            if (Singleton<TerrainManager>.instance.GetShorePos(vector3_1, 50f, out position, out direction, out waterHeight))
                            {
                                position += direction.normalized * info.m_placementOffset;
                                vector3_1 = position;
                                num1 = Mathf.Atan2(direction.x, -direction.z);
                                float minY;
                                float maxY;
                                float buildingY;
                                Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                                minY = Mathf.Min(waterHeight, minY);
                                float num2 = Mathf.Max(vector3_1.y, buildingY);
                                float num3 = vector3_1.y;
                                vector3_1.y = num2;
                                toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | CheckSpace(this, info, relocating, vector3_1, minY, num2 + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)num3 - (double)waterHeight > 128.0)
                                    toolErrors1 |= ToolBase.ToolErrors.HeightTooHigh;
                                if ((double)num2 <= (double)waterHeight)
                                    toolErrors1 |= ToolBase.ToolErrors.ShoreNotFound;
                            }
                            else
                                toolErrors1 = toolErrors2 | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1);
                        }
                        else
                            toolErrors1 = toolErrors2 | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1);
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnSurface)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | CheckSpace(this, info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | CheckSpace(this, info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                            toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * (float)(Math.PI / 180.0);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | CheckSpace(this, info, relocating, vector3_1, minY, buildingY + info.m_size.y, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                    }
                    else
                        toolErrors1 = ToolBase.ToolErrors.Pending | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1);
                    if (info.m_subBuildings != null && info.m_subBuildings.Length != 0)
                    {
                        Matrix4x4 matrix4x4 = new Matrix4x4();
                        matrix4x4.SetTRS(vector3_1, Quaternion.AngleAxis(num1 * 57.29578f, Vector3.down), Vector3.one);
                        for (int index = 0; index < info.m_subBuildings.Length; ++index)
                        {
                            BuildingInfo buildingInfo = info.m_subBuildings[index].m_buildingInfo;
                            Vector3 position = matrix4x4.MultiplyPoint(info.m_subBuildings[index].m_position);
                            float angle = info.m_subBuildings[index].m_angle * (float)(Math.PI / 180.0) + (float)cachedAngleField.GetValue(this);
                            Segment3 connectionSegment2 = new Segment3();
                            int productionRate2;
                            int constructionCost2;
                            toolErrors1 |= buildingInfo.m_buildingAI.CheckBuildPosition((ushort)relocating, ref position, ref angle, waterHeight, elevation, ref connectionSegment2, out productionRate2, out constructionCost2);
                            constructionCost1 += constructionCost2;
                        }
                    }
                    if (flag && Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, constructionCost1) != constructionCost1)
                        toolErrors1 |= ToolBase.ToolErrors.NotEnoughMoney;
                    if (!Singleton<BuildingManager>.instance.CheckLimits())
                        toolErrors1 |= ToolBase.ToolErrors.TooManyObjects;
                    mousePositionField.SetValue(this, vector3_1);
                    mouseAngleField.SetValue(this, num1);
                    connectionSegmentField.SetValue(this, connectionSegment1);
                    productionRateField.SetValue(this, productionRate1);
                    constructionCostField.SetValue(this, constructionCost1);
                    placementErrorsField.SetValue(this, toolErrors1);
                }
                else
                {
                    placementErrorsField.SetValue(this, ToolBase.ToolErrors.RaycastFailed);
                    connectionSegmentField.SetValue(this, new Segment3());
                }
            }
            finally
            {
                this.m_toolController.EndColliding();
            }
        }
    }
}
