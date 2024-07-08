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
			// TODO: Is there any way to make the list of supported types not hardcoded?
			// Seems hard when serialization code relies on generic type parameters.
			SerializeDictionary(GetDict<bool>(), writer);
			SerializeDictionary(GetDict<int>(), writer);
			SerializeDictionary(GetDict<Point>(), writer);
			SerializeDictionary(GetDict<Point3>(), writer);
			writer.Flush();
		}

		protected void Load(BinaryReader reader) {
			dicts.Clear();
			// TODO: Is there any way to make the list of supported types not hardcoded?
			// Seems hard when serialization code relies on generic type parameters.
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(bool), DeserializeDictionary<bool>(reader));
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(int), DeserializeDictionary<int>(reader));
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(Point), DeserializeDictionary<Point>(reader));
			if (reader.BaseStream.Position != reader.BaseStream.Length)
				dicts.Add(typeof(Point3), DeserializeDictionary<Point3>(reader));
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

		protected override void GetValues<T>(int hashKey, StateWrapper<T>[] arr) {
			Dictionary<int, T[]> dict = GetDict<T>();
			if (dict.TryGetValue(hashKey, out var stored)) {
				if (stored.Length != arr.Length) {
					Logg.LogWarning(string.Format(
						"Inconsistent lengths in GetValues. Tried to load {0} but {1} was stored.\n{2}",
						arr.Length, stored.Length, System.Environment.StackTrace
					));
				}
				int count = System.Math.Min (arr.Length, stored.Length);
				for (int i = 0; i < count; i++)
					arr[i].Value = stored[i];
			}
			else {
				for (int i = 0; i < arr.Length; i++)
					arr[i].SetDefault();
			}
		}
		protected override void SetValues<T>(int hashKey, StateWrapper<T>[] arr) {
			Dictionary<int, T[]> dict = GetDict<T>();
			T[] stored;
			if (!dict.TryGetValue(hashKey, out stored) || stored.Length != arr.Length) {
				stored = new T[arr.Length];
				dict[hashKey] = stored;
			}
			for (int i = 0; i < arr.Length; i++)
				stored[i] = arr[i].Value;
		}

		// Serialize

		void SerializeDictionary<T>(Dictionary<int, T[]> dictionary, BinaryWriter writer) where T : struct {
			writer.Write(dictionary.Count);
			foreach (var kvp in dictionary) {
				writer.Write(kvp.Key);
				SerializeArray(kvp.Value, writer);
			}
		}

		void SerializeArray<T>(T[] array, BinaryWriter writer) where T : struct {
			writer.Write(array.Length);
			foreach (var b in array)
				Serialize(b, writer);
		}

		void Serialize<T>(T b, BinaryWriter writer) where T : struct {
			if (b is bool) {
				writer.Write((bool)(object)b);
				return;
			}
			if (b is int) {
				writer.Write((int)(object)b);
				return;
			}
			if (b is IBinarySerializable) {
				((IBinarySerializable)b).Serialize(writer);
				return;
			}
			Logg.LogError("Attempting to serialize unsupported type " + typeof(T).Name);
		}

		// Deserialize

		Dictionary<int, T[]> DeserializeDictionary<T>(BinaryReader reader) where T : struct {
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

		T[] DeserializeArray<T>(BinaryReader reader) where T : struct {
			int count = reader.ReadInt32();
			T[] array = new T[count];
			for (int n = 0; n < count; n++)
				array[n] = Deserialize<T>(reader);
			return array;
		}

		T Deserialize<T>(BinaryReader reader) where T : struct {
			T b = new T();
			if (b is bool) {
				return (T)(object)reader.ReadBoolean();
			}
			if (b is int) {
				return (T)(object)reader.ReadInt32();
			}
			if (b is IBinarySerializable) {
				IBinarySerializable serializableValue = (IBinarySerializable)b;
				serializableValue.Deserialize(reader);
				return (T)serializableValue;
			}
			Logg.LogError("Attempting to deserialize unsupported type " + typeof(T).Name);
			return default;
		}
	}

}
