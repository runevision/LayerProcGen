/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System.Collections.Generic;
using System.Text;

namespace Runevision.SaveState {

	public class StateObject {
		public List<StateWrapper[]> state { get; private set; } = new List<StateWrapper[]>();

		readonly Point position;
		readonly int type;

		public static StateObject Get<T>(Point position, int type, StateWrapper<T> stateObj) {
			return new StateObject(position, type, new StateWrapper<T>[] { stateObj });
		}

		public StateObject(Point position, int type, params StateWrapper[][] stateLists) {
			this.position = position;
			this.type = type;
			foreach (StateWrapper[] stateList in stateLists)
				state.Add(stateList);
			Load();
			WorldState.saveCallbacks += Save;
			WorldState.loadCallbacks += Load;
			WorldState.cleanupCallbacks += Cleanup;
		}

		public void SetDontCleanup() {
			WorldState.cleanupCallbacks -= Cleanup;
		}

		public static T GetGlobal<T>(int hash) {
			Point id = new Point(hash, 0);
			StateObject savedState = StateObject.Get(id, -1, new StateWrapper<T>());
			T value = ((StateWrapper<T>)savedState.state[0][0]).Value;
			savedState.Unload();
			return value;
		}

		public static T GetGlobal<T>(Point id, int type) {
			StateObject savedState = StateObject.Get(id, type, new StateWrapper<T>());
			T value = ((StateWrapper<T>)savedState.state[0][0]).Value;
			savedState.Unload();
			return value;
		}

		public static void SetGlobal<T>(int hash, T value) {
			Point id = new Point(hash, 0);
			StateObject savedState = StateObject.Get(id, -1, new StateWrapper<T>());
			((StateWrapper<T>)savedState.state[0][0]).Value = value;
			savedState.Unload();
		}

		public static void SetGlobal<T>(Point id, int type, T value) {
			StateObject savedState = StateObject.Get(id, type, new StateWrapper<T>());
			((StateWrapper<T>)savedState.state[0][0]).Value = value;
			savedState.Unload();
		}

		public static void SetGlobal<T>(Point id, int type, params T[] values) {
			var stateWrappers = new StateWrapper<T>[values.Length];
			for (int i = 0; i < values.Length; i++)
				stateWrappers[i] = new StateWrapper<T>();
			StateObject savedState = new StateObject(id, type, stateWrappers);
			for (int i = 0; i < values.Length; i++)
				stateWrappers[i].Value = values[i];
			savedState.Unload();
		}

		public void Unload() {
			Save();
			WorldState.saveCallbacks -= Save;
			WorldState.loadCallbacks -= Load;
			WorldState.cleanupCallbacks -= Cleanup;
		}

		public void Cleanup() {
			WorldState.saveCallbacks -= Save;
			WorldState.loadCallbacks -= Load;
			WorldState.cleanupCallbacks -= Cleanup;
		}

		public override int GetHashCode() {
			return position.GetHashCode() + (type * 10000000);
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format("[StateObject {0} at {1} hash:{2}]",
				type, position, GetHashCode().ToString("X4")));
			foreach (var w in state) {
				sb.Append(w.GetType().Name);
				sb.Append(": ");
				foreach (var s in w) {
					sb.Append(s);
					sb.Append(", ");
				}
				sb.Append("\n");
			}
			return sb.ToString();
		}

		void Load() {
			WorldState.instance.GetState(this);
		}

		void Save() {
			WorldState.instance.SetState(this);
		}
	}

}
