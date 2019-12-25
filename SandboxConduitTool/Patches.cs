using Harmony;
using UnityEngine;
using EUtil;
using PeterHan.PLib;
using PeterHan.PLib.Datafiles;
using PeterHan.PLib.Options;

namespace SandboxConduitTool
{
    public class Patches
    {
        static PAction PAction;

        public static void OnLoad(string path)
        {
            PUtil.InitLibrary(true);
            PLocalization.Register();
            POptions.RegisterOptions(typeof(SandboxConduitToolSettings));

            ReadOptions();

            PKeyBinding pKeyBinding = null;
            if (KKeyCodeUtil.TryParse(SandboxConduitToolSettings.Instance.Hotkey, out KKeyCode keyCode, out Modifier modifier))
            {
                pKeyBinding = new PKeyBinding(keyCode, modifier);
            }
            PAction = PAction.Register("SandboxConduitToolAction", "Sandbox Conduit Tool", pKeyBinding);
        }

        private static void ReadOptions()
        {
            SandboxConduitToolSettings.Instance = POptions.ReadSettings<SandboxConduitToolSettings>();
        }

        [HarmonyPatch(typeof(ToolMenu), "CreateSandBoxTools")]
        internal class ToolMenu_CreateSandBoxTools_Patch
        {
            internal static void Postfix()
            {
                ReadOptions();

                var position = Mathf.Clamp(SandboxConduitToolSettings.Instance.ToolPosition, 0, ToolMenu.Instance.sandboxTools.Count);
                var name = SandboxConduitToolStrings.TOOL_NAME;
                var tooltip = SandboxConduitToolStrings.TOOL_TOOLTIP;
                var tc = ToolMenu.CreateToolCollection(name, "icon_action_empty_pipes", PAction.GetKAction(), nameof(SandboxConduitTool), tooltip, largeIcon: false);
                ToolMenu.Instance.sandboxTools.Insert(position, tc);
            }
        }

        [HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
        internal class PlayerController_OnPrefabInit_Patch
        {
            internal static void Postfix(PlayerController __instance)
            {
                var t = new GameObject(nameof(SandboxConduitTool));
                t.AddComponent<SandboxConduitTool>();
                t.transform.SetParent(__instance.gameObject.transform);
                t.gameObject.SetActive(true);
                t.gameObject.SetActive(false);

                __instance.tools = __instance.tools.AddToArray(t.GetComponent<InterfaceTool>());
            }
        }
    }
}
