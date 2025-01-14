﻿using STRINGS;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace SuppressNotifications
{
    class CopyEntitySettingsTool : DragTool
    {
        public static CopyEntitySettingsTool instance;

        private List<int> cells;
        private GameObject sourceGameObject;

        public override void OnPrefabInit()
        {
            // Initialize
            base.OnPrefabInit();
            instance = this;

            // Set the cursor
            boxCursor = CopySettingsTool.Instance.boxCursor;

            // Set the area visualizer
            var avTemplate = CopySettingsTool.Instance.areaVisualizer;
            var areaVisualizer = Util.KInstantiate(avTemplate, gameObject,
                typeof(CopyEntitySettingsTool).Name + "AreaVisualizer");
            areaVisualizer.SetActive(false);
            areaVisualizerSpriteRenderer = areaVisualizer.GetComponent<SpriteRenderer>();

            this.areaVisualizer = areaVisualizer;
            areaVisualizerTextPrefab = CopySettingsTool.Instance.areaVisualizerTextPrefab;

            // Set the visualizer
            visualizer = CopySettingsTool.Instance.visualizer;

            // Set the hover card
            var hoverConfig = gameObject.AddComponent<HoverTextConfiguration>();
            var hoverTemplate = CopySettingsTool.Instance.gameObject.GetComponent<HoverTextConfiguration>();
            hoverConfig.ToolNameStringKey = hoverTemplate.ToolNameStringKey;
            hoverConfig.ActionStringKey = hoverTemplate.ActionStringKey;
            hoverConfig.ActionName = hoverTemplate.ActionName;
            hoverConfig.ToolName = hoverTemplate.ToolName;
        }

        public override void OnDragTool(int cell, int distFromOrigin)
        {
            if (sourceGameObject == null)
                return;

            if (Grid.IsValidCell(cell) && !cells.Contains(cell))
                cells.Add(cell);
        }

        public override void OnDragComplete(Vector3 cursorDown, Vector3 cursorUp)
        {
            if(sourceGameObject.GetComponent<CritterSuppressionButton>() != null)
                CopyCritterSettings();
            if (sourceGameObject.GetComponent<CropSuppressionButton>() != null)
                CopyCropSettings();
        }

        private void CopyCritterSettings()
        {
            var enumerator = Components.Brains.GetEnumerator();
            using (enumerator as IDisposable)
            {
                while (enumerator.MoveNext())
                {
                    var creatureBrain = enumerator.Current as CreatureBrain;

                    if (creatureBrain != null &&
                           creatureBrain.isSpawned &&
                           !creatureBrain.HasTag(GameTags.Dead) &&
                           cells.Contains(Grid.PosToCell(creatureBrain)))
                    {
                        creatureBrain.gameObject.Trigger((int)GameHashes.CopySettings, sourceGameObject);
                        PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, UI.COPIED_SETTINGS, creatureBrain.gameObject.transform, new Vector3(0f, 0.5f, 0f), 1.5f, false, false);
                    }
                }
            }
        }

        private void CopyCropSettings()
        {
            var enumerator = Components.Crops.GetEnumerator();
            using(enumerator as IDisposable)
            {
                while (enumerator.MoveNext())
                {
                    var crop = enumerator.Current as Crop;

                    if(crop != null && cells.Contains(Grid.PosToCell(crop)))
                    {
                        crop.gameObject.Trigger((int)GameHashes.CopySettings, sourceGameObject);
                        PopFXManager.Instance.SpawnFX(PopFXManager.Instance.sprite_Plus, UI.COPIED_SETTINGS, crop.gameObject.transform, new Vector3(0f, 0.5f, 0f), 1.5f, false, false);
                    }
                }
            }
        }

        public void SetSourceObject(GameObject go)
        {
            sourceGameObject = go;
        }

        public void Activate()
        {
            PlayerController.Instance.ActivateTool(this);
        }

        public override void OnActivateTool()
        {
            base.OnActivateTool();
            cells = new List<int>();
        }

        public override void OnDeactivateTool(InterfaceTool new_tool)
        {
            base.OnDeactivateTool(new_tool);
            sourceGameObject = null;
        }
    }
}
