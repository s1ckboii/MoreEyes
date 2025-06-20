﻿using MoreEyes.Core;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

public class CustomPupilType
{
    internal string Name = string.Empty;
    internal string Path = string.Empty;
    internal GameObject Prefab = null!;
    internal LoadedAsset MyBundle = null!;
    internal Sides AllowedPos = Sides.Both;
    internal bool isVanilla = false;
    internal bool inUse = false;

    //internal static List<string> UsedPupilNames = [];

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    internal CustomPupilType(LoadedAsset bundle, string name)
    {
        PupilSetup(bundle, name);
    }

    internal CustomPupilType(string name)
    {
        Name = name;
    }
    internal void PupilSetup(LoadedAsset bundle, string name)
    {
        MyBundle = bundle;
        Path = name;

        Name = name[(name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

        if (Name.EndsWith("_right", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = Sides.Right;
        }
        else if (Name.EndsWith("_left", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = Sides.Left;
        }
        else
        {
            AllowedPos = Sides.Both;
        }

        MyBundle.LoadAssetGameObject(Path, out Prefab);
        if (Prefab == null)
            Plugin.logger.LogWarning($"PUPIL IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
        Plugin.Spam($"AllPupilTypes count - {AllPupilTypes.Count}");
    }

    internal void VanillaSetup(bool isLeft, GameObject original)
    {
        if(isLeft)
        {
            Name = "Standard";
            AllowedPos = Sides.Left;
        }
        else
        {
            Name = "Standard";
            AllowedPos = Sides.Right;
        }

        AddVanillaEye(original);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
    }

    internal void AddVanillaEye(GameObject eyeObject)
    {
        Prefab = UnityEngine.Object.Instantiate(eyeObject);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);
        Prefab.transform.SetParent(null);
        Prefab.SetActive(false);
        isVanilla = true;
    }

    public void MarkPupilUnused()
    {
        inUse = false;

        if (PupilsInUse.Contains(this))
        {
            PupilsInUse.Remove(this);
            Plugin.logger.LogInfo($"{Name} was marked as unused."); // We can get rid of this logger in the future
        }      
    }
}
