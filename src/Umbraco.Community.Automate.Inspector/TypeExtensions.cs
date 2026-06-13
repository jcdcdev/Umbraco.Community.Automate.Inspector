namespace Umbraco.Community.Automate.Inspector;

public static class TypeExtensions
{
    public static string GetTypeName(this Type? type)
    {
        if (type == null)
        {
            return string.Empty;
        }

        // Handle nullable types
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            return nullableType.GetTypeName() + "?";
        }

        // Handle generic types
        if (type.IsGenericType)
        {
            var genericTypeName = (type.Name).Split('`')[0];
            var typeArgs = type.GetGenericArguments();
            var typeArgsNames = typeArgs.Select(GetTypeName).ToArray();
            return $"{genericTypeName}<{string.Join(", ", typeArgsNames)}>";
        }

        return type.Name;
    }

    public static bool IsComplexType(this Type? type, bool includeSystemType = false)
    {
        if (type == null)
        {
            return false;
        }

        if (!includeSystemType && type.Namespace != null && type.Namespace.StartsWith("System"))
        {
            return false;
        }

        // Handle nullable types
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            type = nullableType;
        }

        // Check for primitive types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid) || type == typeof(decimal))
        {
            return false;
        }

        // Handle generic types (e.g., List<T>, IEnumerable<T>)
        if (type.IsGenericType)
        {
            // Check if it's a collection of primitives
            var typeArgs = type.GetGenericArguments();
            if (typeArgs.Length == 1)
            {
                // If the element type is not complex, then the collection itself is not considered complex for our purposes
                return typeArgs[0].IsComplexType();
            }

            // Dictionary<K,V> or other multi-arg generics are considered complex
            return true;
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType.IsComplexType();
        }

        // If it's a value type (struct) that's not a primitive, check its properties
        if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
        {
            return type.GetProperties().Length > 0;
        }

        // Reference types (classes) with properties are complex
        return type.IsClass && type.GetProperties().Length > 0;
    }

    public static string GetComplexTypeSchema(this Type? type)
    {
        if (type == null || !type.IsComplexType())
        {
            return string.Empty;
        }

        // Handle nullable types
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType != null)
        {
            type = nullableType;
        }

        // Handle generic collections - get the element type
        if (type.IsGenericType && type.GetGenericTypeDefinition().FullName != null)
        {
            var typeArgs = type.GetGenericArguments();
            if (typeArgs.Length == 1)
            {
                var elementType = typeArgs[0];
                if (elementType.IsComplexType())
                {
                    return elementType.GetComplexTypeSchema();
                }
            }
        }

        // Handle arrays
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType.IsComplexType())
            {
                return elementType.GetComplexTypeSchema();
            }
        }

        var properties = type.GetProperties().OrderBy(x => x.Name).ToList();
        if (properties.Count == 0)
        {
            return string.Empty;
        }

        var schemaParts = properties.Select(p =>
        {
            var propType = p.PropertyType.GetTypeName();
            return $"{p.Name}: {propType}";
        });

        var parts = string.Join("\n", schemaParts);
        return parts;
    }
}