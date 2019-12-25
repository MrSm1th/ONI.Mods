using Newtonsoft.Json;
using PeterHan.PLib;

namespace InspectTool
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InspectToolSettings
    {
        private static InspectToolSettings instance;
        public static InspectToolSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new InspectToolSettings();

                return instance;
            }
            set
            {
                instance = value;
            }
        }

        [JsonProperty]
        [Option("Total mass", "Show total mass in the text card")]
        public bool ShowTotalMass { get; set; }

        [JsonProperty]
        [Option("Average temperature", "Show average temperature in the text card")]
        public bool ShowAvgTemp { get; set; }

        [JsonProperty]
        [Option("Total heat", "Show total relative heat in the text card")]
        public bool ShowTotalHeat { get; set; }

        [JsonProperty]
        [Option("Relative temperature", "Temperature value (Kelvin) used in total relative heat calculation")]
        [Limit(0, 10000)]
        public int RelativeTemp { get; set; }

        [JsonProperty]
        [Option("Tool Position", "Specifies the tool's position (zero-based) on the toolbar")]
        [Limit(0, 1000)]
        public int ToolPosition { get; set; }

        [JsonProperty]
        [Option("Large Button", "Specifies whether to use the larger version of the tool button")]
        public bool LargeIcon { get; set; }

        [JsonProperty]
        [Option("Hotkey", "A hotkey used to activate the tool (requires restart)")]
        public string Hotkey { get; set; }

        public InspectToolSettings()
        {
            ShowTotalHeat = true;
            ShowTotalMass = true;
            ShowAvgTemp = true;
            RelativeTemp = 293; // ~20C
        }
    }
}
