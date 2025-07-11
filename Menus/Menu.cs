﻿using HarmonyLib;
using MenuLib;
using MenuLib.MonoBehaviors;
using MoreEyes.Core;
using MoreEyes.Core.ModCompats;
using MoreEyes.EyeManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MoreEyes.Menus;

internal sealed class Menu
{
    internal static REPOPopupPage MoreEyesMenu = new();
    internal static REPOAvatarPreview AvatarPreview;

    internal static REPOButton clickedButton;
    internal static REPOButton pupilLeft;
    internal static REPOButton pupilRight;
    internal static REPOButton irisLeft;
    internal static REPOButton irisRight;

    internal static REPOLabel pupilLeftHeader;
    internal static REPOLabel pupilRightHeader;
    internal static REPOLabel irisLeftHeader;
    internal static REPOLabel irisRightHeader;

    internal static REPOSlider redSlider;
    internal static REPOSlider greenSlider;
    internal static REPOSlider blueSlider;

    internal static Material currentMaterial;
    internal static MeshRenderer renderer;

    internal static EyePart currentEyePart;
    internal static EyeSide currentEyeSide;

    private static int currentRed;
    private static int currentGreen;
    private static int currentBlue;

    internal static string eyePart;
    internal static string eyeSide;
    internal static string eyeStyle;

    internal static bool slidersOn;

    internal enum EyePart
    {
        Pupil,
        Iris
    }
    internal enum EyeSide
    {
        Left,
        Right
    }


    private static string GetEyePartName(EyePart part)
    {
        return part switch
        {
            EyePart.Pupil => "Pupil",
            EyePart.Iris => "Iris",
            _ => ""
        };
    }

    private static string GetEyeSideName(EyeSide side)
    {
        return side switch
        {
            EyeSide.Left => "Left",
            EyeSide.Right => "Right",
            _ => ""
        };
    }

    private static string GetEyeStyle()
    {
        return (currentEyePart, currentEyeSide) switch
        {
            (EyePart.Pupil, EyeSide.Left) => pupilLeft.labelTMP.text,
            (EyePart.Pupil, EyeSide.Right) => pupilRight.labelTMP.text,
            (EyePart.Iris, EyeSide.Left) => irisLeft.labelTMP.text,
            (EyePart.Iris, EyeSide.Right) => irisRight.labelTMP.text,
            _ => ""
        };
    }


