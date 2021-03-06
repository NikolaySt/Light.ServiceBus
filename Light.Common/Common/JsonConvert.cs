using System;
using Newtonsoft.Json;

namespace Light.Common
{
	public static class JsonConvert
	{
		private static readonly JsonSerializerSettings JsonSettings;

		static JsonConvert()
		{
			JsonSettings = GetDefaultSettings();
		}

		public static T ToObject<T>(string json)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, JsonSettings);
		}

		public static object ToObject(string json, Type type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));
			return Newtonsoft.Json.JsonConvert.DeserializeObject(json, type, JsonSettings);
		}

		public static string ToJson(object input, bool withTypeInfo = false)
		{
			var settings = JsonSettings;
			if (withTypeInfo)
			{
				settings = GetDefaultSettings();
				settings.TypeNameHandling = TypeNameHandling.All;
			}

			return Newtonsoft.Json.JsonConvert.SerializeObject(input, settings);
		}

		public static void Populate(string json, object target)
		{
			Newtonsoft.Json.JsonConvert.PopulateObject(json, target, JsonSettings);
		}

		private static JsonSerializerSettings GetDefaultSettings()
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.None,
				FloatParseHandling = FloatParseHandling.Decimal,
				Formatting = Formatting.None
			};

			return settings;
		}
	}
}