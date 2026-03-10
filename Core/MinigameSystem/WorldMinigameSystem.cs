using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem.Games;
using Parterraria.Core.MinigameSystem.MinigameUI;
using Parterraria.Core.Synchronization.MinigameSyncing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Parterraria.Core.MinigameSystem;

internal class WorldMinigameSystem : ModSystem
{
    public static WorldMinigameSystem Self => ModContent.GetInstance<WorldMinigameSystem>();

    /// <summary>
    /// If a minigame is currently active.
    /// </summary>
    public static bool InMinigame => Self.playingMinigame is not null;
    public static bool NotReady { get; private set; } = false;

    internal static readonly List<Minigame> worldMinigames = [];

    internal static bool selectingMinigame = false;
    internal static MinigameRanking rankings = null;

    private static int _minigameOverTimer = 0;
    private static int _minigamePreviewTimer = 0;

    private float _minigameTime = 0;
    private float _timerSpeed = 0f;
    private string[] _minigames = [];
    private int _selectedMinigame = 0;

    public Minigame playingMinigame = null;

    public static bool TryAddMinigame(string name, Rectangle rectangle, Point? playerSpawnLocation = null, byte[] data = null)
    {
        Minigame.MinigamesByModAndName[name].ValidateRectangle(ref rectangle);

        if (worldMinigames.Any(x => x.area.Intersects(rectangle)))
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Minigame.Intersecting"));
            return false;
        }

        var game = Minigame.MinigamesByModAndName[name].Clone();
        game.area = rectangle;
        game.playerStartLocation = playerSpawnLocation ?? rectangle.Center.ToVector2().ToTileCoordinates();
        game.OnPlace();

        if (data is not null)
        {
            using var stream = data.ToMemoryStream();
            using var reader = new BinaryReader(stream);
            game.ReadNetData(reader);
        }

