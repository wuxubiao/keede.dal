using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Keede.DAL.Helper.Mapper
{
    #region -- delegate

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    internal delegate object GetValueHandler(object source);

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    internal delegate object ObjectInstanceHandler();

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="value"></param>
    internal delegate void SetValueHandler(object source, object value);

    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    /// <param name="paramters"></param>
    /// <returns></returns>
    internal delegate object FastMethodHandler(object target, object[] paramters);

    #endregion -- delegate

    #region -- PropertyHandler

    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    internal class PropertyHandler
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        public PropertyHandler(PropertyInfo property)
        {
            if (property.CanWrite)
                _mSetValue = ReflectionHandlerFactory.PropertySetHandler(property);
            if (property.CanRead)
                _mGetValue = ReflectionHandlerFactory.PropertyGetHandler(property);
            _mProperty = property;
            IndexProperty = _mProperty.GetGetMethod().GetParameters().Length > 0;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IndexProperty { get; set; }

        private PropertyInfo _mProperty;

        /// <summary>
        ///
        /// </summary>
        public PropertyInfo Property
        {
            get
            {
                return _mProperty;
            }
            set
            {
                _mProperty = value;
            }
        }

        private readonly GetValueHandler _mGetValue;

        /// <summary>
        ///
        /// </summary>
        internal GetValueHandler Get
        {
            get
            {
                return _mGetValue;
            }
        }

        private readonly SetValueHandler _mSetValue;

        /// <summary>
        ///
        /// </summary>
        internal SetValueHandler Set
        {
            get
            {
                return _mSetValue;
            }
        }
    }

    #endregion -- PropertyHandler

    #region -- ReflectionHandlerFactory

    /// <summary>
    ///
    /// </summary>
    [Obsolete("This class is obsolete,don't use it in new project")]
    internal class ReflectionHandlerFactory
    {
        #region field handler

        private static readonly Dictionary<FieldInfo, GetValueHandler> _mFieldGetHandlers = new Dictionary<FieldInfo, GetValueHandler>();
        private static readonly Dictionary<FieldInfo, SetValueHandler> _mFieldSetHandlers = new Dictionary<FieldInfo, SetValueHandler>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static GetValueHandler FieldGetHandler(FieldInfo field)
        {
            GetValueHandler handler;
            if (_mFieldGetHandlers.ContainsKey(field))
            {
                handler = _mFieldGetHandlers[field];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mFieldGetHandlers.ContainsKey(field))
                    {
                        handler = _mFieldGetHandlers[field];
                    }
                    else
                    {
                        handler = CreateFieldGetHandler(field);
                        _mFieldGetHandlers.Add(field, handler);
                    }
                }
            }
            return handler;
        }

        private static GetValueHandler CreateFieldGetHandler(FieldInfo field)
        {
            DynamicMethod dm = new DynamicMethod("CreateFieldGetHandler_AutoMapper", typeof(object), new[] { typeof(object) }, field.DeclaringType);
            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            EmitBoxIfNeeded(ilGenerator, field.FieldType);
            ilGenerator.Emit(OpCodes.Ret);
            return (GetValueHandler)dm.CreateDelegate(typeof(GetValueHandler));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static SetValueHandler FieldSetHandler(FieldInfo field)
        {
            SetValueHandler handler;
            if (_mFieldSetHandlers.ContainsKey(field))
            {
                handler = _mFieldSetHandlers[field];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mFieldSetHandlers.ContainsKey(field))
                    {
                        handler = _mFieldSetHandlers[field];
                    }
                    else
                    {
                        handler = CreateFieldSetHandler(field);
                        _mFieldSetHandlers.Add(field, handler);
                    }
                }
            }
            return handler;
        }

        private static SetValueHandler CreateFieldSetHandler(FieldInfo field)
        {
            DynamicMethod dm = new DynamicMethod("CreateFieldSetHandler_AutoMapper", null, new[] { typeof(object), typeof(object) }, field.DeclaringType);
            ILGenerator ilGenerator = dm.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            EmitCastToReference(ilGenerator, field.FieldType);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);
            return (SetValueHandler)dm.CreateDelegate(typeof(SetValueHandler));
        }

        #endregion field handler

        #region Property Handler

        private static readonly Dictionary<PropertyInfo, GetValueHandler> _mPropertyGetHandlers = new Dictionary<PropertyInfo, GetValueHandler>();
        private static readonly Dictionary<PropertyInfo, SetValueHandler> _mPropertySetHandlers = new Dictionary<PropertyInfo, SetValueHandler>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static SetValueHandler PropertySetHandler(PropertyInfo property)
        {
            SetValueHandler handler;
            if (_mPropertySetHandlers.ContainsKey(property))
            {
                handler = _mPropertySetHandlers[property];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mPropertySetHandlers.ContainsKey(property))
                    {
                        handler = _mPropertySetHandlers[property];
                    }
                    else
                    {
                        handler = CreatePropertySetHandler(property);
                        _mPropertySetHandlers.Add(property, handler);
                    }
                }
            }
            return handler;
        }

        private static SetValueHandler CreatePropertySetHandler(PropertyInfo property)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("CreatePropertySetHandler_AutoMapper", null,
                new[] { typeof(object), typeof(object) }, property.DeclaringType.Module);

            
                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);

                ilGenerator.Emit(OpCodes.Ldarg_1);

                EmitCastToReference(ilGenerator, property.PropertyType);
                if (property.GetSetMethod() != null)
                {
                ilGenerator.EmitCall(OpCodes.Callvirt, property.GetSetMethod(), null);

                ilGenerator.Emit(OpCodes.Ret);
            }
            SetValueHandler setter = (SetValueHandler)dynamicMethod.CreateDelegate(typeof(SetValueHandler));

            return setter;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static GetValueHandler PropertyGetHandler(PropertyInfo property)
        {
            GetValueHandler handler;
            if (_mPropertyGetHandlers.ContainsKey(property))
            {
                handler = _mPropertyGetHandlers[property];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mPropertyGetHandlers.ContainsKey(property))
                    {
                        handler = _mPropertyGetHandlers[property];
                    }
                    else
                    {
                        handler = CreatePropertyGetHandler(property);
                        _mPropertyGetHandlers.Add(property, handler);
                    }
                }
            }
            return handler;
        }

        private static GetValueHandler CreatePropertyGetHandler(PropertyInfo property)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("CreatePropertyGetHandler_AutoMapper", typeof(object), new[] { typeof(object) }, property.DeclaringType.Module);

            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);

            ilGenerator.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);

            EmitBoxIfNeeded(ilGenerator, property.PropertyType);

            ilGenerator.Emit(OpCodes.Ret);

            GetValueHandler getter = (GetValueHandler)dynamicMethod.CreateDelegate(typeof(GetValueHandler));

            return getter;
        }

        #endregion Property Handler

        #region Method Handler

        private static readonly Dictionary<MethodInfo, FastMethodHandler> _mMethodHandlers = new Dictionary<MethodInfo, FastMethodHandler>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        internal static FastMethodHandler MethodHandler(MethodInfo method)
        {
            FastMethodHandler handler;
            if (_mMethodHandlers.ContainsKey(method))
            {
                handler = _mMethodHandlers[method];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mMethodHandlers.ContainsKey(method))
                    {
                        handler = _mMethodHandlers[method];
                    }
                    else
                    {
                        handler = CreateMethodHandler(method);
                        _mMethodHandlers.Add(method, handler);
                    }
                }
            }
            return handler;
        }

        private static FastMethodHandler CreateMethodHandler(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("CreateMethodHandler_AutoMapper", typeof(object), new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            FastMethodHandler invoder = (FastMethodHandler)dynamicMethod.CreateDelegate(typeof(FastMethodHandler));
            return invoder;
        }

        #endregion Method Handler

        #region Instance Handler

        private static readonly Dictionary<Type, ObjectInstanceHandler> _mInstanceHandlers = new Dictionary<Type, ObjectInstanceHandler>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static ObjectInstanceHandler InstanceHandler(Type type)
        {
            ObjectInstanceHandler handler;
            if (_mInstanceHandlers.ContainsKey(type))
            {
                handler = _mInstanceHandlers[type];
            }
            else
            {
                lock (typeof(ReflectionHandlerFactory))
                {
                    if (_mInstanceHandlers.ContainsKey(type))
                    {
                        handler = _mInstanceHandlers[type];
                    }
                    else
                    {
                        handler = CreateInstanceHandler(type);
                        _mInstanceHandlers.Add(type, handler);
                    }
                }
            }
            return handler;
        }

        private static ObjectInstanceHandler CreateInstanceHandler(Type type)
        {
            DynamicMethod method = new DynamicMethod("ObjectInstanceHandler_AutoMapper", type, null, type.Module);
            ILGenerator il = method.GetILGenerator();
            il.DeclareLocal(type, true);
            il.Emit(OpCodes.Newobj, type.GetConstructor(new Type[0]));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
            ObjectInstanceHandler creater = (ObjectInstanceHandler)method.CreateDelegate(typeof(ObjectInstanceHandler));
            return creater;
        }

        #endregion Instance Handler

        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }

    #endregion -- ReflectionHandlerFactory
}