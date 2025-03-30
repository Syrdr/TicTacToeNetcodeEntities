using Unity.NetCode;
using UnityEngine;

public struct ClickedOnGridPositionRpc : IRpcCommand {

    public int x;
    public int y;
    public PlayerType playerType;

}