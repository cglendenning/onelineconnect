#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyMobile.Internal
{
    internal static class AndroidUtil
    {
        internal static void CallJavaStaticMethod(string className, string methodName, params object[] args)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass(className))
            {
                jc.CallStatic(methodName, args);
            }
        }

        internal static T CallJavaStaticMethod<T>(string className, string methodName, params object[] args)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass(className))
            {
                return jc.CallStatic<T>(methodName, args);
            }
        }

        internal static void CallJavaInstanceMethod(AndroidJavaObject jo, string methodName, params object[] args)
        {
            jo.Call(methodName, args);
        }

        internal static T CallJavaInstanceMethod<T>(AndroidJavaObject jo, string methodName, params object[] args)
        {
            return jo.Call<T>(methodName, args);
        }
    }
}
#endif
