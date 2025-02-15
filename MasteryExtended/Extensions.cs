using StardewModdingAPI;
using System.Reflection;

namespace MasteryExtended
{
    /// <summary>Extension methods stolen from SkillPrestige</summary>
    public static class Extensions
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;
        /*********
        ** Public methods
        *********/
        /// <summary>gets the field from an object through reflection, even if it is a private field.</summary>
        /// <typeparam name="T">The type that contains the parameter member</typeparam>
        /// <param name="instance">The instance of the type you wish to get the field from.</param>
        /// <param name="fieldName">The name of the field you wish to retreive.</param>
        /// <returns>Returns null if no field member found.</returns>
        public static object? GetInstanceField<T>(this T instance, string fieldName)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var memberInfo = instance!.GetType().GetField(fieldName, bindingAttributes);
            return memberInfo?.GetValue(instance);
        }

        /// <summary>sets the field from an object through reflection, even if it is a private field.</summary>
        /// <typeparam name="T">The type that contains the parameter member</typeparam>
        /// <typeparam name="TMember">The type of the parameter member</typeparam>
        /// <param name="instance">The instance of the type you wish to set the field value of.</param>
        /// <param name="fieldName">>The name of the field you wish to set.</param>
        /// <param name="value">The value you wish to set the field to.</param>
        public static void SetInstanceField<T, TMember>(this T instance, string fieldName, TMember value)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var memberInfo = instance!.GetType().GetField(fieldName, bindingAttributes);
            memberInfo?.SetValue(instance, value);
        }

        /// <summary>Invokes and returns the value of a function, even if it is a private member.</summary>
        /// <param name="instance">The instance of the type that contains the function to call.</param>
        /// <param name="functionName">The name of the static function.</param>
        /// <param name="arguments">The arguments passed to the static function.</param>
        public static object? InvokeFunction<T>(this T instance, string functionName, params object[] arguments)
        {
            try
            {
                const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var method = instance!.GetType().GetMethod(functionName, bindingAttributes);
                if (method == null) return default;
                return method.Invoke(instance, arguments);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}