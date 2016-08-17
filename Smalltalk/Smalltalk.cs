using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoVI.classes.dialog;

namespace Smalltalk
{
    public class Smalltalk : IPlugin
    {
        #region GUID (readonly)
        readonly Guid GUID = Guid.NewGuid();
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
            DialogTreeStruct[] dialogTree;
            
            dialogTree = new DialogTreeStruct[] { 
                new DialogTreeStruct(
                    new DialogNodePlayer("{Hey|Hi}[ what's up?];Hello[ there]."),
                    new DialogTreeStruct[] { 
                        new DialogTreeStruct(
                            new DialogNodeVI("Hi"),
                            new DialogTreeStruct[] {
                            
                                new DialogTreeStruct(
                                    new DialogNodePlayer("Are you okay?"),
                                    new DialogTreeStruct[] {                                        
                                        new DialogTreeStruct(
                                            new DialogNodeVI("Just a little bummed out. I want to be finished soon."),
                                            new DialogTreeStruct[] { }
                                        )
                                    }
                                ),
                            
                                new DialogTreeStruct(
                                    new DialogNodePlayer("What's new?"),
                                    new DialogTreeStruct[] {
                                        new DialogTreeStruct(
                                            new DialogNodeVI("Nothing new at the western front."),
                                            new DialogTreeStruct[] { }
                                        )
                                    }
                                )
                            }
                        )
                    }
                )
            };

            DialogTreeReader.BuildDialogTree(dialogTree);
        }

        public void OnDialogAction(EvoVI.classes.dialog.DialogNode originNode)
        {

        }

        public void OnGameDataUpdate()
        {

        }
        #endregion
    }
}
