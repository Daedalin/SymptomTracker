using Aspose.Pdf;
using Daedalin.Core.OperationResult;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    public class PDF_Bll
    {
        #region GeneratingReports
        public static OperatingResult GeneratingReports(List<Day> Days, DateOnly From, DateOnly Till, string Path)
        {
            try
            {
                Document document = new Document();
                document.PageInfo = new PageInfo()
                {
                    Margin = new MarginInfo(15, 15, 15, 30),
                    IsLandscape = true,
                };
                Aspose.Pdf.Page page = document.Pages.Add();
                Table table = new Table();
                table.ColumnWidths = "50 150 250 40 40 100 60 100";
                table.Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.DarkGray));
                table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.DarkGray));

                Row row = table.Rows.Add();
                row.MinRowHeight = 20;
                row.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Center;
                row.DefaultCellTextState = new Aspose.Pdf.Text.TextState() { HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center };
                row.Cells.Add("Type");
                row.Cells.Add("Name");
                row.Cells.Add("Beschreibung");
                row.Cells.Add("Zeit").ColSpan = 2;
                row.Cells.Add("Stärke / Menge");
                row.Cells.Add("Wo");
                row.Cells.Add("Zubereitungsart");

                foreach (var Day in Days.OrderBy(t => t.Date))
                {
                    row = table.Rows.Add();
                    row.Cells.Add("").ColSpan = 8;

                    row = table.Rows.Add();
                    row.MinRowHeight = 15;
                    row.Cells.Add(Day.Date.ToShortDateString()).ColSpan = 8;

                    Aspose.Pdf.Color RowColor = Aspose.Pdf.Color.White;
                    foreach (var Event in Day.Events.OrderBy(e=>e.StartTime))
                    {
                        row = table.Rows.Add();
                        row.BackgroundColor = RowColor;
                        row.MinRowHeight = 15;
                        row.Cells.Add(Enums.EventType.FirstOrDefault(e => e.Key == Event.EventType).Value);
                        row.Cells.Add(Event.Name ?? string.Empty);
                        row.Cells.Add(Event.Description ?? string.Empty); // Es gibt warp
                        if (Event.FullTime)
                            row.Cells.Add("Ganztägig").ColSpan = 2;
                        else
                        {
                            row.Cells.Add($"{Event.StartTime?.Hours:D2}:{Event.StartTime?.Minutes:D2}");
                            row.Cells.Add($"{Event.EndTime?.Hours:D2}:{Event.EndTime?.Minutes:D2}");
                        }
                        row.Cells.Add(Event.Quantity ?? string.Empty);
                        row.Cells.Add(Event.Where ?? string.Empty);
                        row.Cells.Add(Event.PreparationMethod ?? string.Empty);

                        if (RowColor == Aspose.Pdf.Color.White)
                            RowColor = Aspose.Pdf.Color.LightGray;
                        else
                            RowColor = Aspose.Pdf.Color.White;
                    }
                }

                page.Paragraphs.Add(table);

                document.Save(Path);

                return OperatingResult.OK();
            }
            catch (Exception ex)
            {
                return OperatingResult.Fail(ex);
            }
        }
        #endregion
    }
}
