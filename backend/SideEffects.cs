namespace Backend {
	public abstract class SideEffect {}

	public class ActionMapAddition : SideEffect {
		public string name = "";
		public bool isTransparent;
	}

	public class ActionMapRemoval : SideEffect {
		public string name = "";

		public ActionMapRemoval(string name = "") => this.name = name;
	}
}