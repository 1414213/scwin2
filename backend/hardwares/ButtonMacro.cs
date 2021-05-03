using System.Threading;
using System.Threading.Tasks;

namespace Backend {
	public class ButtonMacro : Button {
		public Robot.Macro[] Pressed { get; set; } = {};
		public Robot.Macro[] Held { get; set; } = {};
		public Robot.Macro[] Released { get; set; } = {};
		public int RepetitionsPerSecond { get => 1000 / waitTime; set {
			int waitTime = 1000 / value;
			if (waitTime < 0) throw new SettingInvalidException("RepetitionsPerSecond must be > 0.");
			this.waitTime = waitTime;
		} }

		private int waitTime = 10;
		private Task onHold = null!;
		private CancellationTokenSource cancelOnHold = null!;

		protected override void PressImpl() {
			this.DoMacros(Pressed);
			this.cancelOnHold = new CancellationTokenSource();
			onHold = Task.Run(() => {
				while (true) {
					this.DoMacros(Held);
					Thread.Sleep(waitTime);
					cancelOnHold.Token.ThrowIfCancellationRequested();
				}
			}, cancelOnHold.Token);
		}

		protected override void ReleaseImpl() {
			cancelOnHold.Cancel();
			this.DoMacros(Released);
		}

		private void DoMacros(Robot.Macro[] macros) {
			foreach (var macro in macros) {
				robot.DoMacro(macro);
				if (macro.AddActionLayer is string aal) sideEffectsPipe.Enqueue(new ActionMapAddition{
					name = aal,
					isTransparent = macro.AddActionLayerAsTransparent
				});
				if (macro.RemoveActionLayer is string ral) sideEffectsPipe.Enqueue(new ActionMapRemoval{ name = ral });
				Thread.Sleep(macro.Wait);
			}
		}
	}
}