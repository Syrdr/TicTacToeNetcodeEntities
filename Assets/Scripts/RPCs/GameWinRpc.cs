using Unity.NetCode;
using UnityEngine;

public struct GameWinRpc : IRpcCommand {

    public PlayerType winningPlayerType;

}