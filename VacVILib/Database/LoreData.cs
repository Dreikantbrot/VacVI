using VacVI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VacVI.Database
{
    /// <summary> Contains information about the game's lore.</summary>
    public static class LoreData
    {
        /// <summary> Contains information about items like commodities.</summary>
        public static class Items
        {
            #region Regexes (readonly)
            readonly private static Regex ITEM_PARSE_REGEX = new Regex(
                @"\+Item(?<ItemNr>\d+)\s+" +
                @"Lines=(?<DescrLineNr>\d+)\s+" +
                @"(?<Description>[\s\S]+?(?=\s*\+|\s*$))"
            );
            #endregion


            #region Variables
            private static List<ItemEntry> _itemDatabase = new List<ItemEntry>();
            #endregion


            #region Properties
            /// <summary> Returns the item database.
            /// </summary>
            public static List<ItemEntry> ItemDatabase
            {
                get { return _itemDatabase; }
            }
            #endregion


            #region Structs
            [System.Diagnostics.DebuggerDisplay("Item{_itemNr}: {_description}")]
            public struct ItemEntry
            {
                #region Variables
                private int _itemNr;
                private int _descrLineNr;
                private string _description;
                #endregion


                #region Properties
                /// <summary> Returns the item number.
                /// </summary>
                public int ItemNr
                {
                    get { return _itemNr; }
                }


                /// <summary> Resturns the number of description lines.
                /// </summary>
                public int DescrLineNr
                {
                    get { return _descrLineNr; }
                }

                /// <summary> Returns the description.
                /// </summary>
                public string Description
                {
                    get { return _description; }
                }
                #endregion


                #region Constructor
                /// <summary> Creates a new item entry for the database.
                /// </summary>
                /// <param name="pItemNr">The number (ID) of the item.</param>
                /// <param name="pDescrLineNr">The number of lines used for the description.</param>
                /// <param name="pDescription">The item description text.</param>
                public ItemEntry(int pItemNr, int pDescrLineNr, string pDescription)
                {
                    this._itemNr = pItemNr;
                    this._descrLineNr = pDescrLineNr;
                    this._description = pDescription;
                }
                #endregion
            }
            #endregion


            #region Functions
            internal static void BuildItemDatabase()
            {
                string filePath = GameMeta.CurrentGameDirectoryPath + "\\" + "itemdata.dat";
                _itemDatabase.Clear();

                if (!File.Exists(filePath)) { return; }

                string fileContent = File.ReadAllText(filePath).Replace("\r", "");
                MatchCollection itemMatches = ITEM_PARSE_REGEX.Matches(fileContent);

                for (int i = 0; i < itemMatches.Count; i++)
                {
                    int itemNr;
                    int descrLineNr;

                    int.TryParse(itemMatches[i].Groups["ItemNr"].Value, out itemNr);
                    int.TryParse(itemMatches[i].Groups["DescrLineNr"].Value, out descrLineNr);

                    _itemDatabase.Add(
                        new ItemEntry(
                            itemNr,
                            descrLineNr,
                            itemMatches[i].Groups["Description"].Value
                        )
                    );
                }
            }
            #endregion
        }

        
        /// <summary> Contains information about star systems.</summary>
        public static class Systems
        {
            #region Regexes (readonly
            readonly private static Regex SYSTEM_PARSE_REGEX = new Regex(
                @"((?:[+-]?\d+?\s+?[+-]?\d+)" +
                @"[\s\S]+?(?=\s*(?:\n){2}[+-]?\d|\s*$))"
            );

            readonly private static Regex SYSTEM_DETAIL_REGEX_EMERC = new Regex(
                @"(?<X>[+-]?\d+)\s+" + 
                @"(?<Y>[+-]?\d+)\s+" +
                @"(?<Name>.*?):\s*" +
                @"(?<Description>[\S\s]+?(?=$|\n\n))" + 
                @"(?:Economy Classes:\s*(?<Economy>.*?\n))?\s*" +
                @"(?:Faction Details:\s*(?<Factions>[\S\s]*?(?=\s*$|\s*[+-]?\d+)))?"
            );

            readonly private static Regex SYSTEM_DETAIL_REGEX_ELGCY = new Regex(
                @"(?<X>[+-]?\d+)\s+" +
                @"(?<Y>[+-]?\d+)\s+" +
                @"(?<Name>.*?) System Information:\s*" +
                @"(?<Description>[\S\s]+?)" +
                @"(?:Alerts:\s*(?<Alerts>.*?(?=$|\n\n)))\s*" +
                @"(?:Economy Classes:\s*(?<Economy>.*?\n))?"
            );

            readonly private static Regex FACTION_DETAILS_PARSE_REGEX = new Regex(
                @"(?<Faction>.+?)\s*-\s*(?<Status>.+?(?=\s*\n|\s*$))"
            );
            #endregion


            #region Structs
            [System.Diagnostics.DebuggerDisplay("{_name}")]
            public struct SystemEntry
            {
                #region Variables
                private string _name;
                private Vector3D _sectorCoordinates;
                private string _description;
                private string[] _economyClasses;
                private Dictionary<string, string> _factions;
                private string _alerts;
                #endregion


                #region Properties
                /// <summary>[EMERC+] Returns the system name.
                /// </summary>
                public string Name
                {
                    get { return _name; }
                }


                /// <summary>[EMERC+] Returns the system sector coordinates.
                /// </summary>
                public Vector3D SectorCordinates
                {
                    get { return _sectorCoordinates; }
                }


                /// <summary>[EMERC+] Returns the system description
                /// </summary>
                public string Description
                {
                    get { return _description; }
                }

                /// <summary>[EMERC+] Returns a list of economy classes.
                /// </summary>
                public string[] EconomyClasses
                {
                    get { return _economyClasses; }
                }

                /// <summary>[EMERC] Returns the factions within the system (key) and their status inside it (value).
                /// </summary>
                public Dictionary<string, string> Factions
                {
                    get { return _factions; }
                }

                /// <summary>[ELGCY] Returns alerts about the system (e.g. nearby black holes).
                /// </summary>
                public string Alerts
                {
                    get { return _alerts; }
                }
                #endregion


                #region Constructors
                /// <summary> Creates an entry for a star system in the database.
                /// </summary>
                /// <param name="pName">The name of the system.</param>
                /// <param name="pDescription">The system description text.</param>
                /// <param name="pSectorCoordinates">The sector coordinates.</param>
                /// <param name="pEconomyClasses">The list of economy tags for this system.</param>
                /// <param name="pFactions">A dictionary containing present factions as key and their satus within the system as value.</param>
                public SystemEntry(string pName, string pDescription, Vector3D pSectorCoordinates, string[] pEconomyClasses, Dictionary<string, string> pFactions, string pAlerts)
                {
                    this._name = pName;
                    this._sectorCoordinates = pSectorCoordinates;
                    this._description = pDescription;
                    this._economyClasses = pEconomyClasses;
                    this._factions = pFactions;
                    this._alerts = pAlerts;
                }

                public SystemEntry(string pName, string pDescription, double pSectorX, double pSectorY, string[] pEconomyClasses, Dictionary<string, string> pFactions, string pAlerts) :
                    this(pName, pDescription, new Vector3D(pSectorX, pSectorY), pEconomyClasses, pFactions, pAlerts) { }
                #endregion
            }
            #endregion


            #region Variables
            private static List<SystemEntry> _systemDatabase = new List<SystemEntry>();
            #endregion


            #region Properties
            /// <summary> Returns the system database.
            /// </summary>
            public static List<SystemEntry> SystemDatabase
            {
                get { return Systems._systemDatabase; }
            }
            #endregion


            #region Functions
            internal static void BuildSystemDatabase()
            {
                _systemDatabase.Clear();

                if (!File.Exists(GameMeta.CurrentGameDirectoryPath + "\\systemdata.dat")) { return; }

                string fileContent = File.ReadAllText(GameMeta.CurrentGameDirectoryPath + "\\systemdata.dat").Replace("\r", "");
                MatchCollection systemMatches = SYSTEM_PARSE_REGEX.Matches(fileContent);

                for (int i = 0; i < systemMatches.Count; i++)
                {
                    Match systemDetails = (
                        (GameMeta.CurrentGame == GameMeta.SupportedGame.EVOCHRON_MERCENARY) ? SYSTEM_DETAIL_REGEX_EMERC.Match(systemMatches[i].Value) : 
                        (GameMeta.CurrentGame == GameMeta.SupportedGame.EVOCHRON_LEGACY) ? SYSTEM_DETAIL_REGEX_ELGCY.Match(systemMatches[i].Value) :
                        SYSTEM_DETAIL_REGEX_EMERC.Match(systemMatches[i].Value)     // <-- Use Mercenary's patern as the fallback default
                    );

                    double x = 0;
                    double y = 0;
                    double.TryParse(systemDetails.Groups["X"].Value, out x);
                    double.TryParse(systemDetails.Groups["Y"].Value, out y);

                    string[] economyClasses = systemDetails.Groups["Economy"].Value.Split(',');
                    for (int u = 0; u < economyClasses.Length; u++) { economyClasses[u] = economyClasses[u].Trim(); }

                    Dictionary<string, string> factionDetails = new Dictionary<string, string>();
                    MatchCollection factionDetailMatches = FACTION_DETAILS_PARSE_REGEX.Matches(systemDetails.Groups["Factions"].Value);

                    for (int u = 0; u < factionDetailMatches.Count; u++)
                    {
                        factionDetails.Add(factionDetailMatches[u].Groups["Faction"].Value, factionDetailMatches[u].Groups["Status"].Value);
                    }

                    _systemDatabase.Add(
                        new SystemEntry(
                            systemDetails.Groups["Name"].Value,
                            systemDetails.Groups["Description"].Value,
                            x,
                            y,
                            economyClasses,
                            factionDetails,
                            systemDetails.Groups["Alerts"].Value
                        )
                    );
                }
            }
            #endregion
        }


        /// <summary> Contains information about tech, like ship frames and ship components.</summary>
        public static class Tech
        {
            #region Regexes (readonly)
            readonly private static Regex ITEM_PARSE_REGEX = new Regex(
                @"\+(?<ItemType>[a-zA-Z]+)" + 
                @"(?<ItemNr>\d+)\s+" +
                @"Lines=(?<DescrLineNr>\d+)\s+" +
                @"(?<Description>[\s\S]+?(?=\s*\+|\s*$))"
            );
            #endregion


            #region Variables
            private static List<TechEntry> _techDatabase = new List<TechEntry>();
            #endregion


            #region Properties
            /// <summary> Returns the tech database.
            /// </summary>
            public static List<TechEntry> TechDatabase
            {
                get { return _techDatabase; }
            }
            #endregion


            #region Structs
            [System.Diagnostics.DebuggerDisplay("{_itemType}{_techNr}: {_description}")]
            public struct TechEntry
            {
                #region Variables
                private int _techNr;
                private string _itemType;
                private int _descrLineNr;
                private string _description;
                #endregion


                #region Properties
                /// <summary> Returns the tech number.
                /// </summary>
                public int TechNr
                {
                    get { return _techNr; }
                }


                /// <summary> Returns the item type.
                /// </summary>
                public string ItemType
                {
                    get { return _itemType; }
                }


                /// <summary> Resturns the number of description lines.
                /// </summary>
                public int DescrLineNr
                {
                    get { return _descrLineNr; }
                }

                /// <summary> Returns the description.
                /// </summary>
                public string Description
                {
                    get { return _description; }
                }
                #endregion


                #region Constructor
                /// <summary> Creates an entry for an in-game technology for the database.
                /// </summary>
                /// <param name="pItemNr">The number (ID) of the item.</param>
                /// <param name="pItemType">The type/classification of the tech.</param>
                /// <param name="pDescrLineNr">The number of lines used for the description.</param>
                /// <param name="pDescription">The description text.</param>
                public TechEntry(int pItemNr, string pItemType, int pDescrLineNr, string pDescription)
                {
                    this._techNr = pItemNr;
                    this._itemType = pItemType;
                    this._descrLineNr = pDescrLineNr;
                    this._description = pDescription;
                }
                #endregion
            }
            #endregion


            #region Functions
            internal static void BuildTechDatabase()
            {
                string filePath = GameMeta.CurrentGameDirectoryPath + "\\" + "techdata.dat";
                _techDatabase.Clear();

                if (!File.Exists(filePath)) { return; }

                string fileContent = File.ReadAllText(filePath).Replace("\r", "");
                MatchCollection itemMatches = ITEM_PARSE_REGEX.Matches(fileContent);

                for (int i = 0; i < itemMatches.Count; i++)
                {
                    int itemNr;
                    int descrLineNr;

                    int.TryParse(itemMatches[i].Groups["ItemNr"].Value, out itemNr);
                    int.TryParse(itemMatches[i].Groups["DescrLineNr"].Value, out descrLineNr);

                    _techDatabase.Add(
                        new TechEntry(
                            itemNr,
                            itemMatches[i].Groups["ItemType"].Value,
                            descrLineNr,
                            itemMatches[i].Groups["Description"].Value
                        )
                    );
                }
            }
            #endregion
        }
    }
}
