using UnityEngine;
using System.Collections.Generic;
using Action=System.Action;

public class VungleSceneLoom : MonoBehaviour
{
	public interface ILoom {
		void QueueOnMainThread(Action action);
	}

	private static NullLoom _nullLoom = new NullLoom();
	private static LoomDispatcher _loom;
	private static VungleSceneLoom _instance;
	static bool _initialized = false;

	public static ILoom Loom {
		get {
			if (_loom != null) {
				return _loom as ILoom;
			}
			return _nullLoom as ILoom;
		}
	}

	void Awake() {
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject); 
			return; 
		}
		_instance  = this;
		DontDestroyOnLoad(gameObject);
		_loom = new LoomDispatcher();
	}

	public static void Initialize() {
		if (!_initialized)
		{
			var g = new GameObject("VungleSceneLoom");
			_instance = g.AddComponent<VungleSceneLoom>();
			_initialized = true;
		}
	}

	void OnDestroy() {
		_loom = null;
	}

	void Update() {
		if (Application.isPlaying) {
			_loom.Update();
		}
	}

	private class NullLoom : ILoom {
		public void QueueOnMainThread(Action action) {}
	}

	private class LoomDispatcher : ILoom {
		private readonly List<Action> actions = new List<Action>();

		public void QueueOnMainThread(Action action) {
			lock (actions) {
				actions.Add(action);
			}
		}

		public void Update() {
			// Pop the actions from the synchronized list
			Action[] actionsToRun = null;
			lock (actions) {
				actionsToRun = actions.ToArray();
				actions.Clear();
			}

			// Run each action
			foreach (Action action in actionsToRun) {
				action();
			}
		}
	}
}