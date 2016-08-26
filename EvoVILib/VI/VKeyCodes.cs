using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.Engine
{
    /* Source: http://www.kbdedit.com/manual/low_level_vk_list.html */
    public struct VKeyCodes
    {
	    /// <Summary> Abnt C1.</Summary>
	    public const uint VK_ABNT_C1 = 0xC1;

	    /// <Summary> Abnt C2.</Summary>
	    public const uint VK_ABNT_C2 = 0xC2;

	    /// <Summary> Numpad +.</Summary>
	    public const uint VK_ADD = 0x6B;

	    /// <Summary> Attn.</Summary>
	    public const uint VK_ATTN = 0xF6;

	    /// <Summary> Backspace.</Summary>
	    public const uint VK_BACK = 0x08;

	    /// <Summary> Break.</Summary>
	    public const uint VK_CANCEL = 0x03;

	    /// <Summary> Clear.</Summary>
	    public const uint VK_CLEAR = 0x0C;

	    /// <Summary> Cr Sel.</Summary>
	    public const uint VK_CRSEL = 0xF7;

	    /// <Summary> Numpad ..</Summary>
	    public const uint VK_DECIMAL = 0x6E;

	    /// <Summary> Numpad /.</Summary>
	    public const uint VK_DIVIDE = 0x6F;

	    /// <Summary> Er Eof.</Summary>
	    public const uint VK_EREOF = 0xF9;

	    /// <Summary> Esc.</Summary>
	    public const uint VK_ESCAPE = 0x1B;

	    /// <Summary> Execute.</Summary>
	    public const uint VK_EXECUTE = 0x2B;

	    /// <Summary> Ex Sel.</Summary>
	    public const uint VK_EXSEL = 0xF8;

	    /// <Summary> IcoClr.</Summary>
	    public const uint VK_ICO_CLEAR = 0xE6;

	    /// <Summary> IcoHlp.</Summary>
	    public const uint VK_ICO_HELP = 0xE3;

	    /// <Summary> ('0')	0.</Summary>
	    public const uint VK_KEY_0 = 0x30;

	    /// <Summary> ('1')	1.</Summary>
	    public const uint VK_KEY_1 = 0x31;

	    /// <Summary> ('2')	2.</Summary>
	    public const uint VK_KEY_2 = 0x32;

	    /// <Summary> ('3')	3.</Summary>
	    public const uint VK_KEY_3 = 0x33;

	    /// <Summary> ('4')	4.</Summary>
	    public const uint VK_KEY_4 = 0x34;

	    /// <Summary> ('5')	5.</Summary>
	    public const uint VK_KEY_5 = 0x35;

	    /// <Summary> ('6')	6.</Summary>
	    public const uint VK_KEY_6 = 0x36;

	    /// <Summary> ('7')	7.</Summary>
	    public const uint VK_KEY_7 = 0x37;

	    /// <Summary> ('8')	8.</Summary>
	    public const uint VK_KEY_8 = 0x38;

	    /// <Summary> ('9')	9.</Summary>
	    public const uint VK_KEY_9 = 0x39;

	    /// <Summary> ('A')	A.</Summary>
	    public const uint VK_KEY_A = 0x41;

	    /// <Summary> ('B')	B.</Summary>
	    public const uint VK_KEY_B = 0x42;

	    /// <Summary> ('C')	C.</Summary>
	    public const uint VK_KEY_C = 0x43;

	    /// <Summary> ('D')	D.</Summary>
	    public const uint VK_KEY_D = 0x44;

	    /// <Summary> ('E')	E.</Summary>
	    public const uint VK_KEY_E = 0x45;

	    /// <Summary> ('F')	F.</Summary>
	    public const uint VK_KEY_F = 0x46;

	    /// <Summary> ('G')	G.</Summary>
	    public const uint VK_KEY_G = 0x47;

	    /// <Summary> ('H')	H.</Summary>
	    public const uint VK_KEY_H = 0x48;

	    /// <Summary> ('I')	I.</Summary>
	    public const uint VK_KEY_I = 0x49;

	    /// <Summary> ('J')	J.</Summary>
	    public const uint VK_KEY_J = 0x4A;

	    /// <Summary> ('K')	K.</Summary>
	    public const uint VK_KEY_K = 0x4B;

	    /// <Summary> ('L')	L.</Summary>
	    public const uint VK_KEY_L = 0x4C;

	    /// <Summary> ('M')	M.</Summary>
	    public const uint VK_KEY_M = 0x4D;

	    /// <Summary> ('N')	N.</Summary>
	    public const uint VK_KEY_N = 0x4E;

	    /// <Summary> ('O')	O.</Summary>
	    public const uint VK_KEY_O = 0x4F;

	    /// <Summary> ('P')	P.</Summary>
	    public const uint VK_KEY_P = 0x50;

	    /// <Summary> ('Q')	Q.</Summary>
	    public const uint VK_KEY_Q = 0x51;

	    /// <Summary> ('R')	R.</Summary>
	    public const uint VK_KEY_R = 0x52;

	    /// <Summary> ('S')	S.</Summary>
	    public const uint VK_KEY_S = 0x53;

	    /// <Summary> ('T')	T.</Summary>
	    public const uint VK_KEY_T = 0x54;

	    /// <Summary> ('U')	U.</Summary>
	    public const uint VK_KEY_U = 0x55;

	    /// <Summary> ('V')	V.</Summary>
	    public const uint VK_KEY_V = 0x56;

	    /// <Summary> ('W')	W.</Summary>
	    public const uint VK_KEY_W = 0x57;

	    /// <Summary> ('X')	X.</Summary>
	    public const uint VK_KEY_X = 0x58;

	    /// <Summary> ('Y')	Y.</Summary>
	    public const uint VK_KEY_Y = 0x59;

	    /// <Summary> ('Z')	Z.</Summary>
	    public const uint VK_KEY_Z = 0x5A;

	    /// <Summary> Numpad *.</Summary>
	    public const uint VK_MULTIPLY = 0x6A;

	    /// <Summary> NoName.</Summary>
	    public const uint VK_NONAME = 0xFC;

	    /// <Summary> Numpad 0.</Summary>
	    public const uint VK_NUMPAD0 = 0x60;

	    /// <Summary> Numpad 1.</Summary>
	    public const uint VK_NUMPAD1 = 0x61;

	    /// <Summary> Numpad 2.</Summary>
	    public const uint VK_NUMPAD2 = 0x62;

	    /// <Summary> Numpad 3.</Summary>
	    public const uint VK_NUMPAD3 = 0x63;

	    /// <Summary> Numpad 4.</Summary>
	    public const uint VK_NUMPAD4 = 0x64;

	    /// <Summary> Numpad 5.</Summary>
	    public const uint VK_NUMPAD5 = 0x65;

	    /// <Summary> Numpad 6.</Summary>
	    public const uint VK_NUMPAD6 = 0x66;

	    /// <Summary> Numpad 7.</Summary>
	    public const uint VK_NUMPAD7 = 0x67;

	    /// <Summary> Numpad 8.</Summary>
	    public const uint VK_NUMPAD8 = 0x68;

	    /// <Summary> Numpad 9.</Summary>
	    public const uint VK_NUMPAD9 = 0x69;

	    /// <Summary> OEM_1 (: ;).</Summary>
	    public const uint VK_OEM_1 = 0xBA;

	    /// <Summary> OEM_102 (> <).</Summary>
	    public const uint VK_OEM_102 = 0xE2;

	    /// <Summary> OEM_2 (? /).</Summary>
	    public const uint VK_OEM_2 = 0xBF;

	    /// <Summary> OEM_3 (~ `).</Summary>
	    public const uint VK_OEM_3 = 0xC0;

	    /// <Summary> OEM_4 ({ [).</Summary>
	    public const uint VK_OEM_4 = 0xDB;

	    /// <Summary> OEM_5 (| \).</Summary>
	    public const uint VK_OEM_5 = 0xDC;

	    /// <Summary> OEM_6 (} ]).</Summary>
	    public const uint VK_OEM_6 = 0xDD;

	    /// <Summary> OEM_7 (" ').</Summary>
	    public const uint VK_OEM_7 = 0xDE;

	    /// <Summary> OEM_8 (§ !).</Summary>
	    public const uint VK_OEM_8 = 0xDF;

	    /// <Summary> Oem Attn.</Summary>
	    public const uint VK_OEM_ATTN = 0xF0;

	    /// <Summary> Auto.</Summary>
	    public const uint VK_OEM_AUTO = 0xF3;

	    /// <Summary> Ax.</Summary>
	    public const uint VK_OEM_AX = 0xE1;

	    /// <Summary> Back Tab.</Summary>
	    public const uint VK_OEM_BACKTAB = 0xF5;

	    /// <Summary> OemClr.</Summary>
	    public const uint VK_OEM_CLEAR = 0xFE;

	    /// <Summary> OEM_COMMA (< ,).</Summary>
	    public const uint VK_OEM_COMMA = 0xBC;

	    /// <Summary> Copy.</Summary>
	    public const uint VK_OEM_COPY = 0xF2;

	    /// <Summary> Cu Sel.</Summary>
	    public const uint VK_OEM_CUSEL = 0xEF;

	    /// <Summary> Enlw.</Summary>
	    public const uint VK_OEM_ENLW = 0xF4;

	    /// <Summary> Finish.</Summary>
	    public const uint VK_OEM_FINISH = 0xF1;

	    /// <Summary> Loya.</Summary>
	    public const uint VK_OEM_FJ_LOYA = 0x95;

	    /// <Summary> Mashu.</Summary>
	    public const uint VK_OEM_FJ_MASSHOU = 0x93;

	    /// <Summary> Roya.</Summary>
	    public const uint VK_OEM_FJ_ROYA = 0x96;

	    /// <Summary> Touroku.</Summary>
	    public const uint VK_OEM_FJ_TOUROKU = 0x94;

	    /// <Summary> Jump.</Summary>
	    public const uint VK_OEM_JUMP = 0xEA;

	    /// <Summary> OEM_MINUS (_ -).</Summary>
	    public const uint VK_OEM_MINUS = 0xBD;

	    /// <Summary> OemPa1.</Summary>
	    public const uint VK_OEM_PA1 = 0xEB;

	    /// <Summary> OemPa2.</Summary>
	    public const uint VK_OEM_PA2 = 0xEC;

	    /// <Summary> OemPa3.</Summary>
	    public const uint VK_OEM_PA3 = 0xED;

	    /// <Summary> OEM_PERIOD (> .).</Summary>
	    public const uint VK_OEM_PERIOD = 0xBE;

	    /// <Summary> OEM_PLUS (+ =).</Summary>
	    public const uint VK_OEM_PLUS = 0xBB;

	    /// <Summary> Reset.</Summary>
	    public const uint VK_OEM_RESET = 0xE9;

	    /// <Summary> WsCtrl.</Summary>
	    public const uint VK_OEM_WSCTRL = 0xEE;

	    /// <Summary> Pa1.</Summary>
	    public const uint VK_PA1 = 0xFD;

	    /// <Summary> Packet.</Summary>
	    public const uint VK_PACKET = 0xE7;

	    /// <Summary> Play.</Summary>
	    public const uint VK_PLAY = 0xFA;

	    /// <Summary> Process.</Summary>
	    public const uint VK_PROCESSKEY = 0xE5;

	    /// <Summary> Enter.</Summary>
	    public const uint VK_RETURN = 0x0D;

	    /// <Summary> Select.</Summary>
	    public const uint VK_SELECT = 0x29;

	    /// <Summary> Separator.</Summary>
	    public const uint VK_SEPARATOR = 0x6C;

	    /// <Summary> Space.</Summary>
	    public const uint VK_SPACE = 0x20;

	    /// <Summary> Num -.</Summary>
	    public const uint VK_SUBTRACT = 0x6D;

	    /// <Summary> Tab.</Summary>
	    public const uint VK_TAB = 0x09;

	    /// <Summary> Zoom.</Summary>
	    public const uint VK_ZOOM = 0xFB;

	    /// <Summary> no VK mapping.</Summary>
	    public const uint VK__none_ = 0xFF;

	    /// <Summary> Accept.</Summary>
	    public const uint VK_ACCEPT = 0x1E;

	    /// <Summary> Context Menu.</Summary>
	    public const uint VK_APPS = 0x5D;

	    /// <Summary> Browser Back.</Summary>
	    public const uint VK_BROWSER_BACK = 0xA6;

	    /// <Summary> Browser Favorites.</Summary>
	    public const uint VK_BROWSER_FAVORITES = 0xAB;

	    /// <Summary> Browser Forward.</Summary>
	    public const uint VK_BROWSER_FORWARD = 0xA7;

	    /// <Summary> Browser Home.</Summary>
	    public const uint VK_BROWSER_HOME = 0xAC;

	    /// <Summary> Browser Refresh.</Summary>
	    public const uint VK_BROWSER_REFRESH = 0xA8;

	    /// <Summary> Browser Search.</Summary>
	    public const uint VK_BROWSER_SEARCH = 0xAA;

	    /// <Summary> Browser Stop.</Summary>
	    public const uint VK_BROWSER_STOP = 0xA9;

	    /// <Summary> Caps Lock.</Summary>
	    public const uint VK_CAPITAL = 0x14;

	    /// <Summary> Convert.</Summary>
	    public const uint VK_CONVERT = 0x1C;

	    /// <Summary> Delete.</Summary>
	    public const uint VK_DELETE = 0x2E;

	    /// <Summary> Arrow Down.</Summary>
	    public const uint VK_DOWN = 0x28;

	    /// <Summary> End.</Summary>
	    public const uint VK_END = 0x23;

	    /// <Summary> F1.</Summary>
	    public const uint VK_F1 = 0x70;

	    /// <Summary> F10.</Summary>
	    public const uint VK_F10 = 0x79;

	    /// <Summary> F11.</Summary>
	    public const uint VK_F11 = 0x7A;

	    /// <Summary> F12.</Summary>
	    public const uint VK_F12 = 0x7B;

	    /// <Summary> F13.</Summary>
	    public const uint VK_F13 = 0x7C;

	    /// <Summary> F14.</Summary>
	    public const uint VK_F14 = 0x7D;

	    /// <Summary> F15.</Summary>
	    public const uint VK_F15 = 0x7E;

	    /// <Summary> F16.</Summary>
	    public const uint VK_F16 = 0x7F;

	    /// <Summary> F17.</Summary>
	    public const uint VK_F17 = 0x80;

	    /// <Summary> F18.</Summary>
	    public const uint VK_F18 = 0x81;

	    /// <Summary> F19.</Summary>
	    public const uint VK_F19 = 0x82;

	    /// <Summary> F2.</Summary>
	    public const uint VK_F2 = 0x71;

	    /// <Summary> F20.</Summary>
	    public const uint VK_F20 = 0x83;

	    /// <Summary> F21.</Summary>
	    public const uint VK_F21 = 0x84;

	    /// <Summary> F22.</Summary>
	    public const uint VK_F22 = 0x85;

	    /// <Summary> F23.</Summary>
	    public const uint VK_F23 = 0x86;

	    /// <Summary> F24.</Summary>
	    public const uint VK_F24 = 0x87;

	    /// <Summary> F3.</Summary>
	    public const uint VK_F3 = 0x72;

	    /// <Summary> F4.</Summary>
	    public const uint VK_F4 = 0x73;

	    /// <Summary> F5.</Summary>
	    public const uint VK_F5 = 0x74;

	    /// <Summary> F6.</Summary>
	    public const uint VK_F6 = 0x75;

	    /// <Summary> F7.</Summary>
	    public const uint VK_F7 = 0x76;

	    /// <Summary> F8.</Summary>
	    public const uint VK_F8 = 0x77;

	    /// <Summary> F9.</Summary>
	    public const uint VK_F9 = 0x78;

	    /// <Summary> Final.</Summary>
	    public const uint VK_FINAL = 0x18;

	    /// <Summary> Help.</Summary>
	    public const uint VK_HELP = 0x2F;

	    /// <Summary> Home.</Summary>
	    public const uint VK_HOME = 0x24;

	    /// <Summary> Ico00 *.</Summary>
	    public const uint VK_ICO_00 = 0xE4;

	    /// <Summary> Insert.</Summary>
	    public const uint VK_INSERT = 0x2D;

	    /// <Summary> Junja.</Summary>
	    public const uint VK_JUNJA = 0x17;

	    /// <Summary> Kana.</Summary>
	    public const uint VK_KANA = 0x15;

	    /// <Summary> Kanji.</Summary>
	    public const uint VK_KANJI = 0x19;

	    /// <Summary> App1.</Summary>
	    public const uint VK_LAUNCH_APP1 = 0xB6;

	    /// <Summary> App2.</Summary>
	    public const uint VK_LAUNCH_APP2 = 0xB7;

	    /// <Summary> Mail.</Summary>
	    public const uint VK_LAUNCH_MAIL = 0xB4;

	    /// <Summary> Media.</Summary>
	    public const uint VK_LAUNCH_MEDIA_SELECT = 0xB5;

	    /// <Summary> Left Button **.</Summary>
	    public const uint VK_LBUTTON = 0x01;

	    /// <Summary> Left Ctrl.</Summary>
	    public const uint VK_LCONTROL = 0xA2;

	    /// <Summary> Arrow Left.</Summary>
	    public const uint VK_LEFT = 0x25;

	    /// <Summary> Left Alt.</Summary>
	    public const uint VK_LMENU = 0xA4;

	    /// <Summary> Left Shift.</Summary>
	    public const uint VK_LSHIFT = 0xA0;

	    /// <Summary> Left Win.</Summary>
	    public const uint VK_LWIN = 0x5B;

	    /// <Summary> Middle Button **.</Summary>
	    public const uint VK_MBUTTON = 0x04;

	    /// <Summary> Next Track.</Summary>
	    public const uint VK_MEDIA_NEXT_TRACK = 0xB0;

	    /// <Summary> Play / Pause.</Summary>
	    public const uint VK_MEDIA_PLAY_PAUSE = 0xB3;

	    /// <Summary> Previous Track.</Summary>
	    public const uint VK_MEDIA_PREV_TRACK = 0xB1;

	    /// <Summary> Stop.</Summary>
	    public const uint VK_MEDIA_STOP = 0xB2;

	    /// <Summary> Mode Change.</Summary>
	    public const uint VK_MODECHANGE = 0x1F;

	    /// <Summary> Page Down.</Summary>
	    public const uint VK_NEXT = 0x22;

	    /// <Summary> Non Convert.</Summary>
	    public const uint VK_NONCONVERT = 0x1D;

	    /// <Summary> Num Lock.</Summary>
	    public const uint VK_NUMLOCK = 0x90;

	    /// <Summary> Jisho.</Summary>
	    public const uint VK_OEM_FJ_JISHO = 0x92;

	    /// <Summary> Pause.</Summary>
	    public const uint VK_PAUSE = 0x13;

	    /// <Summary> Print.</Summary>
	    public const uint VK_PRINT = 0x2A;

	    /// <Summary> Page Up.</Summary>
	    public const uint VK_PRIOR = 0x21;

	    /// <Summary> Right Button **.</Summary>
	    public const uint VK_RBUTTON = 0x02;

	    /// <Summary> Right Ctrl.</Summary>
	    public const uint VK_RCONTROL = 0xA3;

	    /// <Summary> Arrow Right.</Summary>
	    public const uint VK_RIGHT = 0x27;

	    /// <Summary> Right Alt.</Summary>
	    public const uint VK_RMENU = 0xA5;

	    /// <Summary> Right Shift.</Summary>
	    public const uint VK_RSHIFT = 0xA1;

	    /// <Summary> Right Win.</Summary>
	    public const uint VK_RWIN = 0x5C;

	    /// <Summary> Scrol Lock.</Summary>
	    public const uint VK_SCROLL = 0x91;

	    /// <Summary> Sleep.</Summary>
	    public const uint VK_SLEEP = 0x5F;

	    /// <Summary> Print Screen.</Summary>
	    public const uint VK_SNAPSHOT = 0x2C;

	    /// <Summary> Arrow Up.</Summary>
	    public const uint VK_UP = 0x26;

	    /// <Summary> Volume Down.</Summary>
	    public const uint VK_VOLUME_DOWN = 0xAE;

	    /// <Summary> Volume Mute.</Summary>
	    public const uint VK_VOLUME_MUTE = 0xAD;

	    /// <Summary> Volume Up.</Summary>
	    public const uint VK_VOLUME_UP = 0xAF;

	    /// <Summary> X Button 1 **.</Summary>
	    public const uint VK_XBUTTON1 = 0x05;

	    /// <Summary> X Button 2 **.</Summary>
	    public const uint VK_XBUTTON2 = 0x06;

    }
}
