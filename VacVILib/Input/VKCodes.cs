using System;
using System.ComponentModel;

namespace VacVI.Input
{
    /// <Summary> Contains virtual key codes (Windows).
    /// <para>Source: http://www.kbdedit.com/manual/low_level_list.html
    /// </para>
    /// </Summary>
    public struct VKCodes
    {
        /// <summary> The keys themselves.</summary>
        public enum Keys
        {
            [Description(@"Abnt C1.")]
            /// <Summary>Abnt C1.</Summary>
            ABNT_C1 = 0xC1,

            [Description(@"Abnt C2.")]
            /// <Summary>Abnt C2.</Summary>
            ABNT_C2 = 0xC2,

            [Description(@"Numpad +.")]
            /// <Summary>Numpad +.</Summary>
            ADD = 0x6B,

            [Description(@"Attn.")]
            /// <Summary>Attn.</Summary>
            ATTN = 0xF6,

            [Description(@"Backspace.")]
            /// <Summary>Backspace.</Summary>
            BACK = 0x08,

            [Description(@"Break.")]
            /// <Summary>Break.</Summary>
            CANCEL = 0x03,

            [Description(@"Clear.")]
            /// <Summary>Clear.</Summary>
            CLEAR = 0x0C,

            [Description(@"Cr Sel.")]
            /// <Summary>Cr Sel.</Summary>
            CRSEL = 0xF7,

            [Description(@"Numpad ..")]
            /// <Summary>Numpad ..</Summary>
            DECIMAL = 0x6E,

            [Description(@"Numpad /.")]
            /// <Summary>Numpad /.</Summary>
            DIVIDE = 0x6F,

            [Description(@"Er Eof.")]
            /// <Summary>Er Eof.</Summary>
            EREOF = 0xF9,

            [Description(@"Esc.")]
            /// <Summary>Esc.</Summary>
            ESCAPE = 0x1B,

            [Description(@"Execute.")]
            /// <Summary>Execute.</Summary>
            EXECUTE = 0x2B,

            [Description(@"Ex Sel.")]
            /// <Summary>Ex Sel.</Summary>
            EXSEL = 0xF8,

            [Description(@"IcoClr.")]
            /// <Summary>IcoClr.</Summary>
            ICO_CLEAR = 0xE6,

            [Description(@"IcoHlp.")]
            /// <Summary>IcoHlp.</Summary>
            ICO_HELP = 0xE3,

            [Description(@"('0')	0.")]
            /// <Summary>('0')	0.</Summary>
            KEY_0 = 0x30,

            [Description(@"('1')	1.")]
            /// <Summary>('1')	1.</Summary>
            KEY_1 = 0x31,

            [Description(@"('2')	2.")]
            /// <Summary>('2')	2.</Summary>
            KEY_2 = 0x32,

            [Description(@"('3')	3.")]
            /// <Summary>('3')	3.</Summary>
            KEY_3 = 0x33,

            [Description(@"('4')	4.")]
            /// <Summary>('4')	4.</Summary>
            KEY_4 = 0x34,

            [Description(@"('5')	5.")]
            /// <Summary>('5')	5.</Summary>
            KEY_5 = 0x35,

            [Description(@"('6')	6.")]
            /// <Summary>('6')	6.</Summary>
            KEY_6 = 0x36,

            [Description(@"('7')	7.")]
            /// <Summary>('7')	7.</Summary>
            KEY_7 = 0x37,

            [Description(@"('8')	8.")]
            /// <Summary>('8')	8.</Summary>
            KEY_8 = 0x38,

            [Description(@"('9')	9.")]
            /// <Summary>('9')	9.</Summary>
            KEY_9 = 0x39,

            [Description(@"('A')	A.")]
            /// <Summary>('A')	A.</Summary>
            KEY_A = 0x41,

            [Description(@"('B')	B.")]
            /// <Summary>('B')	B.</Summary>
            KEY_B = 0x42,

            [Description(@"('C')	C.")]
            /// <Summary>('C')	C.</Summary>
            KEY_C = 0x43,

            [Description(@"('D')	D.")]
            /// <Summary>('D')	D.</Summary>
            KEY_D = 0x44,

