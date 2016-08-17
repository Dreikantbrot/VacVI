using EvoVI.classes.dialog;
using System;
using System.Speech.Recognition;

namespace EvoVI.PluginContracts 
{
    public interface IPlugin
    {
        #region Plugin Information
        /// <summary> Returns the plugin's unique ID.
        /// </summary>
        Guid Id{ get; }


        /// <summary> Returns the plugin name.
        /// </summary>
        string Name { get; }


        /// <summary> Returns the plugin version.
        /// </summary>
        string Version { get; }


        /// <summary> Returns the plugin's description.
        /// </summary>
        string Description { get; }
        #endregion


        #region Interface Functions
        /// <summary> Initializes the plugin.
        /// </summary>
        void Initialize();


        /// <summary> Is called, when the plugin is being fired by a dialog.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        /// <param name="originNode">The DialogNode instance that called the plugin.</param>
        void OnDialogAction(DialogNode originNode);


        /// <summary> Gets the plugin's unique ID.
        /// </summary>
        void OnGameDataUpdate();
        #endregion
    }
}
