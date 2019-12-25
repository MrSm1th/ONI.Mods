using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;

namespace InspectTool
{
    public enum ElementState { Solid, Liquid, Gas, Other }

    public struct ElementInfo
    {
        public SimHashes Id;
        public string Name;
        public string NameUppercase;
        public string SubstanceName;
        public float Mass;
        public float Temperature;
        public float HeatCapacity;
        public ElementState State;
        public string Category;

        public static ElementInfo FromCellNumber(int cell)
        {
            var e = Grid.Element[cell];
            return new ElementInfo
            {
                Id = e.id,
                Name = e.name,
                NameUppercase = e.nameUpperCase,
                SubstanceName = e.substance.name,
                State =
                    e.IsSolid ? ElementState.Solid :
                    e.IsLiquid ? ElementState.Liquid :
                    e.IsGas ? ElementState.Gas
                            : ElementState.Other,
                Category = e.GetMaterialCategoryTag().ProperName(),
                Mass = Grid.Mass[cell],
                Temperature = Grid.Temperature[cell],
                HeatCapacity = e.specificHeatCapacity
            };
        }

        public override string ToString()
        {
            var result = Name;
            if (InspectToolSettings.Instance.ShowTotalMass)
            {
                var mass = GetFormattedMass(Mass);
                result += " " + mass;
            }
            if (InspectToolSettings.Instance.ShowAvgTemp)
            {
                var temp = GameUtil.GetFormattedTemperature(Temperature);
                result += " @ " + temp;
            }
            if (InspectToolSettings.Instance.ShowTotalHeat)
            {
                var heat = GetFormattedTotalRelativeHeat(InspectToolSettings.Instance.RelativeTemp);
                result += " (" + heat + ")";
            }

            return result;
        }

        public string GetFormattedTotalRelativeHeat(float relativeT)
        {
            //var tCel = GameUtil.GetTemperatureConvertedFromKelvin(Temperature, GameUtil.TemperatureUnit.Celsius);
            var totalRelativeHeat = (Temperature - relativeT) * Mass * 1000 * HeatCapacity;
            totalRelativeHeat = NormalizeValue(totalRelativeHeat, out string s);

            return $"{totalRelativeHeat:0}{s} DTU";
        }

        private string GetFormattedMass(float massValue, bool addDiggableValue = false)
        {
            // convert to tonnes first
            massValue /= 1000;

            var units = new[]
            {
                UI.UNITSUFFIXES.MASS.TONNE,
                UI.UNITSUFFIXES.MASS.KILOGRAM,
                UI.UNITSUFFIXES.MASS.GRAM,
                UI.UNITSUFFIXES.MASS.MILLIGRAM,
                UI.UNITSUFFIXES.MASS.MICROGRAM
            };

            int unitIdx = 0;
            while (massValue < 5f)
            {
                massValue *= 1000f;
                unitIdx++;
            }

            if (unitIdx >= 4)
                massValue = Mathf.Floor(massValue);

            var unit = units[unitIdx];


            //var unit = UI.UNITSUFFIXES.MASS.KILOGRAM;
            //if (massValue < 5f)
            //{
            //    massValue *= 1000f;
            //    unit = UI.UNITSUFFIXES.MASS.GRAM;
            //}
            //if (massValue < 5f)
            //{
            //    massValue *= 1000f;
            //    unit = UI.UNITSUFFIXES.MASS.MILLIGRAM;
            //}
            //if (massValue < 5f)
            //{
            //    massValue *= 1000f;
            //    unit = UI.UNITSUFFIXES.MASS.MICROGRAM;
            //    massValue = Mathf.Floor(massValue);
            //}

            var fmt = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
            return
                massValue.ToString("#.0", fmt) +
                (addDiggableValue ? " (" + (massValue / 2).ToString("#.0", fmt) + ")" : null) +
                unit;
        }

        private float NormalizeValue(float value, out string suffix)
        {
            //var suffixes = new[] { "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc" };
            int s = 0;
            while (Mathf.Abs(value) > 1000)
            {
                value /= 1000;
                s++;
            }
            //if (s >= suffixes.Length) s = 0;
            //suffix = suffixes[s];
            suffix = InspectToolStrings.UnitSuffixes.ElementAtOrDefault(s);
            return value;
        }
    }

    public static class ElementInspector
    {
        private static int[] selectedCells;

        public static string[] ElementData { get; private set; }

        public static void UpdateElementData(IEnumerable<int> cells)
        {
            selectedCells = cells.ToArray();
            UpdateElementData();
        }

        public static void UpdateElementData()
        {
            if (selectedCells == null || !selectedCells.Any())
                return;

            ElementData = selectedCells
                .Where(ContainsObtainableElement)
                .Select(ProjectToElementInfo)
                .GroupBy(element => element.Id)
                .Select(AggregateToElementInfo)
                .OrderBy(element => element.State)
                .ThenBy(element => element.SubstanceName)
                .Select(e => e.ToString())
                .ToArray();
        }

        private static ElementInfo ProjectToElementInfo(int c)
        {
            return ElementInfo.FromCellNumber(c);
        }

        private static ElementInfo AggregateToElementInfo(IGrouping<SimHashes, ElementInfo> g)
        {
            var first = g.First();
            return new ElementInfo
            {
                Id = g.Key,
                Name = first.Name,
                SubstanceName = first.SubstanceName,
                State = first.State,
                HeatCapacity = first.HeatCapacity,

                Mass = g.Sum(e => e.Mass),
                Temperature = g.Average(e => e.Temperature),
            };
        }

        private static bool ContainsObtainableElement(int cell)
        {
            var element = Grid.Element[cell];
            return element.id != SimHashes.Unobtanium && element.id != SimHashes.Vacuum;
        }
    }
}
