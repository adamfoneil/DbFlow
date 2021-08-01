using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
            using (var cmd = new SqlCommand("SELECT * FROM [changelog].[TableComponents](@schema, @name)", cn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("schema", schema);
                cmd.Parameters.AddWithValue("name", name);

                var data = new DataTable();
                adapter.Fill(data);

                var output = new StringBuilder();

                var byType = data.AsEnumerable().ToLookup(row => row.Field<string>("Type"));
                var byParent = data.AsEnumerable().Where(row => !row.IsNull("Parent")).ToLookup(row => row.Field<string>("Parent"));

                AddItems(output, "Columns", byType["Column"], (xml, children) => ParseColumnDef(xml, children));
                AddItems(output, "Foreign Keys", byType["ForeignKey"], (xml, children) => ParseForeignKeyDef(xml, children), byParent);
                AddItems(output, "Indexes", byType["Index"], (xml, children) => ParseIndexDef(xml, children), byParent);
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
            output.AppendLine($"{heading} ({componentRows.Count()}):");

            foreach (var row in componentRows.OrderBy(row => row.Field<int?>("Position")))
            {
                var name = row.Field<string>("Name");
                var childRows = byParent?.Contains(name) ?? false ? byParent[name] : Enumerable.Empty<DataRow>();
                output.AppendLine($"  [{name}] {parseDefinition(row.Field<string>("Definition"), childRows)}");
            }

            output.AppendLine();
        }

        private static string ParseColumnDef(string xml, IEnumerable<DataRow> childRows)
        {
            var properties = xml.ToDictionary();

            string result = string.Empty;

            if (properties["computed"].Equals("true"))
            {
                result = $"= {properties["expression"]} ";
            }

            result += properties["type"];

            bool isChar = false;
            if (properties["type"].Contains("char"))
            {
                isChar = true;
                int length = int.Parse(properties["length"]);
                if (length > 0)
                {
                    int divisor = (properties["type"].StartsWith("n")) ? 2 : 1;
                    result += $"({length / divisor})";
                }
                else
                {
                    result += "(max)";
                }
            }
            else if (properties["type"].Equals("decimal"))
            {
                result += $"({properties["precision"]}, {properties["scale"]})";
            }

            if (properties["identity"].Equals("true"))
            {
                result += " identity";
            }

            var nullable = (properties["nullable"].Equals("true")) ? "NULL" : "NOT NULL";
            result += " " + nullable;

            if (isChar)
            {
                result += $" ({properties["collation"]})";
            }

            if (properties.ContainsKey("default"))
            {
                result += $" default = {properties["default"]}";
            }

            return result;
        }

        private static string ParseForeignKeyDef(string xml, IEnumerable<DataRow> childRows)
        {
            var properties = xml.ToDictionary();

            var result = $"=> {properties["referencedSchema"]}.{properties["referencedTable"]}";

            if (properties["cascadeUpdate"].Equals("true"))
            {
                result += " update";
            }

            if (properties["cascadeDelete"].Equals("true"))
            {
                result += "delete";
            }

            result += "\r\n";

            foreach (var row in childRows.OrderBy(row => row.Field<int?>("Position")))
            {
                var columnProps = row.Field<string>("Definition").ToDictionary();
                result += $"    {row.Field<string>("Name")} => {columnProps["referencedColumn"]}";
            }

            return result;
        }

        private static string ParseIndexDef(string xml, IEnumerable<DataRow> childRows)
        {
            var properties = xml.ToDictionary();

            var result =
                (properties["primary"].Equals("true")) ? "primary" :
                (properties["uniqueConstraint"].Equals("true")) ? "unique constraint" :
                (properties["unique"].Equals("true")) ? "unique" :
                string.Empty;

            result += (properties["type"].Equals("clustered")) ? " clustered" : " nonclustered";

            if (properties["disabled"].Equals("true"))
            {
                result += " DISABLED";
            }

            if (properties["ignoreDups"].Equals("true"))
            {
                result += " ignore dups";
            }

            // todo: fill factor + padding

            result += "\r\n";

            foreach (var row in childRows)
            {
                var colProps = row.Field<string>("Definition").ToDictionary();
                result += $"    {row.Field<string>("Name")} {colProps["sort"]}";
            }

            return result;
        }

        private static string ParseCheckDef(string xml, IEnumerable<DataRow> childRows)
        {
            var properties = xml.ToDictionary();

            return properties["expression"];
        }

        private static Dictionary<string, string> ToDictionary(this string xml) =>
            XDocument.Parse(xml)
            .Descendants()
            .Where(ele => !ele.IsEmpty)
            .ToDictionary(ele => ele.Name.LocalName, ele => ele.Value);
    }
}
