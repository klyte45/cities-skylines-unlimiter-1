using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework.IO;
using EightyOne.ResourceManagers;
using ICities;

namespace EightyOne.DataExtensions
{
    public class ElectricityManagerDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeEM";

        public override void OnSaveData()
        {
            if (!Util.IsGameMode())
            {
                return;
            }
            var em = ElectricityManager.instance;
            var oldGrid = (IList)typeof(ElectricityManager).GetField("m_electricityGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(em);
            int diff = (FakeElectricityManager.GRID - ElectricityManager.ELECTRICITYGRID_RESOLUTION) / 2;
            var fields = Util.GetFieldsFromStruct(oldGrid[0], FakeElectricityManager.electricityGrid[0]);
            for (var i = 0; i < ElectricityManager.ELECTRICITYGRID_RESOLUTION; i += 1)
            {
                for (var j = 0; j < ElectricityManager.ELECTRICITYGRID_RESOLUTION; j += 1)
                {
                    var oldCellIndex = j * ElectricityManager.ELECTRICITYGRID_RESOLUTION + i;
                    oldGrid[oldCellIndex] = Util.CopyStruct((object)oldGrid[oldCellIndex], FakeElectricityManager.electricityGrid[(j + diff) * FakeElectricityManager.GRID + (i + diff)], fields);
                }
            }

            Util.CopyStructArrayBack(FakeElectricityManager.m_pulseGroups, em, "m_pulseGroups");
            Util.CopyStructArrayBack(FakeElectricityManager.m_pulseUnits, em, "m_pulseUnits");

            Util.SetPropertyValueBack(FakeElectricityManager.m_pulseGroupCount, em, "m_pulseGroupCount");
            Util.SetPropertyValueBack(FakeElectricityManager.m_pulseUnitEnd % FakeElectricityManager.m_pulseUnits.Length, em, "m_pulseUnitEnd");
            Util.SetPropertyValueBack(FakeElectricityManager.m_processedCells, em, "m_processedCells");
            Util.SetPropertyValueBack(FakeElectricityManager.m_conductiveCells, em, "m_conductiveCells");
            Util.SetPropertyValueBack(FakeElectricityManager.m_canContinue, em, "m_canContinue");

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, BuildConfig.DATA_FORMAT_VERSION, new FakeElectricityManager.Data());
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
                var s = DataSerializer.Deserialize<FakeElectricityManager.Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}