using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework.IO;
using EightyOne.ResourceManagers;
using ICities;

namespace EightyOne.DataExtensions
{
    public class WaterManagerDataExtension : SerializableDataExtensionBase
    {
        private const string id = "fakeWM";

        public override void OnSaveData()
        {
            if (!Util.IsGameMode())
            {
                return;
            }
            var wm = WaterManager.instance;
            var oldGrid = (IList)typeof(WaterManager).GetField("m_waterGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wm);
            int oldGridSize = 256;
            int diff = (FakeWaterManager.GRID - oldGridSize) / 2;
            var fields = Util.GetFieldsFromStruct(oldGrid[0], FakeWaterManager.m_waterGrid[0]);
            for (var i = 0; i < oldGridSize; i += 1)
            {
                for (var j = 0; j < oldGridSize; j += 1)
                {
                    var oldCellIndex = j * oldGridSize + i;
                    oldGrid[oldCellIndex] = Util.CopyStruct((object)oldGrid[oldCellIndex], FakeWaterManager.m_waterGrid[(j + diff) * FakeWaterManager.GRID + (i + diff)], fields);
                }
            }

            Util.CopyStructArrayBack(FakeWaterManager.m_waterPulseUnits, wm, "m_waterPulseUnits");
            Util.CopyStructArrayBack(FakeWaterManager.m_sewagePulseUnits, wm, "m_sewagePulseUnits");
            Util.CopyStructArrayBack(FakeWaterManager.m_heatingPulseUnits, wm, "m_heatingPulseUnits");

            Util.SetPropertyValueBack(FakeWaterManager.m_waterPulseGroupCount, wm, "m_waterPulseGroupCount");
            Util.SetPropertyValueBack(FakeWaterManager.m_sewagePulseGroupCount, wm, "m_sewagePulseGroupCount");
            Util.SetPropertyValueBack(FakeWaterManager.m_heatingPulseGroupCount, wm, "m_heatingPulseGroupCount");

            Util.SetPropertyValueBack(FakeWaterManager.m_waterPulseUnitEnd, wm, "m_waterPulseUnitEnd");
            Util.SetPropertyValueBack(FakeWaterManager.m_sewagePulseUnitEnd, wm, "m_sewagePulseUnitEnd");
            Util.SetPropertyValueBack(FakeWaterManager.m_heatingPulseUnitEnd, wm, "m_heatingPulseUnitEnd");


            Util.SetPropertyValueBack(FakeWaterManager.m_processedCells, wm, "m_processedCells");
            Util.SetPropertyValueBack(FakeWaterManager.m_conductiveCells, wm, "m_conductiveCells");
            Util.SetPropertyValueBack(FakeWaterManager.m_canContinue, wm, "m_canContinue");

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, BuildConfig.DATA_FORMAT_VERSION, new FakeWaterManager.Data());
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
                var s = DataSerializer.Deserialize<FakeWaterManager.Data>(ms, DataSerializer.Mode.Memory);
            }
        }
    }
}