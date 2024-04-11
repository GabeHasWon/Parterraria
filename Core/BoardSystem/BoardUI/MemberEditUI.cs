using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class MemberEditUI(object container) : UIState
{
    public static List<Type> EditableTypes = [ typeof(int), typeof(bool), typeof(float), typeof(string), typeof(Vector2) ];

    readonly Ref<object> _container = new(container);

    public override void OnInitialize()
    {
        float width = FontAssets.ItemStack.Value.MeasureString(_container.Value.GetType().Name).X;

        UIPanel panel = new()
        {
            Width = StyleDimension.FromPixels(width),
            Height = StyleDimension.FromPixels(50),
        };

        Append(panel);
    }
}
