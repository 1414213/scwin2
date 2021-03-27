namespace Backend {
	public abstract class SideEffect {}

	public class ActionMapAddition : SideEffect {
		public string name = "";
		public bool isLayered;
	}

	public class ActionMapRemoval : SideEffect {
		public string name = "";
	}
}