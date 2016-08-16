using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smalltalk
{
    public class Smalltalk : IPlugin
    {
        #region GUID (readonly)
        readonly Guid GUID = new Guid("Smalltalk");
        #endregion


        #region Plugin Info
        public string Description
        {
            get
            {
                return "This Plugin enables the VI to engage in smalltalk with the player.\n" +
                    "The conversations themselves don't affect the ship's operations and serve primarily to build the " +
                    "relationship between the player and the VI";
            }
        }

        public Guid Id
        {
            get { return GUID; }
        }

        public string Name
        {
            get { return "Smalltalk"; }
        }

        public string Version
        {
            get { return "0.1"; }
        }
        #endregion


        #region Interface Functions
        public void Initialize()
        {
            // TODO: Build dialog tree
        }

        public void OnDialogAction(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e, EvoVI.classes.dialog.DialogNode originNode)
        {

        }

        public void OnGameDataUpdate()
        {

        }
        #endregion
    }
}
