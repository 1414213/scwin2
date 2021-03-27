#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include "wininputs.h"

namespace winInputs {
	Mouse* Mouse::newMouse() {
		return new Mouse();
	}

	void deleteMouse(Mouse* mouse) {
		delete mouse;
	}

	void Mouse::move(long x, long y) {
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

	void Mouse::moveTo(long x, long y) {
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

	void Mouse::press(Mouse::key button) {
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = 0;
		input.mi.dy = 0;
		switch (button) {
			case Mouse::key::LEFT:
				input.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
				break;
			case Mouse::key::MIDDLE:
				input.mi.dwFlags = MOUSEEVENTF_MIDDLEDOWN;
				break;
			case Mouse::key::RIGHT:
				input.mi.dwFlags = MOUSEEVENTF_RIGHTDOWN;
				break;
		}
		input.mi.mouseData = 0;
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;

		SendInput(1, &input, sizeof(INPUT));
	}

	void Mouse::release(Mouse::key button) {
		INPUT input;
		input.type = INPUT_MOUSE;
		input.mi.dx = 0;
		input.mi.dy = 0;
		switch (button) {
			case Mouse::key::LEFT:
				input.mi.dwFlags = MOUSEEVENTF_LEFTUP;
				break;
			case Mouse::key::MIDDLE:
				input.mi.dwFlags = MOUSEEVENTF_MIDDLEUP;
				break;
			case Mouse::key::RIGHT:
				input.mi.dwFlags = MOUSEEVENTF_RIGHTUP;
				break;
		}
		input.mi.mouseData = 0;
		input.mi.dwExtraInfo = NULL;
		input.mi.time = 0;

		SendInput(1, &input, sizeof(INPUT));
	}

	void Mouse::click(Mouse::key button) {
		Mouse::press(button);
		Mouse::release(button);
	}

	Keyboard* Keyboard::newKeyboard() {
		return new Keyboard();
	}

	void Keyboard::deleteKeyboard(Keyboard* keyboard) {
		delete keyboard;
	}

	void Keyboard::press(Keyboard::key k) {
		INPUT input;
		input.type = INPUT_KEYBOARD;
		input.ki.wVk = 0;
		input.ki.wScan = keyToVirtualKeycode(k);
		input.ki.dwFlags = 0;
		input.ki.time = 0;
		input.ki.dwExtraInfo = 0;

		SendInput(1, &input, sizeof(INPUT));
	}

	void Keyboard::release(Keyboard::key k) {
		INPUT input;
		input.type = INPUT_KEYBOARD;
		input.ki.wVk = 0;
		input.ki.wScan = keyToVirtualKeycode(k);
		input.ki.dwFlags = KEYEVENTF_KEYUP;
		input.ki.time = 0;
		input.ki.dwExtraInfo = 0;
		
		SendInput(1, &input, sizeof(INPUT));
	}

	void Keyboard::tap(Keyboard::key k) {
		Keyboard::press(k);
		Keyboard::release(k);
	}

	unsigned int Keyboard::keyToVirtualKeycode(Keyboard::key k) {
		if ((k >= 48) && (k <= 57)) {
			return k;
		}
		else if ((k >= 65) && (k <= 90)) {
			return k;
		}
		else {
			switch (k) {
				case Keyboard::TAB: return VK_TAB;
				case Keyboard::SPACE: return VK_SPACE;
				case Keyboard::QUOTATION: return VK_OEM_7;
				case Keyboard::COMMA: return VK_OEM_COMMA;
				case Keyboard::DASH: return VK_OEM_MINUS;
				case Keyboard::DOT: return VK_OEM_PERIOD;
				case Keyboard::FORWARD_SLASH: return VK_OEM_2;
				case Keyboard::SEMICOLON: return VK_OEM_1;
				case Keyboard::EQUAL: return VK_OEM_PLUS; //might be wrong idk
				case Keyboard::OPEN_BRACKET: return VK_OEM_4;
				case Keyboard::BACK_SLASH: return VK_OEM_5;
				case Keyboard::CLOSE_BRACKET: return VK_OEM_6;
				case Keyboard::GRAVE: return VK_OEM_3;
				case Keyboard::ESCAPE: return VK_ESCAPE;
				case Keyboard::F1: return VK_F1;
				case Keyboard::F2: return VK_F2;
				case Keyboard::F3: return VK_F3;
				case Keyboard::F4: return VK_F4;
				case Keyboard::F5: return VK_F5;
				case Keyboard::F6: return VK_F6;
				case Keyboard::F7: return VK_F7;
				case Keyboard::F8: return VK_F8;
				case Keyboard::F9: return VK_F9;
				case Keyboard::F10: return VK_F10;
				case Keyboard::F11: return VK_F11;
				case Keyboard::F12: return VK_F12;
				case Keyboard::BACKSPACE: return VK_BACK;
				case Keyboard::CAPS_LOCK: return VK_CAPITAL;
				case Keyboard::ENTER: return VK_RETURN;
				case Keyboard::LEFT_CONTROL: return VK_LCONTROL;
				case Keyboard::LEFT_SYSTEM: return VK_LWIN;
				case Keyboard::LEFT_ALTERNATE: return VK_MENU; //might be wrong
				case Keyboard::LEFT_SHIFT: return VK_LSHIFT;
				case Keyboard::RIGHT_CONTROL: return VK_RCONTROL;
				case Keyboard::RIGHT_ALTERNATE: return VK_MENU; //might be wrong
				case Keyboard::RIGHT_SHIFT: return VK_RSHIFT;
				case Keyboard::INSERT: return VK_INSERT;
				case Keyboard::HOME: return VK_HOME;
				case Keyboard::PAGE_UP: return VK_PRIOR;
				case Keyboard::PAGE_DOWN: return VK_NEXT;
				case Keyboard::DEL: return VK_DELETE;
				case Keyboard::END: return VK_END;
				case Keyboard::LEFT_ARROW: return VK_LEFT;
				case Keyboard::UP_ARROW: return VK_UP;
				case Keyboard::DOWN_ARROW: return VK_DOWN;
				case Keyboard::RIGHT_ARROW: return VK_RIGHT;
				case Keyboard::NUMBER_LOCK: return VK_NUMLOCK;
				case Keyboard::PAD_BACKSLASH: return VK_DIVIDE;
				case Keyboard::PAD_STAR: return VK_MULTIPLY;
				case Keyboard::PAD_DASH: return VK_SUBTRACT;
				case Keyboard::PAD_ADD: return VK_ADD;
				//can't find the keycode for this one
				//case Keyboard::PAD_ENTER: return vk_num
				case Keyboard::PAD_0: return VK_NUMPAD0;
				case Keyboard::PAD_1: return VK_NUMPAD1;
				case Keyboard::PAD_2: return VK_NUMPAD2;
				case Keyboard::PAD_3: return VK_NUMPAD3;
				case Keyboard::PAD_4: return VK_NUMPAD4;
				case Keyboard::PAD_5: return VK_NUMPAD5;
				case Keyboard::PAD_6: return VK_NUMPAD6;
				case Keyboard::PAD_7: return VK_NUMPAD7;
				case Keyboard::PAD_8: return VK_NUMPAD8;
				case Keyboard::PAD_9: return VK_NUMPAD9;
				case Keyboard::PAD_PERIOD: return VK_DECIMAL;
			}
		}
	}
}