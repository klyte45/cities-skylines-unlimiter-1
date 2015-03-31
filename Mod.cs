using CitiesSkylinesDetour;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Unlimiter.Areas;
using Unlimiter.Attributes;
using Unlimiter.Terrain;
using Unlimiter.Trees;
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
                Setup();
                return "Tree Unlimiter";
            }
        }

        public static bool IsEnabled;

        internal const int MOD_TREE_SCALE = 4;
        internal const int DEFAULT_TREE_COUNT = 262144;

        private void Setup()
        {
            try
            {
                var toReplace = new Type[]
                {
                    typeof(BuildingDecoration), typeof(LimitBuildingDecoration),
                    typeof(NaturalResourceManager), typeof(LimitNaturalResourceManager),
                    typeof(TreeTool), typeof(LimitTreeTool),
                    typeof(TreeManager), typeof(LimitTreeManager),
                    typeof(TreeManager.Data), typeof(LimitTreeManager.Data),
                    typeof(GameAreaManager), typeof(FakeGameAreaManager),
                    typeof(GameAreaManager.Data), typeof(FakeGameAreaManager.Data),
                    typeof(NetManager), typeof(FakeNetManager),
                    typeof(ZoneManager), typeof(FakeZoneManager),
                    typeof(ZoneManager.Data), typeof(FakeZoneManager.Data),
                    typeof(ZoneTool), typeof(FakeZoneTool),
                    typeof(PrivateBuildingAI), typeof(FakePrivateBuildingAI),
                    typeof(GameAreaInfoPanel), typeof(FakeGameAreaInfoPanel),
                    typeof(GameAreaTool), typeof(FakeGameAreaTool),
                    typeof(TerrainManager), typeof(FakeTerrainManager)
                };

                for (int i = 0; i < toReplace.Length; i += 2)
                {
                    var from = toReplace[i];
                    var to = toReplace[i + 1];

                    foreach (var method in to.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (method.GetCustomAttributes(typeof(ReplaceMethodAttribute), false).Length == 1)
                            RedirectCalls(from, method);
                    }
                }

                // We should probably check if we're enabled
                PluginsChanged();
                PluginManager.instance.eventPluginsChanged += PluginsChanged;
                PluginManager.instance.eventPluginsStateChanged += PluginsChanged;
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, e.ToString());
                Debug.LogException(e);
            }
        }

        private void PluginsChanged()
        {
            try
            {
                PluginManager.PluginInfo pi = PluginManager.instance.GetPluginsInfo().Where(p => p.publishedFileID.AsUInt64 == 413502249L).FirstOrDefault();
                if (pi != null)
                {
                    IsEnabled = pi.isEnabled;
                }
                else
                {
                    IsEnabled = false;
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "[TreeLimit] Can't find self. No idea if this mod is enabled.");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "[TreeLimit] " + e.GetType() + ": " + e.Message);
            }
        }

        private void RedirectCalls(Type type1, MethodInfo method)
        {
            Debug.LogFormat("{0} ~> {1}", type1, method);
            var parameters = method.GetParameters();

            Type[] types;
            if (parameters.Length > 0 && parameters[0].ParameterType == type1)
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            else
                types = parameters.Select(p => p.ParameterType).ToArray();

            RedirectionHelper.RedirectCalls(type1.GetMethod(method.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, types, null), method);
        }
    }

    public class DataExtension : SerializableDataExtensionBase
    {
        public override void OnLoadData()
        {
            // Deserialize is called from within LimitTreeManager.Data.Deserialize
        }

        public override void OnSaveData()
        {
            LimitTreeManager.CustomSerializer.Serialize();
        }
    }
}
