using VacVI;
using VacVI.Dialog;
using VacVI.Database;
using VacVI.Input;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VacVI.Plugins;
using XamlAnimatedGif;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace VacVIOverlay
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        #region Window Behaviour Override
        /* Those settings make it possible to click "through" the window with the mouse */
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


        #region Enums
        [Flags]
        private enum InitializedSystemTypes
        {
            NONE = 0,
            VI = 1,
            OVERLAY = 2,
            STATUS_ICON_ANIMATOR = 4,
            BACKGRND_LOGO_ANIMATOR = 8,
            LOADING_ANIMATION_DONE = 16,
            ALL_COMPONENTS = 31,
            INITIALIZATION_DONE = 32 | ALL_COMPONENTS
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

        const int MIN_TIMEOUT_TIMER_INTERVAL = 500;
        const int MIN_UPDATE_INDICATOR_INTERVAL = 50;
        #endregion


        #region Variables
        private FileSystemWatcher _savedataWatcher;
        private FileSystemWatcher _gameConfigWatcher;
        private FileSystemWatcher _keymapConfigWatcher;
        private DispatcherTimer _autoPauseVITimer;

        private bool _updateIndicator;
        private bool _playLoadingAnimation;
        private bool _debugMode = true;
        private double _standardIconOpacity = 1;
        private double _standardOverlayOpacity = 0.8;

        private Action _imgBlinkIn;
        private Action _imgBlinkOut;
        private Storyboard _collapseExpandAnimation = new Storyboard();
        private Thread _autoCollapseThread;
        private Action _autoCollapseAction;
        private Action _rotateStatusIconAction;

        private InitializedSystemTypes _initializedSystems = InitializedSystemTypes.NONE;
        #endregion


        #region Constructor
        public OverlayWindow()
        {
            InitializeComponent();

            /* Load the main configuration */
            ConfigurationManager.LoadConfiguration();


            /* Initialize the Overlay */
            VI.Disabled = true; // Disable VI beforehand to prevent VI_OnVIStateChanged from firing

            SpeechEngine.OnVISpeechManipulated += SpeechEngine_OnOnVISpeechManipulated;
            SpeechEngine.OnVISpeechStopped += SpeechEngine_OnVISpeechStopped;
            VI.OnVIStateChanged += VI_OnVIStateChanged;

            AnimationBehavior.AddLoadedHandler(img_StatusIcon, onGifAnimatorLoaded);
            AnimationBehavior.AddLoadedHandler(img_LogoBackground, onGifAnimatorLoaded);

            _standardIconOpacity = img_StatusIcon.Opacity;
            _standardOverlayOpacity = this.Opacity;

            _imgBlinkIn = new Action(() => { img_StatusIcon.Opacity = _standardIconOpacity; });
            _imgBlinkOut = new Action(() => { img_StatusIcon.Opacity = Math.Max(_standardIconOpacity - 0.2, 0); });

            DoubleAnimation collapseAnim;
            TimeSpan animationTime = TimeSpan.FromSeconds(1);
            _collapseExpandAnimation = new Storyboard();
            _autoCollapseAction = new Action(() => { expandCollapse(false); });
            _rotateStatusIconAction = new Action(() => { AnimationBehavior.GetAnimator(img_StatusIcon).Play(); });

            collapseAnim = new DoubleAnimation();
            collapseAnim.Duration = animationTime;
            collapseAnim.To = LogoWidthGrid.ActualWidth;
            Storyboard.SetTarget(collapseAnim, this);
            Storyboard.SetTargetProperty(collapseAnim, new PropertyPath("Width"));
            _collapseExpandAnimation.Children.Add(collapseAnim);

            collapseAnim = new DoubleAnimation();
            collapseAnim.BeginTime = animationTime;
            collapseAnim.Duration = animationTime;
            collapseAnim.To = LogoHeightGrid.ActualHeight;
            Storyboard.SetTarget(collapseAnim, this);
            Storyboard.SetTargetProperty(collapseAnim, new PropertyPath("Height"));
            _collapseExpandAnimation.Children.Add(collapseAnim);


            /* Overlay configuration parameters */
            // Set window position
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


            // Check loading animation parameter
            _playLoadingAnimation = ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(
                ConfigurationManager.SECTION_OVERLAY,
                "Play_Intro"
            );

            // Check game update indicator status
            _updateIndicator = ConfigurationManager.ConfigurationFile.ValueIsBoolAndTrue(
                ConfigurationManager.SECTION_OVERLAY,
                "Display_Update_Indicator"
            );


            /* Initialize all components */
            DialogTreeBuilder.ClearDialogTree();
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
            PluginManager.InitializePluginDialogTrees();


            /* Initialize file watchers */
            FileSystemEventHandler eventHandler;

            eventHandler = new FileSystemEventHandler(OnSaveDataChanged);
            _savedataWatcher = new FileSystemWatcher(GameMeta.DefaultSavedataDirectoryPath, GameMeta.DEFAULT_SAVEDATA_FILENAME);
            _savedataWatcher.Changed += new FileSystemEventHandler(eventHandler);
            _savedataWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
            _savedataWatcher.IncludeSubdirectories = false;
            _savedataWatcher.EnableRaisingEvents = true;

            if (File.Exists(GameMeta.DefaultSavedataPath))
            {
                // Manually touch the file to invoke the event
                File.SetLastWriteTimeUtc(GameMeta.DefaultSavedataPath, DateTime.UtcNow);
            }


            eventHandler = new FileSystemEventHandler(OnGameConfigChanged);
            _gameConfigWatcher = new FileSystemWatcher(GameMeta.DefaultGameSettingsDirectoryPath, GameMeta.DEFAULT_GAMECONFIG_FILENAME);
            _gameConfigWatcher.Changed += new FileSystemEventHandler(eventHandler);
            _gameConfigWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
            _gameConfigWatcher.IncludeSubdirectories = false;
            _gameConfigWatcher.EnableRaisingEvents = true;

            if (File.Exists(GameMeta.DefaultGameSettingsPath))
            {
                // Manually touch the file to invoke the event
                File.SetLastWriteTimeUtc(GameMeta.DefaultGameSettingsPath, DateTime.UtcNow);
            }


            eventHandler = new FileSystemEventHandler(OnKeymapChanged);
            _keymapConfigWatcher = new FileSystemWatcher(GameMeta.DefaultGameSettingsDirectoryPath, GameMeta.KEYMAPPING_FILENAME);
            _keymapConfigWatcher.Changed += new FileSystemEventHandler(eventHandler);
            _keymapConfigWatcher.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.CreationTime);
            _keymapConfigWatcher.IncludeSubdirectories = false;
            _keymapConfigWatcher.EnableRaisingEvents = true;

            if (File.Exists(GameMeta.DefaultKeymapFilePath))
            {
                // Manually touch the file to invoke the event
                File.SetLastWriteTimeUtc(GameMeta.DefaultKeymapFilePath, DateTime.UtcNow);
            }

            _autoPauseVITimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds((SaveDataReader.UpdateInterval < 0) ? MIN_TIMEOUT_TIMER_INTERVAL : SaveDataReader.UpdateInterval),
                DispatcherPriority.Background,
                OnAutoPauseVITimerTick,
                this.Dispatcher
            );
            _autoPauseVITimer.Stop();


            /* Initialize Systems / Start the VI */
            _initializedSystems |= InitializedSystemTypes.VI;
            _initializedSystems |= InitializedSystemTypes.OVERLAY;
            if (!_playLoadingAnimation)
            {
                _initializedSystems |= InitializedSystemTypes.LOADING_ANIMATION_DONE;

                // Start after 1 second delay, giving the overlay time to adjust the color
                new Thread(() => { Thread.Sleep(1000); initializeSystems(); }).Start();
            }
        }
        #endregion


        #region Events
        /// <summary> Initializes the gif animations as soon as the animator as loaded.
        /// Fires when a gif animator has loaded.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The routed event arguments.</param>
        private void onGifAnimatorLoaded(object sender, RoutedEventArgs e)
        {
            if (
                (sender == img_StatusIcon) &&
                ((_initializedSystems & InitializedSystemTypes.STATUS_ICON_ANIMATOR) != InitializedSystemTypes.STATUS_ICON_ANIMATOR)
            )
            {
                _initializedSystems |= InitializedSystemTypes.STATUS_ICON_ANIMATOR;

                // Reset the animation and let it spin 2 times when played
                AnimationBehavior.GetAnimator(img_StatusIcon).Pause();
                AnimationBehavior.GetAnimator(img_StatusIcon).Rewind();
                AnimationBehavior.SetRepeatBehavior(img_StatusIcon, new RepeatBehavior(2));
                AnimationBehavior.GetAnimator(img_StatusIcon).AnimationCompleted += onStatusIconAnmationCompleted;
            }
            else if (
                (sender == img_LogoBackground) &&
                ((_initializedSystems & InitializedSystemTypes.BACKGRND_LOGO_ANIMATOR) != InitializedSystemTypes.BACKGRND_LOGO_ANIMATOR)
            )
            {
                _initializedSystems |= InitializedSystemTypes.BACKGRND_LOGO_ANIMATOR;

                // Reset the animation and let it spin forever when played
                AnimationBehavior.GetAnimator(img_LogoBackground).Pause();
                AnimationBehavior.GetAnimator(img_LogoBackground).Rewind();
                AnimationBehavior.SetRepeatBehavior(img_LogoBackground, RepeatBehavior.Forever);

                // Play loading animation
                if (_playLoadingAnimation)
                {
                    Storyboard story;
                    DoubleAnimation animation;
                    TimeSpan animDuration;

                    double timeOffset = 0;
                    story = new Storyboard();
                    story.Completed += onLoadAnimationCompleted;


                    #region Fade the Window in
                    timeOffset += 4;
                    animDuration = TimeSpan.FromSeconds(1);

                    animation = new DoubleAnimation(_standardOverlayOpacity, animDuration);
                    animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
                    Storyboard.SetTarget(animation, this);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
                    story.Children.Add(animation);
                    timeOffset += animDuration.Seconds;
                    #endregion


                    #region Enhance the Window
                    timeOffset += 0;
                    animDuration = TimeSpan.FromSeconds(4);

                    animation = new DoubleAnimation(this.MaxWidth, animDuration);
                    animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
                    Storyboard.SetTarget(animation, this);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));
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


                    #region Fade status icon in

                    animation = new DoubleAnimation(_standardIconOpacity, animDuration);
                    animation.BeginTime = TimeSpan.FromSeconds(timeOffset);
                    Storyboard.SetTarget(animation, img_StatusIcon);
                    Storyboard.SetTargetProperty(animation, new PropertyPath(System.Windows.Shapes.Shape.OpacityProperty));
                    story.Children.Add(animation);
                    timeOffset += animDuration.Seconds;
                    #endregion


                    // Prepare all elements before starting the animation
                    this.Width = 0;
                    this.Opacity = 0;

                    img_LogoBackground.Opacity = 0;
                    img_LogoBackground_Blur.Radius = 15;

                    txt_VISpeechText.Opacity = 0;
                    img_StatusIcon.Opacity = 0;
                    stckPnl_PlayerAnswers.Visibility = System.Windows.Visibility.Collapsed;

                    story.Begin();
                }
                else
                {
                    // Animation deactivated - animation not started
                    img_LogoBackground.Visibility = System.Windows.Visibility.Collapsed;
                    _initializedSystems |= InitializedSystemTypes.LOADING_ANIMATION_DONE;
                }
            }

            initializeSystems();
        }


        /// <summary> Pauses the loading animation, resets values and attempts initialization.
        /// Fires, when the loading animation has been finished.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The ecent arguments.</param>
        private void onLoadAnimationCompleted(object sender, EventArgs e)
        {
            AnimationBehavior.GetAnimator(img_LogoBackground).Pause();
            AnimationBehavior.GetAnimator(img_LogoBackground).Rewind();
            img_LogoBackground.Visibility = System.Windows.Visibility.Collapsed;

            img_StatusIcon.Opacity = img_StatusIcon.Opacity;
            img_StatusIcon.BeginAnimation(OpacityProperty, null);

            txt_VISpeechText.Text = "";
            txt_VISpeechText.Opacity = 1;
            expandCollapse(false);
            
            
            new Thread(() =>
                {
                    Thread.Sleep(2000);

                    this.Dispatcher.Invoke(() =>
                    {
                        _initializedSystems |= InitializedSystemTypes.LOADING_ANIMATION_DONE;

                        initializeSystems();
                    });
                }
            ).Start();
        }


        /// <summary> Rewinds the status icon animation when completed.
        /// Fires when the status icon animation has been completed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void onStatusIconAnmationCompleted(object sender, EventArgs e)
        {
            // Reset animation
            AnimationBehavior.GetAnimator(img_StatusIcon).Rewind();
        }

        
        /// <summary> Colors the overlay according to the VI's state.
        /// Fires when the VI's state of operation has changed.
        /// </summary>
        /// <param name="obj">The VI state changed event arguments.</param>
        private void VI_OnVIStateChanged(VI.OnVIStateChangedEventArgs obj)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    string[] properties = { "Background", "BorderBrush", "Foreground" };
                    ColorAnimation animation;
                    TimeSpan animationTime = TimeSpan.FromSeconds(1);
                    Storyboard story = new Storyboard();

                    // Animate all colors
                    for (int i = 0; i < properties.Length; i++)
                    {
                        string resource = properties[i];
                        string destResource = resource + "_";

                        // Determine the color resource to change to
                        destResource += (
                            (obj.CurrentState >= VI.VIState.READY) ? "Normal" :                             // <-- Normal colors on ready-state
                            (obj.CurrentState == VI.VIState.BUSY) ? ((i == 1) ? "Grayscale" : "Normal") :   // <-- Gray border only, if in busy-state
                            (obj.CurrentState <= VI.VIState.OFFLINE) ? "Grayscale" :                        // <-- Grayscale everything, if offline
                            "Normal"                                                                        // <-- All other states normal
                        );

                        animation = new ColorAnimation();
                        animation.To = ((SolidColorBrush)this.Resources[destResource]).Color;
                        animation.Duration = animationTime;
                        Storyboard.SetTarget(animation, this);
                        Storyboard.SetTargetProperty(animation, new PropertyPath(properties[i] + ".Color"));
                        story.Children.Add(animation);
                    }

                    // Animate opacity
                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.To = (obj.CurrentState == VI.VIState.SLEEPING) ? 0.4 : _standardOverlayOpacity;    // Opacity lower, when sleeping
                    opacityAnimation.Duration = animationTime;
                    Storyboard.SetTarget(opacityAnimation, this);
                    Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));
                    story.Children.Add(opacityAnimation);

                    story.Begin();
                }
            );
        }


        /// <summary> Expands the overlay and displays spoken text.
        /// Fires each time the VI started to speak.
        /// </summary>
        /// <param name="obj">The VI speech started event arguments.</param>
        private void SpeechEngine_OnOnVISpeechManipulated(SpeechEngine.VISpeechStartedEventArgs obj)
        {
            this.Dispatcher.InvokeAsync(
                () =>
                {
                    // Abort the auto-close thread
                    if (
                        (_autoCollapseThread != null) &&
                        (_autoCollapseThread.IsAlive)
                    )
                    { _autoCollapseThread.Abort(); }

                    // Play the icon animation when the VI speaks
                    this.Dispatcher.InvokeAsync(_rotateStatusIconAction);

                    txt_VISpeechText.Text = obj.DisplayedPhrase;
                    stckPnl_PlayerAnswers.Children.Clear();

                    bool hadPlayerNodes = false;

                    for (int i = 0; i < obj.SpokenDialog.ChildNodes.Count; i++)
                    {
                        if (obj.SpokenDialog.ChildNodes[i].Speaker == DialogBase.DialogSpeaker.PLAYER)
                        {
                            #region Build answer dialog stack
                            string[] sentences = obj.SpokenDialog.ChildNodes[i].RawText.Split(';');

                            TextBlock newTxtBlock;
                            StackPanel sentenceStack = new StackPanel();
                            sentenceStack.Orientation = Orientation.Horizontal;
                            sentenceStack.Margin = new Thickness(0, 0, 0, 10);

                            for (int u = 0; u < sentences.Length; u++)
                            {
                                string currSentence = "--- " + sentences[u];

                                if (String.IsNullOrWhiteSpace(currSentence)) { continue; }

                                int currIndex = 0;
                                MatchCollection matches = DialogBase.CHOICES_REGEX.Matches(currSentence);

                                if (matches.Count > 0)
                                {
                                    #region Build choice stack
                                    for (int j = 0; j < matches.Count; j++)
                                    {
                                        Match currMatch = matches[j];

                                        string choiceType = (
                                            (currMatch.Groups["Choice"].Success) ? "Choice" :
                                            (currMatch.Groups["OptChoice"].Success) ? "OptChoice" :
                                            ""
                                        );
                                        bool hasChoices = currMatch.Groups[choiceType].Success;

                                        // Append "fixed" text
                                        string leadingText = currSentence;
                                        leadingText = leadingText.Substring(currIndex, matches[j].Index - currIndex);
                                        
                                        if (!String.IsNullOrWhiteSpace(leadingText))
                                        {
                                            newTxtBlock = new TextBlock();
                                            newTxtBlock.Text = leadingText.Trim() + (hasChoices ? " " : "");
                                            newTxtBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            newTxtBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                                            sentenceStack.Children.Add(newTxtBlock);
                                        }


                                        // Append choices
                                        if (hasChoices)
                                        {
                                            string[] choices = matches[j].Groups[choiceType].Value.Split('|');

                                            newTxtBlock = new TextBlock();
                                            newTxtBlock.Margin = new Thickness(0);
                                            newTxtBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                            newTxtBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                                            newTxtBlock.TextAlignment = TextAlignment.Center;

                                            if (choiceType == "OptChoice") { newTxtBlock.Foreground = new SolidColorBrush(Colors.Gray); }

                                            for (int k = 0; k < choices.Length; k++)
                                            {
                                                if (String.IsNullOrWhiteSpace(choices[k])) { continue; }
                                                newTxtBlock.Text += ((k > 0) ? "\n" : "") + choices[k].Trim();
                                            }

                                            sentenceStack.Children.Add(newTxtBlock);
                                        }

                                        currIndex = matches[j].Index + currMatch.Length;
                                    }
                                    #endregion
                                }

                                // Append "fixed", trailing text (or the entire text, if no choices)
                                newTxtBlock = new TextBlock();
                                newTxtBlock.Text = ((sentenceStack.Children.Count > 0) ? " " : "") + currSentence.Substring(currIndex).Trim();
                                newTxtBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                                newTxtBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                                sentenceStack.Children.Add(newTxtBlock);

                                stckPnl_PlayerAnswers.Children.Add(sentenceStack);
                            }
                            #endregion

                            hadPlayerNodes = true;
                        }
                    }

                    stckPnl_PlayerAnswers.Visibility = hadPlayerNodes ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    stckPnl_PlayerAnswers.UpdateLayout();
                    expandCollapse(true);
                }
            );
        }


        /// <summary> Collapes the overlay after a 5 second timeout.
        /// Fires each time the VI started to speak.
        /// </summary>
        /// <param name="obj">The VI speech stopped event arguments.</param>
        private void SpeechEngine_OnVISpeechStopped(SpeechEngine.VISpeechStoppedEventArgs obj)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    // Abort the auto-close thread
                    if (
                        (_autoCollapseThread != null) &&
                        (_autoCollapseThread.IsAlive)
                    )
                    { _autoCollapseThread.Abort(); }

                    // ... check if there are any active player nodes waiting for input
                    for (int i = 0; i < obj.SpokenDialog.ChildNodes.Count; i++)
                    {
                        if (
                            (obj.SpokenDialog.ChildNodes[i].Speaker == DialogBase.DialogSpeaker.PLAYER) &&
                            (obj.SpokenDialog.ChildNodes[i].IsReady)
                        )
                        { return; }
                    }

                    // ... and start a new one, if there are no answers for the player
                    _autoCollapseThread = new Thread(
                        () =>
                        {
                            Thread.Sleep(5000);
                            this.Dispatcher.Invoke(_autoCollapseAction);
                        }
                    );
                    _autoCollapseThread.Start();
                }
            );
        }


        /// <summary> Makes the status icon blink on game update.
        /// Fires each time, the "savedata.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnSaveDataChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            if (VI.Disabled) { VI.Disabled = false; }
            SaveDataReader.ReadGameData();

            if (
                (this._updateIndicator) &&
                (SaveDataReader.UpdateInterval > MIN_UPDATE_INDICATOR_INTERVAL) &&
                (_initializedSystems == InitializedSystemTypes.INITIALIZATION_DONE)
            )
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, _imgBlinkIn);

                Thread.Sleep(Math.Max(MIN_UPDATE_INDICATOR_INTERVAL, SaveDataReader.UpdateInterval) / 2);

                Dispatcher.BeginInvoke(DispatcherPriority.Render, _imgBlinkOut);
            }
        }


        /// <summary> Disables the VI if the game has been paused.
        /// Fires when the timeout timer for game data updates ticked.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnAutoPauseVITimerTick(object sender, EventArgs e)
        {
            if (ConfigurationManager.StartupParams.NoIdleTimeout) { return; }

            double updateDeltaTime = (DateTime.Now - SaveDataReader.LastUpdateTime).TotalMilliseconds;
            if (
                (updateDeltaTime > MIN_TIMEOUT_TIMER_INTERVAL) && 
                (updateDeltaTime > (2 * SaveDataReader.UpdateInterval))
            )
            { VI.Disabled = true; }

            // Synchronize update time intervals
            if (SaveDataReader.UpdateInterval != _autoPauseVITimer.Interval.Milliseconds)
            {
                // Update the timeout timespan - min MIN_TIMEOUT_TIMER_INTERVAL ms
                _autoPauseVITimer.Interval = TimeSpan.FromMilliseconds(Math.Max(MIN_TIMEOUT_TIMER_INTERVAL, SaveDataReader.UpdateInterval));
            }
        }


        /// <summary> Updates the color scheme on HUD configuration changes.
        /// Fires each time, the "sw.cfg"-file gets changed.
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


        /// <summary> Updates the keymapping on keymap changes.
        /// Fires each time, the "keymap8.txt"-file gets changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The file system event arguments.</param>
        private void OnKeymapChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            KeyboardControls.LoadKeymap();
        }


        /// <summary> Closes the overlay on game process end.
        /// Fires, when the game process has ended.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void GameProcess_Exited(object sender, EventArgs e)
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


        /// <summary> Stops threads and shuts down plugins.
        /// Fires each time, the overlay has been closed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        private void window_Closed(object sender, EventArgs e)
        {
            if (
                (_autoCollapseThread != null) &&
                (_autoCollapseThread.IsAlive)
            )
            { _autoCollapseThread.Abort(); }

            GameMeta.StopGameProcessSearch();
            PluginManager.ShutdownPlugins();
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
            byte brightness;
            SolidColorBrush backColor, foreColor, borderColor;

            // Colorize the background and border
            backColor = new SolidColorBrush(
                Color.FromArgb(
                    255, 
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].R) * (float)(newR / 100)),
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].G) * (float)(newG / 100)),
                    (byte)Math.Min(255, (HUD_BASE_COLOR[hueMode].B) * (float)(newB / 100))
                )
            );

            Color backgrnd = ((SolidColorBrush)backColor).Color;
            foreColor = new SolidColorBrush(
                Color.FromArgb(
                    255, 
                    (byte)Math.Min(255, backgrnd.R * 2.5),
                    (byte)Math.Min(255, backgrnd.G * 2.5),
                    (byte)Math.Min(255, backgrnd.B * 2.5)
                )
            );

            borderColor = new SolidColorBrush(
                Color.FromArgb(
                    255, 
                    (byte)Math.Min(255, backgrnd.R * 1.25), 
                    (byte)Math.Min(255, backgrnd.G * 1.25), 
                    (byte)Math.Min(255, backgrnd.B * 1.25)
                )
            );


            // Generate a grayscale background and border
            this.Resources["Background_Normal"] = ((SolidColorBrush)backColor).Clone();
            this.Resources["BorderBrush_Normal"] = ((SolidColorBrush)borderColor).Clone();
            this.Resources["Foreground_Normal"] = ((SolidColorBrush)foreColor).Clone();

            brightness = (byte)Math.Max(
                ((SolidColorBrush)backColor).Color.R,
                Math.Max(
                    ((SolidColorBrush)backColor).Color.G,
                    ((SolidColorBrush)backColor).Color.B
                )
            );
            this.Resources["Background_Grayscale"] = new SolidColorBrush(
                Color.FromArgb(((SolidColorBrush)backColor).Color.A, brightness, brightness, brightness)
            );


            brightness = (byte)Math.Max(
                ((SolidColorBrush)borderColor).Color.R,
                Math.Max(
                    ((SolidColorBrush)borderColor).Color.G,
                    ((SolidColorBrush)borderColor).Color.B
                )
            );
            this.Resources["BorderBrush_Grayscale"] = new SolidColorBrush(
                Color.FromArgb(((SolidColorBrush)backColor).Color.A, brightness, brightness, brightness)
            );


            brightness = (byte)(
                (Math.Max(
                    ((SolidColorBrush)foreColor).Color.R,
                    Math.Max(
                        ((SolidColorBrush)foreColor).Color.G,
                        ((SolidColorBrush)foreColor).Color.B
                    )
                ) + 128) % 255
            );
            this.Resources["Foreground_Grayscale"] = new SolidColorBrush(
                Color.FromArgb(
                    ((SolidColorBrush)backColor).Color.A, 
                    (byte)((brightness + 64) % 255), 
                    (byte)((brightness + 64) % 255), 
                    (byte)((brightness + 64) % 255)
                )
            );

            if (
                (VI.State >= VI.VIState.READY) ||
                (_initializedSystems != InitializedSystemTypes.ALL_COMPONENTS)
            )
            {
                this.Background = backColor;
                this.BorderBrush = borderColor;
                this.Foreground = foreColor;
            }
        }


        /// <summary> Initializes / starts the system.
        /// </summary>
        private void initializeSystems()
        {
            if (
                (_initializedSystems != InitializedSystemTypes.ALL_COMPONENTS) ||
                (_initializedSystems == InitializedSystemTypes.INITIALIZATION_DONE)
            )
            { return; }

            // Prevent double initialization
            _initializedSystems = InitializedSystemTypes.INITIALIZATION_DONE;

            // System ready
            VI.State = VI.VIState.READY;

            // Check for existence of savedatasettings.txt
            if (!File.Exists(GameMeta.CurrentSaveDataSettingsTextFilePath)) { SpeechEngine.Say(VacVI.Properties.StringTable.SAVEDATASETTINGS_FILE_NOT_FOUND); }
            
            _autoPauseVITimer.Start();

            DialogTreeBuilder.DialogsActive = true;
            DialogTreeBuilder.DialogRoot.SetActive();

            SpeechEngine.Say(new DialogVI("Systems initialized - Hello World!"), true, SpeechEngine.VoiceModulation, null, true);
        }


        /// <summary> Expands or colllapses the overlay window (discrete / full mode).
        /// </summary>
        private void expandCollapse(bool expand)
        {
            DoubleAnimation widthAnim = ((DoubleAnimation)(_collapseExpandAnimation.Children[0]));
            DoubleAnimation heightAnim = ((DoubleAnimation)(_collapseExpandAnimation.Children[1]));

            widthAnim.To = this.BorderThickness.Left + this.BorderThickness.Right;
            heightAnim.To = this.BorderThickness.Top + this.BorderThickness.Bottom;

            if (expand)
            {
                widthAnim.BeginTime = TimeSpan.FromSeconds(0);
                heightAnim.BeginTime = TimeSpan.FromSeconds(1);

                widthAnim.To += this.MaxWidth;
                heightAnim.To += LogoHeightGrid.ActualHeight + LogoVITextGrid.ActualHeight + LogoAnswerTextGrid.ActualHeight;
            }
            else
            {
                widthAnim.BeginTime = TimeSpan.FromSeconds(1);
                heightAnim.BeginTime = TimeSpan.FromSeconds(0);

                widthAnim.To += LogoWidthGrid.ActualWidth;
                heightAnim.To += LogoHeightGrid.ActualHeight;
            }

            _collapseExpandAnimation.Begin();
        }
        #endregion


        #region [[ Debug ]]
        private void testDialog()
        {
            DialogTreeBranch[] dialogTree = new DialogTreeBranch[]{
                new DialogTreeBranch(
                    new DialogPlayer("What is your favourite food"),
                    new DialogTreeBranch(
                        new DialogVI("I like $(muffins|cornflakes|pizza|pancakes|small children for breakfast). What do you like most?"),
                        new DialogTreeBranch(
                            new DialogPlayer("$[My favourite food is|I like] $(soup|pizza|muffins|cornflakes)."),
                            new DialogTreeBranch(
                                new DialogVI("Good choice!")
                            )
                        ),
                        new DialogTreeBranch(
                            new DialogPlayer("I don't eat."),
                            new DialogTreeBranch(
                                new DialogVI("You should! Your $(parents|mother|father) will worry $[about you] otherwise!")
                            )
                        )
                    )
                )
            };

            DialogTreeBuilder.BuildDialogTree(null, dialogTree);
        }


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
                    "") +  currChild.DebugDisplayText + "\n";

                dialogInfo += buildDialogInfo(currChild, level + 1);
            }

            return dialogInfo;
        }
        #endregion

    }
}
