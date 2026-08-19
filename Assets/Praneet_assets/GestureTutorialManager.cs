using UnityEngine;

public class GestureTutorialManager : MonoBehaviour
{
    [Header("Steps in order")]
    public GestureTutorialStep[] steps;

    private int currentStepIndex = -1;

    private void Start()
    {
        foreach (var step in steps)
            if (step != null) step.gameObject.SetActive(false);

        BeginNextStep();
    }

    private void BeginNextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Length)
        {
            Debug.Log("[GestureTutorialManager] Tutorial complete.");
            return;
        }

        var step = steps[currentStepIndex];
        if (step == null) { BeginNextStep(); return; }

        step.OnGestureCompleted += HandleStepCompleted;
        step.BeginStep();
    }

    private void HandleStepCompleted()
    {
        steps[currentStepIndex].OnGestureCompleted -= HandleStepCompleted;
        BeginNextStep();
    }
}