            [Description(@"('E')	E.")]
            /// <Summary>('E')	E.</Summary>
            KEY_E = 0x45,

            [Description(@"('F')	F.")]
            /// <Summary>('F')	F.</Summary>
            KEY_F = 0x46,

            [Description(@"('G')	G.")]
            /// <Summary>('G')	G.</Summary>
            KEY_G = 0x47,

            [Description(@"('H')	H.")]
            /// <Summary>('H')	H.</Summary>
            KEY_H = 0x48,

            [Description(@"('I')	I.")]
            /// <Summary>('I')	I.</Summary>
            KEY_I = 0x49,

            [Description(@"('J')	J.")]
            /// <Summary>('J')	J.</Summary>
            KEY_J = 0x4A,

            [Description(@"('K')	K.")]
            /// <Summary>('K')	K.</Summary>
            KEY_K = 0x4B,

            [Description(@"('L')	L.")]
            /// <Summary>('L')	L.</Summary>
            KEY_L = 0x4C,

            [Description(@"('M')	M.")]
            /// <Summary>('M')	M.</Summary>
            KEY_M = 0x4D,

            [Description(@"('N')	N.")]
            /// <Summary>('N')	N.</Summary>
            KEY_N = 0x4E,

            [Description(@"('O')	O.")]
            /// <Summary>('O')	O.</Summary>
            KEY_O = 0x4F,

            [Description(@"('P')	P.")]
            /// <Summary>('P')	P.</Summary>
            KEY_P = 0x50,

            [Description(@"('Q')	Q.")]
            /// <Summary>('Q')	Q.</Summary>
            KEY_Q = 0x51,

            [Description(@"('R')	R.")]
            /// <Summary>('R')	R.</Summary>
            KEY_R = 0x52,

            [Description(@"('S')	S.")]
            /// <Summary>('S')	S.</Summary>
            KEY_S = 0x53,

            [Description(@"('T')	T.")]
            /// <Summary>('T')	T.</Summary>
            KEY_T = 0x54,

            [Description(@"('U')	U.")]
            /// <Summary>('U')	U.</Summary>
            KEY_U = 0x55,

            [Description(@"('V')	V.")]
            /// <Summary>('V')	V.</Summary>
            KEY_V = 0x56,

            [Description(@"('W')	W.")]
            /// <Summary>('W')	W.</Summary>
            KEY_W = 0x57,

            [Description(@"('X')	X.")]
            /// <Summary>('X')	X.</Summary>
            KEY_X = 0x58,

            [Description(@"('Y')	Y.")]
            /// <Summary>('Y')	Y.</Summary>
            KEY_Y = 0x59,

            [Description(@"('Z')	Z.")]
            /// <Summary>('Z')	Z.</Summary>
            KEY_Z = 0x5A,

            [Description(@"Numpad *.")]
            /// <Summary>Numpad *.</Summary>
            MULTIPLY = 0x6A,

            [Description(@"NoName.")]
            /// <Summary>NoName.</Summary>
            NONAME = 0xFC,

            [Description(@"Numpad 0.")]
            /// <Summary>Numpad 0.</Summary>
            NUMPAD0 = 0x60,

            [Description(@"Numpad 1.")]
            /// <Summary>Numpad 1.</Summary>
            NUMPAD1 = 0x61,

            [Description(@"Numpad 2.")]
            /// <Summary>Numpad 2.</Summary>
            NUMPAD2 = 0x62,

            [Description(@"Numpad 3.")]
            /// <Summary>Numpad 3.</Summary>
            NUMPAD3 = 0x63,

            [Description(@"Numpad 4.")]
            /// <Summary>Numpad 4.</Summary>
            NUMPAD4 = 0x64,

            [Description(@"Numpad 5.")]
            /// <Summary>Numpad 5.</Summary>
            NUMPAD5 = 0x65,

            [Description(@"Numpad 6.")]
            /// <Summary>Numpad 6.</Summary>
            NUMPAD6 = 0x66,

            [Description(@"Numpad 7.")]
            /// <Summary>Numpad 7.</Summary>
            NUMPAD7 = 0x67,

            [Description(@"Numpad 8.")]
            /// <Summary>Numpad 8.</Summary>
            NUMPAD8 = 0x68,

