using UnityEngine;

// ScriptableObject which can be called as a singleton.
// Loaded on demand. If called first time from a non-main-thread, it waits for the main thread.
public abstract class SingletonAsset<T> : ScriptableObject where T : ScriptableObject, new() {
	static T s_Instance = null;
	static object loadLock = new object();
	public static T instance {
		get {
			if (s_Instance == null) {
				CallbackHub.ExecuteOnMainThread(() => {
					lock (loadLock) {
						if (s_Instance == null) {
							s_Instance = Resources.Load<T>(typeof(T).Name);
							(s_Instance as SingletonAsset<T>).OnInitialize();
						}
					}
				});
				while (s_Instance == null)
					System.Threading.Thread.Sleep(1);
			}
			return s_Instance;
		}
	}

	protected virtual void OnInitialize() { }
}
