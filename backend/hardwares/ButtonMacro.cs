using System.Threading;
using System.Threading.Tasks;

namespace Backend {
	public class ButtonMacro : Button {
		public Robot.Macro[] Pressed { get; set; } = {};
		public Robot.Macro[] Held { get; set; } = {};
		public Robot.Macro[] Released { get; set; } = {};
		public int RepetitionsPerSecond {
			get => 1000 / this.waitTime;
			set {
				int waitTime = 1000 / value;
				if (waitTime < 0) throw new SettingInvalidException("RepetitionsPerSecond must be > 0.");
				this.waitTime = waitTime;
			}
		}

		private int waitTime = 10;
		private Task onHold = null!;
		private CancellationTokenSource cancelOnHold = null!;

		protected override void PressImpl() {
			robot.DoMacro(Pressed);
			this.cancelOnHold = new CancellationTokenSource();
			onHold = Task.Run(() => {
				while (true) {
					foreach (var m in Held) {
						robot.DoMacro(m);
						if (m.Wait > 0) Thread.Sleep(m.Wait);
					}
					Thread.Sleep(waitTime);
					cancelOnHold.Token.ThrowIfCancellationRequested();
				}
			}, cancelOnHold.Token);
		}

		protected override void ReleaseImpl() {
			cancelOnHold.Cancel();
			robot.DoMacro(Released);
		}
	}
}