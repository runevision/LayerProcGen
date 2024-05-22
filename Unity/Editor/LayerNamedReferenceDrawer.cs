/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.LayerProcGen;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Runevision.UnityEditor {

	[CustomPropertyDrawer(typeof(LayerNamedReference))]
	public class LayerNamedReferenceDrawer : PropertyDrawer {

		static string[] layerTypeStrings;
		static GUIContent[] layerTypeContents;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (layerTypeStrings == null) {
				Type layerBaseType = typeof(AbstractChunkBasedDataLayer);
				layerTypeStrings = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(assembly => assembly.GetTypes())
					.Where(t => t != layerBaseType && layerBaseType.IsAssignableFrom(t) && !t.IsGenericType)
					.Select(t => t.FullName)
					.ToArray();
				layerTypeContents = layerTypeStrings.Select(
					s => new GUIContent(s.Substring(s.LastIndexOf('.') + 1))).ToArray();
			}

			var nameProp = property.FindPropertyRelative("className");
			string currentString = nameProp.stringValue;
			int currentValue = Array.IndexOf(layerTypeStrings, currentString);
			EditorGUI.BeginChangeCheck();
			int newValue = EditorGUI.Popup(position, label, currentValue, layerTypeContents);
			if (EditorGUI.EndChangeCheck())
				nameProp.stringValue = layerTypeStrings[newValue];
		}
	}

}
