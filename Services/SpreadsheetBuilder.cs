using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FactoryPlanner.Services
{
    // Emit SpreadsheetML for plan.
    // Has row for each processing step
    // columns are
    // Material | Machine | Num Machines | Material 1  Qty | Material 2 Qty | ... etc 

    internal class SpreadsheetBuilder
    {
        private class Cell
        {
            public Cell() { }
            public Cell(string content, string typeName) 
            { 
                this.content = content;
                this.typeName = typeName;
            }
            public int? index;
            public string content;
            public string typeName;
            public string formula;

            public XElement ToXml()
            {
                var cell = new XElement(xmlns + "Cell",
                    new XElement(xmlns + "Data", new XAttribute(ss + "Type", typeName), content)
                ); ;
                if (formula != null)
                {
                    cell.Add(new XAttribute(ss + "Formula", formula));
                }
                if (index.HasValue)
                {
                    cell.Add(new XAttribute(ss + "Index", index.Value + 1));
                }
                return cell;
            }
        }

        private class Row
        {
            public Row() { }
            public List<Cell> cells = new List<Cell>();
            public void AddCell(string content, string typeName) { cells.Add(new Cell(content, typeName)); }
            public void AddCell(Cell cell) { cells.Add(cell); }

            public XElement ToXml()
            {
                var element = new XElement(xmlns + "Row");
                foreach (var cell in cells)
                {
                    element.Add(cell.ToXml());
                }
                return element;
            }
        }

        private List<Row> rows = new List<Row>();

        private class Column
        {
            public string id;
            public string label;
            public int index;
        }

        public SpreadsheetBuilder()
        {
            doc = XDocument.Parse(EmptyXml);
            rows.Add(new Row());  // pre-load row for labels;
        }

        public void Build(List<ProductionStep> plan) 
        {
            AddLabels();
            BuildRows(plan);
            BuildTotals();
            BuildXml();
        }

        public void Save(string fileName)
        {
            doc.Save(fileName);
        }

        private void BuildXml()
        {
            var table = doc.Root.Descendants(xmlns + "Table").First();
            foreach(var row in rows)
            {
                table.Add(row.ToXml());
            }
        }

        private void AddLabels()
        {
            AddColumn("Material");
            AddColumn("Machine");
            AddColumn("NumMachines");
            AddColumn("");
        }

        private void BuildTotals()
        {
            int rowIndex = rows.Count();
            rows.Add(new Row());  // whitespace
            var row = new Row();
            rows.Add(row);
            for (int i = 4; i < columns.Count; ++i)
            {
                string col = "c" + (columns[i].index + 1).ToString();
                string formula = $"=sum(R2{col}:R{rowIndex}{col})";
                var cell = new Cell("", NumberType)
                {
                    index = columns[i].index,
                    formula = formula
                };
                row.AddCell(cell);
            }
        }

        private void BuildRows(List<ProductionStep> plan)
        {
            foreach (ProductionStep step in plan)
            {
                StepToRow(step);
            }
        }

        private void StepToRow(ProductionStep step)
        {
            var row = new Row();
            rows.Add(row);
            AddFirstColumsOfRow(step, row);
            AddMaterialsToRow(step, row);
        }

        private void AddFirstColumsOfRow(ProductionStep step, Row row)
        {
            row.AddCell(step.TargetMaterial.Name, StringType);
            row.AddCell(step.Recipe.Machine, StringType);
            row.AddCell(step.MachinesRequired.ToString(), NumberType);
        }

        private void AddMaterialsToRow(ProductionStep step, Row row)
        {
            foreach (var material in step.Recipe.Outputs)
            {
                var column = AddColumn(material.Name);
                row.AddCell(MakeFormulaCell(rows.Count(), material.Rate, step.MachinesRequired, column));
            }
            foreach (var material in step.Recipe.Inputs)
            {
                var column = AddColumn(material.Name);
                row.AddCell(MakeFormulaCell(rows.Count(), -material.Rate, step.MachinesRequired, column));
            }
        }

        private Cell MakeFormulaCell(int rowIndex, double rate, double numMachines, Column column)
        {
            return new Cell((rate * numMachines).ToString(), NumberType)
            {
                index = column.index,
                formula = MakeFormula(rowIndex, rate)
            };
        }

        private string MakeFormula(int rowIndex, double rate)
        {
            return $"={rate}*R{rowIndex}C3";
        }

        private List<ProductionStep> plan;

        private List<Column> columns = new List<Column>();

        private Column AddColumn(string label)
        {
            Column column = null;
            if (!MaterialToColumn.TryGetValue(label, out column))
            {
                column = new Column()
                {
                    id = NextColumnId(),
                    label = label,
                    index = columns.Count()
                };
                MaterialToColumn.Add(label, column);
                rows[0].AddCell(label, StringType);
                columns.Add(column);
            }
            return column;
        }

        private string NextColumnId()
        {
            char toChar(int i) { return Convert.ToChar(i + 'A'); }
            int num = columns.Count();
            int mod = num / 26;
            string s = "";
            if (mod > 0)
            {
                s += toChar(mod);
            }
            return s + toChar(num % 26);
        }

        private Dictionary<String, Column> MaterialToColumn = new Dictionary<String, Column>();

        private XDocument doc = new XDocument();

        private const string StringType = "String";
        private const string NumberType = "Number";

        public static readonly XNamespace xmlns = "urn:schemas-microsoft-com:office:spreadsheet";
        public static readonly XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

        private const string EmptyXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<?mso-application progid=\"Excel.Sheet\"?>\r\n" +
            "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">\r\n" +
            "<Worksheet ss:Name=\"Sheet1\">\r\n" +
            "<Table>" +
            "</Table>\r\n </Worksheet>\r\n</Workbook>";
    }
}
