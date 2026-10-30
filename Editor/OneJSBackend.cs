using System;
using System.IO;
using Puerts;
using UnityEditor;
using UnityEngine;

namespace OneJS.Editor {
    public class OneJSBackend {
        public static string GetCurrentBackend() {
            switch (JsEnv.DefaultBackendType) {
                case BackendType.Auto:
                case BackendType.V8:
                    return "V8";
                case BackendType.Node:
                    return "NodeJS";
                case BackendType.QuickJS:
                    return "QuickJS";
                default:
                    return "Unknown";
            }
        }

        public static void SwitchBackend(string backend) {
            if (backend == GetCurrentBackend()) {
                Debug.Log("You are already using " + backend + " backend. No need to switch.");
                return;
            }
            string typeName = backend == "NodeJS" ? "BackendNodeJS" : "Backend" + backend;
            string assembly = backend == "NodeJS" ? "nodejs" : backend.ToLowerInvariant();
            if (Type.GetType("Puerts." + typeName + ", com.tencent.puerts." + assembly) == null) {
                Debug.LogError("Install the matching PuerTS 3.0.3 " + backend + " backend package before selecting it.");
                return;
            }
            JsEnv.DefaultBackendType = backend == "NodeJS" ? BackendType.Node :
                (BackendType)Enum.Parse(typeof(BackendType), backend);
            Debug.Log("OneJS will use " + backend + " for newly created script environments.");
        }
    }
}
