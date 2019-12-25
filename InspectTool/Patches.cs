using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using UnityEngine;
using EUtil;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using PeterHan.PLib.Datafiles;

/* TODO:
 * - element filter sidescreen
 * - liquid pools pressure?
 * - include buildings and other objects?
*/

namespace InspectTool
{
    internal class Patches
    {
        private static PAction PAction;

        public static void OnLoad(string path)
        {
            PUtil.InitLibrary(true);
            PLocalization.Register();
            POptions.RegisterOptions(typeof(InspectToolSettings));

            ReadOptions();

            PKeyBinding pKeyBinding = null;
            if (KKeyCodeUtil.TryParse(InspectToolSettings.Instance.Hotkey, out KKeyCode keyCode, out Modifier modifier))
            {
                pKeyBinding = new PKeyBinding(keyCode, modifier);
            }

            PAction = PAction.Register(InspectToolStrings.ACTION_ID, InspectToolStrings.ACTION_TITLE, pKeyBinding);

            try
            {
                var inspectToolIconSprite = Assembly.GetExecutingAssembly().GetManifestResourceStream("InspectTool.img.inspectToolIcon.dds");
                var inspectCursorSprite = Assembly.GetExecutingAssembly().GetManifestResourceStream("InspectTool.img.inspectCursor.dds");
                InspectToolAssets.InspectToolIcon = SpriteUtil.CreateSpriteDXT5(inspectToolIconSprite, 32, 32);
                InspectToolAssets.InspectToolCursor = SpriteUtil.CreateSpriteDXT5(inspectCursorSprite, 256, 256);
                InspectToolAssets.InspectToolIcon.name = InspectToolStrings.TOOL_ICON_SPRITE_NAME;
                InspectToolAssets.InspectToolCursor.name = InspectToolStrings.CURSOR_SPRITE_NAME;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void ReadOptions()
        {
            InspectToolSettings.Instance = POptions.ReadSettings<InspectToolSettings>();
        }

        [HarmonyPatch(typeof(ToolMenu), "OnPrefabInit")]
        public static class ToolMenu_OnPrefabInit
        {
            public static void Postfix(ToolMenu __instance, List<Sprite> ___icons)
            {
                ___icons.Add(InspectToolAssets.InspectToolIcon);
            }
        }

        [HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
        internal class ToolMenu_CreateBasicTools_Patch
        {
            internal static void Postfix(ToolMenu __instance)
            {
                ReadOptions();

                var position = Mathf.Clamp(InspectToolSettings.Instance.ToolPosition, 0, __instance.basicTools.Count);
                var name = InspectToolStrings.TOOL_NAME;
                var tooltip = InspectToolStrings.TOOL_TOOLTIP;
                var largeIcon = InspectToolSettings.Instance.LargeIcon;
                var tc = ToolMenu.CreateToolCollection(name, InspectToolStrings.TOOL_ICON_SPRITE_NAME, PAction.GetKAction(), nameof(InspectTool), tooltip, largeIcon);
                __instance.basicTools.Insert(position, tc);
            }
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        internal class PlayerController_OnPrefabInit_Patch
        {
            internal static void Postfix(PlayerController __instance)
            {
                var t = new GameObject(nameof(InspectTool));
                t.AddComponent<InspectTool>();
                t.transform.SetParent(__instance.gameObject.transform);
                t.gameObject.SetActive(true);
                t.gameObject.SetActive(false);

                __instance.tools = __instance.tools.AddToArray(t.GetComponent<InterfaceTool>());
            }
        }

        [HarmonyPatch(typeof(Game), "DestroyInstances")]
        internal class Game_DestroyInstances_Patch
        {
            internal static void Prefix()
            {
                InspectTool.DestroyInstance();
            }
        }
    }
}
