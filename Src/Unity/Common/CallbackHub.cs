/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using UnityEngine;

public class CallbackHub : MonoBehaviour {

	public static event Action update;
	public static event Action lateUpdate;
	public static event Action fixedUpdate;

	static CallbackHub instance;

	static int mainThreadID;
	public static bool isMainThread {
		get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadID; }
	}

	void Awake() {
		mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
		if (instance == null)
			instance = this;
	}

	void Update() {
		update?.Invoke();
	}

	void LateUpdate() {
		lateUpdate?.Invoke();
	}

	void FixedUpdate() {
		fixedUpdate?.Invoke();
	}

	public static void GuaranteeInstance() {
		if (instance != null)
			return;
		GameObject go = new GameObject("CallbackHub");
		go.hideFlags = HideFlags.HideAndDontSave;
		Application.quitting += () => Destroy(go);
		go.AddComponent<CallbackHub>();
	}

	public static void ExecuteOnMainThread(Action action) {
		if (action == null)
			return;
		if (isMainThread) {
			// If already on main thread, call action right away.
			action();
			return;
		}

		// Otherwise add a callback that calls action and then removes itself.
		Action handler = null;
		handler = () => {
			action();
			update -= handler;
		};
		update += handler;
	}
}
