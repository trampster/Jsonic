using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Jsonics.ToJson
{
    public class NullableEmitter<T> : ToJsonEmitter where T : struct
    {
        readonly ToJsonEmitters _toJsonEmitters;

        public NullableEmitter(ToJsonEmitters toJsonEmitters)
        {
            _toJsonEmitters = toJsonEmitters;
        }

        public override void EmitProperty(PropertyInfo property, Action<JsonILGenerator> getValueOnStack, JsonILGenerator generator)
        {
            Type type = property.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(type);
            var propertyValueLocal = generator.DeclareLocal(type);
            var endLabel = generator.DefineLabel();
            var nonNullLabel = generator.DefineLabel();

            getValueOnStack(generator);
            generator.GetProperty(property);
            generator.StoreLocal(propertyValueLocal);
            generator.LoadLocalAddress(propertyValueLocal);

            //check for null
            generator.Call(type.GetTypeInfo().GetMethod("get_HasValue", new Type[0]));
            generator.BrIfTrue(nonNullLabel);
            
            //property is null
            generator.Append($"\"{property.Name}\":null");
            generator.Branch(endLabel);

            //property is not null
            generator.Mark(nonNullLabel);

            generator.Append($"\"{property.Name}\":");

            _toJsonEmitters.EmitValue(
                underlyingType, 
                gen =>
                {
                    gen.LoadLocalAddress(propertyValueLocal);
                    gen.Call(type.GetTypeInfo().GetMethod("get_Value", new Type[0]));
                },
                generator);

            generator.Mark(endLabel);
        }

        public override void EmitValue(Type type, Action<JsonILGenerator> getValueOnStack, JsonILGenerator generator)
        {
            getValueOnStack(generator);
            var hasValueLabel = generator.DefineLabel();
            var endLabel = generator.DefineLabel();

            generator.Call(type.GetTypeInfo().GetMethod("get_HasValue", new Type[0]));
            generator.BrIfTrue(hasValueLabel);

            generator.Append("null");
            generator.Branch(endLabel);

            //has value
            generator.Mark(hasValueLabel);

            _toJsonEmitters.EmitValue(
                Nullable.GetUnderlyingType(type),
                gen =>
                {
                    getValueOnStack(generator);
                    gen.Call(type.GetTypeInfo().GetMethod("get_Value", new Type[0]));
                },
                generator);

            //end
            generator.Mark(endLabel);
        }

        public override bool TypeSupported(Type type)
        {
            return type == typeof(T?);
        }
    }
}