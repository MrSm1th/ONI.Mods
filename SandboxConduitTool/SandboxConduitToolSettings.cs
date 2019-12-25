using Newtonsoft.Json;
using PeterHan.PLib;

namespace SandboxConduitTool
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SandboxConduitToolSettings
    {
        private static SandboxConduitToolSettings instance;
        public static SandboxConduitToolSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new SandboxConduitToolSettings();

                return instance;
            }
            set
            {
                instance = value;
            }
        }

        [JsonProperty]
        [Option("Tool Position", "Specifies the tool's position (zero-based) on the toolbar")]
        [Limit(0, 1000)]
        public int ToolPosition { get; set; }

        [JsonProperty]
        [Option("Hotkey", "A hotkey used to activate the tool (requires restart)")]
        public string Hotkey { get; set; }

        public SandboxConduitToolSettings()
        {
            ToolPosition = 5;
        }
    }
}
