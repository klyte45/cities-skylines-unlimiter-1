using ColossalFramework;
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
    }
}
