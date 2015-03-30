using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Zones
{
    internal class FakePrivateBuildingAI
    {
        [ReplaceMethod]
        [Fixme("This uses the building grid, which is ENTIRELY different? That -may- be left to fix after zones technically work")]
        public static void CheckNearbyBuildingZones(Vector3 position)
        {
            int num1 = Mathf.Max((int)(((double)position.x - 35.0) / 64.0 + 135.0), 0);
            int num2 = Mathf.Max((int)(((double)position.z - 35.0) / 64.0 + 135.0), 0);
            int num3 = Mathf.Min((int)(((double)position.x + 35.0) / 64.0 + 135.0), 269);
            int num4 = Mathf.Min((int)(((double)position.z + 35.0) / 64.0 + 135.0), 269);
            Array16<Building> array16 = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] numArray = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort building = numArray[index1 * 270 + index2];
                    int num5 = 0;
                    while ((int)building != 0)
                    {
                        ushort num6 = array16.m_buffer[(int)building].m_nextGridBuilding;
                        if ((array16.m_buffer[(int)building].m_flags & (Building.Flags.Created | Building.Flags.Deleted | Building.Flags.Demolishing)) == Building.Flags.Created)
                        {
                            BuildingInfo info = array16.m_buffer[(int)building].Info;
                            if (info != null && info.m_placementStyle == ItemClass.Placement.Automatic)
                            {
                                ItemClass.Zone zone = info.m_class.GetZone();
                                if (zone != ItemClass.Zone.None && (array16.m_buffer[(int)building].m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None && (double)VectorUtils.LengthSqrXZ(array16.m_buffer[(int)building].m_position - position) <= 1225.0)
                                {
                                    array16.m_buffer[(int)building].m_flags &= ~Building.Flags.ZonesUpdated;
                                    if (!FakeBuilding.CheckZoning(array16.m_buffer[(int)building], zone))
                                        Singleton<BuildingManager>.instance.ReleaseBuilding(building);
                                }
                            }
                        }
                        building = num6;
                        if (++num5 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        [ReplaceMethod]
        public static void SimulationStep(PrivateBuildingAI ai, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            BaseSimulationStep(ai, buildingID, ref buildingData, ref frameData);

            if ((buildingData.m_flags & Building.Flags.ZonesUpdated) != Building.Flags.None)
            {
                SimulationManager instance = Singleton<SimulationManager>.instance;
                if ((int)buildingData.m_fireIntensity != 0 || instance.m_randomizer.Int32(10U) != 0 || (int)Singleton<ZoneManager>.instance.m_lastBuildIndex != (int)instance.m_currentBuildIndex)
                    return;
                buildingData.m_flags &= ~Building.Flags.ZonesUpdated;
                if (FakeBuilding.CheckZoning(buildingData, ai.m_info.m_class.GetZone()))
                    return;
                buildingData.m_flags |= Building.Flags.Demolishing;
                CheckNearbyBuildingZones(buildingData.m_position);
                ++instance.m_currentBuildIndex;
            }
            else
            {
                if ((buildingData.m_flags & (Building.Flags.Abandoned | Building.Flags.Downgrading)) == Building.Flags.None || (int)buildingData.m_majorProblemTimer != (int)byte.MaxValue && (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
                    return;
                SimulationManager instance1 = Singleton<SimulationManager>.instance;
                ZoneManager instance2 = Singleton<ZoneManager>.instance;
                int num;
                switch (ai.m_info.m_class.m_service)
                {
                    case ItemClass.Service.Residential:
                        num = instance2.m_actualResidentialDemand;
                        break;

                    case ItemClass.Service.Commercial:
                        num = instance2.m_actualCommercialDemand;
                        break;

                    case ItemClass.Service.Industrial:
                        num = instance2.m_actualWorkplaceDemand;
                        break;

                    case ItemClass.Service.Office:
                        num = instance2.m_actualWorkplaceDemand;
                        break;

                    default:
                        num = 0;
                        break;
                }
                if (instance1.m_randomizer.Int32(100U) >= num || (int)instance2.m_lastBuildIndex != (int)instance1.m_currentBuildIndex || (double)Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(buildingData.m_position)) > (double)buildingData.m_position.y)
                    return;
                ItemClass.SubService subService = ai.m_info.m_class.m_subService;
                ItemClass.Level level = ItemClass.Level.Level1;
                if (ai.m_info.m_class.m_service == ItemClass.Service.Industrial)
                    ZoneBlock.GetIndustryType(buildingData.m_position, out subService, out level);
                int width = buildingData.Width;
                int length = buildingData.Length;
                BuildingInfo randomBuildingInfo = Singleton<BuildingManager>.instance.GetRandomBuildingInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ai.m_info.m_class.m_service, subService, level, width, length);
                if (randomBuildingInfo == null)
                    return;
                buildingData.m_flags |= Building.Flags.Demolishing;
                ushort building;
                if (Singleton<BuildingManager>.instance.CreateBuilding(out building, ref Singleton<SimulationManager>.instance.m_randomizer, randomBuildingInfo, buildingData.m_position, buildingData.m_angle, length, Singleton<SimulationManager>.instance.m_currentBuildIndex))
                {
                    ++Singleton<SimulationManager>.instance.m_currentBuildIndex;
                    switch (ai.m_info.m_class.m_service)
                    {
                        case ItemClass.Service.Residential:
                            instance2.m_actualResidentialDemand = Mathf.Max(0, instance2.m_actualResidentialDemand - 5);
                            break;

                        case ItemClass.Service.Commercial:
                            instance2.m_actualCommercialDemand = Mathf.Max(0, instance2.m_actualCommercialDemand - 5);
                            break;

                        case ItemClass.Service.Industrial:
                            instance2.m_actualWorkplaceDemand = Mathf.Max(0, instance2.m_actualWorkplaceDemand - 5);
                            break;

                        case ItemClass.Service.Office:
                            instance2.m_actualWorkplaceDemand = Mathf.Max(0, instance2.m_actualWorkplaceDemand - 5);
                            break;
                    }
                }
                ++instance1.m_currentBuildIndex;
            }
        }

        private static void BaseSimulationStep(PrivateBuildingAI ai, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            if ((buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
            {
                GuideController guideController = Singleton<GuideManager>.instance.m_properties;
                if (guideController != null)
                {
                    Singleton<BuildingManager>.instance.m_buildingAbandoned1.Activate(guideController.m_buildingAbandoned1, buildingID);
                    Singleton<BuildingManager>.instance.m_buildingAbandoned2.Activate(guideController.m_buildingAbandoned2, buildingID);
                }
                if ((int)buildingData.m_majorProblemTimer < (int)byte.MaxValue)
                    ++buildingData.m_majorProblemTimer;
                float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius);
            }
            else if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
            {
                GuideController guideController = Singleton<GuideManager>.instance.m_properties;
                if (guideController != null)
                    Singleton<BuildingManager>.instance.m_buildingBurned.Activate(guideController.m_buildingBurned, buildingID);
                float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Abandonment, 10, buildingData.m_position, radius);
            }
            else if ((buildingData.m_flags & Building.Flags.Completed) == Building.Flags.None)
            {
                bool flag = (buildingData.m_flags & Building.Flags.Upgrading) != Building.Flags.None;
                int constructionTime = ai.m_constructionTime;
                frameData.m_constructState = constructionTime != 0 ? (byte)Mathf.Min((int)byte.MaxValue, (int)frameData.m_constructState + 1088 / constructionTime) : byte.MaxValue;
                if ((int)frameData.m_constructState == (int)byte.MaxValue)
                {
                    var p = new object[] { buildingID, buildingData };
                    ai.GetType().GetMethod("BuildingCompleted", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ai, p);
                    buildingData = (Building)p[1];

                    if (Singleton<GuideManager>.instance.m_properties != null)
                        Singleton<BuildingManager>.instance.m_buildingLevelUp.Deactivate(buildingID, true);
                }
                else if (flag)
                {
                    GuideController guideController = Singleton<GuideManager>.instance.m_properties;
                    if (guideController != null)
                        Singleton<BuildingManager>.instance.m_buildingLevelUp.Activate(guideController.m_buildingLevelUp, buildingID);
                }
                if (!flag)
                    return;

                var x = new object[] { buildingID, buildingData, frameData };
                ai.GetType().GetMethod("SimulationStepActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ai, x);
                buildingData = (Building)x[1];
                frameData = (Building.Frame)x[2];
            }
            else
            {
                var x = new object[] { buildingID, buildingData, frameData };
                ai.GetType().GetMethod("SimulationStepActive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ai, x);
                buildingData = (Building)x[1];
                frameData = (Building.Frame)x[2];
            }
        }
    }
}
