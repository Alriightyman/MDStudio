﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDStudioPlus.Targets
{
    public class SDL_Keycode
    {
        private const int SDLK_SCANCODE_MASK = (1 << 30);

        private enum Scancode : int
        {
            SDL_SCANCODE_UNKNOWN = 0,

            SDL_SCANCODE_A = 4,
            SDL_SCANCODE_B = 5,
            SDL_SCANCODE_C = 6,
            SDL_SCANCODE_D = 7,
            SDL_SCANCODE_E = 8,
            SDL_SCANCODE_F = 9,
            SDL_SCANCODE_G = 10,
            SDL_SCANCODE_H = 11,
            SDL_SCANCODE_I = 12,
            SDL_SCANCODE_J = 13,
            SDL_SCANCODE_K = 14,
            SDL_SCANCODE_L = 15,
            SDL_SCANCODE_M = 16,
            SDL_SCANCODE_N = 17,
            SDL_SCANCODE_O = 18,
            SDL_SCANCODE_P = 19,
            SDL_SCANCODE_Q = 20,
            SDL_SCANCODE_R = 21,
            SDL_SCANCODE_S = 22,
            SDL_SCANCODE_T = 23,
            SDL_SCANCODE_U = 24,
            SDL_SCANCODE_V = 25,
            SDL_SCANCODE_W = 26,
            SDL_SCANCODE_X = 27,
            SDL_SCANCODE_Y = 28,
            SDL_SCANCODE_Z = 29,

            SDL_SCANCODE_1 = 30,
            SDL_SCANCODE_2 = 31,
            SDL_SCANCODE_3 = 32,
            SDL_SCANCODE_4 = 33,
            SDL_SCANCODE_5 = 34,
            SDL_SCANCODE_6 = 35,
            SDL_SCANCODE_7 = 36,
            SDL_SCANCODE_8 = 37,
            SDL_SCANCODE_9 = 38,
            SDL_SCANCODE_0 = 39,
            SDL_SCANCODE_RETURN = 40,
            SDL_SCANCODE_ESCAPE = 41,
            SDL_SCANCODE_BACKSPACE = 42,
            SDL_SCANCODE_TAB = 43,
            SDL_SCANCODE_SPACE = 44,
            SDL_SCANCODE_MINUS = 45,
            SDL_SCANCODE_EQUALS = 46,
            SDL_SCANCODE_LEFTBRACKET = 47,
            SDL_SCANCODE_RIGHTBRACKET = 48,
            SDL_SCANCODE_BACKSLASH = 49,
            SDL_SCANCODE_NONUSHASH = 50,
            SDL_SCANCODE_SEMICOLON = 51,
            SDL_SCANCODE_APOSTROPHE = 52,
            SDL_SCANCODE_GRAVE = 53,
            SDL_SCANCODE_COMMA = 54,
            SDL_SCANCODE_PERIOD = 55,
            SDL_SCANCODE_SLASH = 56,
            SDL_SCANCODE_CAPSLOCK = 57,
            SDL_SCANCODE_F1 = 58,
            SDL_SCANCODE_F2 = 59,
            SDL_SCANCODE_F3 = 60,
            SDL_SCANCODE_F4 = 61,
            SDL_SCANCODE_F5 = 62,
            SDL_SCANCODE_F6 = 63,
            SDL_SCANCODE_F7 = 64,
            SDL_SCANCODE_F8 = 65,
            SDL_SCANCODE_F9 = 66,
            SDL_SCANCODE_F10 = 67,
            SDL_SCANCODE_F11 = 68,
            SDL_SCANCODE_F12 = 69,

            SDL_SCANCODE_PRINTSCREEN = 70,
            SDL_SCANCODE_SCROLLLOCK = 71,
            SDL_SCANCODE_PAUSE = 72,
            SDL_SCANCODE_INSERT = 73,
            SDL_SCANCODE_HOME = 74,
            SDL_SCANCODE_PAGEUP = 75,
            SDL_SCANCODE_DELETE = 76,
            SDL_SCANCODE_END = 77,
            SDL_SCANCODE_PAGEDOWN = 78,
            SDL_SCANCODE_RIGHT = 79,
            SDL_SCANCODE_LEFT = 80,
            SDL_SCANCODE_DOWN = 81,
            SDL_SCANCODE_UP = 82,

            SDL_SCANCODE_NUMLOCKCLEAR = 83,
            SDL_SCANCODE_KP_DIVIDE = 84,
            SDL_SCANCODE_KP_MULTIPLY = 85,
            SDL_SCANCODE_KP_MINUS = 86,
            SDL_SCANCODE_KP_PLUS = 87,
            SDL_SCANCODE_KP_ENTER = 88,
            SDL_SCANCODE_KP_1 = 89,
            SDL_SCANCODE_KP_2 = 90,
            SDL_SCANCODE_KP_3 = 91,
            SDL_SCANCODE_KP_4 = 92,
            SDL_SCANCODE_KP_5 = 93,
            SDL_SCANCODE_KP_6 = 94,
            SDL_SCANCODE_KP_7 = 95,
            SDL_SCANCODE_KP_8 = 96,
            SDL_SCANCODE_KP_9 = 97,
            SDL_SCANCODE_KP_0 = 98,
            SDL_SCANCODE_KP_PERIOD = 99,

            SDL_SCANCODE_NONUSBACKSLASH = 100,
            SDL_SCANCODE_APPLICATION = 101,
            SDL_SCANCODE_POWER = 102,
            SDL_SCANCODE_KP_EQUALS = 103,
            SDL_SCANCODE_F13 = 104,
            SDL_SCANCODE_F14 = 105,
            SDL_SCANCODE_F15 = 106,
            SDL_SCANCODE_F16 = 107,
            SDL_SCANCODE_F17 = 108,
            SDL_SCANCODE_F18 = 109,
            SDL_SCANCODE_F19 = 110,
            SDL_SCANCODE_F20 = 111,
            SDL_SCANCODE_F21 = 112,
            SDL_SCANCODE_F22 = 113,
            SDL_SCANCODE_F23 = 114,
            SDL_SCANCODE_F24 = 115,
            SDL_SCANCODE_EXECUTE = 116,
            SDL_SCANCODE_HELP = 117,
            SDL_SCANCODE_MENU = 118,
            SDL_SCANCODE_SELECT = 119,
            SDL_SCANCODE_STOP = 120,
            SDL_SCANCODE_AGAIN = 121,
            SDL_SCANCODE_UNDO = 122,
            SDL_SCANCODE_CUT = 123,
            SDL_SCANCODE_COPY = 124,
            SDL_SCANCODE_PASTE = 125,
            SDL_SCANCODE_FIND = 126,
            SDL_SCANCODE_MUTE = 127,
            SDL_SCANCODE_VOLUMEUP = 128,
            SDL_SCANCODE_VOLUMEDOWN = 129,
            SDL_SCANCODE_KP_COMMA = 133,
            SDL_SCANCODE_KP_EQUALSAS400 = 134,

            SDL_SCANCODE_INTERNATIONAL1 = 135,
            SDL_SCANCODE_INTERNATIONAL2 = 136,
            SDL_SCANCODE_INTERNATIONAL3 = 137,
            SDL_SCANCODE_INTERNATIONAL4 = 138,
            SDL_SCANCODE_INTERNATIONAL5 = 139,
            SDL_SCANCODE_INTERNATIONAL6 = 140,
            SDL_SCANCODE_INTERNATIONAL7 = 141,
            SDL_SCANCODE_INTERNATIONAL8 = 142,
            SDL_SCANCODE_INTERNATIONAL9 = 143,
            SDL_SCANCODE_LANG1 = 144, /**< Hangul/English toggle */
            SDL_SCANCODE_LANG2 = 145, /**< Hanja conversion */
            SDL_SCANCODE_LANG3 = 146, /**< Katakana */
            SDL_SCANCODE_LANG4 = 147, /**< Hiragana */
            SDL_SCANCODE_LANG5 = 148, /**< Zenkaku/Hankaku */
            SDL_SCANCODE_LANG6 = 149, /**< reserved */
            SDL_SCANCODE_LANG7 = 150, /**< reserved */
            SDL_SCANCODE_LANG8 = 151, /**< reserved */
            SDL_SCANCODE_LANG9 = 152, /**< reserved */

            SDL_SCANCODE_ALTERASE = 153, /**< Erase-Eaze */
            SDL_SCANCODE_SYSREQ = 154,
            SDL_SCANCODE_CANCEL = 155,
            SDL_SCANCODE_CLEAR = 156,
            SDL_SCANCODE_PRIOR = 157,
            SDL_SCANCODE_RETURN2 = 158,
            SDL_SCANCODE_SEPARATOR = 159,
            SDL_SCANCODE_OUT = 160,
            SDL_SCANCODE_OPER = 161,
            SDL_SCANCODE_CLEARAGAIN = 162,
            SDL_SCANCODE_CRSEL = 163,
            SDL_SCANCODE_EXSEL = 164,

            SDL_SCANCODE_KP_00 = 176,
            SDL_SCANCODE_KP_000 = 177,
            SDL_SCANCODE_THOUSANDSSEPARATOR = 178,
            SDL_SCANCODE_DECIMALSEPARATOR = 179,
            SDL_SCANCODE_CURRENCYUNIT = 180,
            SDL_SCANCODE_CURRENCYSUBUNIT = 181,
            SDL_SCANCODE_KP_LEFTPAREN = 182,
            SDL_SCANCODE_KP_RIGHTPAREN = 183,
            SDL_SCANCODE_KP_LEFTBRACE = 184,
            SDL_SCANCODE_KP_RIGHTBRACE = 185,
            SDL_SCANCODE_KP_TAB = 186,
            SDL_SCANCODE_KP_BACKSPACE = 187,
            SDL_SCANCODE_KP_A = 188,
            SDL_SCANCODE_KP_B = 189,
            SDL_SCANCODE_KP_C = 190,
            SDL_SCANCODE_KP_D = 191,
            SDL_SCANCODE_KP_E = 192,
            SDL_SCANCODE_KP_F = 193,
            SDL_SCANCODE_KP_XOR = 194,
            SDL_SCANCODE_KP_POWER = 195,
            SDL_SCANCODE_KP_PERCENT = 196,
            SDL_SCANCODE_KP_LESS = 197,
            SDL_SCANCODE_KP_GREATER = 198,
            SDL_SCANCODE_KP_AMPERSAND = 199,
            SDL_SCANCODE_KP_DBLAMPERSAND = 200,
            SDL_SCANCODE_KP_VERTICALBAR = 201,
            SDL_SCANCODE_KP_DBLVERTICALBAR = 202,
            SDL_SCANCODE_KP_COLON = 203,
            SDL_SCANCODE_KP_HASH = 204,
            SDL_SCANCODE_KP_SPACE = 205,
            SDL_SCANCODE_KP_AT = 206,
            SDL_SCANCODE_KP_EXCLAM = 207,
            SDL_SCANCODE_KP_MEMSTORE = 208,
            SDL_SCANCODE_KP_MEMRECALL = 209,
            SDL_SCANCODE_KP_MEMCLEAR = 210,
            SDL_SCANCODE_KP_MEMADD = 211,
            SDL_SCANCODE_KP_MEMSUBTRACT = 212,
            SDL_SCANCODE_KP_MEMMULTIPLY = 213,
            SDL_SCANCODE_KP_MEMDIVIDE = 214,
            SDL_SCANCODE_KP_PLUSMINUS = 215,
            SDL_SCANCODE_KP_CLEAR = 216,
            SDL_SCANCODE_KP_CLEARENTRY = 217,
            SDL_SCANCODE_KP_BINARY = 218,
            SDL_SCANCODE_KP_OCTAL = 219,
            SDL_SCANCODE_KP_DECIMAL = 220,
            SDL_SCANCODE_KP_HEXADECIMAL = 221,

            SDL_SCANCODE_LCTRL = 224,
            SDL_SCANCODE_LSHIFT = 225,
            SDL_SCANCODE_LALT = 226, /**< alt, option */
            SDL_SCANCODE_LGUI = 227, /**< windows, command (apple), meta */
            SDL_SCANCODE_RCTRL = 228,
            SDL_SCANCODE_RSHIFT = 229,
            SDL_SCANCODE_RALT = 230, /**< alt gr, option */
            SDL_SCANCODE_RGUI = 231, /**< windows, command (apple), meta */

            SDL_SCANCODE_MODE = 257,
            SDL_SCANCODE_AUDIONEXT = 258,
            SDL_SCANCODE_AUDIOPREV = 259,
            SDL_SCANCODE_AUDIOSTOP = 260,
            SDL_SCANCODE_AUDIOPLAY = 261,
            SDL_SCANCODE_AUDIOMUTE = 262,
            SDL_SCANCODE_MEDIASELECT = 263,
            SDL_SCANCODE_WWW = 264,
            SDL_SCANCODE_MAIL = 265,
            SDL_SCANCODE_CALCULATOR = 266,
            SDL_SCANCODE_COMPUTER = 267,
            SDL_SCANCODE_AC_SEARCH = 268,
            SDL_SCANCODE_AC_HOME = 269,
            SDL_SCANCODE_AC_BACK = 270,
            SDL_SCANCODE_AC_FORWARD = 271,
            SDL_SCANCODE_AC_STOP = 272,
            SDL_SCANCODE_AC_REFRESH = 273,
            SDL_SCANCODE_AC_BOOKMARKS = 274,

            SDL_SCANCODE_BRIGHTNESSDOWN = 275,
            SDL_SCANCODE_BRIGHTNESSUP = 276,
            SDL_SCANCODE_DISPLAYSWITCH = 277,
            SDL_SCANCODE_KBDILLUMTOGGLE = 278,
            SDL_SCANCODE_KBDILLUMDOWN = 279,
            SDL_SCANCODE_KBDILLUMUP = 280,
            SDL_SCANCODE_EJECT = 281,
            SDL_SCANCODE_SLEEP = 282,

            SDL_SCANCODE_APP1 = 283,
            SDL_SCANCODE_APP2 = 284,
            SDL_NUM_SCANCODES = 512
        }

        public enum Keycode : int
        {
            SDLK_UNKNOWN = 0,

            SDLK_RETURN = (byte)'\r',
            SDLK_ESCAPE = (byte)'\x1b',
            SDLK_BACKSPACE = (byte)'\b',
            SDLK_TAB = (byte)'\t',
            SDLK_SPACE = (byte)' ',
            SDLK_EXCLAIM = (byte)'!',
            SDLK_QUOTEDBL = (byte)'"',
            SDLK_HASH = (byte)'#',
            SDLK_PERCENT = (byte)'%',
            SDLK_DOLLAR = (byte)'$',
            SDLK_AMPERSAND = (byte)'&',
            SDLK_QUOTE = (byte)'\'',
            SDLK_LEFTPAREN = (byte)'(',
            SDLK_RIGHTPAREN = (byte)')',
            SDLK_ASTERISK = (byte)'*',
            SDLK_PLUS = (byte)'+',
            SDLK_COMMA = (byte)',',
            SDLK_MINUS = (byte)'-',
            SDLK_PERIOD = (byte)'.',
            SDLK_SLASH = (byte)'/',
            SDLK_0 = (byte)'0',
            SDLK_1 = (byte)'1',
            SDLK_2 = (byte)'2',
            SDLK_3 = (byte)'3',
            SDLK_4 = (byte)'4',
            SDLK_5 = (byte)'5',
            SDLK_6 = (byte)'6',
            SDLK_7 = (byte)'7',
            SDLK_8 = (byte)'8',
            SDLK_9 = (byte)'9',
            SDLK_COLON = (byte)':',
            SDLK_SEMICOLON = (byte)';',
            SDLK_LESS = (byte)'<',
            SDLK_EQUALS = (byte)'=',
            SDLK_GREATER = (byte)'>',
            SDLK_QUESTION = (byte)'?',
            SDLK_AT = (byte)'@',
            /*
               Skip uppercase letters
             */
            SDLK_LEFTBRACKET = (byte)'[',
            SDLK_BACKSLASH = (byte)'\\',
            SDLK_RIGHTBRACKET = (byte)']',
            SDLK_CARET = (byte)'^',
            SDLK_UNDERSCORE = (byte)'_',
            SDLK_BACKQUOTE = (byte)'`',
            SDLK_a = (byte)'a',
            SDLK_b = (byte)'b',
            SDLK_c = (byte)'c',
            SDLK_d = (byte)'d',
            SDLK_e = (byte)'e',
            SDLK_f = (byte)'f',
            SDLK_g = (byte)'g',
            SDLK_h = (byte)'h',
            SDLK_i = (byte)'i',
            SDLK_j = (byte)'j',
            SDLK_k = (byte)'k',
            SDLK_l = (byte)'l',
            SDLK_m = (byte)'m',
            SDLK_n = (byte)'n',
            SDLK_o = (byte)'o',
            SDLK_p = (byte)'p',
            SDLK_q = (byte)'q',
            SDLK_r = (byte)'r',
            SDLK_s = (byte)'s',
            SDLK_t = (byte)'t',
            SDLK_u = (byte)'u',
            SDLK_v = (byte)'v',
            SDLK_w = (byte)'w',
            SDLK_x = (byte)'x',
            SDLK_y = (byte)'y',
            SDLK_z = (byte)'z',

            SDLK_CAPSLOCK = Scancode.SDL_SCANCODE_CAPSLOCK | SDLK_SCANCODE_MASK,

            SDLK_F1 = Scancode.SDL_SCANCODE_F1 | SDLK_SCANCODE_MASK,
            SDLK_F2 = Scancode.SDL_SCANCODE_F2 | SDLK_SCANCODE_MASK,
            SDLK_F3 = Scancode.SDL_SCANCODE_F3 | SDLK_SCANCODE_MASK,
            SDLK_F4 = Scancode.SDL_SCANCODE_F4 | SDLK_SCANCODE_MASK,
            SDLK_F5 = Scancode.SDL_SCANCODE_F5 | SDLK_SCANCODE_MASK,
            SDLK_F6 = Scancode.SDL_SCANCODE_F6 | SDLK_SCANCODE_MASK,
            SDLK_F7 = Scancode.SDL_SCANCODE_F7 | SDLK_SCANCODE_MASK,
            SDLK_F8 = Scancode.SDL_SCANCODE_F8 | SDLK_SCANCODE_MASK,
            SDLK_F9 = Scancode.SDL_SCANCODE_F9 | SDLK_SCANCODE_MASK,
            SDLK_F10 = Scancode.SDL_SCANCODE_F10 | SDLK_SCANCODE_MASK,
            SDLK_F11 = Scancode.SDL_SCANCODE_F11 | SDLK_SCANCODE_MASK,
            SDLK_F12 = Scancode.SDL_SCANCODE_F12 | SDLK_SCANCODE_MASK,

            SDLK_PRINTSCREEN = Scancode.SDL_SCANCODE_PRINTSCREEN | SDLK_SCANCODE_MASK,
            SDLK_SCROLLLOCK = Scancode.SDL_SCANCODE_SCROLLLOCK | SDLK_SCANCODE_MASK,
            SDLK_PAUSE = Scancode.SDL_SCANCODE_PAUSE | SDLK_SCANCODE_MASK,
            SDLK_INSERT = Scancode.SDL_SCANCODE_INSERT | SDLK_SCANCODE_MASK,
            SDLK_HOME = Scancode.SDL_SCANCODE_HOME | SDLK_SCANCODE_MASK,
            SDLK_PAGEUP = Scancode.SDL_SCANCODE_PAGEUP | SDLK_SCANCODE_MASK,
            SDLK_DELETE = '\x7f',
            SDLK_END = Scancode.SDL_SCANCODE_END | SDLK_SCANCODE_MASK,
            SDLK_PAGEDOWN = Scancode.SDL_SCANCODE_PAGEDOWN | SDLK_SCANCODE_MASK,
            SDLK_RIGHT = Scancode.SDL_SCANCODE_RIGHT | SDLK_SCANCODE_MASK,
            SDLK_LEFT = Scancode.SDL_SCANCODE_LEFT | SDLK_SCANCODE_MASK,
            SDLK_DOWN = Scancode.SDL_SCANCODE_DOWN | SDLK_SCANCODE_MASK,
            SDLK_UP = Scancode.SDL_SCANCODE_UP | SDLK_SCANCODE_MASK,

            SDLK_NUMLOCKCLEAR = Scancode.SDL_SCANCODE_NUMLOCKCLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_DIVIDE = Scancode.SDL_SCANCODE_KP_DIVIDE | SDLK_SCANCODE_MASK,
            SDLK_KP_MULTIPLY = Scancode.SDL_SCANCODE_KP_MULTIPLY | SDLK_SCANCODE_MASK,
            SDLK_KP_MINUS = Scancode.SDL_SCANCODE_KP_MINUS | SDLK_SCANCODE_MASK,
            SDLK_KP_PLUS = Scancode.SDL_SCANCODE_KP_PLUS | SDLK_SCANCODE_MASK,
            SDLK_KP_ENTER = Scancode.SDL_SCANCODE_KP_ENTER | SDLK_SCANCODE_MASK,
            SDLK_KP_1 = Scancode.SDL_SCANCODE_KP_1 | SDLK_SCANCODE_MASK,
            SDLK_KP_2 = Scancode.SDL_SCANCODE_KP_2 | SDLK_SCANCODE_MASK,
            SDLK_KP_3 = Scancode.SDL_SCANCODE_KP_3 | SDLK_SCANCODE_MASK,
            SDLK_KP_4 = Scancode.SDL_SCANCODE_KP_4 | SDLK_SCANCODE_MASK,
            SDLK_KP_5 = Scancode.SDL_SCANCODE_KP_5 | SDLK_SCANCODE_MASK,
            SDLK_KP_6 = Scancode.SDL_SCANCODE_KP_6 | SDLK_SCANCODE_MASK,
            SDLK_KP_7 = Scancode.SDL_SCANCODE_KP_7 | SDLK_SCANCODE_MASK,
            SDLK_KP_8 = Scancode.SDL_SCANCODE_KP_8 | SDLK_SCANCODE_MASK,
            SDLK_KP_9 = Scancode.SDL_SCANCODE_KP_9 | SDLK_SCANCODE_MASK,
            SDLK_KP_0 = Scancode.SDL_SCANCODE_KP_0 | SDLK_SCANCODE_MASK,
            SDLK_KP_PERIOD = Scancode.SDL_SCANCODE_KP_PERIOD | SDLK_SCANCODE_MASK,

            SDLK_APPLICATION = Scancode.SDL_SCANCODE_APPLICATION | SDLK_SCANCODE_MASK,
            SDLK_POWER = Scancode.SDL_SCANCODE_POWER | SDLK_SCANCODE_MASK,
            SDLK_KP_EQUALS = Scancode.SDL_SCANCODE_KP_EQUALS | SDLK_SCANCODE_MASK,
            SDLK_F13 = Scancode.SDL_SCANCODE_F13 | SDLK_SCANCODE_MASK,
            SDLK_F14 = Scancode.SDL_SCANCODE_F14 | SDLK_SCANCODE_MASK,
            SDLK_F15 = Scancode.SDL_SCANCODE_F15 | SDLK_SCANCODE_MASK,
            SDLK_F16 = Scancode.SDL_SCANCODE_F16 | SDLK_SCANCODE_MASK,
            SDLK_F17 = Scancode.SDL_SCANCODE_F17 | SDLK_SCANCODE_MASK,
            SDLK_F18 = Scancode.SDL_SCANCODE_F18 | SDLK_SCANCODE_MASK,
            SDLK_F19 = Scancode.SDL_SCANCODE_F19 | SDLK_SCANCODE_MASK,
            SDLK_F20 = Scancode.SDL_SCANCODE_F20 | SDLK_SCANCODE_MASK,
            SDLK_F21 = Scancode.SDL_SCANCODE_F21 | SDLK_SCANCODE_MASK,
            SDLK_F22 = Scancode.SDL_SCANCODE_F22 | SDLK_SCANCODE_MASK,
            SDLK_F23 = Scancode.SDL_SCANCODE_F23 | SDLK_SCANCODE_MASK,
            SDLK_F24 = Scancode.SDL_SCANCODE_F24 | SDLK_SCANCODE_MASK,
            SDLK_EXECUTE = Scancode.SDL_SCANCODE_EXECUTE | SDLK_SCANCODE_MASK,
            SDLK_HELP = Scancode.SDL_SCANCODE_HELP | SDLK_SCANCODE_MASK,
            SDLK_MENU = Scancode.SDL_SCANCODE_MENU | SDLK_SCANCODE_MASK,
            SDLK_SELECT = Scancode.SDL_SCANCODE_SELECT | SDLK_SCANCODE_MASK,
            SDLK_STOP = Scancode.SDL_SCANCODE_STOP | SDLK_SCANCODE_MASK,
            SDLK_AGAIN = Scancode.SDL_SCANCODE_AGAIN | SDLK_SCANCODE_MASK,
            SDLK_UNDO = Scancode.SDL_SCANCODE_UNDO | SDLK_SCANCODE_MASK,
            SDLK_CUT = Scancode.SDL_SCANCODE_CUT | SDLK_SCANCODE_MASK,
            SDLK_COPY = Scancode.SDL_SCANCODE_COPY | SDLK_SCANCODE_MASK,
            SDLK_PASTE = Scancode.SDL_SCANCODE_PASTE | SDLK_SCANCODE_MASK,
            SDLK_FIND = Scancode.SDL_SCANCODE_FIND | SDLK_SCANCODE_MASK,
            SDLK_MUTE = Scancode.SDL_SCANCODE_MUTE | SDLK_SCANCODE_MASK,
            SDLK_VOLUMEUP = Scancode.SDL_SCANCODE_VOLUMEUP | SDLK_SCANCODE_MASK,
            SDLK_VOLUMEDOWN = Scancode.SDL_SCANCODE_VOLUMEDOWN | SDLK_SCANCODE_MASK,
            SDLK_KP_COMMA = Scancode.SDL_SCANCODE_KP_COMMA | SDLK_SCANCODE_MASK,
            SDLK_KP_EQUALSAS400 =
                Scancode.SDL_SCANCODE_KP_EQUALSAS400 | SDLK_SCANCODE_MASK,

            SDLK_ALTERASE = Scancode.SDL_SCANCODE_ALTERASE | SDLK_SCANCODE_MASK,
            SDLK_SYSREQ = Scancode.SDL_SCANCODE_SYSREQ | SDLK_SCANCODE_MASK,
            SDLK_CANCEL = Scancode.SDL_SCANCODE_CANCEL | SDLK_SCANCODE_MASK,
            SDLK_CLEAR = Scancode.SDL_SCANCODE_CLEAR | SDLK_SCANCODE_MASK,
            SDLK_PRIOR = Scancode.SDL_SCANCODE_PRIOR | SDLK_SCANCODE_MASK,
            SDLK_RETURN2 = Scancode.SDL_SCANCODE_RETURN2 | SDLK_SCANCODE_MASK,
            SDLK_SEPARATOR = Scancode.SDL_SCANCODE_SEPARATOR | SDLK_SCANCODE_MASK,
            SDLK_OUT = Scancode.SDL_SCANCODE_OUT | SDLK_SCANCODE_MASK,
            SDLK_OPER = Scancode.SDL_SCANCODE_OPER | SDLK_SCANCODE_MASK,
            SDLK_CLEARAGAIN = Scancode.SDL_SCANCODE_CLEARAGAIN | SDLK_SCANCODE_MASK,
            SDLK_CRSEL = Scancode.SDL_SCANCODE_CRSEL | SDLK_SCANCODE_MASK,
            SDLK_EXSEL = Scancode.SDL_SCANCODE_EXSEL | SDLK_SCANCODE_MASK,

            SDLK_KP_00 = Scancode.SDL_SCANCODE_KP_00 | SDLK_SCANCODE_MASK,
            SDLK_KP_000 = Scancode.SDL_SCANCODE_KP_000 | SDLK_SCANCODE_MASK,
            SDLK_THOUSANDSSEPARATOR =
                Scancode.SDL_SCANCODE_THOUSANDSSEPARATOR | SDLK_SCANCODE_MASK,
            SDLK_DECIMALSEPARATOR =
                Scancode.SDL_SCANCODE_DECIMALSEPARATOR | SDLK_SCANCODE_MASK,
            SDLK_CURRENCYUNIT = Scancode.SDL_SCANCODE_CURRENCYUNIT | SDLK_SCANCODE_MASK,
            SDLK_CURRENCYSUBUNIT =
                Scancode.SDL_SCANCODE_CURRENCYSUBUNIT | SDLK_SCANCODE_MASK,
            SDLK_KP_LEFTPAREN = Scancode.SDL_SCANCODE_KP_LEFTPAREN | SDLK_SCANCODE_MASK,
            SDLK_KP_RIGHTPAREN = Scancode.SDL_SCANCODE_KP_RIGHTPAREN | SDLK_SCANCODE_MASK,
            SDLK_KP_LEFTBRACE = Scancode.SDL_SCANCODE_KP_LEFTBRACE | SDLK_SCANCODE_MASK,
            SDLK_KP_RIGHTBRACE = Scancode.SDL_SCANCODE_KP_RIGHTBRACE | SDLK_SCANCODE_MASK,
            SDLK_KP_TAB = Scancode.SDL_SCANCODE_KP_TAB | SDLK_SCANCODE_MASK,
            SDLK_KP_BACKSPACE = Scancode.SDL_SCANCODE_KP_BACKSPACE | SDLK_SCANCODE_MASK,
            SDLK_KP_A = Scancode.SDL_SCANCODE_KP_A | SDLK_SCANCODE_MASK,
            SDLK_KP_B = Scancode.SDL_SCANCODE_KP_B | SDLK_SCANCODE_MASK,
            SDLK_KP_C = Scancode.SDL_SCANCODE_KP_C | SDLK_SCANCODE_MASK,
            SDLK_KP_D = Scancode.SDL_SCANCODE_KP_D | SDLK_SCANCODE_MASK,
            SDLK_KP_E = Scancode.SDL_SCANCODE_KP_E | SDLK_SCANCODE_MASK,
            SDLK_KP_F = Scancode.SDL_SCANCODE_KP_F | SDLK_SCANCODE_MASK,
            SDLK_KP_XOR = Scancode.SDL_SCANCODE_KP_XOR | SDLK_SCANCODE_MASK,
            SDLK_KP_POWER = Scancode.SDL_SCANCODE_KP_POWER | SDLK_SCANCODE_MASK,
            SDLK_KP_PERCENT = Scancode.SDL_SCANCODE_KP_PERCENT | SDLK_SCANCODE_MASK,
            SDLK_KP_LESS = Scancode.SDL_SCANCODE_KP_LESS | SDLK_SCANCODE_MASK,
            SDLK_KP_GREATER = Scancode.SDL_SCANCODE_KP_GREATER | SDLK_SCANCODE_MASK,
            SDLK_KP_AMPERSAND = Scancode.SDL_SCANCODE_KP_AMPERSAND | SDLK_SCANCODE_MASK,
            SDLK_KP_DBLAMPERSAND =
                Scancode.SDL_SCANCODE_KP_DBLAMPERSAND | SDLK_SCANCODE_MASK,
            SDLK_KP_VERTICALBAR =
                Scancode.SDL_SCANCODE_KP_VERTICALBAR | SDLK_SCANCODE_MASK,
            SDLK_KP_DBLVERTICALBAR =
                Scancode.SDL_SCANCODE_KP_DBLVERTICALBAR | SDLK_SCANCODE_MASK,
            SDLK_KP_COLON = Scancode.SDL_SCANCODE_KP_COLON | SDLK_SCANCODE_MASK,
            SDLK_KP_HASH = Scancode.SDL_SCANCODE_KP_HASH | SDLK_SCANCODE_MASK,
            SDLK_KP_SPACE = Scancode.SDL_SCANCODE_KP_SPACE | SDLK_SCANCODE_MASK,
            SDLK_KP_AT = Scancode.SDL_SCANCODE_KP_AT | SDLK_SCANCODE_MASK,
            SDLK_KP_EXCLAM = Scancode.SDL_SCANCODE_KP_EXCLAM | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMSTORE = Scancode.SDL_SCANCODE_KP_MEMSTORE | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMRECALL = Scancode.SDL_SCANCODE_KP_MEMRECALL | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMCLEAR = Scancode.SDL_SCANCODE_KP_MEMCLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMADD = Scancode.SDL_SCANCODE_KP_MEMADD | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMSUBTRACT =
                Scancode.SDL_SCANCODE_KP_MEMSUBTRACT | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMMULTIPLY =
                Scancode.SDL_SCANCODE_KP_MEMMULTIPLY | SDLK_SCANCODE_MASK,
            SDLK_KP_MEMDIVIDE = Scancode.SDL_SCANCODE_KP_MEMDIVIDE | SDLK_SCANCODE_MASK,
            SDLK_KP_PLUSMINUS = Scancode.SDL_SCANCODE_KP_PLUSMINUS | SDLK_SCANCODE_MASK,
            SDLK_KP_CLEAR = Scancode.SDL_SCANCODE_KP_CLEAR | SDLK_SCANCODE_MASK,
            SDLK_KP_CLEARENTRY = Scancode.SDL_SCANCODE_KP_CLEARENTRY | SDLK_SCANCODE_MASK,
            SDLK_KP_BINARY = Scancode.SDL_SCANCODE_KP_BINARY | SDLK_SCANCODE_MASK,
            SDLK_KP_OCTAL = Scancode.SDL_SCANCODE_KP_OCTAL | SDLK_SCANCODE_MASK,
            SDLK_KP_DECIMAL = Scancode.SDL_SCANCODE_KP_DECIMAL | SDLK_SCANCODE_MASK,
            SDLK_KP_HEXADECIMAL =
                Scancode.SDL_SCANCODE_KP_HEXADECIMAL | SDLK_SCANCODE_MASK,

            SDLK_LCTRL = Scancode.SDL_SCANCODE_LCTRL | SDLK_SCANCODE_MASK,
            SDLK_LSHIFT = Scancode.SDL_SCANCODE_LSHIFT | SDLK_SCANCODE_MASK,
            SDLK_LALT = Scancode.SDL_SCANCODE_LALT | SDLK_SCANCODE_MASK,
            SDLK_LGUI = Scancode.SDL_SCANCODE_LGUI | SDLK_SCANCODE_MASK,
            SDLK_RCTRL = Scancode.SDL_SCANCODE_RCTRL | SDLK_SCANCODE_MASK,
            SDLK_RSHIFT = Scancode.SDL_SCANCODE_RSHIFT | SDLK_SCANCODE_MASK,
            SDLK_RALT = Scancode.SDL_SCANCODE_RALT | SDLK_SCANCODE_MASK,
            SDLK_RGUI = Scancode.SDL_SCANCODE_RGUI | SDLK_SCANCODE_MASK,

            SDLK_MODE = Scancode.SDL_SCANCODE_MODE | SDLK_SCANCODE_MASK,

            SDLK_AUDIONEXT = Scancode.SDL_SCANCODE_AUDIONEXT | SDLK_SCANCODE_MASK,
            SDLK_AUDIOPREV = Scancode.SDL_SCANCODE_AUDIOPREV | SDLK_SCANCODE_MASK,
            SDLK_AUDIOSTOP = Scancode.SDL_SCANCODE_AUDIOSTOP | SDLK_SCANCODE_MASK,
            SDLK_AUDIOPLAY = Scancode.SDL_SCANCODE_AUDIOPLAY | SDLK_SCANCODE_MASK,
            SDLK_AUDIOMUTE = Scancode.SDL_SCANCODE_AUDIOMUTE | SDLK_SCANCODE_MASK,
            SDLK_MEDIASELECT = Scancode.SDL_SCANCODE_MEDIASELECT | SDLK_SCANCODE_MASK,
            SDLK_WWW = Scancode.SDL_SCANCODE_WWW | SDLK_SCANCODE_MASK,
            SDLK_MAIL = Scancode.SDL_SCANCODE_MAIL | SDLK_SCANCODE_MASK,
            SDLK_CALCULATOR = Scancode.SDL_SCANCODE_CALCULATOR | SDLK_SCANCODE_MASK,
            SDLK_COMPUTER = Scancode.SDL_SCANCODE_COMPUTER | SDLK_SCANCODE_MASK,
            SDLK_AC_SEARCH = Scancode.SDL_SCANCODE_AC_SEARCH | SDLK_SCANCODE_MASK,
            SDLK_AC_HOME = Scancode.SDL_SCANCODE_AC_HOME | SDLK_SCANCODE_MASK,
            SDLK_AC_BACK = Scancode.SDL_SCANCODE_AC_BACK | SDLK_SCANCODE_MASK,
            SDLK_AC_FORWARD = Scancode.SDL_SCANCODE_AC_FORWARD | SDLK_SCANCODE_MASK,
            SDLK_AC_STOP = Scancode.SDL_SCANCODE_AC_STOP | SDLK_SCANCODE_MASK,
            SDLK_AC_REFRESH = Scancode.SDL_SCANCODE_AC_REFRESH | SDLK_SCANCODE_MASK,
            SDLK_AC_BOOKMARKS = Scancode.SDL_SCANCODE_AC_BOOKMARKS | SDLK_SCANCODE_MASK,

            SDLK_BRIGHTNESSDOWN =
                Scancode.SDL_SCANCODE_BRIGHTNESSDOWN | SDLK_SCANCODE_MASK,
            SDLK_BRIGHTNESSUP = Scancode.SDL_SCANCODE_BRIGHTNESSUP | SDLK_SCANCODE_MASK,
            SDLK_DISPLAYSWITCH = Scancode.SDL_SCANCODE_DISPLAYSWITCH | SDLK_SCANCODE_MASK,
            SDLK_KBDILLUMTOGGLE =
                Scancode.SDL_SCANCODE_KBDILLUMTOGGLE | SDLK_SCANCODE_MASK,
            SDLK_KBDILLUMDOWN = Scancode.SDL_SCANCODE_KBDILLUMDOWN | SDLK_SCANCODE_MASK,
            SDLK_KBDILLUMUP = Scancode.SDL_SCANCODE_KBDILLUMUP | SDLK_SCANCODE_MASK,
            SDLK_EJECT = Scancode.SDL_SCANCODE_EJECT | SDLK_SCANCODE_MASK,
            SDLK_SLEEP = Scancode.SDL_SCANCODE_SLEEP | SDLK_SCANCODE_MASK
        };
    }
}