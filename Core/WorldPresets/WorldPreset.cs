using System;
using Terraria.Localization;

namespace Parterraria.Core.WorldPresets;

#nullable enable

/// <summary>
/// Individual instance of a world preset.
/// </summary>
public readonly record struct WorldPreset(string Id, string LocalizationKey, int BoardCount, int MinigameCount, string Authors, Mod Mod, Func<byte[]> FetchFile, string[]? RequiredMods = null)
{
    public LocalizedText Name => Language.GetOrRegister(LocalizationKey + ".Name", () => "World");
    public LocalizedText Description => Language.GetOrRegister(LocalizationKey + ".Description");

    public LocalizedText GetDisplayInfo() => Language.GetText("Mods.Parterraria.Presets.DisplayInfo").WithFormatArgs($"[c/AAFFAA:{BoardCount}]", $"[c/AAAAFF:{MinigameCount}]");
}
