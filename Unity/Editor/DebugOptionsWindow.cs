/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using UnityEngine;
using UnityEditor;

namespace Runevision.UnityEditor {

	public class DebugOptionsWindow : EditorWindow {
		Vector2 scroll;
		GUIStyle foldoutStyle;

		[MenuItem("Window/Debug Options")]
		static void Init() {
			DebugOptionsWindow window = GetWindow<DebugOptionsWindow>();
			window.titleContent = new GUIContent("Debug Options");
			window.Show();
		}

		void OnEnable() {
			DebugOption.UIChanged += Repaint;
		}

		void OnDisable() {
			DebugOption.UIChanged -= Repaint;
		}

		void OnGUI() {
			if (foldoutStyle == null || foldoutStyle.padding.left == 0) {
				foldoutStyle = new GUIStyle(EditorStyles.foldout);
				foldoutStyle.padding.left += 2;
			}

			EditorGUIUtility.hierarchyMode = true;
			scroll = EditorGUILayout.BeginScrollView(scroll);
			GUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			foreach (DebugOption option in DebugOption.root.children)
				DrawOption(option, 0);
			GUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
		}

		void DrawOption(DebugOption option, int level) {
			if (level > 10)
				return;
			if (option.hidden)
				return;

			if (option is DebugButton) {
				if (GUILayout.Button(option.name))
					option.HandleClick();
			}
			else if (option is DebugToggle) {
				EditorGUI.indentLevel--;
				bool newOn = EditorGUILayout.ToggleLeft(option.name, ((DebugToggle)option).enabledSelf);
				if (newOn != ((DebugToggle)option).enabledSelf)
					option.HandleClick();
				EditorGUI.indentLevel++;
			}
			else if (option is DebugFoldout) {
				bool newOn = EditorGUILayout.Foldout(((DebugFoldout)option).enabledSelf, option.name, foldoutStyle);
				if (newOn != ((DebugFoldout)option).enabledSelf)
					option.HandleClick();
			}

			DebugFoldout parent = option as DebugFoldout;
			if (parent != null && parent.enabledSelf) {
				EditorGUI.indentLevel++;
				foreach (DebugOption child in parent.children)
					DrawOption(child, level + 1);
				EditorGUI.indentLevel--;
			}
		}
	}

}
