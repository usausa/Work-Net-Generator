namespace WorkMerge
{
    using Mono.Cecil;

    class Program
    {
        private const string Target =
            "..\\..\\..\\..\\WorkMerge.Target\\bin\\Release\\netstandard2.0\\WorkMerge.Target.dll";
        private const string Option =
            "..\\..\\..\\..\\WorkMerge.Target.Option\\bin\\Release\\netstandard2.0\\WorkMerge.Target.Option.dll";

        private const string Output =
            "..\\..\\..\\..\\WorkMerge.Target\\bin\\Release\\netstandard2.0\\WorkMerge.Target2.dll";

        static void Main()
        {
            var target = AssemblyDefinition.ReadAssembly(Target, new ReaderParameters(ReadingMode.Immediate));
            var option = AssemblyDefinition.ReadAssembly(Option);

            foreach (var typeDefinition in option.MainModule.Types)
            {
                Import(target.MainModule, typeDefinition);
            }

            target.Write(Output);
        }

        static void Import(ModuleDefinition module, TypeDefinition type)
        {
            CreateType(module, type);

            // TODO
        }

        static TypeDefinition CreateType(ModuleDefinition module, TypeDefinition type)
        {
            var newType = new TypeDefinition(type.Namespace, type.Name, type.Attributes);

            newType.BaseType = type.BaseType;   // IF相手なら不要

            foreach (var typeReference in type.Interfaces)
            {
                newType.Interfaces.Add(typeReference);
            }

            //foreach (var typeReference in type.CustomAttributes)
            //{
            //    // TODO Clone
            //    //newType.CustomAttributes.Add(typeReference);
            //}

            // TODO
            module.Types.Add(newType);

            return newType;
        }

        static void CloneField(TypeDefinition newType, FieldDefinition field)
        {

        }
    }
}