            [Description(@"Numpad 9.")]
            /// <Summary>Numpad 9.</Summary>
            NUMPAD9 = 0x69,

            [Description(@"OEM_1 (: ;).")]
            /// <Summary>OEM_1 (: ;).</Summary>
            OEM_1 = 0xBA,

            [Description(@"OEM_102 (> <).")]
            /// <Summary>OEM_102 (> <).</Summary>
            OEM_102 = 0xE2,

            [Description(@"OEM_2 (? /).")]
            /// <Summary>OEM_2 (? /).</Summary>
            OEM_2 = 0xBF,

            [Description(@"OEM_3 (~ `).")]
            /// <Summary>OEM_3 (~ `).</Summary>
            OEM_3 = 0xC0,

            [Description(@"OEM_4 ({ [).")]
            /// <Summary>OEM_4 ({ [).</Summary>
            OEM_4 = 0xDB,

            [Description(@"OEM_5 (| \).")]
            /// <Summary>OEM_5 (| \).</Summary>
            OEM_5 = 0xDC,

            [Description(@"OEM_6 (} ]).")]
            /// <Summary>OEM_6 (} ]).</Summary>
            OEM_6 = 0xDD,

            [Description("OEM_7 (\" ').")]
            /// <Summary>OEM_7 (" ').</Summary>
            OEM_7 = 0xDE,

            [Description(@"OEM_8 (§ !).")]
            /// <Summary>OEM_8 (§ !).</Summary>
            OEM_8 = 0xDF,

            [Description(@"Oem Attn.")]
            /// <Summary>Oem Attn.</Summary>
            OEM_ATTN = 0xF0,

            [Description(@"Auto.")]
            /// <Summary>Auto.</Summary>
            OEM_AUTO = 0xF3,

            [Description(@"Ax.")]
            /// <Summary>Ax.</Summary>
            OEM_AX = 0xE1,

            [Description(@"Back Tab.")]
            /// <Summary>Back Tab.</Summary>
            OEM_BACKTAB = 0xF5,

            [Description(@"OemClr.")]
            /// <Summary>OemClr.</Summary>
            OEM_CLEAR = 0xFE,

            [Description(@"OEM_COMMA (< ,).")]
            /// <Summary>OEM_COMMA (< ,).</Summary>
            OEM_COMMA = 0xBC,

            [Description(@"Copy.")]
            /// <Summary>Copy.</Summary>
            OEM_COPY = 0xF2,

            [Description(@"Cu Sel.")]
            /// <Summary>Cu Sel.</Summary>
            OEM_CUSEL = 0xEF,

            [Description(@"Enlw.")]
            /// <Summary>Enlw.</Summary>
            OEM_ENLW = 0xF4,

            [Description(@"Finish.")]
            /// <Summary>Finish.</Summary>
            OEM_FINISH = 0xF1,

            [Description(@"Loya.")]
            /// <Summary>Loya.</Summary>
            OEM_FJ_LOYA = 0x95,

            [Description(@"Mashu.")]
            /// <Summary>Mashu.</Summary>
            OEM_FJ_MASSHOU = 0x93,

            [Description(@"Roya.")]
            /// <Summary>Roya.</Summary>
            OEM_FJ_ROYA = 0x96,

            [Description(@"Touroku.")]
            /// <Summary>Touroku.</Summary>
            OEM_FJ_TOUROKU = 0x94,

            [Description(@"Jump.")]
            /// <Summary>Jump.</Summary>
            OEM_JUMP = 0xEA,

            [Description(@"OEM_MINUS (_ -).")]
            /// <Summary>OEM_MINUS (_ -).</Summary>
            OEM_MINUS = 0xBD,

            [Description(@"OemPa1.")]
            /// <Summary>OemPa1.</Summary>
            OEM_PA1 = 0xEB,

            [Description(@"OemPa2.")]
            /// <Summary>OemPa2.</Summary>
            OEM_PA2 = 0xEC,

            [Description(@"OemPa3.")]
            /// <Summary>OemPa3.</Summary>
            OEM_PA3 = 0xED,

            [Description(@"OEM_PERIOD (> .).")]
            /// <Summary>OEM_PERIOD (> .).</Summary>
            OEM_PERIOD = 0xBE,

