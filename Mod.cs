using ICities;

namespace EightyOne
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get { return "Allows to use all 81 map tiles"; }
        }

        public string Name
        {
            get { return "81 Tiles (Fixed for C:S 1.2+)"; }
        }
    }
}
