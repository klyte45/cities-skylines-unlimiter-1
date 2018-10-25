using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace EightyOne
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (loading == null || loading.currentMode == AppMode.Game)
            {
                Detours.SetUp();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario && mode != LoadMode.LoadGame)
            {
                return;
            }
            Detours.Deploy();
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogProperties>().m_EdgeFogDistance = 2800f);
            if (Util.IsModActive(Mod.ALL_TILES_START_MOD))
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                    "81 Tiles - Incompatible mod detected",
                    $"'81 Tiles' isn't compatible with 'All Tile Start' mod!\nPlease unsubscribe that mod and use '{Mod.UNLOCK_ALL_TILES_FOR_FREE}' button in '81 Tiles' options entry!",
                false);
            }

            SimulationManager.instance.AddAction(FixNotInParkProblem);
            
        }

        private static void FixNotInParkProblem()
        {
            for (uint i = 0; i < BuildingManager.instance.m_buildings.m_size; i++)
            {
                var data = BuildingManager.instance.m_buildings.m_buffer[i];
                if (data.Info == null || !(data.Info.m_buildingAI is IndustryBuildingAI))
                {
                    continue;
                }
                DistrictManager instance = Singleton<DistrictManager>.instance;
                if ((data.m_problems & Notification.Problem.NotInIndustryArea) != Notification.Problem.None)
                {
                    byte park = instance.GetPark(data.m_position);
                    if (park != (byte) 0)
                    {
                        if (!instance.m_parks.m_buffer[(int) park].IsIndustry)
                            park = (byte) 0;
                        else
                        {
                            var industryBuildingAi = ((IndustryBuildingAI) data.Info.m_buildingAI);
                            if (industryBuildingAi.m_industryType == DistrictPark.ParkType.Industry ||
                                industryBuildingAi.m_industryType != instance.m_parks.m_buffer[(int) park].m_parkType)
                                park = (byte) 0;
                        }
                    }

                    if (park != 0)
                    {
                        BuildingManager.instance.m_buildings.m_buffer[i].m_problems = Notification.RemoveProblems(data.m_problems,
                            Notification.Problem.NotInIndustryArea);
                    }
                }
            }
        }

        public override void OnLevelUnloading()
        {
            Detours.Revert();
        }

        public override void OnReleased()
        {
            base.OnReleased();
            Detours.TearDown();
        }
    }
}
