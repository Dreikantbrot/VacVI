using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.Engine
{
    /// <Summary> Contains DirectInput key codes.
    /// <para>Source: http://www.flint.jp/misc/?q=dik&lang=en
    /// </para>
    /// </Summary>
    public struct DIKCodes
    {
        /// <Summary>Esc</Summary>
        public const int DIK_ESCAPE = 0x01;

        /// <Summary>1</Summary>
        public const int DIK_1 = 0x02;

        /// <Summary>2</Summary>
        public const int DIK_2 = 0x03;

        /// <Summary>3</Summary>
        public const int DIK_3 = 0x04;

        /// <Summary>4</Summary>
        public const int DIK_4 = 0x05;

        /// <Summary>5</Summary>
        public const int DIK_5 = 0x06;

        /// <Summary>6</Summary>
        public const int DIK_6 = 0x07;

        /// <Summary>7</Summary>
        public const int DIK_7 = 0x08;

        /// <Summary>8</Summary>
        public const int DIK_8 = 0x09;

        /// <Summary>9</Summary>
        public const int DIK_9 = 0x0A;

        /// <Summary>0</Summary>
        public const int DIK_0 = 0x0B;

        /// <Summary>-</Summary>
        public const int DIK_MINUS = 0x0C;

        /// <Summary>=</Summary>
        public const int DIK_EQUALS = 0x0D;

        /// <Summary>Back Space</Summary>
        public const int DIK_BACK = 0x0E;

        /// <Summary>Tab</Summary>
        public const int DIK_TAB = 0x0F;

        /// <Summary>Q</Summary>
        public const int DIK_Q = 0x10;

        /// <Summary>W</Summary>
        public const int DIK_W = 0x11;

        /// <Summary>E</Summary>
        public const int DIK_E = 0x12;

        /// <Summary>R</Summary>
        public const int DIK_R = 0x13;

        /// <Summary>T</Summary>
        public const int DIK_T = 0x14;

        /// <Summary>Y</Summary>
        public const int DIK_Y = 0x15;

        /// <Summary>U</Summary>
        public const int DIK_U = 0x16;

        /// <Summary>I</Summary>
        public const int DIK_I = 0x17;

        /// <Summary>O</Summary>
        public const int DIK_O = 0x18;

        /// <Summary>P</Summary>
        public const int DIK_P = 0x19;

        /// <Summary>[</Summary>
        public const int DIK_LBRACKET = 0x1A;

        /// <Summary>]</Summary>
        public const int DIK_RBRACKET = 0x1B;

        /// <Summary>Enter</Summary>
        public const int DIK_RETURN = 0x1C;

        /// <Summary>Ctrl (Left)</Summary>
        public const int DIK_LContol = 0x1D;

        /// <Summary>A</Summary>
        public const int DIK_A = 0x1E;

        /// <Summary>S</Summary>
        public const int DIK_S = 0x1F;

        /// <Summary>D</Summary>
        public const int DIK_D = 0x20;

        /// <Summary>F</Summary>
        public const int DIK_F = 0x21;

        /// <Summary>G</Summary>
        public const int DIK_G = 0x22;

        /// <Summary>H</Summary>
        public const int DIK_H = 0x23;

        /// <Summary>J</Summary>
        public const int DIK_J = 0x24;

        /// <Summary>K</Summary>
        public const int DIK_K = 0x25;

        /// <Summary>L</Summary>
        public const int DIK_L = 0x26;

        /// <Summary>;</Summary>
        public const int DIK_SEMICOLON = 0x27;

        /// <Summary>'</Summary>
        public const int DIK_APOSTROPHE = 0x28;

        /// <Summary>`</Summary>
        public const int DIK_GRAVE = 0x29;

        /// <Summary>Shift (Left)</Summary>
        public const int DIK_LSHIFT = 0x2A;

        /// <Summary>\</Summary>
        public const int DIK_BACKSLASH = 0x2B;

        /// <Summary>Z</Summary>
        public const int DIK_Z = 0x2C;

        /// <Summary>X</Summary>
        public const int DIK_X = 0x2D;

        /// <Summary>C</Summary>
        public const int DIK_C = 0x2E;

        /// <Summary>V</Summary>
        public const int DIK_V = 0x2F;

        /// <Summary>B</Summary>
        public const int DIK_B = 0x30;

        /// <Summary>N</Summary>
        public const int DIK_N = 0x31;

        /// <Summary>M</Summary>
        public const int DIK_M = 0x32;

        /// <Summary>,</Summary>
        public const int DIK_COMMA = 0x33;

        /// <Summary>.</Summary>
        public const int DIK_PERIOD = 0x34;

        /// <Summary>/</Summary>
        public const int DIK_SLASH = 0x35;

        /// <Summary>Shift (Right)</Summary>
        public const int DIK_RSHIFT = 0x36;

        /// <Summary>* (Numpad)</Summary>
        public const int DIK_MULTIPLY = 0x37;

        /// <Summary>Alt (Left)</Summary>
        public const int DIK_LMENU = 0x38;

        /// <Summary>Space</Summary>
        public const int DIK_SPACE = 0x39;

        /// <Summary>Caps Lock</Summary>
        public const int DIK_CAPITAL = 0x3A;

        /// <Summary>F1</Summary>
        public const int DIK_F1 = 0x3B;

        /// <Summary>F2</Summary>
        public const int DIK_F2 = 0x3C;

        /// <Summary>F3</Summary>
        public const int DIK_F3 = 0x3D;

        /// <Summary>F4</Summary>
        public const int DIK_F4 = 0x3E;

        /// <Summary>F5</Summary>
        public const int DIK_F5 = 0x3F;

        /// <Summary>F6</Summary>
        public const int DIK_F6 = 0x40;

        /// <Summary>F7</Summary>
        public const int DIK_F7 = 0x41;

        /// <Summary>F8</Summary>
        public const int DIK_F8 = 0x42;

        /// <Summary>F9</Summary>
        public const int DIK_F9 = 0x43;

        /// <Summary>F10</Summary>
        public const int DIK_F10 = 0x44;

        /// <Summary>Num Lock</Summary>
        public const int DIK_NUMLOCK = 0x45;

        /// <Summary>Scroll Lock</Summary>
        public const int DIK_SCROLL = 0x46;

        /// <Summary>7 (Numpad)</Summary>
        public const int DIK_NUMPAD7 = 0x47;

        /// <Summary>8 (Numpad)</Summary>
        public const int DIK_NUMPAD8 = 0x48;

        /// <Summary>9 (Numpad)</Summary>
        public const int DIK_NUMPAD9 = 0x49;

        /// <Summary>- (Numpad)</Summary>
        public const int DIK_SUBTRACT = 0x4A;

        /// <Summary>4 (Numpad)</Summary>
        public const int DIK_NUMPAD4 = 0x4B;

        /// <Summary>5 (Numpad)</Summary>
        public const int DIK_NUMPAD5 = 0x4C;

        /// <Summary>6 (Numpad)</Summary>
        public const int DIK_NUMPAD6 = 0x4D;

        /// <Summary>+ (Numpad)</Summary>
        public const int DIK_ADD = 0x4E;

        /// <Summary>1 (Numpad)</Summary>
        public const int DIK_NUMPAD1 = 0x4F;

        /// <Summary>2 (Numpad)</Summary>
        public const int DIK_NUMPAD2 = 0x50;

        /// <Summary>3 (Numpad)</Summary>
        public const int DIK_NUMPAD3 = 0x51;

        /// <Summary>0 (Numpad)</Summary>
        public const int DIK_NUMPAD0 = 0x52;

        /// <Summary>. (Numpad)</Summary>
        public const int DIK_DECIMAL = 0x53;

        /// <Summary>F11</Summary>
        public const int DIK_F11 = 0x57;

        /// <Summary>F12</Summary>
        public const int DIK_F12 = 0x58;

        /// <Summary>F13	NEC PC-98</Summary>
        public const int DIK_F13 = 0x64;

        /// <Summary>F14	NEC PC-98</Summary>
        public const int DIK_F14 = 0x65;

        /// <Summary>F15	NEC PC-98</Summary>
        public const int DIK_F15 = 0x66;

        /// <Summary>Kana	Japenese Keyboard</Summary>
        public const int DIK_KANA = 0x70;

        /// <Summary>Convert	Japenese Keyboard</Summary>
        public const int DIK_CONVERT = 0x79;

        /// <Summary>No Convert	Japenese Keyboard</Summary>
        public const int DIK_NOCONVERT = 0x7B;

        /// <Summary>¥	Japenese Keyboard</Summary>
        public const int DIK_YEN = 0x7D;

        /// <Summary>=	NEC PC-98</Summary>
        public const int DIK_NUMPADEQUALS = 0x8D;

        /// <Summary>^	Japenese Keyboard</Summary>
        public const int DIK_CIRCUMFLEX = 0x90;

        /// <Summary>@	NEC PC-98</Summary>
        public const int DIK_AT = 0x91;

        /// <Summary>:	NEC PC-98</Summary>
        public const int DIK_COLON = 0x92;

        /// <Summary>_	NEC PC-98</Summary>
        public const int DIK_UNDERLINE = 0x93;

        /// <Summary>Kanji	Japenese Keyboard</Summary>
        public const int DIK_KANJI = 0x94;

        /// <Summary>Stop	NEC PC-98</Summary>
        public const int DIK_STOP = 0x95;

        /// <Summary>(Japan AX)</Summary>
        public const int DIK_AX = 0x96;

        /// <Summary>(J3100)</Summary>
        public const int DIK_UNLABELED = 0x97;

        /// <Summary>Enter (Numpad)</Summary>
        public const int DIK_NUMPADENTER = 0x9C;

        /// <Summary>Ctrl (Right)</Summary>
        public const int DIK_RCONTROL = 0x9D;

        /// <Summary>, (Numpad)	NEC PC-98</Summary>
        public const int DIK_NUMPADCOMMA = 0xB3;

        /// <Summary>/ (Numpad)</Summary>
        public const int DIK_DIVIDE = 0xB5;

        /// <Summary>Sys Rq</Summary>
        public const int DIK_SYSRQ = 0xB7;

        /// <Summary>Alt (Right)</Summary>
        public const int DIK_RMENU = 0xB8;

        /// <Summary>Pause</Summary>
        public const int DIK_PAUSE = 0xC5;

        /// <Summary>Home</Summary>
        public const int DIK_HOME = 0xC7;

        /// <Summary>↑</Summary>
        public const int DIK_UP = 0xC8;

        /// <Summary>Page Up</Summary>
        public const int DIK_PRIOR = 0xC9;

        /// <Summary>←</Summary>
        public const int DIK_LEFT = 0xCB;

        /// <Summary>→</Summary>
        public const int DIK_RIGHT = 0xCD;

        /// <Summary>End</Summary>
        public const int DIK_END = 0xCF;

        /// <Summary>↓</Summary>
        public const int DIK_DOWN = 0xD0;

        /// <Summary>Page Down</Summary>
        public const int DIK_NEXT = 0xD1;

        /// <Summary>Insert</Summary>
        public const int DIK_INSERT = 0xD2;

        /// <Summary>Delete</Summary>
        public const int DIK_DELETE = 0xD3;

        /// <Summary>Windows</Summary>
        public const int DIK_LWIN = 0xDB;

        /// <Summary>Windows</Summary>
        public const int DIK_RWIN = 0xDC;

        /// <Summary>Menu</Summary>
        public const int DIK_APPS = 0xDD;

        /// <Summary>Power</Summary>
        public const int DIK_POWER = 0xDE;

        /// <Summary>Windows</Summary>
        public const int DIK_SLEEP = 0xDF;
    }
}
