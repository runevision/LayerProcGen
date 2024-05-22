/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace Runevision.Common {

	[RequireComponent(typeof(RectTransform))]
	public class DebugOptions : MonoBehaviour {

		// Static functionality.

		static Rect window1View;
		static Vector2 scroll;
		static RectOffset windowBorder;
		static Rect rect;
		static GUIContent kWindowTitle = new GUIContent("Debug Options");

		static GUIStyle foldoutStyle;

		public static void DrawDebugOptions(Rect rect) {
			if (windowBorder == null)
				windowBorder = new RectOffset(2, 2, 17, 2);
			if (foldoutStyle == null)
				foldoutStyle = CreateFoldoutStyle();

			DebugOptions.rect = rect;
			GUI.Window(100, rect, DebugWindow, kWindowTitle);
		}

		static void DrawOption(DebugOption option, int level, ref Rect rect) {
			if (level > 10)
				return;
			if (option.hidden)
				return;
			Rect inner = rect;
			inner.xMin += level * 15;

			if (option is DebugButton) {
				if (GUI.Button(inner, option.name))
					option.HandleClick();
			}
			else if (option is DebugToggle) {
				bool newOn = GUI.Toggle(inner, ((DebugToggle)option).enabledSelf, option.name);
				if (newOn != ((DebugToggle)option).enabledSelf)
					option.HandleClick();
			}
			else if (option is DebugFoldout) {
				bool newOn = GUI.Toggle(inner, ((DebugFoldout)option).enabledSelf, option.name, foldoutStyle);
				if (newOn != ((DebugFoldout)option).enabledSelf)
					option.HandleClick();
			}

			rect.y += rect.height;

			DebugFoldout parent = option as DebugFoldout;
			if (parent != null && parent.enabledSelf)
				foreach (DebugOption child in parent.children)
					DrawOption(child, level + 1, ref rect);
		}

		static void DebugWindow(int id) {
			Rect window1Rect = DebugOptions.rect;
			window1Rect.x = 0;
			window1Rect.y = 0;
			window1Rect = windowBorder.Remove(window1Rect);
			scroll = GUI.BeginScrollView(window1Rect, scroll, window1View);
			Rect optionsRect = new Rect(0, 0, window1Rect.width - 16, 20);
			Rect rect = optionsRect;
			foreach (DebugOption option in DebugOption.root.children)
				DrawOption(option, 0, ref rect);
			GUI.enabled = true;
			optionsRect.yMax = rect.yMax - rect.height;
			window1View = optionsRect;
			GUI.EndScrollView();
		}

		static GUIStyle CreateFoldoutStyle() {
			GUIStyle style = new GUIStyle(GUI.skin.toggle);
			var collapsed = GetFoldoutImage(false);
			var expanded = GetFoldoutImage(true);
			style.normal.background = collapsed;
			style.hover.background = null;
			style.focused.background = null;
			style.active.background = null;
			style.onNormal.background = expanded;
			style.onHover.background = null;
			style.onFocused.background = null;
			style.onActive.background = null;
			return style;
		}

		static Texture2D GetFoldoutImage(bool expanded) {
			Texture2D tex = new Texture2D(14, 14);
			var pixels = tex.GetPixels();
			Vector2 v1 = new Vector2(1, -2).normalized;
			Vector2 v2 = new Vector2(1, 2).normalized;
			Vector2 v3 = new Vector2(-1, 0).normalized;
			for (int i = 0; i < tex.width; i++) {
				for (int j = 0; j < tex.height; j++) {
					Vector2 vec = expanded ?
						new Vector2(6f - j, i - 6f) :
						new Vector2(i - 7f, j - 7f);
					float pattern = Mathf.Max(
						Mathf.Max(
							Vector2.Dot(vec, v1) + 1,
							Vector2.Dot(vec, v2) + 1
						),
						Vector2.Dot(vec, v3) - 2);
					float alpha = Mathf.InverseLerp(3f, 2f, pattern);
					pixels[14 * j + i] = new Color(1,1,1, alpha); 
				}
			}
			tex.SetPixels(pixels);
			tex.Apply();
			return tex;
		}

		// Component functionality.

		RectTransform overlayRect;
		Canvas canvas;
		float scaleFactor = 1;

		void Awake() {
			overlayRect = GetComponent<RectTransform>();
			canvas = overlayRect.GetComponentInParent<Canvas>();
		}

		void OnGUI() {
			if (canvas)
				scaleFactor = canvas.scaleFactor;

			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scaleFactor);
			
			Rect r = overlayRect.rect;
			r.position += (Vector2)overlayRect.position / scaleFactor;
			DrawDebugOptions(r);

			GUI.matrix = Matrix4x4.identity;
		}
	}

}
