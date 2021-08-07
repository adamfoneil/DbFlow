using DbFlow.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace ChangeLog.Test
{
    [TestClass]
    public class TableDefs
    {
        [TestMethod]
        public void Appointment() => TestRenderer("ChangeLog.Test.Resources.Appointment");

        [TestMethod]
        public void Client() => TestRenderer("ChangeLog.Test.Resources.Client");

        private void TestRenderer(string resourceName)
        {
            var xml = GetResource($"{resourceName}.xml");
            var renderer = new TableDefRenderer();
            var actual = renderer.AsText(xml);
            var expected = GetResource($"{resourceName}.txt");
            Assert.IsTrue(actual.Equals(expected));
        }

        private string GetResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }
    }
}
