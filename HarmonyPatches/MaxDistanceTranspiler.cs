using EightyOne.Areas;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EightyOne.HarmonyPatches
{
    internal static class MaxDistanceTranspiler
    {
        internal static void Apply(Type type, string methodName, Type[] argumentTypes = null)
        {
            PatchUtil.Patch(
                new PatchUtil.MethodDefinition(type, methodName, argumentTypes: argumentTypes),
                null, null,
                new PatchUtil.MethodDefinition(typeof(MaxDistanceTranspiler), (nameof(Transpile))));
        }

        internal static void Undo(Type type, string methodName, Type[] argumentTypes = null)
        {
            PatchUtil.Unpatch(new PatchUtil.MethodDefinition(type, methodName, argumentTypes: argumentTypes));
        }


        private static IEnumerable<CodeInstruction> Transpile(MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("81 Tiles: MaxDistanceTranspiler - Transpiling method: " + original.DeclaringType + "." + original);
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            foreach (var codeInstruction in codes)
            {
                if (SkipInstruction(codeInstruction))
                {
                    newCodes.Add(codeInstruction);
                    continue;
                }
                //100m buffer, just to make sure OutsideConnections don't get treated incorrectly
                var newInstruction = new CodeInstruction(OpCodes.Ldc_R4, FakeGameAreaManager.HALFGRID * 1920.0f - 100f)
                {
                    labels = codeInstruction.labels
                }
                    ;
                newCodes.Add(newInstruction);
                Debug.Log($"81 Tiles: MaxDistanceTranspiler - Replaced distance with {newInstruction.operand}");
            }

            return newCodes.AsEnumerable();
        }

        private static bool SkipInstruction(CodeInstruction codeInstruction)
        {
            return codeInstruction.opcode != OpCodes.Ldc_R4 || (codeInstruction.operand is float f && f < 4799f && f > 4801f);
        }
    }
}