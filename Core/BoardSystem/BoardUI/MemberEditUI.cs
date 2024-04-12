using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class MemberEditUI(object reference, FieldInfo info) : UIState
{
    public static List<Type> EditableTypes = [ typeof(int), typeof(bool), typeof(float), typeof(string), typeof(Vector2) ];

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
                BuildInteger(panel);
                break;
            case short:
                BuildInteger(panel);
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
            int or short => _info.Name + ": " + _info.GetValue(_reference),
            Enum => _info.Name + ": " + _info.GetValue(_reference),
            _ => "[Unknown data type, how'd you get here?]"
        });
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

        void ModifyValue(bool increase) => _info.SetValue(_reference, _info.GetValue(_reference) switch
        {
            int integer => integer + (increase ? 1 : -1),
            short shortVal => shortVal + (increase ? 1 : -1),
            _ => throw null,
        });
    }
}
