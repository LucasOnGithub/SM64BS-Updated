﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using HarmonyLib;
using LibSM64;
using SM64BS.Attributes;
using SM64BS.Behaviours;
using SM64BS.Managers;
using SM64BS.Plugins;
using SM64BS.Plugins.BuiltIn;
using SM64BS.Plugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SM64BS.UI
{
    [HotReload(RelativePathToLayout = @"Views\settingsView.bsml")]
    [ViewDefinition("SM64BS.UI.Views.settingsView.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private MenuMarioManager _marioManager;
        private BasicUIAudioManager _basicUIAudioManager;

        [UIComponent("close-button")]
        private Transform _closeButtonTransform = null;

        [UIComponent("header-bar")]
        private ImageView _headerBarImageView = null;
        [UIComponent("main-modal")]
        private ModalView _modal = null;
        [UIComponent("plugins-list")]
        private CustomListTableData _pluginsListData = null;
        [UIComponent("color-blue")]
        private ColorSetting _colorSettingBlue = null;
        [UIComponent("color-red")]
        private ColorSetting _colorSettingRed = null;
        [UIComponent("color-white")]
        private ColorSetting _colorSettingWhite = null;
        [UIComponent("color-brown1")]
        private ColorSetting _colorSettingBrown1 = null;
        [UIComponent("color-beige")]
        private ColorSetting _colorSettingBeige = null;
        [UIComponent("color-brown2")]
        private ColorSetting _colorSettingBrown2 = null;

        [UIObject("settings-tab-selector")]
        private GameObject _settingsTabSelector = null;

        public Action modalHidden;

        #region UIValues

        [UIValue("name")]
        public string Name
        {
            get { return Plugin.Settings.MarioName; }
            set
            {
                Plugin.Settings.MarioName = value;
                _marioManager.namePlate.SetNamePlateText(value);
            }
        }
        [UIValue("show-nameplate")]
        public bool ShowNamePlate
        {
            get { return Plugin.Settings.ShowNamePlate; }
            set
            {
                Plugin.Settings.ShowNamePlate = value;
                _marioManager.namePlate.gameObject.SetActive(value);
            }
        }
        [UIValue("max-marios")]
        public int MaxMarios
        {
            get { return Plugin.Settings.MaxMarios; }
            set { Plugin.Settings.MaxMarios = value; }
        }
        [UIValue("shade-blue")]
        public Color ShadeBlue
        {
            get { return Plugin.Settings.MarioColors[0]; }
            set { ApplyColor(0, value); }
        }
        [UIValue("blue")]
        public Color Blue
        {
            get { return Plugin.Settings.MarioColors[1]; }
            set { ApplyColor(1, value); }
        }
        [UIValue("shade-red")]
        public Color ShadeRed
        {
            get { return Plugin.Settings.MarioColors[2]; }
            set { ApplyColor(2, value); }
        }
        [UIValue("red")]
        public Color Red
        {
            get { return Plugin.Settings.MarioColors[3]; }
            set { ApplyColor(3, value); }
        }
        [UIValue("shade-white")]
        public Color ShadeWhite
        {
            get { return Plugin.Settings.MarioColors[4]; }
            set { ApplyColor(4, value); }
        }
        [UIValue("white")]
        public Color White
        {
            get { return Plugin.Settings.MarioColors[5]; }
            set { ApplyColor(5, value); }
        }
        [UIValue("shade-brown1")]
        public Color ShadeBrown1
        {
            get { return Plugin.Settings.MarioColors[6]; }
            set { ApplyColor(6, value); }
        }
        [UIValue("brown1")]
        public Color Brown1
        {
            get { return Plugin.Settings.MarioColors[7]; }
            set { ApplyColor(7, value); }
        }
        [UIValue("shade-beige")]
        public Color ShadeBeige
        {
            get { return Plugin.Settings.MarioColors[8]; }
            set { ApplyColor(8, value); }
        }
        [UIValue("beige")]
        public Color Beige
        {
            get { return Plugin.Settings.MarioColors[9]; }
            set { ApplyColor(9, value); }
        }
        [UIValue("shade-brown2")]
        public Color ShadeBrown2
        {
            get { return Plugin.Settings.MarioColors[10]; }
            set { ApplyColor(10, value); }
        }
        [UIValue("brown2")]
        public Color Brown2
        {
            get { return Plugin.Settings.MarioColors[11]; }
            set { ApplyColor(11, value); }
        }

        #endregion

        public void Initialize(MenuMarioManager marioManager)
        {
            _marioManager = marioManager;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            _headerBarImageView.SetField("_skew", 0.0f);
            _closeButtonTransform.Find("BG").GetComponent<ImageView>().SetField("_skew", 0.0f);
            _closeButtonTransform.Find("Underline").GetComponent<ImageView>().SetField("_skew", 0.0f);
            _modal.transform.localPosition = Vector3.up * 30f;
            _modal.blockerClickedEvent += BlockerClickedEventHandler;
            _basicUIAudioManager = Resources.FindObjectsOfTypeAll<BasicUIAudioManager>().First(x => x.GetComponent<AudioSource>().enabled && x.isActiveAndEnabled);

            SetupPluginsList();
        }

        private void SetupPluginsList()
        {
            // Make sure tableView is initialized
            var tableView = (TableView)AccessTools.Field(typeof(CustomListTableData), "tableView").GetValue(_pluginsListData);
            if (tableView == null)
            {
                Plugin.Log.Warn("SetupPluginsList: tableView is null");
                return;
            }

            // Create a fresh list of cells
            List<CustomListTableData.CustomCellInfo> pluginCells = new List<CustomListTableData.CustomCellInfo>
    {
        new CustomListTableData.CustomCellInfo("Nothing", "Disable plugins", null)
    };

            foreach (CustomPlugin plugin in Plugin.LoadedCustomPlugins.Values)
            {
                pluginCells.Add(new CustomListTableData.CustomCellInfo(plugin.Name, plugin.Author, null));
            }

            // Set the new data
            tableView.ReloadData();

            int selectedPluginIndex = Plugin.Settings.SelectedPluginIndex;

            foreach (LevelListTableCell cell in tableView.visibleCells)
            {
                Destroy(cell.GetField<Image, LevelListTableCell>("_coverImage").gameObject);
                cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText").transform.localPosition = new Vector3(-28.5f, -3.05f);
                cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songAuthorText").transform.localPosition = new Vector3(-28.5f, -5.85f);
            }

            // Remove italics/skew
            foreach (ImageView iv in tableView.GetComponentsInChildren<ImageView>(true))
            {
                iv.SetField("_skew", 0.0f);
            }
            foreach (CurvedTextMeshPro tm in tableView.GetComponentsInChildren<CurvedTextMeshPro>(true))
            {
                tm.fontStyle = TMPro.FontStyles.Normal;
            }

            tableView.ScrollToCellWithIdx(selectedPluginIndex, TableView.ScrollPositionType.Beginning, false);
            tableView.SelectCellWithIdx(selectedPluginIndex);
        }


        [UIAction("close-modal")]
        private void HideModalActionHandler()
        {
            HideModal(true);
        }

        [UIAction("reset-colors")]
        private void ResetColors()
        {
            Plugin.Settings.MarioColors = SM64Types.defaultMarioColors.ToList();
            _marioManager.marioColorManager.SetMarioColors(SM64Types.defaultMarioColors);
            _marioManager.marioSpecialEffects.SpawnPopParticles();
            _colorSettingBlue.CurrentColor = SM64Types.defaultMarioColors[1];
            _colorSettingRed.CurrentColor = SM64Types.defaultMarioColors[3];
            _colorSettingWhite.CurrentColor = SM64Types.defaultMarioColors[5];
            _colorSettingBrown1.CurrentColor = SM64Types.defaultMarioColors[7];
            _colorSettingBeige.CurrentColor = SM64Types.defaultMarioColors[9];
            _colorSettingBrown2.CurrentColor = SM64Types.defaultMarioColors[11];
        }

        [UIAction("reset-position")]
        private void ResetPosition()
        {
            HideModal(true);
            _marioManager.namePlate.gameObject.SetActive(false);
            _marioManager.marioSpecialEffects.TeleportOut(() =>
            {
                Plugin.Settings.MarioPosition = new Vector3(2f, 0f, 3f);
                _marioManager.marioGO.GetComponent<SM64Mario>().SetPosition(Plugin.Settings.MarioPosition);
                _marioManager.marioSpecialEffects.TeleportIn(() =>
                {
                    _marioManager.namePlate.gameObject.SetActive(true);
                }, 0f);
            }, 0.5f);
        }

        [UIAction("plugin-select")]
        private void Select(TableView view, int row)
        {
            Plugin.Settings.SelectedPluginIndex = row;
            if (row - 1 < 0)
            {
                Plugin.Settings.SelectedPlugin = "";
                return;
            }
            Plugin.Settings.SelectedPlugin = Plugin.LoadedCustomPlugins.ElementAt(row - 1).Value.PluginId;
        }

        private void ApplyColor(int index, Color32 value)
        {
            Plugin.Settings.MarioColors[index] = value;
            _marioManager.marioColorManager.SetMarioColors(Plugin.Settings.MarioColors.ToArray());
            _marioManager.marioSpecialEffects.SpawnPopParticles();
        }

        private void BlockerClickedEventHandler()
        {
            modalHidden.Invoke();
        }

        public void ShowModal(bool animated)
        {
            _modal.Show(true);
            transform.Find("Blocker").localScale = new Vector3(1.5f, 2.0f, 1.0f);

            var clickSound = (AudioClip)AccessTools.Field(typeof(BasicUIAudioManager), "_buttonClickSound").GetValue(_basicUIAudioManager);
            var source = _basicUIAudioManager.GetComponent<AudioSource>();
            source.PlayOneShot(clickSound);

            foreach (ImageView iv in gameObject.GetComponentsInChildren<ImageView>(true))
            {
                iv.SetField("_skew", 0.0f);
            }
            foreach (CurvedTextMeshPro tm in gameObject.GetComponentsInChildren<CurvedTextMeshPro>(true))
            {
                tm.fontStyle = TMPro.FontStyles.Normal;
            }
            foreach (CurvedTextMeshPro tm in _settingsTabSelector.GetComponentsInChildren<CurvedTextMeshPro>(true))
            {
                tm.transform.localPosition = Vector3.zero;
            }
            if (_pluginsListData == null)
            {
                Plugin.Log?.Error("SetupPluginsList: _pluginsListData is null!");
                return;
            }
            var tableView = (TableView)AccessTools.Field(typeof(CustomListTableData), "tableView").GetValue(_pluginsListData);
            int selectedPluginIndex = Plugin.Settings.SelectedPluginIndex;
            tableView.SelectCellWithIdx(selectedPluginIndex);
            Select(tableView, selectedPluginIndex);
        }

        public void HideModal(bool animated)
        {
            _modal.Hide(true);
            modalHidden.Invoke();
        }
    }
}
