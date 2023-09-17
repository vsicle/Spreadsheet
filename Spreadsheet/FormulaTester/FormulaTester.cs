using SpreadsheetUtilities;

namespace FormulaTester
{
    [TestClass]
    public class FormulaTester
    {
        [TestMethod]
        public void TestMethod1()
        {
            Formula a = new Formula("5+z2-    8 * g5");
            Assert.AreEqual(1, 1);
        }
    }
}