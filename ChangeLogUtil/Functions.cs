using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ChangeLogUtil
{
    public static class Functions
    {
        [SqlProcedure]
        public static void GetTextTableDefinition(string schema, string name)
        {
            using (var cn = new SqlConnection("context connection=true"))
            {
                var output = GetTextTableDefinition(cn, schema, name);
                SqlContext.Pipe.Send(output);
            }            
        }

        public static string GetTextTableDefinition(SqlConnection cn, string schema, string name)
        {
            using (var cmd = new SqlCommand("SELECT * FROM [changelog].[TableComponents](@schema, @name", cn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("schema", schema);
                cmd.Parameters.AddWithValue("name", name);

                var data = new DataTable();
                adapter.Fill(data);

                var output = new StringBuilder();

                var byType = data.AsEnumerable().ToLookup(row => row.Field<string>("Type"));
                var byParent = data.AsEnumerable().Where(row => !row.IsNull("Parent")).ToLookup(row => row.Field<string>("Parent"));

                AddItems(output, "Columns:", byType["Column"], (xml, children) => ParseColumnDef(xml, children));
                AddItems(output, "Foreign Keys:", byType["ForeignKey"], (xml, children) => ParseForeignKeyDef(xml, children), byParent);
                AddItems(output, "Indexes:", byType["Index"], (xml, children) => ParseIndexDef(xml, children), byParent);
                AddItems(output, "Check Constraints:", byType["CheckConstraint"], (xml, children) => ParseCheckDef(xml, children));

                return output.ToString();
            }
        }
       
        private static void AddItems(
            StringBuilder output, string heading, 
            IEnumerable<DataRow> componentRows, 
            Func<string, IEnumerable<DataRow>, string> parseDefinition, 
            ILookup<string, DataRow> byParent = null)
        {
            output.AppendLine(heading);

            foreach (var row in componentRows.OrderBy(row => row.Field<int?>("Position")))
            {
                var name = row.Field<string>("Name");
                var childRows = byParent?.Contains(name) ?? false ? byParent[name] : Enumerable.Empty<DataRow>();
                output.AppendLine($"  {name}: {parseDefinition(row.Field<string>("Definition"), childRows)}");
            }

            output.AppendLine();
        }

        private static string ParseForeignKeyDef(string xml, IEnumerable<DataRow> childRows)
        {
            throw new NotImplementedException();
        }

        private static string ParseColumnDef(string xml, IEnumerable<DataRow> childRows)
        {
            throw new NotImplementedException();
        }

        private static string ParseIndexDef(string xml, IEnumerable<DataRow> childRows)
        {
            throw new NotImplementedException();
        }

        private static string ParseCheckDef(string xml, IEnumerable<DataRow> childRows)
        {
            throw new NotImplementedException();
        }
    }
}
