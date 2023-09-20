using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTest
{
    [TestClass]
    public class SpreadsheetTest
    {
        [TestMethod]
        public void GetCellContents()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", "text");
            Assert.AreEqual(new Formula("A1+A1"), sp.GetCellContents("A2")); // test with formula
            Assert.AreEqual(3, (double)sp.GetCellContents("A1"));   // test with double
            Assert.AreEqual("text", sp.GetCellContents("A3"));      // test with text
            Assert.AreEqual("", sp.GetCellContents("A5"));      // test with empty cell

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
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", "text");
            IEnumerable<string> result = sp.GetNamesOfAllNonemptyCells();

            foreach (string name in result)
            {
                if(!name.Equals("A1") && !name.Equals("A2") && !name.Equals("A3"))
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void FindAllDependents()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", new Formula("A2 + 1"));
            sp.SetCellContents("A4", new Formula("A3 + 1"));
            IEnumerable<string> result = sp.SetCellContents("Z1", 0);
            List<string> expected = new List<string>();
            expected.Add("A2");
            expected.Add("A3");
            expected.Add("A4");

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
            sp.SetCellContents("A3", 4);
            var result = sp.SetCellContents("A1", 2);
            List<string> expected = new List<string>();

            expected.Add("A2");
            expected.Add("A4");

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
        public void AreDependentsUpdatingCorrectly2()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            sp.SetCellContents("A3", new Formula("A2 + 1"));
            sp.SetCellContents("A4", new Formula("A3 + A1"));
            sp.SetCellContents("A3", "word");
            var result = sp.SetCellContents("A1", 2);
            List<string> expected = new List<string>();

            expected.Add("A2");
            expected.Add("A4");

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
        public void SetCellContanetsInvalidName()
        {
            Spreadsheet sp = new Spreadsheet();

            try
            {
                sp.SetCellContents("1", 1);
                Assert.Fail();
            }
            catch(InvalidNameException)
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

        

    }
}