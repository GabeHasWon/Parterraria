using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.WorldPresets;

internal class WorldPresetHooking : ILoadable
{
    public static List<WorldPreset> Presets = [];

    public void Load(Mod mod)
    {
        On_UIWorldCreation.MakeBackAndCreatebuttons += MakePresetButton;

        AddPreset(new WorldPreset("Parterraria!", "Mods.Parterraria.Presets.Parterraria!", 3, 11, "GabeHasWon", mod, () => RetrieveZip(mod, "Parterraria!")));
    }

    public static byte[] RetrieveZip(Mod mod, string name)
    {
        byte[] bytes = mod.GetFileBytes($"Core/WorldPresets/Presets/{name}.zip");
        return bytes;
    }

    public static void AddPreset(WorldPreset preset)
    {
        Presets.Add(preset);

        _ = Language.GetOrRegister(preset.LocalizationKey + ".Name");
        _ = Language.GetOrRegister(preset.LocalizationKey + ".Description");
    }

    private void MakePresetButton(On_UIWorldCreation.orig_MakeBackAndCreatebuttons orig, UIWorldCreation self, UIElement outerContainer)
    {
        orig(self, outerContainer);

        UITextPanel<LocalizedText> uITextPanel2 = new(Language.GetText("Mods.Parterraria.MiscUI.PresetWorlds.Title"), 0.7f, large: true)
        {
            Width = StyleDimension.FromPixelsAndPercent(-10, 0.5f),
            Height = StyleDimension.FromPixels(50f),
            VAlign = 1f,
            HAlign = 0.5f,
            Top = StyleDimension.FromPixels(10)
        };

        uITextPanel2.OnMouseOver += FadedMouseOver;
        uITextPanel2.OnMouseOut += FadedMouseOut;
        uITextPanel2.OnLeftMouseDown += (_, _) => SetToPresetUI(self);
        uITextPanel2.SetSnapPoint("Presets", 0);
        outerContainer.Append(uITextPanel2);
    }

    private static void SetToPresetUI(UIWorldCreation self) => Main.MenuUI.SetState(new WorldPresetState(() => Main.MenuUI.SetState(self)));

    private void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        ((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
        ((UIPanel)evt.Target).BorderColor = Colors.FancyUIFatButtonMouseOver;
    }

    private void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
    {
        ((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.8f;
        ((UIPanel)evt.Target).BorderColor = Color.Black;
    }

    public void Unload() { }
}
