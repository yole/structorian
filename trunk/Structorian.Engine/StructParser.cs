using System;
using System.Collections.Generic;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    public class StructParser
    {
        private class Attribute
        {
            string _key;
            string _value;
            TextPosition _position;

            public Attribute(string name, string value, TextPosition position)
            {
                _key = name;
                _value = value;
                _position = position;
            }

            public string Key
            {
                get { return _key; }
            }

            public string Value
            {
                get { return _value; }
            }

            public TextPosition Position
            {
                get { return _position; }
            }
        }
        
        private StructFile _curStructFile;
        private FieldFactory _fieldFactory = new FieldFactory();
        private AttributeRegistry _attributeRegistry = new AttributeRegistry();

        public StructFile LoadStructs(string strsText)
        {
            return LoadStructs(null, strsText);
        }
        
        public StructFile LoadStructs(string fileName, string strsText)
        {
            StructFile result = new StructFile();
            _curStructFile = result;
            StructLexer lexer = new StructLexer(fileName, strsText);
            while(!lexer.EndOfStream())
            {
                List<Attribute> attrs = new List<Attribute>();
                LoadAttributes(lexer, attrs);
                string token = lexer.GetNextToken(StructTokenType.String);
                if (token == "struct")
                    LoadStruct(lexer, attrs);
                else if (token == "enum")
                    LoadEnum(lexer, attrs);
                else
                    throw new Exception("Unexpected top-level item " + token);
            }
            return result;
        }

        private void LoadStruct(StructLexer lexer, List<Attribute> attrs)
        {
            string name = lexer.GetNextToken(StructTokenType.String);
            if (_curStructFile.GetStructByName(name) != null)
                throw new Exception("Duplicate structure name '" + name + "'");

            StructDef structDef = new StructDef(_curStructFile, name);
            foreach (Attribute attr in attrs)
                structDef.SetAttribute(attr.Key, attr.Value);
            
            LoadFieldGroup(lexer, structDef, null);
            _curStructFile.Add(structDef);
        }

        private void LoadFieldGroup(StructLexer lexer, StructDef structDef, StructField parentField)
        {
            lexer.GetNextToken(StructTokenType.OpenCurly);
            StructField linkToField = null;
            while(lexer.PeekNextToken() != StructTokenType.CloseCurly)
            {
                StructField field = LoadField(lexer, structDef, parentField);

                bool isLinked = false;
                if (linkToField != null)
                    isLinked = linkToField.CanLinkField(field);

                if (isLinked)
                    linkToField.LinkField(field);
                else
                    linkToField = field;
            }
            lexer.GetNextToken(StructTokenType.CloseCurly);
        }

        private StructField LoadField(StructLexer lexer, StructDef structDef, StructField parentField)
        {
            List<Attribute> attrs = new List<Attribute>();
            LoadAttributes(lexer, attrs);
            string fieldType = lexer.GetNextToken(StructTokenType.String);
            StructField field = _fieldFactory.CreateField(structDef, fieldType);
            LoadAttributes(lexer, attrs);
            if (lexer.PeekNextToken() != StructTokenType.Semicolon && lexer.PeekNextToken() != StructTokenType.OpenCurly)
            {
                TextPosition pos = lexer.CurrentPosition;
                string tag = lexer.GetNextToken(StructTokenType.String);
                LoadAttributes(lexer, attrs);
                _attributeRegistry.SetFieldAttribute(field, field.DefaultAttribute, tag, pos);
            }

            foreach (Attribute attr in attrs)
                _attributeRegistry.SetFieldAttribute(field, attr.Key, attr.Value, attr.Position);
            
            if (lexer.PeekNextToken() == StructTokenType.OpenCurly)
                LoadFieldGroup(lexer, structDef, field);
            else
                lexer.GetNextToken(StructTokenType.Semicolon);

            if (parentField == null)
                structDef.AddField(field);
            else
                parentField.AddChildField(field);
            return field;
        }

        private void LoadAttributes(StructLexer lexer, List<Attribute> attrs)
        {
            if (lexer.PeekNextToken() == StructTokenType.OpenSquare)
            {
                lexer.GetNextToken(StructTokenType.OpenSquare);
                while(true)
                {
                    TextPosition pos = lexer.CurrentPosition;
                    string attrName = lexer.GetNextToken(StructTokenType.String);
                    string attrValue;
                    if (lexer.CheckNextToken(StructTokenType.Equals))
                    {
                        attrValue = lexer.GetAttributeValue(out pos);
                    }
                    else
                    {
                        attrValue = "1";
                    }
                    attrs.Add(new Attribute(attrName, attrValue, pos));
                    if (lexer.CheckNextToken(StructTokenType.CloseSquare)) break;
                    if (!lexer.CheckNextToken(StructTokenType.Comma))
                    {
                        throw new Exception("Unexpected token");
                    }
                }
            }
        }
        
        private void LoadEnum(StructLexer lexer, List<Attribute> attrs)
        {
            string name = lexer.GetNextToken(StructTokenType.String);
            EnumDef enumDef = new EnumDef(_curStructFile, name);
            LoadAttributes(lexer, attrs);

            foreach (Attribute attr in attrs)
                enumDef.SetAttribute(attr.Key, attr.Value);
            
            lexer.GetNextToken(StructTokenType.OpenCurly);
            int lastValue = -1;
            while(!lexer.CheckNextToken(StructTokenType.CloseCurly))
            {
                string constName = lexer.GetNextToken(StructTokenType.String);
                if (lexer.CheckNextToken(StructTokenType.Equals))
                {
                    string constValue = lexer.GetNextToken(StructTokenType.String);
                    lastValue = ExpressionParser.Parse(constValue).EvaluateInt(null);
                }
                else
                    lastValue++;
                enumDef.AddValue(constName, lastValue);                
                
                if (!lexer.CheckNextToken(StructTokenType.Comma))
                {
                    if (lexer.PeekNextToken() != StructTokenType.CloseCurly)
                        throw new Exception("Unexpected token in enum: " + lexer.PeekNextToken());
                }
            }
            _curStructFile.Add(enumDef);
        }
    }
}
