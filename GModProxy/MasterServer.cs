namespace GModProxy
{
    public class MasterServer
    {
        public const string SourceMasterServer = "hl2master.steampowered.com";
        public const int SourceMasterPort = 27011;

        public enum Region : byte
        {
            US_EAST_COAST = 0x00,
            US_WEST_COAST = 0x01,
            SOUTH_AMERICA = 0x02,
            EUROPE = 0x03,
            ASIA = 0x04,
            AUSTRALIA = 0x05,
            MIDDLE_EAST = 0x06,
            AFRICA = 0x07,
            ALL = 0xFF,
        }



    }
}
