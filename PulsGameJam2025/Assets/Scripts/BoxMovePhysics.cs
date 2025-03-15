using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

public class BoxMovePhysics : MovementBase
{
    [SerializeField] private float _rollForce = 5f;
    private bool _isMoving;
    [SerializeField] private Enums.CubeSide _currentDownSide = Enums.CubeSide.Bottom;
    [SerializeField] private MMF_Player stepFeedback;
    [SerializeField] private ParticleSystem stepPartices;
    [SerializeField] private Transform[] sides;
    [SerializeField] private PlayerController _playerController;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Only freeze rotation on X and Z axes, allow Y rotation and all position movement
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void Move(Enums.Direction dir)
    {
        if (_isMoving) return;
        if(dir == Enums.Direction.none) return;
        
        Vector3 moveDirection = dir switch
        {
            Enums.Direction.left => Vector3.left,
            Enums.Direction.right => Vector3.right,
            Enums.Direction.forward => Vector3.forward,
            Enums.Direction.back => Vector3.back,
            _ => Vector3.zero
        };
        
        if (moveDirection != Vector3.zero)
        {
            StartCoroutine(RollPhysics(moveDirection));
        }
    }

    private IEnumerator RollPhysics(Vector3 dir)
    {
        _isMoving = true;
        
        // Calculate rotation point and axis
        Vector3 anchor = transform.position + (Vector3.down + dir) * 0.5f;
        Vector3 axis = Vector3.Cross(Vector3.up, dir);
        
        // Create a temporary pivot object
        GameObject pivot = new GameObject("RollPivot");
        pivot.transform.position = anchor;
        
        // Parent the cube to the pivot
        Transform originalParent = transform.parent;
        transform.parent = pivot.transform;
        
        // Apply torque for rotation
        float targetAngle = 90f;
        float currentAngle = 0f;
        
        while (currentAngle < targetAngle)
        {
            float rotationStep = _rollForce * Time.deltaTime;
            currentAngle += rotationStep;
            pivot.transform.rotation *= Quaternion.AngleAxis(rotationStep, axis);
            yield return null;
        }
        
        // Reset parent and cleanup
        transform.parent = originalParent;
        Destroy(pivot);
        
        // Snap to grid
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );
        
        // Post-roll actions
        DownwardRay();
        SetParticlePosition();
        stepPartices.Play();
        stepFeedback.PlayFeedbacks();
        
        _isMoving = false;
    }

    private void SetParticlePosition()
    {
        stepPartices.transform.position = sides[(int)_currentDownSide].position;
    }

    private void DownwardRay()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 1f);
        foreach (RaycastHit hit in hits)
        {
            // Check for ICubeTrigger
            var trigger = hit.collider.GetComponent<ICubeTrigger>();
            if (trigger != null)
            {
                trigger.OnCubeSideLanded(_currentDownSide, _playerController);
            }

            // Check for Side script
            var sideScript = hit.collider.GetComponent<Side>();
            if (sideScript != null)
            {
                // Add any additional logic you need with the side information
                _currentDownSide = sideScript.side;
            }
        }
    }

    public override void Equip(PlayerController playerController)
    {
        playerController.transform.SetParent(null);
    }

    public override void Unequip()
    {
     
    }

    public override void Action()
    {
     
    }
}
