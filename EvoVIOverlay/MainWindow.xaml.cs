using EvoVI;
using EvoVI.Classes.Dialog;
using EvoVI.Database;
using EvoVI.Engine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EvoVIOverlay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Static App Settings
        public static bool NoLoadingAnimation = true;
        #endregion


        #region Window Behaviour Override
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        #endregion


        #region Constants
        readonly Color[] HUD_BASE_COLOR = new Color[] {
            Color.FromArgb(255, 125, 26, 65),   // Standard HUD color for red hue
            Color.FromArgb(255, 65, 125, 26),   // Standard HUD color for green hue
            Color.FromArgb(255, 26, 65, 125)    // Standard HUD color for blue hue
        };

        readonly Color[] HUD_BASE_TEXT_COLOR = new Color[] {
            Color.FromArgb(255, 0, 128, 255),   // Standard text color for red hue
            Color.FromArgb(255, 255, 0, 128),   // Standard text color for green hue
            Color.FromArgb(255, 128, 255, 0)    // Standard text color for blue hue
        };
        #endregion


        #region Variables
        private DispatcherTimer _updateTimer;
        private FileSystemWatcher _savedataWatcher;
        private FileSystemWatcher _gameConfigWatcher;
        #endregion


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();


            /* Play loading animation */
            if (!MainWindow.NoLoadingAnimation) { loadAnimation(); }


            /* Initialize all components */
            VI.Initialize();
            SpeechEngine.Initialize();
            Interactor.Initialize();
            SaveDataReader.BuildDatabase();
            LoreData.Items.BuildItemDatabase();
            LoreData.Systems.BuildSystemDatabase();
            LoreData.Tech.BuildTechDatabase();

            
            /* Load Plugins */
            PluginManager.LoadPlugins();
            EvoVI.PluginManager.InitializePlugins();

            
            /* Initialize update timer */
            _updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            _updateTimer.Tick += new EventHandler(OnUpdateTimerTick);
            _updateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _updateTimer.Start();


            /* Initialize file watchers */
            FileSystemEventHandler eventHandler;

            eventHandler = new FileSystemEventHandler(OnSaveDataChanged);
            _savedataWatcher = new FileSystemWatcher(GameMeta.DefaultSavedataDirectoryPath, GameMeta.DEFAULT_SAVEDATA_FILENAME);
            _savedataWatcher.Changed += new FileSystemEventHandler(eventHandler);
            _savedataWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
            _savedataWatcher.IncludeSubdirectories = false;
            _savedataWatcher.EnableRaisingEvents = true;
            File.SetLastWriteTimeUtc(GameMeta.DefaultSavedataPath, DateTime.UtcNow);

            eventHandler = new FileSystemEventHandler(OnGameConfigChanged);
            _gameConfigWatcher = new FileSystemWatcher(GameMeta.DefaultGameSettingsDirectoryPath, GameMeta.DEFAULT_GAMECONFIG_FILENAME);
            _gameConfigWatcher.Changed += new FileSystemEventHandler(eventHandler);
            _savedataWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
            _gameConfigWatcher.IncludeSubdirectories = false;
            _gameConfigWatcher.EnableRaisingEvents = true;
            File.SetLastWriteTimeUtc(GameMeta.DefaultGameSettingsPath, DateTime.UtcNow);
        }
        #endregion


        #region Events
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            // Title + date and time
            txtBox_TitleInfo.Text = this.Title;
            txtBlck_Time.Text = DateTime.UtcNow.ToLongDateString() + "\n" + DateTime.Now.ToLongTimeString();

            txtBlck_MainInfo.Text = "Current Node: " + ((VI.CurrentDialogNode == null) ? "N/A" : VI.CurrentDialogNode.GUIDisplayText) + "\n" +
                "Active Nodes:\n" +
                buildDialogInfo(DialogTreeBuilder.RootDialogNode);

            txtBlck_StatusInfo.Text = "Target Process: " + Interactor.TargetProcessName + "\n" +
                PluginManager.Plugins.Count + " plugins loaded";
        }


        /// <summary> Fires each time, the "savedata.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnSaveDataChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            SaveDataReader.ReadGameData();

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => { txtBox_FileUpdateStatus.Text = "[!]"; })
            );

            Thread.Sleep(100);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => { txtBox_FileUpdateStatus.Text = "[ ]"; })
            );
        }


        /// <summary> Fires each time, the "sw.cfg"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnGameConfigChanged(object sender, System.IO.FileSystemEventArgs e)
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

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => { setOverlayColor(newR, newG, newB, hueMode); })
            );
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
            this.Background = new SolidColorBrush(
                Color.FromArgb(
                    255, 
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].R) * (float)(newR / 100)),
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].G) * (float)(newG / 100)),
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].B) * (float)(newB / 100))
                )
            );

            if (this.Background is SolidColorBrush)
            {
                Color backgrnd = ((SolidColorBrush)this.Background).Color;

                float colorFactor = 1 - ((float)Math.Max(backgrnd.R, Math.Max(backgrnd.G, backgrnd.B)) / 255);
                this.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)(255 * colorFactor), (byte)(255 * colorFactor), (byte)(255 * colorFactor)));
            }
        }
        #endregion


        #region Animations
        private void loadAnimation()
        {
            double originalOpacity = this.Opacity;
            
            Storyboard story;
            DoubleAnimation animation;
            TimeSpan animDuration;

            double timeOffset = 0;
            story = new Storyboard();


            #region Fade the window in
            timeOffset += 4;
            animDuration = TimeSpan.FromSeconds(1);

            animation = new DoubleAnimation(originalOpacity, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += 0;    // No further time delay to the next step
            #endregion


            #region Enhance the Window
            timeOffset += 0;
            animDuration = TimeSpan.FromSeconds(4);

            animation = new DoubleAnimation(this.MaxWidth, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));
            story.Children.Add(animation);

            animation = new DoubleAnimation(this.MaxHeight, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));
            story.Children.Add(animation);
            timeOffset += animDuration.Seconds;
            #endregion


            #region Fade the logo in
            timeOffset += 0;
            animDuration = TimeSpan.FromSeconds(1.5);

            animation = new DoubleAnimation(0, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, img_LogoBackground);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Effect).Radius"));
            story.Children.Add(animation);

            animation = new DoubleAnimation(1, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, img_LogoBackground);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += animDuration.Seconds;
            #endregion


            #region Fade the logo out
            timeOffset += 4;
            animDuration = TimeSpan.FromSeconds(1.5);

            animation = new DoubleAnimation(15, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, img_LogoBackground);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Effect).Radius"));
            story.Children.Add(animation);

            animation = new DoubleAnimation(0, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, img_LogoBackground);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += animDuration.Seconds;
            #endregion


            #region Fade in text
            timeOffset += 1.5;
            animDuration = TimeSpan.FromSeconds(1);

            animation = new DoubleAnimation(1, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, txtBox_TitleInfo);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += 0.5;

            animation = new DoubleAnimation(1, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, txtBlck_Time);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += 0.5;

            animation = new DoubleAnimation(1, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, txtBlck_MainInfo);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += 0.5;

            animation = new DoubleAnimation(1, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, txtBlck_StatusInfo);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += 0.5;

            animation = new DoubleAnimation(0.5, animDuration);
            animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
            Storyboard.SetTarget(animation, txtBox_FileUpdateStatus);
            Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
            story.Children.Add(animation);
            timeOffset += animDuration.Seconds;
            #endregion


            // Prepare all elements before starting the animation
            this.Width = 0;
            this.Height = 0;
            this.Opacity = 0;

            img_LogoBackground.Opacity = 0;
            img_LogoBackground_blur.Radius = 15;

            txtBlck_MainInfo.Opacity = 0;
            txtBlck_StatusInfo.Opacity = 0;
            txtBlck_Time.Opacity = 0;
            txtBox_FileUpdateStatus.Opacity = 0;
            txtBox_TitleInfo.Opacity = 0;

            story.Begin();
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
                string listeningMark = currChild.IsReady ? 
                    "[" + ((currChild.Speaker == DialogBase.DialogSpeaker.PLAYER) ? "!" : "-") + "]" : 
                    "[ ]";

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
