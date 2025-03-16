using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEditor.Callbacks;
using System.Data.Common;
using Unity.Mathematics;
using System;
using Sirenix.OdinInspector;

public class BoxMove : MovementBase
{
    [SerializeField] private float _rollSpeed = 5;
    private bool _isMoving;
    [SerializeField] private Enums.CubeSide _currentDownSide = Enums.CubeSide.Bottom;
    [SerializeField] private MMF_Player stepFeedback;
    [SerializeField] private MMF_Player cannotMoveFeedback;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private ParticleSystem stepPartices;
    [SerializeField] private Transform[] sides;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] Rigidbody _rb;
    [SerializeField] Collider _collider;
    [SerializeField, Range(0f, 30f)] private float _EquipForce = 5f;
    bool canMove = false;
    bool isLerping = false;
    public HashSet<Collider> safeZones = new HashSet<Collider>();
    [SerializeField] Transform _CurrentCheckpoint;


    void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_collider == null) _collider = GetComponent<Collider>();
        canMove = true;
    }

    private bool CanMoveInDirection(Vector3 direction)
    {
        // Cast a ray from the center of the cube in the movement direction
        float rayDistance = 1f; // Distance of 1 unit (size of the cube)
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;

        // Debug ray for visualization in scene view
        Debug.DrawRay(transform.position, direction * rayDistance, Color.red, 0.1f);

        // Check if there's anything blocking the movement
        if (Physics.Raycast(ray, out hit, rayDistance, wallLayer))
        {
            // If we hit something that's not a trigger, movement is blocked
            if (!hit.collider.isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    public override void Move(Enums.Direction dir)
    {
        if (!canMove) return;
        if (_isMoving) return;
        if (dir == Enums.Direction.none) return;

        Vector3 moveDirection = Vector3.zero;

        switch (dir)
        {
            case Enums.Direction.left:
                moveDirection = Vector3.left;
                break;
            case Enums.Direction.right:
                moveDirection = Vector3.right;
                break;
            case Enums.Direction.forward:
                moveDirection = Vector3.forward;
                break;
            case Enums.Direction.back:
                moveDirection = Vector3.back;
                break;
        }

        // Check if movement is possible before attempting to move
        if (CanMoveInDirection(moveDirection))
        {
            Assemble(moveDirection);
        }
        else
        {
            halfAssemble(moveDirection);
        }
    }

    private void halfAssemble(Vector3 dir)
    {
        var anchor = transform.position + (Vector3.down + dir) * 0.5f;
        var axis = Vector3.Cross(Vector3.up, dir);
        StartCoroutine(HalfRoll(anchor, axis));
    }

    private void Assemble(Vector3 dir)
    {
        var anchor = transform.position + (Vector3.down + dir) * 0.5f;
        var axis = Vector3.Cross(Vector3.up, dir);
        StartCoroutine(Roll(anchor, axis));
    }

    private IEnumerator HalfRoll(Vector3 anchor, Vector3 axis)
    {
        _isMoving = true;
        if (cannotMoveFeedback) yield return cannotMoveFeedback.PlayFeedbacksCoroutine(transform.position);
        SetParticlePosition();
        stepPartices.Play();
        stepFeedback.PlayFeedbacks();

        _isMoving = false;
    }


    private IEnumerator Roll(Vector3 anchor, Vector3 axis)
    {
        _isMoving = true;
        for (var i = 0; i < 90 / _rollSpeed; i++)
        {
            transform.RotateAround(anchor, axis, _rollSpeed);
            yield return new WaitForSeconds(0.01f);
        }
        // Perform raycast after rolling is complete
        SideCheck();
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

 private void SideCheck()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 1f);
        foreach (RaycastHit hit in hits)
        {
            // Check for Side script
            var sideScript = hit.collider.GetComponent<Side>();
            if (sideScript != null)
            {
                // Add any additional logic you need with the side information
                _currentDownSide = sideScript.side;
            }
        }
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
        }
    }

    public override void Equip(PlayerController playerController)
    {
        canMove = false;
        _collider.enabled = true;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        playerController.transform.SetParent(null);
        StartCoroutine(EquipRoutine());
    }

    IEnumerator EquipRoutine()
    {
        yield return new WaitForEndOfFrame();
        _rb.AddForce((Vector3.up + transform.forward).normalized * _EquipForce, ForceMode.Impulse);
    }

    public override void Action()
    {

    }

    public override void Unequip()
    {
        canMove = false;
        _collider.enabled = false;
        _rb.isKinematic = true;
        _rb.useGravity = false;
    }

    private IEnumerator LerpTransform()
    {
        canMove = false;
        isLerping = true;

        // Get start and target values for both rotation and position
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = GetClosestRightAngleRotation(startRotation);
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(
            Mathf.Round(startPosition.x),
            startPosition.y,
            Mathf.Round(startPosition.z)
        );

        float elapsedTime = 0f;
        float lerpDuration = 0.2f;

        while (elapsedTime < lerpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lerpDuration;

            // Lerp both rotation and position
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Ensure we end up exactly at the targets
        transform.rotation = targetRotation;
        transform.position = targetPosition;

        isLerping = false;
        canMove = true;

    }

    private Quaternion GetClosestRightAngleRotation(Quaternion currentRotation)
    {
        Vector3 euler = currentRotation.eulerAngles;
        return Quaternion.Euler(
            Mathf.Round(euler.x / 90f) * 90f,
            Mathf.Round(euler.y / 90f) * 90f,
            Mathf.Round(euler.z / 90f) * 90f
        );
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!CheckIfInSafeZone())
            {
                if (_CurrentCheckpoint != null) transform.position = _CurrentCheckpoint.position;
            }
            _collider.enabled = false;
            _rb.isKinematic = true;
            _rb.useGravity = false;
            if (!isLerping) StartCoroutine(LerpTransform());
        }

        bool CheckIfInSafeZone()
        {
            Collider[] overlappingColliders = Physics.OverlapSphere(transform.position, 0.1f);
            foreach (Collider col in overlappingColliders)
            {
                if (col.isTrigger && col.gameObject.layer == LayerMask.NameToLayer("Safe"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}