using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace InspectTool
{
    public class InspectTool : DragTool
    {
        public static InspectTool Instance;

        public static void DestroyInstance()
        {
            Instance = null;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            Instance = this;

            //interceptNumberKeysForPriority = true;
            gameObject.AddComponent<InspectToolHoverTextCard>();

            if (SelectTool.Instance == null || PrioritizeTool.Instance == null || DigTool.Instance == null)
                return;

            visualizer = new GameObject("CreateBlueprintVisualizer");
            visualizer.SetActive(false);

            GameObject offsetObject = new GameObject();
            SpriteRenderer spriteRenderer = offsetObject.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.white;
            spriteRenderer.sprite = InspectToolAssets.InspectToolCursor;

            offsetObject.transform.SetParent(visualizer.transform);
            offsetObject.transform.localPosition = new Vector3(0, Grid.HalfCellSizeInMeters);
            offsetObject.transform.localScale = new Vector3(
                Grid.CellSizeInMeters / (spriteRenderer.sprite.texture.width / spriteRenderer.sprite.pixelsPerUnit),
                Grid.CellSizeInMeters / (spriteRenderer.sprite.texture.height / spriteRenderer.sprite.pixelsPerUnit)
            );

            offsetObject.SetLayerRecursively(LayerMask.NameToLayer("Overlay"));
            visualizer.transform.SetParent(transform);

            var self = Traverse.Create(this);
            var donorTool = Traverse.Create(SelectTool.Instance);
            //var donorTool2 = Traverse.Create(PrioritizeTool.Instance);
            var donorTool3 = Traverse.Create(DigTool.Instance);

            cursor = donorTool.Field<Texture2D>("cursor").Value;
            self.Field("boxCursor").SetValue(cursor);

            var digAreaVisualizer = donorTool3.Field<GameObject>("areaVisualizer").Value;

            var thisAreaVisualizer = Util.KInstantiate(digAreaVisualizer, gameObject, "inspectToolAreaVisualizer");
            thisAreaVisualizer.SetActive(false);
            thisAreaVisualizer.transform.SetParent(transform);
            thisAreaVisualizer.GetComponent<SpriteRenderer>().color = Color.white;
            thisAreaVisualizer.GetComponent<SpriteRenderer>().material.color = Color.white;

            //visualizer = Util.KInstantiate(donorTool2.Field<GameObject>("visualizer").Value, gameObject, "InspectToolSprite");
            //visualizer.SetActive(false);

            areaVisualizerSpriteRenderer = thisAreaVisualizer.GetComponent<SpriteRenderer>();
            self.Field("areaVisualizer").SetValue(thisAreaVisualizer);
            self.Field("areaColour").SetValue(new Color32(255, 255, 255, 255));
            self.Field("areaVisualizerTextPrefab").SetValue(donorTool3.Field<GameObject>("areaVisualizerTextPrefab").Value);
        }

        public override void OnMouseMove(Vector3 cursorPos)
        {
            base.OnMouseMove(cursorPos);

            if (!Dragging)
                return;

            var selectedCells = new List<int>();

            Grid.PosToXY(downPos, out int x, out int y);
            Grid.PosToXY(cursorPos, out int x2, out int y2);

            if (x2 < x)
            {
                Util.Swap(ref x, ref x2);
            }
            if (y2 < y)
            {
                Util.Swap(ref y, ref y2);
            }
            for (int i = y; i <= y2; i++)
            {
                for (int j = x; j <= x2; j++)
                {
                    int cell = Grid.XYToCell(j, i);
                    if (Grid.IsValidCell(cell) &&
                        Grid.IsVisible(cell) &&
                        (Diggable.IsDiggable(cell) || Grid.IsLiquid(cell) || Grid.IsGas(cell))/* && Grid.IsSolidCell(cell)*/)
                    {
                        selectedCells.Add(cell);
                    }
                }
            }

            ElementInspector.UpdateElementData(selectedCells);
        }

        public override void OnLeftClickDown(Vector3 cursor_pos)
        {
            base.OnLeftClickDown(cursor_pos);

            ElementInspector.UpdateElementData(new[] { Grid.PosToCell(cursor_pos) });
        }
    }
}
