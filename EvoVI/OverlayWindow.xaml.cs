using EvoVI;
using EvoVI.Dialog;
using EvoVI.Database;
using EvoVI.Input;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using EvoVI.Plugins;

namespace EvoVIOverlay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
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
        private FileSystemWatcher _keymapConfigWatcher;
        private bool _playLoadingAnimation;
        private bool _debugMode = true;
        #endregion


        #region Constructor
        public OverlayWindow()
        {
            InitializeComponent();

            /* Load the main configuration */
            ConfigurationManager.LoadConfiguration();


            /* Play loading animation */
            _playLoadingAnimation = ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(
                ConfigurationManager.SECTION_OVERLAY, 
                "Play_Intro"
            );
            if (_playLoadingAnimation)
            {
                VI.State = VI.VIState.OFFLINE;
                loadAnimation();
            }
            else
            {
                // Stop and hide the gif
                img_LogoBackground.Visibility = System.Windows.Visibility.Hidden;
                XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(img_LogoBackground, new RepeatBehavior(0));
            }


            /* Set window position */
            int posX, posY;
            Int32.TryParse(
                ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_OVERLAY, "X"),
                out posX
            );
            Int32.TryParse(
                ConfigurationManager.ConfigurationFile.GetValue(ConfigurationManager.SECTION_OVERLAY, "Y"),
                out posY
            );
            this.Left = -SystemParameters.VirtualScreenLeft + Math.Max(0, posX);
            this.Top = -SystemParameters.VirtualScreenTop + Math.Max(0, posY);


            /* Initialize all components */
            VI.Initialize();
            SpeechEngine.Initialize();
            SaveDataReader.BuildDatabase();
            KeyboardControls.BuildDatabase();
            LoreData.Items.BuildItemDatabase();
            LoreData.Systems.BuildSystemDatabase();
            LoreData.Tech.BuildTechDatabase();

            
            /* Wait for the game process to start */
            if (GameMeta.GameProcess != null)
            {
                GameMeta.GameProcess.EnableRaisingEvents = true;
                GameMeta.GameProcess.Exited += GameProcess_Exited;
            }

            
            /* Load Plugins */
            PluginManager.LoadPlugins();
            PluginManager.InitializePlugins();

            
            /* Initialize update timer */
            _updateTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
            _updateTimer.Tick += new EventHandler(OnUpdateTimerTick);
            _updateTimer.Interval = TimeSpan.FromMilliseconds(1000);
            _updateTimer.Start();


            /* Initialize file watchers */
            FileSystemEventHandler eventHandler;

            if (File.Exists(GameMeta.DefaultSavedataPath))
            {
                eventHandler = new FileSystemEventHandler(OnSaveDataChanged);
                _savedataWatcher = new FileSystemWatcher(GameMeta.DefaultSavedataDirectoryPath, GameMeta.DEFAULT_SAVEDATA_FILENAME);
                _savedataWatcher.Changed += new FileSystemEventHandler(eventHandler);
                _savedataWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
                _savedataWatcher.IncludeSubdirectories = false;
                _savedataWatcher.EnableRaisingEvents = true;
                File.SetLastWriteTimeUtc(GameMeta.DefaultSavedataPath, DateTime.UtcNow);
            }

            if (File.Exists(GameMeta.DefaultGameSettingsPath))
            {
                eventHandler = new FileSystemEventHandler(OnGameConfigChanged);
                _gameConfigWatcher = new FileSystemWatcher(GameMeta.DefaultGameSettingsDirectoryPath, GameMeta.DEFAULT_GAMECONFIG_FILENAME);
                _gameConfigWatcher.Changed += new FileSystemEventHandler(eventHandler);
                _gameConfigWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
                _gameConfigWatcher.IncludeSubdirectories = false;
                _gameConfigWatcher.EnableRaisingEvents = true;
                File.SetLastWriteTimeUtc(GameMeta.DefaultGameSettingsPath, DateTime.UtcNow);
            }

            if (File.Exists(GameMeta.DefaultKeymapFilePath))
            {
                eventHandler = new FileSystemEventHandler(OnKeymapChanged);
                _keymapConfigWatcher = new FileSystemWatcher(GameMeta.DefaultGameSettingsDirectoryPath, GameMeta.KEYMAPPING_FILENAME);
                _keymapConfigWatcher.Changed += new FileSystemEventHandler(eventHandler);
                _keymapConfigWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
                _keymapConfigWatcher.IncludeSubdirectories = false;
                _keymapConfigWatcher.EnableRaisingEvents = true;
                File.SetLastWriteTimeUtc(GameMeta.DefaultKeymapFilePath, DateTime.UtcNow);
            }

            /* Initialize systems manually, if no animation is played */
            if (!_playLoadingAnimation) { initalizeSystems(); }
        }
        #endregion


        #region Events
        /// <summary> Fires, when the game process has ended.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        void GameProcess_Exited(object sender, EventArgs e)
        {
            // Close the overlay together with the game.
            if (this.Dispatcher.CheckAccess())
            {
                this.Close();
            }
            else
            {
                this.Dispatcher.Invoke(new ThreadStart(this.Close));
            }
        }


        /// <summary> Fires each time, the update timer is being triggered (~500ms).
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            updateDisplay();
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


        /// <summary> Fires each time, the "keymap8.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnKeymapChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            KeyboardControls.LoadKeymap();
        }


        /// <summary> Fires each time, the overlay has been closed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void window_Closed(object sender, EventArgs e)
        {
            GameMeta.StopGameProcessSearch();
            PluginManager.ShutdownPlugins();
        }
        #endregion


        #region Functions
        /// <summary> Updates the displayed information of the overlay.
        /// </summary>
        private void updateDisplay()
        {
            // Title + date and time
            txtBox_TitleInfo.Text = this.Title;
            txtBlck_Time.Text = DateTime.UtcNow.ToLongDateString() + "\n" + DateTime.Now.ToLongTimeString();

            if (_debugMode)
            {
                txtBlck_MainInfo.Text = "Current Node: " + ((VI.CurrentDialogNode == null) ? "N/A" : VI.CurrentDialogNode.GUIDisplayText) + "\n" +
                    "Active Nodes:\n" +
                    buildDialogInfo(DialogTreeBuilder.DialogRoot);

                txtBlck_StatusInfo.Text = "Target Process: " + Interactor.TargetProcessName + "\n" +
                    PluginManager.Plugins.Count + " plugins loaded";
            }
        }

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


        /// <summary> Initializes / starts the system.
        /// </summary>
        private void initalizeSystems()
        {
            // System ready
            VI.State = VI.VIState.READY;

            // Check for existence of savedatasettings.txt
            if (!File.Exists(GameMeta.CurrentSaveDataSettingsTextFilePath)) { SpeechEngine.Say(EvoVI.Properties.StringTable.SAVEDATASETTINGS_FILE_NOT_FOUND); }
        }
        #endregion


        #region Animations
        /// <summary> Starts the initial loading animation.
        /// </summary>
        private void loadAnimation()
        {
            double originalOpacity = this.Opacity;
            
            Storyboard story;
            DoubleAnimation animation;
            TimeSpan animDuration;

            double timeOffset = 0;
            story = new Storyboard();
            story.Completed += onLoadAnimationCompleted;


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
            XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(img_LogoBackground, RepeatBehavior.Forever);

            txtBlck_MainInfo.Opacity = 0;
            txtBlck_StatusInfo.Opacity = 0;
            txtBlck_Time.Opacity = 0;
            txtBox_FileUpdateStatus.Opacity = 0;
            txtBox_TitleInfo.Opacity = 0;

            story.Begin();
        }


        /// <summary> Fires, when the loading animation has been finished.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The ecent arguments.</param>
        void onLoadAnimationCompleted(object sender, EventArgs e)
        {
            XamlAnimatedGif.AnimationBehavior.SetRepeatBehavior(img_LogoBackground, new RepeatBehavior(2));
            img_LogoBackground.Visibility = System.Windows.Visibility.Hidden;

            initalizeSystems();
        }
        #endregion


        #region [[ Debug ]]
        private string buildDialogInfo(DialogBase parentNode, int level = 1)
        {
            string dialogInfo = "";

            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                DialogBase currChild = parentNode.ChildNodes[i];
                if (
                    (!_debugMode) && 
                    (
                        (!currChild.IsReady) ||
                        (currChild.Speaker != DialogBase.DialogSpeaker.PLAYER)
                    )
                )
                { return String.Empty; }

                string listeningMark = String.Empty;
                string arrow = String.Empty;
                string importanceMark = String.Empty;
                System.Text.StringBuilder dock = new System.Text.StringBuilder();

                if (_debugMode)
                {
                    // Is listening mark
                    listeningMark = "[" + (
                        currChild.Disabled ? "~" :
                        currChild.IsReady ? "!" :
                        " "
                    ) + "]";

                    // Prepare spacing
                    arrow = (
                        currChild.IsActive ? "====" :
                        (parentNode.IsActive) ? "----" :
                        "    "
                    );

                    importanceMark = "[";
                    for (DialogBase.DialogPriority u = DialogBase.DialogPriority.VERY_LOW; u < currChild.Priority; u++) { importanceMark += "*"; }
                    for (DialogBase.DialogPriority u = currChild.Priority; u < DialogBase.DialogPriority.CRITICAL; u++) { importanceMark += " "; }
                    importanceMark += "]";

                    // Prepare "dock"
                    dock = new System.Text.StringBuilder();
                    dock.Append("|");
                    dock.Append('-', level * 2);
                }

                dialogInfo += "" +
                    (_debugMode ? 
                        (
                            listeningMark + "" +
                            importanceMark + "" +
                            arrow + dock + " " +
                            currChild.Speaker + ": "
                        ) : 
                    "") +  currChild.GUIDisplayText + "\n";

                dialogInfo += buildDialogInfo(currChild, level + 1);
            }

            return dialogInfo;
        }
        #endregion

    }
}
