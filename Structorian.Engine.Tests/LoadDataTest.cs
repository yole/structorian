using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Structorian.Engine.Tests
{
    [TestFixture]
    public class LoadDataTest
    {
        private static InstanceTree PrepareInstanceTree(string structDefs, byte[] data)
        {
            StructFile structFile = new StructParser().LoadStructs(structDefs);
            MemoryStream dataStream = new MemoryStream(data);
            return structFile.Structs[0].LoadData(dataStream);
        }

        public static StructInstance PrepareInstance(string structDefs, byte[] data)
        {
            InstanceTree tree = PrepareInstanceTree(structDefs, data);
            return (StructInstance) tree.Children [0];
        }

        [Test] public void LoadU32()
        {
            StructInstance instance = PrepareInstance("struct BITMAPINFOHEADER { u32 biSize; }", new byte[] { 0x5, 0, 0, 0});
            Assert.AreEqual(1, instance.Cells.Count);
            Assert.AreEqual("5", instance.Cells[0].ToString());
        }

        [Test] public void LoadStr()
        {
            StructInstance instance = PrepareInstance("struct BITMAPFILEHEADER { str [len=2] bfType; }", new byte[] { (byte) 'B', (byte) 'M' });
            Assert.AreEqual(1, instance.Cells.Count);
            Assert.AreEqual("BM", instance.Cells[0].ToString());
        }
        
        [Test] public void LoadStrBadSize()
        {
            StructInstance instance = PrepareInstance("struct BITMAPFILEHEADER { str [len=4] bfType; }", new byte[] { (byte)'B', (byte)'M' });
            Assert.AreEqual(1, instance.Cells.Count);
            Assert.IsTrue(instance.Cells[0].IsError());
        }
        
        [Test] public void LoadNullTerminatedStr()
        {
            StructInstance instance = PrepareInstance("struct BITMAPFILEHEADER { str bfType; }", new byte[] { (byte)'B', (byte)'M', 0, (byte) 'P' });
            Assert.AreEqual("BM", instance.Cells[0].ToString());
        }
        
        [Test] public void SignedVsUnsigned()
        {
            byte[] bytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            StructInstance instance = PrepareInstance("struct BITMAPINFOHEADER { u32 biSize; }", bytes);
            Assert.AreEqual("4294967295", instance.Cells[0].ToString());
            
            instance = PrepareInstance("struct BITMAPINFOHEADER { i32 biSize; }", bytes);
            Assert.AreEqual("-1", instance.Cells[0].ToString());
        }
        
        [Test] public void TestX32()
        {
            StructInstance instance = PrepareInstance("struct BITMAPINFOHEADER { x32 biSize; }", new byte[] { 0x5, 0, 0, 0 });
            Assert.AreEqual("0x00000005", instance.Cells[0].ToString());
        }
        
        [Test] public void LoadChild()
        {
            StructInstance instance = PrepareInstance(
                "struct BITMAPFILEHEADER { x32 biSize; child BITMAPINFOHEADER; } struct BITMAPINFOHEADER { u32 biSize; } ", 
                new byte[] { 0x5, 0, 0, 0, 0x10, 0, 0, 0 });
            Assert.IsTrue(instance.HasChildren);
            Assert.AreEqual(1, instance.Children.Count);
            InstanceTreeNode childInstance = instance.Children[0];
            Assert.IsFalse(childInstance.HasChildren);
            Assert.AreEqual("16", childInstance.Cells[0].ToString());
        }
        
        [Test] public void LoadChildOffset()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B [offset=a]; } struct B { u8 value; } ",
                new byte[] { 2, 0, 17 });
            InstanceTreeNode childInstance = instance.Children[0];
            Assert.AreEqual("17", childInstance.Cells[0].ToString());
        }
        
        [Test] public void LoadChildCount()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B [offset=a, count=2]; } struct B { u8 value; } ",
                new byte[] { 2, 0, 17, 37 });
            Assert.AreEqual(2, instance.Children.Count);
            InstanceTreeNode childInstance = instance.Children[1];
            Assert.AreEqual("37", childInstance.Cells[0].ToString());
        }
        
        [Test] public void NotifyChild()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; child B [offset=a]; } struct B { u8 value; } ",
                new byte[] {2, 0, 17, 37});
            InstanceTreeNode lastAddParent = null;
            InstanceTreeNode lastAddChild = null;
            tree.InstanceAdded += delegate(object sender, InstanceAddedEventArgs e)
                                      {
                                          lastAddParent = e.Parent;
                                          lastAddChild = e.Child;
                                      };
            tree.Children [0].NeedChildren();
            Assert.AreSame(tree.Children [0], lastAddParent);
            Assert.AreSame(tree.Children [0].Children[0], lastAddChild);
        }
        
        [Test] public void Seek()
        {
            InstanceTreeNode instance = PrepareInstance(
                "struct A { seek(3); u8 a; }",
                new byte[] { 2, 0, 17, 37 });
            Assert.AreEqual("37", instance.Cells[0].ToString());
        }
        
        [Test] public void Rewind()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; seek(3); u8 b; rewind; u8 c;}",
                new byte[] { 2, 17, 0, 37 });
            Assert.AreEqual(3, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells[2].ToString());
        }
        
        [Test] public void Repeat()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; repeat(a) { u8 c; } }",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual(3, instance.Cells.Count);
            Assert.AreEqual("37", instance.Cells[2].ToString());
        }
        
        [Test] public void Enum()
        {
            StructInstance instance = PrepareInstance(
                "struct A { enum8 a [enum=E]; } enum E { f, g, h }",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual("h", instance.Cells[0].ToString());
        }

        [Test] public void IfField()
        {
            string strs = "struct A { u8 a; if (a > 1) { u8 b; } }";
            StructInstance instance = PrepareInstance(strs, new byte[] { 2, 17, 37 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells[1].Value);

            StructInstance instance2 = PrepareInstance(strs, new byte[] { 0, 17, 37 });
            Assert.AreEqual(1, instance2.Cells.Count);
        }

        [Test] public void LoadChildIf()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; if (a > 0) { child B [offset=a]; } } struct B { u8 value; } ",
                new byte[] { 2, 0, 17, 37 });
            Assert.AreEqual(1, instance.Children.Count);
        }

        [Test] public void LoadChildIfFalse()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; if (a > 4) { child B [offset=a]; } } struct B { u8 value; } ",
                new byte[] { 2, 0, 17, 37 });
            Assert.AreEqual(0, instance.Children.Count);
        }

        [Test] public void LoadSibling()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; sibling B; } struct B { u8 value; }",
                new byte[] { 17, 37});
            Assert.AreEqual(1, tree.Children[0].Cells.Count);
            Assert.AreEqual(2, tree.Children.Count);
        }

        [Test] public void LoadSiblingDefault()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; if (a > 0) { sibling; } }",
                new byte[] { 17, 0});
            Assert.AreEqual(1, tree.Children[0].Cells.Count);
            Assert.AreEqual(2, tree.Children.Count);
            Assert.AreEqual(1, tree.Children[1].Cells.Count);
        }
        
        [Test] public void InvalidSiblingStruct()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; if (a > 0) { sibling NoSuchStruct; } }",
                new byte[] { 17, 0 });
            Assert.AreEqual(2, tree.Children[0].Cells.Count);
            Assert.IsTrue(tree.Children[0].Cells[1].IsError());
        }
        
        [Test] public void Include()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; include B; } struct B { u8 value; } ",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells[1].Value);
        }

        [Test] public void AssertFalse()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; assert(a == 3); }",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.IsTrue(instance.Cells[1].IsError());
        }

        [Test] public void FieldId()
        {
            StructInstance instance = PrepareInstance(
                "struct A { i8 \"Long nice name\" [id=q]; }",
                new byte[] { 17 });
            Assert.AreEqual(17, instance.EvaluateSymbol("q"));
        }
        
        [Test] public void HiddenField()
        {
            StructInstance instance = PrepareInstance(
                "struct A { [hidden] u8 a; u8 b; }",
                new byte[] { 17, 37 });
            Assert.AreEqual(1, instance.Cells.Count);
            Assert.AreEqual("37", instance.Cells[0].Value);
            Assert.AreEqual(17, instance.EvaluateSymbol("a"));
        }
        
        [Test] public void ChildGroup()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B [group=X]; } struct B { u8 value; } ",
                new byte[] { 2, 17 });
            InstanceTreeNode groupInstance = instance.Children[0];
            Assert.AreEqual("X", groupInstance.NodeName);
            Assert.AreEqual(1, groupInstance.Children.Count);
            Assert.AreEqual("17", groupInstance.Children[0].Cells[0].Value);
        }

        [Test] public void ChildGroupCount()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B [group=X, count=2]; } struct B { u8 value; } ",
                new byte[] { 2, 17, 37 });
            InstanceTreeNode groupInstance = instance.Children[0];
            Assert.AreEqual("X", groupInstance.NodeName);
            Assert.AreEqual(2, groupInstance.Children.Count);
            Assert.AreEqual("17", groupInstance.Children[0].Cells[0].Value);
            Assert.AreEqual("37", groupInstance.Children[1].Cells[0].Value);
        }
        
        [Test] public void NotifyFromGroupContainer()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; child B [group=X]; } struct B { u8 value; } ",
                new byte[] { 2, 17, 37 });
            InstanceTreeNode groupInstance = tree.Children[0].Children [0];

            InstanceTreeNode lastAddParent = null;
            InstanceTreeNode lastAddChild = null;
            tree.InstanceAdded += delegate(object sender, InstanceAddedEventArgs e)
                                      {
                                          lastAddParent = e.Parent;
                                          lastAddChild = e.Child;
                                      };
            groupInstance.NeedChildren();
            Assert.AreSame(groupInstance, lastAddParent);
            Assert.AreSame(groupInstance.Children[0], lastAddChild);
        }

        [Test] public void NotifyFromChildInGroup()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { u8 a; child B [group=X]; } struct B { u8 value; child C; } struct C { u8 q; } ",
                new byte[] { 2, 17, 37 });
            InstanceTreeNode groupInstance = tree.Children[0].Children[0];
            InstanceTreeNode instanceB = groupInstance.Children[0];

            InstanceTreeNode lastAddParent = null;
            InstanceTreeNode lastAddChild = null;
            tree.InstanceAdded += delegate(object sender, InstanceAddedEventArgs e)
                                      {
                                          lastAddParent = e.Parent;
                                          lastAddChild = e.Child;
                                      };
            instanceB.NeedChildren();
            Assert.AreSame(instanceB, lastAddParent);
            Assert.AreSame(instanceB.Children[0], lastAddChild);
        }
        
        [Test] public void BitField()
        {
            StructInstance instance = PrepareInstance(
                "struct A { bitfield(1) { u8 lo [frombit=0, tobit=3]; u8 hi [frombit=4, tobit=7]; } }",
                new byte[] { 0xFC });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("12", instance.Cells[0].Value);
            Assert.AreEqual("15", instance.Cells[1].Value);
        }
        
        [Test] public void EnumInBitField()
        {
            StructInstance instance = PrepareInstance(
                "struct A { bitfield(1) { enum8 e [enum=X, frombit=0, tobit=3]; } } enum X { q=12 }",
                new byte[] { 0xFC });
            Assert.AreEqual(1, instance.Cells.Count);
            Assert.AreEqual("q", instance.Cells[0].Value);
        }
        
        [Test] public void NodeName()
        {
            InstanceTree tree = PrepareInstanceTree(
                "struct A { enum8 e [enum=X]; nodename e; } enum X { q=12 }",
                new byte[] { 0x0C });
            InstanceTreeNode lastChangedNode = null;
            tree.NodeNameChanged += delegate(object sender, NodeNameChangedEventArgs e)
                                        {
                                            lastChangedNode = e.Node;
                                        };
            
            Assert.AreEqual(1, tree.Children [0].Cells.Count);
            Assert.AreEqual("q", tree.Children [0].NodeName);
            Assert.AreSame(tree.Children [0], lastChangedNode);
        }
        
        [Test] public void Switch()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; switch(a) { case 0 { u8 x; } case 2 { u8 b; } } }",
                new byte[] { 2, 17 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("b", instance.Cells[1].Tag);
        }

        [Test] public void SwitchDefault()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; switch(a) { case 0 { u8 x; } default { u8 b; } } }",
                new byte[] { 2, 17 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("b", instance.Cells[1].Tag);
        }
        
        [Test] public void SwitchOnString()
        {
            StructInstance instance = PrepareInstance(
                "struct A { str [len=1] a; switch(a) { case (\"b\") { u8 x; } default { u8 b; } } }",
                new byte[] { (byte) 'b', 17 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("x", instance.Cells[1].Tag);
        }
        
        [Test] public void InheritEnum()
        {
            StructInstance instance = PrepareInstance(
                "struct A { enum8 a [enum=F]; } enum E { h=2 } enum F [inherit=E] { f, g }",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual("h", instance.Cells[0].ToString());
        }
        
        [Test] public void Set()
        {
            StructInstance instance = PrepareInstance(
                "struct A { set8 a [enum=E]; } enum E { f, g, h }",
                new byte[] { 6 });
            Assert.AreEqual("g, h", instance.Cells[0].ToString());
        }
        
        [Test] public void UnixTime()
        {
            StructInstance instance = PrepareInstance(
                "struct A { unixtime t; }",
                new byte[] { 0x42, 0xE8, 0xC5, 0x3D });
            Assert.AreEqual(new DateTime(2002, 11, 4, 3, 23, 46), instance.Cells[0].GetValue());
        }

        [Test] public void ChildInInclude()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; include B; } struct B { child C; } struct C { u8 x; } ",
                new byte[] { 2, 17, 37 });
            Assert.IsTrue(instance.HasChildren);
            Assert.AreEqual(1, instance.Children.Count);
            Assert.AreEqual("17", instance.Children [0].Cells[0].Value);
        }
        
        [Test] public void StrTrailingNull()
        {
            StructInstance instance = PrepareInstance(
                "struct A { str [len=2] a; }",
                new byte[] { (byte) 'B', 0 });
            Assert.AreEqual("B", instance.Cells[0].Value);
        }

        [Test] public void Global()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 x; global a [value=x]; child B; } struct B { str [len=a] s; }",
                new byte[] { 1, (byte)'B' });
            Assert.AreEqual("B", instance.Children [0].Cells[0].Value);
        }

        [Test] public void Local()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 x; local a [value=x*2]; str [len=a] s; }",
                new byte[] { 1, (byte)'B', (byte) 'M' });
            Assert.AreEqual("BM", instance.Cells[1].Value);
        }
        
        [Test] public void EvalStructOffset()
        {
            StructInstance instance = PrepareInstance(
                "struct A { str [len=2] s; child B; } struct B { calc o [value=StructOffset];}",
                new byte[] { (byte)'B', (byte)'M', 1 });
            Assert.AreEqual("2", instance.Children [0].Cells[0].Value);
        }
        
        [Test] public void ImplicitRewind()
        {
            StructInstance instance = PrepareInstance(
                "struct A { child B [count=2]; } struct B { u8 x; seek 3;}",
                new byte[] {1, 2, 3, 4});
            Assert.AreEqual("2", instance.Children[1].Cells[0].Value);
        }
        
        [Test] public void Skip()
        {
            StructInstance instance = PrepareInstance(
                "struct A { skip 3; u8 x; }",
                new byte[] {1, 2, 3, 4});
            Assert.AreEqual("4", instance.Cells[0].Value);
        }
        
        [Test] public void WStr()
        {
            StructInstance instance = PrepareInstance(
                "struct A { wstr [len=2] s; u8 a; }",
                new byte[] { (byte) 'B', 0, (byte) 'M', 0, 5 });
            Assert.AreEqual("BM", instance.Cells[0].Value);
            Assert.AreEqual("5", instance.Cells[1].Value);
        }
        
        [Test] public void IfElseFalse()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; if (a == 2) { str [len=1] b; } else { u8 c; } }",
                new byte[] { 3, 17 });
            Assert.AreEqual("17", instance.Cells[1].Value);
        }
        
        [Test] public void IfElseTrue()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; if (a == 2) { str [len=1] b; } else { u8 c; } }",
                new byte[] { 2, (byte) 'B' });
            Assert.AreEqual("B", instance.Cells[1].Value);
            Assert.AreEqual(2, instance.Cells.Count);
        }
        
        [Test] public void ParentCount()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B; } struct B { calc c [value=ParentCount]; }",
                new byte[] { 2, 17 });
            Assert.AreEqual("1", instance.Children[0].Cells[0].Value);
        }
        
        [Test] public void ParentDotParent()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; child B; } struct B { u8 b; child C; } struct C { calc x [value=Parent.Parent.a]; }",
                new byte[] { 2, 17 });
            Assert.AreEqual("2", instance.Children[0].Children [0].Cells[0].Value);
        }
        
        [Test] public void LoadStrCalcSize()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; str [len=a] b; }",
                new byte[] { 1, (byte) 'B' });
            Assert.AreEqual("B", instance.Cells[1].Value);
        }
        
        [Test, ExpectedException(typeof(ParseException))] 
        public void ElseWithoutIf()
        {
            PrepareInstance("struct A { else { u8 a; } }", new byte[] {1});
        }
        
        [Test] public void IfElseInsideIf()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u8 a; if (a > 0) { if (a == 2) { u8 b; } else { u8 c; } } }",
                new byte[] {2, 17, 37});
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells [1].Value);
            Assert.AreEqual("b", instance.Cells [1].Tag);
        }
        
        [Test] public void EvalGlobalEnum()
        {
            StructInstance instance = PrepareInstance(
                "[global] enum E { o, p, q } struct A { enum8 [enum=E] a; if (a == q) { u8 b; } }",
                new byte[] { 2, 17, 37 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells[1].Value);
        }
        
        [Test] public void EvalGlobalMaskEnum()
        {
            StructInstance instance = PrepareInstance(
                "[globalmask] enum E { o, p, q } struct A { enum8 [enum=E] a; if (a == q) { u8 b; } }",
                new byte[] { 4, 17, 37 });
            Assert.AreEqual(2, instance.Cells.Count);
            Assert.AreEqual("17", instance.Cells[1].Value);
        }
        
        [Test] public void OverrideLocal()
        {
            StructInstance instance = PrepareInstance(
                "struct A { calc a [value=0]; repeat(3) { calc a [value=a+1]; } }",
                new byte[0]);
            Assert.AreEqual(4, instance.Cells.Count);
            Assert.AreEqual("1", instance.Cells[1].Value);
            Assert.AreEqual("3", instance.Cells[3].Value);
        }
        
        [Test] public void CurOffset()
        {
            StructInstance instance = PrepareInstance(
                "struct A { u16 x; calc y [value=CurOffset]; }",
                new byte[2] {4, 0});
            Assert.AreEqual("2", instance.Cells[1].Value);
        }
    }
}
