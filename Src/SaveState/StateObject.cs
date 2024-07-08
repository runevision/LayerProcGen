/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Text;

namespace Runevision.SaveState {

	public class StateObject {
		public List<StateWrapper[]> state { get; private set; } = new List<StateWrapper[]>();

		int hash;

		public static StateObject Get<T>(int hash, StateWrapper<T> stateObj) {
			return new StateObject(hash, new StateWrapper<T>[] { stateObj });
		}

		static StateObject temporary = new StateObject();
		static StateObject GetTemporary<T>(int hash) {
			temporary.hash = hash;
			temporary.state = StateWrapper<T>.GetTemporary();
			return temporary;
		}
		private StateObject() {	}

		public StateObject(int hash, params StateWrapper[][] stateLists) {
			this.hash = hash;
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
			StateObject savedState = GetTemporary<T>(hash);
			savedState.Load();
			T value = ((StateWrapper<T>)savedState.state[0][0]).Value;
			return value;
		}

		public static void SetGlobal<T>(int hash, T value) {
			StateObject savedState = GetTemporary<T>(hash);
			((StateWrapper<T>)savedState.state[0][0]).Value = value;
			savedState.Save();
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
			return hash;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format("[StateObject {0}]",
				hash.ToString("X4")));
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
