using System;

namespace Input {
	public class NoGamepadCreatedException : Exception {
		public NoGamepadCreatedException() : base("No virtual gamepad was created.") {}
	}

	public class SettingNotProportionException : Exception {
		public SettingNotProportionException() {}
		public SettingNotProportionException(string message) : base(message) {}
		public SettingNotProportionException(string message, Exception inner) : base(message, inner) {}
	}

	public class ActionMapNotFoundException : Exception {
		public ActionMapNotFoundException(string name) : base(
			$"Action map {name} couldn't be found in \"ActionMaps\"."
		) {}
	}

	public class SettingInvalidException : Exception {
		public SettingInvalidException() {}
		public SettingInvalidException(string message) : base(message) {}
		public SettingInvalidException(string message, Exception inner) : base(message, inner) {}
	}
}