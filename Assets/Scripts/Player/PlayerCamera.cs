using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player Targeting Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 cameraOffset = new Vector3(2f, 0f, 0f);
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 70f;
    [SerializeField] private float defaultDistance = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float collisionOffset = 0.2f;

    [Space]
    
    [SerializeField] private float lookaheadTime = 0.5f;
    [SerializeField] private float lookaheadSmoothing = 0.5f;
    [SerializeField] private float maxLookaheadDistance = 2f;
    [SerializeField] private float rollLookaheadMultiplier = 1.5f;
    [SerializeField] private float rotationLookaheadMultiplier = 0.5f;
    
    [Header("Lock on Settings")]
    [SerializeField] private float lockOnHeight = 1.5f;
    [SerializeField] private float lockOnDistance = 4f;
    [SerializeField] private float lockOnSmoothTime = 0.2f;
    [SerializeField] private float maxLockOnDistance = 15f;
    [SerializeField] private float transitionDuration = 0.5f;
    
    private PlayerMovement playerMovement;
    private float currentDistance;
    private float preLockDistance;
    private float xRotation;
    private float yRotation;
    private bool isLockedOn;
    private Transform lockOnTarget;
    private Vector3 lockOnPositionVelocity;
    private float transitionProgress;
    private bool isTransitioning;
    private Quaternion startRotation;
    private Vector3 startPosition;
    private Vector3 currentLookaheadOffset;
    private Vector3 lookaheadVelocity;
    private Vector3 lastCameraDirection;

    private void Start()
    {
        if (player == null)
        {
            enabled = false;
            return;
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        yRotation = transform.eulerAngles.y;
        xRotation = transform.eulerAngles.x;
        currentDistance = defaultDistance;
        preLockDistance = currentDistance;
        lastCameraDirection = transform.forward;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateLookahead()
    {
        if (playerMovement == null) return;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null) return;

        Vector3 targetLookaheadOffset = Vector3.zero;

        // Movement-based lookahead
        if (playerMovement.IsMoving())
        {
            float currentLookaheadTime = lookaheadTime;
            
            if (playerMovement.IsRolling())
            {
                currentLookaheadTime *= rollLookaheadMultiplier;
            }

            Vector3 velocity = controller.velocity;
            velocity.y = 0;
            
            targetLookaheadOffset = velocity * currentLookaheadTime;
        }

        //rotation-based lookahead
        Vector3 currentCameraDirection = transform.forward;
        Vector3 rotationDelta = currentCameraDirection - lastCameraDirection;
        targetLookaheadOffset += rotationDelta * rotationLookaheadMultiplier;
        lastCameraDirection = currentCameraDirection;

        targetLookaheadOffset = Vector3.ClampMagnitude(targetLookaheadOffset, maxLookaheadDistance);

        currentLookaheadOffset = Vector3.SmoothDamp(
            currentLookaheadOffset,
            targetLookaheadOffset,
            ref lookaheadVelocity,
            lookaheadSmoothing
        );
    }

    private void LateUpdate()
    {
        if (player == null) return;

        UpdateLookahead();

        if ((isLockedOn || isTransitioning) && lockOnTarget != null)
        {
            if (Vector3.Distance(player.position, lockOnTarget.position) > maxLockOnDistance)
            {
                isLockedOn = false;
                lockOnTarget = null;
                
                if (isTransitioning)
                {
                    transitionProgress = 1f - transitionProgress;
                    startRotation = transform.rotation;
                    startPosition = transform.position;
                }
                else
                {
                    isTransitioning = true;
                    transitionProgress = 0f;
                    startRotation = transform.rotation;
                    startPosition = transform.position;
                }
                
                currentDistance = preLockDistance;
            }
        }

        if (isTransitioning)
        {
            UpdateTransition();
        }
        else if (isLockedOn && lockOnTarget != null)
        {
            UpdateLockOnCamera();
        }
        else
        {
            UpdateFreeCamera();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleLockOn();
        }
    }

    private void UpdateTransition()
    {
        transitionProgress += Time.deltaTime / transitionDuration;
        
        if (transitionProgress >= 1f)
        {
            transitionProgress = 1f;
            isTransitioning = false;
            
            if (!isLockedOn)
            {
                currentDistance = preLockDistance;
            }
        }

        if (isLockedOn)
        {
            Vector3 playerPos = GetLockOnCameraPosition();
            transform.position = Vector3.Lerp(startPosition, playerPos, EaseInOutCubic(transitionProgress));
            transform.rotation = Quaternion.Lerp(startRotation, 
                Quaternion.LookRotation((GetLockOnLookTarget() - playerPos).normalized),
                EaseInOutCubic(transitionProgress));
        }
        else
        {
            Vector3 freeCameraPos = GetFreeCameraPosition();
            transform.position = Vector3.Lerp(startPosition, freeCameraPos, EaseInOutCubic(transitionProgress));
            transform.rotation = Quaternion.Lerp(startRotation,
                Quaternion.Euler(xRotation, yRotation, 0f),
                EaseInOutCubic(transitionProgress));
        }
    }

    private void UpdateFreeCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scroll * 5f, minDistance, maxDistance);
        preLockDistance = currentDistance;

        transform.position = GetFreeCameraPosition();
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private Vector3 GetFreeCameraPosition()
    {
        Vector3 basePlayerPos = player.position + playerOffset;
        Vector3 predictedPlayerPos = basePlayerPos + currentLookaheadOffset;
        
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        Vector3 offsetPosition = predictedPlayerPos + rotation * cameraOffset;
        Vector3 directionFromPlayer = rotation * Vector3.back;
        Vector3 desiredPosition = offsetPosition + directionFromPlayer * currentDistance;

        RaycastHit hit;
        if (Physics.Linecast(basePlayerPos, desiredPosition, out hit))
        {
            float adjustedDistance = Vector3.Distance(basePlayerPos, hit.point) - collisionOffset;
            currentDistance = Mathf.Clamp(adjustedDistance, minDistance, maxDistance);
            desiredPosition = offsetPosition + directionFromPlayer * currentDistance;
        }

        return desiredPosition;
    }

    private void UpdateLockOnCamera()
    {
        if (Vector3.Distance(player.position, lockOnTarget.position) > maxLockOnDistance)
        {
            ToggleLockOn();
            return;
        }

        transform.position = Vector3.SmoothDamp(transform.position, GetLockOnCameraPosition(), ref lockOnPositionVelocity, lockOnSmoothTime);
        transform.rotation = Quaternion.LookRotation((GetLockOnLookTarget() - transform.position).normalized);
    }

    private Vector3 GetLockOnCameraPosition()
    {
        Vector3 middlePoint = Vector3.Lerp(player.position + playerOffset, lockOnTarget.position, 0.5f);
        Vector3 directionToPlayer = (lockOnTarget.position - player.position).normalized;
        Vector3 offsetDirection = Vector3.Cross(Vector3.up, directionToPlayer).normalized;
        Vector3 offsetPosition = middlePoint + offsetDirection * cameraOffset.x;
        
        Vector3 targetCameraPos = offsetPosition - directionToPlayer * lockOnDistance;
        targetCameraPos.y = middlePoint.y + lockOnHeight;
        
        return targetCameraPos;
    }

    private Vector3 GetLockOnLookTarget()
    {
        return Vector3.Lerp(player.position, lockOnTarget.position, 0.5f);
    }

    private void ToggleLockOn()
    {
        if (isLockedOn)
        {
            isLockedOn = false;
            lockOnTarget = null;
        }
        else
        {
            Transform nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                isLockedOn = true;
                lockOnTarget = nearestEnemy;
            }
            else
            {
                return;
            }
        }

        isTransitioning = true;
        transitionProgress = 0f;
        startRotation = transform.rotation;
        startPosition = transform.position;
    }

    private float EaseInOutCubic(float x)
    {
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float nearestDistance = maxLockOnDistance;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                Vector3 directionToEnemy = (enemy.transform.position - player.position).normalized;
                float dotProduct = Vector3.Dot(player.forward, directionToEnemy);
                
                if (dotProduct > 0)
                {
                    nearestDistance = distance;
                    nearest = enemy.transform;
                }
            }
        }

        return nearest;
    }

    public void SetTarget(Transform newPlayer)
    {
        player = newPlayer;
    }

    public bool IsLockedOn() => isLockedOn;
    public Transform GetLockOnTarget() => lockOnTarget;
}