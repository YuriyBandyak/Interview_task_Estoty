using System;
using UnityEngine;

public class GameOverUi : MonoBehaviour {

    private event Action _onRetryButtonAction;

    public void Init(Action OnRetryButtonAction) {
        _onRetryButtonAction = OnRetryButtonAction;
    }

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
    }

    public void OnRetryButton() {
        _onRetryButtonAction.Invoke();
    }

}
