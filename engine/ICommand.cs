using Evo_VI.classes.dialog;
using System.Speech.Recognition;

namespace Evo_VI.engine
{
    interface ICommand
    {
        void Action(object sender, SpeechRecognizedEventArgs e, DialogNode originNode);
    }
}
