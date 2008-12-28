using System;
using System.Collections.Generic;

namespace Structorian.Engine.Fields
{
    class FieldFactory
    {
        private class FieldAlias
        {
            private string _baseName;
            private List<StructParser.Attribute> _attrs;

            public FieldAlias(string baseName, List<StructParser.Attribute> attrs)
            {
                _baseName = baseName;
                _attrs = attrs;
            }

            public string BaseName
            {
                get { return _baseName; }
            }

            public List<StructParser.Attribute> Attrs
            {
                get { return _attrs; }
            }
        }
        
        private Dictionary<string, FieldAlias> _aliasRegistry = new Dictionary<string, FieldAlias>();
        
        public StructField CreateField(StructDef structDef, string name, AttributeRegistry registry)
        {
            FieldAlias alias;
            if (_aliasRegistry.TryGetValue(name, out alias))
            {
                StructField baseField = CreateField(structDef, alias.BaseName, registry);
                foreach(StructParser.Attribute attr in alias.Attrs)
                    registry.SetFieldAttribute(baseField, attr.Key, attr.Value, attr.Position);
                return baseField;
            }
            
            switch(name)
            {
                case "str": return new StrField(structDef, false, false);
                case "cstr": return new StrField(structDef, false, true);
                case "wstr": return new StrField(structDef, true, false);
                case "child": return new ChildField(structDef, false);
                case "sibling": return new ChildField(structDef, true);
                case "seek": return new SeekField(structDef, false);
                case "skip": return new SeekField(structDef, true);
                case "rewind": return new RewindField(structDef);
                case "repeat": return new RepeatField(structDef);
                case "if": return new IfField(structDef);
                case "elif": return new ElseIfField(structDef);
                case "else": return new ElseField(structDef);
                case "include": return new IncludeField(structDef);
                case "assert": return new AssertField(structDef);
                case "bitfield": return new BitfieldField(structDef);
                case "nodename": return new NodenameField(structDef);
                case "switch": return new SwitchField(structDef);
                case "case": return new CaseField(structDef, false);
                case "default": return new CaseField(structDef, true);
                case "unixtime": return new UnixTimeField(structDef);
                case "dosdatetime": return new DosDateTimeField(structDef);
                case "global": return new GlobalField(structDef);
                case "local": return new CalcField(structDef, true);
                case "calc": return new CalcField(structDef, false);
                case "align": return new AlignField(structDef);
                case "blob": return new BlobField(structDef);
                case "image": return new ImageField(structDef);
                case "float": return new FloatField(structDef);
                case "break": return new BreakField(structDef);
                case "i": return new IntField(structDef, 0, false, false);
                case "u": return new IntField(structDef, 0, true, false);
                case "x": return new IntField(structDef, 0, true, true);
                case "enum": return new EnumField(structDef, 0);
                case "message": return new MessageField(structDef, false);
                case "error": return new MessageField(structDef, true);
            }
            
            int size;
            if (name.EndsWith("8"))
            {
                size = 1;
                name = name.Substring(0, name.Length - 1);
            }
            else if (name.EndsWith("16") || name.EndsWith("32") || name.EndsWith("64"))
            {
                size = name.EndsWith("16") ? 2 : name.EndsWith("32") ? 4 : 8;
                name = name.Substring(0, name.Length - 2);
            }
            else
                throw new Exception("Unknown field type " + name);
            
            switch(name)
            {
                case "i":    return new IntField(structDef, size, false, false);
                case "u":    return new IntField(structDef, size, true, false);
                case "x":    return new IntField(structDef, size, true, true);
                case "enum": return new EnumField(structDef, size);
                case "set":  return new SetField(structDef, size);
                case "bits": return new BitsField(structDef, size);
            }
            throw new Exception("Unknown field type " + name);
        }

        public void RegisterAlias(string aliasName, string name, List<StructParser.Attribute> attrs)
        {
            _aliasRegistry [aliasName] = new FieldAlias(name, attrs);
        }
    }
}
