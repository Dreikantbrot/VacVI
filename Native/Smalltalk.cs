using VacVI.Dialog;
using VacVI.Database;
using VacVI.Plugins;
using System;
using System.Collections.Generic;
using VacVI;

namespace Native
{
    public class Smalltalk : IPlugin
    {
        #region Plugin Info
        public GameMeta.SupportedGame CompatibilityFlags
        {
            get { return (GameMeta.SupportedGame.EVOCHRON_MERCENARY | GameMeta.SupportedGame.EVOCHRON_LEGACY); }
        }

        public string Description
        {
            get
            {
                return "This Plugin enables the VI to engage in smalltalk with the player.\n" +
                    "The conversations themselves don't affect the ship's operations and serve primarily to build the " +
                    "relationship between the player and the VI.";
            }
        }

        public Guid Id
        {
            get { return Guid.Parse("bc711d6d-349d-4c47-a9e7-d9edc7c47cd1"); }
        }

        public string Name
        {
            get { return "Smalltalk"; }
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

        public System.Drawing.Bitmap LogoImage
        {
            get { return Properties.Resources.Smalltalk; }
        }
        #endregion


        #region Constants
        private const string DIALOG_SUPER_RICH = "It's... {0} Credits!... But... you do pay your taxes, right?";
        private const string DIALOG_VERY_RICH = "{0} credits... um... since you have so much money: Can I perhaps borrow just a little bit?";
        private const string DIALOG_POOR = "{0} credits - Time to check for a job offering, don't you agree?";
        private const string DIALOG_NORMAL = "$[Your bank account is clocking in at |You have ]{0} credits.";
        #endregion


        #region Variables
        private Dictionary<string, string> _parameter = new Dictionary<string, string>();

        private DialogVI _dialg_cash_superRich = new DialogVI(DIALOG_SUPER_RICH);
        private DialogVI _dialg_cash_veryRich = new DialogVI(DIALOG_VERY_RICH);
        private DialogVI _dialg_cash_poor = new DialogVI(DIALOG_POOR);
        private DialogVI _dialg_cash_normal = new DialogVI(DIALOG_NORMAL);
        #endregion


        #region Interface Functions
        public List<PluginParameterDefault> GetDefaultPluginParameters()
        {
            return new List<PluginParameterDefault>();
        }

        public void Initialize()
        {

        }

        public void BuildDialogTree()
        {
            DialogTreeBranch[] dialog = new DialogTreeBranch[] {
                new DialogTreeBranch(
                    new DialogPlayer("Who are you?"),
                    new DialogTreeBranch(
                        new DialogVI(
                            String.Format(
                                "I am a virtual intelligence and ship assistance software. " + 
                                "$(My name is|You can call me|Please, call me|Call me) \"<{0}-->{1}>\"." + ";" + 
                                "I am <{0}-->{1}> - $[a virtual intelligence and ]your personal ship assistant.", 
                                VI.Name, 
                                VI.PhoneticName
                            )
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "What does my bank account say?" + ";" + 
                        "How$('s| is) my bank account?"
                    ),
                    new DialogTreeBranch(
                        new DialogCommand(
                            "Give comment about the player's amount of cash",
                            DialogBase.DialogPriority.NORMAL,
                            null,
                            this.Id.ToString(),
                            "say_cash"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer(
                        "$[Are ]you ready?"
                    ),
                    new DialogTreeBranch(
                        new DialogCommand(
                            "$[I'm ]ready$[ - let's do this]!"
                        )
                    )
                ),

                new DialogTreeBranch(
                    new DialogPlayer("What is your favourite food"),
                    new DialogTreeBranch(
                        new DialogVI("I like $(muffins|cornflakes|pizza|pancakes|small children for breakfast). What do you like most?"),
                        new DialogTreeBranch(
                            new DialogPlayer("I like $(soup|pizza|muffins|cornflakes)."),
                            new DialogTreeBranch(
                                new DialogVI("Good choice!")
                            )
                        ),
                        new DialogTreeBranch(
                            new DialogPlayer("I don't eat."),
                            new DialogTreeBranch(
                                new DialogVI("You should! Your $(parents|mother|father) will worry $[about you] otherwise!")
                            )
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialog);
        }

        public void OnDialogAction(VacVI.Dialog.DialogBase originNode)
        {
            switch (originNode.Data.ToString())
            {
                case "say_cash":
                    sayHowMyBankAccountIsDoing();
                    break;
            }
        }

        public void OnGameDataUpdate()
        {

        }

        public void OnProgramShutdown()
        {

        }
        #endregion


        #region Custom Functions
        private void sayHowMyBankAccountIsDoing()
        {
            if (PlayerData.Cash > 1000000)
            {
                _dialg_cash_superRich.RawText = String.Format(DIALOG_SUPER_RICH, PlayerData.Cash.ToString());
                SpeechEngine.Say(_dialg_cash_superRich);
            }
            else if (PlayerData.Cash > 500000)
            {
                _dialg_cash_veryRich.RawText = String.Format(DIALOG_VERY_RICH, PlayerData.Cash.ToString());
                SpeechEngine.Say(_dialg_cash_veryRich);
            }
            else if (PlayerData.Cash < 1000)
            {
                _dialg_cash_poor.RawText = String.Format(DIALOG_POOR, PlayerData.Cash.ToString());
                SpeechEngine.Say(_dialg_cash_poor);
            }
            else
            {
                _dialg_cash_normal.RawText = String.Format(DIALOG_NORMAL, PlayerData.Cash.ToString());
                SpeechEngine.Say(_dialg_cash_normal);
            }
        }
        #endregion
    }
}
