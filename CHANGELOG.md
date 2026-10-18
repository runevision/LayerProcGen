# Changelog


## 0.5.0 - 2026-10-18

### Added

- Added average function for all point types.
- Added missing Point3 division by int operator.
- Support duration and depth test for DebugDrawer.DrawRect function.
- Added implicit conversions Point <> int2 and Point3 <> int3.
- Support getting the origin at edit time from the FloatingOriginSetter component.

### Changed

- Set generation threads to lowest priority to not interfere with time sensitive frame jobs.
- Made FloatingOriginSetter simpler and more fool proof by always handling own shift if necessary.
- Optimized Mod and Div operations in Crd class.
- Log values that cause OverflowExceptions in DFloat constructors.
- Wrap MainThreadActionQueue execution in try-finally to ensure the timer watch gets reset.

### Fixed

- Fixed DebugDrawer not handle floating origin right for persistent lines.
- Operate on a copy of topDependencies in LayerUpdateThread to avoid deadlocks.


## 0.4.0 - 2026-06-18

### Added

- Added support for floating origin to support large worlds without suffering floating point precision issues.
   - A floating origin entails a difference between the world origin and the current scene origin. The scene origin can be set via the FloatingOrigin class.
   - Added IOriginHandler interface which classes can implement to handle origin shifting.
   - Added a Unity FloatingOriginSetter component, which automatically updates the floating origin when the GameObject it's attached to has moved beyond an increment distance.
   - Added support for specifying world positions for TransformWrapper, which now handle origin shifts automatically.
   - Added DebugDrawer DPoint overloads interpreted as world positions.
- Added virtual Dispose method to chunks which can be used for disposing native resources after all generation has been stopped.
- Added support for enqueueing routines (IEnumerator) on the MainThreadActionQueue.
- Added public DFloat resolution constants.
- Added conversions between DPoint and int2 (Unity.Mathematics)
- Added a debug toggle to disable SimpleProfiler logging, and a button to log pool stats.
- Added DFloat overloads in RandomHash and Crd classes.
- Added multiplication and division operations for DFloat (and DPoint, DPoint3) with itself.
- Added DebugDrawer.DrawCircle overload that takes a rotation parameter.

## Changed

- Removed LayerManager abort Action. Chunks can instead override Dispose method to clean up and free resources.
- Removed implicit conversions from fixed point to floating point coordinates, that is, between float and DFloat, Vector2 and DPoint, Vector3 and DPoint3. This helps prevent accidental reinterpretations between world coordinates and scene coordinates.
- Allow calling SimpleProfiler.Begin without a handle as a no-op.
- Set MainThreadActionQueue budget as float to allow sub-millisecond precision in budget.
- DebugOptions are now listed alphabetically rather than in a non-deterministic order.
- Throw exception in DEBUG if DFloat constructor causes integer overflow.

### Fixed

- Fixed race conditions in DebugOptions and TopDependency.
- Avoided allocations from delegate closures in ChunkBasedDataLayer and VisualizationManager.
- Avoided allocations from creating lists in ChunkBasedDataLayer.
- Ensured MainThreadActionQueue.NextFrame queue is always executed next frame.
- Fixed bug in PrettyName function for non-generic types nested in generic classes.
- Fixed bug where subsequent specifications of the same DebugOption would reset it to its default value.
- Fixed LayerManager.Unload such that subsequent calls are ignored once already aborting.


## 0.3.0 - 2024-12-07

### Fixed

- Fixed bug that chunk level parameter passed to ChunkBasedDataLayer.IsLoadedAtPosition was not being used.
- Fixed regression bug introduced in 0.2.0 which caused the GenerationSource component to not start generating until moved, if initially at position (0,0).


## 0.2.0 - 2024-07-08

### Added

- Made TransformWrapper setting child to same layer as parent optional.
- Made Point and Point3 implement IBinarySerializable.
- Added doc note to SimpleProfiler.

### Changed

- Implemented WorldState system overhaul.
	- Made StateObject be more generic by taking hash value instead of point and type.
	- Made saving and loading global values avoid allocations.
	- Made WorldState store state as dictionaries of raw structs instead of as StateWrappers.

### Fixed

- Fixed bug that the chunk Reset method didn't clear saved state objects.
- Removed erroneous script asset default references added by Unity which Unity logged errors about.
- Made layer visualization animated transitions update unaffected by time-scale.
- Made GenerationSource not set TopLayerDependency position and size unless changed.


## 0.1.1 - 2024-05-27

### Changed

- Rename assembly definition files to match their specified names.

### Fixed

- Fix terrain sample splat handling accessing heights array out of bounds.


## 0.1.0 - 2024-05-23

- Initial public pre-release.
