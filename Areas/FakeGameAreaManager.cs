using System;
using ColossalFramework;
using ColossalFramework.Math;
using System.Reflection;
using ColossalFramework.IO;
using ColossalFramework.Steamworks;
using ColossalFramework.Threading;
using UnityEngine;
using EightyOne.Attributes;

namespace EightyOne.Areas
{
    [TargetType(typeof(GameAreaManager))]
    public class FakeGameAreaManager : GameAreaManager
    {

        public const int GRID = 9;
        public const float HALFGRID = 4.5f;

        private static FieldInfo _areasUpdatedField;
        private static FieldInfo _areaTexField;
        private static FieldInfo _unlockingField;

        public static void Init()
        {
            _areasUpdatedField = typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_areasUpdatedField == null)
            {
                throw new Exception("m_areasUpdated");
            }
            _areaTexField = typeof(GameAreaManager).GetField("m_areaTex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_areaTexField == null)
            {
                throw new Exception("m_areasUpdated");
            }
            _unlockingField = typeof(GameAreaManager).GetField("m_unlocking", BindingFlags.Instance | BindingFlags.NonPublic);
            if (_unlockingField == null)
            {
                throw new Exception("m_unlocking");
            }

            DestroyImmediate((Texture2D)_areaTexField.GetValue(instance));
            var areaTex = new Texture2D(FakeGameAreaManagerUI.AREA_TEX_SIZE, FakeGameAreaManagerUI.AREA_TEX_SIZE, TextureFormat.ARGB32, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _areaTexField.SetValue(GameAreaManager.instance, areaTex);
            FakeGameAreaManagerInit.UpdateData();
        }

        [ReplaceMethod]
        public int get_MaxAreaCount()
        {
            if (this.m_maxAreaCount == 0)
                //begin mod
                this.m_maxAreaCount = GRID * GRID;
            //end mod
            return this.m_maxAreaCount;
        }

        [ReplaceMethod]
        public new Vector3 GetAreaPositionSmooth(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || x >= GRID || z >= GRID)
            //end mod
            {
                return Vector3.zero;
            }
            Vector3 vector;
            //begin mod
            vector.x = ((float)x - HALFGRID + 0.5f) * 1920f;
            //end mod
            vector.y = 0f;
            //begin mod
            vector.z = ((float)z - HALFGRID + 0.5f) * 1920f;
            //end mod
            vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, true, 0f);
            return vector;
        }

