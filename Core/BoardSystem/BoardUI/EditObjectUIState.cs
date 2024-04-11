using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class EditObjectUIState(object objectToEdit) : UIState
{
    private readonly object _objectToEdit = objectToEdit;

    public override void OnInitialize()
    {
        var panel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(600),
            Height = StyleDimension.FromPixels(120)
        };

        Append(panel);

        ReflectObjectAndAddToPanel(panel);
    }

    private void ReflectObjectAndAddToPanel(UIPanel panel)
    {
        foreach (var item in _objectToEdit.GetType().GetFields())
        {
            if (item.DeclaringType == typeof(object))
                continue;

            if (!MemberEditUI.EditableTypes.Contains(item.FieldType))
                continue;


        }
    }
}
