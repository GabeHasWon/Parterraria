using NewBeginnings.Common.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

internal class MemberEditUI(object reference, FieldInfo info, FieldLocalization? localization = null) : UIState
{
    public static List<Type> EditableTypes = [typeof(int), typeof(short), typeof(ushort), typeof(byte), typeof(bool), typeof(float), typeof(string), typeof(Vector2),
        typeof(Enum), typeof(Point)];

    public object GetValue => _info.GetValue(_reference);

    private string DisplayName => Localization.HasValue ? Localization.Value.Name.Value : _info.Name;

    public readonly FieldLocalization? Localization = localization;

    public bool HasBeenEdited = false;

    private UIText _text = null;

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

        _text = new(DisplayName)
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            Top = StyleDimension.FromPixels(12),
            VAlign = 0.5f
        };

        _text.OnUpdate += (self) => SetText(_text);

        panel.Append(_text);

        object value = _info.GetValue(_reference);
        switch (value)
        {
            case int:
            case short:
            case byte:
            case ushort:
            case float:
            case double:
                BuildNumber(panel);
                break;

            case Point or Vector2 or Point16:
                BuildPoint(panel);
                break;

            default:
                return;
        }
    }

    public override void OnDeactivate() => base.OnDeactivate();

    private void SetText(UIText text)
    {
        object value = _info.GetValue(_reference);
        text.SetText(value switch
        {
            int or short or byte or ushort or float => DisplayName + ": " + value,
            Enum => DisplayName + ": " + value + $" ({Convert.ToInt32((Enum)value)})",
            Point or Vector2 => DisplayName + $": {value}",
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
            Main.LocalPlayer.GetModPlayer<EditToolPlayer>().placeResult = value =>
            {
                var point = (Point)value;

                HasBeenEdited = true;

                if (_info.FieldType == typeof(Point))
                    _info.SetValue(_reference, point);
                else if (_info.FieldType == typeof(Point16))
                    _info.SetValue(_reference, new Point16(point.X, point.Y));
                else if (_info.FieldType == typeof(Vector2))
                    _info.SetValue(_reference, point.ToWorldCoordinates());
            };
        };

        panel.Append(incButton);
    }

    private void BuildNumber(UIPanel panel)
    {
        var incButton = new UIButton<string>("-")
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
        };
        incButton.OnLeftClick += (_, _) => ModifyValue(false);
        panel.Append(incButton);

        var decButton = new UIButton<string>("+")
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
            HAlign = 1f
        };
        decButton.OnLeftClick += (_, _) => ModifyValue(true);
        panel.Append(decButton);

        var input = new UIEditableText(InputType.Number, "#...", EnterEditable, 3)
        {
            Width = StyleDimension.FromPixels(60),
            Height = StyleDimension.Fill,
            HAlign = 1f,
            Left = StyleDimension.FromPixels(-64)
        };
        input.OnUpdate += CheckSetObjectValue;
        panel.Append(input);

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

                case float floatVal:
                    _info.SetValue(_reference, MathHelper.Clamp(floatVal + (increase ? 1 : -1), float.MinValue, float.MaxValue));
                    break;

                default:
                    throw null;
            }

            HasBeenEdited = true;
        }
    }

    private void CheckSetObjectValue(UIElement affectedElement)
    {
        UIEditableText self = affectedElement as UIEditableText;

        if (self.currentValue != string.Empty && !self.CurrentlyTyping)
            EnterEditable(self, ref self.currentValue);
    }

    public void EnterEditable(UIEditableText self, ref string value)
    {
        if (value == string.Empty)
            return;

        object editingValue = _info.GetValue(_reference);

        object newValue = editingValue switch // Require boxing in order to properly type cast
        {
            int => (object)int.Parse(value),
            short => (object)short.Parse(value),
            ushort => (object)ushort.Parse(value),
            byte => (object)byte.Parse(value),
            float => (object)float.Parse(value),
            double => (object)double.Parse(value),
            _ => 0,
        };

        _info.SetValue(_reference, newValue);

        value = "";
    }

    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (UIElement element in Elements)
            element.Draw(spriteBatch);

        if (_text.ContainsPoint(Main.MouseScreen) && Localization.HasValue)
            UICommon.TooltipMouseText(Localization.Value.Description.Value);
    }
}
