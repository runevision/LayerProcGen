# Changelog


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
