using System.Threading;
using System.Threading.Tasks;

namespace Backend {
	public class ButtonMacro : Button {
		public Robot.Macro[] Pressed { get; set; } = {};
		public Robot.Macro[] Held { get; set; } = {};
		public Robot.Macro[] Released { get; set; } = {};
		public int RepetitionsPerSecond { get => 1000 / waitTime; set => waitTime = 1000 / value; }

		private Task onHold;
		private bool isHeld;
		private int waitTime = 10;

		public ButtonMacro() => this.onHold = new Task(() => {
			while (isHeld) {
				foreach (var m in Held) {
					robot.DoMacro(m);
					if (m.Wait > 0) Thread.Sleep(m.Wait);
				}
				Thread.Sleep(waitTime);
			}
		});

		protected override void PressImpl() {
			isHeld = true;
			robot.DoMacro(Pressed);
			onHold.Start();
		}

		protected override void ReleaseImpl() {
			isHeld = false;
			robot.DoMacro(Released);
		}
	}
}