using SpreadsheetUtilities;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {

        /// <summary>
        /// Zero argument constructor for making a blank spreadsheet
        /// </summary>
        public Spreadsheet()
        {

        }


        public override object GetCellContents(string name)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            throw new NotImplementedException();
        }

        public override IList<string> SetCellContents(string name, double number)
        {
            throw new NotImplementedException();
        }

        public override IList<string> SetCellContents(string name, string text)
        {
            throw new NotImplementedException();
        }

        public override IList<string> SetCellContents(string name, Formula formula)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            throw new NotImplementedException();
        }

        private class Cell
        {
            private object contents; // contents of the cell
            private Func<string, double> lookup;

            public Cell()
            {
                contents = "";
            }

            public Cell(double number)
            {
                contents = number;
            }

            public Cell(string text)
            {
                contents = text;
            }

            public Cell(Formula formula, Func<string, double> _lookup)
            {
                contents = formula;
                lookup = _lookup;
            }
        }
    }
}