    internal static void Initialize()
    {
        int modCount = 0;
        if (ModCompats.IsSpawnManagerPresent) modCount++;
        if (ModCompats.IsTwitchChatAPIPresent) modCount++;
        if (ModCompats.IsTwitchTrollingPresent) modCount++;

        float yOffsetStart = 22f;
        float yOffsetPlus = 35f;

        Vector2 buttonPos = new(595f, yOffsetStart + (yOffsetPlus * modCount));

        MenuAPI.AddElementToMainMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, buttonPos));
        MenuAPI.AddElementToLobbyMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
        MenuAPI.AddElementToEscapeMenu(p => MenuAPI.CreateREPOButton("More Eyes", CreatePopupMenu, p, new Vector2(600f, 22f)));
    }

    private static void RandomizeEyeSelection()
    {
        Plugin.Spam("Randomize Eye Selections!");
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        patchedEyes.RandomizeEyes(PlayerAvatar.instance);
        UpdateButtons();

        CustomEyeManager.EmptyTrash();
    }

    private static void ResetEyeSelection()
    {
        Plugin.Spam("Reset Eye Selections!");
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        patchedEyes.GetPlayerMenuEyes(AvatarPreview.playerAvatarVisuals);
        patchedEyes.ResetEyes(PlayerAvatar.instance);
        UpdateButtons();

        CustomEyeManager.EmptyTrash();
    }

    private static void UpdateButtons()
    {
        pupilLeft.labelTMP.text = ApplyGradient(CleanName(PlayerEyeSelection.localSelections.pupilLeft.Name), true);
        pupilRight.labelTMP.text = ApplyGradient(CleanName(PlayerEyeSelection.localSelections.pupilRight.Name), true);
        irisLeft.labelTMP.text = ApplyGradient(CleanName(PlayerEyeSelection.localSelections.irisLeft.Name), true);
        irisRight.labelTMP.text = ApplyGradient(CleanName(PlayerEyeSelection.localSelections.irisRight.Name), true);

        UpdateHeaders();
    }

    private static void SetSliders(int red, int green, int blue)
    {
        redSlider.SetValue(red, false);
        greenSlider.SetValue(green, false);
        blueSlider.SetValue(blue, false);

        currentRed = red;
        currentGreen = green;
        currentBlue = blue;
    }

    private static void UpdateSliders(Color color)
    {
        if (currentMaterial == null) return;

        int red = Mathf.RoundToInt(color.r * 255f);
        int green = Mathf.RoundToInt(color.g * 255f);
        int blue = Mathf.RoundToInt(color.b * 255f);

        RedSlider(red);
        GreenSlider(green);
        BlueSlider(blue);

        SetSliders(currentRed, currentGreen, currentBlue);

    }
    private static void UpdateMaterialColor()
    {
        if (currentMaterial == null) return;

        Color newColor = new(currentRed / 255f, currentGreen / 255f, currentBlue / 255f);
        currentMaterial.SetColor("_EmissionColor", newColor);
    }

    private static void UpdateHeaders()
    {
        eyePart = GetEyePartName(currentEyePart);
        eyeSide = GetEyeSideName(currentEyeSide);
        eyeStyle = GetEyeStyle();
        redSlider.labelTMP.text = $"{eyeStyle}";
        greenSlider.labelTMP.text = ApplyGradient($"{eyePart}");
        blueSlider.labelTMP.text = ApplyGradient($"{eyeSide}");
    }

    private static void BackButton()
    {
        slidersOn = true;
        MoreEyesMenu.ClosePage(true);
    }

    private static void CreatePopupMenu()
    {
        if (MoreEyesMenu.menuPage != null)
        {
            return;
        }

        PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        MoreEyesMenu = MenuAPI.CreateREPOPopupPage(ApplyGradient("More Eyes"), false, true, 0f, new Vector2(-150f, 5f));
        
        AvatarPreview = MenuAPI.CreateREPOAvatarPreview(MoreEyesMenu.transform, new Vector2(471.25f, 156.5f), true, new Color(0f, 0f, 0f, 0.58f));

        AvatarPreview.previewSize = new Vector2(266.6667f, 500f); // original numbers (184, 345)
        AvatarPreview.rectTransform.sizeDelta = new Vector2(266.6667f, 210f); // original (184, 345) same way as previewSize
        AvatarPreview.rigTransform.parent.localScale = new Vector3(2f, 2f, 2f); // original (1, 1, 1)
        AvatarPreview.rigTransform.parent.localPosition = new Vector3(0f, -3.5f, 0f);

        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Back", BackButton, MoreEyesMenu.transform, new Vector2(190, 30)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Randomize", RandomizeEyeSelection, MoreEyesMenu.transform, new Vector2(270, 30)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("Reset", ResetEyeSelection, MoreEyesMenu.transform, new Vector2(400, 30)));
        CustomEyeManager.CheckForVanillaPupils();

        pupilLeft = MenuAPI.CreateREPOButton(ApplyGradient(CleanName(PlayerEyeSelection.localSelections.pupilLeft.Name), true), PupilLeftSliders, MoreEyesMenu.transform, new Vector2(215f, 265f));
        pupilRight = MenuAPI.CreateREPOButton(ApplyGradient(CleanName(PlayerEyeSelection.localSelections.pupilRight.Name), true), PupilRightSliders, MoreEyesMenu.transform, new Vector2(360f, 265f));
        irisLeft = MenuAPI.CreateREPOButton(ApplyGradient(CleanName(PlayerEyeSelection.localSelections.irisLeft.Name), true), IrisLeftSliders, MoreEyesMenu.transform, new Vector2(215, 215f));
        irisRight = MenuAPI.CreateREPOButton(ApplyGradient(CleanName(PlayerEyeSelection.localSelections.irisRight.Name), true), IrisRightSliders, MoreEyesMenu.transform, new Vector2(360, 215f));

        SetTextStyling([pupilLeft, pupilRight, irisLeft, irisRight]);

        pupilLeftHeader = MenuAPI.CreateREPOLabel("Pupil Left", MoreEyesMenu.transform, new Vector2(151.5f, 285f));
        pupilRightHeader = MenuAPI.CreateREPOLabel("Pupil Right", MoreEyesMenu.transform, new Vector2(297.5f, 285f));
        irisLeftHeader = MenuAPI.CreateREPOLabel("Iris Left", MoreEyesMenu.transform, new Vector2(151.5f, 235f));
        irisRightHeader = MenuAPI.CreateREPOLabel("Iris Right", MoreEyesMenu.transform, new Vector2(297.5f, 235f));

        SetHeaderTextStyling([pupilLeftHeader, pupilRightHeader, irisLeftHeader, irisRightHeader]);

        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftPupilPrev, pupilLeft.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightPupilPrev, pupilRight.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", LeftIrisPrev, irisLeft.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton("<", RightIrisPrev, irisRight.transform, new Vector2(-25f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftPupilNext, pupilLeft.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightPupilNext, pupilRight.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", LeftIrisNext, irisLeft.transform, new Vector2(70f, -10f)));
        MoreEyesMenu.AddElement(e => MenuAPI.CreateREPOButton(">", RightIrisNext, irisRight.transform, new Vector2(70f, -10f)));

        redSlider = MenuAPI.CreateREPOSlider(ApplyGradient($"{eyeStyle}"), "<color=#FF0000>Red</color>", RedSlider, MoreEyesMenu.transform, new Vector2(205f, 180f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);
        greenSlider = MenuAPI.CreateREPOSlider(ApplyGradient($"{eyePart}"), "<color=#00FF00>Green</color>", GreenSlider, MoreEyesMenu.transform, new Vector2(205f, 135f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);
        blueSlider = MenuAPI.CreateREPOSlider(ApplyGradient($"{eyeSide}"), "<color=#0000FF>Blue</color>", BlueSlider, MoreEyesMenu.transform, new Vector2(205f, 90f), min: 0, max: 255, barBehavior: REPOSlider.BarBehavior.UpdateWithValue);

        SliderSetups([redSlider, greenSlider, blueSlider]);

        redSlider.gameObject.SetActive(false);
        greenSlider.gameObject.SetActive(false);
        blueSlider.gameObject.SetActive(false);

        slidersOn = false;

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

    private static void SetTextStyling(List<REPOButton> buttons)
    {
        buttons.Do(t =>
        {
            t.overrideButtonSize = new Vector2(75f, 20f);
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

    private static void SliderSetups(List<REPOSlider> root)
    {
        if (root == null) return;

        Material material = PlayerAvatar.instance.playerHealth.bodyMaterial;
        Color baseColor = material.GetColor(Shader.PropertyToID("_AlbedoColor"));
        Color compColor = new(1f - baseColor.r, 1f - baseColor.g, 1f - baseColor.b, baseColor.a);

        foreach (REPOSlider slider in root)
        {
            foreach (Transform t in slider.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "SliderBG")
                {
                    var raws = t.GetComponentsInChildren<RawImage>(true);
                    foreach (var raw in raws)
                    {
                        if (raw != null)
                        {
                            Color zeroAlpha = raw.color;
                            zeroAlpha.a = 0f;
                            raw.color = zeroAlpha;
                        }
                    }
                }
                if (t.name == "Bar")
                {
                    var raws = t.GetComponentsInChildren<RawImage>(true);
                    foreach (var raw in raws)
                    {
                        if (raw != null)
                        {
                            float brightness = 0.299f * compColor.r + 0.587f * compColor.g + 0.114f * compColor.b;
                            float minBrightness = 0.5f;
                            if (brightness < minBrightness)
                            {
                                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                                compColor = Color.Lerp(compColor, Color.white, boost);
                            }
                            raw.color = compColor;
                        }
                    }
                }
                if (t.name == "Bar Text")
                {
                    var texts = t.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                    foreach (var text in texts)
                    {
                        if (text != null)
                        {
                            float brightness = 0.299f * compColor.r + 0.587f * compColor.g + 0.114f * compColor.b;
                            float minBrightness = 0.5f;
                            if (brightness < minBrightness)
                            {
                                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                                compColor = Color.Lerp(compColor, Color.white, boost);
                            }
                            text.color = compColor;
                        }
                    }
                }
                if (t.name == "Bar Text (1)")
                {
                    var raws = t.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                    foreach (var raw in raws)
                    {
                        if (raw != null)
                        {
                            raw.color = Color.black;
                        }
                    }
                }
            }
        }
    }
    public static string CleanName(string fileName)
    {
        string[] toRemove = ["pupil", "pupils", "iris", "irises", "left", "right"];

        string cleaned = fileName.Replace('_', ' ');

        foreach (var word in toRemove)
            cleaned = cleaned.Replace(word, "", StringComparison.OrdinalIgnoreCase);

        return string.Join(' ', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string ApplyGradient(string input, bool inverse = false, float minBrightness = 0.15f)
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
        // Using this one on pupil and iris names to break up using the same style too much
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
            // Needed a min brightness because of darker colors (like black if you have custom colors on)
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
    private static void PupilLeftSliders()
    {
        if (!slidersOn)
        {
            redSlider.gameObject.SetActive(true);
            greenSlider.gameObject.SetActive(true);
            blueSlider.gameObject.SetActive(true);
        }
        slidersOn = true;

        currentEyePart = EyePart.Pupil;
        currentEyeSide = EyeSide.Left;
        eyeStyle = pupilLeft.labelTMP.text;
        UpdateHeaders();

        if (PlayerEyeSelection.localSelections.pupilLeft.Prefab != null)
        {
            renderer = PlayerEyeSelection.localSelections.pupilLeft.Prefab.GetComponentInChildren<MeshRenderer>();
            currentMaterial = renderer.material;
            Color color = currentMaterial.GetColor("_EmissionColor");
            UpdateSliders(color);
        }
    }
    private static void PupilRightSliders()
    {
        if (!slidersOn)
        {
            redSlider.gameObject.SetActive(true);
            greenSlider.gameObject.SetActive(true);
            blueSlider.gameObject.SetActive(true);
        }
        slidersOn = true;

        currentEyePart = EyePart.Pupil;
        currentEyeSide = EyeSide.Right;
        eyeStyle = pupilRight.labelTMP.text;
        UpdateHeaders();

        if (PlayerEyeSelection.localSelections.pupilRight.Prefab != null)
        {
            renderer = PlayerEyeSelection.localSelections.pupilRight.Prefab.GetComponentInChildren<MeshRenderer>();
            currentMaterial = renderer.material;
            Color color = currentMaterial.GetColor("_EmissionColor");
            UpdateSliders(color);
        }
    }
    private static void IrisLeftSliders()
    {
        if (!slidersOn)
        {
            redSlider.gameObject.SetActive(true);
            greenSlider.gameObject.SetActive(true);
            blueSlider.gameObject.SetActive(true);
        }
        slidersOn = true;

        currentEyePart = EyePart.Iris;
        currentEyeSide = EyeSide.Left;
        eyeStyle = irisLeft.labelTMP.text;
        UpdateHeaders();

        if (PlayerEyeSelection.localSelections.irisLeft.Prefab != null)
        {
            renderer = PlayerEyeSelection.localSelections.irisLeft.Prefab.GetComponentInChildren<MeshRenderer>();
            currentMaterial = renderer.material;
            Color color = currentMaterial.GetColor("_EmissionColor");
            UpdateSliders(color);
        }
    }
    private static void IrisRightSliders()
    {
        if (!slidersOn)
        {
            redSlider.gameObject.SetActive(true);
            greenSlider.gameObject.SetActive(true);
            blueSlider.gameObject.SetActive(true);
        }
        slidersOn = true;

        currentEyePart = EyePart.Iris;
        currentEyeSide = EyeSide.Right;
        eyeStyle = irisRight.labelTMP.text;
        UpdateHeaders();

        if (PlayerEyeSelection.localSelections.irisRight.Prefab != null)
        {
            renderer = PlayerEyeSelection.localSelections.irisRight.Prefab.GetComponentInChildren<MeshRenderer>();
            currentMaterial = renderer.material;
            Color color = currentMaterial.GetColor("_EmissionColor");
            UpdateSliders(color);
        }
    }

    private static void RedSlider(int value)
    {
        currentRed = value;
        UpdateMaterialColor();

    }
    private static void GreenSlider(int value)
    {
        currentGreen = value;
        UpdateMaterialColor();

    }
    private static void BlueSlider(int value)
    {
        currentBlue = value;
        UpdateMaterialColor();

    }

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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

        UpdateButtons();

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