            [Description(@"OEM_PLUS (+ =).")]
            /// <Summary>OEM_PLUS (+ =).</Summary>
            OEM_PLUS = 0xBB,

            [Description(@"Reset.")]
            /// <Summary>Reset.</Summary>
            OEM_RESET = 0xE9,

            [Description(@"WsCtrl.")]
            /// <Summary>WsCtrl.</Summary>
            OEM_WSCTRL = 0xEE,

            [Description(@"Pa1.")]
            /// <Summary>Pa1.</Summary>
            PA1 = 0xFD,

            [Description(@"Packet.")]
            /// <Summary>Packet.</Summary>
            PACKET = 0xE7,

            [Description(@"Play.")]
            /// <Summary>Play.</Summary>
            PLAY = 0xFA,

            [Description(@"Process.")]
            /// <Summary>Process.</Summary>
            PROCESSKEY = 0xE5,

            [Description(@"Enter.")]
            /// <Summary>Enter.</Summary>
            RETURN = 0x0D,

            [Description(@"Select.")]
            /// <Summary>Select.</Summary>
            SELECT = 0x29,

            [Description(@"Separator.")]
            /// <Summary>Separator.</Summary>
            SEPARATOR = 0x6C,

            [Description(@"Space.")]
            /// <Summary>Space.</Summary>
            SPACE = 0x20,

            [Description(@"Num -.")]
            /// <Summary>Num -.</Summary>
            SUBTRACT = 0x6D,

            [Description(@"Tab.")]
            /// <Summary>Tab.</Summary>
            TAB = 0x09,

            [Description(@"Zoom.")]
            /// <Summary>Zoom.</Summary>
            ZOOM = 0xFB,

            [Description(@"")]
            /// <Summary>no VK mapping.</Summary>
            _NONE_ = 0xFF,

            [Description(@"Accept.")]
            /// <Summary>Accept.</Summary>
            ACCEPT = 0x1E,

            [Description(@"Context Menu.")]
            /// <Summary>Context Menu.</Summary>
            APPS = 0x5D,

            [Description(@"Browser Back.")]
            /// <Summary>Browser Back.</Summary>
            BROWSER_BACK = 0xA6,

            [Description(@"Browser Favorites.")]
            /// <Summary>Browser Favorites.</Summary>
            BROWSER_FAVORITES = 0xAB,

            [Description(@"Browser Forward.")]
            /// <Summary>Browser Forward.</Summary>
            BROWSER_FORWARD = 0xA7,

            [Description(@"Browser Home.")]
            /// <Summary>Browser Home.</Summary>
            BROWSER_HOME = 0xAC,

            [Description(@"Browser Refresh.")]
            /// <Summary>Browser Refresh.</Summary>
            BROWSER_REFRESH = 0xA8,

            [Description(@"Browser Search.")]
            /// <Summary>Browser Search.</Summary>
            BROWSER_SEARCH = 0xAA,

            [Description(@"Browser Stop.")]
            /// <Summary>Browser Stop.</Summary>
            BROWSER_STOP = 0xA9,

            [Description(@"Caps Lock.")]
            /// <Summary>Caps Lock.</Summary>
            CAPITAL = 0x14,

            [Description(@"Convert.")]
            /// <Summary>Convert.</Summary>
            CONVERT = 0x1C,

            [Description(@"Delete.")]
            /// <Summary>Delete.</Summary>
            DELETE = 0x2E,

            [Description(@"Arrow Down.")]
            /// <Summary>Arrow Down.</Summary>
            DOWN = 0x28,

            [Description(@"End.")]
            /// <Summary>End.</Summary>
            END = 0x23,

            [Description(@"F1.")]
            /// <Summary>F1.</Summary>
            F1 = 0x70,

            [Description(@"F10.")]
            /// <Summary>F10.</Summary>
            F10 = 0x79,

            [Description(@"F11.")]
            /// <Summary>F11.</Summary>
            F11 = 0x7A,

            [Description(@"F12.")]
            /// <Summary>F12.</Summary>
            F12 = 0x7B,

            [Description(@"F13.")]
            /// <Summary>F13.</Summary>
            F13 = 0x7C,

