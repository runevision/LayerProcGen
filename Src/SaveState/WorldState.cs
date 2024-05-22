/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;

namespace Runevision.SaveState {

	/*

	WorldState is a class that holds the current state of the world.
	There is Save, Load, and Reset methods available.
	It's an abstract class with implementations WorldStateBinaryLocal, WorldStatePlayerPrefs.

	WorldState
	 - WorldStateBinary
		- WorldStateLocal (saves to .txt files)
		- WorldStatePlayerPrefs (saves to player prefs)

	The implementations do the serialization/deserialization.

	The interface for data is handled through StateObjects.

	When a dynamic object (or its item placeholder) is created in the world,
	a corresponding StateObject must be created by the chunk that plans the dynamic object.
	Upon creation, the State object calls Load on itself
	(which retrieves data through the global WorldState dictionaries),
	and also registers to global Load and Save callbacks.
	The StateObject must then be added to the states of the relevant chunk that planned the dynamic object.

	Example from MazeChunk.Construct:

			// Create points
			List<StateWrapper<bool>> pointBooleans = new List<StateWrapper<bool>> ();
			for (int i=0; i<maze.GetNodeCount (); i++) {
				MazeNode node = maze.GetNode (i);
				if (node.variant == CellType.MinorLeaf)
					PlacePoints (node.point, node.localDist, pointBooleans);
			}
			states.Add (new StateObject (worldOffset, (int)StateType.MazeCoins, pointBooleans.ToArray ()));

	When the dynamic object is unloaded from the world (chunk is destroyed),
	the chunk calls Unload on all its StateObjects.
	This causes the StateObject to save itself, and unregister from global Load and Save callbacks.

	State that is added to chunk state in chunk Construct:
	- MazeCoins (MazeLayer)
	- MazeLocks (MazeLayer)
	- Artefact (RegionLayer)
	- RegionGate (TemplateLayer)

	Loading should:
	- Pause game.
	- Call WorldState.Load.
	- Call ObjectsLayer.MarkCurrentDynamicsDirty
	- Move player to loaded position.
	- Let world generate with new bounds as normal.
	- Call ObjectsLayer.ReconstructDirtyDynamics
	- Unpause game.

	*/

	public abstract class WorldState {

		public static string saveFileName = "SaveFile";
		// Set externally.
		public static WorldState instance;

		static bool readyToSave;

		public static void Reset() {
			instance.Clear();
			if (loadCallbacks != null)
				loadCallbacks();
			readyToSave = true;
		}
		public static void Save() {
			if (!readyToSave) {
				Logg.LogError("Attempting to save without having reset or loaded first! Suppressing save.");
				return;
			}
			if (saveCallbacks != null)
				saveCallbacks();
			instance.Save(saveFileName);
		}
		public static void Load() {
			instance.Load(saveFileName);
			if (loadCallbacks != null)
				loadCallbacks();
			readyToSave = true;
		}
		public static bool HasSave() {
			return instance.HasSave(saveFileName);
		}
		public static void DeleteSave() {
			instance.DeleteSave(saveFileName);
		}

		public delegate void Callback();
		public static Callback saveCallbacks;
		public static Callback loadCallbacks;
		public static Callback cleanupCallbacks;

		public static void ClearCallbacks() {
			if (cleanupCallbacks != null)
				cleanupCallbacks();
		}

		public void GetState(StateObject state) {
			int hash = state.GetHashCode();
			foreach (StateWrapper[] list in state.state) {
				if (list is StateWrapper<bool>[] boolWrappers)
					GetValues(hash, boolWrappers);
				if (list is StateWrapper<int>[] intWrappers)
					GetValues(hash, intWrappers);
				if (list is StateWrapper<Point>[] pointWrappers)
					GetValues(hash, pointWrappers);
				if (list is StateWrapper<Point3>[] point3Wrappers)
					GetValues(hash, point3Wrappers);
			}
		}

		public void SetState(StateObject state) {
			int hash = state.GetHashCode();
			foreach (StateWrapper[] list in state.state) {
				if (list is StateWrapper<bool>[] boolWrappers)
					SetValues(hash, boolWrappers);
				else if (list is StateWrapper<int>[] intWrappers)
					SetValues(hash, intWrappers);
				else if (list is StateWrapper<Point>[] pointWrappers)
					SetValues(hash, pointWrappers);
				else if (list is StateWrapper<Point3>[] point3Wrappers)
					SetValues(hash, point3Wrappers);
				else
					Logg.LogError($"No support saving data of type {list.GetType().Name}.");
			}
		}

		protected abstract void GetValues<T>(int hashKey, T[] arr) where T : StateWrapper;
		protected abstract void SetValues<T>(int hashKey, T[] arr) where T : StateWrapper;
		public abstract void Clear();
		public abstract void Save(string saveName);
		public abstract void Load(string saveName);
		public abstract bool HasSave(string saveName);
		public abstract void DeleteSave(string saveName);
	}

}
