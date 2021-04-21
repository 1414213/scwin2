#include "isysteminputs.h"

namespace winInputs {
	class Mouse : public ISystemInputs::IMouse {
		public:
			Mouse* newMouse();
			void deleteMouse(Mouse* mouse);
			void move(long x, long y);
			void moveTo(long x, long y);
			void press(key button);
			void release(key button);
			void click(key button);
	};

	class Keyboard : public ISystemInputs::IKeyboard {
		public:
			Keyboard* newKeyboard();
			void deleteKeyboard(Keyboard* keyboard);
			void press(key k);
			void release(key k);
			void tap(key k);
		private:
			unsigned int keyToVirtualKeycode(key k);
	};
}