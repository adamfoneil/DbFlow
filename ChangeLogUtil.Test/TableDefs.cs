using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;

namespace ChangeLogUtil.Test
{
    [TestClass]
    public class TableDefs
    {
        [TestMethod]
        public void Hs5Client()
        {
            using (var cn = LocalDb.GetConnection("Hs5"))
            {
                var output = Functions.GetTextTableDefinition(cn, "dbo", "Client");
            }
        }
    }
}
