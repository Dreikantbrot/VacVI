using System;

namespace EvoVI.Database
{
    public static class EnvironmentalData
    {
        #region Variables - Converted values
        private static OnOffState _inboundMissileAlert = OnOffState.OFF;
        private static int _navPointDistance = 0;
        #endregion


        #region Properties - Converted values
        public static OnOffState InboundMissileAlert { get { return EnvironmentalData._inboundMissileAlert; } }
        public static int NavPointDistance { get { return EnvironmentalData._navPointDistance; } }
        #endregion


        #region Properties - Unconverted values
        public static string LocalSystemName { get { return (string)SaveDataReader.SaveData[14].Value; } }
        public static int GravityLevel { get { return (int)SaveDataReader.SaveData[97].Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert inbound missile alert (line 40)
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.SaveData[39].Value, out _inboundMissileAlert);

            // TODO: Actually convert it, check how this value is being represented
            // Convert waypoint distance (line 83)
            Int32.TryParse((string)SaveDataReader.SaveData[82].Value, out _navPointDistance);
        }
        #endregion
    }
}
