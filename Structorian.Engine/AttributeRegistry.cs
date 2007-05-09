using System;
using System.Collections.Generic;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    enum AttributeType { Expression, Int, String, Bool, StructRef, EnumRef };

    class AttributeRegistry
    {
        private Dictionary<Type, Dictionary<string, AttributeType>> _registry = new Dictionary<Type, Dictionary<string, AttributeType>>();

        internal AttributeRegistry()
        {
            RegisterAttribute(typeof(AlignField), "bytes", AttributeType.Int);
            RegisterAttribute(typeof(AssertField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(BitfieldField), "size", AttributeType.Expression);
            RegisterAttribute(typeof(BlobField), "len", AttributeType.Expression);
            RegisterAttribute(typeof(BlobField), "encoding", AttributeType.String);
            RegisterAttribute(typeof(CalcField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(CaseField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(CaseField), "default", AttributeType.Bool);
            RegisterAttribute(typeof(ChildField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(ChildField), "group", AttributeType.String);
            RegisterAttribute(typeof(ChildField), "struct", AttributeType.StructRef);
            RegisterAttribute(typeof(ChildField), "followchildren", AttributeType.Bool);
            RegisterAttribute(typeof(DosDateTimeField), "timefirst", AttributeType.Bool);
            RegisterAttribute(typeof(EnumField), "enum", AttributeType.EnumRef);
            RegisterAttribute(typeof(EnumField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(GlobalField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(IfField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(IntField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(ElseIfField), "expr", AttributeType.Expression);
            RegisterAttribute(typeof(IncludeField), "struct", AttributeType.StructRef);
            RegisterAttribute(typeof(IncludeField), "replace", AttributeType.Bool);
            RegisterAttribute(typeof(IntBasedField), "frombit", AttributeType.Int);
            RegisterAttribute(typeof(IntBasedField), "tobit", AttributeType.Int);
            RegisterAttribute(typeof(IntBasedField), "bit", AttributeType.Int);
            RegisterAttribute(typeof(MessageField), "text", AttributeType.String);
            RegisterAttribute(typeof(NodenameField), "name", AttributeType.Expression);
            RegisterAttribute(typeof(RepeatField), "count", AttributeType.Expression);
            RegisterAttribute(typeof(SeekField), "offset", AttributeType.Expression);
            RegisterAttribute(typeof(SetField), "enum", AttributeType.EnumRef);
            RegisterAttribute(typeof(StrField), "len", AttributeType.Expression);
            RegisterAttribute(typeof(StrField), "value", AttributeType.Expression);
            RegisterAttribute(typeof(SwitchField), "expr", AttributeType.Expression);
        }

        internal void RegisterAttribute(Type fieldType, string attrName, AttributeType type)
        {
            Dictionary<string, AttributeType> fieldsForType;
            if (!_registry.TryGetValue(fieldType, out fieldsForType))
            {
                fieldsForType = new Dictionary<string, AttributeType>();
                _registry.Add(fieldType, fieldsForType);
            }
            fieldsForType.Add(attrName, type);
        }

        internal void SetFieldAttribute(StructField field, string key, string value, TextPosition pos)
        {
            if (field == null) return;
            
            Type fieldType = field.GetType();
            
            while(true)
            {
                Dictionary<string, AttributeType> fieldsForType;
                if (_registry.TryGetValue(fieldType, out fieldsForType))
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
            
            try
            {
                field.SetAttribute(key, value);
            }
            catch(Exception)
            {
                throw new ParseException("Unknown attribute " + key, pos);
            }
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
                case AttributeType.StructRef:
                    field.StructDef.StructFile.AddReference(new StructReference(field, key, value, pos));
                    break;
                case AttributeType.EnumRef:
                    field.StructDef.StructFile.AddReference(new EnumReference(field, key, value, pos));
                    break;
            }
        }
    }
}

