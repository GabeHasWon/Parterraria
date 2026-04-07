using ReLogic.Content;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Parterraria.Core.WorldPresets;

internal class UIWorldPreset(WorldPreset preset, Action<WorldPreset> overrideDescription, Action<WorldPreset> openPreset) : UIPanel
{
    private readonly WorldPreset _preset = preset;
    private readonly Action<WorldPreset> _overrideDescription = overrideDescription;
    private readonly Action<WorldPreset> _openPreset = openPreset;

    public override void OnInitialize()
    {
        UIImageButton useWorldButton = new(ModContent.Request<Texture2D>("Parterraria/Core/WorldPresets/PresetCheck"))
        {
            Width = StyleDimension.FromPixels(30),
            Height = StyleDimension.FromPixels(30),
            Top = StyleDimension.FromPixels(-2)
        };

        useWorldButton.OnLeftClick += (_, _) => _openPreset(_preset);
        Append(useWorldButton);

        int left = 42;

        if (_preset.Mod.RequestAssetIfExists("icon_small", out Asset<Texture2D> icon))
        {
            UIImage image = new(icon)
            {
                Left = StyleDimension.FromPixels(left),
            };

            Append(image);

            left += 34;
        }

        UIText name = new(_preset.Name)
        {
            Left = StyleDimension.FromPixels(left),
        };

        Append(name);

        UIText info = new(_preset.GetDisplayInfo().Value)
        {
            HAlign = 1,
        };

        Append(info);
    }

    public override void Update(GameTime gameTime) 
    {
        base.Update(gameTime);

        if (ContainsPoint(Main.MouseScreen))
            _overrideDescription.Invoke(_preset);
    }
}
