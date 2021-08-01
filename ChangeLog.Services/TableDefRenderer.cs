using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ChangeLog.Services
{
    public class TableDefRenderer
    {
        /// <summary>
        /// converts the XML returned by table function changelog.TableComponents
        /// to a more readable text format for diff display        
        /// </summary>
        public string AsText(string xml)
        {
            var doc = XDocument.Parse("<root>" + xml + "</root>");
            var tableComponents = doc.Root.Elements().Select(ele =>
            {
                var props = ElementDictionary(ele);
                return new TableComponentResult()
                {
                    Type = ele.Attribute("Type").Value,
                    Name = ele.Attribute("Name").Value,
                    Parent = ele.Attribute("Parent")?.Value,
                    Position = int.TryParse(ele.Attribute("Position")?.Value, out int position) ? position : default,
                    Definition = ElementDictionary(ele.Element("Definition"))
                };
            }).ToArray();

            var output = new StringBuilder();

            var byType = tableComponents.ToLookup(item => item.Type);
            var byParent = tableComponents.Where(item => !string.IsNullOrEmpty(item.Parent)).ToLookup(item => item.Parent);

            AddItems(output, "Columns", byType["Column"], (xml, children) => ParseColumnDef(xml, children));
            AddItems(output, "Foreign Keys", byType["ForeignKey"], (xml, children) => ParseForeignKeyDef(xml, children), byParent);
            AddItems(output, "Indexes", byType["Index"], (xml, children) => ParseIndexDef(xml, children), byParent);
            AddItems(output, "Check Constraints:", byType["CheckConstraint"], (xml, children) => ParseCheckDef(xml, children));

            return output.ToString();

        }

        private static void AddItems(
            StringBuilder output, string heading,
            IEnumerable<TableComponentResult> componentRows,
            Func<Dictionary<string, string>, IEnumerable<TableComponentResult>, string> parseDefinition,
            ILookup<string, TableComponentResult> byParent = null)
        {
            output.AppendLine($"{heading} ({componentRows.Count()}):");

            foreach (var row in componentRows.OrderBy(row => row.Position))
            {
                var childRows = byParent?.Contains(row.Name) ?? false ? byParent[row.Name] : Enumerable.Empty<TableComponentResult>();
                output.AppendLine($"  [{row.Name}] {parseDefinition(row.Definition, childRows)}");
            }

            output.AppendLine();
        }

        private static string ParseColumnDef(Dictionary<string, string> properties, IEnumerable<TableComponentResult> childRows)
        {
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

        private static string ParseForeignKeyDef(Dictionary<string, string> properties, IEnumerable<TableComponentResult> childRows)
        {
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

            foreach (var row in childRows.OrderBy(row => row.Position))
            {
                result += $"    {row.Name} => {row.Definition["referencedColumn"]}";
            }

            return result;
        }

        private static string ParseIndexDef(Dictionary<string, string> properties, IEnumerable<TableComponentResult> childRows)
        {
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
                result += $"    {row.Name} {row.Definition["sort"]}";
            }

            return result;
        }

        private static string ParseCheckDef(Dictionary<string, string> properties, IEnumerable<TableComponentResult> childRows)
        {
            return properties["expression"];
        }

        private static Dictionary<string, string> ElementDictionary(XElement element) => element
            .Descendants()
            .Where(ele => !ele.IsEmpty)
            .ToDictionary(ele => ele.Name.LocalName, ele => ele.Value);

        private class TableComponentResult
        {
            public string Type { get; set; }
            public string Parent { get; set; }
            public string Name { get; set; }
            public int? Position { get; set; }
            public Dictionary<string, string> Definition { get; set; }
        }
    }
}
