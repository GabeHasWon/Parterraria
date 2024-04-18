using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

internal class MemberEditUI(object reference, FieldInfo info) : UIState
{
    public static List<Type> EditableTypes = [typeof(int), typeof(short), typeof(ushort), typeof(byte), typeof(bool), typeof(float), typeof(string), typeof(Vector2), 
        typeof(Enum), typeof(Point)];

    public object GetValue => _info.GetValue(_reference);

    readonly object _reference = reference;
    readonly FieldInfo _info = info;

    public override void OnInitialize()
    {
        UIPanel panel = new()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
        };
        Append(panel);

        UIText text = new(_info.Name)
        {
            Width = StyleDimension.Fill,
            VAlign = 0.5f
        };
        text.OnUpdate += (_) => SetText(text);
        panel.Append(text);

        object value = _info.GetValue(_reference);
        switch (value)
        {
            case int:
            case short:
            case byte:
            case ushort:
                BuildInteger(panel);
                break;

            case Point:
                BuildPoint(panel);
                break;

            default:
                return;
        }
    }

    private void SetText(UIText text)
    {
        object value = _info.GetValue(_reference);
        text.SetText(value switch
        {
            int or short or byte or ushort => _info.Name + ": " + value,
            Enum => _info.Name + ": " + value + $" ({Convert.ToInt32((Enum)value)})",
            Point or Vector2 => _info.Name + $": {value}",
            _ => "[Unknown data type, how'd you get here?]"
        });
    }

    private void BuildPoint(UIPanel panel)
    {
        var incButton = new UIButton<string>("Set")
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
        };

        incButton.OnLeftClick += (_, _) =>
        {
            Main.LocalPlayer.GetModPlayer<EditToolPlayer>().placeDelay = 5;
            Main.LocalPlayer.GetModPlayer<EditToolPlayer>().placingType = new Point();
            Main.LocalPlayer.GetModPlayer<EditToolPlayer>().placeResult = (object value) => _info.SetValue(_reference, (Point)value);
        };
        
        panel.Append(incButton);
    }

    private void BuildInteger(UIPanel panel)
    {
        var incButton = new UIButton<string>("+")
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
        };
        incButton.OnLeftClick += (_, _) => ModifyValue(true);
        panel.Append(incButton);

        var decButton = new UIButton<string>("-")
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
            HAlign = 1f
        };
        decButton.OnLeftClick += (_, _) => ModifyValue(false);
        panel.Append(decButton);

        void ModifyValue(bool increase)
        {
            switch (_info.GetValue(_reference))
            {
                case int integer:
                    _info.SetValue(_reference, integer + (increase ? 1 : -1));
                    break;

                case short shortVal:
                    _info.SetValue(_reference, (short)(shortVal + (increase ? 1 : -1)));
                    break;

                case ushort ushortVal:
                    _info.SetValue(_reference, (ushort)Math.Clamp((short)(ushortVal + (increase ? 1 : -1)), ushort.MinValue, ushort.MaxValue));
                    break;

                case byte byteVal:
                    _info.SetValue(_reference, (byte)MathHelper.Clamp(byteVal + (increase ? 1 : -1), 0, 255));
                    break;

                default:
                    throw null;
            }
        }
    }
}
