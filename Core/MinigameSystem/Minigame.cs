using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem;

public abstract class Minigame : ModType
{
    public enum MinigameWinType : byte
    {
        First,
        InOrder,
        Last,
    }

    [Flags]
    public enum MinigamePlayType : byte
    {
        None = 0,
        FreeForAll = 1 << 1,
        Team = 1 << 2,
        Duel = 1 << 3,

        All = FreeForAll | Team | Duel
    }

    internal static Dictionary<string, Minigame> MinigamesByModAndName = [];
    internal static Dictionary<int, Minigame> MinigamesById = [];

    /// <summary>
    /// How this minigame is won.
    /// </summary>
    public abstract MinigameWinType WinType { get; }

    /// <summary>
    /// If this is a free-for-all, team, duel game, or any combination of these. Duo games will only be selected by duel nodes.
    /// </summary>
    public abstract MinigamePlayType AvailablePlayType { get; }

    /// <summary>
    /// If this game forces PvP on.
    /// </summary>
    public virtual bool PvPGame => false;

    public virtual string LocalizationPath => "Mods." + Mod.Name + ".Party.Minigames." + GetType().Name;
    public LocalizedText DisplayName => Language.GetText(LocalizationPath + ".Name");
    public LocalizedText Description => Language.GetText(LocalizationPath + ".Description");

    public bool Beaten { get; internal set; }
    public int PlayTime { get; protected set; }
    public virtual int MaxPlayTime { get; }
    public MinigamePlayType PlayType { get; internal set; }

    protected virtual bool DrawDefaultUI => true;

    /// <summary>
    /// Internal ID used for IO/netsync.
    /// </summary>
    [HideFromEdit]
    internal int netId = 0;

    /// <summary>
    /// The area, in tile coordinates, of this minigame. This should only be set on placement.
    /// </summary> 
    [HideFromEdit]
    public Rectangle area = default;

    public Point playerStartLocation = default;

    /// <summary>
    /// Maps field name to their localization.
    /// </summary>
    [HideFromEdit]
    internal Dictionary<string, FieldLocalization> fieldLocalizations = [];

    protected sealed override void Register()
    {
        ModTypeLookup<Minigame>.Register(this);
        MinigamesByModAndName.Add(Mod.Name + "/" + GetType().Name, this);
        MinigamesById.Add(MinigamesById.Count, this);

        _ = DebugDisplayPositions();

        Language.GetOrRegister(LocalizationPath + ".Name", () => GetType().Name);
        Language.GetOrRegister(LocalizationPath + ".Description", () => GetType().Name);

        FieldInfo[] fields = GetType().GetFields(EditObjectUIState.FieldReflectionFlags);

        foreach (FieldInfo field in fields)
        {
            if (field.DeclaringType == typeof(object) || Attribute.IsDefined(field, typeof(HideFromEditAttribute)))
                continue;

            if (field.DeclaringType == typeof(Minigame)) // Force consistent localizations for minigame members
            {
                LocalizedText minigameFieldName = Language.GetOrRegister("Mods." + Mod.Name + ".Party.Minigames." + field.Name + ".Name", () => field.Name);
                LocalizedText minigameFieldDesc = Language.GetOrRegister("Mods." + Mod.Name + ".Party.Minigames." + field.Name + ".Description", () => "");
                fieldLocalizations.Add(field.Name, new FieldLocalization(minigameFieldName, minigameFieldDesc));
                continue;
            }

            string key = LocalizationPath + ".Fields." + field.Name + ".Name";
            string descKey = LocalizationPath + ".Fields." + field.Name + ".Description";

            if (Attribute.GetCustomAttribute(field, typeof(MemberLocalizableAttribute)) is MemberLocalizableAttribute member)
            {
                if (member.ForceKey is { } newKey)
                    key = newKey;

                if (member.ForceDescription is { } newDescription)
                    descKey = newDescription;
            }

            LocalizedText name = Language.GetOrRegister(key, () => field.Name);
            LocalizedText description = descKey == string.Empty ? LocalizedText.Empty : Language.GetOrRegister(descKey, () => "");
            fieldLocalizations.Add(field.Name, new FieldLocalization(name, description));
        }
    }

    /// <summary>
    /// Updates the minigame. Runs on all clients + server.
    /// </summary>
    public void Update()
    {
        PlayTime++;
        InternalUpdate();
    }

    ///<inheritdoc cref="Update"/>
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
    /// <param name="playing">Whether this is running during <see cref="OnSet"/> or during <see cref="OnStart"/>.</param>
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

    /// <summary>
    /// Returns the result of <see cref="WriteNetData(BinaryWriter)"/> as a byte array.
    /// </summary>
    public byte[] GetNetBytes() => NetUtils.WriteAsBytes(WriteNetData);

    public virtual void ReadNetData(BinaryReader reader)
    {
    }

    internal virtual void Reward(MinigameRanking rankings, Player plr) => rankings.Reward(plr);

    public abstract bool ValidateRectangle(ref Rectangle rectangle);

    public MinigamePlayType GetRandomPlayType()
    {
        PriorityQueue<MinigamePlayType, float> options = new();

        for (int i = 1; i <= System.Numerics.BitOperations.PopCount((uint)MinigamePlayType.All); ++i)
        {
            var flag = (MinigamePlayType)(1 << i);

            if (AvailablePlayType.HasFlag(flag))
                options.Enqueue(flag, Main.rand.Next());
        }

        return options.Dequeue();
    }

    protected static void RectangleMinimumTiles(ref Rectangle rectangle, int minWidth, int minHeight, out bool modified)
    {
        modified = false;

        if (rectangle.Width < minWidth * 16)
        {
            rectangle.Width = minWidth * 16;
            modified = true;
        }

        if (rectangle.Height < minHeight * 16)
        {
            rectangle.Height = minHeight * 16;
            modified = true;
        }
    }

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
            (object position, LocalizedText name)[] positions = DebugDisplayPositions();

            foreach (var item in positions)
            {
                Vector2 position = item.position switch
                {
                    Point16 point16 => point16.ToWorldCoordinates(0, 0),
                    Vector2 vecPosition => vecPosition,
                    Point point => point.ToWorldCoordinates(0, 0),
                    _ => throw null,
                };

                DrawCommon.DrawPositionMarker(position, item.name.Value);
            }
        }

        InternalDraw(debug);
    }

    protected virtual (object, LocalizedText)[] DebugDisplayPositions() => [];

    /// <summary>
    /// Draws the minigame.
    /// </summary>
    /// <param name="debug">Whether the player is building or debugging minigames.</param>
    protected virtual void InternalDraw(bool debug) { }

    public void DrawUI()
    {
        if (DrawDefaultUI && MaxPlayTime > 0 && !WorldMinigameSystem.NotReady)
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
        tag.Add(nameof(netId), netId);
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
            game.netId = tag.GetInt(nameof(netId));
            game.LoadData(tag);
            return game;
        }

        return null;
    }

    public virtual void LoadData(TagCompound tag) { }
}
