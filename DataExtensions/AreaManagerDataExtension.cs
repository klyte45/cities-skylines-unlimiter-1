using System.IO;
using System.Linq;
using ColossalFramework.IO;
using EightyOne.Areas;
using ICities;

namespace EightyOne.DataExtensions
{
    public class AreaManagerDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeGAM";

        public override void OnSaveData()
        {
            if (!Util.IsGameMode())
            {
                return;
            }
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