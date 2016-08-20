
namespace EvoVI.Database
{
    public static class HudData
    {
        #region Enums
        public enum HudStatus { OFF = 0, PARTIAL = 1, FULL = 2 };
        public enum TargetDisplayStatus { DETAIL = 0, LIST = 1 };
        #endregion


        #region Variables - Converted values
        private static OnOffState _navigationConsole = OnOffState.OFF;
        private static OnOffState _inventoryConsole = OnOffState.OFF;
        private static OnOffState _tradeConsole = OnOffState.OFF;
        private static HudStatus _hud = HudStatus.OFF;
        private static TargetDisplayStatus _targetDisplay = TargetDisplayStatus.DETAIL;
        #endregion


        #region Properties - Converted values
        public static OnOffState NavigationConsole { get { return HudData._navigationConsole; } }
        public static OnOffState InventoryConsole { get { return HudData._inventoryConsole; } }
        public static OnOffState TradeConsole { get { return HudData._tradeConsole; } }
        public static HudStatus Hud { get { return HudData._hud; } }
        public static TargetDisplayStatus TargetDisplay { get { return HudData._targetDisplay; } }
        #endregion


        #region Properties - Unconverted Values
        public static int TotalHostilesOnRadar { get { return (int)SaveDataReader.SaveData[93].Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert navigation console status (line 88)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[87].Value, out _navigationConsole);

            // Convert inventory console status (line 89)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[88].Value, out _inventoryConsole);

            // Convert trade console status (line 90)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[89].Value, out _tradeConsole);

            // Convert HUD status (line 92)
            switch ((int)SaveDataReader.SaveData[91].Value)
            {
                case 0: _hud = HudStatus.OFF; break;
                case 1: _hud = HudStatus.PARTIAL; break;
                case 2: _hud = HudStatus.FULL; break;
            }

            // Convert target display status (line 93)
            switch ((int)SaveDataReader.SaveData[92].Value)
            {
                case 0: _targetDisplay = TargetDisplayStatus.DETAIL; break;
                case 1: _targetDisplay = TargetDisplayStatus.LIST; break;
            }
        }
        #endregion
    }
}
