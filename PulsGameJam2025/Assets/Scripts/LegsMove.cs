using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEditor.Callbacks;

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
    PlayerController m_playerController;
    [SerializeField] Animator animator;
    [SerializeField] AnimationCurve walkCurve;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] bool executingAction;
    bool equipped;

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
                if (CheckGrounded()) animator.SetBool("isWalking", true);

                break;
            case Enums.Direction.left:
                moveDirection = Vector3.left;
                lookDirection = Vector3.left;
                if (CheckGrounded()) animator.SetBool("isWalking", true);
                break;
            default:
                moveDirection = Vector3.zero;
                animator.SetBool("isWalking", false);
                break;
        }
    }

    private bool CheckGrounded()
    {

        return Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, _groundCheckDistance, _groundLayer);
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
        float currentClipTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        var moveSpeed = moveDirection.x * _speed ;
        if(CheckGrounded()) moveSpeed *= walkCurve.Evaluate(currentClipTime);
        _rb.linearVelocity = new Vector3(moveSpeed, _rb.linearVelocity.y, 0);
    }

    public override void Equip(PlayerController playerController)
    {
        m_playerController = playerController;
        playerController.transform.rotation = HeadPosition.rotation;
        playerController.transform.position = HeadPosition.position;
        _rb.isKinematic = false;
        playerController.transform.SetParent(HeadPosition);
        equipped = true;
    }

    public override void Unequip()
    {
        if(!equipped) return;
        equipped = false;
        m_playerController.SetDefaultMover();
        transform.position = _startPosition;
        _rb.linearVelocity = Vector3.zero;
        _rb.isKinematic = true;
        action.Reset();
        animator.SetBool("isWalking", false);
    }

    public override void Action()
    {
        if (action == null) return;
        int uses = action.Execute();
        if (uses <= 0)
        {
          
          StartCoroutine(ActionSequence());
        }
    }

    IEnumerator ActionSequence()
    {
        yield return action.ExecuteCoroutine();
        Unequip();
    }
}