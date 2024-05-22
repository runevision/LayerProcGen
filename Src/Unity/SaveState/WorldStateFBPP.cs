/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.IO;

namespace Runevision.SaveState {

	public class WorldStateFBPP : WorldStateBinary {
		public override void Save(string saveName) {
			using (MemoryStream stream = new MemoryStream()) {
				using (BinaryWriter writer = new BinaryWriter(stream)) {
					Save(writer);
				}
				stream.Flush();
				FBPP.SetString(saveName, System.Convert.ToBase64String(stream.ToArray()));
				FBPP.Save();
			}
		}

		public override void Load(string saveName) {
			string tmp = FBPP.GetString(saveName, string.Empty);
			using (MemoryStream stream = new MemoryStream(System.Convert.FromBase64String(tmp)))
			using (BinaryReader reader = new BinaryReader(stream))
				Load(reader);
		}

		public override bool HasSave(string saveName) {
			return FBPP.HasKey(saveName);
		}

		public override void DeleteSave(string saveName) {
			FBPP.DeleteKey(saveName);
		}
	}

}
