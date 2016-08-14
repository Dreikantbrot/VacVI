using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Evo_VI.engine
{
    public static class Interactor
    {
        #region DLL Imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);
        #endregion


        #region Flags
        [Flags]
        private enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        private enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008,
        }
        #endregion


        #region Structs
        private struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public readonly MouseInput mi;
            [FieldOffset(0)]
            public KeyboardInput ki;
            [FieldOffset(0)]
            public readonly HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public readonly int dx;
            public readonly int dy;
            public readonly uint mouseData;
            public readonly uint dwFlags;
            public readonly uint time;
            public readonly IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public readonly uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardwareInput
        {
            public readonly uint uMsg;
            public readonly ushort wParamL;
            public readonly ushort wParamH;
        }
        #endregion


        #region Variables
        private static Process _targetProcess = null;
        private static IntPtr _targetWindowHandle;
        #endregion


        #region Private Functions
        /// <summary> Gets a process by it's name and updates target process and window handle.
        /// </summary>
        /// <param name="process">The process name (without file extension).</param>
        private static void getAllProcessesByName(string process)
        {
            _targetProcess = Process.GetProcessesByName(process).FirstOrDefault();
            if (_targetProcess != null) { _targetWindowHandle = _targetProcess.MainWindowHandle; }
        }
        #endregion


        #region Public Functions
        /// <summary> Initializes the Interactor.
        /// </summary>
        public static void Initialize()
        {
            // TODO: Remove dummy
            getAllProcessesByName("EvochronMercenary");
        }


        /// <summary> Sends the specified key to the target application, if active.
        /// </summary>
        /// <param name="key">The keycode.</param>
        /// <param name="isScancode">If true, the keycode will be interpreted as a scan code, else as unicode.</param>
        public static void SendKey(uint key, bool isScancode = false)
        {
            Input[] inputs;

            inputs = new Input[1];
            inputs[0].type = (int)InputType.Keyboard;
            inputs[0].u.ki.wScan = (ushort)key;
            inputs[0].u.ki.dwFlags = (uint)(KeyEventF.KeyDown | (isScancode ? KeyEventF.Scancode : KeyEventF.Unicode));

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));

            Thread.Sleep(30);

            inputs[0].u.ki.dwFlags = (uint)(KeyEventF.KeyUp | (isScancode ? KeyEventF.Scancode : KeyEventF.Unicode));

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        }
        #endregion
    }
}
