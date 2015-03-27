using CitiesSkylinesDetour;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Unlimiter.Areas;
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
                // All of these should behave identical to the vanilla variant if the mod is not enabled. Probably. Somewhat.
                RedirectCalls(typeof(BuildingDecoration), typeof(LimitBuildingDecoration), "ClearDecorations");
                RedirectCalls(typeof(BuildingDecoration), typeof(LimitBuildingDecoration), "SaveProps");
                RedirectCalls(typeof(NaturalResourceManager), typeof(LimitNaturalResourceManager), "TreesModified");
                //RedirectCalls(typeof(NaturalResourceManager), typeof(LimitNaturalResourceManager), "GetTileResources");
                RedirectCalls(typeof(TreeTool), typeof(LimitTreeTool), "ApplyBrush");
                RedirectCalls(typeof(TreeManager.Data), typeof(LimitTreeManager.Data), "Serialize");
                RedirectCalls(typeof(TreeManager.Data), typeof(LimitTreeManager.Data), "Deserialize");
                //RedirectCalls(typeof(TreeInstance), typeof(RotatingTreeInstance), "RenderLod");

                Debug.Log("RenderInstance");
                RedirectionHelper.RedirectCalls(typeof(TreeInstance).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static),
                    typeof(RotatingTreeInstance).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Static));

                foreach (var method in typeof(LimitTreeManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(TreeManager), method);

                foreach (var method in typeof(FakeGameAreaManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(GameAreaManager), method);
                Debug.Log("CalculateTilePrice");
                RedirectionHelper.RedirectCalls(typeof(GameAreaManager).GetMethod("CalculateTilePrice", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, new Type[] { typeof(int) }, null), typeof(FakeGameAreaManager).GetMethod("CalculateTilePrice", BindingFlags.Public | BindingFlags.Static));
                
                foreach (var method in typeof(FakeNetManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(NetManager), method);

                foreach (var method in typeof(FakeZoneManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(ZoneManager), method);
                foreach (var method in typeof(FakeZoneManager.Data).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(ZoneManager.Data), method);
                foreach (var method in typeof(FakeBuilding).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(Building), method);
#if false
                foreach (var method in typeof(FakeZoneBlock).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(ZoneBlock), method);
#endif

                foreach (var method in typeof(FakeZoneTool).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(ZoneTool), method);
#if false
                Debug.Log("CheckSpace");
                RedirectionHelper.RedirectCalls(typeof(ZoneManager).GetMethod("CheckSpace", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Vector3), typeof(float), typeof(int), typeof(int), typeof(int).MakeByRefType() }, null), typeof(FakeZoneManager).GetMethod("CheckSpace", BindingFlags.Public | BindingFlags.Static));
#endif

                RedirectCalls(typeof(GameAreaTool), typeof(FakeGameAreaTool), "OnToolGUI");
                RedirectCalls(typeof(GameAreaInfoPanel), typeof(FakeGameAreaInfoPanel), "ShowInternal");
                RedirectCalls(typeof(GameAreaInfoPanel), typeof(FakeGameAreaInfoPanel), "UpdatePanel");
                
                // We should probably check if we're enabled
                PluginsChanged();
                PluginManager.instance.eventPluginsChanged += PluginsChanged;
                PluginManager.instance.eventPluginsStateChanged += PluginsChanged;
            }
            catch(Exception e)
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

        private void RedirectCalls(Type type1, Type type2, string p)
        {
            Debug.LogFormat("{0}/{1}/{2}", type1, type2, p);
            RedirectionHelper.RedirectCalls(type1.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), type2.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Static));
        }

        private void RedirectCalls(Type type1, MethodInfo method)
        {
            Debug.LogFormat("{0} ~> {1}", type1, method);
            var parameters = method.GetParameters();

            Type[] types;
            if (parameters[0].ParameterType == type1)
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            else
                types = parameters.Select(p => p.ParameterType).ToArray();

            RedirectionHelper.RedirectCalls(type1.GetMethod(method.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, types, null), method);
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
