using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTest
{
    /// <summary>
    /// Testing class for methods of Spreadsheet.cs 
    /// </summary>
    [TestClass]
    public class SpreadsheetTest
    {
        [TestMethod]
        public void GetCellContents()
        {
            // make spreadsheet and add contents
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", "text");

            //check that contents are what we expect
            Assert.AreEqual(new Formula("A1+A1"), sp.GetCellContents("A2")); // test with formula
            Assert.AreEqual(3, (double)sp.GetCellContents("A1"));   // test with double
            Assert.AreEqual("text", sp.GetCellContents("A3"));      // test with text
            Assert.AreEqual("", sp.GetCellContents("A5"));      // test with empty cell

            // expect a exception to be thrown due to invalid name, if no exception is thrown test will fail
            try
            {
                sp.GetCellContents("25");
                Assert.IsTrue(false);
            }
            catch (InvalidNameException)
            {

            }
        }

        [TestMethod]
        public void GetNamesofNonEmptyCells()
        {
            // make non-empty cells
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", "text");
            // call the method being tested
            IEnumerable<string> result = sp.GetNamesOfAllNonemptyCells();
            int loopCount = 0;
            // make sure the results are correct and nothing unexpected is returned
            foreach (string name in result)
            {
                loopCount++;
                if (!name.Equals("A1") && !name.Equals("A2") && !name.Equals("A3"))
                {
                    Assert.IsTrue(false);
                }
            }
            // make sure three values were returned
            Assert.AreEqual(3, loopCount);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void FindAllDependents()
        {
            // make spreadsheet with no dependencies
            Spreadsheet sp = new Spreadsheet();

            // set a cell, it should return itself
            IEnumerable<string> result = sp.SetCellContents("Z1", 0);

            // expected results
            List<string> expected = new List<string>();
            expected.Add("Z1");

            foreach (string name in result)
            {
                if (!expected.Contains(name))
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.IsTrue(true);

            sp.SetCellContents("A1", 0);
            sp.SetCellContents("A2", new Formula("A1+1"));
            sp.SetCellContents("A3", new Formula("A2+1"));
            sp.SetCellContents("A4", new Formula("A3+1"));

            // set a cell, it should return list of all dependents
            result = sp.SetCellContents("A1", 1);

            // expected results
            expected = new List<string>();
            expected.Add("A1");
            expected.Add("A2");
            expected.Add("A3");
            expected.Add("A4");

            // check each item in the dependents list against the expected return value
            foreach (string name in result)
            {
                if (!expected.Contains(name))
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void AreDependentsUpdatingCorrectly()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", new Formula("A2 + 1"));
            sp.SetCellContents("A4", new Formula("A3 + A1"));

            // make A3 not depend on anything
            sp.SetCellContents("A3", 4);
            var result = sp.SetCellContents("A1", 2);

            // list of expected dependents that need to now be updated
            List<string> expected = new List<string>();
            expected.Add("A1");
            expected.Add("A2");
            expected.Add("A4");
            int resCount = 0;
            // make sure all results are in IEnumerable
            foreach (string name in result)
            {
                resCount++;
                if (!expected.Contains(name))
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.AreEqual(3, resCount);
            Assert.IsTrue(true);
        }


        [TestMethod]
        public void AreDependentsUpdatingCorrectly2()
        {

            // same as previous test, but with a string rather than a number

            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", new Formula("A2 + 1"));
            sp.SetCellContents("A4", new Formula("A3 + A1"));
            sp.SetCellContents("A3", "word");
            var result = sp.SetCellContents("A1", 2);
            List<string> expected = new List<string>();

            expected.Add("A1");
            expected.Add("A2");
            expected.Add("A4");

            int resCount = 0;

            // check expected return values against returned values
            foreach (string name in result)
            {
                resCount++;
                if (!expected.Contains(name))
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.AreEqual(3, resCount);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SetCellContanetsInvalidName()
        {
            Spreadsheet sp = new Spreadsheet();

            // check lots of invalid names to make sure they are rejected
            try
            {
                sp.SetCellContents("1", 1);
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }

            try
            {
                sp.SetCellContents("25x", "test");
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }

            try
            {
                sp.SetCellContents("x 2", new Formula("A1+B2"));
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }


        }

        [TestMethod]
        public void BlankCellResult()
        {
            Spreadsheet sp = new Spreadsheet();

            Assert.AreEqual("", sp.GetCellContents("A1"));
        }

        [TestMethod]
        public void CircularDependencies()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", new Formula("B2"));
            try
            {
                sp.SetCellContents("B2", new Formula("A1"));
                Assert.Fail();
            }
            catch(CircularException) 
            {
                
            }
        }

        [TestMethod]
        public void CaseSensitity()
        {
            var sp = new Spreadsheet();

            sp.SetCellContents("A1", 1);
            sp.SetCellContents("a1", 3);

            Assert.AreEqual(1.0, sp.GetCellContents("A1"));
            Assert.AreEqual(3.0, sp.GetCellContents("a1"));
        }

        [TestMethod]
        public void InvalidFormula()
        {
            var sp = new Spreadsheet();

            try
            {
                sp.SetCellContents("A1", new Formula("2x+1"));
                Assert.Fail();
            }
            catch (FormulaFormatException)
            {

            }            
        }
    }
}