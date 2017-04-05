using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public class ClassUtils
    {


        public static T GetField<T>(string fieldName, object obj)
        {
            FieldInfo dynField = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynField == null) dynField = obj.GetType().GetTypeInfo().BaseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)dynField.GetValue(obj);
        }

        public static void SetField(string fieldName, object obj, object value)
        {
            FieldInfo dynField = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynField == null) dynField = obj.GetType().GetTypeInfo().BaseType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            dynField.SetValue(obj, value);
        }
        public static T GetProperty<T>(string propertyName, object obj)
        {
            PropertyInfo dynProperty = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynProperty == null) dynProperty = obj.GetType().GetTypeInfo().BaseType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)dynProperty.GetValue(obj);
        }

        public static void SetProperty(string propertyName, object obj, object value)
        {
            PropertyInfo dynProperty = obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynProperty == null) dynProperty = obj.GetType().GetTypeInfo().BaseType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            dynProperty.SetValue(obj, value);
        }

        public static T Invoke<T> (string methodName, object obj, params object[] methodParams)
        {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynMethod == null) dynMethod = obj.GetType().GetTypeInfo().BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)dynMethod.Invoke(obj, methodParams );
        }

        public static void Invoke(string methodName, object obj, params object[] methodParams)
        {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (dynMethod == null) dynMethod = obj.GetType().GetTypeInfo().BaseType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(obj, methodParams );
        }
    }
}
