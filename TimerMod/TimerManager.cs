using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TimerMod.Utils;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TimerMod
{
    public class TimerManager
    {
        private const float UpdateInterval = 1f;

        private static TimerManager _instance;

        private static readonly List<string> StartMessages = new List<string>
        {
            "The countdown has started! 10 minutes left!", "Timer’s on! We’ve got 10 minutes to live!", "Here we go—10 minutes on the clock!",
            "The timer’s ticking! 10 minutes, people!", "Ten minutes until doom—let’s move!", "Clock’s running—10 minutes to survive!",
            "It’s begun! 10 minutes before we blow!", "Timer activated! 10 minutes, no pressure!", "We’re on the clock—10 minutes left!",
            "Countdown’s live! 10 minutes to go!"
        };

        private static readonly List<string> AddTimeMessages = new List<string>
        {
            "Five more minutes! Let’s hustle!", "Added 5 minutes—keep moving!", "We’ve got 5 extra minutes—go!", "Five minutes added! Don’t slack!",
            "More time! 5 minutes to breathe!", "Timer bumped—5 minutes more!", "Five minutes gained—use it wisely!", "Extra 5 minutes—let’s not waste it!",
            "Time extended! 5 minutes added!", "Five more minutes on the clock—hurry!"
        };

        private static readonly List<string> DeathMessages = new List<string>
        {
            "I’m about to explode!", "This is the end—BOOM!", "Timer got me—goodbye!", "I’m blowing up—argh!", "No time left—kaboom!",
            "It’s over—I’m done!", "Exploding now—see ya!", "The clock won—blast!", "I’m a goner—boom!", "Time’s up—farewell!",
            "Bye Bye world!", "I’m toast—kaboom!", "I’m exploding—help!"
        };

        private static readonly List<PlayerAvatar> PlayerAvatars = new List<PlayerAvatar>();
        private static readonly List<(PlayerAvatar avatar, float delay, string message)> PendingExplosions = new List<(PlayerAvatar, float, string)>();

        private static readonly List<string> TwoMinuteWarnings = new List<string>
        {
            "Two minutes left! Hurry up!", "Only 2 minutes—things are getting tense!", "Two minutes to go—move it!", "We’re down to 2 minutes—panic time!",
            "Two minutes remaining—oh no!", "Clock’s at 2 minutes—speed up!", "Two minutes until boom—help!", "Just 2 minutes left—run!",
            "Two minutes on the timer—yikes!", "We’ve got 2 minutes—do something!"
        };

        private static readonly List<string> ThirtySecondWarnings = new List<string>
        {
            "30 seconds! I’m doomed!", "Half a minute left—I’m sweating!", "30 seconds to live—oh man!", "Thirty seconds—say your prayers!",
            "30 seconds on the clock—help me!", "Half a minute until kaboom!", "30 seconds left—I’m toast!", "Thirty seconds—this is it!",
            "30 seconds remaining—goodbye!", "Half a minute—I can’t make it!"
        };

        private GameObject _canvas;
        private bool _explosionsScheduled;
        private bool _hasResetForNonPlayable;

        private bool _isTimerRunning;
        private float _lastTime;
        private float _timer;

        private TextMeshProUGUI _timerText;
        private SemiUI _timerTextSemiUI;

        private float _timeSinceLastUpdate;

        public static TimerManager Instance => _instance ?? (_instance = new TimerManager());

        public void OnLevelChange(Level level)
        {
            if (level == null) return;

            Plugin.Logger.LogInfo($"Level changed to {level.name}");

            Reset();
            TryInitializeUI();
        }

        public void OnGameDirectorUpdate(GameDirector gameDirector)
        {
            if (!SemiFunc.RunIsLevel() || RunManager.instance == null)
            {
                if (_canvas != null)
                {
                    Object.Destroy(_canvas);
                    _canvas = null;
                    _timerText = null;
                }

                _isTimerRunning = false;
                return;
            }

            if (!_isTimerRunning && PendingExplosions.Count == 0) return;

            var realtimeSinceStartup = Time.realtimeSinceStartup;
            var deltaTime = realtimeSinceStartup - _lastTime;
            _lastTime = realtimeSinceStartup;

            if (_isTimerRunning)
            {
                _timer -= deltaTime;
                _timeSinceLastUpdate += deltaTime;

                if (_timeSinceLastUpdate >= UpdateInterval)
                {
                    UpdateTimerText();
                    _timeSinceLastUpdate = 0f;

                    if (PhotonNetwork.IsMasterClient)
                    {
                        if (_timer <= 120f && _timer > 119f)
                        {
                            SendRandomMessageToAll(TwoMinuteWarnings);
                        }
                        else if (_timer <= 30f && _timer > 29f)
                        {
                            SendRandomMessageToAll(ThirtySecondWarnings);
                        }
                        else if (_timer <= 0f && !_explosionsScheduled)
                        {
                            ScheduleExplosions();
                            _explosionsScheduled = true;
                        }
                    }
                }
            }

            ProcessPendingExplosions(deltaTime);
        }

        public void OnExtraction()
        {
            if (!SemiFunc.RunIsLevel()) return;

            if (_canvas == null)
            {
                TryInitializeUI();
            }

            if (!_isTimerRunning)
            {
                _timer = 600f;
                _isTimerRunning = true;
                _lastTime = Time.realtimeSinceStartup - UpdateInterval;
                _timeSinceLastUpdate = 0f;
                if (PhotonNetwork.IsMasterClient)
                {
                    UpdatePlayerList();
                    SendRandomMessageToAll(StartMessages);
                }
            }
            else
            {
                _timer += 300f;
                if (PhotonNetwork.IsMasterClient)
                {
                    SendRandomMessageToAll(AddTimeMessages);
                }
            }

            UpdateTimerText();
        }

        private void Reset()
        {
            _isTimerRunning = false;
            _timer = 600f;
            _hasResetForNonPlayable = false;
            _explosionsScheduled = false;
            PendingExplosions.Clear();
        }

        private void TryInitializeUI()
        {
            if (!SemiFunc.RunIsLevel() || RunManager.instance == null) return;

            if (_canvas != null && _timerText != null && _canvas.activeSelf) return;

            _canvas = GameObject.Find("UI/HUD/HUD Canvas/HUD/Game Hud/");

            var timerTextObject = new GameObject("TimerText");
            timerTextObject.transform.SetParent(_canvas.transform, false);

            _timerText = timerTextObject.AddComponent<TextMeshProUGUI>();
            _timerText.text = "10:00";
            _timerText.fontSize = 36f;
            _timerText.alignment = TextAlignmentOptions.Center;

            timerTextObject.SetActive(true);

            var rectTransform = _timerText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-4, -34);
            rectTransform.sizeDelta = new Vector2(200f, 50f);

            var haulUI = Object.FindObjectOfType<HaulUI>();
            if (haulUI != null && haulUI.uiText != null)
            {
                _timerText.font = haulUI.uiText.font;
                _timerText.fontMaterial = haulUI.uiText.fontMaterial;
                _timerTextSemiUI = timerTextObject.AddComponent<SemiUI>();
            }
            else
            {
                Plugin.Logger.LogWarning("Wtf is going on with the HaulUI?");
            }
        }

        private void UpdateTimerText()
        {
            if (_timerText == null || !_canvas.activeSelf)
            {
                Plugin.Logger.LogWarning("TimerText or Canvas lost, reinitializing...");
                TryInitializeUI();
            }

            var minutes = Mathf.FloorToInt(Mathf.Max(_timer, 0f) / 60f);
            var seconds = Mathf.FloorToInt(Mathf.Max(_timer, 0f) % 60f);
            var text = $"{minutes:00}:{seconds:00}";

            if (_timerText == null) return;

            _timerText.text = text;
            if (_timer <= 60f)
            {
                _timerText.color = Color.red;
            }
            else if (_timer <= 120f)
            {
                _timerText.color = new Color(1f, 0.5f, 0f);
            }
            else
            {
                _timerText.color = Color.white;
            }
        }

        private static void UpdatePlayerList()
        {
            PlayerAvatars.Clear();
            foreach (var player in PhotonNetwork.PlayerList)
            {
                var playerAvatar = PlayerUtils.FindPlayerAvatar(player);
                if (playerAvatar == null) continue;

                PlayerAvatars.Add(playerAvatar);
                Plugin.Logger.LogInfo($"Player added to list: {player.NickName}");
            }
        }

        private static void SendRandomMessageToAll(List<string> messageList)
        {
            var list = messageList.OrderBy(x => Guid.NewGuid()).ToList();
            var num = 0;
            foreach (var playerAvatar in PlayerAvatars)
            {
                if (playerAvatar == null || !PlayerUtils.IsPlayerAlive(playerAvatar)) continue;

                var text = list[num % list.Count];
                playerAvatar.ChatMessageSend(text, false);
                num++;
            }
        }

        private static void ScheduleExplosions()
        {
            foreach (var playerAvatar in PlayerAvatars)
            {
                if (playerAvatar == null || playerAvatar.playerHealth == null || !PlayerUtils.IsPlayerAlive(playerAvatar)) continue;

                var text = DeathMessages[Random.Range(0, DeathMessages.Count)];
                playerAvatar.ChatMessageSend(text, false);
                var delay = text.Length * 0.25f;
                PendingExplosions.Add((playerAvatar, delay, text));
                Plugin.Logger.LogDebug($"Scheduled explosion for {playerAvatar.name} in {delay}s: '{text}'");
            }

            Instance._isTimerRunning = false;
        }

        private static void ProcessPendingExplosions(float deltaTime)
        {
            for (var i = PendingExplosions.Count - 1; i >= 0; i--)
            {
                var (avatar, delay, message) = PendingExplosions[i];
                delay -= deltaTime;
                if (delay <= 0f)
                {
                    if (avatar != null && avatar.playerHealth != null && PlayerUtils.IsPlayerAlive(avatar))
                    {
                        avatar.playerHealth.HurtOther(1000, Vector3.zero, false);
                        Plugin.Logger.LogDebug($"Player {avatar.name} exploded after saying: '{message}'!");
                    }

                    PendingExplosions.RemoveAt(i);
                }
                else
                {
                    PendingExplosions[i] = (avatar, delay, message);
                }
            }
        }

        public void HideTimerText()
        {
            _timerTextSemiUI?.Hide();
        }
    }
}