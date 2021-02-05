using System;
using System.Collections;
using System.Collections.Generic;
using _differences.Scripts.PVPBot;
using Differences;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerHolderPvP : MonoBehaviour
{
    [SerializeField] private Image playerAvatar;
    [SerializeField] private TMP_Text countMedalText;
    [SerializeField] private PullObjectsUI[] diffStatusLevels;

    private DataPicture[] pictures;


    private void Start()
    {
        
    }

    internal void Setup(DataPicture[] pictures, Action onSuccessInit = null)
    {
        this.pictures = pictures;

        for (int i = 0; i < pictures.Length; i++)
        {
            diffStatusLevels[i].StartPull<UICheckMark>();
        }
    }

    private void RefreshDiffStatus()
    {
       
    }
}
