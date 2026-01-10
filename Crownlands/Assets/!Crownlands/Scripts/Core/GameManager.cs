using Mirror;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public List<Transform> spawnPoints = new();
    [Server]
    public void RespawnPlayer(Health health)
    {
        if (!isServer) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        health.TargetRespawn(health.connectionToClient, spawnPoint.position);
        health.currentHealth = health.MaxHealth;
    }
}
