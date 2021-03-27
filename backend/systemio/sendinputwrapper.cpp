#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include "sendinputwrapper.h"


unsigned short keyToVirtualKeycode(int k) {
	if ((k >= 48) && (k <= 57)) {
		return k;
	}
	else if ((k >= 65) && (k <= 90)) {
		return k;
	}
	else {
		switch ((key)k) {
			case key::TAB: return VK_TAB;
			case key::SPACE: return VK_SPACE;
			case key::QUOTATION: return VK_OEM_7;
			case key::COMMA: return VK_OEM_COMMA;
			case key::DASH: return VK_OEM_MINUS;
			case key::DOT: return VK_OEM_PERIOD;
			case key::FORWARD_SLASH: return VK_OEM_2;
			case key::SEMICOLON: return VK_OEM_1;
			case key::EQUAL: return VK_OEM_PLUS; //might be wrong idk
			case key::OPEN_BRACKET: return VK_OEM_4;
			case key::BACK_SLASH: return VK_OEM_5;
			case key::CLOSE_BRACKET: return VK_OEM_6;
			case key::GRAVE: return VK_OEM_3;
			case key::ESCAPE: return VK_ESCAPE;
			case key::F1: return VK_F1;
			case key::F2: return VK_F2;
			case key::F3: return VK_F3;
			case key::F4: return VK_F4;
			case key::F5: return VK_F5;
			case key::F6: return VK_F6;
			case key::F7: return VK_F7;
			case key::F8: return VK_F8;
			case key::F9: return VK_F9;
			case key::F10: return VK_F10;
			case key::F11: return VK_F11;
			case key::F12: return VK_F12;
			case key::BACKSPACE: return VK_BACK;
			case key::CAPS_LOCK: return VK_CAPITAL;
			case key::ENTER: return VK_RETURN;
			case key::LEFT_CONTROL: return VK_LCONTROL;
			case key::LEFT_SYSTEM: return VK_LWIN;
			case key::LEFT_ALTERNATE: return VK_MENU; //might be wrong
			case key::LEFT_SHIFT: return VK_LSHIFT;
			case key::RIGHT_CONTROL: return VK_RCONTROL;
			case key::RIGHT_ALTERNATE: return VK_MENU; //might be wrong
			case key::RIGHT_SHIFT: return VK_RSHIFT;
			case key::INSERT: return VK_INSERT;
			case key::HOME: return VK_HOME;
			case key::PAGE_UP: return VK_PRIOR;
			case key::PAGE_DOWN: return VK_NEXT;
			case key::DEL: return VK_DELETE;
			case key::END: return VK_END;
			case key::LEFT_ARROW: return VK_LEFT;
			case key::UP_ARROW: return VK_UP;
			case key::DOWN_ARROW: return VK_DOWN;
			case key::RIGHT_ARROW: return VK_RIGHT;
			case key::NUMBER_LOCK: return VK_NUMLOCK;
			case key::PAD_BACKSLASH: return VK_DIVIDE;
			case key::PAD_STAR: return VK_MULTIPLY;
			case key::PAD_DASH: return VK_SUBTRACT;
			case key::PAD_ADD: return VK_ADD;
			//can't find the keycode for this one
			//case key::PAD_ENTER: return vk_num
			case key::PAD_0: return VK_NUMPAD0;
			case key::PAD_1: return VK_NUMPAD1;
			case key::PAD_2: return VK_NUMPAD2;
			case key::PAD_3: return VK_NUMPAD3;
			case key::PAD_4: return VK_NUMPAD4;
			case key::PAD_5: return VK_NUMPAD5;
			case key::PAD_6: return VK_NUMPAD6;
			case key::PAD_7: return VK_NUMPAD7;
			case key::PAD_8: return VK_NUMPAD8;
			case key::PAD_9: return VK_NUMPAD9;
			case key::PAD_PERIOD: return VK_DECIMAL;
			default: return 0;
		}
	}
}

extern "C" {
	void MouseMoveTo(long x, long y) {
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = x;
		input.mi.dy = y;
		input.mi.dwFlags = (MOUSEEVENTF_ABSOLUTE|MOUSEEVENTF_MOVE);
		input.mi.mouseData = 0;
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;

		SendInput(1, &input, sizeof(INPUT));
	}

	void MouseMoveBy(long x, long y) {
		y = -y;
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = x;
		input.mi.dy = y;
		input.mi.dwFlags = MOUSEEVENTF_MOVE;
		input.mi.mouseData = 0;
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;

		SendInput(1, &input, sizeof(INPUT));
	}

	void MousePress(int keycode) {
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = 0;
		input.mi.dy = 0;
		switch (keycode) {
			case 1:
				input.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
				input.mi.mouseData = 0;
				break;
			case 2:
				input.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
				input.mi.mouseData = 0;
				break;
			case 3:
				input.mi.dwFlags = MOUSEEVENTF_MIDDLEDOWN;
				input.mi.mouseData = 0;
				break;
			case 4:
				input.mi.dwFlags = MOUSEEVENTF_XDOWN;
				input.mi.mouseData = XBUTTON1;
				break;
			case 5:
				input.mi.dwFlags = MOUSEEVENTF_XDOWN;
				input.mi.mouseData = XBUTTON2;
				break;
			default:
				return;
		}
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;
		SendInput(1, &input, sizeof(INPUT));
	}

	void MouseRelease(int keycode) {
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = 0;
		input.mi.dy = 0;
		switch (keycode) {
			case 1:
				input.mi.dwFlags = MOUSEEVENTF_LEFTUP;
				input.mi.mouseData = 0;
				break;
			case 2:
				input.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
				input.mi.mouseData = 0;
				break;
			case 3:
				input.mi.dwFlags = MOUSEEVENTF_MIDDLEUP;
				input.mi.mouseData = 0;
				break;
			case 4:
				input.mi.dwFlags = MOUSEEVENTF_XUP;
				input.mi.mouseData = XBUTTON1;
				break;
			case 5:
				input.mi.dwFlags = MOUSEEVENTF_XUP;
				input.mi.mouseData = XBUTTON2;
				break;
			default:
				return;
		}
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;
		SendInput(1, &input, sizeof(INPUT));
	}

	void MouseScroll(int amount, bool asClicks) {
		if (asClicks) amount *= WHEEL_DELTA;
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = 0;
		input.mi.dy = 0;
		input.mi.dwFlags = MOUSEEVENTF_WHEEL;
		input.mi.mouseData = amount;
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;
		SendInput(1, &input, sizeof(INPUT));
	}

	void KeyboardPress(int keycode) {
		INPUT input;
		input.type = INPUT_KEYBOARD;
		input.ki.wVk = keyToVirtualKeycode(keycode);
		input.ki.wScan = 0;
		input.ki.dwFlags = 0;
		input.ki.time = 0;
		input.ki.dwExtraInfo = 0;
		SendInput(1, &input, sizeof(INPUT));
	}

	void KeyboardRelease(int keycode) {
		INPUT input;
		input.type = INPUT_KEYBOARD;
		input.ki.wVk = keyToVirtualKeycode(keycode);
		input.ki.wScan = 0;
		input.ki.dwFlags = KEYEVENTF_KEYUP;
		input.ki.time = 0;
		input.ki.dwExtraInfo = 0;
		SendInput(1, &input, sizeof(INPUT));
	}
}