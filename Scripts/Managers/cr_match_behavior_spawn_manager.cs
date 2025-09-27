using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class cr_match_behavior_spawn_manager : cr_MonoBehavior
{
    public cr_MatchBehavior MatchBehavior => cr_networking.matchBehavior;
    public cr_match_spawn[] Spawns;
    
    public cr_match_spawn GetMatchSpawnForPlayer(cr_player_api player)
    {
        if (!MatchBehavior.Object.HasStateAuthority) return null;
    
        var playerTeam = player.gameplayData.GetTeam();

        var cSpawn = Spawns.LastOrDefault();
        
        switch (MatchBehavior.SpawnType)
        {
            case cr_MatchBehavior_spawn_behaviors.Random:
                cSpawn = GetRandomSpawnForTeam(playerTeam);
            break;
            
            case cr_MatchBehavior_spawn_behaviors.RoundRobin:
                cSpawn = GetRoundRobinSpawnForTeam(playerTeam);
            break;
        }

        return cSpawn;
    }

    public cr_match_spawn GetRandomSpawnForTeam(cr_player_gameplay_team team)
    {
        List<cr_match_spawn> availableSpawns = new();
        
        for(int i = 0; i < Spawns.Length; i++)
        {
            if(Spawns[i].AssociatedTeam == team || Spawns[i].AssociatedTeam == cr_player_gameplay_team.Unassigned)
            {
                availableSpawns.Add(Spawns[i]);
            }
        }
        
        int randomIndex = Random.Range(0, availableSpawns.Count);
        
        return availableSpawns[randomIndex];
    }

    public Dictionary<cr_player_gameplay_team, int> TeamSpawnIndexes = new();
    public cr_match_spawn GetRoundRobinSpawnForTeam(cr_player_gameplay_team team)
    {
        if(!TeamSpawnIndexes.ContainsKey(team))
        {
            TeamSpawnIndexes.Add(team, 0);
            return FindNthSpawnOfTeam(team, 0);
        }

        int lastAccessedIndex = TeamSpawnIndexes[team];
        int maxSpawnsForTeam = GetSpawnsForTeam(team).Length;

        int newIndex = 0;
            // 1                            2
        if(!(lastAccessedIndex + 1 >= maxSpawnsForTeam)) // if the next index is not larger or equal to max, add one and move along
        {
            newIndex = lastAccessedIndex + 1;
        }
        
        TeamSpawnIndexes[team] = newIndex;
        
        return FindNthSpawnOfTeam(team, newIndex);
    }
    
    public cr_match_spawn FindNthSpawnOfTeam(cr_player_gameplay_team team, int index)
    {
        var possibleSpawns = GetSpawnsForTeam(team);

        return possibleSpawns[index];
    }
    
    public cr_match_spawn[] GetSpawnsForTeam(cr_player_gameplay_team team)
    {
        List<cr_match_spawn> possibleSpawns = new();
        
        for(int i = 0; i < Spawns.Length; i++)
        {
            if(Spawns[i].AssociatedTeam == team || Spawns[i].AssociatedTeam == cr_player_gameplay_team.Unassigned)
            {
                possibleSpawns.Add(Spawns[i]);
            }
        }
        
        return possibleSpawns.ToArray();
    }
}

[System.Serializable]
public class cr_match_spawn
{
    public cr_player_gameplay_team AssociatedTeam;
    public Transform SpawnPoint;
}