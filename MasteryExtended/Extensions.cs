using StardewModdingAPI;
using System.Reflection;

namespace MasteryExtended
{
    internal static class Extensions
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        private const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        extension(object instance)
        {
            /// <summary>Gets the field from an object through reflection.</summary>
            /// <param name="fieldName">The name of the field you wish to retrieve.</param>
            /// <returns>Returns null if no field member found.</returns>
            public object? GetInstanceField(string fieldName)
            {
                var memberInfo = instance!.GetType().GetField(fieldName, bindingAttributes);
                return memberInfo?.GetValue(instance);
            }

            /// <summary>Sets the field from an object through reflection.</summary>
            /// <typeparam name="TMember">The type of the parameter member</typeparam>
            /// <param name="fieldName">The name of the field you wish to set.</param>
            /// <param name="value">The value you wish to set the field to.</param>
            public void SetInstanceField<TMember>(string fieldName, TMember value)
            {
                var memberInfo = instance!.GetType().GetField(fieldName, bindingAttributes);
                memberInfo?.SetValue(instance, value);
            }

            /// <summary>Invokes and returns the value of a function.</summary>
            /// <param name="functionName">The name of the function to call.</param>
            /// <param name="arguments">The arguments passed to the function.</param>
            public object? InvokeFunction(string functionName, params object[] arguments)
            {
                try
                {
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

        extension(int n)
        {
            internal int DisplayLength =>
                n == 0 ? 1 : (n > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));
        }
    }
}