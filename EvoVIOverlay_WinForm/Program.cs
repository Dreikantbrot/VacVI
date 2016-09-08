using EvoVI.Database;
using EvoVI.Engine;
using System;
using System.Windows.Forms;

namespace EvoVI
{    
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Overlay());
        }


        #region Public Functions
        /// <summary> Updates program settings.
        /// </summary>
        public static void UpdateSettings()
        {

        }
        #endregion
    }
}
