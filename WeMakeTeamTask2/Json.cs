using System.Buffers.Text;
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
            options.Converters.Add(new DateTimeConverter4Json('O'));
            return JsonSerializer.Serialize<T>(obj, options);
        }

        internal static T? CreateEntity<T>(string json) where T : class
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,                 

            };
            options.Converters.Add(new DateTimeConverter4Json('O'));
            var entity = JsonSerializer.Deserialize<T>(json, options);            
            return entity;
        }
    }



    public class DateTimeConverter4Json : JsonConverter<DateTime>
    {
        readonly char _standartFormat;
        public DateTimeConverter4Json(char standartFormat)
        {
            _standartFormat = standartFormat;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Перевод пришедшего времени в local date.

            int indexTZseparate = 27;
            // костыль - если 27 символ пробел (32) меняем его на '+' (43), поидеи клиентская сторона должна заменять '+' на %2b
            // и тогда тут будет плюс как и положено!            
            if (reader.ValueSpan[indexTZseparate] == 32)
            {
                Span<byte> bytesValue = new Span<byte>(new byte[reader.ValueSpan.Length]);
                reader.ValueSpan.CopyTo(bytesValue);
                // замена пробела на плюс
                bytesValue[indexTZseparate] = 43;
                if (Utf8Parser.TryParse(bytesValue, out DateTimeOffset value, out _, _standartFormat))
                {
                    return value.LocalDateTime;                   
                }
            }
            else
            {
                // дополнительно проверка наличия разделителя тайм зоны 
                if (reader.ValueSpan[indexTZseparate] != 43 || reader.ValueSpan[indexTZseparate] != 45)
                    throw new FormatException( "Не верный формат даты. Не верно указана тайм зона.");

                if (Utf8Parser.TryParse(reader.ValueSpan, out DateTimeOffset value, out _, _standartFormat))
                {
                    return value.LocalDateTime;
                }
            }                                             

            throw new FormatException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Для каждого формата свое кол-во байт 33 для O,
            // нужна доработка!
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
