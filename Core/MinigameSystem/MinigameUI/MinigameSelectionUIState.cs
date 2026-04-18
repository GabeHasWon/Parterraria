using Parterraria.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.MinigameSystem.MinigameUI;

internal class MinigameSelectionUIState : UIState
{
    public readonly record struct MinigameDisplay(string Name, bool IsPvP);

    public delegate void SetMinigameDelegate(string value);

    private readonly SetMinigameDelegate _setMinigame;

    internal int selectedMinigame = 0;

    private string[] _minigames;
    private int _selectedGame;
    private float _minigameTime;

    public MinigameSelectionUIState(SetMinigameDelegate setMinigame, int selectedGame = -1, string[] minigames = null)
    {
        _setMinigame = setMinigame;
        _minigames = minigames;
        _selectedGame = selectedGame;
        _minigameTime = 0;

        if (_selectedGame == -1)
            _selectedGame = RandomizeSelectedGame();
    }

    internal static int RandomizeSelectedGame() => Main.rand.Next(34, 34 + 4 * 3);

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_selectedGame == -1)
            _selectedGame = RandomizeSelectedGame();

        UpdateTimers();
    }

    private void UpdateTimers()
    {
        _minigameTime = MathHelper.Lerp(_minigameTime, _selectedGame + 0.5f, 0.012f);
        selectedMinigame = (int)(_minigameTime % 4);

        if ((Main.netMode != NetmodeID.SinglePlayer || Main.instance.IsActive) && _minigameTime >= _selectedGame)
            _setMinigame(_minigames[Math.Abs(selectedMinigame - 1) % 4]);
    }

    public override void OnInitialize()
    {
        var minigamePanel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(240),
            Height = StyleDimension.FromPixels(56),
            HAlign = 0.5f,
            VAlign = 0.2f,
            Top = StyleDimension.FromPixels(-90)
        };
        Append(minigamePanel);

        var minigame = new UIText(Language.GetText("Mods.Parterraria.MiscUI.Minigame"), 1, true)
        {
            HAlign = 0.5f,
            VAlign = 0.5f,
        };
        minigamePanel.Append(minigame);

        var panel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(200),
            Height = StyleDimension.FromPixels(46 * 4),
            HAlign = 0.5f,
            VAlign = 0.2f
        };

        Append(panel);

        MinigameDisplay[] minigamesDisplay = new MinigameDisplay[4];

        if (_minigames is null)
            _minigames = DetermineMinigames(minigamesDisplay);
        else
        {
            for (int i = 0; i < minigamesDisplay.Length; ++i)
            {
                Minigame game = ModContent.Find<Minigame>(_minigames[i]);
                minigamesDisplay[i] = new(game.DisplayName.Value, game.PvPGame);
            } 
        }

        for (int i = 0; i < _minigames.Length; ++i)
        {
            int slot = i;
            UIText text = new(minigamesDisplay[i].Name)
            {
                VAlign = 0.1f + 0.25f * i,
                HAlign = 0.5f,
            };

            text.OnUpdate += (_) =>
            {
                Color baseColor = minigamesDisplay[slot].IsPvP ? CommonColors.PvPGame : Color.White;
                Color darkColor = minigamesDisplay[slot].IsPvP ? CommonColors.PvPDark : CommonColors.DarkGray;
                text.SetText($"[c/{(slot == selectedMinigame ? darkColor.Hex3() : baseColor.Hex3())}:{minigamesDisplay[slot].Name}]");
            };
            panel.Append(text);
        }
    }

    public static string[] DetermineMinigames(MinigameDisplay[] minigamesDisplay = null)
    {
        minigamesDisplay ??= new MinigameDisplay[4];
        string[] minigames = new string[4];
        HashSet<string> gameNames = [];
        HashSet<Minigame> games = [];

        foreach (var item in WorldMinigameSystem.worldMinigames)
        {
            if (!gameNames.Contains(item.Name))
                games.Add(item);

            gameNames.Add(item.Name);
        }

        for (int i = 0; i < 4; ++i)
        {
            if (games.Count > 0)
            {
                Minigame game = Main.rand.Next(games.ToArray());
                minigames[i] = game.FullName;
                minigamesDisplay[i] = new(game.DisplayName.Value, game.PvPGame);
                games.Remove(game);
            }
            else
            {
                int slot = Main.rand.Next(i);
                minigames[i] = minigames[slot];
                minigamesDisplay[i] = minigamesDisplay[slot];
            }
        }

        return minigames;
    }
}
