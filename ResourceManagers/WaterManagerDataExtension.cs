using System.Collections;
using System.IO;
using System.Reflection;
using ColossalFramework.IO;
using ICities;
using System.Linq;

namespace EightyOne.ResourceManagers
{
    public class WaterManagerDataExtension : SerializableDataExtensionBase
    {

        private const string id = "fakeWM";

        public override void OnSaveData()
        {
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

            Util.CopyStructArrayBack(FakeWaterManager.m_nodeData, wm, "m_nodeData");
            Util.CopyStructArrayBack(FakeWaterManager.m_waterPulseGroups, wm, "m_waterPulseGroups");
            Util.CopyStructArrayBack(FakeWaterManager.m_sewagePulseGroups, wm, "m_sewagePulseGroups");
            Util.CopyStructArrayBack(FakeWaterManager.m_waterPulseUnits, wm, "m_waterPulseUnits");
            Util.CopyStructArrayBack(FakeWaterManager.m_sewagePulseUnits, wm, "m_sewagePulseUnits");

            Util.SetPropertyValueBack(FakeWaterManager.m_waterPulseGroupCount, wm, "m_waterPulseGroupCount");
            Util.SetPropertyValueBack(FakeWaterManager.m_sewagePulseGroupCount, wm, "m_sewagePulseGroupCount");
            Util.SetPropertyValueBack(FakeWaterManager.m_waterPulseUnitEnd, wm, "m_waterPulseUnitEnd");
            Util.SetPropertyValueBack(FakeWaterManager.m_sewagePulseUnitEnd, wm, "m_sewagePulseUnitEnd");
            Util.SetPropertyValueBack(FakeWaterManager.m_processedCells, wm, "m_processedCells");
            Util.SetPropertyValueBack(FakeWaterManager.m_conductiveCells, wm, "m_conductiveCells");
            Util.SetPropertyValueBack(FakeWaterManager.m_canContinue, wm, "m_canContinue");

            using (var ms = new MemoryStream())
            {
                DataSerializer.Serialize(ms, DataSerializer.Mode.Memory, 1u, new FakeWaterManager.Data());
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