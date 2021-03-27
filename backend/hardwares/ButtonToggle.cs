namespace Backend {
	public class ButtonToggle : Button {
		public Button ButtonToToggle { get; set; } = new ButtonKey();
		public bool IsPressElseRelease { get; set; } = true;

		protected override void PressImpl() {
			if (IsPressElseRelease) ButtonToToggle.Press();
			else ButtonToToggle.Release();
		}

		protected override void ReleaseImpl() {}
	}
}