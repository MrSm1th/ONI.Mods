namespace InspectTool
{
    public static class InspectToolStrings
    {
        public static string ACTION_ID = "InspectTool.InspectToolActionID";
        public static string ACTION_TITLE = "Inspect tool";

        public static string TOOL_ICON_SPRITE_NAME = "icon_action_inspect";
        public static string CURSOR_SPRITE_NAME = "cursor_action_inspect";

        public static LocString TOOL_NAME = new LocString("Inspect");
        public static LocString TOOL_TOOLTIP = new LocString("Inspect properties of selected elements {Hotkey}");

        public static LocString HOVER_TEXT_TITLE = new LocString("Elements");

        public static LocString HOVER_TEXT_NO_ELEMENTS = new LocString("No obtainable elements");

        public static class UnitSuffixes
        {
            public const string None        = "";
            public const string Kilo        = "K";
            public const string Million     = "M";
            public const string Billion     = "B";
            public const string Trillion    = "T";
            public const string Quadrillion = "Qa";
            public const string Quintillion = "Qi";
            public const string Sextillion  = "Sx";
            public const string Septillion  = "Sp";
            public const string Octillion   = "Oc";
            public const string Nonillion   = "No";
            public const string Decillion   = "Dc";

            private static readonly string[] Suffixes = new string[]
            {
                None,
                Kilo,
                Million,
                Billion,
                Trillion,
                Quadrillion,
                Quintillion,
                Sextillion,
                Septillion,
                Octillion,
                Nonillion,
                Decillion
            };

            public static string ElementAtOrDefault(int index)
            {
                if (index < Suffixes.Length)
                    return Suffixes[index];

                return string.Empty;
            }
        }
    }
}
