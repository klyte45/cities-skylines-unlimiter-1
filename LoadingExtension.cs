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
            if (Util.IsModActive(Mod.ALL_TILES_START_MOD))
            {
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(
                    "81 Tiles - Incompatible mod detected",
                    $"'81 Tiles' isn't compatible with 'All Tile Start' mod!\nPlease unsubscribe that mod and use '{Mod.UNLOCK_ALL_TILES_FOR_FREE}' button in '81 Tiles' options entry!",
                false);
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
