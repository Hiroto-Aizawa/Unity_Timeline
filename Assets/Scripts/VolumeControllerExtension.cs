using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Cinemachine;

[ExecuteAlways]
public class VolumeControllerExtension : CinemachineExtension
{
    [SerializeField] private Volume volume; // 仮想カメラごとに割り当てる
    [SerializeField] private Transform focusTarget; // 焦点対象
    [SerializeField] private float defaultFocusDistance = 10f;

    private float originalPriority;
    private DepthOfField dof;

    private bool isActive = false;

    protected override void Awake()
    {
        base.Awake();
        if (volume != null)
            originalPriority = volume.priority;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        TryGetDOF();
    }

    protected override void OnDisable()
    {
        ResetPriority();
    }

    private void TryGetDOF()
    {
        if (volume != null && volume.profile.TryGet(out dof) == false)
        {
            dof = null;
        }
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Aim)
            return;

        bool shouldBeActive = CinemachineCore.Instance.IsLive(vcam);

        if (shouldBeActive && !isActive)
        {
            ActivateVolume();
        }
        else if (!shouldBeActive && isActive)
        {
            ResetPriority();
        }

        if (isActive && dof != null && dof.focusDistance.overrideState)
        {
            float focusDistance = defaultFocusDistance;

            if (focusTarget != null)
            {
                Vector3 cameraPos = state.FinalPosition;
                focusDistance = Vector3.Distance(cameraPos, focusTarget.position);
            }

            dof.focusDistance.value = focusDistance;
        }
    }

    private void ActivateVolume()
    {
        if (volume != null)
        {
            originalPriority = volume.priority;
            volume.priority = 100f;
            isActive = true;
        }
    }

    private void ResetPriority()
    {
        if (volume != null)
        {
            volume.priority = originalPriority;
            isActive = false;
        }
    }
}
