using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.Input
{
    /// <Summary> Contains DirectInput key codes.
    /// <para>Source: http://www.flint.jp/misc/?q=dik&lang=en
    /// </para>
    /// </Summary>
    public struct DIKCodes
    {
        public enum Keys : int
        {
            [Description(@"")]
            /// <Summary>None</Summary>
            _NONE_ = 0x00,

            [Description(@"Esc")]
            /// <Summary>Esc</Summary>
            ESCAPE = 0x01,

            [Description(@"1")]
            /// <Summary>1</Summary>
            KEY_1 = 0x02,

            [Description(@"2")]
            /// <Summary>2</Summary>
            KEY_2 = 0x03,

            [Description(@"3")]
            /// <Summary>3</Summary>
            KEY_3 = 0x04,

            [Description(@"4")]
            /// <Summary>4</Summary>
            KEY_4 = 0x05,

            [Description(@"5")]
            /// <Summary>5</Summary>
            KEY_5 = 0x06,

            [Description(@"6")]
            /// <Summary>6</Summary>
            KEY_6 = 0x07,

            [Description(@"7")]
            /// <Summary>7</Summary>
            KEY_7 = 0x08,

            [Description(@"8")]
            /// <Summary>8</Summary>
            KEY_8 = 0x09,

            [Description(@"9")]
            /// <Summary>9</Summary>
            KEY_9 = 0x0A,

            [Description(@"0")]
            /// <Summary>0</Summary>
            KEY_0 = 0x0B,

            [Description(@"-")]
            /// <Summary>-</Summary>
            MINUS = 0x0C,

            [Description(@"=")]
            /// <Summary>=</Summary>
            EQUALS = 0x0D,

            [Description(@"Back Space")]
            /// <Summary>Back Space</Summary>
            BACK = 0x0E,

            [Description(@"Tab")]
            /// <Summary>Tab</Summary>
            TAB = 0x0F,

            [Description(@"Q")]
            /// <Summary>Q</Summary>
            Q = 0x10,

            [Description(@"W")]
            /// <Summary>W</Summary>
            W = 0x11,

            [Description(@"E")]
            /// <Summary>E</Summary>
            E = 0x12,

            [Description(@"R")]
            /// <Summary>R</Summary>
            R = 0x13,

            [Description(@"T")]
            /// <Summary>T</Summary>
            T = 0x14,

            [Description(@"Y")]
            /// <Summary>Y</Summary>
            Y = 0x15,

            [Description(@"U")]
            /// <Summary>U</Summary>
            U = 0x16,

            [Description(@"I")]
            /// <Summary>I</Summary>
            I = 0x17,

            [Description(@"O")]
            /// <Summary>O</Summary>
            O = 0x18,

            [Description(@"P")]
            /// <Summary>P</Summary>
            P = 0x19,

            [Description(@"[")]
            /// <Summary>[</Summary>
            LBRACKET = 0x1A,

            [Description(@"]")]
            /// <Summary>]</Summary>
            RBRACKET = 0x1B,

            [Description(@"Enter")]
            /// <Summary>Enter</Summary>
            RETURN = 0x1C,

            [Description(@"Ctrl (Left)")]
            /// <Summary>Ctrl (Left)</Summary>
            LContol = 0x1D,

            [Description(@"A")]
            /// <Summary>A</Summary>
            A = 0x1E,

            [Description(@"S")]
            /// <Summary>S</Summary>
            S = 0x1F,

            [Description(@"D")]
            /// <Summary>D</Summary>
            D = 0x20,

            [Description(@"F")]
            /// <Summary>F</Summary>
            F = 0x21,

            [Description(@"G")]
            /// <Summary>G</Summary>
            G = 0x22,

            [Description(@"H")]
            /// <Summary>H</Summary>
            H = 0x23,

            [Description(@"J")]
            /// <Summary>J</Summary>
            J = 0x24,

            [Description(@"K")]
            /// <Summary>K</Summary>
            K = 0x25,

            [Description(@"L")]
            /// <Summary>L</Summary>
            L = 0x26,

            [Description(@";")]
            /// <Summary>;</Summary>
            SEMICOLON = 0x27,

            [Description(@"'")]
            /// <Summary>'</Summary>
            APOSTROPHE = 0x28,

            [Description(@"`")]
            /// <Summary>`</Summary>
            GRAVE = 0x29,

            [Description(@"Shift (Left)")]
            /// <Summary>Shift (Left)</Summary>
            LSHIFT = 0x2A,

            [Description(@"\")]
            /// <Summary>\</Summary>
            BACKSLASH = 0x2B,

            [Description(@"Z")]
            /// <Summary>Z</Summary>
            Z = 0x2C,

            [Description(@"X")]
            /// <Summary>X</Summary>
            X = 0x2D,

            [Description(@"C")]
            /// <Summary>C</Summary>
            C = 0x2E,

            [Description(@"V")]
            /// <Summary>V</Summary>
            V = 0x2F,

            [Description(@"B")]
            /// <Summary>B</Summary>
            B = 0x30,

            [Description(@"N")]
            /// <Summary>N</Summary>
            N = 0x31,

            [Description(@"M")]
            /// <Summary>M</Summary>
            M = 0x32,

            [Description(@",")]
            /// <Summary>,</Summary>
            COMMA = 0x33,

            [Description(@".")]
            /// <Summary>.</Summary>
            PERIOD = 0x34,

            [Description(@"/")]
            /// <Summary>/</Summary>
            SLASH = 0x35,

            [Description(@"Shift (Right)")]
            /// <Summary>Shift (Right)</Summary>
            RSHIFT = 0x36,

            [Description(@"* (Numpad)")]
            /// <Summary>* (Numpad)</Summary>
            MULTIPLY = 0x37,

            [Description(@"Alt (Left)")]
            /// <Summary>Alt (Left)</Summary>
            LMENU = 0x38,

            [Description(@"Space")]
            /// <Summary>Space</Summary>
            SPACE = 0x39,

            [Description(@"Caps Lock")]
            /// <Summary>Caps Lock</Summary>
            CAPITAL = 0x3A,

            [Description(@"F1")]
            /// <Summary>F1</Summary>
            F1 = 0x3B,

            [Description(@"F2")]
            /// <Summary>F2</Summary>
            F2 = 0x3C,

            [Description(@"F3")]
            /// <Summary>F3</Summary>
            F3 = 0x3D,

            [Description(@"F4")]
            /// <Summary>F4</Summary>
            F4 = 0x3E,

            [Description(@"F5")]
            /// <Summary>F5</Summary>
            F5 = 0x3F,

            [Description(@"F6")]
            /// <Summary>F6</Summary>
            F6 = 0x40,

            [Description(@"F7")]
            /// <Summary>F7</Summary>
            F7 = 0x41,

            [Description(@"F8")]
            /// <Summary>F8</Summary>
            F8 = 0x42,

            [Description(@"F9")]
            /// <Summary>F9</Summary>
            F9 = 0x43,

            [Description(@"F10")]
            /// <Summary>F10</Summary>
            F10 = 0x44,

            [Description(@"Num Lock")]
            /// <Summary>Num Lock</Summary>
            NUMLOCK = 0x45,

            [Description(@"Scroll Lock")]
            /// <Summary>Scroll Lock</Summary>
            SCROLL = 0x46,

            [Description(@"7 (Numpad)")]
            /// <Summary>7 (Numpad)</Summary>
            NUMPAD7 = 0x47,

            [Description(@"8 (Numpad)")]
            /// <Summary>8 (Numpad)</Summary>
            NUMPAD8 = 0x48,

            [Description(@"9 (Numpad)")]
            /// <Summary>9 (Numpad)</Summary>
            NUMPAD9 = 0x49,

            [Description(@"- (Numpad)")]
            /// <Summary>- (Numpad)</Summary>
            SUBTRACT = 0x4A,

            [Description(@"4 (Numpad)")]
            /// <Summary>4 (Numpad)</Summary>
            NUMPAD4 = 0x4B,

            [Description(@"5 (Numpad)")]
            /// <Summary>5 (Numpad)</Summary>
            NUMPAD5 = 0x4C,

            [Description(@"6 (Numpad)")]
            /// <Summary>6 (Numpad)</Summary>
            NUMPAD6 = 0x4D,

            [Description(@"+ (Numpad)")]
            /// <Summary>+ (Numpad)</Summary>
            ADD = 0x4E,

            [Description(@"1 (Numpad)")]
            /// <Summary>1 (Numpad)</Summary>
            NUMPAD1 = 0x4F,

            [Description(@"2 (Numpad)")]
            /// <Summary>2 (Numpad)</Summary>
            NUMPAD2 = 0x50,

            [Description(@"3 (Numpad)")]
            /// <Summary>3 (Numpad)</Summary>
            NUMPAD3 = 0x51,

            [Description(@"0 (Numpad)")]
            /// <Summary>0 (Numpad)</Summary>
            NUMPAD0 = 0x52,

            [Description(@". (Numpad)")]
            /// <Summary>. (Numpad)</Summary>
            DECIMAL = 0x53,

            [Description(@"F11")]
            /// <Summary>F11</Summary>
            F11 = 0x57,

            [Description(@"F12")]
            /// <Summary>F12</Summary>
            F12 = 0x58,

            [Description(@"F13	NEC PC-98")]
            /// <Summary>F13	NEC PC-98</Summary>
            F13 = 0x64,

            [Description(@"F14	NEC PC-98")]
            /// <Summary>F14	NEC PC-98</Summary>
            F14 = 0x65,

            [Description(@"F15	NEC PC-98")]
            /// <Summary>F15	NEC PC-98</Summary>
            F15 = 0x66,

            [Description(@"Kana	Japenese Keyboard")]
            /// <Summary>Kana	Japenese Keyboard</Summary>
            KANA = 0x70,

            [Description(@"Convert	Japenese Keyboard")]
            /// <Summary>Convert	Japenese Keyboard</Summary>
            CONVERT = 0x79,

            [Description(@"No Convert	Japenese Keyboard")]
            /// <Summary>No Convert	Japenese Keyboard</Summary>
            NOCONVERT = 0x7B,

            [Description(@"¥	Japenese Keyboard")]
            /// <Summary>¥	Japenese Keyboard</Summary>
            YEN = 0x7D,

            [Description(@"=	NEC PC-98")]
            /// <Summary>=	NEC PC-98</Summary>
            NUMPADEQUALS = 0x8D,

            [Description(@"^	Japenese Keyboard")]
            /// <Summary>^	Japenese Keyboard</Summary>
            CIRCUMFLEX = 0x90,

            [Description(@"@	NEC PC-98")]
            /// <Summary>@	NEC PC-98</Summary>
            AT = 0x91,

            [Description(@":	NEC PC-98")]
            /// <Summary>:	NEC PC-98</Summary>
            COLON = 0x92,

            [Description(@"_	NEC PC-98")]
            /// <Summary>_	NEC PC-98</Summary>
            UNDERLINE = 0x93,

            [Description(@"Kanji	Japenese Keyboard")]
            /// <Summary>Kanji	Japenese Keyboard</Summary>
            KANJI = 0x94,

            [Description(@"Stop	NEC PC-98")]
            /// <Summary>Stop	NEC PC-98</Summary>
            STOP = 0x95,

            [Description(@"(Japan AX)")]
            /// <Summary>(Japan AX)</Summary>
            AX = 0x96,

            [Description(@"(J3100)")]
            /// <Summary>(J3100)</Summary>
            UNLABELED = 0x97,

            [Description(@"Enter (Numpad)")]
            /// <Summary>Enter (Numpad)</Summary>
            NUMPADENTER = 0x9C,

            [Description(@"Ctrl (Right)")]
            /// <Summary>Ctrl (Right)</Summary>
            RCONTROL = 0x9D,

            [Description(@", (Numpad)	NEC PC-98")]
            /// <Summary>, (Numpad)	NEC PC-98</Summary>
            NUMPADCOMMA = 0xB3,

            [Description(@"/ (Numpad)")]
            /// <Summary>/ (Numpad)</Summary>
            DIVIDE = 0xB5,

            [Description(@"Sys Rq")]
            /// <Summary>Sys Rq</Summary>
            SYSRQ = 0xB7,

            [Description(@"Alt (Right)")]
            /// <Summary>Alt (Right)</Summary>
            RMENU = 0xB8,

            [Description(@"Pause")]
            /// <Summary>Pause</Summary>
            PAUSE = 0xC5,

            [Description(@"Home")]
            /// <Summary>Home</Summary>
            HOME = 0xC7,

            [Description(@"↑")]
            /// <Summary>↑</Summary>
            UP = 0xC8,

            [Description(@"Page Up")]
            /// <Summary>Page Up</Summary>
            PRIOR = 0xC9,

            [Description(@"←")]
            /// <Summary>←</Summary>
            LEFT = 0xCB,

            [Description(@"→")]
            /// <Summary>→</Summary>
            RIGHT = 0xCD,

            [Description(@"End")]
            /// <Summary>End</Summary>
            END = 0xCF,

            [Description(@"↓")]
            /// <Summary>↓</Summary>
            DOWN = 0xD0,

            [Description(@"Page Down")]
            /// <Summary>Page Down</Summary>
            NEXT = 0xD1,

            [Description(@"Insert")]
            /// <Summary>Insert</Summary>
            INSERT = 0xD2,

            [Description(@"Delete")]
            /// <Summary>Delete</Summary>
            DELETE = 0xD3,

            [Description(@"Windows")]
            /// <Summary>Windows</Summary>
            LWIN = 0xDB,

            [Description(@"Windows")]
            /// <Summary>Windows</Summary>
            RWIN = 0xDC,

            [Description(@"Menu")]
            /// <Summary>Menu</Summary>
            APPS = 0xDD,

            [Description(@"Power")]
            /// <Summary>Power</Summary>
            POWER = 0xDE,

            [Description(@"Windows")]
            /// <Summary>Windows</Summary>
            SLEEP = 0xDF,
        }


        #region Functions
        /// <summary> Gets the description tag entry from the SupportedGame enum.
        /// </summary>
        /// <param name="keycode">The DirectInput keycode for which to get the description.</param>
        /// <returns>The DirectInput key code.</returns>
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
