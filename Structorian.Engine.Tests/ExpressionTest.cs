using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Structorian.Engine.Tests
{
    [TestFixture]
    public class ExpressionTest
    {
        [Test] public void EvaluatePrimitive()
        {
            Expression expr = ExpressionParser.Parse("2");
            Assert.AreEqual(2, expr.Evaluate(null));
        }
        
        [Test] public void EvaluateSymbol()
        {
            Expression expr = ExpressionParser.Parse("len");
            StructInstance instance = LoadDataTest.PrepareInstance("struct data { i32 len; }", new byte[] {5, 0, 0, 0});
            Assert.AreEqual(5, expr.Evaluate(instance));
        }
        
        [Test] public void EvaluateMult()
        {
            Expression expr = ExpressionParser.Parse("2*3");
            Assert.AreEqual(6, expr.Evaluate(null));
        }

        [Test] public void EvaluateDiv()
        {
            Expression expr = ExpressionParser.Parse("6/2");
            Assert.AreEqual(3, expr.Evaluate(null));
        }

        [Test] public void EvaluatePlus()
        {
            Expression expr = ExpressionParser.Parse("5+2*3");
            Assert.AreEqual(11, expr.Evaluate(null));
        }

        [Test] public void EvaluateMinus()
        {
            Expression expr = ExpressionParser.Parse("5-2*3");
            Assert.AreEqual(-1, expr.Evaluate(null));
        }

        [Test]
        public void EvaluateParent()
        {
            Expression expr = ExpressionParser.Parse("parent.len");
            StructInstance instance = LoadDataTest.PrepareInstance(
                "struct data { i8 len; child ch; } struct ch { i8 len; } ", 
                new byte[] { 5, 17, 0, 0 });
            Assert.AreEqual(5, expr.Evaluate(instance.Children[0] as StructInstance));
        }
        
        [Test] public void EvaluateGT()
        {
            Assert.IsTrue(ExpressionParser.Parse("3 > 2").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 > 3").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 > 2").EvaluateBool(null));
        }

        [Test] public void EvaluateLT()
        {
            Assert.IsFalse(ExpressionParser.Parse("3 < 2").EvaluateBool(null));
            Assert.IsTrue(ExpressionParser.Parse("2 < 3").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 > 2").EvaluateBool(null));
        }

        [Test] public void EvaluateGE()
        {
            Assert.IsTrue(ExpressionParser.Parse("3 >= 2").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 >= 3").EvaluateBool(null));
            Assert.IsTrue(ExpressionParser.Parse("2 >= 2").EvaluateBool(null));
        }

        [Test] public void EvaluateLE()
        {
            Assert.IsFalse(ExpressionParser.Parse("3 <= 2").EvaluateBool(null));
            Assert.IsTrue(ExpressionParser.Parse("2 <= 3").EvaluateBool(null));
            Assert.IsTrue(ExpressionParser.Parse("2 <= 2").EvaluateBool(null));
        }

        [Test]
        public void EvaluateNE()
        {
            Assert.IsTrue(ExpressionParser.Parse("3 != 2").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 != 2").EvaluateBool(null));
        }

        [Test] public void EvaluateEQ()
        {
            Assert.IsTrue(ExpressionParser.Parse("2 == 2").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("3 == 2").EvaluateBool(null));
        }
        
        [Test] public void EQString()
        {
            Assert.IsTrue(ExpressionParser.Parse("\"FORM\" == \"FORM\"").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("\"FORM\" == \"NORM\"").EvaluateBool(null));
        }
        
        [Test] public void ExprToString()
        {
            Assert.AreEqual("2 == 2", ExpressionParser.Parse("2 == 2").ToString());
        }
        
        [Test] public void EvaluateHex()
        {
            Assert.AreEqual(15, ExpressionParser.Parse("0x0F").EvaluateInt(null));
        }
        
        [Test] public void EvaluateAnd()
        {
            Assert.IsTrue(ExpressionParser.Parse("2 == 2 && 3 == 3").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 == 2 && 3 == 2").EvaluateBool(null));
        }

        [Test] public void EvaluateOr()
        {
            Assert.IsTrue(ExpressionParser.Parse("2 == 2 || 3 == 2").EvaluateBool(null));
            Assert.IsFalse(ExpressionParser.Parse("2 == 3 || 3 == 2").EvaluateBool(null));
        }
        
        [Test] public void EvaluateParens()
        {
            Assert.AreEqual(21, ExpressionParser.Parse("(5+2)*3").EvaluateInt(null));
        }

        [Test] public void EvaluateUnaryMinus()
        {
            Assert.AreEqual(-4, ExpressionParser.Parse("-4").EvaluateInt(null));
        }
        
        [Test] public void EvaluateBitOps()
        {
            Assert.AreEqual(2, ExpressionParser.Parse("6 & 3").EvaluateInt(null));
            Assert.AreEqual(7, ExpressionParser.Parse("6 | 3").EvaluateInt(null));
        }
        
        [Test] public void EvaluateNot()
        {
            Assert.IsFalse(ExpressionParser.Parse("!(2 == 2)").EvaluateBool(null));
        }
        
        [Test] public void EvaluateStringConcat()
        {
            Assert.AreEqual("B2", ExpressionParser.Parse("\"B\" + 2").EvaluateString(null));
        }
    }
}
