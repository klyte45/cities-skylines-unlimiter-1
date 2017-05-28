using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

namespace EightyOne.Zones
{
    [TargetType(typeof(BuildingTool))]
    internal class FakeBuildingTool : BuildingTool
    {

        private bool m_mouseRayValid => (bool)typeof(BuildingTool)
            .GetField("m_mouseRayValid", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(this);

        private Ray m_mouseRay => (Ray)typeof(BuildingTool)
            .GetField("m_mouseRay", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(this);

        private float m_mouseRayLength => (float)typeof(BuildingTool)
            .GetField("m_mouseRayLength", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(this);

        private float m_mouseAngle { 
            set
            { 
                typeof(BuildingTool)
                .GetField("m_mouseAngle", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, value);
            }
            get
            {
               return (float)typeof(BuildingTool)
                    .GetField("m_mouseAngle", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
            }
        }

        private Segment3 m_connectionSegment
        {
            set
            {
                typeof(BuildingTool)
                    .GetField("m_connectionSegment", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, value);
            }
        }

        private Vector3 m_mousePosition
        {
            set
            {
                typeof(BuildingTool).GetField("m_mousePosition", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, value);
            }
        }

        private ToolBase.ToolErrors m_placementErrors
        {
            set
            {
                typeof(BuildingTool).GetField("m_placementErrors", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, value);
            }
        }

        private int m_productionRate
        {
            set
            {
                typeof(BuildingTool).GetField("m_productionRate", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, value);
            }
        }

        private int m_constructionCost
        {
            set
            {
                typeof(BuildingTool).GetField("m_constructionCost", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, value);
            }
        }

        [RedirectReverse(true)]
        private float GetElevation(BuildingInfo info)
        {
            UnityEngine.Debug.Log($"{info}");
            return 0.0f;
        }

        [RedirectReverse(true)]
        private void GetPrefabInfo(out BuildingInfo info, out int relocating)
        {
            info = null;
            relocating = 0;
            UnityEngine.Debug.Log($"{info}-{relocating}");
        }

        [RedirectReverse(true)]
        private void FindClosestZone(BuildingInfo info, ushort block, Vector3 refPos, ref float minD, ref float min2, ref Vector3 minPos, ref float minAngle)
        {
            UnityEngine.Debug.Log($"{info}-{block}-{refPos}-{minD}-{min2}-{minPos}-{minAngle}");
        }
        
        [RedirectMethod]
        public override void SimulationStep()
        {
            BuildingInfo info;
            int relocating;
            this.GetPrefabInfo(out info, out relocating);
            if (info == null)
                return;
            ulong[] collidingSegments;
            ulong[] collidingBuildings;
            this.m_toolController.BeginColliding(out collidingSegments, out collidingBuildings);
            try
            {
                ToolBase.RaycastOutput output;
                if (m_mouseRayValid && ToolBase.RayCast(new ToolBase.RaycastInput(this.m_mouseRay, this.m_mouseRayLength), out output))
                {
                    Vector3 vector3_1 = output.m_hitPos;
                    float num1 = this.m_mouseAngle;
                    bool flag = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None;
                    float waterHeight = 0.0f;
                    Segment3 connectionSegment1 = new Segment3();
                    float elevation = this.GetElevation(info);
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
                                ushort nextGridBlock = instance.m_zoneGrid[index1 * FakeZoneManager.GRIDSIZE + index2];
                                //end mod
                                int num10 = 0;
                                while ((int)nextGridBlock != 0)
                                {
                                    Vector3 position = instance.m_blocks.m_buffer[(int)nextGridBlock].m_position;
                                    if ((double)Mathf.Max(Mathf.Max(num2 - 46f - position.x, num3 - 46f - position.z), Mathf.Max((float)((double)position.x - (double)num4 - 46.0), (float)((double)position.z - (double)num5 - 46.0))) < 0.0)
                                        this.FindClosestZone(info, nextGridBlock, output.m_hitPos, ref minD, ref min2, ref vector3_1, ref num1);
                                    nextGridBlock = instance.m_blocks.m_buffer[(int)nextGridBlock].m_nextGridBlock;
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
                                ToolBase.ToolErrors toolErrors3 = BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                        ToolBase.ToolErrors toolErrors3 = BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_3, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                        ToolBase.ToolErrors toolErrors3 = BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_3, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                                toolErrors2 = BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                    toolErrors2 |= ToolBase.ToolErrors.SlopeTooSteep;
                                vector3_1.y = buildingY;
                            }
                        }
                        toolErrors1 = toolErrors2 | info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1);
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline || info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround)
                    {
                        Vector3 pos;
                        Vector3 dir;
                        bool isQuay;
                        bool canal = BuildingTool.SnapToCanal(vector3_1, out pos, out dir, out isQuay, 40f, false);
                        Vector3 position;
                        Vector3 direction;
                        bool shorePos = Singleton<TerrainManager>.instance.GetShorePos(pos, 50f, out position, out direction, out waterHeight);
                        if (canal)
                        {
                            vector3_1 = pos;
                            num1 = Mathf.Atan2(dir.x, -dir.z);
                            float minY1;
                            float maxY;
                            float buildingY;
                            Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY1, out maxY, out buildingY);
                            float minY2 = minY1 - 20f;
                            float num2 = Mathf.Max(vector3_1.y, buildingY);
                            float y = vector3_1.y;
                            vector3_1.y = num2;
                            toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.Shoreline, relocating, vector3_1, minY2, num2 + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                            if ((double)y - (double)minY2 > 128.0)
                                toolErrors1 |= ToolBase.ToolErrors.HeightTooHigh;
                        }
                        else if (shorePos)
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
                                float y = vector3_1.y;
                                vector3_1.y = num2;
                                toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.Shoreline, relocating, vector3_1, minY, num2 + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                                if ((double)y - (double)waterHeight > 128.0)
                                    toolErrors1 |= ToolBase.ToolErrors.HeightTooHigh;
                                if ((double)num2 <= (double)waterHeight)
                                    toolErrors1 = toolErrors1 & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater) | ToolBase.ToolErrors.ShoreNotFound;
                            }
                            else
                                toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater) | ToolBase.ToolErrors.ShoreNotFound;
                        }
                        else if (info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround)
                        {
                            Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                            vector3_1 -= quaternion * info.m_centerOffset;
                            num1 = this.m_angle * ((float)System.Math.PI / 180f);
                            float minY;
                            float maxY;
                            float buildingY;
                            Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                            vector3_1.y = buildingY;
                            toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.OnGround, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                            if ((toolErrors1 & ToolBase.ToolErrors.CannotBuildOnWater) == ToolBase.ToolErrors.None && (double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                                toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                        }
                        else
                            toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater) | ToolBase.ToolErrors.ShoreNotFound;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnSurface || info.m_placementMode == BuildingInfo.PlacementMode.OnTerrain)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * ((float)System.Math.PI / 180f);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * ((float)System.Math.PI / 180f);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
                        if ((double)maxY - (double)minY > (double)info.m_maxHeightOffset)
                            toolErrors1 |= ToolBase.ToolErrors.SlopeTooSteep;
                    }
                    else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        Quaternion quaternion = Quaternion.AngleAxis(this.m_angle, Vector3.down);
                        vector3_1 -= quaternion * info.m_centerOffset;
                        num1 = this.m_angle * ((float)System.Math.PI / 180f);
                        float minY;
                        float maxY;
                        float buildingY;
                        Building.SampleBuildingHeight(vector3_1, num1, info.m_cellWidth, info.m_cellLength, info, out minY, out maxY, out buildingY);
                        vector3_1.y = buildingY;
                        toolErrors1 = info.m_buildingAI.CheckBuildPosition((ushort)relocating, ref vector3_1, ref num1, waterHeight, elevation, ref connectionSegment1, out productionRate1, out constructionCost1) | BuildingTool.CheckSpace(info, info.m_placementMode, relocating, vector3_1, minY, buildingY + info.m_collisionHeight, num1, info.m_cellWidth, info.m_cellLength, true, collidingSegments, collidingBuildings);
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
                            float angle = info.m_subBuildings[index].m_angle * ((float)System.Math.PI / 180f) + num1;
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
                    if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None && (UnityEngine.Object)this.m_toolController.m_editPrefabInfo != (UnityEngine.Object)null)
                    {
                        BuildingInfo editPrefabInfo = this.m_toolController.m_editPrefabInfo as BuildingInfo;
                        if ((UnityEngine.Object)editPrefabInfo != (UnityEngine.Object)null && (UnityEngine.Object)editPrefabInfo.m_buildingAI != (UnityEngine.Object)null && !(editPrefabInfo.m_buildingAI is IntersectionAI))
                            toolErrors1 = ToolBase.ToolErrors.None;
                    }
                    this.m_mousePosition = vector3_1;
                    m_mouseAngle = (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None ? this.m_angle * ((float)System.Math.PI / 180f) : num1;
                    m_connectionSegment = connectionSegment1;
                    this.m_productionRate = productionRate1;
                    this.m_constructionCost = constructionCost1;
                    this.m_placementErrors = toolErrors1;
                }
                else
                {
                    this.m_placementErrors = ToolBase.ToolErrors.RaycastFailed;
                    this.m_connectionSegment = new Segment3();
                }
            }
            finally
            {
                this.m_toolController.EndColliding();
            }
        }
    }
}
