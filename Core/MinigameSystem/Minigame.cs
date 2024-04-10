using System;
using System.Collections.Generic;
using Terraria.Localization;

namespace Parterraria.Core.MinigameSystem;

internal abstract class Minigame : ModType
{
    public static Dictionary<string, Minigame> MinigamesByModAndName = [];
    public static Dictionary<int, Minigame> MinigamesById = [];

    public virtual string LocalizationPath => "Mods." + Mod.Name + ".Party.Minigames." + GetType().Name;
    public LocalizedText DisplayName => Language.GetText(LocalizationPath + ".Name");
    public LocalizedText Description => Language.GetText(LocalizationPath + ".Description");

    public bool Beaten { get; protected set; }
    public int PlayTime { get; protected set; }

    public Rectangle area = default;

    protected sealed override void Register()
    {
        ModTypeLookup<Minigame>.Register(this);
        MinigamesByModAndName.Add(Mod.Name + "/" + GetType().Name, this);
        MinigamesById.Add(MinigamesById.Count, this);

        Language.GetOrRegister(LocalizationPath + ".Name", () => GetType().Name);
        Language.GetOrRegister(LocalizationPath + ".Description", () => GetType().Name);
    }

    public void Update()
    {
        PlayTime++;
        InternalUpdate();
    }

    public abstract void InternalUpdate();
    public virtual Minigame Clone() => MemberwiseClone() as Minigame;
    public virtual void OnStart() { }
    public abstract bool ValidateRectangle(ref Rectangle rectangle);
    public virtual void Draw() { }
}
