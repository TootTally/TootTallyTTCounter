using System.Collections.Generic;
using System.Linq;
using TMPro;
using TootTallyCore.APIServices;
using TootTallyCore.Utils.TootTallyGlobals;
using TootTallyDiffCalcLibs;
using TootTallyGameModifiers;
using UnityEngine;

namespace TootTallyTTCounter
{
    public class TTCounter : MonoBehaviour
    {
        private TMP_Text _counterText;
        private bool _isSongRated;
        public List<float[]> levelData;
        private Chart _chart;
        public string[] modifiers;
        private float _targetTT;
        private float _currentTT;
        private float _updateTimer;
        private float _timeSinceLastScore;

        void Awake()
        {
            _isSongRated = false;
            modifiers = null;
            _targetTT = 0;
            _currentTT = 0;
            _counterText = gameObject.GetComponent<TMP_Text>();
            _counterText.enableWordWrapping = false;
            _counterText.fontSize = 12;
            _counterText.alignment = TextAlignmentOptions.Center;
            var rect = _counterText.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .92f);
            rect.sizeDelta = Vector2.zero;
        }

        void Update()
        {
            _updateTimer += Time.unscaledDeltaTime;
            _timeSinceLastScore += Time.unscaledDeltaTime;
            if (_updateTimer > .06f && _currentTT != _targetTT)
            {
                _currentTT = EaseTTValue(_currentTT, _targetTT - _currentTT, _timeSinceLastScore, 2f);
                if (_currentTT < 0 || _targetTT < 0)
                    _currentTT = _targetTT = 0;
                UpdateTTText();
                _updateTimer = 0;
            }
        }

        public void OnScoreChanged(int totalScore, int hitCount, int noteIndex)
        {
            if (_chart.trackRef == "" || _chart.indexToMaxScoreDict == null || !_chart.indexToMaxScoreDict.ContainsKey(noteIndex)) return;
            float percent = totalScore / (float)_chart.indexToMaxScoreDict[noteIndex];
            _targetTT = Utils.CalculateScoreTT(_chart, TootTallyGlobalVariables.gameSpeedMultiplier, hitCount, _chart.indexToNoteCountDict[noteIndex] , percent, modifiers); //Estimate of custom curve
            _timeSinceLastScore = 0;
        }

        public void SetChartData(Chart chart, SerializableClass.SongDataFromDB songData = null)
        {
            if (chart.trackRef == "") return;
            _isSongRated = songData != null && songData.is_rated;
            _chart = chart;
            var modifiersString = GameModifierManager.GetModifiersString();
            if (modifiersString != "None")
                modifiers = modifiersString.Split(',');
            _targetTT = _currentTT = Utils.CalculateScoreTT(chart, TootTallyGlobalVariables.gameSpeedMultiplier, 1, 1, 1, modifiers);
            UpdateTTText();
        }

        private readonly float CHAR_SPACING = 7.2f;

        private void UpdateTTText()
        {
            var wholeNumber = (int)_currentTT;
            var decimalNumber = (_currentTT - (int)_currentTT).ToString("0.00").Substring(2);
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
