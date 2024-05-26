using System.ComponentModel;
using System.Reflection;

namespace TruliooExtension.Extensions;

public static class EnumExtension
{
    public static string GetDescription(this Enum e)
    {
        DescriptionAttribute? attribute =
            e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                as DescriptionAttribute;

        return attribute?.Description ?? e.ToString();
    }
}