using PrimeTween;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUi : MonoBehaviour {

    [SerializeField] private VerticalLayoutGroup _contentHolder;
    [SerializeField] private TMP_Text _enemiesKilledCountText;
    [SerializeField] private TMP_Text _missedEnemiesCountText;
    [SerializeField] private TMP_Text _collisionWithEnemiesCountText;
    [SerializeField] private TMP_Text _shotsAccuracyPercentageText;
    [SerializeField] private TMP_Text _effectiveHealthPoewrUpsCountText;
    [SerializeField] private TMP_Text _pickedPowerUpsCountText;

    private CanvasRenderer[] renderers;
    private event Action _onRetryButtonAction;

    private Func<int> _enemiesKilledCountGetter;
    private Func<int> _totalEnemiesCountGetter;
    private Func<int> _collisionWithEnemiesCountGetter;
    private Func<float> _shotsAccuracyPercentageGetter;
    private Func<int> _effectiveHealthPoewrUpsCountGetter;
    private Func<int> _pickedPowerUpsCountGetter;

    public void Init(Action OnRetryButtonAction) {
        _onRetryButtonAction = OnRetryButtonAction;
        renderers = GetComponentsInChildren<CanvasRenderer>();
    }

    public void SetGetters(Func<int> enemiesKilledCountGetter, Func<int> totalEnemiesCountGetter, Func<int> collisionWithEnemiesCountGetter, Func<float> shotsAccuracyPercentageGetter, Func<int> effectiveHealthPoewrUpsCountGetter, Func<int> pickedPowerUpsCountGetter) {
        _enemiesKilledCountGetter = enemiesKilledCountGetter;
        _totalEnemiesCountGetter = totalEnemiesCountGetter;
        _collisionWithEnemiesCountGetter = collisionWithEnemiesCountGetter;
        _shotsAccuracyPercentageGetter = shotsAccuracyPercentageGetter;
        _effectiveHealthPoewrUpsCountGetter = effectiveHealthPoewrUpsCountGetter;
        _pickedPowerUpsCountGetter = pickedPowerUpsCountGetter;
    }

    public void Open() {
        SetAllComponentsAlpha(0);
        gameObject.SetActive(true);
        var anim = Tween.Custom(0, 1f, duration: 1f, onValueChange: newVal => SetAllComponentsAlpha(newVal));
        UpdateStats();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentHolder.transform as RectTransform);
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void OnRetryButton() {
        _onRetryButtonAction.Invoke();
    }

    private void SetAllComponentsAlpha(float value) {
        foreach (var renderer in renderers)
        {
            renderer.SetAlpha(value);
        }
    }

    private void UpdateStats() {
        var enemiesKilledCount = _enemiesKilledCountGetter.Invoke();
        _enemiesKilledCountText.text = enemiesKilledCount.ToString();
        _missedEnemiesCountText.text = (_totalEnemiesCountGetter.Invoke() - enemiesKilledCount).ToString();
        _collisionWithEnemiesCountText.text = _collisionWithEnemiesCountGetter.Invoke().ToString();
        var shotsPercentage = Mathf.RoundToInt(_shotsAccuracyPercentageGetter.Invoke() * 100f);
        _shotsAccuracyPercentageText.text = $"{shotsPercentage}%";
        _effectiveHealthPoewrUpsCountText.text = _effectiveHealthPoewrUpsCountGetter.Invoke().ToString();
        _pickedPowerUpsCountText.text = _pickedPowerUpsCountGetter.Invoke().ToString();
    }
}
