using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowShoot : MonoBehaviour
{
    // =========================================================
    // BOW
    // =========================================================

    [Header("BOW POINTS")]
    [SerializeField] private Transform stringTop;
    [SerializeField] private Transform stringBottom;
    [SerializeField] private Transform pullPoint;
    [SerializeField] private Transform grabHandle;
    [SerializeField] private Transform bowCenter;


    // =========================================================
    // HANDS
    // =========================================================

    [Header("HANDS")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;


    // =========================================================
    // RIGHT HAND INPUT
    // =========================================================

    [Header("RIGHT HAND INPUT")]

    // Gán:
    // XRI Right Interaction / Select Value
    [SerializeField]
    private InputActionReference rightGripAction;


    // =========================================================
    // ARROW
    // =========================================================

    [Header("ARROW")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;


    [Header("ARROW ROTATION")]
    [SerializeField]
    private Vector3 arrowRotationOffset =
        new Vector3(-90f, 0f, 0f);


    // =========================================================
    // GRAB
    // =========================================================

    [Header("STRING GRAB")]

    [SerializeField]
    private float grabDistance = 0.20f;


    // =========================================================
    // PULL
    // =========================================================

    [Header("PULL")]

    [SerializeField]
    private float maxPullDistance = 0.5f;

    [SerializeField]
    private float minPullToShoot = 0.05f;


    // =========================================================
    // SHOOT
    // =========================================================

    [Header("SHOOT")]

    [SerializeField]
    private float minShootSpeed = 3f;

    [SerializeField]
    private float maxShootSpeed = 20f;


    // =========================================================
    // COLLISION
    // =========================================================

    [Header("COLLISION")]

    [SerializeField]
    private float ignoreCollisionTime = 0.15f;


    // =========================================================
    // PRIVATE
    // =========================================================

    private GameObject currentArrow;
    private Rigidbody currentArrowRb;
    private Transform currentArrowNockPoint;


    // Vị trí nghỉ
    private Vector3 pullPointRestLocalPosition;

    private Vector3 grabHandleRestLocalPosition;
    private Quaternion grabHandleRestLocalRotation;


    // Có đang trong một chu kỳ kéo dây không
    private bool isPulling = false;


    // Grip tay phải ở frame trước
    private bool previousRightGrip = false;


    // Lực kéo hiện tại
    private float currentPullDistance = 0f;


    // =========================================================
    // MỐC TAY PHẢI
    //
    // Khi bắt đầu kéo hoặc quay lại tay phải,
    // vị trí tay hiện tại được dùng làm mốc.
    //
    // Sau đó chỉ tính phần DI CHUYỂN thêm của tay.
    //
    // Nhờ vậy xoay cung bằng tay trái sẽ không làm
    // currentPullDistance nhảy về 0.
    // =========================================================

    private Vector3 rightHandAnchorPosition;

    private float pullDistanceAtAnchor = 0f;

    private bool hasRightHandAnchor = false;


    private Collider[] bowColliders;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (pullPoint != null)
        {
            pullPointRestLocalPosition =
                pullPoint.localPosition;
        }


        if (grabHandle != null)
        {
            grabHandleRestLocalPosition =
                grabHandle.localPosition;

            grabHandleRestLocalRotation =
                grabHandle.localRotation;
        }


        bowColliders =
            GetComponentsInChildren<Collider>(
                true
            );
    }


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (rightGripAction != null)
        {
            rightGripAction.action.Enable();
        }
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        if (rightGripAction != null)
        {
            rightGripAction.action.Disable();
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleRightGrip();
    }


    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        if (!isPulling)
            return;


        bool rightGrip =
            IsRightGripPressed();


        // =====================================================
        // CHỈ THAY ĐỔI LỰC KHI TAY PHẢI ĐANG ACTIVE
        // =====================================================

        if (rightGrip &&
            hasRightHandAnchor)
        {
            UpdatePullDistanceFromRightHand();
        }


        // =====================================================
        // DÙ TAY PHẢI ACTIVE HAY KHÔNG
        //
        // Vẫn ép PullPoint theo lực đã lưu.
        //
        // Khi xoay cung bằng tay trái,
        // dây sẽ xoay theo cung nhưng giữ nguyên độ kéo.
        // =====================================================

        ApplyStoredPullDistance();


        UpdateArrow();
    }


    // =========================================================
    // RIGHT GRIP
    // =========================================================

    private bool IsRightGripPressed()
    {
        if (rightGripAction == null)
            return false;


        if (rightGripAction.action == null)
            return false;


        return
            rightGripAction.action.ReadValue<float>()
            > 0.5f;
    }


    // =========================================================
    // HANDLE RIGHT GRIP
    // =========================================================

    private void HandleRightGrip()
    {
        bool rightGrip =
            IsRightGripPressed();


        // =====================================================
        // GRIP VỪA BẬT
        // =====================================================

        if (rightGrip &&
            !previousRightGrip)
        {
            if (!isPulling)
            {
                // Một phát bắn mới
                TryStartPull();
            }
            else
            {
                // =================================================
                // ĐANG CÓ TÊN + ĐANG GIỮ LỰC
                //
                // Nghĩa là:
                // vừa chỉnh cung bằng tay trái
                // rồi quay lại tay phải.
                //
                // Tạo mốc mới nhưng KHÔNG reset lực.
                // =================================================

                SetRightHandAnchor();


                Debug.Log(
                    "QUAY LAI TAY PHAI | GIU LUC = " +
                    currentPullDistance
                );
            }
        }


        // =====================================================
        // GRIP TAY PHẢI VỪA TẮT
        // =====================================================

        if (!rightGrip &&
            previousRightGrip &&
            isPulling)
        {
            // =================================================
            // Nếu G bàn phím vẫn đang được giữ
            //
            // -> người dùng chỉ vừa chuyển từ Right sang Left.
            // -> KHÔNG BẮN.
            // =================================================

            if (Keyboard.current != null &&
                Keyboard.current.gKey.isPressed)
            {
                hasRightHandAnchor = false;


                Debug.Log(
                    "CHUYEN SANG TAY TRAI | GIU LUC = " +
                    currentPullDistance
                );
            }
            else
            {
                // =================================================
                // G thật sự được THẢ
                // -> BẮN
                // =================================================

                TryShoot();
            }
        }


        previousRightGrip =
            rightGrip;
    }


    // =========================================================
    // START PULL
    // =========================================================

    private void TryStartPull()
    {
        if (rightHand == null ||
            pullPoint == null)
        {
            return;
        }


        // =====================================================
        // KHÔNG ĐO VỚI GrabHandle.position NỮA.
        //
        // GrabHandle có thể đã bị XR kéo đi.
        //
        // Ta đo với VỊ TRÍ NGHỈ CỦA DÂY.
        // =====================================================

        Vector3 stringRestPosition =
            GetStringRestWorldPosition();


        float distance =
            Vector3.Distance(
                rightHand.position,
                stringRestPosition
            );


        if (distance > grabDistance)
        {
            Debug.Log(
                "RIGHT HAND CHUA GAN DAY | Distance = " +
                distance
            );

            return;
        }


        // Đã có tên rồi thì tuyệt đối không spawn thêm
        if (currentArrow != null)
            return;


        isPulling = true;


        currentPullDistance = 0f;


        SetRightHandAnchor();


        SpawnArrow();


        Debug.Log(
            "BAT DAU KEO DAY"
        );
    }


    // =========================================================
    // LẤY VỊ TRÍ NGHỈ CỦA DÂY
    // =========================================================

    private Vector3 GetStringRestWorldPosition()
    {
        if (pullPoint == null)
            return transform.position;


        return pullPoint.parent.TransformPoint(
            pullPointRestLocalPosition
        );
    }


    // =========================================================
    // SET MỐC TAY PHẢI
    // =========================================================

    private void SetRightHandAnchor()
    {
        if (rightHand == null)
            return;


        rightHandAnchorPosition =
            rightHand.position;


        pullDistanceAtAnchor =
            currentPullDistance;


        hasRightHandAnchor = true;
    }


    // =========================================================
    // UPDATE PULL DISTANCE
    // =========================================================

    private void UpdatePullDistanceFromRightHand()
    {
        if (rightHand == null)
            return;


        Vector3 shootDirection =
            GetShootDirection();


        Vector3 pullDirection =
            -shootDirection;


        // =====================================================
        // CHỈ LẤY DELTA TỪ MỐC TAY PHẢI
        //
        // Không lấy khoảng cách tuyệt đối tới cung.
        //
        // Vì vậy cung có di chuyển/xoay do tay trái
        // cũng không làm mất lực.
        // =====================================================

        Vector3 handDelta =
            rightHand.position -
            rightHandAnchorPosition;


        float extraPull =
            Vector3.Dot(
                handDelta,
                pullDirection
            );


        currentPullDistance =
            pullDistanceAtAnchor +
            extraPull;


        currentPullDistance =
            Mathf.Clamp(
                currentPullDistance,
                0f,
                maxPullDistance
            );
    }


    // =========================================================
    // APPLY STORED PULL
    // =========================================================

    private void ApplyStoredPullDistance()
    {
        if (pullPoint == null)
            return;


        Vector3 restPosition =
            GetStringRestWorldPosition();


        Vector3 pullDirection =
            -GetShootDirection();


        pullPoint.position =
            restPosition +
            pullDirection *
            currentPullDistance;
    }


    // =========================================================
    // SHOOT DIRECTION
    // =========================================================

    private Vector3 GetShootDirection()
    {
        if (pullPoint == null ||
            bowCenter == null)
        {
            return transform.forward;
        }


        Vector3 restPosition =
            GetStringRestWorldPosition();


        Vector3 direction =
            bowCenter.position -
            restPosition;


        if (direction.sqrMagnitude <
            0.0001f)
        {
            return transform.forward;
        }


        return direction.normalized;
    }


    // =========================================================
    // BOW UP
    // =========================================================

    private Vector3 GetBowUp()
    {
        if (stringTop == null ||
            stringBottom == null)
        {
            return transform.up;
        }


        Vector3 up =
            stringTop.position -
            stringBottom.position;


        if (up.sqrMagnitude <
            0.0001f)
        {
            return transform.up;
        }


        return up.normalized;
    }


    // =========================================================
    // SPAWN ARROW
    // =========================================================

    private void SpawnArrow()
    {
        if (currentArrow != null)
            return;


        if (arrowPrefab == null ||
            arrowSpawnPoint == null)
        {
            isPulling = false;

            return;
        }


        currentArrow =
            Instantiate(
                arrowPrefab,
                arrowSpawnPoint.position,
                Quaternion.identity
            );


        currentArrowRb =
            currentArrow.GetComponent<Rigidbody>();


        if (currentArrowRb == null)
        {
            Debug.LogError(
                "ARROW KHONG CO RIGIDBODY!"
            );


            Destroy(currentArrow);


            currentArrow = null;

            isPulling = false;


            return;
        }


        currentArrowNockPoint =
            FindChildByName(
                currentArrow.transform,
                "ArrowNockPoint"
            );


        if (currentArrowNockPoint == null)
        {
            Debug.LogError(
                "KHONG TIM THAY ArrowNockPoint!"
            );


            Destroy(currentArrow);


            currentArrow = null;

            currentArrowRb = null;

            isPulling = false;


            return;
        }


        currentArrowRb.isKinematic = true;

        currentArrowRb.useGravity = false;


        AlignArrowToString();


        Debug.Log(
            $"TAO TEN | Cung: {gameObject.name} | Script ID: {GetEntityId()}",
           this);
    }


    // =========================================================
    // FIND CHILD
    // =========================================================

    private Transform FindChildByName(
        Transform parent,
        string childName
    )
    {
        Transform[] children =
            parent.GetComponentsInChildren<Transform>(
                true
            );


        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child;
            }
        }


        return null;
    }


    // =========================================================
    // UPDATE ARROW
    // =========================================================

    private void UpdateArrow()
    {
        if (currentArrow == null)
            return;


        AlignArrowToString();
    }


    // =========================================================
    // ALIGN ARROW
    // =========================================================

    private void AlignArrowToString()
    {
        if (currentArrow == null ||
            currentArrowNockPoint == null ||
            arrowSpawnPoint == null)
        {
            return;
        }


        Vector3 shootDirection =
            GetShootDirection();


        Vector3 bowUp =
            GetBowUp();


        Quaternion rotation =
            Quaternion.LookRotation(
                shootDirection,
                bowUp
            );


        currentArrow.transform.rotation =
            rotation *
            Quaternion.Euler(
                arrowRotationOffset
            );


        Vector3 offset =
            arrowSpawnPoint.position -
            currentArrowNockPoint.position;


        currentArrow.transform.position +=
            offset;
    }


    // =========================================================
    // TRY SHOOT
    // =========================================================

    private void TryShoot()
    {
        if (!isPulling)
            return;


        // =====================================================
        // KHÔNG BẮN TÊN LỰC 0
        // =====================================================

        if (currentPullDistance <
            minPullToShoot)
        {
            Debug.Log(
                "LUC KEO QUA NHO -> KHONG BAN"
            );


            CancelArrow();

            return;
        }


        ReleaseString();
    }


    // =========================================================
    // RELEASE STRING
    // =========================================================

    private void ReleaseString()
    {
        if (!isPulling)
            return;


        // =====================================================
        // LƯU LỰC TRƯỚC KHI BẤT KỲ THỨ GÌ RESET
        // =====================================================

        float savedPullDistance =
            currentPullDistance;


        Vector3 savedShootDirection =
            GetShootDirection();


        Debug.Log(
            "RELEASE | Saved Pull = " +
            savedPullDistance
        );


        isPulling = false;

        hasRightHandAnchor = false;


        ShootArrow(
            savedPullDistance,
            savedShootDirection
        );


        ResetBowString();


        currentPullDistance = 0f;
    }


    // =========================================================
    // SHOOT
    // =========================================================

    private void ShootArrow(
        float savedPullDistance,
        Vector3 savedShootDirection
    )
    {
        if (currentArrow == null ||
            currentArrowRb == null)
        {
            return;
        }


        // Căn lần cuối
        AlignArrowToString();


        float pullPercent =
            savedPullDistance /
            maxPullDistance;


        pullPercent =
            Mathf.Clamp01(
                pullPercent
            );


        float shootSpeed =
            Mathf.Lerp(
                minShootSpeed,
                maxShootSpeed,
                pullPercent
            );


        GameObject firedArrow =
            currentArrow;


        Rigidbody firedRb =
            currentArrowRb;


        SetArrowCollisionIgnored(
            firedArrow,
            true
        );


        firedRb.isKinematic = false;

        firedRb.useGravity = true;


        firedRb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;


        firedRb.interpolation =
            RigidbodyInterpolation.Interpolate;


        firedRb.angularVelocity =
            Vector3.zero;


        firedRb.linearVelocity =
            savedShootDirection *
            shootSpeed;


        Debug.Log(
            "BAN TEN | Pull = " +
            savedPullDistance +
            " | Power = " +
            pullPercent +
            " | Speed = " +
            shootSpeed
        );


        StartCoroutine(
            RestoreCollision(
                firedArrow
            )
        );
        Destroy(firedArrow, 10f);

        currentArrow = null;

        currentArrowRb = null;

        currentArrowNockPoint = null;
    }


    // =========================================================
    // CANCEL ARROW
    // =========================================================

    private void CancelArrow()
    {
        isPulling = false;

        hasRightHandAnchor = false;


        if (currentArrow != null)
        {
            Destroy(currentArrow);
        }


        currentArrow = null;

        currentArrowRb = null;

        currentArrowNockPoint = null;


        currentPullDistance = 0f;


        ResetBowString();
    }


    // =========================================================
    // RESET
    // =========================================================

    private void ResetBowString()
    {
        if (pullPoint != null)
        {
            pullPoint.localPosition =
                pullPointRestLocalPosition;
        }


        // =====================================================
        // QUAN TRỌNG:
        //
        // Bản trước thiếu phần này.
        // Sau vài phát GrabHandle bị nằm ở vị trí kéo cũ.
        // =====================================================

        if (grabHandle != null)
        {
            grabHandle.localPosition =
                grabHandleRestLocalPosition;


            grabHandle.localRotation =
                grabHandleRestLocalRotation;
        }
    }


    // =========================================================
    // COLLISION
    // =========================================================

    private void SetArrowCollisionIgnored(
        GameObject arrow,
        bool ignore
    )
    {
        if (arrow == null)
            return;


        Collider[] arrowColliders =
            arrow.GetComponentsInChildren<Collider>(
                true
            );


        if (bowColliders != null)
        {
            foreach (Collider arrowCol in arrowColliders)
            {
                if (arrowCol == null)
                    continue;


                foreach (Collider bowCol in bowColliders)
                {
                    if (bowCol == null)
                        continue;


                    if (arrowCol == bowCol)
                        continue;


                    Physics.IgnoreCollision(
                        arrowCol,
                        bowCol,
                        ignore
                    );
                }
            }
        }


        IgnoreHandCollision(
            arrowColliders,
            leftHand,
            ignore
        );


        IgnoreHandCollision(
            arrowColliders,
            rightHand,
            ignore
        );
    }


    // =========================================================
    // HAND COLLISION
    // =========================================================

    private void IgnoreHandCollision(
        Collider[] arrowColliders,
        Transform hand,
        bool ignore
    )
    {
        if (hand == null)
            return;


        Collider[] handColliders =
            hand.GetComponentsInChildren<Collider>(
                true
            );


        foreach (Collider arrowCol in arrowColliders)
        {
            if (arrowCol == null)
                continue;


            foreach (Collider handCol in handColliders)
            {
                if (handCol == null)
                    continue;


                Physics.IgnoreCollision(
                    arrowCol,
                    handCol,
                    ignore
                );
            }
        }
    }


    // =========================================================
    // RESTORE COLLISION
    // =========================================================

    private IEnumerator RestoreCollision(
        GameObject arrow
    )
    {
        yield return new WaitForSeconds(
            ignoreCollisionTime
        );


        if (arrow == null)
            yield break;


        SetArrowCollisionIgnored(
            arrow,
            false
        );
    }
}