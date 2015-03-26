using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Unlimiter.Areas
{
    class FakeGameAreaInfoPanel
    {
        private static void ShowInternal(GameAreaInfoPanel g, int areaIndex)
        {
            g.GetType().GetField("m_AreaIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(g, areaIndex);
            int x;
            int z;
            FakeGameAreaManager.GetTileXZ(GameAreaManager.instance, areaIndex, out x, out z);
            uint ore;
            uint oil;
            uint forest;
            uint fertility;
            uint water;
            Singleton<NaturalResourceManager>.instance.GetTileResources(x, z, out ore, out oil, out forest, out fertility, out water);
            float num = (float)(3686400.0 / (18225.0 / 16.0) * (double)byte.MaxValue);
            float endValue1 = Mathf.Pow((float)oil / num, g.m_OilExponent);
            float endValue2 = Mathf.Pow((float)ore / num, g.m_OreExponent);
            float endValue3 = Mathf.Pow((float)forest / num, g.m_ForestryExponent);
            float endValue4 = Mathf.Pow((float)fertility / num, g.m_FarmingExponent);
            ValueAnimator.Cancel("Oil");
            ValueAnimator.Cancel("Ore");
            ValueAnimator.Cancel("Forest");
            ValueAnimator.Cancel("Fertility");

            UIProgressBar m_OilResources = (UIProgressBar)g.GetType().GetField("m_OilResources").GetValue(g);
            UIProgressBar m_OreResources = (UIProgressBar)g.GetType().GetField("m_OreResources").GetValue(g);
            UIProgressBar m_ForestryResources = (UIProgressBar)g.GetType().GetField("m_ForestryResources").GetValue(g);
            UIProgressBar m_FertilityResources = (UIProgressBar)g.GetType().GetField("m_FertilityResources").GetValue(g);

            m_OilResources.value = 0.0f;
            m_OreResources.value = 0.0f;
            m_ForestryResources.value = 0.0f;
            m_FertilityResources.value = 0.0f;
            ValueAnimator.Animate("Oil", (Action<float>)(val => m_OilResources.value = val), new AnimatedFloat(0.0f, endValue1, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Ore", (Action<float>)(val => m_OreResources.value = val), new AnimatedFloat(0.0f, endValue2, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Forest", (Action<float>)(val => m_ForestryResources.value = val), new AnimatedFloat(0.0f, endValue3, g.m_InterpolationTime, g.m_InterpolationEasingType));
            ValueAnimator.Animate("Fertility", (Action<float>)(val => m_FertilityResources.value = val), new AnimatedFloat(0.0f, endValue4, g.m_InterpolationTime, g.m_InterpolationEasingType));
            g.GetType().GetMethod("UpdatePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(g, null);
        }
        private static void UpdatePanel(GameAreaInfoPanel g)
        {
            // Start
            var m_FullscreenContainer = UIView.Find("FullScreenContainer");
            if ((UnityEngine.Object)m_FullscreenContainer != (UnityEngine.Object)null)
                m_FullscreenContainer.AttachUIComponent(g.gameObject);
            var m_Title = g.Find<UILabel>("Title");
            var m_BuildableArea = g.Find<UILabel>("BuildableArea");
            var m_Price = g.Find<UILabel>("Price");
            var m_PurchasePanel = g.Find<UIPanel>("PurchasePanel");
            var m_OilResources = g.Find<UIProgressBar>("ResourceBarOil");
            var m_OreResources = g.Find<UIProgressBar>("ResourceBarOre");
            var m_ForestryResources = g.Find<UIProgressBar>("ResourceBarForestry");
            var m_FertilityResources = g.Find<UIProgressBar>("ResourceBarFarming");
            var m_OilNoResources = g.Find("ResourceOil").Find<UISprite>("NoNoNo");
            var m_OreNoResources = g.Find("ResourceOre").Find<UISprite>("NoNoNo");
            var m_ForestryNoResources = g.Find("ResourceForestry").Find<UISprite>("NoNoNo");
            var m_FertilityNoResources = g.Find("ResourceFarming").Find<UISprite>("NoNoNo");
            var m_Water = g.Find<UISprite>("Water");
            var m_NoWater = m_Water.Find<UISprite>("NoNoNo");
            var m_Highway = g.Find<UISprite>("Highway");
            var m_NoHighway = m_Highway.Find<UISprite>("NoNoNo");
            var m_InHighway = m_Highway.Find<UISprite>("Incoming");
            var m_OutHighway = m_Highway.Find<UISprite>("Outgoing");
            var m_Train = g.Find<UISprite>("Train");
            var m_NoTrain = m_Train.Find<UISprite>("NoNoNo");
            var m_InTrain = m_Train.Find<UISprite>("Incoming");
            var m_OutTrain = m_Train.Find<UISprite>("Outgoing");
            var m_Ship = g.Find<UISprite>("Ship");
            var m_NoShip = m_Ship.Find<UISprite>("NoNoNo");
            var m_InShip = m_Ship.Find<UISprite>("Incoming");
            var m_OutShip = m_Ship.Find<UISprite>("Outgoing");
            var m_Plane = g.Find<UISprite>("Plane");
            var m_NoPlane = m_Plane.Find<UISprite>("NoNoNo");
            var m_InPlane = m_Plane.Find<UISprite>("Incoming");
            var m_OutPlane = m_Plane.Find<UISprite>("Outgoing");


            // UpdatePanel
            int m_AreaIndex = (int)g.GetType().GetField("m_AreaIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(g);
            if (m_AreaIndex == -1)
                return;
            int x;
            int z;
            // That you have to use all this to change ONE MERE FUCJKIGN METHOD call is literally the worst piece of code in this mod until now.
            // Singleton<GameAreaManager>.instance.GetTileXZ(m_AreaIndex, out x, out z);
            FakeGameAreaManager.GetTileXZ(GameAreaManager.instance, m_AreaIndex, out x, out z);
            Vector3 vector3_1 = Camera.main.WorldToScreenPoint(Singleton<GameAreaManager>.instance.GetAreaPositionSmooth(x, z));
            UIView uiView = g.component.GetUIView();
            Vector2 vector2 = !((UnityEngine.Object)m_FullscreenContainer != (UnityEngine.Object)null) ? uiView.GetScreenResolution() : m_FullscreenContainer.size;
            Vector3 vector3_2 = vector3_1 / uiView.inputScale;
            Vector3 vector3_3 = UIPivotExtensions.UpperLeftToTransform(g.component.pivot, g.component.size, g.component.arbitraryPivotOffset);
            Vector3 vector3_4 = (Vector3)(uiView.ScreenPointToGUI((Vector2)vector3_2) + new Vector2(vector3_3.x, vector3_3.y));
            if ((double)vector3_4.x < 0.0)
                vector3_4.x = 0.0f;
            if ((double)vector3_4.y < 0.0)
                vector3_4.y = 0.0f;
            if ((double)vector3_4.x + (double)g.component.width > (double)vector2.x)
                vector3_4.x = vector2.x - g.component.width;
            if ((double)vector3_4.y + (double)g.component.height > (double)vector2.y)
                vector3_4.y = vector2.y - g.component.height;
            g.component.relativePosition = vector3_4;
            uint ore;
            uint oil;
            uint forest;
            uint fertility;
            uint water;
            Singleton<NaturalResourceManager>.instance.GetTileResources(x, z, out ore, out oil, out forest, out fertility, out water);
            m_OilNoResources.isVisible = (int)oil == 0;
            m_OreNoResources.isVisible = (int)ore == 0;
            m_ForestryNoResources.isVisible = (int)forest == 0;
            m_FertilityNoResources.isVisible = (int)fertility == 0;
            bool flag1 = (int)water != 0;
            m_Water.tooltip = !flag1 ? Locale.Get("AREA_NO_WATER") : Locale.Get("AREA_YES_WATER");
            m_NoWater.isVisible = !flag1;
            int incoming;
            int outgoing;
            Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip, out incoming, out outgoing);
            int tileNodeCount1 = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportShip);
            bool flag2 = incoming == 0 && outgoing == 0 || tileNodeCount1 == 0;
            m_Ship.tooltip = !flag2 ? Locale.Get("AREA_YES_SHIPCONNECTION") : Locale.Get("AREA_NO_SHIPCONNECTION");
            m_NoShip.isVisible = flag2;
            m_InShip.isVisible = incoming > 0 && tileNodeCount1 > 0;
            m_OutShip.isVisible = outgoing > 0 && tileNodeCount1 > 0;
            Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain, out incoming, out outgoing);
            int tileNodeCount2 = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTrain);
            bool flag3 = incoming == 0 && outgoing == 0 || tileNodeCount2 == 0;
            m_Train.tooltip = !flag3 ? Locale.Get("AREA_YES_TRAINCONNECTION") : Locale.Get("AREA_NO_TRAINCONNECTION");
            m_NoTrain.isVisible = flag3;
            m_InTrain.isVisible = incoming > 0 && tileNodeCount2 > 0;
            m_OutTrain.isVisible = outgoing > 0 && tileNodeCount2 > 0;
            Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportPlane, out incoming, out outgoing);
            bool flag4 = incoming == 0 && outgoing == 0;
            m_Plane.tooltip = !flag4 ? Locale.Get("AREA_YES_PLANECONNECTION") : Locale.Get("AREA_NO_PLANECONNECTION");
            m_NoPlane.isVisible = flag4;
            m_InPlane.isVisible = incoming > 0;
            m_OutPlane.isVisible = outgoing > 0;
            Singleton<BuildingManager>.instance.CalculateOutsideConnectionCount(ItemClass.Service.Road, ItemClass.SubService.None, out incoming, out outgoing);
            int tileNodeCount3 = Singleton<NetManager>.instance.GetTileNodeCount(x, z, ItemClass.Service.Road, ItemClass.SubService.None);
            bool flag5 = incoming == 0 && outgoing == 0 || tileNodeCount3 == 0;
            m_Highway.tooltip = !flag5 ? Locale.Get("AREA_YES_HIGHWAYCONNECTION") : Locale.Get("AREA_NO_HIGHWAYCONNECTION");
            m_NoHighway.isVisible = flag5;
            m_InHighway.isVisible = incoming > 0 && tileNodeCount3 > 0;
            m_OutHighway.isVisible = outgoing > 0 && tileNodeCount3 > 0;
            float num1 = (float)(3686400.0 / (18225.0 / 16.0) * (double)byte.MaxValue);
            float tileFlatness = Singleton<TerrainManager>.instance.GetTileFlatness(x, z);
            float num2 = tileFlatness * (num1 - (float)water) / num1;
            int amount = Singleton<GameAreaManager>.instance.CalculateTilePrice(ore, oil, forest, fertility, water, !flag5, !flag3, !flag2, !flag4, tileFlatness);
            m_BuildableArea.text = string.Format(Locale.Get("AREA_BUILDABLE"), (object)string.Format(Locale.Get("VALUE_PERCENTAGE"), (object)Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(num2 * 100f), 100))));
            m_Price.text = (amount / 100).ToString(Settings.moneyFormat, (IFormatProvider)LocaleManager.cultureInfo);
            bool flag6 = Singleton<GameAreaManager>.instance.IsUnlocked(x, z);
            m_Title.text = Locale.Get(!flag6 ? "AREA_NEWTILE" : "AREA_OWNEDTILE");
            m_PurchasePanel.isVisible = !flag6;
            m_PurchasePanel.isEnabled = Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.LandPrice, amount) == amount;
            float num3 = Mathf.Pow((float)oil / num1, g.m_OilExponent);
            float num4 = Mathf.Pow((float)ore / num1, g.m_OreExponent);
            float num5 = Mathf.Pow((float)forest / num1, g.m_ForestryExponent);
            float num6 = Mathf.Pow((float)fertility / num1, g.m_FarmingExponent);
            if (!ValueAnimator.IsAnimating("Oil"))
                m_OilResources.value = num3;
            if (!ValueAnimator.IsAnimating("Ore"))
                m_OreResources.value = num4;
            if (!ValueAnimator.IsAnimating("Forest"))
                m_ForestryResources.value = num5;
            if (ValueAnimator.IsAnimating("Fertility"))
                return;
            m_FertilityResources.value = num6;
        }
    }
}
