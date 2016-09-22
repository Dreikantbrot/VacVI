
namespace VacVI.Database
{
    /// <summary> Contains information about the HUD.</summary>
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
        /// <summary> [EMERC+] Returns the state of the navigation console.</summary>
        public static OnOffState? NavigationConsole 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (OnOffState?)HudData._navigationConsole : null; }
		}


        /// <summary> [ELGCY+] Returns the state of the build and deploy console.</summary>
        public static OnOffState? BuildConsole 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? (OnOffState?)HudData._buildConsole : null; }
		}


        /// <summary> [EMERC+] Returns the state of the inventory console.</summary>
        public static OnOffState? InventoryConsole 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (OnOffState?)HudData._inventoryConsole : null; }
		}


        /// <summary> [EMERC+] Returns the state of the ship-to-ship trade console.</summary>
        public static OnOffState? TradeConsole 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (OnOffState?)HudData._tradeConsole : null; }
		}


        /// <summary> [EMERC+] Returns the HUD display mode.</summary>
        public static HudStatus? Hud 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (HudStatus?)HudData._hud : null; }
		}


        /// <summary> [EMERC+] Returns the target display mode.</summary>
        public static TargetDisplayStatus? TargetDisplay 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (TargetDisplayStatus?)HudData._targetDisplay : null; }
		}
        #endregion


        #region Properties - Unconverted Values
        /// <summary> [EMERC+] Returns the total number of hostiles on radar.</summary>
        public static int? TotalHostilesOnRadar 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TOTAL_HOSTILES_IN_RADAR_RANGE).Value : null; }
		}
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        internal static void Update()
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
