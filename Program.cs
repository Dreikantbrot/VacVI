using Evo_VI.engine;
using System;
using System.Windows.Forms;

namespace Evo_VI
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
            SpeechEngine.Initialize();
            Interactor.Initialize();
            Database.InGameData.Initialize();
            VI.Initialize();

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
