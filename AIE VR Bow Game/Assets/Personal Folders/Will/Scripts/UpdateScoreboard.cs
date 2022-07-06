using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScoreboard : MonoBehaviour
{
    public scoreManager scoreShiz;

    public scoreManager LastScoreOnScoreboards;

    // Start is called before the first frame update
    void Start()
    {
        scoreShiz.shtFired = 0;
        scoreShiz.kEnemy = 0;
        scoreShiz.tpShtFired = 0;
        scoreShiz.highRound = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddToEnemiesKilled()
    {
        scoreShiz.kEnemy += 1;
    }

    public void AddToShtFired()
    {
        scoreShiz.shtFired += 1;
    }

    public void AddToTpShtFired()
    {
        scoreShiz.tpShtFired += 1;
    }

    public void AddToRoundNumber()
    {
        scoreShiz.highRound += 1;
    }

    public void UpdateLastRound()
    {
        LastScoreOnScoreboards.shtFired = scoreShiz.shtFired;
        LastScoreOnScoreboards.kEnemy = scoreShiz.kEnemy;
        LastScoreOnScoreboards.tpShtFired = scoreShiz.tpShtFired;
        LastScoreOnScoreboards.highRound = scoreShiz.highRound;
    }

    public void ResetScore()
    {
        scoreShiz.shtFired = 0;
        scoreShiz.kEnemy = 0;
        scoreShiz.tpShtFired = 0;
        scoreShiz.highRound = 0;
    }
}
