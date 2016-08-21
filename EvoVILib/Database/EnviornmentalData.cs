using EvoVI.Classes.Math;
using System;

namespace EvoVI.Database
{
    public static class EnvironmentalData
    {
        #region Constants (Parameter Names)
        const string PARAM_LOCAL_SYSTEM_NAME = "LOCAL SYSTEM NAME";
        const string PARAM_GRAVTY_LEVEL = "GRAVITY LEVEL";
        const string PARAM_INBOUND_MISSILE_ALERT = "INBOUND MISSILE ALERT";
        const string PARAM_IN_SECTOR_WAYPOINT_X_COORDINATE = "IN-SECTOR WAYPOINT X COORDINATE";
        const string PARAM_IN_SECTOR_WAYPOINT_Y_COORDINATE = "IN-SECTOR WAYPOINT Y COORDINATE";
        const string PARAM_IN_SECTOR_WAYPOINT_Z_COORDINATE = "IN-SECTOR WAYPOINT Z COORDINATE";
        const string PARAM_SECTOR_WAYPOINT_SX_COORDINATE = "SECTOR WAYPOINT SX COORDINATE";
        const string PARAM_SECTOR_WAYPOINT_SY_COORDINATE = "SECTOR WAYPOINT SY COORDINATE";
        const string PARAM_SECTOR_WAYPOINT_SZ_COORDINATE = "SECTOR WAYPOINT SZ COORDINATE";
        const string PARAM_NAVIGATION_WAYPOINT_DISTANCE = "NAVIGATION WAYPOINT DISTANCE";
        #endregion


        #region Variables - Converted values
        private static OnOffState _inboundMissileAlert;
        private static Vector3D _waypointCoordinates = new Vector3D();
        private static Vector3D _waypointSectorCoordinates = new Vector3D();
        private static int _navPointDistance;
        #endregion


        #region Properties - Converted values
        public static OnOffState? InboundMissileAlert 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (OnOffState?)EnvironmentalData._inboundMissileAlert : null; }
		}


        public static Vector3D WaypointCoordinates 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? EnvironmentalData._waypointCoordinates : null; }
		}


        public static Vector3D WaypointSectorCoordinates 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY) ? EnvironmentalData._waypointSectorCoordinates : null; }
		}


        public static int? NavPointDistance 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)EnvironmentalData._navPointDistance : null; }
		}
        #endregion


        #region Properties - Unconverted values
        public static string LocalSystemName 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (string)SaveDataReader.GetEntry(PARAM_LOCAL_SYSTEM_NAME).Value : null; }
		}


        public static int? GravityLevel 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_GRAVTY_LEVEL).Value : null; }
		}
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert inbound missile alert
            SaveDataReader.ConvertOnOffState((int)SaveDataReader.GetEntry(PARAM_INBOUND_MISSILE_ALERT).Value, out _inboundMissileAlert);

            // Only in Evochron Legacy or later
            if (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_LEGACY)
            {
                // Convert waypoint coordinates
                _waypointCoordinates.X = (int)SaveDataReader.GetEntry(PARAM_IN_SECTOR_WAYPOINT_X_COORDINATE).Value;
                _waypointCoordinates.Y = (int)SaveDataReader.GetEntry(PARAM_IN_SECTOR_WAYPOINT_Y_COORDINATE).Value;
                _waypointCoordinates.Z = (int)SaveDataReader.GetEntry(PARAM_IN_SECTOR_WAYPOINT_Z_COORDINATE).Value;
                _waypointSectorCoordinates.X = (int)SaveDataReader.GetEntry(PARAM_SECTOR_WAYPOINT_SX_COORDINATE).Value;
                _waypointSectorCoordinates.Y = (int)SaveDataReader.GetEntry(PARAM_SECTOR_WAYPOINT_SY_COORDINATE).Value;
                _waypointSectorCoordinates.Z = (int)SaveDataReader.GetEntry(PARAM_SECTOR_WAYPOINT_SZ_COORDINATE).Value;
            }

            // TODO: Check how this value is being represented (just as integer or also as something like "4k" or ">10k")
            // Convert waypoint distance
            int.TryParse((string)SaveDataReader.GetEntry(PARAM_NAVIGATION_WAYPOINT_DISTANCE).Value, out _navPointDistance);
        }
        #endregion
    }
}
