using SpreadsheetUtilities;
using SS;

namespace SpreadsheetTest
{
    [TestClass]
    public class SpreadsheetTest
    {
        [TestMethod]
        public void BasicTest()
        {
            Spreadsheet sp = new Spreadsheet();
            sp.SetCellContents("A1", 3);
            sp.SetCellContents("A2", new Formula("A1+A1"));
            var temp = sp.GetNamesOfAllNonemptyCells();
            var t2 = sp.GetCellContents("A2");
        }

    }
}