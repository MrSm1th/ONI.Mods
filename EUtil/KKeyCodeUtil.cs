using System;
using System.Linq;

namespace EUtil
{
    public static class KKeyCodeUtil
    {
        public static bool TryParse(string value, out KKeyCode keyCode, out Modifier modifier)
        {
            keyCode = KKeyCode.None;
            modifier = Modifier.None;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var values = value
                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            if (values.Length == 0)
                return false;

            if (!Enum.TryParse<KKeyCode>(values.Last(), true, out keyCode))
            {
                keyCode = KKeyCode.None;
                return false;
            }

            for (int i = 0; i < values.Length - 1; i++)
            {
                if (!Enum.TryParse<Modifier>(values[i], true, out Modifier m))
                {
                    modifier = Modifier.None;
                    return false;
                }
                else
                {
                    modifier |= m;
                }
            }

            return true;
        }
    }
}
