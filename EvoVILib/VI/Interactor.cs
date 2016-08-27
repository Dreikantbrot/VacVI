using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

// TODO: Input will not be recognized, if the game is run as administrator

namespace EvoVI.Engine
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


        #region Structs (private)
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


        #region Enums
        public enum KeyPressMode
        {
            KEY_DOWN = 1,
            KEY_UP = 2,
            KEY_PRESS = (KEY_DOWN & KEY_UP),
            KEY_PRESS_NO_SLEEP = 4
        }
        #endregion


        #region Variables
        private static Process _targetProcess = null;
        private static IntPtr _targetWindowHandle;
        #endregion


        #region Properties
        /// <summary> Returns the name of the currently targeted process.
        /// </summary>
        public static string TargetProcessName
        {
            get { return (Interactor._targetProcess == null) ? "<null>" : Interactor._targetProcess.ProcessName; }
        }
        #endregion


        #region Functions
        /// <summary> Initializes the Interactor.
        /// </summary>
        public static void Initialize()
        {
            // TODO: Remove dummy
            getAllProcessesByName("EvochronMercenary");
        }


        /// <summary> Gets a process by it's name and updates target process and window handle.
        /// </summary>
        /// <param name="process">The process name (without file extension).</param>
        private static void getAllProcessesByName(string process)
        {
            _targetProcess = Process.GetProcessesByName(process).FirstOrDefault();
            if (_targetProcess != null) { _targetWindowHandle = _targetProcess.MainWindowHandle; }
        }


        /// <summary> Sends the input information to the currently active window via SendInput.
        /// </summary>
        /// <param name="inputs">The input information.</param>
        /// <param name="pressMode">The mode of the keypress. Determines how the key is pressed.</param>
        /// <param name="pressTime">The time to wait in ms after pressing the key and before releasing it.</param>
        /// <param name="isScancode">Whether the key code within the input info array is a scancode or not.
        /// <para>If set to false, the key will be interpreted as a unicode character.</para>
        /// </param>
        private static void pressKey(Input[] inputs, KeyPressMode pressMode, int pressTime, bool isScancode)
        {
            uint result = 0;
            if ((pressMode & KeyPressMode.KEY_DOWN) == pressMode)
            {
                inputs[0].u.ki.dwFlags = (uint)(KeyEventF.KeyDown | (isScancode ? KeyEventF.Scancode : KeyEventF.Unicode));
                result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            }

            if (pressTime > 0) { Thread.Sleep(pressTime); }

            if ((pressMode & KeyPressMode.KEY_UP) == pressMode)
            {
                inputs[0].u.ki.dwFlags = (uint)(KeyEventF.KeyUp | (isScancode ? KeyEventF.Scancode : KeyEventF.Unicode));
                result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            }
        }


        /// <summary> Types the given text into the target application, if active.
        /// <para>Warning: This command might only work in certain menus and not while in-game.</para>
        /// </summary>
        /// <param name="message">The text to type.</param>
        public static void TypeText(string message)
        {
            for (int i = 0; i < message.Length; i++) { PressKey(message[i], KeyPressMode.KEY_PRESS, 0, false); }
        }


        /// <summary> Sends the specified key to the target application, if active.
        /// </summary>
        /// <param name="keyCode">The keycode.</param>
        /// <param name="pressMode">The mode of the keypress. Determines how the key is pressed.</param>
        /// <param name="pressTime">The time to wait in ms after pressing the key and before releasing it.</param>
        /// <param name="isScancode">If true, the keycode will be interpreted as a scan code, else as unicode.</param>
        public static void PressKey(uint keyCode, KeyPressMode pressMode = KeyPressMode.KEY_PRESS, int pressTime = 0, bool isScancode = true)
        {
            PressKey(MapVirtualKey(keyCode, 0), pressMode, pressTime, isScancode);
        }
        

        /// <summary> Sends the specified key to the target application, if active.
        /// </summary>
        /// <param name="key">The keycode.</param>
        /// <param name="pressMode">The mode of the keypress. Determines how the key is pressed.</param>
        /// <param name="pressTime">The time to wait in ms after pressing the key and before releasing it.</param>
        /// <param name="isScancode">If true, the keycode will be interpreted as a scan code, else as unicode.</param>
        public static void PressKey(int key, KeyPressMode pressMode = KeyPressMode.KEY_PRESS, int pressTime = 0, bool isScancode = true)
        {
            Input[] inputs;

            inputs = new Input[1];
            inputs[0].type = (int)InputType.Keyboard;
            inputs[0].u.ki.wScan = (ushort)key;

            pressKey(inputs, pressMode, pressTime, isScancode);
        }
        #endregion
    }
}
