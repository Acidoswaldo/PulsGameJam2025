using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Tilemaps;
using System.Collections;

public class JumpAction : PartAction
{
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] int MaxUses;
    int Uses;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField, ReadOnly] private bool _isGrounded;
    [SerializeField] float cooldown = 1f;
    float jumpCooldown = 1f;
    [SerializeField] Animator animator;
    [SerializeField] float waitTime;


    void Start()
    {
        Reset();
    }
    void Update()
    {
        if (jumpCooldown > 0)
        {
            jumpCooldown -= Time.deltaTime;
        }
    }
    public override int Execute()
    {
        if (_rigidbody == null) { return 0; }
        if (jumpCooldown > 0) { return Uses; }
        if (CheckGrounded())
        {
            _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode.Impulse);
            Uses--;
            jumpCooldown = cooldown;
            if(animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
        return Uses;
    }

    public override IEnumerator ExecuteCoroutine(){
        yield return new WaitForSeconds(waitTime);
    }
    private bool CheckGrounded()
    {
        return Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, _groundCheckDistance, _groundLayer);
    }

    public override void Reset()
    {
        Uses = MaxUses;
    }
}

