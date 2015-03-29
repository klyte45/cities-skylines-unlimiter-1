using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unlimiter.Attributes;

namespace Unlimiter.Areas
{
    class FakeGameAreaTool
    {
        [ReplaceMethod]
        public static void OnToolGUI(GameAreaTool g)
        {
            ToolController m_toolController = (ToolController) g.GetType().GetField("m_toolController", System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(g);

            if (m_toolController.IsInsideUI)
                return;
            UnityEngine.Event current = UnityEngine.Event.current;
            if (current.type != EventType.MouseDown)
                return;
            if (current.button == 0)
            {
                int m_mouseAreaIndex = (int) g.GetType().GetField("m_mouseAreaIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(g);
                if (m_mouseAreaIndex != -1)
                {
                    if (m_toolController != null && (m_toolController.m_mode & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
                    {
                        Singleton<SimulationManager>.instance.AddAction(g.UnlockArea(m_mouseAreaIndex));
                    }
                    else
                    {
                        int x;
                        int z;
                        // tweaked: Singleton<GameAreaManager>.instance.GetTileXZ(m_mouseAreaIndex, out x, out z);
                        FakeGameAreaManager.GetTileXZ(GameAreaManager.instance, m_mouseAreaIndex, out x, out z);
                        if (Singleton<GameAreaManager>.instance.CanUnlock(x, z) || Singleton<GameAreaManager>.instance.IsUnlocked(x, z))
                        {
                            UIInput.MouseUsed();
                            GameAreaInfoPanel.Show(m_mouseAreaIndex);
                        }
                        else
                            GameAreaInfoPanel.Hide();
                    }
                }
                else
                    GameAreaInfoPanel.Hide();
            }
            else if (current.button == 1)
            { }
        }
    }
}
