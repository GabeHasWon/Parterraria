using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.WorldPresets;

internal class WorldPresetState(Action returnAction) : UIState
{
    private readonly Action _returnAction = returnAction;

    public override void OnInitialize()
    {
        const int Buffer = 18;

        UIElement hiddenElement = new()
        {
            Width = StyleDimension.FromPixels(500f),
            Height = StyleDimension.FromPixels(484f + Buffer),
            Top = StyleDimension.FromPixels(170f - Buffer),
            HAlign = 0.5f,
            VAlign = 0f
        };
        hiddenElement.SetPadding(0f);
        Append(hiddenElement);

        UIPanel panel = new()
        {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPixels(360 + Buffer),
            Top = StyleDimension.FromPixels(50f),
            BackgroundColor = new Color(33, 43, 79) * 0.8f
        };
        panel.SetPadding(12);
        hiddenElement.Append(panel);

        UIButton<string> backButton = new("x")
        {
            Width = StyleDimension.FromPixels(32),
            Height = StyleDimension.FromPixels(32),
            HAlign = 1,
            VAlign = 0,
        };

        backButton.OnLeftClick += (_, _) => _returnAction();
        panel.Append(backButton);

        UIText title = new(Language.GetText("Mods.Parterraria.MiscUI.PresetWorlds.Title"), 0.8f, true);
        panel.Append(title);

        AddListUI(panel);
    }

    private void AddListUI(UIPanel panel)
    {
        UIPanel listPanel = new()
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixelsAndPercent(-80, 1),
            Top = StyleDimension.FromPixels(36),
        };

        panel.Append(listPanel);
    }
}