        worldMinigames.Add(game);
        return true;
    }

    internal static void RemoveMinigame(Minigame minigame) => worldMinigames.Remove(minigame);

    internal static void DrawMinigameUI()
    {
        if (!InMinigame)
            return;

        Self.playingMinigame.DrawUI();

        if (_minigameOverTimer > 0)
            rankings?.Draw(Math.Min(_minigameOverTimer / 120f, 1));
    }

    internal static void DrawMinigames()
    {
        if (Main.LocalPlayer.HeldItem.ModItem is MinigameTool)
        {
            foreach (var item in worldMinigames)
                DebugDrawMinigames(item);
        }

        if (!InMinigame)
            return;

        Self.playingMinigame.Draw(false);

        if (_minigamePreviewTimer++ < 240)
        {
            Color alpha = Color.White;

            if (_minigamePreviewTimer > 120)
                alpha *= 1 - (_minigamePreviewTimer - 120f) / 120f;

            DrawCommon.CenteredString(FontAssets.DeathText.Value, Main.ScreenSize.ToVector2() / new Vector2(2f, 4f), Self.playingMinigame.DisplayName.Value, alpha);

            var descPos = Main.ScreenSize.ToVector2() / new Vector2(2f, 4f) + new Vector2(0, 40);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, descPos, Self.playingMinigame.Description.Value, alpha, new Vector2(0.5f));
        }
        else if (_minigamePreviewTimer > 260 && NotReady)
        {
            var p = new Vector2(Main.screenWidth / 2f, 60);
            Color color = Color.White * MathF.Min((_minigamePreviewTimer - 260) / 120f, 1);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, p, Self.playingMinigame.DisplayName.Value, color);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, p + new Vector2(0, 40), Self.playingMinigame.Description.Value, color, new Vector2(0.5f));
            DrawCommon.CenteredString(FontAssets.DeathText.Value, p + new Vector2(0, 80), Language.GetTextValue("Mods.Parterraria.MiscUI.Practice"), color, new Vector2(0.4f));
        }
    }

    private static void DebugDrawMinigames(Minigame game)
    {
        var loc = game.area;
        loc.Location -= Main.screenPosition.ToPoint();

        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, loc, Color.White * 0.1f);
        DrawCommon.CenteredString(FontAssets.DeathText.Value, loc.Location.ToVector2() + new Vector2(loc.Width / 2, 20), game.DisplayName.Value, Color.White);

        game.Draw(true);
    }

    public override void PreUpdatePlayers()
    {
        if (InMinigame)
        {
            if (NotReady)
            {
                bool ready = true;

                for (int i = 0; i < Main.maxPlayers; ++i)
                {
                    Player plr = Main.player[i];

                    if (plr.active && !plr.GetModPlayer<PlayingBoardPlayer>().minigameReady)
                    {
                        ready = false;
                        break;
                    }
                }

                if (ready)
                {
                    NotReady = false;
                    playingMinigame.OnStart();

                    foreach (var plr in Main.ActivePlayers)
                        playingMinigame.SetupPlayer(plr, true);
                }
            }
            else
                playingMinigame.Update();

            if (playingMinigame.Beaten)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && rankings is null)
                {
                    rankings = playingMinigame.GetRanking();

                    if (Main.netMode == NetmodeID.Server)
                        new SyncMinigameRankingModule(rankings).Send();
                }

                if (_minigameOverTimer++ > 240)
                    CompleteMinigame();
            }
        }

        if (WorldBoardSystem.PlayingParty && WorldBoardSystem.GameFinished)
        {
            WorldBoardSystem.CompleteGameFunctionality();
            return;
        }

        if (!WorldBoardSystem.PlayingParty || InMinigame || worldMinigames.Count == 0 || Main.netMode == NetmodeID.MultiplayerClient || selectingMinigame || 
            !Main.dedServ && Main.npcShop > 0)
        {
            if (selectingMinigame && Main.netMode == NetmodeID.Server)
                RollMinigameOnServer();

            return;
        }

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.active)
                continue;

            PlayingBoardPlayer board = plr.GetModPlayer<PlayingBoardPlayer>();
            
            if (!board.hasGoneOnCurrentTurn || board.isMoving || board.promptingSplit)
                return;
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
            BoardUISystem.SetMiscUI(new MinigameSelectionUIState(StartMinigame));
        else
        {
            _minigames = MinigameSelectionUIState.DetermineMinigames();
            _timerSpeed = Main.rand.NextFloat(2f, 2.5f);
            new SyncMinigameRollUIModule(_timerSpeed, _minigames).Send(-1, -1, false);
        }

        selectingMinigame = true;
    }

    public void RollMinigameOnServer()
    {
        _minigameTime += _timerSpeed;
        _timerSpeed *= 0.98f;

        if (_minigameTime > 1)
        {
            _selectedMinigame++;
            _minigameTime = 0;
        }

        if (_selectedMinigame >= 4)
            _selectedMinigame = 0;

        if ((Main.netMode != NetmodeID.SinglePlayer || Main.instance.IsActive) && _timerSpeed < 0.005f)
            StartMinigame(_minigames[_selectedMinigame]);
    }

    internal void StopParty()
    {
        playingMinigame = null;
        selectingMinigame = false;
        rankings = null;
        NotReady = true;
    }

    private void CompleteMinigame()
    {
        playingMinigame.OnStop();

        WorldBoardSystem.CompleteMinigame(playingMinigame, rankings);

        playingMinigame = null;
        rankings = null;
    }

    public override void ClearWorld()
    {
        playingMinigame = null;
        worldMinigames.Clear();
    }

    public void StartMinigame(string minigameName) => StartMinigame(minigameName, -1);

    public void StartMinigame(string minigameName, int minigameSlot = -1)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient && minigameSlot == -1)
            return;

        _minigamePreviewTimer = 0;
        _minigameOverTimer = 0;
        var choices = worldMinigames.Where(x => x.FullName == minigameName).ToArray();

        if (minigameSlot == -1)
            minigameSlot = Main.rand.Next(choices.Length);

        playingMinigame = worldMinigames.First(x => x is CraftLargeDiamondGame).Clone(); //choices[minigameSlot].Clone(); //
        playingMinigame.OnSet();
        NotReady = true;
        selectingMinigame = false;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.active)
                continue;

            plr.Center = playingMinigame.playerStartLocation.ToWorldCoordinates();
            plr.fallStart = (int)(plr.Center.Y / 16f);
            plr.GetModPlayer<PlayingBoardPlayer>().minigameReady = false;
            playingMinigame.SetupPlayer(plr, false);
        }

        if (Main.netMode != NetmodeID.Server)
            BoardUISystem.CloseMiscUI();
        else
            new SyncMinigameStartModule(minigameName, minigameSlot).Send(-1, -1, false);
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag.Add("minigameCount", (short)worldMinigames.Count);

        for (int i = 0; i < worldMinigames.Count; i++)
        {
            Minigame item = worldMinigames[i];
            TagCompound game = [];
            item.SaveData(game);
            tag.Add("game" + i, game);
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        short count = tag.GetShort("minigameCount");

        for (int i = 0; i < count; ++i)
        {
            TagCompound game = tag.GetCompound("game" + i);
            var minigame = Minigame.LoadMinigame(game);

            if (minigame is null)
                continue;

            worldMinigames.Add(minigame);
        }
    }
}
