using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.MinigameSystem.MinigameUI;

internal class MinigameSelectionUIState(MinigameSelectionUIState.SetMinigameDelegate setMinigame) : UIState
{
    public delegate void SetMinigameDelegate(string value);

    private string[] _minigames = [];
    private int _selectedMinigame = 0;
    private SetMinigameDelegate _setMinigame = setMinigame;
    private int _timer = 0;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _timer++;

        if (Main.instance.IsActive && _timer > 240)
            _setMinigame(_minigames[_selectedMinigame]);
    }

    public override void OnInitialize()
    {
        var panel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(400),
            Height = StyleDimension.FromPixels(50 * 4),
            HAlign = 0.5f,
            VAlign = 0.2f
        };

        Append(panel);

        _minigames = new string[4];
        string[] minigamesDisplay = new string[4];
        HashSet<Minigame> games = [];

        foreach (var item in WorldMinigameSystem.worldMinigames)
        {
            if (!games.Contains(item))
                games.Add(item);
        }

        for (int i = 0; i < 4; ++i)
        {
            if (games.Count > 0)
            {
                Minigame game = Main.rand.Next(games.ToArray());
                _minigames[i] = game.FullName;
                minigamesDisplay[i] = game.DisplayName.Value;
                games.Remove(game);
            }
            else
            {
                int slot = Main.rand.Next(i);
                _minigames[i] = _minigames[slot];
                minigamesDisplay[i] = minigamesDisplay[slot];
            }
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
}
