using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EightyOne.Areas;
using EightyOne.Attributes;
using EightyOne.ResourceManagers;
using EightyOne.Terrain;
using EightyOne.Zones;

namespace EightyOne
{
    public static class Detours
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        public static bool IsEnabled;

        public static void Redirect()
        {
            if (IsEnabled)
            {
                return;
            }
            IsEnabled = true;
            FakeWaterManager.Init();
            FakeDistrictManager.Init();
            FakeDistrictTool.Init();
            FakeImmaterialResourceManager.Init();
            FakeTerrainManager.Init();
            FakeZoneManager.Init();
            FakeZoneTool.Init();
            FakeElectricityManager.Init();
            redirects = new Dictionary<MethodInfo, RedirectCallsState>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var customAttributes = type.GetCustomAttributes(typeof(TargetType), false);
                if (customAttributes.Length != 1)
                {
                    continue;
                }
                var targetType = ((TargetType)customAttributes[0]).Type;
                RedirectType(type, targetType);

            }
            FakeGameAreaManager.Init();
        }

        private static void RedirectType(Type type, Type targetType)
        {
            foreach (
                var method in
                    type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Where(method => method.GetCustomAttributes(typeof (ReplaceMethodAttribute), false).Length == 1))
            {
                RedirectMethod(targetType, method);
            }
        }

        private static void RedirectMethod(Type targetType, MethodInfo detour)
        {
            var parameters = detour.GetParameters();

            Type[] types;
            if (parameters.Length > 0 && (parameters[0].ParameterType == targetType || parameters[0].ParameterType == targetType.MakeByRefType()))
                types = parameters.Skip(1).Select(p => p.ParameterType).ToArray();
            else
                types = parameters.Select(p => p.ParameterType).ToArray();

            var originalMethod = targetType.GetMethod(detour.Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static, null, types, null);

            redirects.Add(originalMethod, RedirectionHelper.RedirectCalls(originalMethod, detour));
        }

        public static void Revert()
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
    }
}