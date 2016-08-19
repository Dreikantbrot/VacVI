using EvoVI.engine;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using EvoVI.classes.dialog;

namespace EvoVI
{
    public partial class Overlay : Form
    {
        #region Constants
        readonly Color[] HUD_BASE_COLOR = new Color[] {
            Color.FromArgb(125, 26, 65),                // Standard HUD color for red hue
            Color.FromArgb(65, 125, 26),                // Standard HUD color for green hue
            Color.FromArgb(26, 65, 125)                 // Standard HUD color for blue hue
        };

        readonly Color[] HUD_BASE_TEXT_COLOR = new Color[] {
            Color.FromArgb(0, 128, 255),                // Standard text color for red hue
            Color.FromArgb(255, 0, 128),                // Standard text color for green hue
            Color.FromArgb(128, 255, 0)                 // Standard text color for blue hue
        };
        #endregion


        #region Variables
        private string _text = "";

        private string _debugMsg = "";
        private DateTime _lastDebugMsg = DateTime.Now;
        #endregion


        #region Constructor
        public Overlay()
        {
            InitializeComponent();

            /* Initialize UI */
            _text = this.Text;
            System.IO.File.SetLastWriteTimeUtc("C:\\sw3dg\\EvochronMercenary\\sw.cfg", DateTime.UtcNow);
            System.IO.File.SetLastWriteTimeUtc("C:\\sw3dg\\EvochronMercenary\\savedata.txt", DateTime.UtcNow);

            /* Initialize Plugins */
            PluginLoader.InitializeAll();
            
        }
        #endregion


        #region Events
        private string buildDialogInfo(DialogBase parentNode, int level = 1)
        {
            string dialogInfo = "";

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                DialogBase currChild = parentNode.ChildNodes[i];

                // Is listening mark
                string listeningMark = currChild.IsReady ? "[!]" : "[ ]";

                // Prepare spacing
                string arrow = "" +
                (
                    currChild.IsActive ? "====" :
                    (parentNode.IsActive) ? "----" :
                    "    "
                );

                string importanceMark = "[";
                for (DialogBase.DialogImportance u = DialogBase.DialogImportance.LOW; u < currChild.Importance; u++) { importanceMark += "*"; }
                for (DialogBase.DialogImportance u = currChild.Importance; u < DialogBase.DialogImportance.CRITICAL; u++) { importanceMark += " "; }
                importanceMark += "]";

                // Prepare "dock"
                string dock = "|";
                for (int u = 0; u < level; u++) { dock += "--"; }

                // Draw "is active" mark
                if (!currChild.Disabled)
                {
                    dialogInfo += "" +
                        listeningMark + "" + 
                        importanceMark + "" + 
                        arrow + dock + " " +
                        currChild.Speaker + ": " + 
                        (
                            currChild.Speaker == DialogBase.DialogSpeaker.PLAYER ||
                            currChild.Speaker == DialogBase.DialogSpeaker.COMMAND
                            ? currChild.Text : "<???>"
                        ) + "\n";
                }
                dialogInfo += buildDialogInfo(currChild, level + 1);
            }
            
            return dialogInfo;
        }

        /// <summary> Fires each time the form needs to be drawn.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            string info = "\n\n" + DateTime.UtcNow.ToLongDateString() + "\n" + DateTime.Now.ToLongTimeString();
            TextRenderer.DrawText(e.Graphics, _text + info, this.Font, new Point(10, 10), this.ForeColor);

            Font debugFont = new System.Drawing.Font(this.Font.FontFamily, 8, this.Font.Style);
            string dialogInfo = "Current Node: " + VI.CurrentDialogNode.Text + "\nActive Nodes:\n" + buildDialogInfo(DialogTreeReader.RootDialogNode);
            TextRenderer.DrawText(e.Graphics, dialogInfo, debugFont, new Point(10, 80), this.ForeColor);

            e.Graphics.DrawLine(new Pen(this.ForeColor), 5, this.Height - 40 - 5, this.Width - 5, this.Height - 40 - 5);
            TextRenderer.DrawText(e.Graphics, PluginLoader.Plugins.Count + " plugins loaded", debugFont, new Point(10, this.Height - 40), this.ForeColor);

            // Invalidate to force redraw
            this.Invalidate();
        }


        /// <summary> Fires each time, the "savedata.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void GameDataWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Database.IngameData.SaveDataReader.ReadGameData(e.FullPath);
        }


        /// <summary> Fires each time, the "sw.cfg"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void ConfigWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath)) { return; }

            int hueMode = 2;
            int newR = HUD_BASE_COLOR[hueMode].R;
            int newG = HUD_BASE_COLOR[hueMode].G;
            int newB = HUD_BASE_COLOR[hueMode].B;
            string[] configContent = File.ReadAllLines(e.FullPath);

            Int32.TryParse(configContent[24], out newR);
            Int32.TryParse(configContent[25], out newG);
            Int32.TryParse(configContent[26], out newB);
            Int32.TryParse(configContent[23], out hueMode);

            hueMode = 2 - hueMode;

            setOverlayColor(newR, newG, newB, hueMode);
        }
        #endregion


        #region Functions
        /// <summary> Adjusts the overlay's background and text color to match the HUD.
        /// </summary>
        /// <param name="newR">The new red channel intensity.</param>
        /// <param name="newG">The new green channel intensity.</param>
        /// <param name="newB">The new blue channel intensity.</param>
        /// <param name="hueMode">The currently set hue mode.</param>
        private void setOverlayColor(int newR, int newG, int newB, int hueMode)
        {
            this.BackColor = Color.FromArgb(
                (int)Math.Min(255, (HUD_BASE_COLOR[hueMode].R) * (float)(newR / 100)),
                (int)Math.Min(255, (HUD_BASE_COLOR[hueMode].G) * (float)(newG / 100)),
                (int)Math.Min(255, (HUD_BASE_COLOR[hueMode].B) * (float)(newB / 100))
            );

            float colorFactor = 1 - ((float)Math.Max(this.BackColor.R, Math.Max(this.BackColor.G, this.BackColor.B)) / 255);
            this.ForeColor = Color.FromArgb((int)(255 * colorFactor), (int)(255 * colorFactor), (int)(255 * colorFactor));
        }
        #endregion
    }
}
