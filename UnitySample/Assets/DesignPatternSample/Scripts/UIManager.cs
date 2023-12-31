﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace DesignPatternSample
{
    public class UIManager : MonoBehaviour, IObserver
    {
        // 実績フェードイン時間
        private const float ACHIEVEMNT_FADEINTIME = 60.0f;
        // 実績通常持続時間
        private const float ACHIEVEMNT_ACTIVETIME = 120.0f;
        // 実績フェードアウト時間
        private const float ACHIEVEMNT_FADEOUTTIME = 60.0f;

        public Image _achievementsImage;
        public Text _achievementsInfoText;
        public Text _achievementsDetailText;

        public Text _pauseText;

        private Subject _player;
        private Subject _achievementManager;

        private SampleSceneManager manager => SampleSceneManager.GetInstance();

        private void Awake()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    _player = playerController;
                }
            }

            var manager = GameObject.FindGameObjectWithTag("Manager");
            if (manager != null)
            {
                var achievementManager = manager.GetComponent<AchievementManager>();
                if (achievementManager != null)
                {
                    _achievementManager = achievementManager;
                }
            }
        }

        private void OnEnable()
        {
            if(_player)
            {
                _player.AddObserver(this);
            }

            if(_achievementManager)
            {
                _achievementManager.AddObserver(this);
            }
        }

        private void OnDisable()
        {
            if (_player)
            {
                _player.RemoveOvserver(this);
            }

            if (_achievementManager)
            {
                _achievementManager.RemoveOvserver(this);
            }
        }

        /// <summary>
        /// 一時停止UI表示
        /// </summary>
        public void ShowPause(bool pause)
        {
            _pauseText.enabled = pause;
        }

        /// <summary>
        /// 実績UI表示
        /// </summary>
        private void ShowAchievement(string text)
        {
            _achievementsImage.enabled = true;
            _achievementsInfoText.enabled = true;
            _achievementsDetailText.enabled = true;
            _achievementsDetailText.text = text;
            StartCoroutine(CoShowAchievement());
        }

        /// <summary>
        /// 実績UI非表示
        /// </summary>
        private void HideAchievement()
        {
            _achievementsImage.enabled = false;
            _achievementsInfoText.enabled = false;
            _achievementsDetailText.enabled = false;
            _achievementsDetailText.text = "";
            StopCoroutine(CoShowAchievement());
        }

        /// <summary>
        /// 観測者に通知メッセージを送信
        /// </summary>
        public void OnNotify(NotifyMessage message)
        {
            if (message != null)
            {
                Type messageType = message.GetType();
                if (messageType == typeof(PauseMessage))
                {
                    var pauseMessage = message as PauseMessage;
                    if (pauseMessage != null)
                    {
                        ShowPause(pauseMessage.paused);
                    }
                }
                else if (messageType == typeof(UnlockAchievementMessage))
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
            }
        }

        /// <summary>
        /// (コルーチン)実績UIフェード表示
        /// </summary>
        IEnumerator CoShowAchievement(float startAlpha = 0.0f, float endAlpha = 1.0f)
        {
            float timer = 0.0f;
            Color imageColor = _achievementsImage.color;
            Color textColor = _achievementsDetailText.color;

            _achievementsImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 0.0f);
            _achievementsInfoText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);
            _achievementsDetailText.color = new Color(textColor.r, textColor.g, textColor.b, 0.0f);

            while (timer < ACHIEVEMNT_FADEINTIME && manager)
            {
                float timeRate = timer / ACHIEVEMNT_FADEINTIME;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, timeRate);
                imageColor.a = alpha;
                textColor.a = alpha;
                _achievementsImage.color = imageColor;
                _achievementsInfoText.color = textColor;
                _achievementsDetailText.color = textColor;
                timer += manager.GetTimeMultiplier();
                yield return null;
            }
            timer = 0.0f;

            while (timer < ACHIEVEMNT_ACTIVETIME && manager)
            {
                timer += manager.GetTimeMultiplier();
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
                timer += manager.GetTimeMultiplier();
                yield return null;
            }

            HideAchievement();
        }
    }
}
