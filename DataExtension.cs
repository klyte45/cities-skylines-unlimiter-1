using ICities;


namespace Unlimiter
{
    public class DataExtension : SerializableDataExtensionBase
    {
        public override void OnLoadData()
        {
            // Deserialize is called from within LimitTreeManager.Data.Deserialize
        }

        public override void OnSaveData()
        {
            //LimitTreeManager.CustomSerializer.Serialize();
        }
    }
}
