using System.Linq;
using BirdsiteLive.Domain.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Domain.Tests.Tools
{
    [TestClass]
    public class PatternsParserTests
    {
        [TestMethod]
        public void Parse_Simple_Test()
        {
            #region Stubs
            var entry = "test";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("test", result.First());
            #endregion
        }

        [TestMethod]
        public void Parse_Null_Test()
        {
            #region Stubs
            string entry = null;
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(0, result.Length);
            #endregion
        }

        [TestMethod]
        public void Parse_WhiteSpace_Test()
        {
            #region Stubs
            var entry = "  ";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(0, result.Length);
            #endregion
        }

        [TestMethod]
        public void Parse_PipeSeparator_Test()
        {
            #region Stubs
            var entry = "test|test2";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }

        [TestMethod]
        public void Parse_SemicolonSeparator_Test()
        {
            #region Stubs
            var entry = "test;test2";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }

        [TestMethod]
        public void Parse_CommaSeparator_Test()
        {
            #region Stubs
            var entry = "test,test2";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }

        [TestMethod]
        public void Parse_SemicolonSeparator_EmptyEntry_Test()
        {
            #region Stubs
            var entry = "test;test2;";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }

        [TestMethod]
        public void Parse_SemicolonSeparator_WhiteSpace_Test()
        {
            #region Stubs
            var entry = "test; test2";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }

        [TestMethod]
        public void Parse_SemicolonSeparator_EmptyEntry_WhiteSpace_Test()
        {
            #region Stubs
            var entry = "test; test2; ";
            #endregion

            var result = PatternsParser.Parse(entry);

            #region Validations
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("test", result[0]);
            Assert.AreEqual("test2", result[1]);
            #endregion
        }
    }
}