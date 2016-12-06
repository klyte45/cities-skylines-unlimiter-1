using System.Reflection;
using ColossalFramework;
using EightyOne.Redirection;
using UnityEngine;

namespace EightyOne.Areas
{
    [TargetType(typeof(DisasterManager))]
    public class FakeDisasterManager : DisasterManager
    {
        public const int GRID = 450;
        public const int HALFGRID = 225;

        private byte[] m_hazardAmount; //TODO(earalov): copy from original
        private uint[] m_evacuationMap; //TODO(earalov): copy from original
        private Texture2D m_hazardTexture; //TODO(earalov): copy from original

        [RedirectMethod]
        private void UpdateHazardMapping()
        {
            if (this.m_hazardMapVisible == 0)
                return;
            Vector4 vec;
            //begin mod
            vec.z = 1.0f / (38.4f * GRID);
            //end mod
            vec.x = 0.5f;
            vec.y = 0.5f;
            vec.w = 0.5f;
            Shader.SetGlobalVector("_DisasterHazardMapping", vec);
            this.m_hazardModified = 0;
            this.UpdateTexture();
        }

        [RedirectMethod]
        public Color SampleDisasterHazardMap(Vector3 pos)
        {
            pos.x /= 38.4f;
            pos.z /= 38.4f;
            //begin mod
            int num1 = Mathf.Max((int)((double)pos.x + HALFGRID), 0);
            int num2 = Mathf.Max((int)((double)pos.z + HALFGRID), 0);
            int num3 = Mathf.Min((int)((double)pos.x + 1.0 + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)((double)pos.z + 1.0 + HALFGRID), GRID - 1);
            float num5 = pos.x - (float)((double)num1 - HALFGRID + 0.5);
            float num6 = pos.z - (float)((double)num2 - HALFGRID + 0.5);
            float num7 = (float)this.m_hazardAmount[num2 * GRID + num1];
            float num8 = (float)this.m_hazardAmount[num2 * GRID + num3];
            float num9 = (float)this.m_hazardAmount[num4 * GRID + num1];
            float num10 = (float)this.m_hazardAmount[num4 * GRID + num3];
            //end mod
            float num11 = num7 + (num8 - num7) * num5;
            float num12 = num9 + (num10 - num9) * num5;
            //begin mod
            float num13 = (float)(((double)num11 + ((double)num12 - (double)num11) * (double)num6) *  (1.0 / ((double)GRID - 1.0)));
            //end mod
            InfoProperties infoProperties = Singleton<InfoManager>.instance.m_properties;
            if ((double)num13 < 0.5)
                return ColorUtils.LinearLerp(infoProperties.m_neutralColor, infoProperties.m_modeProperties[29].m_activeColorB, num13 * 2f);
            return ColorUtils.LinearLerp(infoProperties.m_modeProperties[29].m_activeColorB, infoProperties.m_modeProperties[29].m_targetColor, (float)((double)num13 * 2.0 - 1.0));
        }

