using VacVI.Dialog;
using VacVI.Database;
using System;
using System.Collections.Generic;

namespace VacVI.Plugins 
{
    /// <summary> An interface for creating plugins.</summary>
    public interface IPlugin
    {
        #region Plugin Information
        /// <summary> Returns the plugin's unique ID.
        /// </summary>
        Guid Id{ get; }


        /// <summary> Returns the plugin's unique name.
        /// <para>This field must not be empty or consisting of whitespaces only!</para>
        /// </summary>
        string Name { get; }


        /// <summary> Returns the plugin's description.
        /// </summary>
        string Description { get; }


        /// <summary> Returns the plugin's compatibility flags.
        /// </summary>
        GameMeta.SupportedGame CompatibilityFlags { get; }


        /// <summary> Returns the plugin version.
        /// </summary>
        string Version { get; }


        /// <summary> Returns the plugin's logo.
        /// </summary>
        System.Drawing.Bitmap LogoImage { get; }


        /// <summary> Returns the author of the plugin.
        /// </summary>
        string Author { get; }


        /// <summary> Returns the homepage of the author / plugin forum thread.
        /// </summary>
        string Homepage { get; }
        #endregion


        #region Interface Functions
        /// <summary> Gets the default parameters for the plugin.
        /// This function is being called by the PluginLoader before plugin initialization.
        /// </summary>
        List<PluginParameterDefault> GetDefaultPluginParameters();


        /// <summary> Initializes the plugin.
        /// </summary>
        void Initialize();


        /// <summary> Gets the dialog tree used by the plugin.
        /// This function is being called by the PluginLoader before plugin initialization.
        /// </summary>
        void BuildDialogTree();


        /// <summary> Is called, when the plugin is being fired by a dialog.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        /// <param name="originNode">The DialogNode instance that called the plugin.</param>
        void OnDialogAction(DialogBase originNode);


        /// <summary> Gets called each time game data has been retrieved and updated.
        /// The interval is dependant on the user's savedatasettings.txt file content.
        /// </summary>
        void OnGameDataUpdate();


        /// <summary> Gets called each time, the main application has been.
        /// </summary>
        void OnProgramShutdown();
        #endregion
    }
}
