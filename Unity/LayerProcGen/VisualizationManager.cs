/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using Runevision.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runevision.LayerProcGen {

	public interface ILayerVisualization {
		void VisualizationUpdate();
	}

	/// <summary>
	/// A manager for displaying various visualizations of data layers.
	/// </summary>
	/// <remarks>
	/// The manager can visualize chunk bounds, layer bounds, layer dependencies,
	/// and has hooks for letting you display your own debug layer visualizations.
	/// 
	/// <para>To use it, add the VisualizationManager component to a GameObject in Unity.
	/// For each layer you want to visualize, add an entry in the Layers list where you
	/// select the layer in the dropdown, and choose a color for that layer.</para>
	/// 
	/// <para>When the game is running, the VisualizationManager is controlled via
	/// <see cref="DebugOption">DebugOptions</see>. In the debug options you need to both
	/// enable the <c>LayerVis</c> toggle and each layer you want visualized under the
	/// <c>Layers</c> foldout.</para>
	/// </remarks>
	public class VisualizationManager : MonoBehaviour {

		public static VisualizationManager instance;

		public DebugToggle debugChunkBounds = DebugToggle.Create("LayerVis/Chunk Bounds", true);
		public DebugToggle debugLayerBounds = DebugToggle.Create("LayerVis/Layer Bounds", true);
		public DebugToggle debugExactLayerBounds = DebugToggle.Create("LayerVis/Exact Layer Bounds", false);
		public DebugToggle debugSources = DebugToggle.Create("LayerVis/Generation Sources", false);
		public DebugToggle debugSeparate = DebugToggle.Create("LayerVis/Separate Layers", false);
		public DebugToggle debugLayerLabels = DebugToggle.Create("LayerVis/Separate Layers/Layer Labels", true);
		public DebugToggle debugDependencies = DebugToggle.Create("LayerVis/Separate Layers/Dependencies", true);
		public DebugButton debugToggleAllLayers = DebugButton.Create(">Layers/Toggle All");

		public class LayerLevelVis {
			public readonly IChunkBasedDataLayer layer;
			public readonly int level;
			public string name;
			public GUIContent nameContent;
			public float depthValue;
			public float depthValueRaw;
			public Vector2 center;
			public Vector2 size;
			public DebugToggle debugLayerLevel;
			public DebugToggle debugLayer;
			public LayerSpec layerSpec;
			public Color color { get { return layerSpec.color; } }

			public LayerLevelVis(IChunkBasedDataLayer layer, int level, LayerSpec layerSpec) {
				this.layer = layer;
				this.level = level;
				name = layer.GetType().PrettyName();
				if (((AbstractChunkBasedDataLayer)layer).GetLevelCount() > 1)
					name += " " + level;
				nameContent = new GUIContent(name);
				this.layerSpec = layerSpec;
				string debugPath = ">Layers/" + layer.GetType().Name + "/>Levels/Level " + level;
				debugLayerLevel = DebugToggle.Create(debugPath , true);
				debugLayer = debugLayerLevel.parent.parent as DebugToggle;
				if (layer.GetLevelCount() <= 1) {
					debugLayerLevel.hidden = true;
					debugLayerLevel.parent.hidden = true;
				}
			}
		}

		[Serializable]
		public class LayerSpec {
			public LayerNamedReference layerClassName;
			public Color color = Color.white;
		}

		public List<LayerSpec> layers = new List<LayerSpec>();

		public enum SeparationUnit { WorldUnits, Chunks }
		
		[Header("Layer Spacing")]
		public float worldOffset;
		public SeparationUnit spacingUnit = SeparationUnit.Chunks;
		public float spacingAmount = 4;

		List<LayerLevelVis> layerLevels;

		void Start() {
			layerLevels = new List<LayerLevelVis>();
			foreach (LayerSpec spec in layers) {
				AddLayer(spec);
			}
			debugSeparate.animValueDuration = 0.5f;
			instance = this;

			debugToggleAllLayers.Callback += () => {
				bool allEnabled = true;
				foreach (LayerLevelVis vis in layerLevels) {
					if (!vis.debugLayer.enabledSelf) {
						allEnabled = false;
						break;
					}
				}
				foreach (LayerLevelVis vis in layerLevels) {
					vis.debugLayer.SetEnabled(!allEnabled);
				}
			};
		}

		#if UNITY_EDITOR
		void OnEnable() {
			UnityEditor.SceneView.duringSceneGui += OnDuringSceneGui;
		}

		void OnDisable() {
			UnityEditor.SceneView.duringSceneGui -= OnDuringSceneGui;
		}
		#endif

		void AddLayer(LayerSpec layerSpec) {
			IChunkBasedDataLayer layer = (IChunkBasedDataLayer)layerSpec.layerClassName.GetLayerInstance();
			if (layer == null)
				return;
			for (int i = layer.GetLevelCount() - 1; i >= 0; i--) {
				layerLevels.Add(new LayerLevelVis(layer, i, layerSpec));
			}
		}

		LayerLevelVis GetVisualizationInfo(IChunkBasedDataLayer layer, int level) {
			foreach (var vis in instance.layerLevels) {
				if (vis.layer == layer && vis.level == level)
					return vis;
			}
			return null;
		}

		public static Matrix4x4 BeginDebugDraw(IChunkBasedDataLayer layer, int level) {
			if (instance == null)
				return Matrix4x4.identity;
			var vis = instance.GetVisualizationInfo(layer, level);
			if (vis == null)
				return Matrix4x4.identity;
			float depth = vis.depthValueRaw;
			Matrix4x4 matrix = Matrix4x4.identity;
			float animValue = instance.debugSeparate.animValue;
			if (DebugDrawer.xzMode) {
				matrix.m11 = 1f - animValue;
				matrix.m13 = depth * animValue;
			}
			else {
				matrix.m22 = 1f - animValue;
				matrix.m23 = depth * animValue;
			}
			DebugDrawer.matrix = matrix;
			return matrix;
		}

		public static void EndDebugDraw() {
			DebugDrawer.matrix = Matrix4x4.identity;
		}

		void DebugDrawBoundsOfLevel(AbstractLayerChunk chunk, float depth, Color color, float padding) {
			Color col = color;
			DebugDrawer.DrawRect(
				chunk.bounds.min + Vector2.one * padding,
				chunk.bounds.max - Vector2.one * padding,
				depth, col);
		}

		void Update() {
			// Draw top layer dependencies / generation sources.
			if (debugSources.visible) {
				List<TopLayerDependency> topDependencies = LayerManager.instance.topDependencies;
				Color col = Color.white;
				col.a = debugSources.animAlpha;
				foreach (TopLayerDependency topDep in topDependencies) {
					Vector2 min = topDep.focus - (Vector2)topDep.size * 0.5f;
					Vector2 max = min + (Vector2)topDep.size;
					DebugDrawer.DrawRect(min, max, 0f, col);

					if (debugDependencies.visible) {
						var depVis = GetVisualizationInfo(topDep.layer, topDep.level);
						if (depVis == null || !depVis.debugLayerLevel.visible)
							continue;

						Color depCol = col;
						depCol.a *= debugDependencies.animAlpha;
						depCol.a *= depVis.debugLayerLevel.animAlpha;
						DrawBoxDependency(min, max, 0, 0, 0, depVis.depthValue, depCol, true);
					}
				}
			}
			
			// Draw layers.
			float layerSeparation = debugSeparate.animValue;
			float sign = DebugDrawer.xzMode ? -1 : 1;
			float signedSpacingAmount = spacingAmount * sign;
			float depth = worldOffset * sign;
			IChunkBasedDataLayer lastLayer = null;
			float lastChunkSize = 0;
			float lastLayerAnimValue = 0;
			for (int i = 0; i < layerLevels.Count; i++) {
				LayerLevelVis vis = layerLevels[i];
				if (spacingUnit == SeparationUnit.WorldUnits) {
					depth += vis.debugLayerLevel.animValue * signedSpacingAmount;
				}
				else {
					float currentChunkSize = Mathf.Max(vis.layer.chunkW, vis.layer.chunkH);
					float currentLayerAnimValue = vis.debugLayerLevel.animValue;
					float maxChunkSize = Mathf.Max(currentChunkSize, lastChunkSize);
					maxChunkSize = Mathf.Lerp(currentChunkSize, maxChunkSize, lastLayerAnimValue);
					depth += currentLayerAnimValue * maxChunkSize * signedSpacingAmount;
					lastChunkSize = currentChunkSize;
					lastLayerAnimValue = currentLayerAnimValue;
				}
				vis.depthValueRaw = depth;
				vis.depthValue = vis.depthValueRaw * layerSeparation;

				UpdateLayer(vis.layer, vis.level);

				if (vis.layer != lastLayer) {
					lastLayer = vis.layer;
					if (vis.layer is ILayerVisualization layerVisualization)
						layerVisualization.VisualizationUpdate();
				}
			}
		}

		void UpdateLayer(IChunkBasedDataLayer layer, int level) {
			var vis = GetVisualizationInfo(layer, level);
			if (!vis.debugLayerLevel.visible)
				return;

			// Calculate center and size.
			Point chunkSize = layer.chunkSize;
			Point minIndex = new Point(int.MaxValue, int.MaxValue);
			Point maxIndex = new Point(int.MinValue, int.MinValue);
			bool any = false;
			layer.HandleAllAbstractChunks(level, c => {
				minIndex = Point.Min(minIndex, c.index);
				maxIndex = Point.Max(maxIndex, c.index);
				any = true;
			});
			if (!any)
				return;

			vis.center = (Vector2)((minIndex + maxIndex + Point.one) * chunkSize) * 0.5f;
			vis.size = (Vector2)((maxIndex + Point.one - minIndex) * chunkSize);

			// Get layer color and opacity.
			float alpha = vis.debugLayerLevel.animAlpha;
			Color col = vis.color;
			col.a = alpha;

			// Draw chunk boundaries and calculate index bounds.
			if (debugChunkBounds.visible) {
				float depth = vis.depthValue;
				Color chunkCol = col;
				chunkCol.a *= debugChunkBounds.animValue * 0.3f;
				layer.HandleAllAbstractChunks(level, c => {
					DebugDrawBoundsOfLevel(c, depth, chunkCol, 0f);
				});
			}

			// Draw layer bounds, either exact or bounding box.
			Color colExact = col;
			colExact.a *= debugExactLayerBounds.animAlpha;
			if (colExact.a > 0f) {
				DrawExactLayerBounds(vis, layer, level, colExact);
			}
			Color colBox = col;
			colBox.a *= 1f - debugExactLayerBounds.animAlpha;
			if (colBox.a > 0f) {
				DrawBoxLayerBounds(vis, layer, level, colBox);
			}
		}

		void DrawBoxLayerBounds(LayerLevelVis vis, IChunkBasedDataLayer layer, int level, Color col) {
			float depth = vis.depthValue;
			
			Vector3 min = vis.center - vis.size * 0.5f;
			Vector3 max = vis.center + vis.size * 0.5f;

			// Draw layer dependencies.
			if (debugDependencies.visible) {
				Color depCol = col;
				depCol.a *= debugDependencies.animAlpha;

				// Draw dependencies on other layers.
				layer.HandleDependenciesForLevel(level, dep => {
					var depVis = GetVisualizationInfo((IChunkBasedDataLayer)dep.layer, dep.level);
					DrawBoxDependency(vis, depVis, min, max, dep.hPadding, dep.vPadding, depCol, true);
				});

				// Draw internal dependency on previous level of own layer.
				if (level > 0) {
					var depVis = GetVisualizationInfo(layer, level - 1);
					DrawBoxDependency(vis, depVis, min, max, layer.chunkW, layer.chunkH, depCol, false);
				}
			}

			// Draw layer bounds.
			if (debugLayerBounds.visible) {
				Color layerCol = col;
				layerCol.a *= debugLayerBounds.animAlpha;
				DebugDrawer.DrawRect(min, max, depth, layerCol);
			}
		}

		// Temporary structures used in DrawExactLayerBounds.
		static Dictionary<Point, byte> borderDict = new Dictionary<Point, byte>();
		static HashSet<Point> usedCorners = new HashSet<Point>();
		static List<Vector2> corners = new List<Vector2>();
		static readonly Point[] dirs = new Point[] { Point.right, Point.up, -Point.right, -Point.up };

		void SetBorderBit(int bit, Point p) {
			byte value = 0;
			borderDict.TryGetValue(p, out value);
			value |= (byte)(1 << bit);
			borderDict[p] = value;
		}
		
		void DrawExactLayerBounds(LayerLevelVis vis, IChunkBasedDataLayer layer, int level, Color col) {
			borderDict.Clear();
			usedCorners.Clear();
			corners.Clear();

			layer.HandleAllAbstractChunks(level, c => {
				Point p = c.index;
				SetBorderBit(0, p); // Bottom left corner
				SetBorderBit(1, p + Point.right); // Bottom right corner
				SetBorderBit(2, p + Point.one); // Top right corner
				SetBorderBit(3, p + Point.up); // Top left corner
			});

			Point chunkSize = layer.chunkSize;
			foreach (var kvp in borderDict) {
				if (usedCorners.Contains(kvp.Key))
					continue;
				byte val = kvp.Value;
				for (int testDir = 0; testDir < 4; testDir++) {
					if (val == (1 << testDir)) {
						int dir = testDir;
						Point p = kvp.Key;
						Point start = p;
						Point offset = dirs[dir];
						int count = 0;
						Point cornerDir = dirs[(dir + 3) % 4] - offset;
						Point lastCornerDir = cornerDir;
						while (count < 1000) {
							Point lastCorner = p;
							count++;
							usedCorners.Add(p);
							int numConvex = 1 << ((dir + 1) % 4);
							int numDouble = (1 << ((dir + 3) % 4)) + numConvex;
							int numConcave = 1 + 2 + 4 + 8 - (1 << ((dir + 2) % 4));
							while (count < 1000) {
								count++;
								p += offset;
								if (!borderDict.TryGetValue(p, out val)) {
									Logg.LogError("Border point " + p + " not in dictionary");
									break;
								}
								if (val == numConcave || val == numDouble) {
									dir = (dir + 3) % 4;
									cornerDir = dirs[dir] - offset;
									offset = dirs[dir];
									break;
								}
								if (val == numConvex) {
									dir = (dir + 1) % 4;
									cornerDir = offset - dirs[dir];
									offset = dirs[dir];
									break;
								}
							}
							corners.Add((Vector2)lastCorner * chunkSize);
							corners.Add((Vector2)lastCornerDir);
							corners.Add((Vector2)p * chunkSize);
							corners.Add((Vector2)cornerDir);
							lastCornerDir = cornerDir;
							if (p == start)
								break;
						}
					}
				}
			}

			// Draw layer dependencies.
			if (debugDependencies.visible) {
				Color depCol = col;
				depCol.a *= debugDependencies.animAlpha;

				// Draw dependencies on other layers.
				layer.HandleDependenciesForLevel(level, dep => {
					var depVis = GetVisualizationInfo((IChunkBasedDataLayer)dep.layer, dep.level);
					DrawExactDependency(vis, depVis, corners, dep.hPadding, dep.vPadding, depCol, true);
				});

				// Draw internal dependency on previous level of own layer.
				if (level > 0) {
					var depVis = GetVisualizationInfo(layer, level - 1);
					DrawExactDependency(vis, depVis, corners, layer.chunkW, layer.chunkH, depCol, false);
				}
			}

			// Draw layer bounds.
			float depth = vis.depthValue;
			for (int i = 0; i < corners.Count; i += 4) {
				Vector3 p1 = corners[i + 0];
				Vector3 p2 = corners[i + 2];
				p1.z = p2.z = depth;
				DebugDrawer.DrawLine(FlipYZ(p1), FlipYZ(p2), col);
			}
		}

		static void DrawBoxDependency(
			LayerLevelVis vis,
			LayerLevelVis depVis,
			Vector3 min, Vector3 max, int hPadding, int vPadding, Color col, bool drawProjection
		) {
			if (depVis == null || !depVis.debugLayerLevel.visible)
				return;
			float depth = vis.depthValue;
			float depthDif = (depVis.depthValue - vis.depthValue);
			Color dCol = col;
			dCol.a *= depVis.debugLayerLevel.animAlpha;
			DrawBoxDependency(min, max, hPadding, vPadding, depth, depthDif, dCol, drawProjection);
		}
		
		static void DrawBoxDependency(
			Vector3 min, Vector3 max, int hPadding, int vPadding,
			float depth, float depthDif, Color col, bool drawProjection
		) {
			col.a *= 0.5f;

			// Draw lines connecting the two layers.
			DebugDrawer.DrawRay(
				FlipYZ(new Vector3(min.x, min.y, depth)),
				FlipYZ(new Vector3(-hPadding, -vPadding, depthDif)), col);
			DebugDrawer.DrawRay(
				FlipYZ(new Vector3(max.x, min.y, depth)),
				FlipYZ(new Vector3(hPadding, -vPadding, depthDif)), col);
			DebugDrawer.DrawRay(
				FlipYZ(new Vector3(min.x, max.y, depth)),
				FlipYZ(new Vector3(-hPadding, vPadding, depthDif)), col);
			DebugDrawer.DrawRay(
				FlipYZ(new Vector3(max.x, max.y, depth)),
				FlipYZ(new Vector3(hPadding, vPadding, depthDif)), col);

			// Draw layers projecting dependency area onto depended on layer.
			if (drawProjection) {
				min -= new Vector3(hPadding, vPadding, 0f);
				max += new Vector3(hPadding, vPadding, 0f);
				depth += depthDif;
				DebugDrawer.DrawRect(min, max, depth, col);
			}
		}

		static void DrawExactDependency(
			LayerLevelVis vis,
			LayerLevelVis depVis,
			List<Vector2> corners, int hPadding, int vPadding, Color col, bool drawProjection
		) {
			if (depVis == null || !depVis.debugLayerLevel.visible)
				return;
			float depth = vis.depthValue;
			float depthDif = (depVis.depthValue - vis.depthValue);
			Color dCol = col;
			dCol.a *= depVis.debugLayerLevel.animAlpha;
			DrawExactDependency(corners, hPadding, vPadding, depth, depthDif, dCol, drawProjection);
		}

		static void DrawExactDependency(
			List<Vector2> corners, int hPadding, int vPadding,
			float depth, float depthDif, Color col, bool drawProjection
		) {
			col.a *= 0.5f;
			for (int i = 0; i < corners.Count; i += 4) {
				Vector3 p1 = corners[i + 0];
				Vector3 p2 = corners[i + 2];
				p1.z = p2.z = depth;
				Vector3 v1 = corners[i + 1];
				v1.x *= hPadding;
				v1.y *= vPadding;
				v1.z = depthDif;
				Vector3 v2 = corners[i + 3];
				v2.x *= hPadding;
				v2.y *= vPadding;
				v2.z = depthDif;
				// Draw lines connecting the two layers.
				DebugDrawer.DrawRay(FlipYZ(p1), FlipYZ(v1), col);
				// Draw layers projecting dependency area onto depended on layer.
				if (drawProjection)
					DebugDrawer.DrawLine(FlipYZ(p1 + v1), FlipYZ(p2 + v2), col);
			}
		}

		static Vector3 FlipYZ(Vector3 v) {
			return DebugDrawer.xzMode ? new Vector3(v.x, v.z, v.y) : v;
		}

		static GUIStyle labelStyle;

		void OnGUI() {
			DrawLayerLabels(Camera.main, DrawText);
		}

		void DrawLayerLabels(Camera cam, Action<LayerLevelVis, Vector3, Color, GUIStyle, Vector2, Vector2> labelMethod) {
			if (cam == null || layerLevels == null)
				return;

			if (!debugLayerLabels.visible)
				return;

			// Fade out labels when they would camera is looking in direction of depth axis.
			Vector3 depthAxis = DebugDrawer.xzMode ? Vector3.up : Vector3.forward;
			float angle = Mathf.Abs(90f - Vector3.Angle(cam.transform.forward, depthAxis));
			float angleAlpha = Mathf.InverseLerp(55f, 45f, angle);
			if (angleAlpha == 0f)
				return;

			if (labelStyle == null) {
				labelStyle = new GUIStyle();
				labelStyle.normal.textColor = Color.white;
				labelStyle.hover.textColor = labelStyle.normal.textColor;
				labelStyle.padding = new RectOffset(0, 0, 3, 6);
			}

			Vector3 right = cam.transform.right.normalized;
			float planeExtendingRight = Vector3.Cross(depthAxis, right).magnitude;
			Vector2 screenOffset = Vector2.right * 10f;
			Vector2 textPivot = new Vector2(0f, 0.5f);
			
			for (int i = 0; i < layerLevels.Count; i++) {
				LayerLevelVis vis = layerLevels[i];
				if (!vis.debugLayerLevel.visible)
					continue;
				Vector3 center = vis.center;
				center.z = vis.depthValue;
				center = FlipYZ(center);
				Vector3 size = FlipYZ(vis.size);
				Color col = vis.color;
				col.a = vis.debugLayerLevel.animAlpha * debugLayerLabels.animAlpha * angleAlpha;
				col.a *= col.a;
				float sizeAlongRight = Vector3.Scale(right, size).magnitude * 0.5f;
				float sizeMagnitude = size.magnitude * 0.15f;
				Vector3 pos = center + right * (sizeAlongRight + sizeMagnitude) * planeExtendingRight;
				labelMethod(vis, pos, col, labelStyle, screenOffset, textPivot);
			}
		}

		static void DrawText(
			LayerLevelVis vis, Vector3 worldPos, Color color, GUIStyle labelStyle,
			Vector2 screenOffset = default, Vector2 pivot = default
		) {
			Vector3 screenPosRaw = Camera.main.WorldToScreenPoint(worldPos);
			if (screenPosRaw.z < 0f)
				return;
			Vector2 screenPos = screenPosRaw;
			screenPos.y = -screenPos.y + Screen.height;
			screenPos += screenOffset;
			if (screenPos.y < 0f || screenPos.y > Screen.height ||
				screenPos.x < 0f || screenPos.x > Screen.width
			)
				return;

			GUIUtility.ScaleAroundPivot(Vector2.one * 2f, screenPos);
			Vector2 size = labelStyle.CalcSize(vis.nameContent);
			screenPos -= Vector2.Scale(size, pivot);
			Color restoreColor = GUI.color;
			GUI.color = color;
			GUI.Label(new Rect(screenPos.x, screenPos.y, size.x, size.y), vis.nameContent, labelStyle);
			GUI.color = restoreColor;
			GUI.matrix = Matrix4x4.identity;
		}

		#if UNITY_EDITOR
		void OnDuringSceneGui(UnityEditor.SceneView sceneView) {
			UnityEditor.Handles.BeginGUI();
			DrawLayerLabels(sceneView.camera, DrawSceneViewTextMethod);
			UnityEditor.Handles.EndGUI();
		}

		static void DrawSceneViewTextMethod(
			LayerLevelVis vis, Vector3 worldPos, Color color, GUIStyle labelStyle,
			Vector2 screenOffset = default, Vector2 pivot = default
		) {
			UnityEditor.SceneView view = UnityEditor.SceneView.currentDrawingSceneView;
			Vector3 screenPosRaw = view.camera.WorldToScreenPoint(worldPos);
			if (screenPosRaw.z < 0f)
				return;
			Vector2 screenPos = UnityEditor.EditorGUIUtility.PixelsToPoints(screenPosRaw);
			#if UNITY_2022_2_OR_NEWER
			screenPos += view.position.size - view.cameraViewport.size;
			#else
			screenPos += new Vector2(0, 20);
			#endif
			screenPos.y = -screenPos.y + view.position.height;
			screenPos += screenOffset;
			if (screenPos.y < 0f || screenPos.y > Screen.height ||
				screenPos.x < 0f || screenPos.x > Screen.width
			)
				return;

			Vector2 size = labelStyle.CalcSize(vis.nameContent);
			screenPos -= Vector2.Scale(size, pivot);
			Color restoreColor = GUI.color;
			GUI.color = color;
			Rect position = new Rect(screenPos.x, screenPos.y, size.x, size.y);
			if (GUI.Button(position, vis.nameContent, labelStyle)) {
				Vector3 boundsCenter = vis.center;
				boundsCenter.z = vis.depthValue;
				Vector3 boundsSize = vis.size;
				Bounds bounds = new Bounds(FlipYZ(boundsCenter), FlipYZ(boundsSize));
				view.Frame(bounds, false);
			}
			GUI.color = restoreColor;
		}
		#endif
	}

}
