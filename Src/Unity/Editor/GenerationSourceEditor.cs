/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.LayerProcGen;
using UnityEditor;
using UnityEngine;

namespace Runevision.UnityEditor {

	[CustomEditor(typeof(GenerationSource))]
	[CanEditMultipleObjects]
	public class GenerationSourceEditor : Editor {
		public bool HasFrameBounds() {
			return true;
		}

		public Bounds OnGetFrameBounds() {
			GenerationSource source = target as GenerationSource;
			Vector3 size = new Vector3(source.size.x, source.size.y, source.size.y);
			return new Bounds(source.transform.position, size);
		}
	}

}
