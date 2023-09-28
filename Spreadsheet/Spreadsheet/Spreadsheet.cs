using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
    /// 
    /// Modified and implemented by Vasil Vassilev
    /// Latest Change made on 9/22/2023
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Dictionary<string, Cell> cells;
        private DependencyGraph dependencyGraph;
        protected static Func<string, string> Normalize;
        protected static Func<string, bool> IsValid;


        /// <summary>
        /// Zero argument constructor for making a blank spreadsheet
        /// </summary>
        public Spreadsheet() : base("default")
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
            // default normalizer and IsValid function
            Normalize = s => s;
            IsValid = s => true;
        }

        /// <summary>
        /// 3 argument constructor that takes a normalizer, validator, and versionIdentifier
        /// </summary>
        /// <param name="_normalize">normalizer function, input: string, output:string</param>
        /// <param name="_isValid"></param>
        /// <param name="versionNum"></param>
        public Spreadsheet(Func<string, string> _normalize, Func<string, bool> _isValid, string versionNum) : base(versionNum)
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
            Normalize = _normalize;
            IsValid = _isValid;

        }

        public Spreadsheet(string filePath, Func<string, string> _normalize, Func<string, bool> _isValid, string versionNum) : base(versionNum)
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
            Normalize = _normalize;
            IsValid = _isValid;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.
        /// The return value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            // if name is valid and its cell is assigned, return its contents
            if (IsValidName(name) && cells.ContainsKey(name))
            {
                return cells[name].contents;
            }
            // if its a valid name, but hasn't been assigned return blank string
            else if (IsValidName(name))
            {
                return "";
            }
            // name is invalid if we get to this point
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
                // if a cell is not null or empty then add its name to the return list
                if (!string.IsNullOrEmpty(cells[cellName].contents.ToString()))
                {
                    nonEmptyCellNames.Add(cellName);
                }
            }
            // return the list
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
        protected override IList<string> SetCellContents(string name, double number)
        {
            // check name validity
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            // if cell hasn't been used before assign it directly
            else if (!cells.ContainsKey(name))
            {
                cells.Add(name, new Cell(number));
            }
            else
            // check what was in there before to update dependencies
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
            // Use pre-built method to find all cells dependent on "name", save as list, and return
            List<string> dependentCells = GetCellsToRecalculate(name).ToList();
            return dependentCells;

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
        protected override IList<string> SetCellContents(string name, string text)
        {
            // check or valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            // if cell hasn't been used before, assign it directly
            else if (!cells.ContainsKey(name))
            {
                cells.Add(name, new Cell(text));
            }
            else
            // if cell has been used before, update accordingly and update dependencies if needed
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
            // Use pre-built method to find all cells dependent on "name", save as list, and return
            List<string> dependentCells = GetCellsToRecalculate(name).ToList();
            return dependentCells;

        }


        // IS THIS CLASS NEEDED SINCE CELL TAKES CARE OF FORMULA CREATION
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
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            // check or valid name
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            // if cell hasn't been used before, assign it directly
            else if (!cells.ContainsKey(name))
            {
                cells.Add(name, new Cell(formula));
            }

            // get every variable in the formula, add dependencies such that 
            // "We" (name) depend on those variables, since they are in a formula, in "our" cell
            foreach (string variable in formula.GetVariables())
            {
                dependencyGraph.AddDependency(variable, name);
            }
            // Use pre-built method to find all cells dependent on "name", save as list, and return
            List<string> dependentCells = GetCellsToRecalculate(name).ToList();
            return dependentCells;

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
            // return all dependents (direct) in dependencyGraph of name
            return dependencyGraph.GetDependents(name);
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "Cells" - a data structure containing 0 or more cell entries
        ///           Each cell entry has a field (or key) named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "StringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of StringForm is that string
        ///               - If the contents is a double d, the value of StringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of StringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "Cells": {
        ///     "A1": {
        ///       "StringForm": "5"
        ///     },
        ///     "B3": {
        ///       "StringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            // check name for validity
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }
            // return the value of the cell (should be double, text, or FormulaError)
            else
            {
                return cells[name].value;
            }

        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            // Check if the cell name is valid
            if (!IsValidName(name))
            {
                throw new InvalidNameException();
            }

            // if number, use number constructor
            if (Double.TryParse(content, out double number))
            {
                return SetCellContents(name, number);
            }
            // if formula use formula constructor
            else if (content.StartsWith("="))
            {

                Formula formula = new Formula(content.Substring(1), Normalize, IsValid);
                return SetCellContents(name, formula);

            }
            // if string, use string method
            else
            {
                return SetCellContents(name, content);
            }
        }

        private static double Lookup(string name, Dictionary<string, Cell> cells)
        {
            if (cells.ContainsKey(name))
            {
                // if value is a number, return it, otherwise there is an issue
                if (cells[name].value is double)
                {
                    return (double)cells[name].value;
                }
                else
                {
                    throw new FormulaFormatException("You have a variable that leads to a string being used inside of a formula," +
                                                        " number or other formula must be used");
                }
            }
            else
            {
                throw new FormulaFormatException("You have a variable that leads to a blank cell being used inside of a formula," +
                                                        " number or other formula must be used");
            }
        }


        /// <summary>
        /// Cell class for private use only, representation of a single cell in excel 
        /// Contents should be a string, double, or Formula object
        /// </summary>
        internal class Cell
        {
            private Dictionary<string, Cell> cells;
            public object contents { get; set; } // contents of the cell
            public object value { get; set; } // value of the cell (Evalution of formula, value of variable, string, double)

            /// <summary>
            /// Constructor for Cell containing a number
            /// </summary>
            /// <param name="number">double number to be contained in cell</param>
            public Cell(double number, Dictionary<string, Cell> _cells)
            {
                cells = _cells;
                contents = number;
                value = number;
            }

            // TODO: Change/Update comment
            /// <summary>
            /// Constructor for cell containing a string
            /// </summary>
            /// <param name="text">string text to be contained in cell</param>
            public Cell(string text, Dictionary<string, Cell> _cells)
            {
                cells = _cells;
                contents = text;
                value = text;

            }

            // TODO: Make Evaluate work
            public Cell(Formula formula, Dictionary<string, Cell> _cells)
            {
                cells = _cells;
                contents = formula;
                value = formula.Evaluate(Lookup, cells);
            }
        }
    }
}