        [ReplaceMethod]
        public new bool CanUnlock(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || (x >= GRID || z >= GRID) || (this.m_areaCount >= this.MaxAreaCount || !Singleton<UnlockManager>.instance.Unlocked(this.m_areaCount) || this.m_areaGrid[z * GRID + x] != 0))
                return false;
            //end mod
            bool result = this.IsUnlocked(x, z - 1) || this.IsUnlocked(x - 1, z) || this.IsUnlocked(x + 1, z) || this.IsUnlocked(x, z + 1);
            //begin mod
            //end mod
            return result;
        }

        [ReplaceMethod]
        public new int GetArea(int x, int z)
        {
            //begin mod
            if (x >= 0 && z >= 0 && (x < GRID && z < GRID))
                return this.m_areaGrid[z * GRID + x];
            int num = 0;
            return x < -num || z < -num || (x >= GRID + num || z >= GRID + num) ? -2 : -1;
            //end mod
        }

        [ReplaceMethod]
        public new bool IsUnlocked(int x, int z)
        {
            //begin mod
            if (x < 0 || z < 0 || (x >= GRID || z >= GRID))
                return false;
            return this.m_areaGrid[z * GRID + x] != 0;
            //end mod
        }

        [ReplaceMethod]
        public new int GetAreaIndex(Vector3 p)
        {
            //begin mod
            int num1 = Mathf.FloorToInt((float)((double)p.x / 1920.0 + HALFGRID));
            int num2 = Mathf.FloorToInt((float)((double)p.z / 1920.0 + HALFGRID));
            if (num1 < 0 || num2 < 0 || (num1 >= GRID || num2 >= GRID))
                return -1;
            return num2 * GRID + num1;
            //end mod
        }

        //This method gets inlined and can't be detoured
        public static void GetTileXZ(int tile, out int x, out int z)
        {
            //begin mod
            x = tile % GRID;
            z = tile / GRID;
            //end mod
        }

        //This method gets inlined and can't be detoured
        public static int GetTileIndex(int x, int z)
        {
            //begin mod
            return z * GRID + x;
            //end mod
        }

        [ReplaceMethod]
        public new bool UnlockArea(int index)
        {
            _unlockingField.SetValue(this, true);
            try
            {
                //begin mod
                int x = index % GRID;
                int z = index / GRID;
                //end mod
                if (this.CanUnlock(x, z))
                {
                    this.m_areaNotUnlocked.Deactivate();
                    CODebugBase<LogChannel>.Log(LogChannel.Core, "Unlocking new area");
                    this.m_areaGrid[index] = ++this.m_areaCount;
                    _areasUpdatedField.SetValue(this, true);
                    //begin mod
                    if (this.m_areaCount == 9)
                        //end mod
                        ThreadHelper.dispatcher.Dispatch((System.Action)(() =>
                        {
                            if (Steam.achievements["SIMulatedCity"].achieved)
                                return;
                            Steam.achievements["SIMulatedCity"].Unlock();
                        }));
                    //begin mod
                    float minX = (float)(((double)x - HALFGRID) * 1920.0);
                    float maxX = (float)(((double)(x + 1) - HALFGRID) * 1920.0);
                    float minZ = (float)(((double)z - HALFGRID) * 1920.0);
                    float maxZ = (float)(((double)(z + 1) - HALFGRID) * 1920.0);
                    //end mod
                    Singleton<ZoneManager>.instance.UpdateBlocks(minX, minZ, maxX, maxZ);

                    if (Singleton<TerrainManager>.instance.SetDetailedPatch(x, z))
                    {
                        Singleton<MessageManager>.instance.TryCreateMessage(this.m_properties.m_unlockMessage, Singleton<MessageManager>.instance.GetRandomResidentID());
                        //begin mod
                        //end mod
                        return true;

                    }
                    --this.m_areaCount;
                    this.m_areaGrid[index] = 0;

                    _areasUpdatedField.SetValue(this, true);
                }
                return false;
            }
            finally
            {
                _unlockingField.SetValue(this, false);
            }
        }

        [ReplaceMethod]
        public new bool ClampPoint(ref Vector3 position)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
                return true;
            }
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
                return true;
            }
            //begin mod
            int x = Mathf.FloorToInt((float)((double)position.x / 1920.0 + HALFGRID));
            int z = Mathf.FloorToInt((float)((double)position.z / 1920.0 + HALFGRID));
            //end mod
            if (this.GetArea(x, z) > 0)
                return true;
            Rect rect1 = new Rect();
            //begin mod
            rect1.xMin = -1920 * HALFGRID;
            rect1.yMin = -1920 * HALFGRID;
            rect1.xMax = 1920 * HALFGRID;
            rect1.yMax = 1920 * HALFGRID;
            //end mod
            float num3 = 1000000f;
            for (int index1 = -1; index1 <= 1; ++index1)
            {
                for (int index2 = -1; index2 <= 1; ++index2)
                {
                    if (this.GetArea(x + index2, z + index1) > 0)
                    {
                        Rect rect2 = new Rect();
                        //begin mod
                        rect2.xMin = (float)(((double)(x + index2) - HALFGRID) * 1920.0);
                        rect2.yMin = (float)(((double)(z + index1) - HALFGRID) * 1920.0);
                        //end mod
                        rect2.xMax = rect2.xMin + 1920f;
                        rect2.yMax = rect2.yMin + 1920f;
                        float num1 = Mathf.Max(Mathf.Max(position.x - rect2.xMax, rect2.xMin - position.x), Mathf.Max(position.z - rect2.yMax, rect2.yMin - position.z));
                        if ((double)num1 < (double)num3)
                        {
                            rect1 = rect2;
                            num3 = num1;
                        }
                    }
                }
            }
            if ((double)position.x < (double)rect1.xMin)
                position.x = rect1.xMin;
            if ((double)position.x > (double)rect1.xMax)
                position.x = rect1.xMax;
            if ((double)position.z < (double)rect1.yMin)
                position.z = rect1.yMin;
            if ((double)position.z > (double)rect1.yMax)
                position.z = rect1.yMax;
            return (double)num3 != 1000000.0;

        }


        [ReplaceMethod]
        public new bool PointOutOfArea(Vector3 p)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
            }
            else
            {
                bool flag = (availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None;
                //begin mod
                int area = this.GetArea(Mathf.FloorToInt((float)((double)p.x / 1920.0 + HALFGRID)), Mathf.FloorToInt((float)((double)p.z / 1920.0 + HALFGRID)));
                //end mod
                if (area == -2 || !flag && area <= 0)
                    return true;
            }
            return false;
        }


        [ReplaceMethod]
        public new bool QuadOutOfArea(Quad2 quad)
        {
            ItemClass.Availability availability = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((availability & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                //begin mod
                //end mod
            }
            else
            {
                bool flag = (availability & ItemClass.Availability.MapEditor) != ItemClass.Availability.None;
                Vector2 vector2_1 = quad.Min();
                Vector2 vector2_2 = quad.Max();
                //begin mod
                int num1 = Mathf.FloorToInt((float)(((double)vector2_1.x - 8.0) / 1920.0 + HALFGRID));
                int num2 = Mathf.FloorToInt((float)(((double)vector2_1.y - 8.0) / 1920.0 + HALFGRID));
                int num3 = Mathf.FloorToInt((float)(((double)vector2_2.x + 8.0) / 1920.0 + HALFGRID));
                int num4 = Mathf.FloorToInt((float)(((double)vector2_2.y + 8.0) / 1920.0 + HALFGRID));
                //end mod
                for (int z = num2; z <= num4; ++z)
                {
                    for (int x = num1; x <= num3; ++x)
                    {
                        int area = this.GetArea(x, z);
                        if (area == -2 || !flag && area <= 0)
                        {
                            //begin mod
                            if (quad.Intersect(new Quad2()
                            {
                                a = new Vector2((float)(((double)x - HALFGRID) * 1920.0 - 8.0), (float)(((double)z - HALFGRID) * 1920.0 - 8.0)),
                                b = new Vector2((float)(((double)x - HALFGRID) * 1920.0 - 8.0), (float)(((double)z - HALFGRID + 1.0) * 1920.0 + 8.0)),
                                c = new Vector2((float)(((double)x - HALFGRID + 1.0) * 1920.0 + 8.0), (float)(((double)z - HALFGRID + 1.0) * 1920.0 + 8.0)),
                                d = new Vector2((float)(((double)x - HALFGRID + 1.0) * 1920.0 + 8.0), (float)(((double)z - HALFGRID) * 1920.0 - 8.0))
                            }))
                                //end mod
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        [TargetType(typeof(GameAreaManager.Data))]
        public class FakeData : IDataContainer
        {
            private static FieldInfo _startTileField = typeof(GameAreaManager).GetField("m_startTile", BindingFlags.NonPublic | BindingFlags.Instance);
            private static FieldInfo _fieldInfo1 = typeof(GameAreaManager).GetField("m_buildableArea0", BindingFlags.Instance | BindingFlags.NonPublic);
            private static FieldInfo _fieldInfo2 = typeof(GameAreaManager).GetField("m_buildableArea1", BindingFlags.Instance | BindingFlags.NonPublic);
            private static FieldInfo _fieldInfo3 = typeof(GameAreaManager).GetField("m_buildableArea2", BindingFlags.Instance | BindingFlags.NonPublic);
            private static FieldInfo _fieldInfo4 = typeof(GameAreaManager).GetField("m_buildableArea3", BindingFlags.Instance | BindingFlags.NonPublic);

            [ReplaceMethod]
            public void Serialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "GameAreaManager");
                GameAreaManager instance = Singleton<GameAreaManager>.instance;
                //begin mod
                int[] numArray = new int[AREAGRID_RESOLUTION * AREAGRID_RESOLUTION];
                int length = numArray.Length;
                int areaCount = 0;
                for (var i = 0; i < 5; i += 1)
                {
                    for (var j = 0; j < 5; j += 1)
                    {
                        var grid = GameAreaManager.instance.m_areaGrid[(j + 2) * GRID + (i + 2)];
                        numArray[j * AREAGRID_RESOLUTION + i] = grid;
                        if (grid != 0)
                        {
                            areaCount += 1;
                        }
                    }
                }
                s.WriteUInt8((uint)areaCount);
                //end mod
                s.WriteUInt8((uint)(int)_startTileField.GetValue(instance));
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int index = 0; index < length; ++index)
                    @byte.Write((byte)numArray[index]);
                @byte.EndWrite();

                s.WriteObject<GenericGuide>(instance.m_areaNotUnlocked);
                s.WriteFloat((float)_fieldInfo1.GetValue(instance));
                s.WriteFloat((float)_fieldInfo2.GetValue(instance));
                s.WriteFloat((float)_fieldInfo3.GetValue(instance));
                s.WriteFloat((float)_fieldInfo4.GetValue(instance));
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "GameAreaManager");
            }

            [ReplaceMethod(true)]
            public void Deserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "GameAreaManager");
                GameAreaManager instance = Singleton<GameAreaManager>.instance;
                //begin mod
                instance.m_areaGrid = new int[AREAGRID_RESOLUTION * AREAGRID_RESOLUTION];
                //end mod
                int[] numArray = instance.m_areaGrid;
                int length = numArray.Length;
                instance.m_areaCount = (int)s.ReadUInt8();
                //begin mod
                instance.m_maxAreaCount = AREAGRID_RESOLUTION * AREAGRID_RESOLUTION;
                //end mod
                _startTileField.SetValue(instance, s.version < 137U ? 12 : (int)s.ReadUInt8());
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                for (int index = 0; index < length; ++index)
                    numArray[index] = (int)@byte.Read();
                @byte.EndRead();
                instance.m_areaNotUnlocked = s.version < 87U ? (GenericGuide)null : s.ReadObject<GenericGuide>();
                if (s.version >= 199U)
                {
                    _fieldInfo1.SetValue(instance, s.ReadFloat());
                    _fieldInfo2.SetValue(instance, s.ReadFloat());
                    _fieldInfo3.SetValue(instance, s.ReadFloat());
                    _fieldInfo4.SetValue(instance, s.ReadFloat());
                }
                else
                {
                    _fieldInfo1.SetValue(instance, -1f);
                    _fieldInfo2.SetValue(instance, -1f);
                    _fieldInfo3.SetValue(instance, -1f);
                    _fieldInfo4.SetValue(instance, -1f);
                }
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "GameAreaManager");
            }

            public void AfterDeserialize(DataSerializer s)
            {
                throw new NotImplementedException();
            }
        }

        public new class Data : IDataContainer
        {
            public static int[] _loadedGrid;

            public void Serialize(DataSerializer s)
            {
                var num = instance.m_areaGrid.Length;
                var @byte = EncodedArray.Byte.BeginWrite(s);
                for (var i = 0; i < num; i++)
                {
                    @byte.Write((byte)instance.m_areaGrid[i]);
                }
                @byte.EndWrite();
            }

            public void Deserialize(DataSerializer s)
            {
                _loadedGrid = new int[GRID * GRID];
                var @byte = EncodedArray.Byte.BeginRead(s);
                for (var i = 0; i < _loadedGrid.Length; i++)
                {
                    _loadedGrid[i] = (int)@byte.Read();
                }
                @byte.EndRead();
            }

            public void AfterDeserialize(DataSerializer s)
            {
 
            }
        }
    }
}
