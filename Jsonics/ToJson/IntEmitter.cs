using System;
using System.Reflection;

namespace Jsonics.ToJson
{
    internal class IntEmitter : ToJsonEmitter
    {
        internal override void EmitProperty(IJsonPropertyInfo property, Action<JsonILGenerator> getValueOnStack, JsonILGenerator generator)
        {
            generator.Append($"\"{property.Name}\":");

            EmitValue(
                property.Type, 
                (gen, address) =>
                {
                    getValueOnStack(gen);
                    property.EmitGetValue(gen);
                },
                generator);
        }

        internal override void EmitValue(Type type, Action<JsonILGenerator, bool> getValueOnStack, JsonILGenerator generator)
        {
            getValueOnStack(generator, false);
            generator.AppendInt();
        }

        internal override bool TypeSupported(Type type)
        {
            return type == typeof(int) || type.GetTypeInfo().IsEnum;
        }
    }
}