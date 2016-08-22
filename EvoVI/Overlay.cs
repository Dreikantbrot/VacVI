using EvoVI.Engine;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using EvoVI.Classes.Dialog;
using System.Threading;
using EvoVI.Database;

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
        private NotifyIcon trayIcon;
        private bool _savedataUpdated = false;
        private Thread _resetSaveDataNotifier;
        private int _defaultWidth;
        private int _defaultHeight;
        private double _defaultOpacity;
        private Point _startPosition;
        #endregion


        #region Constructor
        public Overlay()
        {
            InitializeComponent();

            _defaultWidth = this.Width;
            _defaultHeight = this.Height;
            _defaultOpacity = this.Opacity;

            _startPosition = new Point(this.Location.X, this.Location.Y);
            
            this.Width = 0;
            this.Height = 0;
            this.Opacity = 0;

            /* Initialize UI */
            _text = this.Text;
            System.IO.File.SetLastWriteTimeUtc(GameMeta.DefaultGameSettingsPath, DateTime.UtcNow);
            if (File.Exists(GameMeta.DefaultSavedataPath)) { System.IO.File.SetLastWriteTimeUtc(GameMeta.DefaultSavedataPath, DateTime.UtcNow); }

            /* Initialize Plugins */
            PluginLoader.InitializeAll();
            
            /* Draw System Tray Smybol */
            ContextMenu trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = this.Text;
            trayIcon.Icon = new Icon(this.Icon, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }
        #endregion


        #region Events
        /// <summary> Fires each time the form needs to be drawn.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Invalidate to force redraw
            this.Invalidate();
            
            Expand();

            string updateMark = "[" + (_savedataUpdated ? "!" : " ") + "]";
            TextRenderer.DrawText(
                e.Graphics, 
                updateMark, 
                this.Font, 
                new Point(this.Width - TextRenderer.MeasureText(updateMark, this.Font).Width - 20, 10), 
                this.ForeColor
            );
            
            if (
                (_savedataUpdated) &&
                (
                    (_resetSaveDataNotifier == null) ||
                    (_resetSaveDataNotifier.ThreadState == ThreadState.Unstarted) ||
                    (_resetSaveDataNotifier.ThreadState == ThreadState.Stopped)
                )
            )
            {
                _resetSaveDataNotifier = new Thread(n => { Thread.Sleep(100); _savedataUpdated = false; });
                _resetSaveDataNotifier.Start();
            }

            string info = "\n\n" + DateTime.UtcNow.ToLongDateString() + "\n" + DateTime.Now.ToLongTimeString();
            TextRenderer.DrawText(e.Graphics, _text + info, this.Font, new Point(10, 10), this.ForeColor);

            Font debugFont = new System.Drawing.Font(this.Font.FontFamily, 8, this.Font.Style);
            string dialogInfo = "Current Node: " + VI.CurrentDialogNode.GUIDisplayText + "\nActive Nodes:\n" + buildDialogInfo(DialogTreeReader.RootDialogNode);
            TextRenderer.DrawText(e.Graphics, dialogInfo, debugFont, new Point(10, 80), this.ForeColor);

            e.Graphics.DrawLine(new Pen(this.ForeColor), 5, this.Height - 40 - 5, this.Width - 5, this.Height - 40 - 5);
            TextRenderer.DrawText(e.Graphics, PluginLoader.Plugins.Count + " plugins loaded", debugFont, new Point(10, this.Height - 40), this.ForeColor);
        }


        public void Expand()
        {

            if (this.Width < _defaultWidth)
            {
                // if (expandBottomToTop) { this.Left = (_startPosition.Y + _defaultWidth) - this.Width - 40; }
                this.Width = Math.Min(this.Width + 5, _defaultWidth);
            }
            
            if (this.Height < _defaultHeight)
            {
                // if (expandRightToLeft) { this.Top = (_startPosition.Y + _defaultHeight) - this.Height - 40; }
                this.Height = Math.Min(this.Height + 5, _defaultHeight);
            }

            if (this.Opacity < _defaultOpacity) { this.Opacity = Math.Min(this.Opacity + 0.01, _defaultOpacity); }
        }


        /// <summary> Fires each time, the "savedata.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void GameDataWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            SaveDataReader.ReadGameData();
            _savedataUpdated = true;
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

            int.TryParse(configContent[24], out newR);
            int.TryParse(configContent[25], out newG);
            int.TryParse(configContent[26], out newB);
            int.TryParse(configContent[23], out hueMode);

            hueMode = 2 - hueMode;

            setOverlayColor(newR, newG, newB, hueMode);
        }


        /// <summary> Fires each time the form has been closed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The form closed event arguments.</param>
        private void Overlay_FormClosing(object sender, FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            this.ShowInTaskbar = false;

            if (
                (_resetSaveDataNotifier != null) &&
                (_resetSaveDataNotifier.ThreadState != ThreadState.Unstarted) &&
                (_resetSaveDataNotifier.ThreadState != ThreadState.Stopped) &&
                (_resetSaveDataNotifier.ThreadState != ThreadState.Aborted) &&
                (_resetSaveDataNotifier.ThreadState != ThreadState.AbortRequested)
           )
            { _resetSaveDataNotifier.Abort(); }
        }

        /// <summary> Fires each time the user s closing the application via the tray icon.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnExit(object sender, EventArgs e)
        {
            this.Close();
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


        /// <summary> Updates the filepath filters for all file watchers.
        /// </summary>
        public void UpdatePaths()
        {
            GameDataWatcher.Filter = GameMeta.DefaultSavedataPath;
            GameConfigWatcher.Filter = GameMeta.DefaultGameSettingsPath;
        }
        #endregion


        #region Overrides
        #region Form Style Constants
        // Source: 
        /// <Summary> The window accepts drag-drop files.
        /// </Summary>
        const int WS_EX_ACCEPTFILES = 0x00000010;

        /// <Summary> Forces a top-level window onto the taskbar when the window is visible.
        /// </Summary>
        const int WS_EX_APPWINDOW = 0x00040000;

        /// <Summary> The window has a border with a sunken edge.
        /// </Summary>
        const int WS_EX_CLIENTEDGE = 0x00000200;

        /// <Summary> Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// Windows 2000:  This style is not supported.
        /// </Summary>
        const int WS_EX_COMPOSITED = 0x02000000;

        /// <Summary> The title bar of the window includes a question mark. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
        /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
        /// </Summary>
        const int WS_EX_CONTEXTHELP = 0x00000400;

        /// <Summary> The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
        /// </Summary>
        const int WS_EX_CONTROLPARENT = 0x00010000;

        /// <Summary> The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
        /// </Summary>
        const int WS_EX_DLGMODALFRAME = 0x00000001;

        /// <Summary> The window is a layered window. This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
        /// Windows 8:  The WS_EX_LAYERED style is supported for top-level windows and child windows. Previous Windows versions support WS_EX_LAYERED only for top-level windows.
        /// </Summary>
        const int WS_EX_LAYERED = 0x00080000;

        /// <Summary> If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.
        /// </Summary>
        const int WS_EX_LAYOUTRTL = 0x00400000;

        /// <Summary> The window has generic left-aligned properties. This is the default.
        /// </Summary>
        const int WS_EX_LEFT = 0x00000000;

        /// <Summary> If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
        /// </Summary>
        const int WS_EX_LEFTSCROLLBAR = 0x00004000;

        /// <Summary> The window text is displayed using left-to-right reading-order properties. This is the default.
        /// </Summary>
        const int WS_EX_LTRREADING = 0x00000000;

        /// <Summary> The window is a MDI child window.
        /// </Summary>
        const int WS_EX_MDICHILD = 0x00000040;

        /// <Summary> A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
        /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
        /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
        /// </Summary>
        const int WS_EX_NOACTIVATE = 0x08000000;

        /// <Summary> The window does not pass its window layout to its child windows.
        /// </Summary>
        const int WS_EX_NOINHERITLAYOUT = 0x00100000;

        /// <Summary> The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        /// </Summary>
        const int WS_EX_NOPARENTNOTIFY = 0x00000004;

        /// <Summary> The window does not render to a redirection surface. This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        /// </Summary>
        const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

        /// <Summary> The window is an overlapped window.
        /// </Summary>
        const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);

        /// <Summary> The window is palette window, which is a modeless dialog box that presents an array of commands.
        /// </Summary>
        const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

        /// <Summary> The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
        /// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
        /// </Summary>
        const int WS_EX_RIGHT = 0x00001000;

        /// <Summary> The vertical scroll bar (if present) is to the right of the client area. This is the default.
        /// </Summary>
        const int WS_EX_RIGHTSCROLLBAR = 0x00000000;

        /// <Summary> If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
        /// </Summary>
        const int WS_EX_RTLREADING = 0x00002000;

        /// <Summary> The window has a three-dimensional border style intended to be used for items that do not accept user input.
        /// </Summary>
        const int WS_EX_STATICEDGE = 0x00020000;

        /// <Summary> The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
        /// </Summary>
        const int WS_EX_TOOLWINDOW = 0x00000080;

        /// <Summary> The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
        /// </Summary>
        const int WS_EX_TOPMOST = 0x00000008;

        /// <Summary> The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
        /// To achieve transparency without these restrictions, use the SetWindowRgn function.
        /// </Summary>
        const int WS_EX_TRANSPARENT = 0x00000020;

        /// <Summary>
        /// </Summary>
        const int WS_EX_WINDOWEDGE = 0x00000100;
        #endregion


        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= (WS_EX_TOPMOST | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT);

                return createParams;
            }
        }
        #endregion


        #region [[ Debug ]]
        private string buildDialogInfo(DialogBase parentNode, int level = 1)
        {
            string dialogInfo = "";

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                DialogBase currChild = parentNode.ChildNodes[i];

                // Is listening mark
                string listeningMark = currChild.IsReady ? "[!]" : "[ ]";

                // Prepare spacing
                string arrow = (
                    currChild.IsActive ? "====" :
                    (parentNode.IsActive) ? "----" :
                    "    "
                );

                string importanceMark = "[";
                for (DialogBase.DialogImportance u = DialogBase.DialogImportance.LOW; u < currChild.Importance; u++) { importanceMark += "*"; }
                for (DialogBase.DialogImportance u = currChild.Importance; u < DialogBase.DialogImportance.CRITICAL; u++) { importanceMark += " "; }
                importanceMark += "]";

                // Prepare "dock"
                System.Text.StringBuilder dock = new System.Text.StringBuilder();
                dock.Append("|");
                dock.Append('-', level * 2);

                // Draw "is active" mark
                if (!currChild.Disabled)
                {
                    dialogInfo += "" +
                        listeningMark + "" +
                        importanceMark + "" +
                        arrow + dock + " " +
                        currChild.Speaker + ": " + currChild.GUIDisplayText + "\n";
                }
                dialogInfo += buildDialogInfo(currChild, level + 1);
            }

            return dialogInfo;
        }
        #endregion
    }
}
