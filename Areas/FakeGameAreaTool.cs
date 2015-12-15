// Decompiled with JetBrains decompiler
// Type: EightyOne.Areas.FakeGameAreaTool
// Assembly: EightyOne, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C1C5025C-90CC-4DA3-8508-29E1E863A7F2
// Assembly location: D:\Games\Steam\steamapps\workshop\content\_255710\422554572\EightyOne.dll
// Compiler-generated code is shown

using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using EightyOne.Redirection;
using UnityEngine;

namespace EightyOne.Areas
{
    [TargetType(typeof(GameAreaTool))]
    public class FakeGameAreaTool : GameAreaTool
    {
        //TODO(earalov): validate this field in a init method
        private static FieldInfo _mouseAreaIndexField = typeof(GameAreaTool).GetField("m_mouseAreaIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        [RedirectMethod]
        protected override void OnToolGUI()
        {
            ToolController toolController = ToolsModifierControl.toolController;
            int num = (int)_mouseAreaIndexField.GetValue(this);
            if (toolController.IsInsideUI)
                return;
            Event current = Event.current;
            if (current.type != EventType.MouseDown)
                return;
            if (current.button == 0)
            {
                if (num != -1)
                {
                    if ((Object)toolController != (Object)null && (toolController.m_mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
                    {
                        //begin mod
                        //end mod
                    }
                    else
                    {
                        int x;
                        int z;
                        //begin mod
                        FakeGameAreaManager.GetTileXZ(num, out x, out z); //This method gets inlined and can't be detoured
                        //end mod
                        if (Singleton<GameAreaManager>.instance.CanUnlock(x, z) || Singleton<GameAreaManager>.instance.IsUnlocked(x, z))
                        {
                            UIInput.MouseUsed();
                            GameAreaInfoPanel.Show(num);
                        }
                        else
                            GameAreaInfoPanel.Hide();
                    }
                }
                else
                    GameAreaInfoPanel.Hide();
            }
            else if (current.button != 1)
                ;
        }
    }
}
