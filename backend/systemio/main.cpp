#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <synchapi.h>
#include <iostream>
#include "sendinputwrapper.h"

int main() {
	Sleep(3000);

	// winInputs::Mouse m;
	// ISystemInputs::IMouse* mouse = &m;
	// mouse->press(winInputs::Mouse::key::LEFT);
	// mouse->move(50, 0);
	// Sleep(200);
	// mouse->move(0, 20);
	// mouse->release(winInputs::Mouse::key::LEFT);

	// MouseMoveTo(500, 500);
	// MousePress(1);
	// MouseMoveTo(550, 480);
	// MouseRelease(1);

	KeyboardPress(69);
	KeyboardRelease(69);
	
	return 0;
}