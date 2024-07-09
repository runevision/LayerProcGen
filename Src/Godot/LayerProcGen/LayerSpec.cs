#if GODOT4
using Godot;
using Godot.Collections;

namespace Runevision.LayerProcGen;

[GlobalClass]
public partial class LayerSpec : Resource
{
    public StringName layerClassName;
    [Export] public Color color = Colors.White;

    public override Variant _Get(StringName property)
    {
        return (string)property switch
        {
            nameof(layerClassName) => layerClassName ?? string.Empty,
            _ => base._Get(property)
        };
    }

    public override bool _Set(StringName property, Variant value)
    {
        switch (property)
        {
            case nameof(layerClassName):
                layerClassName = value.AsStringName();
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
                { "name", nameof(layerClassName) },
                { "type", (int)Variant.Type.StringName },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", GenerationSource.FillLayerHintString() }
            }
        };

        return properties;
    }
}
#endif
