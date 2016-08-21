using System;

namespace EvoVI.Database
{
    public static class PlayerData
    {
        #region Constants (Parameter Names)
        const string PARAM_PILOT_NAME = "PILOT NAME";
        const string PARAM_TOTAL_KILLS = "TOTAL KILLS";
        const string PARAM_TOTAL_CONTRACTS = "TOTAL CONTRACTS";
        const string PARAM_SKILL_AND_PROFICIENCY_RATING = "SKILL AND PROFICIENCY RATING";
        const string PARAM_MILITARY_RANK = "MILITARY RANK";
        const string PARAM_CASH = "CASH";
        #endregion


        #region Variables - Converted Values
        private static int _cash;
        #endregion


        #region Properties (Converted Values)
        public static int? Cash 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)_cash : null; }
		}
        #endregion


        #region Properties (Unconverted Values)
        public static string Name 
		{
			get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (string)SaveDataReader.GetEntry(PARAM_PILOT_NAME).Value : null; }
		}


        public static int? TotalKills 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TOTAL_KILLS).Value : null; }
		}


        public static int? TotalContracts 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_TOTAL_CONTRACTS).Value : null; }
		}


        public static int? SkillAndProficiencyRating 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_SKILL_AND_PROFICIENCY_RATING).Value : null; }
		}


        public static int? MilitaryRating 
		{
            get { return (GameMeta.CurrentGame >= GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? (int?)SaveDataReader.GetEntry(PARAM_MILITARY_RANK).Value : null; }
		}
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert cash
            int.TryParse(((string)SaveDataReader.GetEntry(PARAM_CASH).Value).Replace(",", ""), out _cash);
        }
        #endregion
    }
}
