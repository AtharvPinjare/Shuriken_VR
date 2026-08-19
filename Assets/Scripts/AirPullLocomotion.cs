using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class AirPullLocomotion : MonoBehaviour
{
    [Header("Hand Sources — drag the exact Hand/HandRef your gesture pipeline uses")]
    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object _leftHandSource;

    [SerializeField, Interface(typeof(IHand))]
    private UnityEngine.Object _rightHandSource;

    [Header("Tuning")]
    [SerializeField, Min(0f)] private float movementMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float handPinchThreshold = 0.7f;
    [SerializeField, Range(0f, 1f)] private float controllerGripThreshold = 0.7f;
    [SerializeField] private HandFinger gripFinger = HandFinger.Index;
    [SerializeField, Min(0f)] private float maxHandDeltaPerFrame = 0.25f;
    [SerializeField] private bool lockVerticalMovement = true;

    // -----------------------------------------
    // MOVEMENT TUTORIAL
    // -----------------------------------------

    [Header("Movement Tutorial")]
    [SerializeField] private float tutorialPullDistance = 0.15f;

    public UnityEvent OnTutorialPullCompleted;

    private float _tutorialPullDistance;
    private bool _tutorialPullCompleted;

    // -----------------------------------------

    private OVRCameraRig _cameraRig;
    private Transform _trackingSpace;
    private Transform _leftHandAnchor;
    private Transform _rightHandAnchor;
    private IHand _leftHand;
    private IHand _rightHand;
    private HandMotion _leftMotion;
    private HandMotion _rightMotion;

    private struct HandMotion
    {
        public bool HasPreviousPosition;
        public Vector3 PreviousTrackingPosition;
    }

    private void Awake()
    {
        _leftHand = _leftHandSource as IHand;
        _rightHand = _rightHandSource as IHand;

        if (_leftHand == null)
        {
            Debug.LogError($"{nameof(AirPullLocomotion)}: Left Hand Source is not assigned " +
                "or does not implement IHand. Hand-tracking pull will not work for the left hand.", this);
        }

        if (_rightHand == null)
        {
            Debug.LogError($"{nameof(AirPullLocomotion)}: Right Hand Source is not assigned " +
                "or does not implement IHand. Hand-tracking pull will not work for the right hand.", this);
        }
    }

    private void OnEnable()
    {
        ResetHandMotion();

        _tutorialPullDistance = 0f;
        _tutorialPullCompleted = false;
    }

    private void LateUpdate()
    {
        if (!EnsureRigReferences())
            return;

        Vector3 combinedDelta = Vector3.zero;
        int movingHandCount = 0;

        AccumulateHandDelta(
            _leftHand,
            _leftHandAnchor,
            OVRInput.Controller.LTouch,
            OVRInput.Hand.HandLeft,
            ref _leftMotion,
            ref combinedDelta,
            ref movingHandCount
        );

        AccumulateHandDelta(
            _rightHand,
            _rightHandAnchor,
            OVRInput.Controller.RTouch,
            OVRInput.Hand.HandRight,
            ref _rightMotion,
            ref combinedDelta,
            ref movingHandCount
        );

        if (movingHandCount == 0)
            return;

        // EXISTING MOVEMENT BEHAVIOUR
        transform.position -= combinedDelta * movementMultiplier;
    }

    private bool EnsureRigReferences()
    {
        if (_cameraRig == null)
            _cameraRig = GetComponent<OVRCameraRig>();

        if (_cameraRig == null)
        {
            enabled = false;
            return false;
        }

        _trackingSpace = _cameraRig.trackingSpace;
        _leftHandAnchor = _cameraRig.leftHandAnchor;
        _rightHandAnchor = _cameraRig.rightHandAnchor;

        return _trackingSpace != null &&
               _leftHandAnchor != null &&
               _rightHandAnchor != null;
    }

    private void AccumulateHandDelta(
        IHand hand,
        Transform handAnchor,
        OVRInput.Controller controller,
        OVRInput.Hand inputHand,
        ref HandMotion motion,
        ref Vector3 combinedDelta,
        ref int movingHandCount)
    {
        bool controllerIsInHand =
            OVRInput.GetControllerIsInHandState(inputHand)
            == OVRInput.ControllerInHandState.ControllerInHand;

        bool isTrackedHand =
            !controllerIsInHand &&
            hand != null &&
            hand.IsConnected &&
            hand.IsTrackedDataValid;

        bool isGripping =
            isTrackedHand
            ? hand.GetFingerPinchStrength(gripFinger) >= handPinchThreshold
            : OVRInput.Get(
                OVRInput.Axis1D.PrimaryHandTrigger,
                controller
            ) >= controllerGripThreshold;

        if (!isGripping ||
            !TryGetTrackingPosition(
                hand,
                handAnchor,
                isTrackedHand,
                out Vector3 currentPosition))
        {
            motion.HasPreviousPosition = false;

            // Reset tutorial accumulation when grip is released.
            _tutorialPullDistance = 0f;

            return;
        }

        if (motion.HasPreviousPosition)
        {
            Vector3 trackingDelta =
                currentPosition - motion.PreviousTrackingPosition;

            // -----------------------------------------
            // EXISTING MOVEMENT CODE
            // -----------------------------------------

            Vector3 worldDelta =
                _trackingSpace.TransformVector(trackingDelta);

            if (lockVerticalMovement)
                worldDelta.y = 0f;

            if (worldDelta.sqrMagnitude >
                maxHandDeltaPerFrame * maxHandDeltaPerFrame)
            {
                worldDelta =
                    worldDelta.normalized * maxHandDeltaPerFrame;
            }

            combinedDelta += worldDelta;
            movingHandCount++;

            // -----------------------------------------
            // MOVEMENT TUTORIAL DETECTION
            // -----------------------------------------

            if (!_tutorialPullCompleted)
            {
                _tutorialPullDistance += trackingDelta.magnitude;

                if (_tutorialPullDistance >= tutorialPullDistance)
                {
                    _tutorialPullCompleted = true;

                    Debug.Log(
                        "[AirPullLocomotion] Movement tutorial gesture completed."
                    );

                    OnTutorialPullCompleted?.Invoke();
                }
            }
        }

        motion.PreviousTrackingPosition = currentPosition;
        motion.HasPreviousPosition = true;
    }

    private bool TryGetTrackingPosition(
        IHand hand,
        Transform handAnchor,
        bool isTrackedHand,
        out Vector3 trackingPosition)
    {
        if (isTrackedHand &&
            hand.GetRootPose(out Pose handPose))
        {
            trackingPosition =
                _trackingSpace.InverseTransformPoint(handPose.position);

            return true;
        }

        if (handAnchor != null)
        {
            trackingPosition =
                _trackingSpace.InverseTransformPoint(
                    handAnchor.position
                );

            return true;
        }

        trackingPosition = default;
        return false;
    }

    private void ResetHandMotion()
    {
        _leftMotion = default;
        _rightMotion = default;
    }
}