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
                DataSerializer.Deserialize<FakeGameAreaManager.Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}