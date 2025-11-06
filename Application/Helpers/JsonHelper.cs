using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Application.Helpers
{
    public class JsonHelper
    {
        public static T? DeserializeJSON<T>(string? valueJson) where T : class
        {
            if (string.IsNullOrEmpty(valueJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(valueJson);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}