using System.Collections;
using System.Reflection;
using System.Text;

namespace TruliooExtension.Helpers
{
    public static class ObjectInitializerHelper
    {
        public static string ObjectInitializer<T>(T? obj)
        {
            if (Equals(obj, default(T)))
                return string.Empty;
            
            StringBuilder sb = new ();
            
            sb.Clear();
            sb.Append($"{typeof(T).Name} obj=");
            
            GenerateObjectInitializer(sb, obj);

            RemoveLastComma(sb);
            
            sb.Append(';');
            
            return CodeFormatterHelper.FormatCode(sb.ToString());
        }

        private static void GenerateObjectInitializer(StringBuilder sb, object? obj)
        {
            if (obj == null)
                return;

            var objType = obj.GetType();
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            sb.Append($"new {objType.Name}(){{");

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                object? propValue = property.GetValue(obj);

                if (propValue == null || (propValue is string str && string.IsNullOrEmpty(str)))
                    continue;

                sb.Append($"{property.Name}=");
                GeneratePropertyValue(sb, propValue);
                
                // remove the comma from the last property
                if (i == properties.Length - 1)
                {
                    RemoveLastComma(sb);
                }
            }
            sb.Append("},");
        }

        private static void GeneratePropertyValue(StringBuilder sb, object? propValue)
        {
            switch (propValue)
            {
                case null:
                    return;
                case string stringValue:
                    sb.Append($"\"{stringValue}\",");
                    break;
                case IEnumerable enumerable:
                    GenerateEnumerable(sb, enumerable);
                    break;
                case Enum enumValue when enumValue.GetType().IsEnumDefined(enumValue):
                    sb.Append($"{enumValue.GetType().Name}.{Enum.Parse(enumValue.GetType(), enumValue.ToString()).ToString()},");
                    break;
                case DateTime:
                case DateTimeOffset:
                case Guid:
                    var type = propValue.GetType().Name;
                    sb.Append($"{type}.Parse(\"{propValue}\"),");
                    break;
                case bool boolValue:
                    sb.Append(boolValue.ToString().ToLower() + ",");
                    break;
                case Uri uri:
                    sb.Append("new Uri(\"" + uri + "\"),");
                    break;
                case not null when propValue.GetType().IsClass:
                    GenerateObjectInitializer(sb, propValue);
                    break;
                case not null when propValue.GetType().IsValueType:
                    sb.Append($"{propValue},");
                    break;
                default:
                    sb.Append($"\"{propValue}\",");
                    break;
            }
        }

        private static void GenerateEnumerable(StringBuilder sb, IEnumerable enumerable)
        {
            var enumerableType = enumerable.GetType().GetGenericArguments().Length != 0 ? enumerable.GetType().GetGenericArguments()[0] : enumerable.GetType().GetElementType();
            
            string enumerableTypeName = enumerableType!.Name;
            if (enumerableType == typeof(string) || enumerableTypeName == "Object")
            {
                enumerableTypeName = enumerableType.Name.ToLower();
            }
            
            var items = enumerable.Cast<object>().ToList();
            bool isArray = enumerable.GetType().IsArray;
            if (items.Count == 0)
            {
                var prefix = isArray ? "Array.Empty" : "new List";
                sb.Append($"{prefix}<{enumerableTypeName}>(),");
                return;
            }

            if (isArray)
            {
                sb.Append($"new {enumerableTypeName}[]{{");
            }
            else
            {
                sb.Append($"new List<{enumerableTypeName}>(){{");
            }
            
            for (int i = 0; i < items.Count; i++)
            {
                GeneratePropertyValue(sb, items[i]);

                // remove the comma from the last item
                if (i == items.Count - 1)
                {
                    RemoveLastComma(sb);
                }
            }

            sb.Append("},");
        }
        
        private static void RemoveLastComma(StringBuilder sb)
        {
            if (sb.Length > 0 && sb[^1] == ',')
            {
                sb.Remove(sb.Length - 1, 1);
            }
        }
    }
}