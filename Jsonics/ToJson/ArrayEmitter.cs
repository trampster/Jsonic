using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Jsonics.ToJson
{
    internal class ArrayEmitter : ToJsonEmitter
    {
        readonly ListMethods _listMethods;
        readonly FieldBuilder _stringBuilderField;
        readonly TypeBuilder _typeBuilder;
        readonly ToJsonEmitters _toJsonEmitters;


        internal ArrayEmitter(ListMethods listMethods, FieldBuilder stringBuilderField, TypeBuilder typeBuilder, ToJsonEmitters toJsonEmitters)
        {
            _listMethods = listMethods;
            _stringBuilderField = stringBuilderField;
            _typeBuilder = typeBuilder;
            _toJsonEmitters = toJsonEmitters;
        }

        internal override void EmitProperty(IJsonPropertyInfo property, Action<JsonILGenerator> getValueOnStack, JsonILGenerator generator)
        {
            var propertyValueLocal = generator.DeclareLocal(property.Type);
            var endLabel = generator.DefineLabel();
            var nonNullLabel = generator.DefineLabel();

            getValueOnStack(generator);
            property.EmitGetValue(generator);
            generator.StoreLocal(propertyValueLocal);
            generator.LoadLocal(propertyValueLocal);

            //check for null
            generator.BrIfTrue(nonNullLabel);
            
            //property is null
            generator.Append($"\"{property.Name}\":null");
            generator.Branch(endLabel);

            //property is not null
            generator.Mark(nonNullLabel);
            generator.Append($"\"{property.Name}\":");
            EmitValue(property.Type, (gen, address) =>
            { 
                if(address)
                {
                    gen.LoadLocalAddress(propertyValueLocal);
                }
                else
                {
                    gen.LoadLocal(propertyValueLocal);
                }
            }, generator);

            generator.Mark(endLabel);
        }

        internal override void EmitValue(Type type, Action<JsonILGenerator, bool> getValueOnStack, JsonILGenerator generator)
        {
            var methodInfo = _listMethods.GetMethod(
                type,
                () => EmitArrayMethod(type.GetElementType(), (gen, getElementOnStack) => _toJsonEmitters.EmitValue(type.GetElementType(), getElementOnStack, gen)));
            
            generator.Pop(); //remove StringBuilder from the stack
            generator.LoadArg(typeof(object), 0, false);  //load this
            generator.LoadStaticField(_stringBuilderField);
            getValueOnStack(generator, false);
            
            generator.Call(methodInfo);            
        }

        internal override bool TypeSupported(Type type)
        {
            return type.IsArray;
        }

        internal MethodBuilder EmitArrayMethod(Type elementType, Action<JsonILGenerator, Action<JsonILGenerator, bool>> emitElement)
        {
            Type arrayType = elementType.MakeArrayType();

            var methodBuilder = _typeBuilder.DefineMethod(
                "Get" + Guid.NewGuid().ToString().Replace("-", ""),
                MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(StringBuilder),
                new Type[] { typeof(StringBuilder), arrayType});
            
            var generator = new JsonILGenerator(methodBuilder.GetILGenerator(), new StringBuilder());
            var emptyArray = generator.DefineLabel();
            var beforeLoop = generator.DefineLabel();

            generator.LoadArg(arrayType, 2, false);
            generator.LoadLength();
            generator.ConvertToInt32();
            generator.LoadConstantInt32(1);
            generator.BranchIfLargerThan(emptyArray);

            //length > 1
            generator.LoadArg(typeof(StringBuilder), 1, false);
            generator.LoadConstantInt32('[');
            generator.EmitAppend(typeof(char));
            emitElement(generator, (gen, address) => 
            {
                gen.LoadArg(arrayType, 2, false);
                gen.LoadConstantInt32(0);
                gen.LoadArrayElement(elementType, address);
            });
            generator.Pop();
            generator.Branch(beforeLoop);

            //empty array
            generator.Mark(emptyArray);
            generator.LoadArg(typeof(StringBuilder), 1, false);
            generator.Append("[]");
            generator.Return();

            //before loop            
            generator.Mark(beforeLoop);
            generator.LoadConstantInt32(1);
            var indexLocal = generator.DeclareLocal(typeof(int));
            generator.StoreLocal(indexLocal);

            var lengthCheckLabel = generator.DefineLabel();
            generator.Branch(lengthCheckLabel);

            //loop start
            var loopStart = generator.DefineLabel();
            generator.Mark(loopStart);
            generator.LoadArg(typeof(StringBuilder), 1, false);
            generator.LoadConstantInt32(',');
            generator.EmitAppend(typeof(char));
            emitElement(generator, (gen, address) => 
            {
                gen.LoadArg(arrayType, 2, false);
                gen.LoadLocal(indexLocal);
                gen.LoadArrayElement(elementType, address);
            });
            generator.Pop();
            generator.LoadLocal(indexLocal);
            generator.LoadConstantInt32(1);
            generator.Add();
            generator.StoreLocal(indexLocal);

            generator.Mark(lengthCheckLabel);
            generator.LoadLocal(indexLocal);
            generator.LoadArg(arrayType, 2, false);
            generator.LoadLength();
            generator.ConvertToInt32();
            generator.BranchIfLargerThan(loopStart);
            //end loop

            generator.LoadArg(typeof(StringBuilder), 1, false);
            generator.LoadConstantInt32(']');
            generator.EmitAppend(typeof(char));
            generator.Return();
            
            return methodBuilder;
        }
    }
}