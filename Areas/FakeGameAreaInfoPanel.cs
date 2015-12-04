using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System;
using System.Reflection;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Areas
{
    [TargetType(typeof(GameAreaInfoPanel))]
    internal class FakeGameAreaInfoPanel
    {
        private static FieldInfo m_AreaIndex;
        private static UIComponent m_FullscreenContainer;
        private static UIProgressBar m_OilResources;
        private static UIProgressBar m_OreResources;
        private static UIProgressBar m_ForestryResources;
        private static UIProgressBar m_FertilityResources;
        private static UISprite m_OilNoResources;
        private static UISprite m_OreNoResources;
        private static UISprite m_ForestryNoResources;
        private static UISprite m_FertilityNoResources;
        private static UILabel m_Title;
        private static UISprite m_Water;
        private static UISprite m_NoWater;
        private static UISprite m_Highway;
        private static UISprite m_NoHighway;
        private static UISprite m_InHighway;
        private static UISprite m_OutHighway;
        private static UISprite m_Train;
        private static UISprite m_NoTrain;
        private static UISprite m_InTrain;
        private static UISprite m_OutTrain;
        private static UISprite m_Ship;
        private static UISprite m_NoShip;
        private static UISprite m_InShip;
        private static UISprite m_OutShip;
        private static UISprite m_Plane;
        private static UISprite m_NoPlane;
        private static UISprite m_InPlane;
        private static UISprite m_OutPlane;
        private static UILabel m_BuildableArea;
        private static UILabel m_Price;
        private static UIPanel m_PurchasePanel;

        public static void Init(GameAreaInfoPanel g)
        {
            m_AreaIndex = g.GetType().GetField("m_AreaIndex", BindingFlags.NonPublic | BindingFlags.Instance);
           m_FullscreenContainer = UIView.Find("FullScreenContainer");
           m_Title = g.Find<UILabel>("Title");
           m_BuildableArea = g.Find<UILabel>("BuildableArea");
           m_Price = g.Find<UILabel>("Price");
           m_PurchasePanel = g.Find<UIPanel>("PurchasePanel");
           m_OilResources = g.Find<UIProgressBar>("ResourceBarOil");
           m_OreResources = g.Find<UIProgressBar>("ResourceBarOre");
           m_ForestryResources = g.Find<UIProgressBar>("ResourceBarForestry");
           m_FertilityResources = g.Find<UIProgressBar>("ResourceBarFarming");
           m_OilNoResources = g.Find("ResourceOil").Find<UISprite>("NoNoNo");
           m_OreNoResources = g.Find("ResourceOre").Find<UISprite>("NoNoNo");
           m_ForestryNoResources = g.Find("ResourceForestry").Find<UISprite>("NoNoNo");
           m_FertilityNoResources = g.Find("ResourceFarming").Find<UISprite>("NoNoNo");
           m_Water = g.Find<UISprite>("Water");
           m_NoWater =m_Water.Find<UISprite>("NoNoNo");
           m_Highway = g.Find<UISprite>("Highway");
           m_NoHighway =m_Highway.Find<UISprite>("NoNoNo");
           m_InHighway =m_Highway.Find<UISprite>("Incoming");
           m_OutHighway =m_Highway.Find<UISprite>("Outgoing");
           m_Train = g.Find<UISprite>("Train");
           m_NoTrain =m_Train.Find<UISprite>("NoNoNo");
           m_InTrain =m_Train.Find<UISprite>("Incoming");
           m_OutTrain =m_Train.Find<UISprite>("Outgoing");
           m_Ship = g.Find<UISprite>("Ship");
           m_NoShip =m_Ship.Find<UISprite>("NoNoNo");
           m_InShip =m_Ship.Find<UISprite>("Incoming");
           m_OutShip =m_Ship.Find<UISprite>("Outgoing");
           m_Plane = g.Find<UISprite>("Plane");
           m_NoPlane =m_Plane.Find<UISprite>("NoNoNo");
           m_InPlane =m_Plane.Find<UISprite>("Incoming");
           m_OutPlane = m_Plane.Find<UISprite>("Outgoing");
        }

        [ReplaceMethod]
        private static void ShowInternal(GameAreaInfoPanel g, int areaIndex)
        {
            if (m_AreaIndex == null)
            {
                Init(g);
            }
            m_AreaIndex.SetValue(g, areaIndex);
            int x;
            int z;
            FakeGameAreaManager.GetTileXZ(GameAreaManager.instance, areaIndex, out x, out z);
            uint num;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            Singleton<NaturalResourceManager>.instance.GetTileResources(x, z, out num, out num2, out num3, out num4, out num5);
            float num6 = 3686400f;
            float num7 = 1139.0625f;
            float num8 = num6 / num7 * 255f;
            float endValue = Mathf.Pow(num2 / num8, g.m_OilExponent);
            float endValue2 = Mathf.Pow(num / num8, g.m_OreExponent);
            float endValue3 = Mathf.Pow(num3 / num8, g.m_ForestryExponent);
            float endValue4 = Mathf.Pow(num4 / num8, g.m_FarmingExponent);
            ValueAnimator.Cancel("Oil");
            ValueAnimator.Cancel("Ore");
            ValueAnimator.Cancel("Forest");
            ValueAnimator.Cancel("Fertility");
            m_OilResources.value = 0f;
            m_OreResources.value = 0f;
            m_ForestryResources.value = 0f;
            m_FertilityResources.value = 0f;
            ValueAnimator.Animate("Oil", delegate(float val)
            {
                m_OilResources.value = val;
            }, new AnimatedFloat(0f, endValue, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Ore", delegate(float val)
            {
                m_OreResources.value = val;
            }, new AnimatedFloat(0f, endValue2, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Forest", delegate(float val)
            {
                m_ForestryResources.value = val;
            }, new AnimatedFloat(0f, endValue3, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Fertility", delegate(float val)
            {
                m_FertilityResources.value = val;
            }, new AnimatedFloat(0f, endValue4, g.m_InterpolationTime, g.m_InterpolationEasingType));
            UpdatePanel(g);
        }

        [ReplaceMethod]
        private static void UpdatePanel(GameAreaInfoPanel g)
        {
            if (m_AreaIndex == null)
            {
                Init(g);
            }
            var areaIndex = (int)m_AreaIndex.GetValue(g);
            if (areaIndex != -1)
            {
                int x;
                int z;
                FakeGameAreaManager.GetTileXZ(GameAreaManager.instance,areaIndex, out x, out z);
                Vector3 areaPositionSmooth = Singleton<GameAreaManager>.instance.GetAreaPositionSmooth(x, z);
                Vector3 vector = Camera.main.WorldToScreenPoint(areaPositionSmooth);
                UIView uIView = g.component.GetUIView();
                Vector2 vector2 = (!(m_FullscreenContainer != null)) ? uIView.GetScreenResolution() : m_FullscreenContainer.size;
                vector /= uIView.inputScale;
                Vector3 vector3 = g.component.pivot.UpperLeftToTransform(g.component.size, g.component.arbitraryPivotOffset);
                Vector3 relativePosition = uIView.ScreenPointToGUI(vector) + new Vector2(vector3.x, vector3.y);
                if (relativePosition.x < 0f)
                {
                    relativePosition.x = 0f;
                }
                if (relativePosition.y < 0f)
                {
                    relativePosition.y = 0f;
                }
                if (relativePosition.x + g.component.width > vector2.x)
                {
                    relativePosition.x = vector2.x - g.component.width;
                }
                if (relativePosition.y + g.component.height > vector2.y)
                {
                    relativePosition.y = vector2.y - g.component.height;
                }
                g.component.relativePosition = relativePosition;
                uint num;
                uint num2;
                uint num3;
                uint num4;
                uint num5;
                Singleton<NaturalResourceManager>.instance.GetTileResources(x, z, out num, out num2, out num3, out num4, out num5);
                m_OilNoResources.isVisible = (num2 == 0u);
                m_OreNoResources.isVisible = (num == 0u);
                m_ForestryNoResources.isVisible = (num3 == 0u);
                m_FertilityNoResources.isVisible = (num4 == 0u);
                bool flag = num5 != 0u;
                m_Water.tooltip = ((!flag) ? Locale.Get("AREA_NO_WATER") : Locale.Get("AREA_YES_WATER"));
                m_NoWater.isVisible = !flag;
                int num6;
                int num7;
                Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, out num6, out num7);
                int tileNodeCount = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip);
                bool flag2 = (num6 == 0 && num7 == 0) || tileNodeCount == 0;
                m_Ship.tooltip = ((!flag2) ? Locale.Get("AREA_YES_SHIPCONNECTION") : Locale.Get("AREA_NO_SHIPCONNECTION"));
                m_NoShip.isVisible = flag2;
                m_InShip.isVisible = (num6 > 0 && tileNodeCount > 0);
                m_OutShip.isVisible = (num7 > 0 && tileNodeCount > 0);
                Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, out num6, out num7);
                tileNodeCount = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain);
                bool flag3 = (num6 == 0 && num7 == 0) || tileNodeCount == 0;
                m_Train.tooltip = ((!flag3) ? Locale.Get("AREA_YES_TRAINCONNECTION") : Locale.Get("AREA_NO_TRAINCONNECTION"));
                m_NoTrain.isVisible = flag3;
                m_InTrain.isVisible = (num6 > 0 && tileNodeCount > 0);
                m_OutTrain.isVisible = (num7 > 0 && tileNodeCount > 0);
                Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, out num6, out num7);
                bool flag4 = num6 == 0 && num7 == 0;
                m_Plane.tooltip = ((!flag4) ? Locale.Get("AREA_YES_PLANECONNECTION") : Locale.Get("AREA_NO_PLANECONNECTION"));
                m_NoPlane.isVisible = flag4;
                m_InPlane.isVisible = (num6 > 0);
                m_OutPlane.isVisible = (num7 > 0);
                Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.Road, ItemClass.SubService.None, out num6, out num7);
                tileNodeCount = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.Road, ItemClass.SubService.None);
                bool flag5 = (num6 == 0 && num7 == 0) || tileNodeCount == 0;
                m_Highway.tooltip = ((!flag5) ? Locale.Get("AREA_YES_HIGHWAYCONNECTION") : Locale.Get("AREA_NO_HIGHWAYCONNECTION"));
                m_NoHighway.isVisible = flag5;
                m_InHighway.isVisible = (num6 > 0 && tileNodeCount > 0);
                m_OutHighway.isVisible = (num7 > 0 && tileNodeCount > 0);
                float num8 = 3686400f;
                float num9 = 1139.0625f;
                float num10 = num8 / num9 * 255f;
                float tileFlatness = Singleton<TerrainManager>.instance.GetTileFlatness(x, z);
                float num11 = tileFlatness * (num10 - num5) / num10;
                int num12 = Singleton<GameAreaManager>.instance.CalculateTilePrice(num, num2, num3, num4, num5, !flag5, !flag3, !flag2, !flag4, tileFlatness);
                m_BuildableArea.text = string.Format(Locale.Get("AREA_BUILDABLE"), string.Format(Locale.Get("VALUE_PERCENTAGE"), Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(num11 * 100f), 100))));
                m_Price.text = (num12 / 100).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                bool flag6 = Singleton<GameAreaManager>.instance.IsUnlocked(x, z);
                m_Title.text = Locale.Get((!flag6) ? "AREA_NEWTILE" : "AREA_OWNEDTILE");
                m_PurchasePanel.isVisible = !flag6;
                m_PurchasePanel.isEnabled = (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.LandPrice, num12) == num12);
                float value = Mathf.Pow(num2 / num10, g.m_OilExponent);
                float value2 = Mathf.Pow(num / num10, g.m_OreExponent);
                float value3 = Mathf.Pow(num3 / num10, g.m_ForestryExponent);
                float value4 = Mathf.Pow(num4 / num10, g.m_FarmingExponent);
                if (!ValueAnimator.IsAnimating("Oil"))
                {
                    m_OilResources.value = value;
                }
                if (!ValueAnimator.IsAnimating("Ore"))
                {
                    m_OreResources.value = value2;
                }
                if (!ValueAnimator.IsAnimating("Forest"))
                {
                    m_ForestryResources.value = value3;
                }
                if (!ValueAnimator.IsAnimating("Fertility"))
                {
                    m_FertilityResources.value = value4;
                }
            }
        }
    }
}
