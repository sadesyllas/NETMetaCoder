using System.Collections.Generic;
using System.Reflection;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// An equality comparer that checks if two <see cref="ParameterInfo"/> objects are equal.
    /// </summary>
    public class ParameterInfoEqualityComparer : IEqualityComparer<ParameterInfo>
    {
        /// <summary>
        /// Checks if two <see cref="ParameterInfo"/> objects are equal.
        /// </summary>
        /// <param name="parameterInfo1"></param>
        /// <param name="parameterInfo2"></param>
        /// <returns></returns>
        // ReSharper disable AssignNullToNotNullAttribute
        public bool Equals(ParameterInfo parameterInfo1, ParameterInfo parameterInfo2) =>
            GetHashCode(parameterInfo1) == GetHashCode(parameterInfo2);
        // ReSharper restore AssignNullToNotNullAttribute

        /// <summary>
        /// Calculates the hash code of a <see cref="ParameterInfo"/>.
        ///
        /// It is meant to be used by <see cref="Equals"/>.
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public int GetHashCode(ParameterInfo parameterInfo)
        {
            var modifiersMultiplier = parameterInfo.IsOptional ? 1 : 2;
            modifiersMultiplier *= parameterInfo.IsIn ? 10 : 20;
            modifiersMultiplier *= parameterInfo.IsLcid ? 100 : 200;
            modifiersMultiplier *= parameterInfo.IsOut ? 1_000 : 2_000;
            modifiersMultiplier *= parameterInfo.IsRetval ? 10_000 : 20_000;

            return unchecked(
                parameterInfo.ParameterType.Name.GetHashCode() *
                (parameterInfo.Position + 1) *
                modifiersMultiplier);
        }
    }
}
