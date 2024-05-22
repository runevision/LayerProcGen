/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using UnityEditor;
using UnityEngine;

namespace Runevision.UnityEditor {

	[CustomPropertyDrawer(typeof(Point3))]
	public class Point3Drawer : PropertyDrawer {
		GUIContent[] subLabels = {
			new GUIContent("X"),
			new GUIContent("Y"),
			new GUIContent("Z")
		};

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position = EditorGUI.PrefixLabel(position, label);
			EditorGUI.MultiPropertyField(position, subLabels, property.FindPropertyRelative("x"));
		}
	}

}
