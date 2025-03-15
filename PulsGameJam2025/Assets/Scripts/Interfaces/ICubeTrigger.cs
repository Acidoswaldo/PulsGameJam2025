using UnityEngine;

public interface ICubeTrigger
{
    void OnCubeSideLanded(Enums.CubeSide sideThatLanded, PlayerController controller);
} 