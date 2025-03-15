using Parterraria.Core.MinigameSystem.Games;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace Parterraria.Core.MinigameSystem.MinigameUI;

internal class MinigameSelectionUIState(MinigameSelectionUIState.SetMinigameDelegate setMinigame, float timerSpeed = -1, string[] minigames = null) : UIState
{
    public delegate void SetMinigameDelegate(string value);

    private string[] _minigames = minigames;
    private int _selectedMinigame = 0;
    private SetMinigameDelegate _setMinigame = setMinigame;
    private float _timerSpeed = timerSpeed;
    private float _minigameTime = 0;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_timerSpeed == -1)
            _timerSpeed = Main.rand.NextFloat(2f, 2.5f);

        UpdateTimers();
    }

    private void UpdateTimers()
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
        {
            var mini = ModContent.GetInstance<SplashArtGame>();
            _setMinigame(mini.FullName);
            //_setMinigame(_minigames[_selectedMinigame]);
         }
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

        var minigame = new UIText("Minigame!", 1, true)
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

        string[] minigamesDisplay = new string[4];

        if (_minigames is null)
            _minigames = DetermineMinigames(minigamesDisplay);
        else
        {
            for (int i = 0; i < minigamesDisplay.Length; ++i)
                minigamesDisplay[i] = ModContent.Find<Minigame>(_minigames[i]).DisplayName.Value;
        }

        for (int i = 0; i < _minigames.Length; ++i)
        {
            int slot = i;
            UIText text = new(minigamesDisplay[i])
            {
                VAlign = 0.1f + 0.25f * i,
                HAlign = 0.5f
            };
            text.OnUpdate += (_) => text.SetText($"[c/{(slot == _selectedMinigame ? "444444" : "FFFFFF")}:{minigamesDisplay[slot]}]");
            panel.Append(text);
        }
    }

    public static string[] DetermineMinigames(string[] minigamesDisplay = null)
    {
        minigamesDisplay ??= new string[4];
        string[] minigames = new string[4];
        HashSet<Minigame> games = [];

        foreach (var item in WorldMinigameSystem.worldMinigames)
        {
            games.Add(item);
        }

        for (int i = 0; i < 4; ++i)
        {
            if (games.Count > 0)
            {
                Minigame game = Main.rand.Next(games.ToArray());
                minigames[i] = game.FullName;
                minigamesDisplay[i] = game.DisplayName.Value;
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
