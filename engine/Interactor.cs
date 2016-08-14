using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Evo_VI.engine
{
    public static class Interactor
    {
        #region User32 DLL Imports
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion


        #region Variables
        private static Process targetProcess = null;
        private static IntPtr targetWindowHandle;
        #endregion


        #region Private Functions
        /// <summary>
        /// Gets a process by it's name and updates target process and window handle.
        /// </summary>
        /// <param name="process">The process name (without file extension)</param>
        static void getAllProcessesByName(string process)
        {
            targetProcess = Process.GetProcessesByName(process).FirstOrDefault();
            targetWindowHandle = targetProcess.MainWindowHandle;
        }
        #endregion


        #region Public Functions
        /// <summary>
        /// Initializes the Interactor.
        /// </summary>
        public static void Initialize()
        {
            // TODO: Remove dummy
            getAllProcessesByName("notepad");
        }


        /// <summary>
        /// Presses the specified key, if the target process is active.
        /// </summary>
        /// <param name="key">The key to press</param>
        public static void PressKey(Keys key)
        {
            // TODO: Parse the entire key string instead of just passing a single key
            IntPtr hwnd = GetForegroundWindow();
            if (targetWindowHandle != hwnd) { return; }

            SetForegroundWindow(targetWindowHandle);

            /* Convert Special keys to their correct string */
            // TODO: Build key library
            string keyString = key.ToString();
            switch(key)
            {
                case Keys.Back:     keyString = "{BACKSPACE}"; break;
                case Keys.CapsLock: keyString = "{CAPSLOCK}"; break;
                case Keys.Enter:    keyString = "{ENTER}"; break;

                case Keys.Delete:   keyString = "{DELETE}"; break;
                case Keys.Home:     keyString = "{HOME}"; break;
                case Keys.Insert:   keyString = "{INSERT}"; break;
                case Keys.PageUp:   keyString = "{PGUP}"; break;
                case Keys.PageDown: keyString = "{PGDN}"; break;
                
                case Keys.Up:       keyString = "{UP}"; break;
                case Keys.Down:     keyString = "{DOWN}"; break;
                case Keys.Left:     keyString = "{LEFT}"; break;
                case Keys.Right:    keyString = "{RIGHT}"; break;
            }
            SendKeys.SendWait(keyString);
        }
        #endregion
    }
}
