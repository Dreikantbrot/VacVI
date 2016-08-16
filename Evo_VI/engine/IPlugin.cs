using Evo_VI.classes.dialog;
using System.Speech.Recognition;

namespace Evo_VI.PluginContracts 
{
    public interface IPlugin
    {
        #region Plugin Information
        /// <summary> Gets the plugin's unique ID.
        /// </summary>
        /// <returns>The plugin's ID.</returns>
        string GetId();


        /// <summary> Gets the plugin name.
        /// </summary>
        /// <returns>The plugin name.</returns>
        string GetName();


        /// <summary> Gets the plugin version.
        /// </summary>
        /// <returns>The plugin version.</returns>
        string GetVersion();


        /// <summary> Gets the plugin's description.
        /// </summary>
        /// <returns>The plugin's description.</returns>
        string GetDescription();
        #endregion


        #region Interface Functions
        /// <summary> Initializes the plugin.
        /// </summary>
        void Initialization();


        /// <summary> Is called, when the plugin is beng fired by a dialog.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The speech recognition engine's event arguments.</param>
        /// <param name="originNode">The DialogNode instance that called the plugin.</param>
        void OnDialogAction(object sender, SpeechRecognizedEventArgs e, DialogNode originNode);


        /// <summary> Gets the plugin's unique ID.
        /// </summary>
        void OnGameDataUpdate();
        #endregion
    }
}
