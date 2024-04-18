using Steamworks;
using System;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI.EditUI;

internal class EditObjectUIState(object objectToEdit, Action<object> setObjectFunc) : UIState
{
    private readonly Action<object> _setObjectFunc = setObjectFunc;

    private object _objectToEdit = objectToEdit;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _setObjectFunc(_objectToEdit);
    }

    public override void OnInitialize()
    {
        var panel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(600),
            Height = StyleDimension.FromPixels(200),
            HAlign = 0.5f,
            VAlign = 0.2f
        };

        Append(panel);
        AddMemberEdits(panel);
    }

    private void AddMemberEdits(UIPanel panel)
    {
        UIList list = new()
        {
            Width = StyleDimension.FromPixelsAndPercent(-24, 1),
            Height = StyleDimension.Fill,
        };
        panel.Append(list);

        UIScrollbar bar = new()
        {
            Width = StyleDimension.FromPixels(20),
            Height = StyleDimension.Fill,
            HAlign = 1f
        };
        list.SetScrollbar(bar);
        panel.Append(bar);

        foreach (var item in _objectToEdit.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (item.DeclaringType == typeof(object))
                continue;

            if (!MemberEditUI.EditableTypes.Contains(item.FieldType) && !MemberEditUI.EditableTypes.Any(x => x.IsAssignableFrom(item.FieldType)))
                continue;

            MemberEditUI edit = new(_objectToEdit, item)
            {
                Width = StyleDimension.Fill,
                Height = StyleDimension.FromPixels(60),
            };
            edit.OnUpdate += (_) => UpdateValue(edit, item, ref _objectToEdit);
            list.Add(edit);
        }
    }

    private void UpdateValue(MemberEditUI edit, FieldInfo info, ref object obj) => info.SetValue(obj, edit.GetValue);
}
