using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace SandboxConduitTool
{
    public class SandboxConduitToolHoverTextCard : HoverTextConfiguration
    {
        protected override void OnSpawn()
        {
            base.OnSpawn();

            var template = DigTool.Instance?.gameObject?.GetComponent<HoverTextConfiguration>();
            if (template != null)
            {
                ToolTitleTextStyle = template.ToolTitleTextStyle;

                Styles_BodyText = template.Styles_BodyText;
                Styles_Instruction = template.Styles_Instruction;
                Styles_Title = template.Styles_Title;
                Styles_Values = template.Styles_Values;
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            //ToolName = SandboxConduitToolStrings.HOVER_TEXT_TITLE_INJECT;
            ActionName = UI.TOOLS.BUILD.TOOLACTION_DRAG;
        }

        public override void UpdateHoverElements(List<KSelectable> selected)
        {
            HoverTextScreen screen = HoverTextScreen.Instance;
            Sprite dash = screen.GetSprite("dash");

            HoverTextDrawer txt = screen.BeginDrawing();
            txt.BeginShadowBar();

            string title = Input.GetKey(KeyCode.LeftShift)
                ? SandboxConduitToolStrings.HOVER_TEXT_TITLE_CLEAR
                : SandboxConduitToolStrings.HOVER_TEXT_TITLE_INJECT;
            txt.DrawText(title.ToUpper(), ToolTitleTextStyle);

            DrawInstructions(screen, txt);

            txt.EndShadowBar();
            txt.EndDrawing();
        }
    }
}
