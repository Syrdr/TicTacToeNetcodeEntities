using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {


    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tieColor;
    [SerializeField] private Button rematchButton;



    private void Awake() {
        rematchButton.onClick.AddListener(() => {
            EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
            entityManager.CreateEntity(typeof(RematchRpc), typeof(SendRpcCommandRequest));
        });
    }

    private void Start() {
        DOTSEventsMonoBehaviour.Instance.OnGameWin += DOTSEventsMonoBehaviour_OnGameWin;
        DOTSEventsMonoBehaviour.Instance.OnGameRematch += DOTSEventsMonoBehaviour_OnGameRematch;
        DOTSEventsMonoBehaviour.Instance.OnGameTie += DOTSEventsMonoBehaviour_OnGameTie;

        Hide();
    }

    private void DOTSEventsMonoBehaviour_OnGameTie(object sender, System.EventArgs e) {
        resultTextMesh.text = "TIE!";
        resultTextMesh.color = tieColor;

        Show();
    }

    private void DOTSEventsMonoBehaviour_OnGameRematch(object sender, System.EventArgs e) {
        Hide();
    }

    private void DOTSEventsMonoBehaviour_OnGameWin(object sender, DOTSEventsMonoBehaviour.OnGameWinEventArgs e) {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameClientDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameClientData));
        GameClientData gameClientData = gameClientDataEntityQuery.GetSingleton<GameClientData>();

        if (gameClientData.localPlayerType == e.winningPlayerType) {
            resultTextMesh.text = "YOU WIN!";
            resultTextMesh.color = winColor;
        } else {
            resultTextMesh.text = "YOU LOSE!";
            resultTextMesh.color = loseColor;
        }

        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}