using Oculus.Interaction.Input;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class AirPullLocomotion : MonoBehaviour
{
    [SerializeField, Min(0f)] private float movementMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float handPinchThreshold = 0.7f;
    [SerializeField, Range(0f, 1f)] private float controllerGripThreshold = 0.7f;
    [SerializeField] private HandFinger gripFinger = HandFinger.Index;
    [SerializeField, Min(0f)] private float maxHandDeltaPerFrame = 0.25f;
    [SerializeField, Min(0.1f)] private float handResolveInterval = 2f;

    private OVRCameraRig _cameraRig;
    private Transform _trackingSpace;
    private Transform _leftHandAnchor;
    private Transform _rightHandAnchor;
    private IHand _leftHand;
    private IHand _rightHand;
    private HandMotion _leftMotion;
    private HandMotion _rightMotion;
    private float _nextHandResolveTime;

    private struct HandMotion
    {
        public bool HasPreviousPosition;
        public Vector3 PreviousTrackingPosition;
    }

    private void OnEnable()
    {
        ResetHandMotion();
    }

    private void LateUpdate()
    {
        if (!EnsureRigReferences())
            return;

        ResolveHandsIfNeeded();

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

        // Averaging the combined deltas preserves two-hand control without applying the same pull twice.
        transform.position -= (combinedDelta / movingHandCount) * movementMultiplier;
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

        return _trackingSpace != null && _leftHandAnchor != null && _rightHandAnchor != null;
    }

    private void ResolveHandsIfNeeded()
    {
        if ((_leftHand != null && _rightHand != null) || Time.unscaledTime < _nextHandResolveTime)
            return;

        _nextHandResolveTime = Time.unscaledTime + handResolveInterval;

        foreach (MonoBehaviour component in GetComponentsInChildren<MonoBehaviour>(true))
        {
            IHand hand = component as IHand;
            if (hand == null)
                continue;

            if (hand.Handedness == Handedness.Left && _leftHand == null)
                _leftHand = hand;
            else if (hand.Handedness == Handedness.Right && _rightHand == null)
                _rightHand = hand;
        }
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
        bool controllerIsInHand = OVRInput.GetControllerIsInHandState(inputHand)
            == OVRInput.ControllerInHandState.ControllerInHand;
        bool isTrackedHand = !controllerIsInHand
            && hand != null
            && hand.IsConnected
            && hand.IsTrackedDataValid;
        bool isGripping = isTrackedHand
            ? hand.GetFingerPinchStrength(gripFinger) >= handPinchThreshold
            : OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller) >= controllerGripThreshold;

        if (!isGripping || !TryGetTrackingPosition(hand, handAnchor, isTrackedHand, out Vector3 currentPosition))
        {
            motion.HasPreviousPosition = false;
            return;
        }

        if (motion.HasPreviousPosition)
        {
            Vector3 trackingDelta = currentPosition - motion.PreviousTrackingPosition;
            Vector3 worldDelta = _trackingSpace.TransformVector(trackingDelta);

            if (worldDelta.sqrMagnitude > maxHandDeltaPerFrame * maxHandDeltaPerFrame)
                worldDelta = worldDelta.normalized * maxHandDeltaPerFrame;

            combinedDelta += worldDelta;
            movingHandCount++;
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
        if (isTrackedHand && hand.GetRootPose(out Pose handPose))
        {
            trackingPosition = _trackingSpace.InverseTransformPoint(handPose.position);
            return true;
        }

        if (handAnchor != null)
        {
            trackingPosition = _trackingSpace.InverseTransformPoint(handAnchor.position);
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
