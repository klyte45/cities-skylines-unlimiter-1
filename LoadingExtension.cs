using ColossalFramework;
using ICities;
using UnityEngine;

namespace EightyOne
{
    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            Detours.SetUp();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            Detours.Deploy();
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogProperties>().m_EdgeFogDistance = 2800f);
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
