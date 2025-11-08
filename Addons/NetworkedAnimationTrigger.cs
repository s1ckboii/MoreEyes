/*
using MoreEyes.Components;
using Photon.Pun;
using UnityEngine;
using static MoreEyes.Utility.Enums;

namespace MoreEyes.Addons;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class NetworkedAnimationTrigger : MonoBehaviour
{
    [Header("Key Binding")]
    public NumpadKey triggerKey = NumpadKey.Keypad5;

    [Header("Animator Parameter")]
    public string paramName = "MyBool";

    public enum ParamType { Bool, Trigger, Float, Int }
    public ParamType paramType = ParamType.Bool;

    public float floatValue = 0f;
    public int intValue = 0;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        PatchedEyes localEyes = PatchedEyes.Local?.CurrentSelections?.patchedEyes;
        if (localEyes == null || localEyes.gameObject != GetComponentInParent<PatchedEyes>()?.gameObject)
            return;

        if (Input.GetKeyDown((KeyCode)triggerKey))
        {
            TriggerLocal();

            if (PhotonNetwork.IsConnected && MoreEyesNetwork.instance != null)
                TriggerAndSync();
        }
    }

    private void TriggerLocal()
    {
        ApplyToAnimator(paramType, paramName, floatValue, intValue, null);
    }

    private void TriggerAndSync()
    {
        string playerID = PatchedEyes.Local.CurrentSelections.playerID;

        switch (paramType)
        {
            case ParamType.Bool:
                bool newBool = animator.GetBool(paramName);
                MoreEyesNetwork.instance.photonView.RPC(nameof(MoreEyesNetwork.RPC_SetEyeAnimBool), RpcTarget.Others, playerID, paramName, newBool);
                break;

            case ParamType.Trigger:
                MoreEyesNetwork.instance.photonView.RPC(nameof(MoreEyesNetwork.RPC_SetEyeAnimTrigger), RpcTarget.Others, playerID, paramName);
                break;

            case ParamType.Float:
                MoreEyesNetwork.instance.photonView.RPC(
                    nameof(MoreEyesNetwork.RPC_SetEyeAnimFloat), RpcTarget.Others, playerID, paramName, floatValue);
                break;

            case ParamType.Int:
                MoreEyesNetwork.instance.photonView.RPC(nameof(MoreEyesNetwork.RPC_SetEyeAnimInt), RpcTarget.Others, playerID, paramName, intValue);
                break;
        }
    }

    private void ApplyToAnimator(ParamType type, string name, float f, int i, bool? b)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return;

        switch (type)
        {
            case ParamType.Bool:
                bool newBool = b ?? !animator.GetBool(name);
                animator.SetBool(name, newBool);
                break;

            case ParamType.Trigger:
                animator.SetTrigger(name);
                break;

            case ParamType.Float:
                animator.SetFloat(name, f);
                break;

            case ParamType.Int:
                animator.SetInteger(name, i);
                break;
        }
    }
}
*/