﻿using UnityEngine;
using MenuLib;
using MenuLib.MonoBehaviors;
using System.Collections.Generic;
using HarmonyLib;
using System.Linq;
using MoreEyes.Core;
using MoreEyes.EyeManagement;
using System.Collections;
using MoreEyes.Core.ModCompats;
using System;

namespace MoreEyes.Menus;

internal sealed class Menu
{
    internal static REPOButton clickedButton;
    internal static REPOPopupPage MoreEyesMenu = new();
    internal static REPOAvatarPreview AvatarPreview;
    internal static REPOLabel pupilLeft;
    internal static REPOLabel pupilRight;
    internal static REPOLabel irisLeft;
    internal static REPOLabel irisRight;
    internal static REPOLabel pupilLeftHeader;
    internal static REPOLabel pupilRightHeader;
    internal static REPOLabel irisLeftHeader;
    internal static REPOLabel irisRightHeader;


    internal static void Initialize()
    {
        Vector2 buttonSize = Spawnmanager.IsSpawnManagerPresent
            ? new Vector2(600f, 52f)
            : new Vector2(600f, 22f);

        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, buttonSize));
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
    }

    private static void RandomizeEyeSelection()
    {
        Plugin.Spam("Randomize Eye Selections!");
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        patchedEyes.RandomizeEyes(PlayerAvatar.instance);
        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void ResetEyeSelection()
    {
        Plugin.Spam("Reset Eye Selections!");
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        patchedEyes.ResetEyes(PlayerAvatar.instance);
        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void UpdateLabels()
    {
        pupilLeft.labelTMP.text = ApplyGradient(PlayerEyeSelection.localSelections.pupilLeft.Name, true);
        pupilRight.labelTMP.text = ApplyGradient(PlayerEyeSelection.localSelections.pupilRight.Name, true);
        irisLeft.labelTMP.text = ApplyGradient(PlayerEyeSelection.localSelections.irisLeft.Name, true);
        irisRight.labelTMP.text = ApplyGradient(PlayerEyeSelection.localSelections.irisRight.Name, true);
    }

    private static void CreatePopupMenu()
    {
        if (MoreEyesMenu.menuPage != null)
            return;

        PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        MoreEyesMenu = MenuAPI.CreateREPOPopupPage(ApplyGradient("More Eyes"), false, true, 0f, new Vector2(-150f, 0f));
        if (SemiFunc.MenuLevel())
        {
            AvatarPreview = MenuAPI.CreateREPOAvatarPreview(MoreEyesMenu.transform, new Vector2(500, 18), true, new Color(0f, 0f, 0f, 0.69f));
        }
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Back", () => MoreEyesMenu.ClosePage(true), MoreEyesMenu.transform, new Vector2(190, 20)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Randomize", RandomizeEyeSelection, MoreEyesMenu.transform, new Vector2(270, 20)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Reset", ResetEyeSelection, MoreEyesMenu.transform, new Vector2(400, 20)));
        CustomEyeManager.CheckForVanillaPupils();

        pupilLeft = MenuAPI.CreateREPOLabel(ApplyGradient(PlayerEyeSelection.localSelections.pupilLeft.Name, true), MoreEyesMenu.transform, new Vector2(160, 250));
        pupilRight = MenuAPI.CreateREPOLabel(ApplyGradient(PlayerEyeSelection.localSelections.pupilRight.Name, true), MoreEyesMenu.transform, new Vector2(310, 250));
        irisLeft = MenuAPI.CreateREPOLabel(ApplyGradient(PlayerEyeSelection.localSelections.irisLeft.Name, true), MoreEyesMenu.transform, new Vector2(160, 200));
        irisRight = MenuAPI.CreateREPOLabel(ApplyGradient(PlayerEyeSelection.localSelections.irisRight.Name, true), MoreEyesMenu.transform, new Vector2(310, 200));

        SetTextStyling([pupilLeft, pupilRight, irisLeft, irisRight]);

        pupilLeftHeader = MenuAPI.CreateREPOLabel("Pupil Left", MoreEyesMenu.transform, new Vector2(160, 270));
        pupilRightHeader = MenuAPI.CreateREPOLabel("Pupil Right", MoreEyesMenu.transform, new Vector2(310, 270));
        irisLeftHeader = MenuAPI.CreateREPOLabel("Iris Left", MoreEyesMenu.transform, new Vector2(160, 220));
        irisRightHeader = MenuAPI.CreateREPOLabel("Iris Right", MoreEyesMenu.transform, new Vector2(310, 220));

        SetHeaderTextStyling([pupilLeftHeader, pupilRightHeader, irisLeftHeader, irisRightHeader]);

        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftPupilPrev, pupilLeft.transform, new Vector2(45f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightPupilPrev, pupilRight.transform, new Vector2(45f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftIrisPrev, irisLeft.transform, new Vector2(45f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightIrisPrev, irisRight.transform, new Vector2(45f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftPupilNext, pupilLeft.transform, GetRightOfElement(pupilLeft.rectTransform) + new Vector2(-75f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightPupilNext, pupilRight.transform, GetRightOfElement(pupilRight.rectTransform) + new Vector2(-75f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftIrisNext, irisLeft.transform, GetRightOfElement(irisLeft.rectTransform) + new Vector2(-75f, -5f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightIrisNext, irisRight.transform, GetRightOfElement(irisRight.rectTransform) + new Vector2(-75f, -5f)));

        // Material's RGB sliders

        // They should only pop up when someone clicks on .... thinking
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOSlider("Red", ApplyGradient("Change red component"),new Action<float>(RedSlider), MoreEyesMenu.transform, new Vector2(205f, 180f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOSlider("Green", ApplyGradient("Change green component"),new Action<float>(GreenSlider), MoreEyesMenu.transform, new Vector2(205f, 135f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOSlider("Blue", ApplyGradient("Change blue component"), new Action<float>(BlueSlider), MoreEyesMenu.transform, new Vector2(205f, 90f)));

        MoreEyesMenu.StartCoroutine(WaitForPlayerMenu());
    }

    //this may not need to be a coroutine
    //originally made this an enum because I believed I needed to wait to update the visual
    private static IEnumerator WaitForPlayerMenu()
    {
        PatchedEyes local = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        local.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        yield return null;
        PatchedEyes.SetLocalEyes();
        MoreEyesMenu.OpenPage(false);
        Plugin.Spam("Replaced menu eyes!");
    }

    private static void SetTextStyling(List<REPOLabel> labels)
    {
        labels.Do(t =>
        {
            t.labelTMP.fontStyle = TMPro.FontStyles.SmallCaps;
            t.labelTMP.fontSize = 18f;
            t.labelTMP.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
        });
    }

    private static void SetHeaderTextStyling(List<REPOLabel> labels)
    {
        labels.Do(t =>
        {
            t.labelTMP.fontStyle = TMPro.FontStyles.Underline;
            t.labelTMP.fontSize = 18f;
            t.labelTMP.horizontalAlignment = TMPro.HorizontalAlignmentOptions.Center;
            t.labelTMP.color = Color.white;
        });
    }

    public static string ApplyGradient(string input, bool inverse = false, float minBrightness = 0.15f)
    {
        Material material = PlayerAvatar.instance.playerHealth.bodyMaterial;

        Color baseColor = material.GetColor(Shader.PropertyToID("_AlbedoColor"));
        Color startColor = baseColor;
        Color endColor;

        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;

        float adjustmentAmount = 0.6f;

        if (luminance < 0.5f)
        {
            endColor = Color.Lerp(baseColor, Color.white, adjustmentAmount);
        }
        else
        {
            endColor = Color.Lerp(baseColor, Color.black, adjustmentAmount);
        }

        if (inverse)
        {
            (endColor, startColor) = (startColor, endColor);
        }

        string result = "";
        int len = input.Length;

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / Mathf.Max(1, len - 1);
            Color lerped = Color.Lerp(startColor, endColor, t);

            float brightness = 0.299f * lerped.r + 0.587f * lerped.g + 0.114f * lerped.b;
            if (brightness < minBrightness)
            {
                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                lerped = Color.Lerp(lerped, Color.white, boost);
            }

            string hex = ColorUtility.ToHtmlStringRGB(lerped);
            result += $"<color=#{hex}>{input[i]}</color>";
        }

        return result;
    }


    private static Vector2 GetRightOfElement(RectTransform rect)
    {
        Vector3[] allCorners = new Vector3[4];
        rect.GetLocalCorners(allCorners);
        Plugin.Spam($"Count: {allCorners.Length}");
        return (Vector2)allCorners[3];
    }


    private enum EyePart { IrisLeft, IrisRight, PupilLeft, PupilRight };

    private static EyePart currentSelection;

    private static void ColorSlider(float value, int channelIndex)
    {
        // These might just be done in another class
        // CurrentPupil's material -> base color -> RGB
        // CurrentIris's material -> base color -> RGB

        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        // Apply all to playerAvatarVisuals

        // Select when clicked on .... thinking

        Material material = currentSelection switch
        {


            EyePart.IrisLeft => PlayerEyeSelection.localSelections.irisLeft.material,
            EyePart.IrisRight => PlayerEyeSelection.localSelections.irisRight.material,
            EyePart.PupilLeft => PlayerEyeSelection.localSelections.pupilLeft.material,
            EyePart.PupilRight => PlayerEyeSelection.localSelections.pupilRight.material,
            _ => null
        };

        if (material == null) return;

        Color color = material.color;
        color[channelIndex] = value;
        material.color = color;


        /*
            Color leftIrisColor = PlayerEyeSelection.localSelections.irisLeft.material.color;
        Color rightIrisColor = PlayerEyeSelection.localSelections.irisRight.material.color;
        Color leftPupilColor = PlayerEyeSelection.localSelections.pupilLeft.material.color;
        Color rightPupilColor = PlayerEyeSelection.localSelections.pupilRight.material.color;

        if ()

        leftIrisColor[channelIndex] = normalized;
        rightIrisColor[channelIndex] = normalized;
        leftPupilColor[channelIndex] = normalized;
        rightPupilColor[channelIndex] = normalized;

        // Only change one of these values base on the slider
        */
    }

    private static void RedSlider(float value) => ColorSlider(value, 0);
    private static void GreenSlider(float value) => ColorSlider(value, 1);
    private static void BlueSlider(float value) => ColorSlider(value, 2);

    private static void LeftIrisNext()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomIrisType> noRights = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.irisLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noRights.Count}");

        int selected = CycleIndex(currentIndex + 1, 0, noRights.Count - 1);

        CustomIrisType newSelection = noRights[selected];

        patchedEyes.SelectIris(newSelection, true);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightIrisNext()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomIrisType> noLefts = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.irisRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noLefts.Count}");

        int selected = CycleIndex(currentIndex + 1, 0, noLefts.Count - 1);

        CustomIrisType newSelection = noLefts[selected];

        patchedEyes.SelectIris(newSelection, false);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftIrisPrev()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomIrisType> noRights = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.irisLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noRights.Count}");

        int selected = CycleIndex(currentIndex - 1, 0, noRights.Count - 1);

        CustomIrisType newSelection = noRights[selected];

        patchedEyes.SelectIris(newSelection, true);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightIrisPrev()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomIrisType> noLefts = CustomEyeManager.AllIrisTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.irisRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllIrisTypes.Count}, Filtered: {noLefts.Count}");

        int selected = CycleIndex(currentIndex - 1, 0, noLefts.Count - 1);

        CustomIrisType newSelection = noLefts[selected];

        patchedEyes.SelectIris(newSelection, false);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftPupilNext()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomPupilType> noRights = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.pupilLeft);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noRights.Count}");

        int selected = CycleIndex(currentIndex + 1, 0, noRights.Count - 1);

        CustomPupilType newSelection = noRights[selected];

        patchedEyes.SelectPupil(newSelection, true);
        patchedEyes.SelectIris(PlayerEyeSelection.localSelections.irisLeft, true);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightPupilNext()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomPupilType> noLefts = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();
        noLefts.Do(p => Plugin.Spam(p.Name));

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.pupilRight);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noLefts.Count}");

        int selected = CycleIndex(currentIndex + 1, 0, noLefts.Count - 1);

        CustomPupilType newSelection = noLefts[selected];

        patchedEyes.SelectPupil(newSelection, false);
        patchedEyes.SelectIris(PlayerEyeSelection.localSelections.irisRight, false);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    private static void LeftPupilPrev()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomPupilType> noRights = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Right);

        noRights.Distinct();

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noRights.Count}");

        int currentIndex = noRights.IndexOf(PlayerEyeSelection.localSelections.pupilLeft);

        int selected = CycleIndex(currentIndex - 1, 0, noRights.Count - 1);

        Plugin.Spam($"currentIndex = {currentIndex}, selected = {selected}");

        CustomPupilType newSelection = noRights[selected];

        patchedEyes.SelectPupil(newSelection, true);
        patchedEyes.SelectIris(PlayerEyeSelection.localSelections.irisLeft, true);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }
    private static void RightPupilPrev()
    {
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);

        List<CustomPupilType> noLefts = CustomEyeManager.AllPupilTypes.FindAll(i => i.AllowedPos != CustomEyeManager.Sides.Left);

        noLefts.Distinct();

        int currentIndex = noLefts.IndexOf(PlayerEyeSelection.localSelections.pupilRight);

        int selected = CycleIndex(currentIndex - 1, 0, noLefts.Count - 1);

        Plugin.Spam($"CustomPupils Total: {CustomEyeManager.AllPupilTypes.Count}, Filtered: {noLefts.Count}");

        CustomPupilType newSelection = noLefts[selected];

        patchedEyes.SelectPupil(newSelection, false);
        patchedEyes.SelectIris(PlayerEyeSelection.localSelections.irisRight, false);

        UpdateLabels();

        CustomEyeManager.EmptyTrash();
    }

    public static int CycleIndex(int value, int min, int max)
    {
        if (value < min)
        {
            Plugin.Spam($"Returning max! {max}");
            return max; // added this because im lazy
        }

        if (value > max)
        {
            Plugin.Spam($"Returning min! {min}");
            return min;
        }

        Plugin.Spam($"Returning value! {value}");
        return value;
    }
}