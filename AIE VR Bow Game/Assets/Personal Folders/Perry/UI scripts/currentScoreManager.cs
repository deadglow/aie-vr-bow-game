using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class currentScoreManager : MonoBehaviour
{
    public int shtFired;
    public int kEnemy;
    public int tpShtFired;
    public int currRound;

    public TMP_Text currentRound;
    public TMP_Text shtsFired;
    public TMP_Text tpShotFired;
    public TMP_Text killedEnemy;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpDateText()
    {
        shtsFired.text = shtFired.ToString();

        killedEnemy.text = kEnemy.ToString();

        tpShotFired.text = tpShtFired.ToString();

        currentRound.text = currentRound.ToString();
    }
}
