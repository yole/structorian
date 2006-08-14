using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Structorian.Engine.Fields;

namespace Structorian.Engine.Tests
{
    [TestFixture]
    public class StructParserTest
    {
        [Test] public void ParseSingleEmptyStruct()
        {
            StructParser parser = new StructParser();
            StructFile structFile = parser.LoadStructs("struct BITMAPINFOHEADER { }");
            Assert.AreEqual(1, structFile.Structs.Count);
            StructDef structDef = structFile.Structs[0];
            Assert.AreEqual("BITMAPINFOHEADER", structDef.Name);
        }
        
        [Test] public void ParseSingleFieldStruct()
        {
            StructParser parser = new StructParser();
            StructFile structFile = parser.LoadStructs("struct BITMAPINFOHEADER { u32 biSize; }");
            StructDef structDef = structFile.Structs[0];
            Assert.AreEqual(1, structDef.Fields.Count);
            Assert.AreEqual("biSize", structDef.Fields[0].Tag);
        }
        
        [Test] public void ParseFieldWithAttributes()
        {
            StructParser parser = new StructParser();
            StructFile structFile = parser.LoadStructs("struct BITMAPFILEHEADER { str [len=2] bfType; }");
            StructDef structDef = structFile.Structs[0];
            Assert.AreEqual(1, structDef.Fields.Count);
            Assert.IsInstanceOfType(typeof(StrField), structDef.Fields [0]);
            StrField field = (StrField) structDef.Fields[0];
            Assert.AreEqual("bfType", field.Tag);
            Assert.AreEqual("2", field.GetExpressionAttribute("len").ToString());
        }
        
        [Test, ExpectedException(typeof(Exception))] public void DuplicateStructName()
        {
            new StructParser().LoadStructs("struct A { } struct A { }");
        }
        
        [Test] public void StructWithAttributes()
        {
            StructFile structFile = new StructParser().LoadStructs("[filemask=\"*.bmp\"] struct BITMAPFILEHEADER { }");
            StructDef structDef = structFile.Structs[0];
            Assert.AreEqual("*.bmp", structDef.FileMask);
        }
        
        [Test] public void FieldGroup()
        {
            StructFile structFile = new StructParser().LoadStructs("struct a { repeat(8) { u8; } }");
            StructDef structDef = structFile.Structs[0];
            Assert.AreEqual(1, structDef.Fields[0].ChildFields.Count);
        }
        
        [Test, ExpectedException(typeof(Exception))] 
        public void NoChildrenOnU8()
        {
            new StructParser().LoadStructs("struct a { u8 a { u8; } }");
        }
        
        [Test] public void Enum()
        {
            StructFile structFile = new StructParser().LoadStructs("enum a { b, c }");
            EnumDef enumDef = structFile.GetEnumByName("a");
            Assert.IsNotNull(enumDef);
            Assert.AreEqual("b", enumDef.ValueToString(0));
            Assert.AreEqual("c", enumDef.ValueToString(1));
        }
        
        [Test] public void EnumWithValue()
        {
            StructFile structFile = new StructParser().LoadStructs("enum a { b=5, c=8 }");
            EnumDef enumDef = structFile.GetEnumByName("a");
            Assert.AreEqual("b", enumDef.ValueToString(5));
            Assert.AreEqual("c", enumDef.ValueToString(8));
        }
        
        [Test] public void ErrorInExpression()
        {
            ParseException parseException = null;
            try
            {
                new StructParser().LoadStructs("struct A\n{  \n  calc x [value=)];\n}");
            }
            catch(ParseException ex)
            {
                parseException = ex;
            }
            Assert.IsNotNull(parseException);
            Assert.AreEqual(3, parseException.Position.Line);
            Assert.AreEqual(16, parseException.Position.Col);
        }
        
        [Test] public void Alias()
        {
            StructFile structFile = new StructParser().LoadStructs("alias str [len=8] resref; struct A { resref r; }");
            StructField field = structFile.Structs [0].Fields [0];
            Assert.IsInstanceOfType(typeof(StrField), field);
            Assert.AreEqual(8, field.GetExpressionAttribute("len").EvaluateInt(null));
        }
        
        [Test] public void BadStructAttr()
        {
            StructParser parser = new StructParser();
            parser.LoadStructs("[someshit] struct A { }");
            Assert.AreEqual(1, parser.Errors.Count);
            Assert.AreEqual("Unknown attribute someshit", parser.Errors[0].Message);
        }
        
        [Test] public void UnknownField()
        {
            StructParser parser = new StructParser();
            parser.LoadStructs("struct A { someshit a; othershit b; }");
            Assert.AreEqual(2, parser.Errors.Count);
        }
    }
}
