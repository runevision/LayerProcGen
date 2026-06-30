/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#if GODOT4

using System.Collections.Generic;
using Godot;

namespace Runevision.Common {

	/// <summary>
	/// A utility for drawing lines for debug visualizations.
	/// </summary>
	/// <remarks>
	/// Functions that take Vector3 parameters draw in 3D space. Functions that take
	/// Vector2 parameters draw in either the XY or XZ plane depending on whether
	/// the xzMode property is set to true.
	/// </remarks>
	public partial class DebugDrawer : Node {
		struct Line {
			public Vector3 start;
			public Vector3 end;
			public Color color;
			public float startTime;
			public float duration;

			public Line(Vector3 start, Vector3 end, Color color, float startTime, float duration) {
				this.start = start;
				this.end = end;
				this.color = color;
				this.startTime = startTime;
				this.duration = duration;
			}

			public bool DurationElapsed() {
				return s_LastTime - startTime >= duration;
			}

			public void Draw() {
				DebugDraw3D.DrawLine(start, end, color, duration);
			}
		}

		// Methods that take Vector2 or Rect will draw in xz plane if true, otherwise xy plane.
		public static bool xzMode = false;

		static DebugDrawer s_Instance;
		static ShaderMaterial s_MatZOn;
		static ShaderMaterial s_MatZOff;
		static float s_LastTime = -1;

		[Export]
		public bool display = true;
		[Export(PropertyHint.Layers3DPhysics)]
		public long debugLayers = ~0;
		[Export]
		public Shader shaderZOn;
		[Export]
		public Shader shaderZOff;

		public static float alpha = 1f;
		public static Projection matrix = Projection.Identity;

		static Gradient.ColorSpace colorSpace;
		// List<Line> linesZOn;
		// List<Line> linesZOff;
		List<Line> linesZOnMultiFrame;
		List<Line> linesZOffMultiFrame;

		public override void _EnterTree() {
			if (s_Instance != null) {
				this.QueueFree();
				// DestroyImmediate(this);
				return;
			}
			s_Instance = this;
			SetMaterial();
			// linesZOn = new List<Line>();
			// linesZOff = new List<Line>();
			linesZOnMultiFrame = new List<Line>();
			linesZOffMultiFrame = new List<Line>();
			colorSpace = Gradient.ColorSpace.LinearSrgb;
			//  TODO: colorSpace = QualitySettings.activeColorSpace;
		}

		void SetMaterial() {
			s_MatZOn = new ShaderMaterial();
			s_MatZOn.Shader = shaderZOn;
			// s_MatZOn.hideFlags = HideFlags.HideAndDontSave; //TODO
			s_MatZOff = new ShaderMaterial();
			s_MatZOff.Shader = shaderZOff;
			// s_MatZOff.hideFlags = HideFlags.HideAndDontSave; //TODO
		}

		public override void _Process(double deltaTime) {
			// linesZOn.Clear();
			// linesZOff.Clear();
			CullList(linesZOnMultiFrame);
			CullList(linesZOffMultiFrame);
			s_LastTime = (float)deltaTime;
		}

		void CullList(List<Line> list) {
			lock (list) {
				for (int i = list.Count - 1; i >= 0; i--) {
					if (list[i].DurationElapsed()) {
						list[i] = list[^1];
						list.RemoveAt(list.Count - 1);
					}
				}
			}
		}

		void OnRenderObject() {
			if (!display)
				return;
			// if ((Camera.current.cullingMask & debugLayers) == 0) //TODO
			// 	return;

			 s_MatZOn.Shader.Set("Pass",0); //TODO: s_MatZOn.SetPass(0);
			// GL.Begin(GL.LINES);
			// DrawList(linesZOn);
			DrawList(linesZOnMultiFrame);
			// GL.End();

			s_MatZOff.Shader.Set("Pass",0); //TODO: s_MatZOff.SetPass(0);
			// GL.Begin(GL.LINES);
			// DrawList(linesZOff);
			DrawList(linesZOffMultiFrame);
			// GL.End();
		}

		void DrawList(List<Line> lines) {
			lock (lines) {
				for (int i = lines.Count - 1; i >= 0; i--)
					lines[i].Draw();
			}
		}

		public static void DrawLine(Vector3 start, Vector3 end) {
			DrawLine(start, end, Colors.White);
		}

