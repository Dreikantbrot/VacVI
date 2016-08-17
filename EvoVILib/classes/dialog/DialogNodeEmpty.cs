using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.classes.dialog
{
    public class DialogNodeEmpty : DialogNode
    {
        public DialogNodeEmpty(string pText = " ", DialogImportance pImportance = DialogImportance.NORMAL, IPlugin pPluginToStart = null) : 
        base(pText, pImportance, pPluginToStart)
        {
            this._speaker = DialogSpeaker.NOBODY;
        }
    }
}
