using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class EnumExtensions
    {
        static IDictionary<int, String> GetEnumValueNames(Type type)
        {
            var names = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => f.GetCustomAttribute<DisplayAttribute>()?.Name ?? f.Name);

            var values = Enum.GetValues(type).Cast<int>();

            var dictionary = names.Zip(values, (n, v) => new KeyValuePair<int, string>(v, n))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

            return dictionary;
        }

        public static String GetDisplayName<T>(this T value) where T : Enum
        {
            var enumValue = Enum.GetValues(typeof(T)).Cast<int>().Single(v => v == System.Convert.ToInt32(value);
            var dictionary = GetEnumValueNames(value.GetType());
            return dictionary[enumValue];
        }
    }
}
