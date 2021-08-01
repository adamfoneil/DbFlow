using ChangeLog.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace ChangeLog.Test
{
    [TestClass]
    public class TableDefs
    {
        [TestMethod]
        public void Appointment()
        {
            var xml = GetResource("ChangeLog.Test.Resources.Appointment.xml");
            var renderer = new TableDefXmlRenderer();
            var actual = renderer.AsText(xml);
            var expected = GetResource("ChangeLog.Test.Resources.Appointment.txt");
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
