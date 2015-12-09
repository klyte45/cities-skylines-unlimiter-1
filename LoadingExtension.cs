using ICities;
using UnityEngine;

namespace EightyOne
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private const string CheatGameObject = "UnlockAllCheat";

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }
            var cheat = new GameObject(CheatGameObject);
            cheat.AddComponent<UnlockAllCheat>();
            Detours.Redirect();
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
        }

        public override void OnLevelUnloading()
        {
            Detours.Revert();
            var cheat = GameObject.Find(CheatGameObject);
            if (cheat != null)
            {
                Object.Destroy(cheat);
            }
        }
    }
}
