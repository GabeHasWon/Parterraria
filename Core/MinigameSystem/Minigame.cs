using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem;

internal abstract class Minigame : ModType
{
    public enum MinigameWinType : byte
    {
        First,
        InOrder,
        Last,
    }

    public static Dictionary<string, Minigame> MinigamesByModAndName = [];
    public static Dictionary<int, Minigame> MinigamesById = [];

    public abstract MinigameWinType WinType { get; }
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
    
    /// <summary>
    /// Called when the minigame is set as the next minigame.
    /// </summary>
    public virtual void OnSet() { }

    /// <summary>
    /// Called when the minigame is set, per player.
    /// </summary>
    /// <param name="plr">Relevant player.</param>
    public virtual void SetupPlayer(Player plr) { }

    /// <summary>
    /// Called when all players are ready to start the minigame.
    /// </summary>
    public virtual void OnStart() { }

    /// <summary>
    /// Called when the minigame is exited.
    /// </summary>
    public virtual void OnStop() { }

    /// <summary>
    /// Called when the minigame is unset, per player.
    /// </summary>
    /// <param name="plr">Relevant player.</param>
    public virtual void ResetPlayer(Player plr) { }

    public abstract bool ValidateRectangle(ref Rectangle rectangle);
    public virtual void Draw() { }

    public void SaveData(TagCompound tag)
    {
        tag.Add("type", FullName);
        tag.Add(nameof(area), area);
        InternalSave(tag);
    }

    protected virtual void InternalSave(TagCompound tag) { }

    public static Minigame LoadMinigame(TagCompound tag)
    {
        string modQualifiedName = tag.GetString("type");

        if (ModContent.TryFind(modQualifiedName, out Minigame storedGame))
        {
            Minigame game = storedGame.Clone();
            game.area = tag.Get<Rectangle>(nameof(area));
            game.LoadData(tag);
            return game;
        }

        return null;
    }

    public virtual void LoadData(TagCompound tag) { }
}
