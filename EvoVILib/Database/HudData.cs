
namespace EvoVI.Database
{
    public static class HudData
    {
        #region Constants (Parameter Names)
        const string PARAM_TOTAL_HOSTILES_IN_RADAR_RANGE = "TOTAL HOSTILES IN RADAR RANGE";
        const string PARAM_NAVIGATION_CONSOLE_STATUS = "NAVIGATION CONSOLE STATUS";
        const string PARAM_BUILD_CONSOLE_STATUS = "BUILD CONSOLE STATUS";
        const string PARAM_INVENTORY_CONSOLE_STATUS = "INVENTORY CONSOLE STATUS";
        const string PARAM_TRADE_CONSOLE_STATUS = "TRADE CONSOLE STATUS";
        const string PARAM_HUD_STATUS = "HUD STATUS";
        const string PARAM_TARGET_DISPLAY_STATUS = "TARGET DISPLAY STATUS";
        #endregion


        #region Enums
        public enum HudStatus { OFF = 0, PARTIAL = 1, FULL = 2 };
        public enum TargetDisplayStatus { DETAIL = 0, LIST = 1 };
        #endregion


        #region Variables - Converted values
        private static OnOffState _navigationConsole;
        private static OnOffState _buildConsole;
        private static OnOffState _inventoryConsole;
        private static OnOffState _tradeConsole;
        private static HudStatus _hud;
        private static TargetDisplayStatus _targetDisplay;
        #endregion


        #region Properties - Converted values
        public static OnOffState NavigationConsole { get { return HudData._navigationConsole; } }
        public static OnOffState BuildConsole { get { return HudData._buildConsole; } }
        public static OnOffState InventoryConsole { get { return HudData._inventoryConsole; } }
        public static OnOffState TradeConsole { get { return HudData._tradeConsole; } }
        public static HudStatus Hud { get { return HudData._hud; } }
        public static TargetDisplayStatus TargetDisplay { get { return HudData._targetDisplay; } }
        #endregion


        #region Properties - Unconverted Values
        public static int TotalHostilesOnRadar { get { return (int)SaveDataReader.GetEntry(PARAM_TOTAL_HOSTILES_IN_RADAR_RANGE).Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert navigation console status
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.GetEntry(PARAM_NAVIGATION_CONSOLE_STATUS).Value, out _navigationConsole);

            // Only in Evochron Legacy or later
            if (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY)
            {
                // Convert build console status
                SaveDataReader.ConvertOnOffState((int)SaveDataReader.GetEntry(PARAM_BUILD_CONSOLE_STATUS).Value, out _buildConsole);
            }

            // Convert inventory console status
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.GetEntry(PARAM_INVENTORY_CONSOLE_STATUS).Value, out _inventoryConsole);

            // Convert trade console status
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.GetEntry(PARAM_TRADE_CONSOLE_STATUS).Value, out _tradeConsole);

            // Convert HUD status
            switch ((int)SaveDataReader.GetEntry(PARAM_HUD_STATUS).Value)
            {
                case 0: _hud = HudStatus.OFF; break;
                case 1: _hud = HudStatus.PARTIAL; break;
                case 2: _hud = HudStatus.FULL; break;
            }

            // Convert target display status
            switch ((int)SaveDataReader.GetEntry(PARAM_TARGET_DISPLAY_STATUS).Value)
            {
                case 0: _targetDisplay = TargetDisplayStatus.DETAIL; break;
                case 1: _targetDisplay = TargetDisplayStatus.LIST; break;
            }
        }
        #endregion
    }
}
