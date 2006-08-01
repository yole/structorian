using System;
using System.Collections.Generic;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    enum AttributeType { Expression, Int, String, Bool };

    class AttributeRegistry
    {
        private Dictionary<Type, Dictionary<string, AttributeType>> myRegistry = new Dictionary<Type, Dictionary<string, AttributeType>>();

        internal AttributeRegistry()
        {
            RegisterAttribute(typeof(AlignField), "bytes", AttributeType.Int);
            RegisterAttribute(typeof(AssertField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(BitfieldField), "size", AttributeType.Expression);
            RegisterAttribute(typeof(CalcField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(CaseField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(CaseField), "default", AttributeType.Bool);
            RegisterAttribute(typeof(ChildField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "group", AttributeType.String);
            RegisterAttribute(typeof(DosDateTimeField), "timefirst", AttributeType.Bool);
            RegisterAttribute(typeof(GlobalField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(IfField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(IncludeField), "replace", AttributeType.Bool);
            RegisterAttribute(typeof(IntBasedField), "frombit", AttributeType.Int);
            RegisterAttribute(typeof(IntBasedField), "tobit", AttributeType.Int);
            RegisterAttribute(typeof(NodenameField), "name", AttributeType.Expression);
            RegisterAttribute(typeof(RepeatField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(SeekField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(StrField), "len", AttributeType.Expression);
            RegisterAttribute(typeof(SwitchField), "expr", AttributeType.Expression);
        }

        internal void RegisterAttribute(Type fieldType, string attrName, AttributeType type)
        {
            Dictionary<string, AttributeType> fieldsForType;
            if (!myRegistry.TryGetValue(fieldType, out fieldsForType))
            {
                fieldsForType = new Dictionary<string, AttributeType>();
                myRegistry.Add(fieldType, fieldsForType);
            }
            fieldsForType.Add(attrName, type);
        }

        internal void SetFieldAttribute(StructField field, string key, string value, TextPosition pos)
        {
            Type fieldType = field.GetType();
            
            while(true)
            {
                Dictionary<string, AttributeType> fieldsForType;
                if (myRegistry.TryGetValue(fieldType, out fieldsForType))
                {
                    AttributeType attrType;
                    if (fieldsForType.TryGetValue(key, out attrType))
                    {
                        SetFieldAttributeValue(field, key, attrType, value, pos);
                        return;
                    }
                }
                if (fieldType.Equals(typeof(StructField))) break;
                fieldType = fieldType.BaseType;
            }
            
            field.SetAttribute(key, value);
        }

        private void SetFieldAttributeValue(StructField field, string key, AttributeType type, string value,
            TextPosition pos)
        {
            switch(type)
            {
                case AttributeType.Expression:
                    try
                    {
                        field.SetAttributeValue(key, ExpressionParser.Parse(value));
                    }
                    catch(ParseException ex)
                    {
                        throw new ParseException(ex.Message, pos.OffsetBy(ex.Position));
                    }
                    break;
                case AttributeType.String:
                    field.SetAttributeValue(key, value);
                    break;
                case AttributeType.Int:
                    field.SetAttributeValue(key, Int32.Parse(value));
                    break;
                case AttributeType.Bool:
                    field.SetAttributeValue(key, (Int32.Parse(value) != 0));
                    break;
            }
        }
    }
}

