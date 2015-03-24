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

        internal const int MAX_TREE_COUNT = 1028 * 1028; //262144 * 8;
        //internal const int TREE_GRID_DIMENSION = 1080 * 1080;// 291600; 

        private void Setup()
        {
            try
            {
                RedirectCalls(typeof(BuildingDecoration), typeof(FakeBuildingDecoration), "ClearDecorations");
                RedirectCalls(typeof(BuildingDecoration), typeof(FakeBuildingDecoration), "SaveProps");
                RedirectCalls(typeof(NaturalResourceManager), typeof(FakeNaturalResourceManager), "TreesModified");
                RedirectCalls(typeof(TreeTool), typeof(FakeTreeTool), "ApplyBrush");
                RedirectCalls(typeof(TreeManager.Data), typeof(FakeTreeManager.Data), "Serialize");
                RedirectCalls(typeof(TreeManager.Data), typeof(FakeTreeManager.Data), "Deserialize");

                foreach (var method in typeof(FakeTreeManager).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public))
                    RedirectCalls(typeof(TreeManager), typeof(FakeTreeManager), method.Name);
            }
            catch(Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, e.ToString());
                Debug.LogException(e);
            }
        }

        private void RedirectCalls(Type type1, Type type2, string p)
        {
            Debug.LogFormat("{0}/{1}/{2}", type1, type2, p);
            RedirectionHelper.RedirectCalls(type1.GetMethod(p, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), type2.GetMethod(p, BindingFlags.Public | BindingFlags.Static));
        }
    }
}
