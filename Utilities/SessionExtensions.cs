using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ArtGalleryFinal.Utilities
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value)); // Using System.Text.Json for serialization
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value); // Using System.Text.Json for deserialization
        }
    }
}
