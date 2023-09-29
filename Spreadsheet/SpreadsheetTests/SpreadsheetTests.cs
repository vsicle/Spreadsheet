using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SpreadsheetUtilities;
using SS;
using System.Text.RegularExpressions;

namespace SpreadsheetTest
{
    /// <summary>
    /// Testing class for methods of Spreadsheet.cs 
    /// </summary>
    [TestClass]
    public class SpreadsheetTest
    {

        [TestMethod]
        public void TestSaveMethod()
        {

            Spreadsheet sp = new Spreadsheet();
            sp.SetContentsOfCell("A1", "5.0");
            sp.SetContentsOfCell("B3", "= A1 +2");


            string testFileName = "save.txt";

            sp.Save(testFileName);

            Assert.IsTrue(File.Exists(testFileName));

            string jsonData = File.ReadAllText(testFileName);

            // copy of the example serialization from xml comment
            string expectedJsonData = "{\r\n        \"Cells\": {\r\n            \"A1\": {\r\n              \"StringForm\": \"5\"\r\n            },\r\n            \"B3\": {\r\n              \"StringForm\": \"=A1+2\"\r\n            }\r\n          },\r\n          \"Version\": \"default\"\r\n        }";
            Assert.AreEqual(Regex.Replace(expectedJsonData, @"\s+", ""), Regex.Replace(jsonData, @"\s+", ""));


        }


        [TestMethod]
        public void AreValuesUpdatingCorrectly()
        {
            Spreadsheet sp = new Spreadsheet();

            // single layer dependency
            sp.SetContentsOfCell("A1", "2");
            sp.SetContentsOfCell("A2", "=A1");
            Assert.AreEqual(2.0, sp.GetCellValue("A1"));
            Assert.AreEqual(2.0, sp.GetCellValue("A2"));

            Spreadsheet sp1 = new Spreadsheet();

            // single layer dependency with math formula
            sp1.SetContentsOfCell("A1", "2");
            sp1.SetContentsOfCell("A2", "=A1 * 4");
            Assert.AreEqual(2.0, sp1.GetCellValue("A1"));
            Assert.AreEqual(8.0, sp1.GetCellValue("A2"));

            //update contents of A2, its new value should be 0
            sp1.SetContentsOfCell("A2", "=A1 - A1");
            Assert.AreEqual(0.0, sp1.GetCellValue("A2"));
        }

        [TestMethod]
        public void FormulaLeadingToBlankOrStringCell()
        {
            Spreadsheet sp = new Spreadsheet();

            try
            {
                sp.SetContentsOfCell("A1", "=A2");
                Assert.Fail();
            }
            catch (FormulaFormatException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }


            try
            {
                sp.SetContentsOfCell("A1", "text");
                sp.SetContentsOfCell("A2", "=A1");
                Assert.Fail();
            }
            catch (FormulaFormatException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }

        }





        //  vvvvvvvvv
        // Tests from PS4 
        //  vvvvvvvvv


