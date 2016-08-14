using Evo_VI.engine;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Evo_VI
{
    public partial class Overlay : Form
    {
        string _text = "";
        public Overlay()
        {
            InitializeComponent();

            /* Initialize UI */
            _text = this.Text;

            /* Initialize all components */
            SpeechEngine.Initialize();
            Interactor.Initialize();
            VI.Initialize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            string info = "\n\n" + DateTime.UtcNow.ToLongDateString() + "\n" + DateTime.Now.ToLongTimeString();
            TextRenderer.DrawText(e.Graphics, _text + info, this.Font, new Point(10, 10), SystemColors.ControlText);

            Font debugFont = new System.Drawing.Font(this.Font.FontFamily, 8, this.Font.Style);
            e.Graphics.DrawLine(new Pen(SystemColors.ControlText), 5, this.Height - 40 - 5, this.Width - 5, this.Height - 40 - 5);
            TextRenderer.DrawText(e.Graphics, "Debug", debugFont, new Point(10, this.Height - 40), SystemColors.ControlText);

            this.Invalidate();
        }
    }
}
