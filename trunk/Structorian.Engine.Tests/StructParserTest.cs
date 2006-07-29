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
    }
}
