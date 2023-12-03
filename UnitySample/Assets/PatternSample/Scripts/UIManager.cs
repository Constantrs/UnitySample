﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour, IObserver
{
    private const float ACHIEVEMNT_FADEINTIME = 60.0f;
    private const float ACHIEVEMNT_ACTIVETIME = 120.0f;
    private const float ACHIEVEMNT_FADEOUTTIME = 60.0f;

    public Image _achievementsImage;
    public Text _achievementsInfoText;
    public Text _achievementsDetailText;

    public Text _pauseText;
    private SampleManager manager => SampleManager.GetInstance();
    private Subject _achievementManager;
    private Subject _player;

    // Start is called before the first frame update
    private void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            _player = playerController;
        }

        var achievementManager = gameObject.GetComponent<AchievementManager>();
        if (achievementManager != null)
        {
            _achievementManager = achievementManager;
        }
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        _player?.AddObserver(this);
        _achievementManager?.AddObserver(this);
    }

    private void OnDisable()
    {
        _player?.RemoveOvserver(this);
        _achievementManager?.RemoveOvserver(this);
    }

    public void OnNotify(NotifyMessage message)
    {
        if (message != null)
        {
            Type messageType = message.GetType();
            if (messageType == typeof(UnlockAchievementMessage))
            {
                var achievementMessage = message as UnlockAchievementMessage;
                if (achievementMessage != null)
                {
                    switch (achievementMessage.achievementId)
                    {
                        case 0:
                            ShowAchievement("アイテムコレクター");
                            break;
                    }
                }
            }
            if (messageType == typeof(PauseMessage))
            {
                var pauseMessage = message as PauseMessage;
                if (pauseMessage != null)
                {
                    ShowPause(pauseMessage.paused);
                }
            }
        }
    }

    private void ShowPause(bool pause)
    {
        _pauseText.enabled = pause;
    }


    private void ShowAchievement(string text)
    {
        _achievementsImage.enabled = true;
        _achievementsInfoText.enabled = true;
        _achievementsDetailText.enabled = true;
        _achievementsDetailText.text = text;
        StartCoroutine(CoShowAchievement());
    }

    private void HideAchievement()
    {
        _achievementsImage.enabled = false;
        _achievementsInfoText.enabled = false;
        _achievementsDetailText.enabled = false;
        _achievementsDetailText.text = "";
        StopCoroutine(CoShowAchievement());
    }

    IEnumerator CoShowAchievement(float startAlpha = 0.0f, float endAlpha = 1.0f)
    {
        float timer = 0.0f;
        Color imageColor = _achievementsImage.color;
        Color textColor =_achievementsDetailText.color;

        _achievementsImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0.0f);
        _achievementsInfoText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);
        _achievementsDetailText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);

        while(timer < ACHIEVEMNT_FADEINTIME && manager)
        {
            float timeRate = timer / ACHIEVEMNT_FADEINTIME;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, timeRate);
            imageColor.a = alpha;
            textColor.a = alpha;
            _achievementsImage.color = imageColor;
            _achievementsInfoText.color = textColor;
            _achievementsDetailText.color = textColor;
            timer += manager.GetTimeScale();
            yield return null;
        }
        timer = 0.0f;

        while (timer < ACHIEVEMNT_ACTIVETIME && manager)
        {
            timer += manager.GetTimeScale();
            yield return null;
        }
        timer = 0.0f;

        while (timer < ACHIEVEMNT_FADEOUTTIME && manager)
        {
            float timeRate = timer / ACHIEVEMNT_FADEOUTTIME;
            float alpha = Mathf.Lerp(endAlpha, startAlpha, timeRate);
            imageColor.a = alpha;
            textColor.a = alpha;
            _achievementsImage.color = imageColor;
            _achievementsInfoText.color = textColor;
            _achievementsDetailText.color = textColor;
            timer += manager.GetTimeScale();
            yield return null;
        }

        HideAchievement();
    }
}
