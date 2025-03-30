using UnityEngine;

public class ConnectingUI : MonoBehaviour {



    private void Start() {
        DOTSEventsMonoBehaviour.Instance.OnGameStarted += DOTSEventsMonoBehaviour_OnGameStarted;
    }

    private void DOTSEventsMonoBehaviour_OnGameStarted(object sender, System.EventArgs e) {
        Hide();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}