using UnityEngine;

namespace SandboxConduitTool
{
    public class SandboxConduitTool : BrushTool
    {
        private enum ConduitType { Solid, Liqud, Gas }

        private const int MAX_SOLID_MASS = 20;
        private const int MAX_LIQUID_MASS = 10;
        private const int MAX_GAS_MASS = 1;

        private bool updateSolidFlowVisualization;
        private bool updateLiquidFlowVisualization;
        private bool updateGasFlowVisualization;

        private int lastCell = -1;
        private bool CursorCellChanged => lastCell != currentCell;
        private SandboxSettings Settings => SandboxToolParameterMenu.instance.settings;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            affectFoundation = true;
            gameObject.AddComponent<SandboxConduitToolHoverTextCard>();
        }

        protected override void OnPaintCell(int cell, int distFromOrigin)
        {
            base.OnPaintCell(cell, distFromOrigin);

            if (!CursorCellChanged)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                ClearConduit(cell);
            }
            else
            {
                FillConduit(cell);
            }
        }

        public override void OnMouseMove(Vector3 cursorPos)
        {
            base.OnMouseMove(cursorPos);

            if (Dragging && CursorCellChanged)
                UpdateFlowVisualization();

            if (Dragging)
                lastCell = currentCell;
        }

        private void ClearConduit(int cell)
        {
            var clearSolid = OverlayScreen.Instance.mode == OverlayModes.SolidConveyor.ID;
            var clearLiquid = OverlayScreen.Instance.mode == OverlayModes.LiquidConduits.ID;
            var clearGas = OverlayScreen.Instance.mode == OverlayModes.GasConduits.ID;

            if (!clearSolid && !clearLiquid && !clearGas)
                clearSolid = clearLiquid = clearGas = true;

            if (clearSolid && Game.Instance.solidConduitFlow.HasConduit(cell))
            {
                var p = Game.Instance.solidConduitFlow.RemovePickupable(cell);
                if (p != null && p.TotalAmount > 0)
                {
                    DestroyPickupable(p);

                    SpawnMinusFX(p.PrimaryElement.name, cell);
                    updateSolidFlowVisualization = true;
                }
            }

            if (clearLiquid && Game.Instance.liquidConduitFlow.HasConduit(cell))
            {
                var e = Game.Instance.liquidConduitFlow.RemoveElement(cell, MAX_LIQUID_MASS);
                if (e.mass > 0)
                {
                    SpawnMinusFX(ElementLoader.FindElementByHash(e.element).name, cell);
                    updateLiquidFlowVisualization = true;
                }
            }

            if (clearGas && Game.Instance.gasConduitFlow.HasConduit(cell))
            {
                var e = Game.Instance.gasConduitFlow.RemoveElement(cell, MAX_GAS_MASS);
                if (e.mass > 0)
                {
                    SpawnMinusFX(ElementLoader.FindElementByHash(e.element).name, cell);
                    updateGasFlowVisualization = true;
                }
            }
        }

        private void FillConduit(int cell)
        {
            //if (conduitFlow == null)
            //{
            //    Debug.LogWarning("Unexpected element: " + Settings.Element.name, this);
            //    return false;
            //}

            var element = ElementLoader.elements[Settings.GetIntSetting(SandboxSettings.KEY_SELECTED_ELEMENT)];
            var diseaseIdx = Db.Get().Diseases.GetIndex(Db.Get().Diseases.Get(Settings.GetStringSetting(SandboxSettings.KEY_SELECTED_DISEASE)).id);
            var mass = Settings.GetFloatSetting(SandboxSettings.KEY_MASS);
            var temperature = Settings.GetFloatSetting(SandboxSettings.KEY_TEMPERATURE);
            var diseaseCount = Settings.GetIntSetting(SandboxSettings.KEY_DISEASE_COUNT);

            var wrongElement =
                !element.IsSolid &&
                !element.IsLiquid &&
                !element.IsGas;

            var conduitType =
                element.IsSolid ? ConduitType.Solid :
                element.IsLiquid ? ConduitType.Liqud :
                ConduitType.Gas;

            if (wrongElement || !SetContents(cell, conduitType, element, mass, temperature, diseaseIdx, diseaseCount))
            {
                SpawnMinusFX(SandboxConduitToolStrings.POPFX_NO_CONDUIT, cell);

                //UISounds.PlaySound(UISounds.Sound.Negative);
            }
            else
            {
                SpawnPlusFX(element.name, cell);
            }
        }

        private bool SetContents(int cell, ConduitType conduitType, Element element, float mass, float temperature, byte diseaseIdx, int diseaseCount)
        {
            if (conduitType == ConduitType.Solid)
            {
                var conduitFlow = Game.Instance.solidConduitFlow;
                if (!conduitFlow.HasConduit(cell))
                    return false;

                mass = Mathf.Clamp(mass, 0, MAX_SOLID_MASS);
                var res = element.substance.SpawnResource(Vector3.zero, mass, temperature, diseaseIdx, diseaseCount);
                var pc = res.GetComponent<Pickupable>();
                var p = conduitFlow.RemovePickupable(cell);
                if (p != null)
                {
                    DestroyPickupable(p);
                }

                conduitFlow.SetContents(cell, pc);
                updateSolidFlowVisualization = true;
            }
            else
            {
                ConduitFlow conduitFlow = conduitType == ConduitType.Liqud
                    ? Game.Instance.liquidConduitFlow
                    : Game.Instance.gasConduitFlow;

                if (!conduitFlow.HasConduit(cell))
                    return false;

                var maxMass = conduitType == ConduitType.Liqud ? MAX_LIQUID_MASS : MAX_GAS_MASS;
                mass = Mathf.Clamp(mass, 0, maxMass);

                var contents = new ConduitFlow.ConduitContents(element.id, mass, temperature, diseaseIdx, diseaseCount);
                conduitFlow.SetContents(cell, contents);

                if (conduitType == ConduitType.Liqud)
                    updateLiquidFlowVisualization = true;
                else
                    updateGasFlowVisualization = true;
            }

            return true;
        }

        private void DestroyPickupable(Pickupable p)
        {
            // I have no idea what I'm doing

            p.gameObject.SetActive(false);
            p.gameObject.DeleteObject();

            Destroy(p);
        }

        private void SpawnPlusFX(string text, int cell)
        {
            SpawnFX(PopFXManager.Instance.sprite_Plus, text, cell);
        }

        private void SpawnMinusFX(string text, int cell)
        {
            SpawnFX(PopFXManager.Instance.sprite_Negative, text, cell);
        }

        private void SpawnFX(Sprite sprite, string text, int cell)
        {
            if (Settings.GetIntSetting(SandboxSettings.KEY_BRUSH_SIZE) == 1)
                PopFXManager.Instance.SpawnFX(sprite, text, null, Grid.CellToPosCBC(cell, visualizerLayer));
        }

        public override void OnLeftClickDown(Vector3 cursor_pos)
        {
            base.OnLeftClickDown(cursor_pos);

            lastCell = currentCell;

            UpdateFlowVisualization();
        }

        public override void OnLeftClickUp(Vector3 cursor_pos)
        {
            base.OnLeftClickUp(cursor_pos);

            lastCell = -1;
        }

        private void UpdateFlowVisualization()
        {
            if (updateSolidFlowVisualization)
            {
                Game.Instance.solidConduitFlow.ForceRebuildNetworks();
                updateSolidFlowVisualization = false;
            }
            if (updateLiquidFlowVisualization)
            {
                Game.Instance.liquidConduitFlow.ForceRebuildNetworks();
                updateLiquidFlowVisualization = false;
            }
            if (updateGasFlowVisualization)
            {
                Game.Instance.gasConduitFlow.ForceRebuildNetworks();
                updateGasFlowVisualization = false;
            }
        }

        //public void Activate()
        //{
        //    PlayerController.Instance.ActivateTool(this);
        //}

        protected override void OnActivateTool()
        {
            base.OnActivateTool();
            SandboxToolParameterMenu.instance.gameObject.SetActive(true);
            SandboxToolParameterMenu.instance.DisableParameters();
            SandboxToolParameterMenu.instance.brushRadiusSlider.row.SetActive(true);
            SandboxToolParameterMenu.instance.massSlider.row.SetActive(true);
            SandboxToolParameterMenu.instance.temperatureSlider.row.SetActive(true);
            SandboxToolParameterMenu.instance.elementSelector.row.SetActive(true);
            SandboxToolParameterMenu.instance.diseaseSelector.row.SetActive(true);
            SandboxToolParameterMenu.instance.diseaseCountSlider.row.SetActive(true);
        }

        protected override void OnDeactivateTool(InterfaceTool new_tool)
        {
            base.OnDeactivateTool(new_tool);
            SandboxToolParameterMenu.instance.gameObject.SetActive(value: false);
        }
    }
}
