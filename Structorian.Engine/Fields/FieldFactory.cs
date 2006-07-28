using System;

namespace Structorian.Engine.Fields
{
    class FieldFactory
    {
        public StructField CreateField(StructDef structDef, string name)
        {
            switch(name)
            {
                case "str": return new StrField(structDef);
                case "child": return new ChildField(structDef, false);
                case "sibling": return new ChildField(structDef, true);
                case "seek": return new SeekField(structDef, false);
                case "skip": return new SeekField(structDef, true);
                case "rewind": return new RewindField(structDef);
                case "repeat": return new RepeatField(structDef);
                case "if": return new IfField(structDef);
                case "include": return new IncludeField(structDef);
                case "assert": return new AssertField(structDef);
                case "bitfield": return new BitfieldField(structDef);
                case "nodename": return new NodenameField(structDef);
                case "switch": return new SwitchField(structDef);
                case "case": return new CaseField(structDef, false);
                case "default": return new CaseField(structDef, true);
                case "unixtime": return new UnixTimeField(structDef);
                case "global": return new GlobalField(structDef);
                case "local": return new CalcField(structDef, true);
                case "calc": return new CalcField(structDef, false);
            }
            
            int size = 0;
            if (name.EndsWith("8"))
            {
                size = 1;
                name = name.Substring(0, name.Length - 1);
            }
            else if (name.EndsWith("16") || name.EndsWith("32"))
            {
                size = name.EndsWith("16") ? 2 : 4;
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
            }
            throw new Exception("Unknown field type " + name);
        }
    }
}
