using NetEasy;
using Parterraria.Core.MinigameSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parterraria.Core.Synchronization.MinigameSyncing;

[Serializable]
public class SyncMinigameRankingModule(MinigameRanking rank) : Module
{
    private readonly int[] values = [.. rank.Ranking.Select(x => x.Key)];
    private readonly MinigameReward[] rewards = [.. rank.Ranking.Select(x => x.Value)];

    protected override void Receive()
    {
        WorldMinigameSystem.rankings = new MinigameRanking();
        Dictionary<int, MinigameReward> rankings = [];

        for (int i = 0; i < values.Length; ++i)
            rankings.Add(values[i], rewards[i]);

        WorldMinigameSystem.rankings.Ranking = rankings;
    }
}
