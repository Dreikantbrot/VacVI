using Evo_VI.engine;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Evo_VI
{
    public partial class Overlay : Form
    {
        public Overlay()
        {
            InitializeComponent();

            /* Initialize all components */
            SpeechEngine.Initialize();
            Interactor.Initialize();
        }
    }
}
