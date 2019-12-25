using System;

namespace EUtil
{
    public class EDebug
    {
        public static void SerializeToLog(object e)
        {
            var _serializer = new Newtonsoft.Json.JsonSerializer
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,

            };

            Try(() =>
            {
                _serializer.Serialize(Console.Out, e);
                Console.WriteLine();
            });
        }

        public static bool Try(System.Action action)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        public static void LogPadded(string message)
        {
            Debug.Log("================== " + message);
        }
    }
}
