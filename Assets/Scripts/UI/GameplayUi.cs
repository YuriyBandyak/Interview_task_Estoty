using PrimeTween;
using TMPro;
using UnityEngine;

public class GameplayUi : MonoBehaviour {

    [SerializeField] private TMP_Text _labelScore;
    [SerializeField] private RectTransform _health;

    private CanvasRenderer[] renderers;

    private int _score = 0;

    private void Awake() {
        _labelScore.text = "0";
        UpdateHealth(0);
        renderers = GetComponentsInChildren<CanvasRenderer>();
    }

    public void AddScore(int s) {
        _score += s;
        _labelScore.text = _score.ToString();
    }

    public void UpdateHealth(int h) {
        for (int i = 0; i < _health.childCount; i++)
        {
            _health.GetChild(i).gameObject.SetActive((i + 1) <= h);
        }
    }

    public void FadeOut() {
        var anim = Tween.Custom(1f, 0, duration: 1f, onValueChange: newVal => SetAllComponentsAlpha(newVal));
    }

    private void SetAllComponentsAlpha(float value) {
        foreach (var renderer in renderers)
        {
            renderer.SetAlpha(value);
        }
    }
}
