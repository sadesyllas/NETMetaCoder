// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// The result returned by <see cref="NETMetaCoderAttribute.Intercept"/>.
    /// </summary>
    public class InterceptionResult
    {
        /// <summary>
        /// Constructs a new <see cref="InterceptionResult"/> instance.
        /// </summary>
        protected InterceptionResult()
        {
        }

        /// <summary>
        /// True whenever a call to a method is intercepted, as decided by the implementation of the
        /// <see cref="NETMetaCoderAttribute"/>.
        /// </summary>
        public bool IsIntercepted { get; protected set; }

        /// <summary>
        /// An object to be provided and interpreted by the implementation of the <see cref="NETMetaCoderAttribute"/>.
        /// </summary>
        public object Context { get; protected set; }

        /// <summary>
        /// A helper method that returns an <see cref="InterceptionResult"/> and is meant to be used when no
        /// interception is desired.
        /// </summary>
        /// <param name="context"></param>
        public static InterceptionResult NotIntercepted(object context = null) => new InterceptionResult
        {
            Context = context
        };

        /// <summary>
        /// A helper method that returns an <see cref="InterceptionResult"/> and is meant to be used when interception
        /// is desired.
        /// </summary>
        /// <param name="context"></param>
        public static InterceptionResult Intercepted(object context = null) => new InterceptionResult
        {
            IsIntercepted = true,
            Context = context
        };
    }

    /// <summary>
    /// The result returned by the generic <c>Intercept</c> methods in <see cref="NETMetaCoderAttribute"/>.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of <see cref="Value"/> which is returned instead of the original method result, in case that
    /// interception is desired by the implementation of <see cref="NETMetaCoderAttribute"/>.
    /// has occurred.
    /// </typeparam>
    public sealed class InterceptionResult<TValue> : InterceptionResult
    {
        private InterceptionResult()
        {
        }

        /// <summary>
        /// The value to use
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// A helper method that returns an <see cref="InterceptionResult{T}"/> and is meant to be used when no
        /// interception is desired.
        /// </summary>
        /// <param name="context"></param>
        public new static InterceptionResult<TValue> NotIntercepted(object context = null) =>
            new InterceptionResult<TValue>
            {
                Context = context
            };

        /// <summary>
        /// A helper method that returns an <see cref="InterceptionResult{T}"/> and is meant to be used when
        /// interception is desired.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        public static InterceptionResult<TValue> Intercepted(TValue value, object context = null) =>
            new InterceptionResult<TValue>
            {
                IsIntercepted = true,
                Context = context,
                Value = value
            };

        /// <summary>
        /// A converter function to turn an <see cref="InterceptionResult"/> into an
        /// <see cref="InterceptionResult{TValue}"/>, using a default value for <see cref="Value"/>.
        /// </summary>
        /// <param name="interceptionResult"></param>
        public static InterceptionResult<TValue> From(InterceptionResult interceptionResult) =>
            interceptionResult.IsIntercepted
                ? Intercepted(default, interceptionResult.Context)
                : NotIntercepted(interceptionResult.Context);

        /// <summary>
        /// A converter function to turn an <see cref="InterceptionResult"/> into an
        /// <see cref="InterceptionResult{TValue}"/>.
        /// </summary>
        /// <param name="interceptionResult"></param>
        /// <typeparam name="TOtherValue"></typeparam>
        public static InterceptionResult<TValue> From<TOtherValue>(InterceptionResult<TOtherValue> interceptionResult)
            where TOtherValue : TValue =>
            interceptionResult.IsIntercepted
                ? Intercepted(interceptionResult.Value, interceptionResult.Context)
                : NotIntercepted(interceptionResult.Context);
    }
}
