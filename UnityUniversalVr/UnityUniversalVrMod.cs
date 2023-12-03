﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;

namespace UnityUniversalVr;

[BepInPlugin("raicuparta.unityuniversalvr", "Unity Universal VR", "0.1.0")]
public class UnityUniversalVrMod : BaseUnityPlugin
{
    private readonly KeyboardKey _toggleVrKey = new (KeyboardKey.KeyCode.F3);
    private readonly KeyboardKey _reparentCameraKey = new (KeyboardKey.KeyCode.F4);

    private bool _vrEnabled;
    private bool _setUpDone;
    private Type _xrSettingsType;
    private PropertyInfo _loadedDeviceNameProperty;

    private void Awake()
    {
        _xrSettingsType =
            Type.GetType("UnityEngine.XR.XRSettings, UnityEngine.XRModule") ??
            Type.GetType("UnityEngine.XR.XRSettings, UnityEngine.VRModule");

        _loadedDeviceNameProperty = _xrSettingsType.GetProperty("loadedDeviceName");
    }

    private void Update()
    {
        if (_toggleVrKey.UpdateIsDown())
        {
            if (!_vrEnabled)
            {
                if (!_setUpDone)
                {
                    SetUpVr();
                }
                else
                {
                    SetVrEnabled(true);
                }
            }
            else
            {
                SetVrEnabled(false);
            }
        }

        if (_reparentCameraKey.UpdateIsDown())
        {
            ReparentCamera();
        }

        if (!_setUpDone && _loadedDeviceNameProperty.GetValue(null, null) != "")
        {
            FinishSetUp();
        }
    }
    
    private void SetUpVr()
    {
        Console.WriteLine("Toggling VR...");

        if (_xrSettingsType != null)
        {
            MethodInfo loadDeviceByNameMethod = _xrSettingsType.GetMethod("LoadDeviceByName", new[] { typeof(string) });
            if (loadDeviceByNameMethod != null)
            {
                object[] parameters = { "OpenVR" };
                loadDeviceByNameMethod.Invoke(null, parameters);
            }
            else
            {
                Console.WriteLine("Failed to get method LoadDeviceByName");
            }
        }
        else
        {
            Console.WriteLine("Failed to get type UnityEngine.XR.XRSettings");
        }
    }

    private void FinishSetUp()
    {
        SetVrEnabled(true);
        
        Type inputTrackingType = 
            Type.GetType("UnityEngine.XR.InputTracking, UnityEngine.XRModule") ??
            Type.GetType("UnityEngine.XR.InputTracking, UnityEngine.VRModule");

        if (inputTrackingType != null)
        {
            PropertyInfo disablePositionalTrackingProperty = inputTrackingType.GetProperty("disablePositionalTracking");
            if (disablePositionalTrackingProperty != null)
            {
                disablePositionalTrackingProperty.SetValue(null, true, null);
            }
            else
            {
                Console.WriteLine("Failed to get property disablePositionalTracking");
            }
        }
        else
        {
            Console.WriteLine("Failed to get type UnityEngine.XR.InputTracking");
        }

        _setUpDone = true;
    }

    private void SetVrEnabled(bool enabled)
    {
        Console.WriteLine($"Setting VR enable status to {enabled}");
        PropertyInfo enableVr = _xrSettingsType.GetProperty("enabled");
        if (enableVr != null)
        {
            enableVr.SetValue(null, enabled, null);
        }
        else
        {
            Console.WriteLine("Failed to get property enabled");
        }

        _vrEnabled = enabled;
    }
    
    private static void ReparentCamera() {

        Console.WriteLine("Reparenting Camera...");

        
        Console.WriteLine("Reparenting Camera 1");
        Type cameraType = Type.GetType("UnityEngine.Camera, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        object mainCamera = cameraType.GetProperty("main").GetValue(null, null);
        cameraType.GetProperty("enabled").SetValue(mainCamera, false, null);

        Console.WriteLine("Reparenting Camera 2");
        Type gameObjectType = Type.GetType("UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        object vrCameraObject = Activator.CreateInstance(gameObjectType);
        MethodInfo addComponentMethod = gameObjectType.GetMethod("AddComponent", new[] { typeof(Type) });
        object vrCamera = addComponentMethod.Invoke(vrCameraObject, new[] { cameraType });
        object mainCameraTransform = cameraType.GetProperty("transform").GetValue(mainCamera, null);
        object vrCameraTransform = cameraType.GetProperty("transform").GetValue(vrCamera, null);
        Type transformType = Type.GetType("UnityEngine.Transform, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        
        Console.WriteLine("Reparenting Camera 3");
        transformType.GetProperty("parent").SetValue(vrCameraTransform, mainCameraTransform, null);
        transformType.GetProperty("localPosition").SetValue(vrCameraTransform, null, null);
        
        Console.WriteLine("Reparenting Camera end");
    }
}