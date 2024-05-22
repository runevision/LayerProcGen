/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Runevision.Common {

	public class DebugFoldout : DebugOption {
		public bool enabledSelf { get; protected set; }
		public List<DebugOption> children = new List<DebugOption>();

		public static DebugFoldout Create(string namePath, bool defaultOn = false) {
			DebugFoldout option = GetOrCreate<DebugFoldout>(namePath);
			option.enabledSelf = defaultOn;
			return option;
		}

		public override void HandleClick() {
			enabledSelf = !enabledSelf;
			NotifyUIChanged();
		}

		internal static DebugFoldout CreateRoot() {
			return new DebugFoldout() { name = "", enabledSelf = true };
		}
	}

	public class DebugToggle : DebugFoldout {
		public bool enabled { get; protected set; }
		public event Action<bool> Callback;

		// Animation related properties.
		public float animValueLinear { get; protected set; }
		public float animValueDuration { get; set; } = 0.25f;
		public float animValue { get; protected set; }
		public float animAlpha { get { return animValueLinear; } }
		public bool visible { get { return animValueLinear > 0; } }
		public float animTarget { get { return enabled ? 1f : 0f; } }

		public static DebugToggle Create(string namePath, bool defaultOn = false, Action<bool> callback = null) {
			DebugToggle option = GetOrCreate<DebugToggle>(namePath);
			option.enabledSelf = defaultOn;
			if (callback != null)
				option.Callback += callback;
			option.UpdateCheckSelf();
			option.ResetAnimValue();
			return option;
		}

		public override void HandleClick() {
			SetEnabled(!enabledSelf);
		}

		public void SetEnabled(bool isEnabled) {
			enabledSelf = isEnabled;
			if (UpdateCheckSelf())
				NotifyUIChanged();
		}

		protected bool UpdateCheckSelf() {
			bool old = enabled;

			enabled = enabledSelf;
			if (enabled) {
				DebugFoldout curParent = parent;
				while (curParent != null) {
					if (curParent is DebugToggle toggle) {
						enabled &= toggle.enabled;
						break;
					}
					curParent = curParent.parent;
				}
			}

			if (enabled != old) {
				Callback?.Invoke(enabled);
				foreach (DebugOption o in children)
					if (o is DebugFoldout foldout)
						UpdateNestedEnabled(foldout, enabled);
				return true;
			}
			return false;
		}

		protected static void UpdateNestedEnabled(DebugFoldout foldout, bool parentEnabled) {
			bool noChange = false;
			if (foldout is DebugToggle toggle) {
				bool oldEnabled = toggle.enabled;
				toggle.enabled = toggle.enabledSelf && parentEnabled;
				if (oldEnabled == toggle.enabled)
					noChange = true;
				else
					toggle.Callback?.Invoke(toggle.enabled);
				parentEnabled &= toggle.enabled;
			}

			if (!noChange) {
				foreach (DebugOption o in foldout.children)
					if (o is DebugFoldout childFoldout)
						UpdateNestedEnabled(childFoldout, parentEnabled);
			}
		}

		public override void UpdateAnimValue(float delta) {
			if (animValueLinear == animTarget)
				return;
			animValueLinear = MoveTowards(animValueLinear, animTarget, delta / animValueDuration);
			animValue = SmoothStep(0f, 1f, animValueLinear);
		}

		public void ResetAnimValue() {
			animValueLinear = animTarget;
			animValue = animValueLinear;
		}

		static float MoveTowards(float current, float target, float maxDelta) {
			if (Math.Abs(target - current) <= maxDelta)
				return target;
			return current + Math.Sign(target - current) * maxDelta;
		}

		static float SmoothStep(float from, float to, float t) {
			t = Math.Min(Math.Max(t, 0f), 1f);
			t = -2f * t * t * t + 3f * t * t;
			return to * t + from * (1f - t);
		}
	}

	public class DebugRadioButton : DebugToggle {
		public static new DebugRadioButton Create(string namePath, bool defaultOn = false, Action<bool> callback = null) {
			DebugRadioButton option = GetOrCreate<DebugRadioButton>(namePath);
			if (defaultOn)
				option.enabledSelf = true;
			if (callback != null)
				option.Callback += callback;
			option.UpdateCheckSelf();
			return option;
		}

		public override void HandleClick() {
			if (enabledSelf)
				return;
			foreach (DebugOption sibling in parent.children)
				if (sibling is DebugRadioButton radioButton)
					radioButton.SetEnabled(radioButton == this);
		}
	}

	public class DebugButton : DebugOption {
		public event Action Callback;

		public static DebugButton Create(string namePath, Action callback = null) {
			DebugButton option = GetOrCreate<DebugButton>(namePath);
			if (callback != null)
				option.Callback += callback;
			return option;
		}

		public override void HandleClick() {
			Callback?.Invoke();
		}
	}

	/// <summary>
	/// The central class in a system for quickly specifying debug options.
	/// </summary>
	/// <remarks>
	/// The functionality primarily resides in this class and the derived classes,
	/// <see cref="DebugToggle"/>, <see cref="DebugRadioButton"/>, <see cref="DebugFoldout"/>,
	/// <see cref="DebugButton"/>.
	/// 
	/// <para>Disabling a <see cref="DebugToggle"/> temporarily disables all child toggles under it,
	/// and also hides them (as well as other control types under it).</para>
	/// 
	/// <para>Collaping a <see cref="DebugFoldout"/> (a disclosure widget) also hides all
	/// child controls under it, but does not affect their enabled state.</para>
	/// 
	/// <para>A <see cref="DebugRadioButton"/> looks and behaves the same as a toggle,
	/// except that when it's enabled, it disables all sibling radio buttons, and clicking
	/// an already enabled radio button has no effect.</para>
	/// 
	/// <para>A <see cref="DebugButton"/> is a simple button which you can register a callback to.</para>
	/// </remarks>
	/// <h2>Specification in code</h2>
	/// <remarks>
	/// Debug options are specified with a path parameter, and the UI for the debug options 
	/// automatically displays the options in a hierarchy based on those paths.
	/// </remarks>
	/// <example>
	/// Example specification:
	/// <code>
	/// public DebugToggle debugRadiuses =
	/// 	DebugToggle.Create(">Layers/ExampleLayer/Radiuses");
	/// </code>
	/// The above code would implicitly create a foldout called "Layers",
	/// with an implicitly created child Toggle called "ExampleLayer",
	/// with a child toggle called "Radiuses", which is the one returned by the function.
	/// </example>
	/// <remarks>
	/// The paths are divided by forward slashes <c>/</c>. Each parent control is either a toggle 
	/// or a foldout. Foldouts are created by prepending the name with a greater-than sign <c>&gt;</c>
	/// (mimicking the look of a foldout/disclosure triangle).
	/// 
	/// If the same control is created from multiple places, either explicitly or implicitly,
	/// they will reference the same control, so that no duplicates are created.
	/// </remarks>
	/// <h2>Usage in code</h2>
	/// <remarks>
	/// The value of a DebugToggle or DebugRadioButton is checked via its enabled property.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Code executed each update.
	/// if (debugRadiuses.enabled) {
	/// 	// Do stuff.
	/// }
	/// </code>
	/// </example>
	/// <remarks>
	/// It's also possible to register callbacks instead of checking the enabled property on the fly.
	/// </remarks>
	/// <example>
	/// <code>
	/// // Code executed once.
	/// debugRadiuses.Callback += enabled => {
	/// 	// Change stuff based on enabled bool.
	/// }
	/// </code>
	/// </example>
	/// <h2>UI display and interaction</h2>
	/// <remarks>
	/// While the core DebugOption functionality is not Unity-dependent, the front-end for
	/// interacting with the controls is.
	/// 
	/// <b>Debug Options component</b><br/>
	/// Add the <see cref="DebugOptions"/> component to a RectTransform somewhere under a Canvas,
	/// and the debug options will be displayed within the RectTransform rect during Play Mode
	/// and in builds. The component uses IMGUI, the RectTransform is just for layout.
	/// Alternatively, you can call the static method <c>DebugOptions.DrawDebugOptions(Rect rect)</c>
	/// from an <c>OnGUI</c> method of your own Unity component code.
	/// 
	/// <b>Debug Options window</b><br/>
	/// The <see cref="Runevision.UnityEditor.DebugOptionsWindow">DebugOptionsWindow</see> is a Unity editor window that displays the debug options.
	/// When running the game in Play Mode in the Unity editor, this makes it possible to use
	/// the debug options without displaying them in the game itself.
	/// This is particularly useful for recording polished videos.
	/// The window is opened with the menu <c>Window > Debug Options</c>.
	/// 
	/// Similar classes could be made for drawing the debug options under different frameworks
	/// than Unity, especially if they support immediate-mode UI.
	/// </remarks>
	public abstract class DebugOption {

		public string name { get; protected set; }
		public bool hidden { get; set; }
		public DebugFoldout parent;

		public static DebugFoldout root = DebugFoldout.CreateRoot();

		public static event Action UIChanged;
		protected static void NotifyUIChanged() { UIChanged?.Invoke(); }

		static Dictionary<string, DebugOption> s_Options = new Dictionary<string, DebugOption>();

		public static void UpdateAnimValues(float timeDelta) {
			foreach (DebugOption option in s_Options.Values)
				option.UpdateAnimValue(timeDelta);
		}

		protected DebugOption() { }

		public abstract void HandleClick();
		public virtual void UpdateAnimValue(float delta) { }

		internal static T GetOrCreate<T>(string namePath) where T : DebugOption, new() {
			if (s_Options.TryGetValue(namePath, out DebugOption existingOption))
				return (T)existingOption;

			string[] path = namePath.Split('/');
			DebugFoldout parent = root;
			for (int i = 0; i < path.Length; i++) {
				bool self = (i == path.Length - 1);
				string name = path[i];
				bool isFoldout = false;
				if (name[0] == '>') {
					name = name.Substring(1);
					isFoldout = true;
				}
				DebugOption matchingOption = parent.children.FirstOrDefault(
					e =>
						(self || e is DebugFoldout)
						&& (isFoldout == (e.GetType() == typeof(DebugFoldout)))
						&& e.name == name);
				if (matchingOption != null) {
					// Get existing option if it already exists.
					if (self) {
						s_Options[namePath] = matchingOption;
						return (T)matchingOption;
					}
					// If we're here, we know that matchingOption exists and is a DebugToggle.
					parent = matchingOption as DebugFoldout;
				}
				else if (!self) {
					// Create parent option if it doesn't already exist.
					DebugFoldout option = isFoldout ? new DebugFoldout() : new DebugToggle();
					option.name = name;
					option.parent = parent;
					parent.children.Add(option);
					parent = option;
				}
				else {
					// This is the actual new option (if it didn't already exist).
					DebugOption option = new T();
					option.name = name;
					option.parent = parent;
					parent.children.Add(option);
					s_Options[namePath] = option;
					return (T)option;
				}
			}
			return null;
		}
	}
}
