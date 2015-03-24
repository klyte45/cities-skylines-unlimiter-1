using CitiesSkylinesDetour;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Unlimiter
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get
            {
                return "More stuffs";
            }
        }

        public string Name
        {
            get
            {
                Setup();
                return "Lavabucket ho!";
            }
        }

        internal const int MOD_SCALE = 3;
        internal const int DEFAULT_TREE_COUNT = 262144;
        internal const int MAX_TREE_COUNT = MOD_SCALE * DEFAULT_TREE_COUNT;

        private void Setup()
        {
            try
            {
                RedirectCalls(typeof(BuildingDecoration), typeof(LimitBuildingDecoration), "ClearDecorations");
                RedirectCalls(typeof(BuildingDecoration), typeof(LimitBuildingDecoration), "SaveProps");
                RedirectCalls(typeof(NaturalResourceManager), typeof(LimitNaturalResourceManager), "TreesModified");
                RedirectCalls(typeof(TreeTool), typeof(LimitTreeTool), "ApplyBrush");
                RedirectCalls(typeof(TreeManager.Data), typeof(LimitTreeManager.Data), "Serialize");
                RedirectCalls(typeof(TreeManager.Data), typeof(LimitTreeManager.Data), "Deserialize");

                foreach (var method in typeof(LimitTreeManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic))
                    RedirectCalls(typeof(TreeManager), typeof(LimitTreeManager), method.Name);
            }
            catch(Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, e.ToString());
                Debug.LogException(e);
            }
        }

        private void RedirectCalls(Type type1, Type type2, string p)
        {
            //Debug.LogFormat("{0}/{1}/{2}", type1, type2, p);
            RedirectionHelper.RedirectCalls(type1.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), type2.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Static));
        }
    }
}
