using UnityEngine;

public class LegsEquiper : MonoBehaviour, ICubeTrigger
{
    [SerializeField] private MovementBase mover;
    public void OnCubeSideLanded(Enums.CubeSide sideThatLanded, PlayerController controller)
    {
        Debug.Log($"Side that landed: {sideThatLanded}");
        if(sideThatLanded == Enums.CubeSide.Bottom)
        {
            controller.SetMover(mover);
        }
    }
}
