using UnityEngine;

public abstract class MovementBase : MonoBehaviour, IMove
{
    [SerializeField] protected float moveSpeed = 5f;
    public abstract void Move(Enums.Direction direction);
    public abstract void Equip(PlayerController playerController);

    public abstract void Unequip();
    public abstract void Action();
}
