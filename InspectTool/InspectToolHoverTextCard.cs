using System.Collections.Generic;
using STRINGS;
using UnityEngine;

namespace InspectTool
{
    public class InspectToolHoverTextCard : HoverTextConfiguration
    {
        private System.DateTime lastUpdated = System.DateTime.Now;

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

            ToolName = ((string)InspectToolStrings.TOOL_NAME).ToUpper();
            ActionName = UI.TOOLS.BUILD.TOOLACTION_DRAG;
        }

        public override void UpdateHoverElements(List<KSelectable> selected)
        {
            var now = System.DateTime.Now;
            if ((now - lastUpdated).TotalMilliseconds > 200) // recalculating everything every frame is unnecessary
            {
                ElementInspector.UpdateElementData();
                lastUpdated = now;
            }

            DrawTextCard();
        }

        private void DrawTextCard()
        {
            int cell = Grid.PosToCell(Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
            if (!Grid.IsValidCell(cell))
                return;

            HoverTextScreen screen = HoverTextScreen.Instance;
            Sprite dash = screen.GetSprite("dash");

            HoverTextDrawer txt = screen.BeginDrawing();
            txt.BeginShadowBar();

            if (!Grid.IsVisible(cell))
            {
                txt.DrawIcon(screen.GetSprite("iconWarning"));
                txt.DrawText(UI.TOOLS.GENERIC.UNKNOWN, Styles_BodyText.Standard);
            }
            else
            {
                DrawTitle(screen, txt);
                DrawInstructions(screen, txt);

                var standard = Styles_BodyText.Standard;
                if (!InspectTool.Instance.Dragging)
                {
                    var element = ElementInfo.FromCellNumber(cell);

                    txt.NewLine();
                    txt.DrawText(element.NameUppercase, ToolTitleTextStyle);

                    txt.NewLine();
                    txt.DrawIcon(dash);
                    txt.DrawText(element.Category, standard);

                    txt.NewLine();
                    txt.DrawIcon(dash);
                    string[] array = WorldInspector.MassStringsReadOnly(cell);
                    txt.DrawText(array[0], Styles_Values.Property.Standard);
                    txt.DrawText(array[1], Styles_Values.Property_Decimal.Standard);
                    txt.DrawText(array[2], Styles_Values.Property.Standard);
                    txt.DrawText(array[3], Styles_Values.Property.Standard);

                    txt.NewLine();
                    txt.DrawIcon(dash);
                    txt.DrawText(GameUtil.GetFormattedTemperature(element.Temperature), standard);

                    txt.NewLine();
                    txt.DrawIcon(dash);
                    txt.DrawText(element.GetFormattedTotalRelativeHeat(InspectToolSettings.Instance.RelativeTemp), standard);

                    if (Grid.Solid[cell] && Diggable.IsDiggable(cell))
                    {
                        txt.NewLine();
                        txt.DrawIcon(dash);
                        txt.DrawText(GameUtil.GetHardnessString(Grid.Element[cell]), standard);
                    }
                }
                else
                {
                    txt.NewLine();
                    txt.DrawText(((string)InspectToolStrings.HOVER_TEXT_TITLE).ToUpper(), ToolTitleTextStyle);

                    var elements = ElementInspector.ElementData;
                    if (elements != null && elements.Length > 0)
                    {
                        foreach (var e in elements)
                        {
                            txt.NewLine();
                            txt.DrawIcon(dash);
                            txt.DrawText(e, standard);
                        }
                    }
                    else
                    {
                        txt.NewLine();
                        txt.DrawText(InspectToolStrings.HOVER_TEXT_NO_ELEMENTS, standard);
                    }
                }
            }

            txt.EndShadowBar();
            txt.EndDrawing();
        }
    }
}
