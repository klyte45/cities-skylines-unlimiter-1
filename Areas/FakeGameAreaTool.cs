using ColossalFramework;
using ColossalFramework.UI;
using System.Reflection;
using EightyOne.RedirectionFramework.Attributes;
using UnityEngine;

namespace EightyOne.Areas
{
    [TargetType(typeof(GameAreaTool))]
    public class FakeGameAreaTool : GameAreaTool
    {
        //TODO(earalov): validate this field in a init method
        private static FieldInfo _mouseAreaIndexField = typeof(GameAreaTool).GetField("m_mouseAreaIndex", BindingFlags.Instance | BindingFlags.NonPublic);

        [RedirectMethod]
        protected override void OnToolGUI(Event e)
        {
            ToolController toolController = ToolsModifierControl.toolController;
            int num = (int)_mouseAreaIndexField.GetValue(this);
            if (toolController.IsInsideUI)
                return;
            if (e.type != EventType.MouseDown)
                return;
            if (e.button == 0)
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
            else if (e.button != 1)
                ;
        }
    }
}
