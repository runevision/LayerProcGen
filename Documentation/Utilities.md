# Utilities

LayerProcGen comes with a handful of utilities. The ones in the Runevision.LayerProcGen namespace relate to layer based generation specifically, while those in the Runevision.Common namespace are more general-purpose.

## LayerProcGen-specific utilities

### Visualization Manager

(Unity-only for now) [VisualizationManager](#Runevision.LayerProcGen.VisualizationManager) is a manager for displaying various visualizations of the data layers.

## General-purpose utilities

### Debug Options

The [DebugOption](#Runevision.Common.DebugOption) class and related classes is a system for quickly specifying debug options. Most of it is Unity-independent (specification and usage in code, and how the options self-assemble into a hierarchy), while the simple code that displays the controls is Unity-specific.

### Object Pools

The [IPool](#Runevision.Common.IPool) interface represents an object pool that poolable objects can be retrieved from and returned to. It's implemented by [ObjectPool](#Runevision.Common.ObjectPool), [ArrayPool](#Runevision.Common.ArrayPool), [Array2DPool](#Runevision.Common.Array2DPool), [Array3DPool](#Runevision.Common.Array3DPool) and [ListPool](#Runevision.Common.ListPool).

### Simple Profiler

[SimpleProfiler](#Runevision.Common.SimpleProfiler) is a simple tool for measuring execution time of generation processes. It's used by the LayerManager.

### Logg

[Logg](#Runevision.Common.Logg) is a simple Unity-independent wrapper for logging messages, warnings and errors.

### Debug Drawer

(Unity-only for now) [DebugDrawer](#Runevision.Common.DebugDrawer) is a utility for drawing lines for debug visualizations.
