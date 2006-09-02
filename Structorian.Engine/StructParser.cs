using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    public class StructParser
    {
        internal class Attribute
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
        private List<ParseException> _errors = new List<ParseException>();

        public ReadOnlyCollection<ParseException> Errors
        {
            get { return _errors.AsReadOnly(); }
        }
        
        public StructFile LoadStructs(string fileName, string strsText)
        {
            StructSourceContext context = new StructSourceContext();
            context.AddSourceText(fileName, strsText);
            return LoadStructs(fileName, context);
        }

        public StructFile LoadStructs(string strsText)
        {
            return LoadStructs("", strsText);
        }

        public StructFile LoadStructs(string fileName, StructSourceContext context)
        {
            StructFile result = new StructFile();
            _curStructFile = result;
            LoadStructFile(fileName, context);
            foreach(ReferenceBase reference in result.References)
            {
                try
                {
                    reference.Resolve();
                }
                catch(ParseException ex)
                {
                    _errors.Add(ex);
                }
            }
            if (_errors.Count == 0)
                return result;
            return null;
        }

        private void LoadStructFile(string fileName, StructSourceContext context)
        {
            StructLexer lexer = new StructLexer(fileName, context.GetSourceText(fileName));
            while(!lexer.EndOfStream())
            {
                List<Attribute> attrs = new List<Attribute>();
                LoadAttributes(lexer, attrs);
                string token = lexer.GetNextToken(StructTokenType.String);
                if (token == "struct")
                    LoadStruct(lexer, attrs);
                else if (token == "enum")
                    LoadEnum(lexer, attrs);
                else if (token == "alias")
                    LoadAlias(lexer, attrs);
                else if (token == "include")
                {
                    string includeName = lexer.GetNextToken(StructTokenType.String);
                    lexer.GetNextToken(StructTokenType.Semicolon);
                    LoadStructFile(includeName, context);
                }
                else
                    throw new Exception("Unexpected top-level item " + token);
            }
        }

        private void LoadStruct(StructLexer lexer, List<Attribute> attrs)
        {
            string name = lexer.GetNextToken(StructTokenType.String);
            if (_curStructFile.GetStructByName(name) != null)
                throw new Exception("Duplicate structure name '" + name + "'");
            
            LoadAttributes(lexer, attrs);

            StructDef structDef = new StructDef(_curStructFile, name);
            foreach (Attribute attr in attrs)
            {
                try
                {
                    structDef.SetAttribute(attr.Key, attr.Value, attr.Position);
                }
                catch(ParseException ex)
                {
                    _errors.Add(ex);
                }
            }
            
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

                if (field != null)
                {
                    bool isLinked = false;
                    if (linkToField != null)
                        isLinked = linkToField.CanLinkField(field);

                    if (isLinked)
                        linkToField.LinkField(field);
                    else
                        linkToField = field;

                    field.Validate();
                }
            }
            lexer.GetNextToken(StructTokenType.CloseCurly);
        }

        private StructField LoadField(StructLexer lexer, StructDef structDef, StructField parentField)
        {
            List<Attribute> attrs = new List<Attribute>();
            LoadAttributes(lexer, attrs);
            TextPosition fieldPosition = lexer.CurrentPosition;
            string fieldType = lexer.GetNextToken(StructTokenType.String);
            StructField field = null;
            try
            {
                field = _fieldFactory.CreateField(structDef, fieldType, _attributeRegistry);
                field.Position = fieldPosition;
            }
            catch(Exception ex)
            {
                _errors.Add(new ParseException(ex.Message, fieldPosition));
            }
            LoadAttributes(lexer, attrs);
            if (lexer.PeekNextToken() != StructTokenType.Semicolon && lexer.PeekNextToken() != StructTokenType.OpenCurly)
            {
                TextPosition pos = lexer.CurrentPosition;
                string tag = lexer.GetNextToken(StructTokenType.String);
                LoadAttributes(lexer, attrs);
                if (field != null)
                    _attributeRegistry.SetFieldAttribute(field, field.DefaultAttribute, tag, pos);
            }

            foreach (Attribute attr in attrs)
                _attributeRegistry.SetFieldAttribute(field, attr.Key, attr.Value, attr.Position);

            if (lexer.PeekNextToken() == StructTokenType.OpenCurly)
                LoadFieldGroup(lexer, structDef, field);
            else
                lexer.GetNextToken(StructTokenType.Semicolon);
            
            if (field != null)
            {
                field.EndPosition = lexer.LastTokenEndPosition;
                if (parentField == null)
                    structDef.AddField(field);
                else
                    parentField.AddChildField(field);
            }
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

        private void LoadAlias(StructLexer lexer, List<Attribute> attrs)
        {
            string baseName = lexer.GetNextToken(StructTokenType.String);
            LoadAttributes(lexer, attrs);
            string aliasName = lexer.GetNextToken(StructTokenType.String);
            LoadAttributes(lexer, attrs);
            lexer.GetNextToken(StructTokenType.Semicolon);
            
            _fieldFactory.RegisterAlias(aliasName, baseName, attrs);
        }
    }
}
