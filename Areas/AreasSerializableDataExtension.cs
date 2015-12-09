using System.IO;
using System.Reflection;
using ColossalFramework.IO;
using ICities;
using System.Linq;

namespace EightyOne.Areas
{
    public class AreasSerializableDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeGAM";

        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                int num = FakeGameAreaManager.areaGrid.Length;
                EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
                for (int i = 0; i < num; i++)
                {
                    @byte.Write((byte)FakeGameAreaManager.areaGrid[i]);
                }
                @byte.EndWrite();
            }

            public void Deserialize(DataSerializer s)
            {
                FakeGameAreaManager.areaGrid = new int[FakeGameAreaManager.GRID * FakeGameAreaManager.GRID];
                int num = FakeGameAreaManager.areaGrid.Length;

                EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
                for (int i = 0; i < num; i++)
                {
                    FakeGameAreaManager.areaGrid[i] = (int)@byte.Read();
                }
                @byte.EndRead();
            }

            public void AfterDeserialize(DataSerializer s)
            {
                typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameAreaManager.instance, true);
            }
        }

        public override void OnSaveData()
        {
            var currentCount = 0;
            for (var i = 0; i < 5; i += 1)
            {
                for (var j = 0; j < 5; j += 1)
                {
                    var grid = FakeGameAreaManager.areaGrid[(j + 2) * FakeGameAreaManager.GRID + (i + 2)];
                    GameAreaManager.instance.m_areaGrid[j * 5 + i] = grid;
                    if (grid != 0)
                    {
                        currentCount += 1;
                    }
                }
            }
            GameAreaManager.instance.m_areaCount = currentCount;
            typeof(GameAreaManager).GetField("m_areasUpdated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GameAreaManager.instance, true);
            GameAreaManager.instance.m_maxAreaCount = 25;

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 1u, new Data());
                var data = ms.ToArray();
                serializableDataManager.SaveData(id, data);
            }
        }

        public override void OnLoadData()
        {
            if (!serializableDataManager.EnumerateData().Contains(id))
            {
                return;
            }
            var data = serializableDataManager.LoadData(id);
            using (var ms = new MemoryStream(data))
            {
                var s = DataSerializer.Deserialize<Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}