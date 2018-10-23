using System.IO;
using System.Linq;
using ColossalFramework.IO;
using EightyOne.ResourceManagers;
using ICities;

namespace EightyOne.DataExtensions
{
    public class DistrictManagerDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeDM";

        public override void OnSaveData()
        {
            if (!Util.IsGameMode())
            {
                return;
            }
            int oldGridSize = 512;
            int diff = (FakeDistrictManager.GRID - oldGridSize) / 2;
            for (var i = 0; i < oldGridSize; i += 1)
            {
                for (var j = 0; j < oldGridSize; j += 1)
                {
                    DistrictManager.instance.m_districtGrid[j * oldGridSize + i] = FakeDistrictManager.districtGrid[(j + diff) * FakeDistrictManager.GRID + (i + diff)];
                    DistrictManager.instance.m_parkGrid[j * oldGridSize + i] = FakeDistrictManager.parkGrid[(j + diff) * FakeDistrictManager.GRID + (i + diff)];
                }
            }

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 2u, new FakeDistrictManager.Data());
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
                var s = DataSerializer.Deserialize<FakeDistrictManager.Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}