            [Description(@"F14.")]
            /// <Summary>F14.</Summary>
            F14 = 0x7D,

            [Description(@"F15.")]
            /// <Summary>F15.</Summary>
            F15 = 0x7E,

            [Description(@"F16.")]
            /// <Summary>F16.</Summary>
            F16 = 0x7F,

            [Description(@"F17.")]
            /// <Summary>F17.</Summary>
            F17 = 0x80,

            [Description(@"F18.")]
            /// <Summary>F18.</Summary>
            F18 = 0x81,

            [Description(@"F19.")]
            /// <Summary>F19.</Summary>
            F19 = 0x82,

            [Description(@"F2.")]
            /// <Summary>F2.</Summary>
            F2 = 0x71,

            [Description(@"F20.")]
            /// <Summary>F20.</Summary>
            F20 = 0x83,

            [Description(@"F21.")]
            /// <Summary>F21.</Summary>
            F21 = 0x84,

            [Description(@"F22.")]
            /// <Summary>F22.</Summary>
            F22 = 0x85,

            [Description(@"F23.")]
            /// <Summary>F23.</Summary>
            F23 = 0x86,

            [Description(@"F24.")]
            /// <Summary>F24.</Summary>
            F24 = 0x87,

            [Description(@"F3.")]
            /// <Summary>F3.</Summary>
            F3 = 0x72,

            [Description(@"F4.")]
            /// <Summary>F4.</Summary>
            F4 = 0x73,

            [Description(@"F5.")]
            /// <Summary>F5.</Summary>
            F5 = 0x74,

            [Description(@"F6.")]
            /// <Summary>F6.</Summary>
            F6 = 0x75,

            [Description(@"F7.")]
            /// <Summary>F7.</Summary>
            F7 = 0x76,

            [Description(@"F8.")]
            /// <Summary>F8.</Summary>
            F8 = 0x77,

            [Description(@"F9.")]
            /// <Summary>F9.</Summary>
            F9 = 0x78,

            [Description(@"Final.")]
            /// <Summary>Final.</Summary>
            FINAL = 0x18,

            [Description(@"Help.")]
            /// <Summary>Help.</Summary>
            HELP = 0x2F,

            [Description(@"Home.")]
            /// <Summary>Home.</Summary>
            HOME = 0x24,

            [Description(@"Ico00 *.")]
            /// <Summary>Ico00 *.</Summary>
            ICO_00 = 0xE4,

            [Description(@"Insert.")]
            /// <Summary>Insert.</Summary>
            INSERT = 0x2D,

            [Description(@"Junja.")]
            /// <Summary>Junja.</Summary>
            JUNJA = 0x17,

            [Description(@"Kana.")]
            /// <Summary>Kana.</Summary>
            KANA = 0x15,

            [Description(@"Kanji.")]
            /// <Summary>Kanji.</Summary>
            KANJI = 0x19,

            [Description(@"App1.")]
            /// <Summary>App1.</Summary>
            LAUNCH_APP1 = 0xB6,

            [Description(@"App2.")]
            /// <Summary>App2.</Summary>
            LAUNCH_APP2 = 0xB7,

            [Description(@"Mail.")]
            /// <Summary>Mail.</Summary>
            LAUNCH_MAIL = 0xB4,

            [Description(@"Media.")]
            /// <Summary>Media.</Summary>
            LAUNCH_MEDIA_SELECT = 0xB5,

            [Description(@"Left Button **.")]
            /// <Summary>Left Button **.</Summary>
            LBUTTON = 0x01,

            [Description(@"Left Ctrl.")]
            /// <Summary>Left Ctrl.</Summary>
            LCONTROL = 0xA2,

            [Description(@"Arrow Left.")]
            /// <Summary>Arrow Left.</Summary>
            LEFT = 0x25,

            [Description(@"Left Alt.")]
            /// <Summary>Left Alt.</Summary>
            LMENU = 0xA4,

            [Description(@"Left Shift.")]
            /// <Summary>Left Shift.</Summary>
            LSHIFT = 0xA0,

            [Description(@"Left Win.")]
            /// <Summary>Left Win.</Summary>
            LWIN = 0x5B,

