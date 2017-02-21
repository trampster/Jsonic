using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Jsonics
{
    public class JsonFactory
    {
        public static IJsonConverter<T> Compile<T>()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(Guid.NewGuid().ToString()),
                AssemblyBuilderAccess.Run);
            var module = assemblyBuilder.DefineDynamicModule("module1");

            var typeBuilder = module.DefineType("Person.Endurer", TypeAttributes.Public|TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(typeof(IJsonConverter<T>));

            var methodBuilder = typeBuilder.DefineMethod(
                "ToJson",
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(string),
                new Type[] { typeof(T) });

            var generator = methodBuilder.GetILGenerator();
        
            var builder = new StringBuilder();
            builder.Append("{{");
            var type = typeof(T);
            var emitProperties = new List<Action>();
            int propertyIndex = 0;

            bool isFirstProperty = true;
            foreach(var property in type.GetRuntimeProperties())
            {
                if((!property.CanRead && property.CanWrite))
                {
                    continue;
                }
                if(!isFirstProperty)
                {
                    builder.Append(",");
                }
                isFirstProperty = false;

                if(property.PropertyType == typeof(string))
                {
                    CreateStringProperty<T>(builder, emitProperties, propertyIndex, property, generator);
                }
                else if(property.PropertyType == typeof(int) || property.PropertyType == typeof(uint) ||
                   property.PropertyType == typeof(long) || property.PropertyType == typeof(ulong) ||
                   property.PropertyType == typeof(byte) || property.PropertyType == typeof(sbyte) ||
                   property.PropertyType == typeof(short) || property.PropertyType == typeof(ushort) ||
                   property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                {
                    CreateNumberProperty<T>(builder, emitProperties, propertyIndex, property, generator);
                }

                propertyIndex++;
            }
            builder.Append("}}");

            //string.Format
            generator.Emit(OpCodes.Ldstr, builder.ToString());

            foreach(var emitProperty in emitProperties)
            {
                emitProperty();
            }

            var types = new List<Type>();
            types.Add(typeof(string));
            for(int index = 0; index < propertyIndex; index++)
            {
                types.Add(typeof(object));
            }

            generator.Emit(OpCodes.Call, typeof(string).GetRuntimeMethod("Format", types.ToArray()));

            generator.Emit(OpCodes.Ret);


            var typeInfo = typeBuilder.CreateTypeInfo();
            var myType = typeInfo.AsType();

            return (IJsonConverter<T>)Activator.CreateInstance(myType);
        }

        static void CreateStringProperty<T>(StringBuilder formatBuilder, List<Action> emitProperties, int propertyIndex, PropertyInfo property, ILGenerator generator)
        {
            formatBuilder.Append($"\"{property.Name}\":\"{{{propertyIndex}}}\"");
            emitProperties.Add(() => 
            { 
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, typeof(T).GetRuntimeMethod($"get_{property.Name}", new Type[0]));
            });
        }

        static void CreateNumberProperty<T>(StringBuilder formatBuilder, List<Action> emitProperties, int propertyIndex, PropertyInfo property, ILGenerator generator)
        {
            formatBuilder.Append($"\"{property.Name}\":{{{propertyIndex}}}");
            emitProperties.Add(() => 
            { 
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Call, typeof(T).GetRuntimeMethod($"get_{property.Name}", new Type[0]));
                generator.Emit(OpCodes.Box, property.PropertyType);
            });
        }
    }
}