		public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0, bool depthTest = false) {
			//UnityEngine.Debug.DrawLine (start, end, color, duration, depthTest);
			if (duration == 0 && !s_Instance.display)
				return;
			if (start == end)
				return;

			color.A *= alpha;
			if (colorSpace == Gradient.ColorSpace.LinearSrgb)
				color.A *= color.A;
			start = matrix.MultiplyPoint3x4(start);
			end = matrix.MultiplyPoint3x4(end);
			if (duration <= 0)
			{
				DebugDraw3D.DrawLine(start, end, color, duration);

				// if (depthTest) //TODO: not implemented by DebugDraw3D, yet: https://github.com/DmitriySalnikov/godot_debug_draw_3d/issues/44
				// {
				// 	DebugDraw3D.DrawLine(start, end, color, duration);
					// lock (s_Instance.linesZOn) {
					// 	s_Instance.linesZOn.Add(new Line(start, end, color, s_LastTime, duration));
					// }
				// }
				// else {
					// lock (s_Instance.linesZOff)
					// {
					// 	s_Instance.linesZOff.Add(new Line(start, end, color, s_LastTime, duration));
					// }
				// }
			}
			else {
				if (depthTest) {
					lock (s_Instance.linesZOnMultiFrame) {
						s_Instance.linesZOnMultiFrame.Add(new Line(start, end, color, s_LastTime, duration));
					}
				}
				else {
					lock (s_Instance.linesZOffMultiFrame) {
						s_Instance.linesZOffMultiFrame.Add(new Line(start, end, color, s_LastTime, duration));
					}
				}
			}
		}

		// Draw a line from start to start + dir with color for a duration of time and with or without depth testing.
		// If duration is 0 then the ray is rendered 1 frame.
		public static void DrawRay(Vector3 start, Vector3 dir) {
			DrawLine(start, start + dir, Colors.White);
		}

