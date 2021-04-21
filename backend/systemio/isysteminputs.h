namespace ISystemInputs {
	class IMouse {
		public:
			enum key {
				LEFT = 1,
				RIGHT,
				MIDDLE,
				FOUR,
				FIVE,
			};
			virtual void move(long x, long y) = 0;
			virtual void moveTo(long x, long y) = 0;
			virtual void press(key button) = 0;
			virtual void release(key button) = 0;
			virtual void click(key button) = 0;
			int getKeyMinValue() {
				return (int)key::LEFT;
			}

			int getKeyMaxValue() {
				return (int)key::FIVE;
			}
	};

	class IKeyboard {
		public:
			enum key {
				TAB = '\t', // int value of 9
				SPACE = ' ',
				QUOTATION = '\'',
				COMMA = ',',
				DASH = '-',
				DOT = '.',
				FORWARD_SLASH = '/',
				ROW_0 = '0',
				ROW_1 = '1',
				ROW_2 = '2',
				ROW_3 = '3',
				ROW_4 = '4',
				ROW_5 = '5',
				ROW_6 = '6',
				ROW_7 = '7',
				ROW_8 = '8',
				ROW_9 = '9',
				SEMICOLON = ';',
				EQUAL = '=',
				A = 'A',
				B = 'B',
				C = 'C',
				D = 'D',
				E = 'E',
				F = 'F',
				G = 'G',
				H = 'H',
				I = 'I',
				J = 'J',
				K = 'K',
				L = 'L',
				M = 'M',
				N = 'N',
				O = 'O',
				P = 'P',
				Q = 'Q',
				R = 'R',
				S = 'S',
				T = 'T',
				U = 'U',
				V = 'V',
				W = 'W',
				X = 'X',
				Y = 'Y',
				Z = 'Z',
				OPEN_BRACKET = '[',
				BACK_SLASH = '\\',
				CLOSE_BRACKET = ']',
				GRAVE = '`',
				ESCAPE,
				F1,
				F2,
				F3,
				F4,
				F5,
				F6,
				F7,
				F8,
				F9,
				F10,
				F11,
				F12,
				BACKSPACE,
				CAPS_LOCK,
				ENTER,
				LEFT_CONTROL,
				LEFT_SYSTEM, //note: no corresponding right key
				LEFT_ALTERNATE,
				LEFT_SHIFT,
				RIGHT_CONTROL,
				RIGHT_ALTERNATE,
				RIGHT_SHIFT,
				INSERT,
				HOME,
				PAGE_UP,
				PAGE_DOWN,
				DEL,
				END,
				LEFT_ARROW,
				UP_ARROW,
				DOWN_ARROW,
				RIGHT_ARROW,
				NUMBER_LOCK,
				PAD_BACKSLASH,
				PAD_STAR,
				PAD_DASH,
				PAD_ADD,
				PAD_ENTER,
				PAD_0,
				PAD_1,
				PAD_2,
				PAD_3,
				PAD_4,
				PAD_5,
				PAD_6,
				PAD_7,
				PAD_8,
				PAD_9,
				PAD_PERIOD, // int value of 146
			};
			virtual void press(key k) = 0;
			virtual void release(key k) = 0;
			virtual void tap(key k) = 0;
			int getKeyMinValue() {
				return (int)key::TAB;
			}

			int getKeyMaxValue() {
				return (int)key::PAD_PERIOD;
			}
	};

	class IGamepad {
		public:
			enum key {
				FACE_SOUTH = 147,
				FACE_EAST,
				FACE_WEST,
				FACE_NORTH,
				DPAD_LEFT,
				DPAD_RIGHT,
				DPAD_UP,
				DPAD_DOWN,
				LSTICK_CLICK,
				RSTICK_CLICK,
				LBUMPER,
				RBUMPER,
				FORWARD,
				BACKWARD,
				HOME,
			};
			virtual void press(key k) = 0;
			virtual void release(key k) = 0;
			virtual void tap(key k) = 0;
			virtual void moveLStick(int x, int y) = 0;
			virtual void moveRStick(int x, int y) = 0;
			virtual void pullLTrigger(unsigned int x, unsigned int y) = 0;
			virtual void pullRTrigger(unsigned int x, unsigned int y) = 0;
			int getKeyMinValue() {
				return (int)key::FACE_SOUTH;
			}

			int getKeyMaxValue() {
				return (int)key::HOME;
			}
	};
}