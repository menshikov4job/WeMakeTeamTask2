п»їusing System.Buffers.Text;
using System.Buffers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WeMakeTeamTask2
{
    internal static class Json
    {
        internal static string CreateJson<T>(T? obj) where T : class
        {
            if (obj == null) return "{}";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping

            };
            options.Converters.Add(new DateTimeOffSetConverter4Json('O'));
            return JsonSerializer.Serialize<T>(obj, options);
        }

        internal static T? CreateEntity<T>(string json) where T : class
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,                 

            };
            options.Converters.Add(new DateTimeOffSetConverter4Json('O'));
            var entity = JsonSerializer.Deserialize<T>(json, options);            
            return entity;
        }
    }

    public class DateTimeOffSetConverter4Json : JsonConverter<DateTimeOffset>
    {
        readonly char _standartFormat;
        public DateTimeOffSetConverter4Json(char standartFormat)
        {
            _standartFormat = standartFormat;
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // РџРµСЂРµРІРѕРґ РїСЂРёС€РµРґС€РµРіРѕ РІСЂРµРјРµРЅРё РІ local date.

            int indexTZseparate = 27;
            // РєРѕСЃС‚С‹Р»СЊ - РµСЃР»Рё 27 СЃРёРјРІРѕР» РїСЂРѕР±РµР» (32) РјРµРЅСЏРµРј РµРіРѕ РЅР° '+' (43), РїРѕРёРґРµРё РєР»РёРµРЅС‚СЃРєР°СЏ СЃС‚РѕСЂРѕРЅР° РґРѕР»Р¶РЅР° Р·Р°РјРµРЅСЏС‚СЊ '+' РЅР° %2b
            // Рё С‚РѕРіРґР° С‚СѓС‚ Р±СѓРґРµС‚ РїР»СЋСЃ РєР°Рє Рё РїРѕР»РѕР¶РµРЅРѕ!            
            if (reader.ValueSpan[indexTZseparate] == 32)
            {
                Span<byte> bytesValue = new Span<byte>(new byte[reader.ValueSpan.Length]);
                reader.ValueSpan.CopyTo(bytesValue);
                // Р·Р°РјРµРЅР° РїСЂРѕР±РµР»Р° РЅР° РїР»СЋСЃ
                bytesValue[indexTZseparate] = 43;
                if (Utf8Parser.TryParse(bytesValue, out DateTimeOffset value, out _, _standartFormat))
                {
                    return value;
                }
            }
            else
            {
                // РґРѕРїРѕР»РЅРёС‚РµР»СЊРЅРѕ РїСЂРѕРІРµСЂРєР° РЅР°Р»РёС‡РёСЏ СЂР°Р·РґРµР»РёС‚РµР»СЏ С‚Р°Р№Рј Р·РѕРЅС‹ 
                if (reader.ValueSpan[indexTZseparate] != 43 || reader.ValueSpan[indexTZseparate] != 45)
                    throw new FormatException("РќРµ РІРµСЂРЅС‹Р№ С„РѕСЂРјР°С‚ РґР°С‚С‹. РќРµ РІРµСЂРЅРѕ СѓРєР°Р·Р°РЅР° С‚Р°Р№Рј Р·РѕРЅР°.");

                if (Utf8Parser.TryParse(reader.ValueSpan, out DateTimeOffset value, out _, _standartFormat))
                {
                    return value;
                }
            }

            throw new FormatException();
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            // Р”Р»СЏ РєР°Р¶РґРѕРіРѕ С„РѕСЂРјР°С‚Р° СЃРІРѕРµ РєРѕР»-РІРѕ Р±Р°Р№С‚ 33 РґР»СЏ O,
            // РЅСѓР¶РЅР° РґРѕСЂР°Р±РѕС‚РєР°!
            if (_standartFormat != 'O') throw new NotImplementedException();
            Span<byte> utf8Date = new byte[33];

            if (Utf8Formatter.TryFormat(value, utf8Date, out _, new StandardFormat(_standartFormat)))
            {
                writer.WriteStringValue(utf8Date);
            }
            else
                throw new FormatException();
        }
    }
}