        [RedirectMethod]
        private void UpdateTexture()
        {
            //begin mod
            for (int index1 = 0; index1 < GRID; ++index1)
            {
                for (int index2 = 0; index2 < GRID; ++index2)
                    this.m_hazardAmount[index1 * GRID + index2] = (byte)0;
            }
            //end mod
            for (int index = 0; index < this.m_disasters.m_size; ++index)
            {
                if ((this.m_disasters.m_buffer[index].m_flags & (DisasterData.Flags.Created | DisasterData.Flags.Deleted)) == DisasterData.Flags.Created)
                {
                    DisasterInfo info = this.m_disasters.m_buffer[index].Info;
                    InfoManager.SubInfoMode subMode;
                    if ((UnityEngine.Object)info != (UnityEngine.Object)null && info.m_disasterAI.GetHazardSubMode(out subMode) && (m_hazardMapVisible & 1 << (int)(subMode & (InfoManager.SubInfoMode)31)) != 0)
                        info.m_disasterAI.UpdateHazardMap((ushort)index, ref this.m_disasters.m_buffer[index], this.m_hazardAmount);
                }
            }
            //begin mod
            for (int y = 0; y < GRID; ++y)
            {
                for (int x = 0; x < GRID; ++x)
                {
                    int num1 = 0;
                    if (y > 0)
                    {
                        if (x > 0)
                            num1 += (int)this.m_hazardAmount[(y - 1) * GRID + (x - 1)] * 5;
                        num1 += (int)this.m_hazardAmount[(y - 1) * GRID + x] * 7;
                        if (x < GRID - 1)
                            num1 += (int)this.m_hazardAmount[(y - 1) * GRID + (x + 1)] * 5;
                    }
                    if (x > 0)
                        num1 += (int)this.m_hazardAmount[y * GRID + (x - 1)] * 7;
                    int num2 = num1 + (int)this.m_hazardAmount[y * GRID + x] * 14;
                    if (x < GRID - 1)
                        num2 += (int)this.m_hazardAmount[y * GRID + (x + 1)] * 7;
                    if (y < GRID - 1)
                    {
                        if (x > 0)
                            num2 += (int)this.m_hazardAmount[(y + 1) * GRID + (x - 1)] * 5;
                        num2 += (int)this.m_hazardAmount[(y + 1) * GRID + x] * 7;
                        if (x < GRID - 1)
                            num2 += (int)this.m_hazardAmount[(y + 1) * GRID + (x + 1)] * 5;
                    }
                    //end mod
                    float num3 = Mathf.Clamp01((float)num2 * 6.325111E-05f);
                    Color color;
                    color.r = num3;
                    color.g = num3;
                    color.b = num3;
                    color.a = num3;
                    this.m_hazardTexture.SetPixel(x, y, color);
                }
            }
            this.m_hazardTexture.Apply();
        }

        [RedirectMethod]
        public void AddEvacuationArea(Vector3 position, float radius)
        {
            //begin mod
            int num1 = Mathf.Max((int)(((double)position.x - (double)radius) / 38.4000015258789 + HALFGRID), 0);
            int num2 = Mathf.Max((int)(((double)position.z - (double)radius) / 38.4000015258789 + HALFGRID), 0);
            int num3 = Mathf.Min((int)(((double)position.x + (double)radius) / 38.4000015258789 + HALFGRID), GRID - 1);
            int num4 = Mathf.Min((int)(((double)position.z + (double)radius) / 38.4000015258789 + HALFGRID), GRID - 1);
            //end mod
            radius *= radius;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                //begin mod
                float num5 = (float)(((double)index1 - HALFGRID + 0.5) * 38.4000015258789) - position.z;
                //end mod
                float num6 = num5 * num5;
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    //begin mod
                    float num7 = (float)(((double)index2 - HALFGRID + 0.5) * 38.4000015258789) - position.x;
                    //end mod
                    float num8 = num7 * num7;
                    if ((double)num6 + (double)num8 < (double)radius)
                    {
                        //begin mod
                        int num9 = index1 * GRID + index2;
                        //end mod
                        this.m_evacuationMap[num9 >> 4] |= (uint)(1 << ((num9 & 15) << 1)); //TODO(earalov): fix (without changing evacuation map size)
                    }
                }
            }
        }

        [RedirectMethod]
        public bool IsEvacuating(Vector3 position)
        {
            //begin mod
            int num1 = Mathf.Clamp((int)((double)position.x / 38.4000015258789 + HALFGRID), 0, GRID - 1);
            int num2 = Mathf.Clamp((int)((double)position.z / 38.4000015258789 + HALFGRID), 0, GRID - 1) * GRID + num1;
            //end mod
            return ((int)this.m_evacuationMap[num2 >> 4] & 3 << ((num2 & 15) << 1)) != 0;  //TODO(earalov): fix (without changing evacuation map size)
        }

        private int m_hazardMapVisible => (int)typeof(DisasterManager).GetField("m_hazardMapVisible",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

        private int m_hazardModified
        {
            set
            {
                typeof(DisasterManager).GetField("m_hazardModified",
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, value);
            }
        }
    }
}