        [TestMethod]
        public void GetCellContents()
        {
            // make sp and add contents
            Spreadsheet sp = new Spreadsheet();
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("A2", "=A1+A1");
            sp.SetContentsOfCell("A3", "text");

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
        public void GetCellValue()
        {
            Spreadsheet sp = new Spreadsheet();

            // check exceptions
            try
            {
                sp.GetCellValue("2");
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail("wrong exception thrown");
            }

            // check if values are calculated correctly

            sp.SetContentsOfCell("A1", "1");
            sp.SetContentsOfCell("A2", "2");
            sp.SetContentsOfCell("A3", "3");
            sp.SetContentsOfCell("A4", "4");

            sp.SetContentsOfCell("B1", "= A1 * 2");
            sp.SetContentsOfCell("B2", "= B1 * A4");
            sp.SetContentsOfCell("B3", "=B2+1");
            sp.SetContentsOfCell("B4", "=A1+A2+A3+A4+B1+B2+B3");

            Assert.AreEqual(2.0, sp.GetCellValue("B1"));
            Assert.AreEqual(8.0, sp.GetCellValue("B2"));
            Assert.AreEqual(9.0, sp.GetCellValue("B3"));
            Assert.AreEqual(29.0, sp.GetCellValue("B4"));

            // change A1 to see if everything changes accordingly
            sp.SetContentsOfCell("A1", "5");

            Assert.AreEqual(10.0, sp.GetCellValue("B1"));
            Assert.AreEqual(40.0, sp.GetCellValue("B2"));
            Assert.AreEqual(41.0, sp.GetCellValue("B3"));
            Assert.AreEqual(105.0, sp.GetCellValue("B4"));

            // add C1 to make sure division works
            sp.SetContentsOfCell("C1", "=B4 / B1");
            Assert.AreEqual(10.5, sp.GetCellValue("C1"));
        }


        [TestMethod]
        public void GetNamesofNonEmptyCells()
        {
            // make non-empty cells
            Spreadsheet sp = new Spreadsheet();
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("A2", "=A1+A1");
            sp.SetContentsOfCell("A3", "text");
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
            // make sp with no dependencies
            Spreadsheet sp = new Spreadsheet();

            // set a cell, it should return itself
            IEnumerable<string> result = sp.SetContentsOfCell("Z1", "0");

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

            sp.SetContentsOfCell("A1", "0");
            sp.SetContentsOfCell("A2", "=A1+1");
            sp.SetContentsOfCell("A3", "=A2+1");
            sp.SetContentsOfCell("A4", "=A3+1");

            // set a cell, it should return list of all dependents
            result = sp.SetContentsOfCell("A1", "1");

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
            sp.SetContentsOfCell("A1", "3");
            sp.SetContentsOfCell("A2", "=A1+A1");
            sp.SetContentsOfCell("A3", "=A2 + 1");
            sp.SetContentsOfCell("A4", "=A3 + A1");

            // make A3 not depend on anything
            sp.SetContentsOfCell("A3", "4");
            var result = sp.SetContentsOfCell("A1", "2");

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
        public void SetCellContanetsInvalidName()
        {
            Spreadsheet sp = new Spreadsheet();

            // check lots of invalid names to make sure they are rejected
            try
            {
                sp.SetContentsOfCell("1", "1");
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }

            try
            {
                sp.SetContentsOfCell("25x", "test");
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }

            try
            {
                sp.SetContentsOfCell("x 2", "=A1+B2");
                Assert.Fail();
            }
            catch (InvalidNameException)
            {
                Assert.IsTrue(true);
            }


        }

        [TestMethod]
        public void BlankCellContentsAndValue()
        {
            Spreadsheet sp = new Spreadsheet();

            Assert.AreEqual("", sp.GetCellContents("A1"));
            Assert.AreEqual("", sp.GetCellValue("B1"));
        }

        [TestMethod]
        public void CircularDependencies()
        {

            Spreadsheet sp = new Spreadsheet();
            sp.SetContentsOfCell("A1", "3.5");
            sp.SetContentsOfCell("B1", "2");
            sp.SetContentsOfCell("A1", "=B1");

            try
            {
                sp.SetContentsOfCell("B1", "=A1");
                Assert.Fail();
            }
            catch (CircularException)
            {
                Assert.IsTrue(true);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CaseSensitity()
        {
            var sp = new Spreadsheet();

            sp.SetContentsOfCell("A1", "1");
            sp.SetContentsOfCell("a1", "3");

            Assert.AreEqual(1.0, sp.GetCellContents("A1"));
            Assert.AreEqual(3.0, sp.GetCellContents("a1"));
        }

        [TestMethod]
        public void InvalidFormula()
        {
            var sp = new Spreadsheet();

            try
            {
                sp.SetContentsOfCell("A1", "=2x+1");
                Assert.Fail();
            }
            catch (FormulaFormatException)
            {

            }
        }
    }
}