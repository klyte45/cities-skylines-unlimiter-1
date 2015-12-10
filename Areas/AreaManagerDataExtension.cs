using System.IO;
using System.Reflection;
using ColossalFramework.IO;
using ICities;
using System.Linq;

namespace EightyOne.Areas
{
    public class AreaManagerDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeGAM";

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
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 1u, new FakeGameAreaManager.Data());
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
                var s = DataSerializer.Deserialize<FakeGameAreaManager.Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}