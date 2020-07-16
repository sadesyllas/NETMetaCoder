using System.Reflection;
using System.Threading.Tasks;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// Extension methods for wrapping APIs related to type reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns true if the <see cref="MethodInfo"/> defines that the method returns a <see cref="Task"/> or a
        /// <see cref="Task{T}"/>.
        /// </summary>
        /// <param name="methodInfo"></param>
        public static bool IsAsync(this MethodInfo methodInfo) => typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
    }
}
