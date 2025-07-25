﻿using System.Text.Json;
namespace NguyenKhoa_2280601517.Extensions
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key,
    object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string
    key)
        {
            var value = session.GetString(key);
            return value == null ? default :
    JsonSerializer.Deserialize<T>(value);
        }
    }
}
