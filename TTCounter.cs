using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using TootTallyCore.APIServices;
using TootTallyCore.Utils.Helpers;
using TootTallyLeaderboard.Replays;
using UnityEngine;

namespace TootTallyTTCounter
{
    public class TTCounter : MonoBehaviour
    {
        private int _gameMaxScore;
        private TMP_Text _counterText;
        private bool _isSongRated;
        private float _baseChartTT;
        public List<float[]> levelData;
        private float _targetTT;
        private float _currentTT;
        private float _updateTimer;
        private float _timeSinceLastScore;

        void Awake()
        {
            _isSongRated = false;
            _gameMaxScore = 0;
            _baseChartTT = 0;
            _targetTT = 0;
            _currentTT = 0;
            _counterText = gameObject.GetComponent<TMP_Text>();
            _counterText.enableWordWrapping = false;
            _counterText.fontSize = 12;
            _counterText.alignment = TextAlignmentOptions.Left;
            var rect = _counterText.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.08f, 0);
            rect.sizeDelta = Vector2.zero;
        }

        void Update()
        {
            _updateTimer += Time.deltaTime;
            _timeSinceLastScore += Time.deltaTime;
            if (_updateTimer > .06f && _currentTT != _targetTT)
            {
                _currentTT = EaseTTValue(_currentTT, _targetTT - _currentTT, _timeSinceLastScore, 2f);
                UpdateTTText();
                _updateTimer = 0;
            }
        }

        public void OnScoreChanged(int totalScore, int noteIndex)
        {
            _gameMaxScore += TTUtils.GetRealMax(levelData[noteIndex][1], noteIndex);
            float percent = (float)totalScore / _gameMaxScore;
            _targetTT = TTUtils.CalculateScoreTT(_baseChartTT, percent);
            _timeSinceLastScore = 0;
        }

        public void SetChartData(SerializableClass.SongDataFromDB songData)
        {
            _isSongRated = songData.is_rated;
            _baseChartTT = CalcBaseTTFromSongData(songData);
            _targetTT = _currentTT = TTUtils.CalculateScoreTT(_baseChartTT, 1);
            UpdateTTText();
        }

        private static float CalcBaseTTFromSongData(SerializableClass.SongDataFromDB songData)
        {
            var gameSpeed = ReplaySystemManager.gameSpeedMultiplier;
            float diffIndex = Mathf.Clamp((int)((gameSpeed - .5f) / .25f),0,5);
            

            float diffMin = diffIndex * .25f + .5f;
            float diffMax = diffMin + .25f;

            float by = (gameSpeed - diffMin) / (diffMax - diffMin);

            float diff = EasingHelper.Lerp(songData.speed_diffs[(int)diffIndex], songData.speed_diffs[(int)diffIndex + 1], by);
            return TTUtils.CalculateBaseTT(diff);
        }

        private readonly float CHAR_SPACING = 7.2f;

        private void UpdateTTText()
        {
            var wholeNumber = (int)_currentTT;
            var decimalNumber = (_currentTT - (int)_currentTT).ToString("0.00", CultureInfo.InvariantCulture).Substring(2);
            _counterText.text =
                    $"<mspace=mspace={CHAR_SPACING}>{wholeNumber}</mspace>" + //Int part of the number
                    $"." +
                    $"<mspace=mspace={CHAR_SPACING}>{decimalNumber}</mspace>tt" + //Float part of the number, don't ask.
                    $"{(_isSongRated ? "" : "(Unrated) ")}";
        }

        private float EaseTTValue(float currentTT, float diff, float timeSum, float duration) =>
            Mathf.Max(diff * (-Mathf.Pow(2f, -10f * timeSum / duration) + 1f) * 1024f / 1023f + currentTT, 0f);
    }
}
