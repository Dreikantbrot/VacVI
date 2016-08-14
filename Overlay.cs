using Evo_VI.engine;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Evo_VI
{
    public partial class Overlay : Form
    {
        public static Label lbl_dbg;
        public static string lbl_dbg_oriTxt;

        public Overlay()
        {
            InitializeComponent();

            lbl_dbg = lbl_OverlayTitle;
            lbl_dbg_oriTxt = lbl_OverlayTitle.Text;

            /* Initialize all components */
            SpeechEngine.Initialize();
            Interactor.Initialize();
            VI.Initialize();
        }
    }
}
