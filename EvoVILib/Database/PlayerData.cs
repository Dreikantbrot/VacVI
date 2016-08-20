using System;

namespace EvoVI.Database
{
    public static class PlayerData
    {
        #region Variables - Converted Values
        private static int _cash = 0;
        #endregion


        #region Properties (Converted Values)
        public static int Cash { get { return _cash; } }
        #endregion


        #region Properties (Unconverted Values)
        public static string Name { get { return (string)SaveDataReader.SaveData[0].Value; } }
        public static int TotalKills { get { return (int)SaveDataReader.SaveData[67].Value; } }
        public static int TotalContracts { get { return (int)SaveDataReader.SaveData[68].Value; } }
        public static int SkillAndProficiencyRating { get { return (int)SaveDataReader.SaveData[69].Value; } }
        public static int MilitaryRating { get { return (int)SaveDataReader.SaveData[70].Value; } }
        #endregion


        #region Functions
        /// <summary> Converts some data into a more convenient form.
        /// </summary>
        public static void Update()
        {
            // Convert cash (line 3)
            Int32.TryParse(((string)SaveDataReader.SaveData[2].Value).Replace(",", ""), out _cash);
        }
        #endregion
    }
}
