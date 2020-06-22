using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// The main entry point into the functionality provided by the <c>NETMetaCoder</c> library.
    ///
    /// This type is to be used as the base class for all attribute implementations that wrap methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    // ReSharper disable once InconsistentNaming
    public abstract class NETMetaCoderAttribute : Attribute
    {
        /// <summary>
        /// This method is called when the methods attribute is first read and cached, during runtime.
        ///
        /// It is meant to initialize the attribute's state.
        /// </summary>
        /// <param name="isAsync">
        /// True if the method that is being wrapped returns a <see cref="Task"/> or <see cref="Task{T}"/>.
        /// </param>
        /// <param name="containerType">
        /// The type of the class or struct that contains the method.
        /// </param>
        /// <param name="returnType">
        /// The type of the method that is being wrapped.
        /// </param>
        /// <param name="methodName">
        /// The name of the method that is being wrapped.
        /// </param>
        /// <param name="parameterTypes">
        /// The types of the parameters of the method that is being wrapped.
        /// </param>
        public virtual void Init(bool isAsync, Type containerType, Type returnType, string methodName,
            Type[] parameterTypes)
        {
        }

        /// <summary>
        /// This method is called for synchronous wrapped methods that do not return a value.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation is expected to decide whether to intercept the call
        /// to the wrapped method.
        /// </summary>
        /// <param name="arguments">
        /// The arguments that were passed to the currently processed invocation of the wrapped method.
        /// </param>
        /// <returns>
        /// Returns an <see cref="InterceptionResult"/> that represents whether or not the call to the original method
        /// has been intercepted.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual InterceptionResult Intercept(object[] arguments) => InterceptionResult.NotIntercepted();

        /// <summary>
        /// This method is called for synchronous wrapped methods that return a value.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation is expected to decide whether to intercept the call
        /// to the wrapped method.
        /// </summary>
        /// <param name="arguments">
        /// The arguments that were passed to the currently processed invocation of the wrapped method.
        /// </param>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called.
        /// </param>
        /// <returns>
        /// Returns an <see cref="InterceptionResult{T}"/> that represents whether or not the call to the original
        /// method has been intercepted and if it has, the value which should replace <paramref name="value"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual InterceptionResult<TValue> Intercept<TValue>(object[] arguments, ref TValue value)
        {
            var result = Intercept(arguments);

            return InterceptionResult<TValue>.From(result);
        }

        /// <summary>
        /// This method is called for asynchronous wrapped methods that return a value, through a <see cref="Task"/>.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation is expected to decide whether to intercept the call
        /// to the wrapped method.
        /// </summary>
        /// <param name="arguments">
        /// The arguments that were passed to the currently processed invocation of the wrapped method.
        /// </param>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called.
        /// </param>
        /// <returns>
        /// Returns an <see cref="InterceptionResult{T}"/> that represents whether or not the call to the original
        /// method has been intercepted and if it has, the value which should replace <paramref name="value"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual InterceptionResult<Task<TValue>> Intercept<TValue>(object[] arguments, ref Task<TValue> value)
        {
            var result = Intercept(arguments);

            return InterceptionResult<Task<TValue>>.From(result);
        }

        /// <summary>
        /// This method is called to handle the <see cref="InterceptionResult"/> returned by <see cref="Intercept"/>.
        ///
        /// This method will be unconditionally called either when the call to the original method is intercepted, or
        /// not.
        /// </summary>
        /// <param name="interceptionResult"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void HandleInterceptionResult(ref InterceptionResult interceptionResult)
        {
        }

        /// <summary>
        /// This method is called to handle the <see cref="InterceptionResult{T}"/> returned by
        /// <c>Intercept&lt;T&gt;</c>.
        ///
        /// This method will be unconditionally called either when the call to the original method is intercepted, or
        /// not.
        /// </summary>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called, even by the call to <c>Intercept&lt;T&gt;</c>.
        /// </param>
        /// <param name="interceptionResult"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void HandleInterceptionResult<TValue>(ref TValue value,
            ref InterceptionResult<TValue> interceptionResult)
        {
            var result = (InterceptionResult) interceptionResult;

            HandleInterceptionResult(ref result);
        }

        /// <summary>
        /// This method is called to handle the <see cref="InterceptionResult{T}"/> returned by
        /// <c>Intercept&lt;Task&lt;T&gt;&gt;</c>.
        ///
        /// This method will be unconditionally called either when the call to the original method is intercepted, or
        /// not.
        /// </summary>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called, even by the call to <c>Intercept&lt;Task&lt;T&gt;&gt;</c>.
        /// </param>
        /// <param name="interceptionResult"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void HandleInterceptionResult<TValue>(ref Task<TValue> value,
            ref InterceptionResult<Task<TValue>> interceptionResult)
        {
            var result = (InterceptionResult) interceptionResult;

            HandleInterceptionResult(ref result);
        }

        /// <summary>
        /// This method is called to handle any exception thrown by the wrapped call to original method, when that
        /// method is synchronous and does not return a value.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation can choose to handle the exception through this
        /// method.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="interceptionResult"></param>
        /// <returns>
        /// True if the exception is handled.
        ///
        /// Otherwise, the exception is rethrown.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool HandleException(Exception exception, ref InterceptionResult interceptionResult) => false;

        /// <summary>
        /// This method is called to handle any exception thrown by the wrapped call to original method, when that
        /// method is synchronous and returns a value.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation can choose to handle the exception through this
        /// method.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called, even by the call to <c>Intercept&lt;T&gt;</c>.
        /// </param>
        /// <param name="interceptionResult"></param>
        /// <returns>
        /// True if the exception is handled.
        ///
        /// Otherwise, the exception is rethrown.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool HandleException<TValue>(Exception exception, ref TValue value,
            ref InterceptionResult<TValue> interceptionResult)
        {
            var interceptionResultTmp = (InterceptionResult) interceptionResult;

            return HandleException(exception, ref interceptionResultTmp);
        }

        /// <summary>
        /// This method is called to handle any exception thrown by the wrapped call to original method, when that
        /// method is synchronous and returns a value.
        ///
        /// The <see cref="NETMetaCoderAttribute"/> implementation can choose to handle the exception through this
        /// method.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="value">
        /// The reference to the value that is to be returned by the wrapped method invocation.
        ///
        /// This value may have already been changed by another <see cref="NETMetaCoderAttribute"/> implementation, by
        /// the time that this method gets called, even by the call to <c>Intercept&lt;Task&lt;T&gt;&gt;</c>.
        /// </param>
        /// <param name="interceptionResult"></param>
        /// <returns>
        /// True if the exception is handled.
        ///
        /// Otherwise, the exception is rethrown.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool HandleException<TValue>(Exception exception, ref Task<TValue> value,
            ref InterceptionResult<Task<TValue>> interceptionResult)
        {
            var interceptionResultTmp = (InterceptionResult) interceptionResult;

            return HandleException(exception, ref interceptionResultTmp);
        }
    }
}
