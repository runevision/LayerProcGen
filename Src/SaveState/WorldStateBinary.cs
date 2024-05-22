/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Runevision.SaveState {

	public abstract class WorldStateBinary : WorldState {
		Dictionary<System.Type, IDictionary> dicts = new Dictionary<System.Type, IDictionary>();

		protected void Save(BinaryWriter writer) {
			Serialize(GetDict<StateWrapper<bool>>(), writer);
			Serialize(GetDict<StateWrapper<int>>(), writer);
			Serialize(GetDict<StateWrapper<Point>>(), writer);
			Serialize(GetDict<StateWrapper<Point3>>(), writer);
			writer.Flush();
		}

		protected void Load(BinaryReader reader) {
			dicts.Clear();
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(StateWrapper<bool>), DeserializeDictionary<StateWrapper<bool>>(reader));
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(StateWrapper<int>), DeserializeDictionary<StateWrapper<int>>(reader));
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(StateWrapper<Point>), DeserializeDictionary<StateWrapper<Point>>(reader));
			if (reader.PeekChar() != -1)
				dicts.Add(typeof(StateWrapper<Point3>), DeserializeDictionary<StateWrapper<Point3>>(reader));
		}

		public override void Clear() {
			foreach (var dict in dicts.Values)
				dict.Clear();
		}

		Dictionary<int, T[]> GetDict<T>() {
			dicts.TryGetValue(typeof(T), out IDictionary dict);
			if (dict != null)
				return (Dictionary<int, T[]>)dict;
			dict = new Dictionary<int, T[]>();
			dicts.Add(typeof(T), dict);
			return (Dictionary<int, T[]>)dict;
		}

		protected override void GetValues<T>(int hashKey, T[] arr) {
			GetValues(GetDict<T>(), hashKey, arr);
		}

		protected void GetValues<T>(Dictionary<int, T[]> dict, int hashKey, T[] arr) where T : StateWrapper {
			if (dict.TryGetValue(hashKey, out var stored)) {
				if (stored.Length != arr.Length) {
					Logg.LogWarning(string.Format(
						"Inconsistent lengths in GetValues. Tried to load {0} but {1} was stored.\n{2}",
						arr.Length, stored.Length, System.Environment.StackTrace
					));
				}
				else {
					for (int i = 0; i < arr.Length; i++)
						arr[i].objectValue = stored[i].objectValue;
				}
			}
			else {
				for (int i = 0; i < arr.Length; i++)
					arr[i].SetDefault();
			}
		}
		protected override void SetValues<T>(int hashKey, T[] arr) {
			GetDict<T>()[hashKey] = arr;
		}

		// Serialize

		void Serialize<T>(Dictionary<int, T[]> dictionary, BinaryWriter writer) where T : StateWrapper {
			writer.Write(dictionary.Count);
			foreach (var kvp in dictionary) {
				writer.Write(kvp.Key);
				Serialize(kvp.Value, writer);
			}
		}

		void Serialize<T>(T[] array, BinaryWriter writer) where T : StateWrapper {
			writer.Write(array.Length);
			foreach (var b in array)
				Serialize(b, writer);
		}

		void Serialize<T>(T b, BinaryWriter writer) where T : StateWrapper {
			if (b is StateWrapper<bool>) {
				writer.Write((bool)(b.objectValue));
			}
			if (b is StateWrapper<int>) {
				writer.Write((int)(b.objectValue));
			}
			if (b is StateWrapper<Point>) {
				Point point = (Point)(b.objectValue);
				writer.Write(point.x);
				writer.Write(point.y);
			}
			if (b is StateWrapper<Point3>) {
				Point3 point = (Point3)(b.objectValue);
				writer.Write(point.x);
				writer.Write(point.y);
				writer.Write(point.z);
			}
		}

		// Deserialize

		Dictionary<int, T[]> DeserializeDictionary<T>(BinaryReader reader) where T : StateWrapper {
			int count = reader.ReadInt32();
			var dictionary = new Dictionary<int, T[]>(count);
			for (int n = 0; n < count; n++) {
				var key = reader.ReadInt32();
				var value = DeserializeArray<T>(reader);
				dictionary.Add(key, value);
				//Debug.Logg.Log ("Deserializing key "+key+": "+new String (value.Select (e => e ? 't' : '.').ToArray ()));
			}
			return dictionary;
		}

		T[] DeserializeArray<T>(BinaryReader reader) where T : StateWrapper {
			int count = reader.ReadInt32();
			T[] array = new T[count];
			for (int n = 0; n < count; n++)
				array[n] = Deserialize<T>(reader);
			return array;
		}

		T Deserialize<T>(BinaryReader reader) where T : StateWrapper {
			if (typeof(T) == typeof(StateWrapper<bool>)) {
				return (T)(object)new StateWrapper<bool>(reader.ReadBoolean());
			}
			if (typeof(T) == typeof(StateWrapper<int>)) {
				return (T)(object)new StateWrapper<int>(reader.ReadInt32());
			}
			if (typeof(T) == typeof(StateWrapper<Point>)) {
				return (T)(object)new StateWrapper<Point>(new Point(reader.ReadInt32(), reader.ReadInt32()));
			}
			if (typeof(T) == typeof(StateWrapper<Point3>)) {
				return (T)(object)new StateWrapper<Point3>(new Point3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()));
			}
			return default;
		}
	}

}
