using UnityEngine;
using Sirenix.OdinInspector;

public class LegsMove : MovementBase
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnSpeed = 360;
    [ReadOnly, SerializeField] private Vector3 moveDirection;
    Vector3 lookDirection;
    [SerializeField] Transform HeadPosition;
    [SerializeField] Transform visuals;
    [SerializeField] private PartAction action;
    Vector3 _startPosition;

    [Header("Jump Settings")]

    PlayerController m_playerController;

    void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();
        if (action == null)
            action = GetComponent<PartAction>();
    }
    void Start()
    {
        _startPosition = transform.position;
    }
    private void Update()
    {
        Look();
    }

    public override void Move(Enums.Direction direction)
    {
        switch (direction)
        {
            case Enums.Direction.right:
                moveDirection = Vector3.right;
                lookDirection = Vector3.right;
                break;
            case Enums.Direction.left:
                moveDirection = Vector3.left;
                lookDirection = Vector3.left;
                break;
            default:
                moveDirection = Vector3.zero;
                break;
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }


    private void Look()
    {
        var rot = Quaternion.LookRotation(lookDirection, Vector3.up);
        visuals.rotation = Quaternion.RotateTowards(visuals.rotation, rot, _turnSpeed * Time.deltaTime);
    }

    private void ApplyMovement()
    {
        _rb.linearVelocity = new Vector3(moveDirection.x * _speed, _rb.linearVelocity.y, 0);
    }

    public override void Equip(PlayerController playerController)
    {
        m_playerController = playerController;
        playerController.transform.rotation = HeadPosition.rotation;
        playerController.transform.position = HeadPosition.position;
        playerController.transform.SetParent(HeadPosition);
    }

    public override void Unequip()
    {
        m_playerController.SetDefaultMover();
        transform.position = _startPosition;
        _rb.linearVelocity = Vector3.zero;
        action.Reset();
    }

    public override void Action()
    {
        if (action == null) return;
        int uses = action.Execute();
        if (uses <= 0)
        {
            Unequip();
        }
    }
}