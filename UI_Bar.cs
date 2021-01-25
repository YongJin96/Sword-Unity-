using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bar : MonoBehaviour
{
    private Image HealthBar;
    public float NowHP;
    private float MaxHP = 1000f;
    PlayerStat Player;

    void Start()
    {
        HealthBar = GetComponent<Image>();
        Player = FindObjectOfType<PlayerStat>();
    }

    void Update()
    {
        NowHP = Player.nowHP;
        HealthBar.fillAmount = NowHP / MaxHP;
    }
}
