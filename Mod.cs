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
using Unlimiter.Zones;

namespace Unlimiter
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get
            {
                return "Allows you to place way more trees!";
            }
        }

        public string Name
        {
            get
            {
                return "Tree Unlimiter";
            }
        }
    }

    public class ModLoad : LoadingExtensionBase
    {
        public static bool IsEnabled;
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        public override void OnLevelLoaded(LoadMode mode)
        {
            EnableHooks();
        }

        private void EnableHooks()
        {
            if (IsEnabled)
            {
                return;
            }
            IsEnabled = true;
            FakeGameAreaManager.Init();
            FakeElectricityManager.Init();
            FakeWaterManager.Init();
            FakeDistrictManager.Init();
            FakeDistrictTool.Init();

            var toReplace = new Type[]
                {
                    //typeof(BuildingDecoration), typeof(LimitBuildingDecoration),
                    //typeof(NaturalResourceManager), typeof(LimitNaturalResourceManager),
                    //typeof(TreeTool), typeof(LimitTreeTool),
                    //typeof(TreeManager), typeof(LimitTreeManager),
                    //typeof(TreeManager.Data), typeof(LimitTreeManager.Data),
                    typeof(GameAreaManager), typeof(FakeGameAreaManager),
                    typeof(NetManager), typeof(FakeNetManager),
                    typeof(ZoneManager), typeof(FakeZoneManager),
                    typeof(BuildingTool ), typeof(FakeBuildingTool),
                    typeof(ZoneTool), typeof(FakeZoneTool),
                    typeof(PrivateBuildingAI), typeof(FakePrivateBuildingAI),
                    ////typeof(GameAreaInfoPanel), typeof(FakeGameAreaInfoPanel),
                    ////typeof(GameAreaTool), typeof(FakeGameAreaTool),
                    //typeof(TerrainManager), typeof(FakeTerrainManager)

                    typeof(ElectricityManager), typeof(FakeElectricityManager),
                    typeof(WaterManager), typeof(FakeWaterManager),
                    typeof(ImmaterialResourceManager), typeof(FakeImmaterialResourceManager),
                    typeof(DistrictManager), typeof(FakeDistrictManager),
                    typeof(DistrictTool), typeof(FakeDistrictTool),
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
            FakeGameAreaManager.UnlockAll();
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

            Debug.LogFormat("{0} ~> {1}", originalMethod.Name, method.Name);
            redirects.Add(originalMethod, RedirectionHelper.RedirectCalls(originalMethod, method));
        }

        public override void OnLevelUnloading()
        {
            DisableHooks();
        }

        private void DisableHooks()
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
        }
    }
}
