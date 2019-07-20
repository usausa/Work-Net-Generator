namespace WorkMerge
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;

    using System;
    using System.Linq;

    public class TypeImporter
    {
        readonly ModuleDefinition target;

        //readonly ReflectionHelper reflectionHelper;

        public TypeImporter(ModuleDefinition target)
        {
            this.target = target;
            //reflectionHelper = new ReflectionHelper(this);
        }

        public TypeDefinition Clone(TypeDefinition type)
        {
            var types = target.Types;
            var nt = CreateType(type, types, null);

            foreach (var field in type.Fields)
            {
                CloneTo(field, nt);
            }

            foreach (var method in type.Methods)
            {
                CloneTo(method, nt);
            }

            return nt;
        }

        TypeDefinition CreateType(TypeDefinition type, Collection<TypeDefinition> col, string rename)
        {
            var nt = new TypeDefinition(type.Namespace, rename ?? type.Name, type.Attributes);
            col.Add(nt);

            CopyGenericParameters(type.GenericParameters, nt.GenericParameters, nt);
            if (type.BaseType != null)
            {
                nt.BaseType = Import(type.BaseType, nt);
            }

            if (type.HasLayoutInfo)
            {
                nt.ClassSize = type.ClassSize;
                nt.PackingSize = type.PackingSize;
            }

            CopyTypeReferences(type.Interfaces, nt.Interfaces, nt);
            CopyCustomAttributes(type.CustomAttributes, nt.CustomAttributes, nt);
            return nt;
        }

        void CloneTo(FieldDefinition field, TypeDefinition nt)
        {
            var nf = new FieldDefinition(field.Name, field.Attributes, Import(field.FieldType, nt));
            nt.Fields.Add(nf);

            if (field.HasConstant)
            {
                nf.Constant = field.Constant;
            }

            if (field.HasMarshalInfo)
            {
                nf.MarshalInfo = field.MarshalInfo;
            }

            if (field.InitialValue != null && field.InitialValue.Length > 0)
            {
                nf.InitialValue = field.InitialValue;
            }

            if (field.HasLayoutInfo)
            {
                nf.Offset = field.Offset;
            }

            CopyCustomAttributes(field.CustomAttributes, nf.CustomAttributes, nt);
        }

        /// <summary>
        /// Clones a parameter into a newly created method
        /// </summary>
        void CloneTo(ParameterDefinition param, MethodDefinition context, Collection<ParameterDefinition> col)
        {
            var pd = new ParameterDefinition(param.Name, param.Attributes, Import(param.ParameterType, context));
            if (param.HasConstant)
            {
                pd.Constant = param.Constant;
            }
            if (param.HasMarshalInfo)
            {
                pd.MarshalInfo = param.MarshalInfo;
            }
            if (param.HasCustomAttributes)
            {
                CopyCustomAttributes(param.CustomAttributes, pd.CustomAttributes, context);
            }
            col.Add(pd);
        }

        void CloneTo(MethodDefinition method, TypeDefinition type)
        {
            // use void placeholder as we'll do the return type import later on (after generic parameters)
            var nm = new MethodDefinition(method.Name, method.Attributes, target.TypeSystem.Void)
            {
                ImplAttributes = method.ImplAttributes
            };

            type.Methods.Add(nm);

            CopyGenericParameters(method.GenericParameters, nm.GenericParameters, nm);

            foreach (var param in method.Parameters)
            {
                CloneTo(param, nm, nm.Parameters);
            }

            foreach (var ov in method.Overrides)
            {
                nm.Overrides.Add(Import(ov, nm));
            }

            CopyCustomAttributes(method.CustomAttributes, nm.CustomAttributes, nm);

            nm.ReturnType = Import(method.ReturnType, nm);
            nm.MethodReturnType.Attributes = method.MethodReturnType.Attributes;
            if (method.MethodReturnType.HasConstant)
            {
                nm.MethodReturnType.Constant = method.MethodReturnType.Constant;
            }
            if (method.MethodReturnType.HasMarshalInfo)
            {
                nm.MethodReturnType.MarshalInfo = method.MethodReturnType.MarshalInfo;
            }
            if (method.MethodReturnType.HasCustomAttributes)
            {
                CopyCustomAttributes(method.MethodReturnType.CustomAttributes, nm.MethodReturnType.CustomAttributes, nm);
            }

            if (method.HasBody)
            {
                CloneTo(method.Body, nm);
            }
            method.Body = null; // frees memory

            nm.IsAddOn = method.IsAddOn;
            nm.IsRemoveOn = method.IsRemoveOn;
            nm.IsGetter = method.IsGetter;
            nm.IsSetter = method.IsSetter;
            nm.CallingConvention = method.CallingConvention;
        }

        private CustomAttributeNamedArgument Copy(CustomAttributeNamedArgument namedArg, IGenericParameterProvider context)
        {
            return new CustomAttributeNamedArgument(namedArg.Name, Copy(namedArg.Argument, context));
        }

        private CustomAttributeArgument Copy(CustomAttributeArgument arg, IGenericParameterProvider context)
        {
            return new CustomAttributeArgument(Import(arg.Type, context), ImportCustomAttributeValue(arg.Value, context));
        }

        private object ImportCustomAttributeValue(object obj, IGenericParameterProvider context)
        {
            if (obj is TypeReference reference)
                return Import(reference, context);
            if (obj is CustomAttributeArgument argument)
                return Copy(argument, context);
            if (obj is CustomAttributeArgument[] attributeArguments)
                return attributeArguments.Select(a => Copy(a, context)).ToArray();
            return obj;
        }

        void CloneTo(MethodBody body, MethodDefinition parent)
        {
            var nb = new MethodBody(parent);
            parent.Body = nb;

            nb.MaxStackSize = body.MaxStackSize;
            nb.InitLocals = body.InitLocals;
            nb.LocalVarToken = body.LocalVarToken;

            foreach (var var in body.Variables)
            {
                nb.Variables.Add(new VariableDefinition(Import(var.VariableType, parent)));
                //nb.Variables.Add(new VariableDefinition(var.Name,
                //    Import(var.VariableType, parent)));
            }

            foreach (var instr in body.Instructions)
            {
                Instruction ni;

                if (instr.OpCode.Code == Code.Calli)
                {
                    var callSIte = (CallSite)instr.Operand;
                    var ncs = new CallSite(Import(callSIte.ReturnType, parent))
                    {
                        HasThis = callSIte.HasThis,
                        ExplicitThis = callSIte.ExplicitThis,
                        CallingConvention = callSIte.CallingConvention
                    };
                    foreach (var param in callSIte.Parameters)
                    {
                        CloneTo(param, parent, ncs.Parameters);
                    }
                    ni = Instruction.Create(instr.OpCode, ncs);
                }
                else
                {
                    switch (instr.OpCode.OperandType)
                    {
                        case OperandType.InlineArg:
                        case OperandType.ShortInlineArg:
                            if (instr.Operand == body.ThisParameter)
                            {
                                ni = Instruction.Create(instr.OpCode, nb.ThisParameter);
                            }
                            else
                            {
                                var param = body.Method.Parameters.IndexOf((ParameterDefinition)instr.Operand);
                                ni = Instruction.Create(instr.OpCode, parent.Parameters[param]);
                            }
                            break;
                        case OperandType.InlineVar:
                        case OperandType.ShortInlineVar:
                            var var = body.Variables.IndexOf((VariableDefinition)instr.Operand);
                            ni = Instruction.Create(instr.OpCode, nb.Variables[var]);
                            break;
                        case OperandType.InlineField:
                            ni = Instruction.Create(instr.OpCode, Import((FieldReference)instr.Operand, parent));
                            break;
                        case OperandType.InlineMethod:
                            ni = Instruction.Create(instr.OpCode, Import((MethodReference)instr.Operand, parent));
                            break;
                        case OperandType.InlineType:
                            ni = Instruction.Create(instr.OpCode, Import((TypeReference)instr.Operand, parent));
                            break;
                        case OperandType.InlineTok:
                            if (instr.Operand is TypeReference)
                            {
                                ni = Instruction.Create(instr.OpCode, Import((TypeReference)instr.Operand, parent));
                            }
                            else if (instr.Operand is FieldReference)
                            {
                                ni = Instruction.Create(instr.OpCode, Import((FieldReference)instr.Operand, parent));
                            }
                            else if (instr.Operand is MethodReference)
                            {
                                ni = Instruction.Create(instr.OpCode, Import((MethodReference)instr.Operand, parent));
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                            break;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.InlineBrTarget:
                            ni = Instruction.Create(instr.OpCode, (Instruction)instr.Operand); // TODO review
                            break;
                        case OperandType.InlineSwitch:
                            ni = Instruction.Create(instr.OpCode, (Instruction[])instr.Operand); // TODO review
                            break;
                        case OperandType.InlineR:
                            ni = Instruction.Create(instr.OpCode, (double)instr.Operand);
                            break;
                        case OperandType.ShortInlineR:
                            ni = Instruction.Create(instr.OpCode, (float)instr.Operand);
                            break;
                        case OperandType.InlineNone:
                            ni = Instruction.Create(instr.OpCode);
                            break;
                        case OperandType.InlineString:
                            ni = Instruction.Create(instr.OpCode, (string)instr.Operand);
                            break;
                        case OperandType.ShortInlineI:
                            ni = instr.OpCode == OpCodes.Ldc_I4_S
                                ? Instruction.Create(instr.OpCode, (sbyte)instr.Operand)
                                : Instruction.Create(instr.OpCode, (byte)instr.Operand);
                            break;
                        case OperandType.InlineI8:
                            ni = Instruction.Create(instr.OpCode, (long)instr.Operand);
                            break;
                        case OperandType.InlineI:
                            ni = Instruction.Create(instr.OpCode, (int)instr.Operand);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                // TODO
                //ni.SequencePoint = instr.SequencePoint;
                nb.Instructions.Add(ni);
            }

            for (var i = 0; i < body.Instructions.Count; i++)
            {
                var instr = nb.Instructions[i];
                if (instr.OpCode.OperandType != OperandType.ShortInlineBrTarget &&
                     instr.OpCode.OperandType != OperandType.InlineBrTarget)
                {
                    continue;
                }

                instr.Operand = GetInstruction(body, nb, (Instruction)body.Instructions[i].Operand);
            }

            foreach (var eh in body.ExceptionHandlers)
            {
                var neh = new ExceptionHandler(eh.HandlerType)
                {
                    TryStart = GetInstruction(body, nb, eh.TryStart),
                    TryEnd = GetInstruction(body, nb, eh.TryEnd),
                    HandlerStart = GetInstruction(body, nb, eh.HandlerStart),
                    HandlerEnd = GetInstruction(body, nb, eh.HandlerEnd)
                };

                switch (eh.HandlerType)
                {
                    case ExceptionHandlerType.Catch:
                        neh.CatchType = Import(eh.CatchType, parent);
                        break;
                    case ExceptionHandlerType.Filter:
                        neh.FilterStart = GetInstruction(body, nb, eh.FilterStart);
                        break;
                }

                nb.ExceptionHandlers.Add(neh);
            }
        }

        private FieldReference Import(FieldReference reference, IGenericParameterProvider context)
        {
            //var importReference = platformFixer.FixPlatformVersion(reference);
            //return target.Import(importReference, context);
            return target.Import(reference, context);
        }

        internal static Instruction GetInstruction(MethodBody oldBody, MethodBody newBody, Instruction i)
        {
            var pos = oldBody.Instructions.IndexOf(i);
            if (pos > -1 && pos < newBody.Instructions.Count)
                return newBody.Instructions[pos];

            return null /*newBody.Instructions.Outside*/;
        }

        void CopyGenericParameters(Collection<GenericParameter> input, Collection<GenericParameter> output, IGenericParameterProvider nt)
        {
            foreach (var gp in input)
            {
                var ngp = new GenericParameter(gp.Name, nt)
                {
                    Attributes = gp.Attributes
                };

                output.Add(ngp);
            }
            // delay copy to ensure all generics parameters are already present
            Copy(input, output, (gp, ngp) => CopyTypeReferences(gp.Constraints, ngp.Constraints, nt));
            Copy(input, output, (gp, ngp) => CopyCustomAttributes(gp.CustomAttributes, ngp.CustomAttributes, nt));
        }

        static void Copy<T>(Collection<T> input, Collection<T> output, Action<T, T> action)
        {
            for (var i = 0; i < input.Count; i++)
            {
                action.Invoke(input[i], output[i]);
            }
        }

        void CopyTypeReferences(Collection<TypeReference> input, Collection<TypeReference> output, IGenericParameterProvider context)
        {
            foreach (var ta in input)
            {
                output.Add(Import(ta, context));
            }
        }

        // TODO
        void CopyTypeReferences(Collection<InterfaceImplementation> input, Collection<InterfaceImplementation> output, IGenericParameterProvider context)
        {
            foreach (var ta in input)
            {
                target.Import(ta.InterfaceType);
                output.Add(ta);
            }
        }

        void CopyCustomAttributes(Collection<CustomAttribute> input, Collection<CustomAttribute> output, IGenericParameterProvider context)
        {
            foreach (var ca in input)
            {
                output.Add(Copy(ca, context));
            }
        }

        private CustomAttribute Copy(CustomAttribute ca, IGenericParameterProvider context)
        {
            var newCa = new CustomAttribute(Import(ca.Constructor));
            foreach (var arg in ca.ConstructorArguments)
                newCa.ConstructorArguments.Add(Copy(arg, context));
            foreach (var arg in ca.Fields)
                newCa.Fields.Add(Copy(arg, context));
            foreach (var arg in ca.Properties)
                newCa.Properties.Add(Copy(arg, context));
            return newCa;
        }

        // TODO Changed ?

        TypeReference Import(TypeReference reference, IGenericParameterProvider context)
        {
            return context == null ? target.Import(reference) : target.Import(reference, context);
        }

        private MethodReference Import(MethodReference reference, IGenericParameterProvider context)
        {
            return target.Import(reference, context);
        }

        private MethodReference Import(MethodReference reference)
        {
            return target.Import(reference);
        }
    }
}
