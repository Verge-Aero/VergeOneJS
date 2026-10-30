#if PUERTS_DISABLE_IL2CPP_OPTIMIZATION || (!PUERTS_IL2CPP_OPTIMIZATION && (UNITY_WEBGL || UNITY_IPHONE)) || !ENABLE_IL2CPP

using System;
using System.Linq;
using System.Reflection;
using Puerts;

namespace OneJS.Utils {
    public class GenericDelegateWrapper {
        JsEnv _jsEnv;
        EventInfo _eventInfo;
        ScriptObject _handler;

        Delegate _del;

        public static Delegate Wrap(JsEnv jsEnv, EventInfo eventInfo, ScriptObject handler) =>
            new GenericDelegateWrapper(jsEnv, eventInfo, handler).GetWrapped();

        /// <summary>
        /// https://nondisplayable.ca/2017/03/31/using-reflection-to-bind-lambda-to-event-handler.html
        /// </summary>
        public GenericDelegateWrapper(JsEnv jsEnv, EventInfo eventInfo, ScriptObject handler) {
            _jsEnv = jsEnv;
            _eventInfo = eventInfo;
            _handler = handler;

            var handlerType = _eventInfo.EventHandlerType;
            MethodInfo invoke = handlerType.GetMethod("Invoke");

            if (invoke.ReturnType != typeof(void)) {
                throw new ArgumentException("[GenericDelegateWrapper] Only support event delegate that return nothing.");
            }
            
            ParameterInfo[] pars = invoke.GetParameters();
            var paramTypes = pars.Select(p => p.ParameterType).ToArray();
            var methodInfo =
                typeof(GenericDelegateWrapper).GetMethod(nameof(GetAction), paramTypes.Length, Array.Empty<Type>())
                ?? throw new ArgumentException("[GenericDelegateWrapper] Only support handler with up to 4 parameters.",
                    nameof(handler));

            if (paramTypes.Length > 0) {
                methodInfo = methodInfo.MakeGenericMethod(paramTypes);
            }

            var h = (Delegate)methodInfo.Invoke(this, Array.Empty<object>());
            _del = Delegate.CreateDelegate(handlerType, h, "Invoke");
        }

        public Delegate GetWrapped() {
            return _del;
        }

        public Action GetAction() {
            return _jsEnv.Eval<Func<ScriptObject, Action>>("handler => (...args) => handler(...args)")(_handler);
        }

        public Action<A> GetAction<A>() {
            return _jsEnv.Eval<Func<ScriptObject, Action<A>>>("handler => (...args) => handler(...args)")(_handler);
        }

        public Action<A, B> GetAction<A, B>() {
            return _jsEnv.Eval<Func<ScriptObject, Action<A, B>>>("handler => (...args) => handler(...args)")(_handler);
        }

        public Action<A, B, C> GetAction<A, B, C>() {
            return _jsEnv.Eval<Func<ScriptObject, Action<A, B, C>>>("handler => (...args) => handler(...args)")(_handler);
        }

        public Action<A, B, C, D> GetAction<A, B, C, D>() {
            return _jsEnv.Eval<Func<ScriptObject, Action<A, B, C, D>>>("handler => (...args) => handler(...args)")(_handler);
        }
    }
}
#endif