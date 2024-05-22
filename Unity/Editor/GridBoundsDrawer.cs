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

	[CustomPropertyDrawer(typeof(GridBounds))]
	public class GridBoundsDrawer : PropertyDrawer {
		GUIContent[] posLabels = {
			new GUIContent("X"),
			new GUIContent("Y")
		};
		GUIStyle labelRight = "RightLabel";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.height = EditorGUIUtility.singleLineHeight;
			Rect orig = position;
			position = EditorGUI.PrefixLabel(position, label);
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			GUI.Label(new Rect(orig.x, position.y, orig.width - position.width - 5, position.height), "Min", labelRight);
			EditorGUI.MultiPropertyField(position, posLabels, property.FindPropertyRelative("min.x"));
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			GUI.Label(new Rect(orig.x, position.y, orig.width - position.width - 5, position.height), "Max", labelRight);
			EditorGUI.MultiPropertyField(position, posLabels, property.FindPropertyRelative("max.x"));
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
		}
	}

}
