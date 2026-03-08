using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
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
    public virtual int MaxPlayTime { get; }

    protected virtual bool DrawDefaultUI => true;

    /// <summary>
    /// The area, in tile coordinates, of this minigame. This should only be set on placement.
    /// </summary> 
    [HideFromEdit]
    public Rectangle area = default;

    public Point playerStartLocation = default;

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
    public abstract MinigameRanking GetRanking();
    public virtual Minigame Clone() => MemberwiseClone() as Minigame;

    /// <summary>
    /// Called when the minigame is first placed in-world.
    /// </summary>
    public virtual void OnPlace() { }
    
    /// <summary>
    /// Called when the minigame is set as the next minigame.
    /// </summary>
    public virtual void OnSet() { }

    /// <summary>
    /// Called when the minigame is set, per player.
    /// </summary>
    /// <param name="plr">Relevant player.</param>
    /// <param name="playing">Whether this is running during <see cref="Minigame.OnSet"/> or during <see cref="OnStart"/>.</param>
    public virtual void SetupPlayer(Player plr, bool playing) { }

    /// <summary>
    /// Called when all players are ready to start the minigame. This is called on all clients and the server.
    /// </summary>
    public virtual void OnStart() { }

    /// <summary>
    /// Called when the minigame is exited. This is called on all clients and the server.
    /// </summary>
    public virtual void OnStop() { }

    /// <summary>
    /// Called when the minigame is unset, per player.
    /// </summary>
    /// <param name="plr">Relevant player.</param>
    public virtual void ResetPlayer(Player plr) { }

    public virtual void WriteNetData(BinaryWriter writer)
    {
    }

    public virtual void ReadNetData(BinaryReader reader)
    {
    }

    internal virtual void Reward(MinigameRanking rankings, Player plr) => rankings.Reward(plr);

    public abstract bool ValidateRectangle(ref Rectangle rectangle);

    public void Draw(bool debug)
    {
        Rectangle screenRect = new ((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
        screenRect.Inflate(400, 400);

        if (!area.Intersects(screenRect))
        {
            return;
        }

        if (debug)
        {
            DrawCommon.DrawPositionMarker(playerStartLocation.ToWorldCoordinates(0, 0), Language.GetTextValue("Mods.Parterraria.MiscUI.StartPosition"));

            var points = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => (typeof(Point).IsAssignableFrom(x.FieldType) || typeof(Vector2).IsAssignableFrom(x.FieldType)
                    || typeof(Point16).IsAssignableFrom(x.FieldType)) && x.DeclaringType != typeof(Minigame));

            FieldInfo[] points2 = [.. points];

            foreach (var item in points)
            {
                object value = item.GetValue(this);

                Vector2 position = value switch
                {
                    Point16 point16 => point16.ToWorldCoordinates(0, 0),
                    Vector2 vecPosition => vecPosition,
                    Point point => point.ToWorldCoordinates(0, 0),
                    _ => throw null,
                };

                DrawCommon.DrawPositionMarker(position, item.Name);
            }
        }

        InternalDraw(debug);
    }

    /// <summary>
    /// Draws the minigame.
    /// </summary>
    /// <param name="debug">Whether the player is building or debugging minigames.</param>
    protected virtual void InternalDraw(bool debug) { }

    public void DrawUI()
    {
        if (DrawDefaultUI && MaxPlayTime > 0)
        {
            string time = MathF.Max((MaxPlayTime - PlayTime) / 60f, 0).ToString("#0.##");
            DrawCenteredTextFromTop($"{Language.GetTextValue("Mods.Parterraria.MiscUI.TimeLeft")}: " + time + "s", 30);
        }

        InternalDrawUI();
    }

    protected static void DrawCenteredTextFromTop(string text, float yOffset, float scale = 0.5f)
    {
        var position = new Vector2(Main.screenWidth / 2, yOffset);
        DrawCommon.CenteredString(FontAssets.DeathText.Value, position, text, Color.White, Vector2.One * scale);
    }

    protected virtual void InternalDrawUI() { }

    public void SaveData(TagCompound tag)
    {
        tag.Add("type", FullName);
        tag.Add(nameof(area), area);
        tag.Add(nameof(playerStartLocation), playerStartLocation);
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
            game.playerStartLocation = tag.Get<Point>(nameof(playerStartLocation));
            game.LoadData(tag);
            return game;
        }

        return null;
    }

    public virtual void LoadData(TagCompound tag) { }
}
