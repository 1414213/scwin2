using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using api = SteamControllerApi;


namespace Backend {
	public abstract class Button : Hardware {
		public bool IsDualStage { get; set; }
		[JsonIgnore]
		public bool IsPressed => this.isPressed;
		[JsonIgnore]
		public bool IsSecondPress => isSecondPress;

		protected api.InputData? Input => input;

		api.InputData? input;
		bool isPressed = false;
		bool isRepetitious = false;
		bool isSecondPress;

		public Button(bool isRepetitious = false) {
			this.isRepetitious = isRepetitious;
		}

		public void Press() {
			if (isRepetitious) this.PressImpl();
			else {
				if (!isPressed) {
					isPressed = true;
					if (IsDualStage) {
						if (isSecondPress) this.ReleaseImpl();
						else this.PressImpl();
					} else this.PressImpl();
					isSecondPress = isSecondPress ? false : true;
				}
			}
		}

		public void Release() {
			if (isRepetitious) this.ReleaseImpl();
			else {
				if (isPressed) {
					isPressed = false;
					if (!IsDualStage) this.ReleaseImpl();
				}
			}
		}

		public void Tap() {
			// ensures that if tapped while held, a button press is still sent.
			// Not sure if this is a good idea or not.
			if (isPressed) {
				this.Release();
				this.Press();
			} else {
				this.Press();
				this.Release();
			}
		}

		protected abstract void PressImpl();
		protected abstract void ReleaseImpl();

		public override void DoEvent(api.InputData e) {
			if (!e.IsButton) throw new ArgumentException(e + " isn't a button.");

			input = e;
			if ((e.Flags & api.Flags.Pressed) == api.Flags.Pressed) this.Press();
			else this.Release();
		}

		public override void ReleaseAll() => this.Release();
	}
}