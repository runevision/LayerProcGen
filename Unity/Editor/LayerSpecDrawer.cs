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

	[CustomPropertyDrawer(typeof(VisualizationManager.LayerSpec))]
	public class LayerSpecDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			position.width -= 36 + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("layerClassName"), GUIContent.none);
			position.x += position.width + EditorGUIUtility.standardVerticalSpacing;
			position.width = 36;
			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("color"), GUIContent.none);
			EditorGUI.indentLevel = oldIndent;
		}
	}

}
