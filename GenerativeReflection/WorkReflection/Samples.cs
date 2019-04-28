// ReSharper disable ConvertToAutoProperty
namespace WorkReflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Smart.Reflection;
    using Smart.Reflection.Generative;

    public class SampleDataTypeMetadata : ITypeMetadataHolder
    {
        private readonly Dictionary<ConstructorInfo, IActivator> activators = new Dictionary<ConstructorInfo, IActivator>();

        private readonly Dictionary<PropertyInfo, IAccessor> accessors = new Dictionary<PropertyInfo, IAccessor>();

        public SampleDataTypeMetadata()
        {
            var ctor0 = typeof(Data).GetConstructor(Type.EmptyTypes);
            activators[ctor0] = new Activator0(ctor0);

            var ctor1 = typeof(Data).GetConstructor(new[] { typeof(int), typeof(string) });
            activators[ctor1] = new Activator1(ctor1);

            var piIntValue = typeof(Data).GetProperty(nameof(Data.IntValue));
            accessors[piIntValue] = new IntValueAccessor(piIntValue);

            var piStringValue = typeof(Data).GetProperty(nameof(Data.StringValue));
            accessors[piStringValue] = new StringValueAccessor(piStringValue);
        }

        public IActivator FindActivator(ConstructorInfo ci)
        {
            return activators[ci];
        }

        public IAccessor FindAccessor(PropertyInfo pi)
        {
            return accessors[pi];
        }

        // Generative Activators

        private sealed class Activator0 : IActivator
        {
            private readonly ConstructorInfo source;

            public ConstructorInfo Source => source;

            public Activator0(ConstructorInfo source)
            {
                this.source = source;
            }

            public object Create(params object[] arguments)
            {
                return new Data();
            }
        }

        private sealed class Activator1 : IActivator
        {
            private readonly ConstructorInfo source;

            public ConstructorInfo Source => source;

            public Activator1(ConstructorInfo source)
            {
                this.source = source;
            }

            public object Create(params object[] arguments)
            {
                return new Data((int)arguments[0], (string)arguments[1]);
            }
        }

        // Generative Accessors

        private sealed class IntValueAccessor : IAccessor
        {
            private readonly PropertyInfo source;

            private readonly string name;

            private readonly Type type;

            public PropertyInfo Source => source;

            public string Name => name;

            public Type Type => type;

            public bool CanRead => true;

            public bool CanWrite => true;

            public IntValueAccessor(PropertyInfo source)
            {
                this.source = source;
                name = source.Name;
                type = source.PropertyType;
            }

            public object GetValue(object target)
            {
                return ((Data)target).IntValue;
            }

            public void SetValue(object target, object value)
            {
                if (value == null)
                {
                    ((Data)target).IntValue = default;
                }
                else
                {
                    ((Data)target).IntValue = (int)value;
                }
            }
        }

        private sealed class StringValueAccessor : IAccessor
        {
            private readonly PropertyInfo source;

            private readonly string name;

            private readonly Type type;

            public PropertyInfo Source => source;

            public string Name => name;

            public Type Type => type;

            public bool CanRead => true;

            public bool CanWrite => true;

            public StringValueAccessor(PropertyInfo source)
            {
                this.source = source;
                name = source.Name;
                type = source.PropertyType;
            }

            public object GetValue(object target)
            {
                return ((Data)target).StringValue;
            }

            public void SetValue(object target, object value)
            {
                ((Data)target).StringValue = (string)value;
            }
        }
    }
}