            [Description(@"Middle Button **.")]
            /// <Summary>Middle Button **.</Summary>
            MBUTTON = 0x04,

            [Description(@"Next Track.")]
            /// <Summary>Next Track.</Summary>
            MEDIA_NEXT_TRACK = 0xB0,

            [Description(@"Play / Pause.")]
            /// <Summary>Play / Pause.</Summary>
            MEDIA_PLAY_PAUSE = 0xB3,

            [Description(@"Previous Track.")]
            /// <Summary>Previous Track.</Summary>
            MEDIA_PREV_TRACK = 0xB1,

            [Description(@"Stop.")]
            /// <Summary>Stop.</Summary>
            MEDIA_STOP = 0xB2,

            [Description(@"Mode Change.")]
            /// <Summary>Mode Change.</Summary>
            MODECHANGE = 0x1F,

            [Description(@"Page Down.")]
            /// <Summary>Page Down.</Summary>
            NEXT = 0x22,

            [Description(@"Non Convert.")]
            /// <Summary>Non Convert.</Summary>
            NONCONVERT = 0x1D,

            [Description(@"Num Lock.")]
            /// <Summary>Num Lock.</Summary>
            NUMLOCK = 0x90,

            [Description(@"Jisho.")]
            /// <Summary>Jisho.</Summary>
            OEM_FJ_JISHO = 0x92,

            [Description(@"Pause.")]
            /// <Summary>Pause.</Summary>
            PAUSE = 0x13,

            [Description(@"Print.")]
            /// <Summary>Print.</Summary>
            PRINT = 0x2A,

            [Description(@"Page Up.")]
            /// <Summary>Page Up.</Summary>
            PRIOR = 0x21,

            [Description(@"Right Button **.")]
            /// <Summary>Right Button **.</Summary>
            RBUTTON = 0x02,

            [Description(@"Right Ctrl.")]
            /// <Summary>Right Ctrl.</Summary>
            RCONTROL = 0xA3,

            [Description(@"Arrow Right.")]
            /// <Summary>Arrow Right.</Summary>
            RIGHT = 0x27,

            [Description(@"Right Alt.")]
            /// <Summary>Right Alt.</Summary>
            RMENU = 0xA5,

            [Description(@"Right Shift.")]
            /// <Summary>Right Shift.</Summary>
            RSHIFT = 0xA1,

            [Description(@"Right Win.")]
            /// <Summary>Right Win.</Summary>
            RWIN = 0x5C,

            [Description(@"Scrol Lock.")]
            /// <Summary>Scrol Lock.</Summary>
            SCROLL = 0x91,

            [Description(@"Sleep.")]
            /// <Summary>Sleep.</Summary>
            SLEEP = 0x5F,

            [Description(@"Print Screen.")]
            /// <Summary>Print Screen.</Summary>
            SNAPSHOT = 0x2C,

            [Description(@"Arrow Up.")]
            /// <Summary>Arrow Up.</Summary>
            UP = 0x26,

            [Description(@"Volume Down.")]
            /// <Summary>Volume Down.</Summary>
            VOLUME_DOWN = 0xAE,

            [Description(@"Volume Mute.")]
            /// <Summary>Volume Mute.</Summary>
            VOLUME_MUTE = 0xAD,

            [Description(@"Volume Up.")]
            /// <Summary>Volume Up.</Summary>
            VOLUME_UP = 0xAF,

            [Description(@"X Button 1 **.")]
            /// <Summary>X Button 1 **.</Summary>
            XBUTTON1 = 0x05,

            [Description(@"X Button 2 **.")]
            /// <Summary>X Button 2 **.</Summary>
            XBUTTON2 = 0x06,
        }


        #region Functions
        /// <summary> Gets the description tag entry from the SupportedGame enum.
        /// </summary>
        /// <param name="keycode">The virtual keycode for which to get the description.</param>
        /// <returns>The virtual key code.</returns>
        public static string GetDescription(Keys keycode)
        {
            Type valType = typeof(Keys);
            string name = Enum.GetName(valType, keycode);

            if (name != null)
            {
                System.Reflection.FieldInfo field = valType.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                    if (attribute != null) { return attribute.Description; }
                }
            }

            return null;
        }
        #endregion
    }
}
