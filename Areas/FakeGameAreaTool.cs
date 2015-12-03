using ColossalFramework;
using ColossalFramework.UI;
using EightyOne.Attributes;
using System.Reflection;
using UnityEngine;

namespace EightyOne.Areas
{
  public class FakeGameAreaTool
  {
    [ReplaceMethod]
    protected static void OnToolGUI(GameAreaTool t)
    {
      ToolController toolController = (ToolController) ((object) t).GetType().GetField("m_toolController", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) t);
      int tile = (int) ((object) t).GetType().GetField("m_mouseAreaIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) t);
      if (toolController.IsInsideUI)
        return;
        Event current = Event.current;
      if (current.type != 0)
        return;
      if (current.button == 0)
      {
        if (tile != -1)
        {
          if (toolController != null && toolController.m_mode!=ItemClass.Availability.MapEditor)
          {
            Singleton<SimulationManager>.instance.AddAction(t.UnlockArea(tile));
          }
          else
          {
            int x;
            int z;
            FakeGameAreaManager.GetTileXZ(Singleton<GameAreaManager>.instance, tile, out x, out z);
            if (Singleton<GameAreaManager>.instance.CanUnlock(x, z) || Singleton<GameAreaManager>.instance.IsUnlocked(x, z))
            {
              UIInput.MouseUsed();
              GameAreaInfoPanel.Show(tile);
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
