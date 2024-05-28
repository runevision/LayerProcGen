using System.Linq;
using Godot;
using Godot.Collections;
using Runevision.Common;
using AppDomain = System.AppDomain;
using Type = System.Type;

namespace Runevision.LayerProcGen;

[Tool]
public partial class GenerationSource
{
    static string[] layerTypeStrings;

    [Export]
    private string Layer
    {
        get => layer.className;
        set => layer.className = value;
    }

    [Export]
    private Vector2 Size
    {
        get => size;
        set => size = (Point)value;
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "Layer" },
                { "type", (int)Variant.Type.StringName },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", FillLayerHintString() }
            }
        };

        return properties;
    }

    public static string FillLayerHintString()
    {
        if (layerTypeStrings == null)
        {
            var layerBaseType = typeof(AbstractChunkBasedDataLayer);
            layerTypeStrings = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t != layerBaseType && layerBaseType.IsAssignableFrom(t) && !t.IsGenericType)
                .Select(t => t.FullName)
                .ToArray();
        }

        return string.Join(',', layerTypeStrings.Select(s => s[(s.LastIndexOf('.') + 1)..]));
    }
}