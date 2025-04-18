﻿using Zenject;
using UnityEngine;
using LibSM64;
using System;
using SM64BS.Behaviours;
using SM64BS.Utils;
using SM64BS.UI;
using HMUI;
using UnityEngine.UI;

namespace SM64BS.Managers
{
    internal class MenuMarioManager : IInitializable
    {
        private readonly AppMarioManager _appMarioManager;
        private readonly ResourceUtilities _utils;

        internal GameObject marioGO;
        internal NamePlate namePlate;
        internal VRPointerListener vrPointerListener;
        internal SettingsUIManager settingsUIManager;
        internal MarioColorManager marioColorManager;
        internal MarioSpecialEffects marioSpecialEffects;

        public MenuMarioManager(AppMarioManager appMarioManager, ResourceUtilities utils)
        {
            _appMarioManager = appMarioManager;
            _utils = utils;
        }

        public void Initialize()
        {
            float groundScale = 5f;

            GameObject groundGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _appMarioManager.AddMenuTerrain(groundGO.AddComponent<SM64StaticTerrain>());
            groundGO.GetComponent<MeshRenderer>().enabled = false;
            groundGO.transform.localScale = Vector3.one * groundScale;

            AddSM64Collision(GameObject.Find("MenuEnvironmentManager/DefaultMenuEnvironment/Notes"));
            AddSM64Collision(GameObject.Find("MenuEnvironmentManager/DefaultMenuEnvironment/PileOfNotes"));

            SM64Context.RefreshStaticTerrain();

            Vector3 spawnPos = Plugin.Settings.MarioPosition;
            if (Mathf.Abs(spawnPos.x) > groundScale*5f || Mathf.Abs(spawnPos.z) > groundScale*5f)
            {
                spawnPos = new Vector3(2f, 0f, 3f);
                Plugin.Settings.MarioPosition = spawnPos;
            }

            SM64Mario sm64Mario = _appMarioManager.SpawnMario(spawnPos, Quaternion.LookRotation(new Vector3(0, spawnPos.y, 0) - spawnPos));
            marioGO = sm64Mario.gameObject;
            marioGO.AddComponent<MarioBehaviour>();
            marioColorManager = marioGO.AddComponent<MarioColorManager>();
            settingsUIManager = marioGO.AddComponent<SettingsUIManager>();
            marioSpecialEffects = marioGO.AddComponent<MarioSpecialEffects>();
            VRInputProvider inputProvider = marioGO.AddComponent<VRInputProvider>();
            inputProvider.camera = Camera.main;
            sm64Mario.RefreshInputProvider();

            namePlate = new GameObject("NamePlate").AddComponent<NamePlate>();
            namePlate.transform.SetParent(marioGO.transform);
            namePlate.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            namePlate.Initialize(_utils);
            namePlate.gameObject.SetActive(Plugin.Settings.ShowNamePlate);

            vrPointerListener = new GameObject("VRPointerListener").AddComponent<VRPointerListener>();
            vrPointerListener.Initialize(namePlate, settingsUIManager);
            vrPointerListener.transform.SetParent(marioGO.transform);
            vrPointerListener.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            settingsUIManager.Initialize(this);
            settingsUIManager.CreateFloatingScreen();

            marioSpecialEffects.Initialize(_utils);

            RaycastShadow shadow = new GameObject("Shadow").AddComponent<RaycastShadow>();
            shadow.utils = _utils;
            shadow.transform.SetParent(marioGO.transform);
            shadow.transform.localPosition = Vector3.zero;
            shadow.transform.localRotation = Quaternion.identity;

            _appMarioManager.menuMarioGO = marioGO;
        }

        private void AddSM64Collision(GameObject root)
        {
            foreach (BoxCollider bc in root.GetComponentsInChildren<BoxCollider>())
            {
                if (bc.transform.parent.GetComponent<Animation>())
                {
                    _appMarioManager.AddMenuTerrain(bc.gameObject.AddComponent<SM64DynamicTerrain>());
                }
                else
                {
                    _appMarioManager.AddMenuTerrain(bc.gameObject.AddComponent<SM64StaticTerrain>());
                }
            }
        }
    }
}
