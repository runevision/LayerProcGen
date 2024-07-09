#if GODOT4
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
    static string[]? layerTypeStrings;

    // [Export] //with export enabled the serialisation works, but not the dynamical layer name
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


    // with _get and _set enabled the "FillLayerHintString()" works, but it looses serialisation after recompiling, so serialisation is broken.
    public override Variant _Get(StringName property) =>
        (property + string.Empty) switch
        {
            nameof(Layer) => layer.className,
            _ => base._Get(property)
        };

    public override bool _Set(StringName property, Variant value)
    {
        switch (property)
        {
            case nameof(Layer):
                layer.className = value+"";
                return true;
            default:
                return base._Set(property, value);
        }
    }

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", nameof(Layer)},
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
                .OfType<string>()
                .ToArray();
        }

        return string.Join(',', layerTypeStrings.Select(s => s[(s.LastIndexOf('.') + 1)..]));
    }
}
#endif
