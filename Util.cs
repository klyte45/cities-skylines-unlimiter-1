using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace EightyOne
{
    internal static class Util
    {
        private const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static void CopyArray(IList newArray, object em, string propertyName)
        {
            var oldArray = (IList)em.GetType().GetField(propertyName, AllFlags).GetValue(em);
            for (var i = 0; i < newArray.Count; i += 1)
            {
                newArray[i] = oldArray[i];
            }
        }

        public static void CopyArrayBack(IList newArray, object em, string propertyName)
        {
            var oldArray = (IList)em.GetType().GetField(propertyName, AllFlags).GetValue(em);
            for (var i = 0; i < newArray.Count; i += 1)
            {
                oldArray[i] = newArray[i];
            }
        }

        public static void CopyStructArray(IList newArray, object em, string propertyName)
        {
            var oldArray = (IList)em.GetType().GetField(propertyName, AllFlags).GetValue(em);
            var fields = GetFieldsFromStruct(newArray[0], oldArray[0]);
            for (var i = 0; i < newArray.Count; i += 1)
            {
                newArray[i] = CopyStruct((object)newArray[0], oldArray[i], fields);
            }
        }

        public static void CopyStructArrayBack(IList newArray, object em, string propertyName)
        {
            var oldArray = (IList)em.GetType().GetField(propertyName, AllFlags).GetValue(em);
            var fields = GetFieldsFromStruct(oldArray[0], newArray[0]);
            for (var i = 0; i < newArray.Count; i += 1)
            {
                oldArray[i] = CopyStruct((object)oldArray[i], newArray[i], fields);
            }
        }


        public static Dictionary<FieldInfo, FieldInfo> GetFieldsFromStruct(object newArray, object oldArray)
        {
            var fields = new Dictionary<FieldInfo, FieldInfo>();
            foreach (var f in oldArray.GetType().GetFields(AllFlags))
            {
                fields.Add(newArray.GetType().GetField(f.Name, AllFlags), f);
            }
            return fields;
        }

        public static void SetPropertyValue<T>(ref T result, object obj, string propName)
        {
            result = (T)obj.GetType().GetField(propName, AllFlags).GetValue(obj);
        }

        public static void SetPropertyValueBack(object result, object obj, string propName)
        {
            obj.GetType().GetField(propName, AllFlags).SetValue(obj, result);
        }

        public static object CopyStruct(object newObj, object original, Dictionary<FieldInfo, FieldInfo> fields)
        {
            foreach (var field in fields)
            {
                if (field.Key.FieldType != field.Value.FieldType)
                {
                    if (field.Key.FieldType == typeof(byte))
                    {
                        var oo = Mathf.Clamp((ushort)field.Value.GetValue(original), 0, 255);
                        field.Key.SetValue(newObj, (byte)oo);
                        continue;
                    }
                }
                field.Key.SetValue(newObj, field.Value.GetValue(original));
            }
            return newObj;
        }

        public static bool IsModActive(ulong modId)
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            return plugins.Any(p => p != null && p.isEnabled && p.publishedFileID.AsUInt64 == modId);
        }

        public static bool IsModActive(string modName)
        {
            var plugins = PluginManager.instance.GetPluginsInfo();
            return (from plugin in plugins.Where(p => p != null && p.isEnabled)
                    select plugin.GetInstances<IUserMod>() into instances
                    where instances.Any()
                    select instances[0].Name into name
                    where name == modName
                    select name).Any();
        }

        public static Type FindType(string className)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types.Where(type => type.Name == className))
                    {
                        return type;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            return null;
        }

        public static bool IsGameMode()
        {
            return ToolManager.instance.m_properties.m_mode == ItemClass.Availability.Game;
        }

    }
}