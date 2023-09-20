using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected).
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// We are not concerned with values in PS4, but to give context for the future of the project,
    /// the value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid). 
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> cells;
        private DependencyGraph dependencyGraph;
        /// <summary>
        /// Zero argument constructor for making a blank spreadsheet
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.
        /// The return value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (IsValidName(name) && cells.ContainsKey(name))
            {
                return cells[name].contents;
            }
            else if (IsValidName(name))
            {
                return "";
            }
            else
            {
                throw new InvalidNameException();
            }
        }

        /// <summary>
        /// Helper method do decide if the input string is a valid variable
        /// </summary>
        /// <param name="str">input string that is being validated for proper variable format</param> 
        /// <returns>boolean whether the string is a valid variable format</returns> 
        private static bool IsValidName(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> nonEmptyCellNames = new List<string>();

            // for every cell that has been "added/modified from base state"
            // if not null or empty add it
            foreach (string cellName in cells.Keys)
            {
                if (!string.IsNullOrEmpty(cells[cellName].contents.ToString()))
                {
                    nonEmptyCellNames.Add(cellName);
                }
            }
            return nonEmptyCellNames;

        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, double number)
        {
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            else if (!cells.ContainsKey(name))
            {
                cells.Add(name, new Cell(number));
            }
            else
            {
                // if prev content was a formula remove name from their dependencies
                if (cells[name].contents is Formula)
                {
                    // make temp holder for Fomula to get its variables
                    Formula tempFormula = (Formula)cells[name].contents;

                    // this cell no longer depends on the variables of the formula that was in it
                    foreach (string variable in tempFormula.GetVariables())
                    {
                        dependencyGraph.RemoveDependency(variable, name);
                    }

                }

                // change the contents of the cell after doing check for it being a formula
                cells[name].contents = number;

            }

            return FindAllDependents(name);

        }

        /// <summary>
        /// Recursively finds all direct and indirect dependents of the specified cell.
        /// </summary>
        private List<string> FindAllDependents(string name)
        {
            List<string> allDependents = new List<string>();
            HashSet<string> visited = new HashSet<string>();

            // Start DFS traversal
            Visit(name, visited, allDependents);

            return allDependents;
        }

        /// <summary>
        /// Helper method for DFS traversal to find dependents.
        /// </summary>
        private void Visit(string cell, HashSet<string> visited, List<string> allDependents)
        {
            visited.Add(cell);

            // Add all direct dependents to the result list
            foreach (string dependent in dependencyGraph.GetDependents(cell))
            {
                allDependents.Add(dependent);

                // Continue DFS for unvisited dependents
                if (!visited.Contains(dependent))
                {
                    Visit(dependent, visited, allDependents);
                }
            }
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, string text)
        {
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            else if (!cells.ContainsKey(name))
            {
                cells.Add(name, new Cell(text));
            }
            else
            {
                // if prev content was a formula remove name from their dependencies
                if (cells[name].contents is Formula)
                {
                    // make temp holder for Fomula to get its variables
                    Formula tempFormula = (Formula)cells[name].contents;

                    // this cell no longer depends on the variables of the formula that was in it
                    foreach (string variable in tempFormula.GetVariables())
                    {
                        dependencyGraph.RemoveDependency(variable, name);
                    }

                }

                // change the contents of the cell after doing check for it being a formula
                cells[name].contents = text;

            }

            return FindAllDependents(name);

        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetCellContents(string name, Formula formula)
        {
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            else if (!cells.ContainsKey(name))
            {
                //TODO figure out lookup function
                cells.Add(name, new Cell(formula, null));
            }

            // get every variable in the formula, add dependencies such that 
            // "We" (name) depend on those variables, since they are in a formula, in "our" cell
            foreach (string variable in formula.GetVariables())
            {
                dependencyGraph.AddDependency(variable, name);
            }

            return FindAllDependents(name);

        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dependencyGraph.GetDependents(name);
        }

        /// <summary>
        /// Cell class for private use only, representation of a single cell in excel 
        /// </summary>
        private class Cell
        {
            public object contents { get; set; } // contents of the cell
            private Func<string, double> lookup;


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