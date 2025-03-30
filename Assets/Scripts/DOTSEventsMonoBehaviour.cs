using System;
using UnityEngine;

public class DOTSEventsMonoBehaviour : MonoBehaviour {


    public static DOTSEventsMonoBehaviour Instance { get; private set; }



    public event EventHandler<OnClientConnectedEventArgs> OnClientConnectedEvent;
    public class OnClientConnectedEventArgs : EventArgs {
        public int connectionId;
    }
    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs {
        public PlayerType winningPlayerType;
    }
    public event EventHandler OnGameRematch;
    public event EventHandler OnGameTie;


    private void Awake() {
        Instance = this;
    }

    public void TriggerOnClientConnectedEvent(int connectionId) {
        OnClientConnectedEvent?.Invoke(this, new OnClientConnectedEventArgs {
            connectionId = connectionId,
        });
    }

    public void TriggerOnGameStarted() {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerOnGameWin(PlayerType winningPlayerType) {
        OnGameWin?.Invoke(this, new OnGameWinEventArgs {
            winningPlayerType = winningPlayerType
        });
    }

    public void TriggerOnGameRematch() {
        OnGameRematch?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerOnGameTie() {
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }
}