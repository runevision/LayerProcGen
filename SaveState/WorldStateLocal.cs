/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace Runevision.SaveState {

	public class WorldStateLocal : WorldStateBinary {
		public override void Save(string saveName) {
			using (BinaryWriter writer = new BinaryWriter(File.Open(saveName + ".bin", FileMode.Create)))
				Save(writer);
		}

		public override void Load(string saveName) {
			using (BinaryReader reader = new BinaryReader(File.Open(saveName + ".bin", FileMode.Open)))
				Load(reader);
		}

		public override bool HasSave(string saveName) {
			return File.Exists(saveName + ".bin");
		}

		public override void DeleteSave(string saveName) {
			File.Delete(saveName + ".bin");
		}
	}

}
