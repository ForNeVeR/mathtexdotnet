using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Syracuse.UnitTesting
{
    internal static class ReflectionExtensions
    {
        public static FieldTypeInfo[] GetFieldTypes(this Type type)
        {
            return (from fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic)
                    let fieldTypeConverterAttrib = fieldInfo.GetCustomAttributes(
                        typeof(TypeConverterAttribute), true).SingleOrDefault()
                        as TypeConverterAttribute
                    let fieldTypeConverter = (fieldTypeConverterAttrib == null) ? null :
                        Activator.CreateInstance(Type.GetType(
                        fieldTypeConverterAttrib.ConverterTypeName)) as TypeConverter
                    select new FieldTypeInfo()
                    {
                        FieldInfo = fieldInfo,
                        TypeConverter = fieldTypeConverter ??
                        TypeDescriptor.GetConverter(fieldInfo.FieldType)
                    }).ToArray();
        }


    }

    internal struct FieldTypeInfo
    {
        public FieldInfo FieldInfo;
        public TypeConverter TypeConverter;
    }
}
