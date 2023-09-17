using SpreadsheetUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {
        [TestMethod]
        public void CreatingValidFormula()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void CreatingValidFormul()
        {
            // a method that converts all the letters in a string to upper case
            Func<string, string> N = str => str.ToUpper();
            // a method that returns true only if a string consists of one letter followed by one digit
            Func<string, bool> V = str => Regex.IsMatch(str, @"^[a-zA-Z][0-9]$");

            new Formula("x2+y3", N, V);  // should succeed
            try
            {
                new Formula("x+y3", N, V);  // should throw an exception, since V(N("x")) is false
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }
            try
            {
                new Formula("2x+y3", N, V);  // should throw an exception, since "2x+y3" is syntactically incorrect.
                Debug.Assert(false);
            }
            catch (FormulaFormatException)
            {
                // Expected.
            }

            //new Formula("x+7", N, s => true).Evaluate(L);  // is 11
            //new Formula("x+7").Evaluate(L);  // is 9

            //new Formula("x+y*z", N, s => true).GetVariables();  // should enumerate "X", "Y", and "Z"
            //new Formula("x+X*z", N, s => true).GetVariables();  // should enumerate "X" and "Z".
            //new Formula("x+X*z").GetVariables();  // should enumerate "x", "X", and "z".

            Debug.Assert(new Formula("x + y", N, s => true).ToString() == "X+Y");  // should return "X+Y"
            Debug.Assert(new Formula("x + Y").ToString() == "x+Y");  // should return "x+Y"

            Debug.Assert(new Formula("1+1").Equals(null) == false);  // the argument must be of type Formula
            Debug.Assert(new Formula("1+1").Equals("test") == false);  // the argument must be of type Formula
            Formula f = new Formula("1+1");
            Debug.Assert(new Formula("1+1").Equals("1+1+1") == false);  // The token list lengths must be the same
            Debug.Assert(new Formula("1+2").Equals("2+1") == false);  // The token order must be the same.
            Debug.Assert(new Formula("123").Equals("abc") == false);  // This should not throw exceptions.
            Debug.Assert(f.Equals(f) == true);  // comparing to self is true
            Debug.Assert(new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) == true);  // is true
            Debug.Assert(new Formula("x1+y2").Equals(new Formula("X1+Y2")) == false);  // is false
            //Debug.Assert(new Formula("x1+y2").GetHashCode() == (new Formula("X1+Y2").GetHashCode()) == false);  // is false
            Debug.Assert(new Formula("x1+y2").Equals(new Formula("y2+x1")) == false);  // is false
            Debug.Assert(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) == true);  // is true
        }

        [TestMethod]
        public void CreatingValidFormu()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void CreatingValidForm()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);
        }

        [TestMethod]
        public void CreatingValidFo()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);
        }
    }
}