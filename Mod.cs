using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Unlimiter.Areas;
using Unlimiter.Attributes;
using Unlimiter.ResourceManagers;
using Unlimiter.Terrain;
using Unlimiter.Zones;

namespace Unlimiter
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get
            {
                return "Unlocks 81 tiles";
            }
        }

        public string Name
        {
            get
            {
                return "81 Tile Unlock";
            }
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        public static Unlimiter unlimiter;
        public static GameObject gm;

        public override void OnLevelLoaded(LoadMode mode)
        {
            gm = new GameObject("unlimiter");
            unlimiter = gm.AddComponent<Unlimiter>();
            unlimiter.EnableHooks();
        }

        public override void OnLevelUnloading()
        {
            unlimiter.DisableHooks();
            GameObject.Destroy(gm);
        }

    }

    public class Unlimiter : MonoBehaviour
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        public static bool IsEnabled;

        public void EnableHooks()
        {
            if (IsEnabled)
            {
                return;
            }
            IsEnabled = true;
            FakeElectricityManager.Init();
            FakeWaterManager.Init();
            FakeDistrictManager.Init();
            FakeDistrictTool.Init();
            FakeImmaterialResourceManager.Init();
            FakeTerrainManager.Init();
            FakeZoneManager.Init();
            FakeNetManager.Init();
            FakeZoneTool.Init();

            var toReplace = new Type[]
                {
                    typeof(GameAreaManager), typeof(FakeGameAreaManager),
                    typeof(GameAreaInfoPanel), typeof(FakeGameAreaInfoPanel),
                    typeof(GameAreaTool), typeof(FakeGameAreaTool),

                    typeof(NetManager), typeof(FakeNetManager),
                    typeof(ZoneManager), typeof(FakeZoneManager),
                    typeof(BuildingTool ), typeof(FakeBuildingTool),
                    typeof(Building ), typeof(FakeBuilding),

                    typeof(ZoneTool), typeof(FakeZoneTool),
                    //typeof(PrivateBuildingAI), typeof(FakePrivateBuildingAI),
                    typeof(TerrainManager), typeof(FakeTerrainManager),

                    typeof(ElectricityManager), typeof(FakeElectricityManager),
                    typeof(WaterManager), typeof(FakeWaterManager),
                    typeof(ImmaterialResourceManager), typeof(FakeImmaterialResourceManager),
                    typeof(DistrictManager), typeof(FakeDistrictManager),
                    typeof(DistrictTool), typeof(FakeDistrictTool),
                   typeof(NaturalResourceManager), typeof(FakeNatualResourceManager),
                };

            redirects = new Dictionary<MethodInfo, RedirectCallsState>();
            for (int i = 0; i < toReplace.Length; i += 2)
            {
                var from = toReplace[i];
                var to = toReplace[i + 1];

                foreach (var method in to.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (method.GetCustomAttributes(typeof(ReplaceMethodAttribute), false).Length == 1)
                    {
                        AddRedirect(from, method);
                    }
                }
            }
            FakeGameAreaManager.Init();
        }

        private void AddRedirect(Type type1, MethodInfo method)
        {
            var parameters = method.GetParameters();

            Type[] types;
            if (parameters.Length > 0 && parameters[0].ParameterType == type1)
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            else
                types = parameters.Select(p => p.ParameterType).ToArray();

            var originalMethod = type1.GetMethod(method.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, types, null);
            if (originalMethod == null)
            {
                Debug.Log("Cannot find " + method.Name);
            }
            redirects.Add(originalMethod, RedirectionHelper.RedirectCalls(originalMethod, method));
        }

        public void DisableHooks()
        {
            if (!IsEnabled)
            {
                return;
            }
            IsEnabled = false;
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            FakeImmaterialResourceManager.OnDestroy();
            FakeDistrictManager.OnDestroy();
            FakeWaterManager.OnDestroy();
            FakeElectricityManager.OnDestroy();
            FakeGameAreaManager.OnDestroy();
        }

        public void Update()
        {
            if ((Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) && Input.GetKeyDown(KeyCode.U))
            {
                FakeGameAreaManager.UnlockAll();
            }
        }
    }
}
