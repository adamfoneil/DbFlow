using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Diagnostics;

namespace ChangeLogUtil.Test
{
    [TestClass]
    public class TableDefs
    {
        [TestMethod]
        public void Hs5Client() => ExecuteHs5("dbo", "Client");

        [TestMethod]
        public void Hs5Transaction() => ExecuteHs5("dbo", "Transaction");

        [TestMethod]
        public void Hs5PatientItem() => ExecuteHs5("dbo", "PatientItem");

        [TestMethod]
        public void BlazorAOEvent() => ExecuteBlazorAO("changelog", "Event");

        private void ExecuteHs5(string schema, string name) => ExecuteInner("Hs5", schema, name);        

        private void ExecuteBlazorAO(string schema, string name) => ExecuteInner("BlazorAO", schema, name);

        private void ExecuteInner(string database, string schema, string name)
        {
            using (var cn = LocalDb.GetConnection(database))
            {
                var output = Functions.GetTextTableDefinition(cn, schema, name);
                Debug.Print(output);
            }
        }

    }
}
