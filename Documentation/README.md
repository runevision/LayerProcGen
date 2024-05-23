# LayerProcGen

LayerProcGen is a framework that can be used to implement layer-based procedural generation that's **infinite**, **deterministic** and **contextual**. It works out of the box in Unity but can be used in any C#-compatible engine.

**[Documentation](https://runevision.github.io/LayerProcGen/) - [GitHub](https://github.com/runevision/LayerProcGen)**

![](./ContextualTransition.gif)

The framework does not itself include any procedural generation algorithms. At its core, it's a way to keep track of dependencies between generation processes in a powerful spatial way.

> *Generating infinite worlds in chunks is a well-known concept since Minecraft.*
> 
> *However, there is a widespread misconception that the chunk-based approach can’t be used deterministically with algorithms where the surroundings of a chunk would need to affect the chunk itself.*
> 
> *LayerProcGen is designed to help with just that.*

## Features

**Contextual & deterministic**  
A central purpose of the framework is to support contextual generation while staying deterministic. Procedural operations can be performed across chunk boundaries, producing seamless results for context-based operations such as blurring, point relaxation, or path-finding. This is possible by dividing the generation into multiple layers and keeping a strict separation between the input and output of each layer.  
[Contextual Generation](./ContextualGeneration.md)

**Plan at scale with intent**  
Chunks in one layer can be orders of magnitude larger than chunks in another layer, and you can design them to operate at different levels of abstraction. You can use top-down planning to e.g. have road signs point to distant locations, unlock entire regions based on player progress, or have NPCs talk about things at the other side of the continent.  
[Planning at Scale](./PlanningAtScale.md)

**Bring your own algorithms**  
You implement data layers by creating pairs of layer and chunk classes, and you can use whichever generation techniques you want there, as long as they are suitable for generation in chunks on the fly.  
[Layers and Chunks](./LayersAndChunks.md)

**Handles dependencies**  
The framework makes it possible to build many different chunk-based procedural data layers with dependencies between each other. It automatically generates depended on chunks when they are needed by chunks in other layers, or by top level requirements.  
[Layer Dependencies](./LayerDependencies.md)

**Two-dimensional infinity**  
The framework arranges chunks in either a horizontal or vertical plane. It can be used for 2D or 3D worlds, but 3D worlds can only extend infinitely in two dimensions, similar to Minecraft. The infinity is pseudo-infinite, as it is limited by the range of 32-bit integer numbers and the specifics of which calculations you use in your procedural processes.

**Multi-threaded**  
The framework is multi-threaded based on Parallel.ForEach functionality in .Net. The degree of parallelism automatically scales to the number of available cores. When needed, actions can be enqueued to be performed on the main thread.

## Installation in Unity

LayerProcGen requires Unity 2019.4 or later.

Install LayerProcGen as a Unity Package Manager package from this Git URL:

`https://github.com/runevision/LayerProcGen.git#upm`

See [Unity's instructions here](https://docs.unity3d.com/Manual/upm-ui-giturl.html). If you already have Git installed, it's simply these steps:

- Open the Package Manager window via `Window > Package Manager`
- Click the `+` button and choose `Add package from git URL...`
- Paste the URL: `https://github.com/runevision/LayerProcGen.git#upm`
- Click `Add`

### Samples

You can also import samples for how to use the framework on the `Samples` tab of the package.

The *Simple Samples* have no special requirements.

The *Terrain Sample* requires:

- Burst package
- Input System package
- Player Settings: Enabling `Allow unsafe code`
- Player Settings: Setting `Active Input Handling` to `Both`
- Only tested with Builtin Render Pipeline

## Platform support

The functionality should in general work on all platforms.

The save-state functionality depends on the open-source FBPP (File Based Player Prefs) solution, which has been tested on Windows, MacOS, Linux, iOS and Android. Extending it to work on other platforms will likely be straightforward for someone porting a game for those platforms.

## License

LayerProcGen is licensed under the [Mozilla Public License, v. 2.0](https://mozilla.org/MPL/2.0/).

You can read a summary [here](https://choosealicense.com/licenses/mpl-2.0/). In short: If you make changes/improvements to LayerProcGen, you must share those for free with the community. But the rest of the source code for your game or application is not subject to this license, so there's nothing preventing you from creating proprietary and commercial games that use LayerProcGen.
