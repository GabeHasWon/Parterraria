using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem.Games;
using Parterraria.Core.MinigameSystem.MinigameUI;
using Parterraria.Core.Synchronization;
using Parterraria.Core.Synchronization.MinigameSyncing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Enums;
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
    internal static readonly Dictionary<int, Minigame> worldMinigamesByNetId = [];

    internal static bool selectingMinigame = false;
    internal static MinigameRanking rankings = null;
    internal static int anticipationTime = -1;

    private static int _minigameOverTimer = 0;
    private static int _minigamePreviewTimer = 0;

    private readonly bool[] _wasPvp = new bool[Main.maxPlayers];

    private float _minigameTime = 0;
    private string[] _minigames = [];
    private int _selectedMinigame = 0;

    public Minigame playingMinigame = null;

    /// <summary>
    /// Tries to add a minigame. If it succeeds, adds the minigame and syncs it.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="rectangle"></param>
    /// <param name="playerSpawnLocation"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool TryAddMinigame(string name, Rectangle rectangle, Point? playerSpawnLocation = null, byte[] data = null, bool sync = false)
    {
        Minigame.MinigamesByModAndName[name].ValidateRectangle(ref rectangle);

        if (worldMinigames.Any(x => x.area.Intersects(rectangle)))
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Minigame.Intersecting"), CommonColors.Error);
            return false;
        }
        
        if (sync && Main.netMode == NetmodeID.MultiplayerClient)
            new SyncMinigameModule(name, rectangle, rectangle.Center.ToVector2().ToTileCoordinates(), data).Send(-1, -1, false);
        else
        {
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

            int id = 0;

            foreach (Minigame miniGame in worldMinigames)
            {
                if (miniGame.netId == id)
                    id++;
            }

            game.netId = id;

            worldMinigames.Add(game);
            worldMinigamesByNetId.Add(id, game);
        }

        return true;
    }

    internal static void RemoveMinigame(Minigame minigame)
    {
        int netId = minigame.netId;
        worldMinigames.Remove(minigame);
        worldMinigamesByNetId.Remove(netId);
    }

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

    /// <summary>
    /// Handles most of the game loop, like checking for minigames, doing win functionality and updating the minigame, if any.
    /// </summary>
    public override void PreUpdatePlayers()
    {
        if (InMinigame)
            UpdateDuringMinigame();

        if (WorldBoardSystem.PlayingParty && WorldBoardSystem.GameFinished)
        {
            WorldBoardSystem.CompleteGameFunctionality();
            return;
        }

        if (!WorldBoardSystem.PlayingParty || InMinigame || worldMinigames.Count == 0 || Main.netMode == NetmodeID.MultiplayerClient || selectingMinigame || AnyInShop())
        {
            if (selectingMinigame && Main.netMode == NetmodeID.Server)
                RollMinigameOnServer();

            return;
        }

        RollMinigame();
    }

    private void UpdateDuringMinigame()
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

            if (ready && anticipationTime == -1)
                anticipationTime = playingMinigame.AncitipationTime;

            if (ready && anticipationTime-- == 0)
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
                    new SyncMinigameRankingModule(rankings).Send(-1, -1, false);
            }

            if (_minigameOverTimer++ > 240)
                CompleteMinigame();
        }
    }

    private void RollMinigame()
    {
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
            _minigameTime = 0;
            _selectedMinigame = MinigameSelectionUIState.RandomizeSelectedGame();
            new SyncMinigameRollUIModule(_selectedMinigame, _minigames).Send(-1, -1, false);
        }

        selectingMinigame = true;
    }

    private static bool AnyInShop()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return Main.npcShop > 0;
        else
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (SyncInShopModule.InShop(player.whoAmI))
                    return true;
            }

            return false;
        }
    }

    public void RollMinigameOnServer()
    {
        _minigameTime = MathHelper.Lerp(_minigameTime, _selectedMinigame + 0.5f, 0.012f);

        if ((Main.netMode != NetmodeID.SinglePlayer || Main.instance.IsActive) && _minigameTime >= _selectedMinigame)
            StartMinigame(_minigames[Math.Abs(_selectedMinigame - 1) % 4]);
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
        playingMinigame.Beaten = false;

        WorldBoardSystem.CompleteMinigame(playingMinigame, rankings);

        playingMinigame = null;
        rankings = null;

        foreach (Player player in Main.ActivePlayers)
        {
            player.hostile = _wasPvp[player.whoAmI];
            player.GetModPlayer<MinigameDisablePlayer>().Enable();

            if (player.whoAmI == Main.myPlayer)
                NetMessage.SendData(MessageID.TogglePVP, -1, -1, null, Main.myPlayer);
        }

        _minigameTime = 0;
    }

    public override void ClearWorld()
    {
        playingMinigame = null;
        worldMinigames.Clear();
    }

    public void StartMinigame(string minigameName) => StartMinigame(minigameName, -1);

    public void StartMinigame(string minigameName, int minigameSlot = -1, Minigame.MinigamePlayType playType = Minigame.MinigamePlayType.None)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient && minigameSlot == -1)
            return;

        _minigamePreviewTimer = 0;
        _minigameOverTimer = 0;
        var choices = worldMinigames.Where(x => x.FullName == minigameName).ToArray();

        if (minigameSlot == -1)
            minigameSlot = Main.rand.Next(choices.Length);

        playingMinigame = worldMinigames.First(x => x is DuelingPistolGame).Clone();// choices[minigameSlot].Clone(); //
        playingMinigame.PlayType = Minigame.MinigamePlayType.FreeForAll;

        if (playType == Minigame.MinigamePlayType.None)
            playingMinigame.PlayType = playingMinigame.GetRandomPlayType();

        // TODO: Team mode
        if (playingMinigame.PvPGame)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                _wasPvp[player.whoAmI] = player.hostile;
                player.team = (int)Team.None;
                player.hostile = true;

                if (player.whoAmI == Main.myPlayer)
                    NetMessage.SendData(MessageID.TogglePVP, -1, -1, null, Main.myPlayer);
            }
        }

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
        {
            new SyncMinigameStartModule(minigameName, minigameSlot, playingMinigame.GetRandomPlayType()).Send(-1, -1, false);
        }
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
