using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MovementBase defaultMover;
    [SerializeField] private MovementBase mover; // This should be of type IMove

    void Start()
    {
        defaultMover = GetComponent<MovementBase>();
    }
    void Update()
    {
        MoveInput();
        ActionInput();
    }

    void ActionInput()
    {
        if (Input.GetKey(KeyCode.Space)) mover.Action();
    }
    void MoveInput()
    {
        if (Input.GetKey(KeyCode.A)) mover.Move(Enums.Direction.left);
        else if (Input.GetKey(KeyCode.D)) mover.Move(Enums.Direction.right);
        else if (Input.GetKey(KeyCode.W)) mover.Move(Enums.Direction.forward);
        else if (Input.GetKey(KeyCode.S)) mover.Move(Enums.Direction.back);
        else mover.Move(Enums.Direction.none);
    }

    public void SetMover(MovementBase mover)
    {
        this.mover = mover;
        mover.Equip(this);
    }

    internal void SetDefaultMover()
    {
      SetMover(defaultMover);
    }
}