		public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration = 0, bool depthTest = false) {
			if (dir == Vector3.Zero)
				return;
			DrawLine(start, start + dir, color, duration, depthTest);
		}

		public static void DrawRect(float xMin, float yMin, float xMax, float yMax, Color color) {
			DrawRect(new Vector2(xMin, yMin), new Vector2(xMax, yMax), 0, color);
		}

		public static void DrawRect(Vector2 min, Vector2 max, float depth, Color color) {
			if (xzMode) {
				DrawLine(
					new Vector3(min.X, depth, min.Y),
					new Vector3(min.X, depth, max.Y),
					color
				);
				DrawLine(
					new Vector3(max.X, depth, min.Y),
					new Vector3(max.X, depth, max.Y),
					color
				);
				DrawLine(
					new Vector3(min.X, depth, min.Y),
					new Vector3(max.X, depth, min.Y),
					color
				);
				DrawLine(
					new Vector3(min.X, depth, max.Y),
					new Vector3(max.X, depth, max.Y),
					color
				);
			}
			else {
				DrawLine(
					new Vector3(min.X, min.Y, depth),
					new Vector3(min.X, max.Y, depth),
					color
				);
				DrawLine(
					new Vector3(max.X, min.Y, depth),
					new Vector3(max.X, max.Y, depth),
					color
				);
				DrawLine(
					new Vector3(min.X, min.Y, depth),
					new Vector3(max.X, min.Y, depth),
					color
				);
				DrawLine(
					new Vector3(min.X, max.Y, depth),
					new Vector3(max.X, max.Y, depth),
					color
				);
			}
		}

		static Vector3 Mode(Vector2 pos) {
			if (xzMode)
				return new Vector3(pos.X, 0, pos.Y);
			else
				return new Vector3(pos.X, pos.Y, 0);
		}

		public static void DrawCross(Vector2 pos, float size, Color color, float duration = 0, bool depthTest = false) {
			DrawCross(Mode(pos), size, color, duration, depthTest);
		}

		public static void DrawCross(Vector3 pos, float size, Color color, float duration = 0, bool depthTest = false) {
			DrawRay(pos + Mode(new Vector2(+1, +1) * size), Mode(new Vector2(-2, -2) * size), color, duration, depthTest);
			DrawRay(pos + Mode(new Vector2(+1, -1) * size), Mode(new Vector2(-2, +2) * size), color, duration, depthTest);
		}

		public static void DrawCircle(Vector2 pos, float radius, int segments, Color color, float duration = 0, bool depthTest = false) {
			Vector2 p1 = Vector2.Right * radius + pos;
			for (int i = 0; i < segments; i++) {
				Vector2 p2 = CirclePoint((i + 1f) / segments) * radius + pos;
				DrawLine(Mode(p1), Mode(p2), color, duration, depthTest);
				p1 = p2;
			}
		}

		public static void DrawCircle(Vector3 pos, float radius, int segments, Color color, float duration = 0, bool depthTest = false) {
			Vector3 p1 = Vector3.Right * radius + pos;
			for (int i = 0; i < segments; i++) {
				Vector3 rot = Vector3.Zero;
				rot[xzMode ? 1 : 2] = (i + 1f) * 360f / segments;
				Vector3 p2 = Quaternion.FromEuler(rot) * Vector3.Right * radius + pos;
				DrawLine(p1, p2, color, duration, depthTest);
				p1 = p2;
			}
		}

		static Vector2 CirclePoint(float fraction) {
			float f = fraction * 2 * Mathf.Pi;
			return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		}

		// Draw an arrow from start to end with color for a duration of time and with or without depth testing.
		// If duration is 0 then the arrow is rendered 1 frame.
		public static void DrawLineArrow(Vector3 start, Vector3 end, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, float duration = 0, bool depthTest = false) {
			DrawArrow(start, end - start, color, arrowHeadLength, arrowHeadAngle, duration, depthTest);
		}

		// Draw an arrow from start to start + dir with color for a duration of time and with or without depth testing.
		// If duration is 0 then the arrow is rendered 1 frame.
		public static void DrawArrow(Vector3 start, Vector3 dir, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20, float duration = 0, bool depthTest = false) {
			if (dir == Vector3.Zero)
				return;
			DrawRay(start, dir, color, duration, depthTest);
			Vector3 right = QuaternionExtensions.LookRotation(dir) * Quaternion.FromEuler(new Vector3(0, 180 + arrowHeadAngle, 0)) * Vector3.Forward;
			Vector3 left = QuaternionExtensions.LookRotation(dir) * Quaternion.FromEuler(new Vector3(0, 180 - arrowHeadAngle, 0)) * Vector3.Forward;
			DrawRay(start + dir, right * arrowHeadLength, color, duration, depthTest);
			DrawRay(start + dir, left * arrowHeadLength, color, duration, depthTest);
		}

		// Draw a square with color for a duration of time and with or without depth testing.
		// If duration is 0 then the square is renderer 1 frame.
		public static void DrawSquare(Vector3 pos, Vector3 scale, Color color, Vector3? rot = null, float duration = 0, bool depthTest = false) {
			
			DrawSquare(ProjectionExtensions.TRS(pos, Quaternion.FromEuler(rot ?? Vector3.Zero), scale), color, duration, depthTest);
		}

		// Draw a square with color for a duration of time and with or without depth testing.
		// If duration is 0 then the square is renderer 1 frame.
		public static void DrawSquare(Projection matrix, Color color, float duration = 0, bool depthTest = false) {
			Vector3
			p1 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, .5f)),
			p2 = matrix.MultiplyPoint3x4(new Vector3(.5f, 0, -.5f)),
			p3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, -.5f)),
			p4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f));

			DrawLine(p1, p2, color, duration, depthTest);
			DrawLine(p2, p3, color, duration, depthTest);
			DrawLine(p3, p4, color, duration, depthTest);
			DrawLine(p4, p1, color, duration, depthTest);
		}

		// Draw a cube with color for a duration of time and with or without depth testing.
		// If duration is 0 then the square is renderer 1 frame.
		public static void DrawCube(Vector3 pos, Vector3 scale, Color color, Vector3? rot = null, float duration = 0, bool depthTest = false) {
			DrawCube(ProjectionExtensions.TRS(pos, Quaternion.FromEuler(rot ?? Vector3.Zero), scale), color, duration, depthTest);
		}

		// Draw a cube with color for a duration of time and with or without depth testing.
		// If duration is 0 then the square is renderer 1 frame.
		public static void DrawCube(Projection matrix, Color color, float duration = 0, bool depthTest = false) {
			Vector3
			down1 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, .5f)),
			down2 = matrix.MultiplyPoint3x4(new Vector3(.5f, -.5f, -.5f)),
			down3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, -.5f)),
			down4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, -.5f, .5f)),
			up1 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, .5f)),
			up2 = matrix.MultiplyPoint3x4(new Vector3(.5f, .5f, -.5f)),
			up3 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, -.5f)),
			up4 = matrix.MultiplyPoint3x4(new Vector3(-.5f, .5f, .5f));

			DrawLine(down1, down2, color, duration, depthTest);
			DrawLine(down2, down3, color, duration, depthTest);
			DrawLine(down3, down4, color, duration, depthTest);
			DrawLine(down4, down1, color, duration, depthTest);

			DrawLine(down1, up1, color, duration, depthTest);
			DrawLine(down2, up2, color, duration, depthTest);
			DrawLine(down3, up3, color, duration, depthTest);
			DrawLine(down4, up4, color, duration, depthTest);

			DrawLine(up1, up2, color, duration, depthTest);
			DrawLine(up2, up3, color, duration, depthTest);
			DrawLine(up3, up4, color, duration, depthTest);
			DrawLine(up4, up1, color, duration, depthTest);
		}
	}

}
#endif
