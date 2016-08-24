using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;

namespace ShipSystemControl
{
    public class ShipSystemControl : IPlugin
    {
        #region GUID (readonly)
        readonly Guid GUID = Guid.NewGuid();
        #endregion


        #region Plugin Info
        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (GameMeta.SupportedGame.EVOCHRON_MERCENARY | GameMeta.SupportedGame.EVOCHRON_LEGACY); }
        }

        public string Description
        {
            get
            {
                return "Enables the VI to control basic ship functions like switching weapons and initiating jumps.";
            }
        }

        public Guid Id
        {
            get { return GUID; }
        }

        public string Name
        {
            get { return "Ship System Control"; }
        }

        public string Version
        {
            get { return "0.1"; }
        }

        public string Author
        {
            get { return "Scavenger4711"; }
        }

        public string Homepage
        {
            get { return ""; }
        }
        #endregion


        #region Interface Functions
        public void Initialize()
        {
            // TODO: Build dialog tree
        }

        public void OnDialogAction(DialogBase originNode)
        {

        }

        public void OnGameDataUpdate()
        {

        }

        public Dictionary<string, string> GetDefaultPluginParameters()
        {
            return new Dictionary<string, string>();
        }
        #endregion
    }
}
