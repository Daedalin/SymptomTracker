using Aspose.Pdf;
using Aspose.Pdf.Text;
using Daedalin.Core.OperationResult;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.BLL
{
    public class PDF_Bll
    {
        #region GeneratingReports
        public static OperatingResult GeneratingReports(List<Day> Days, List<string> ImagePaths, DateOnly From, DateOnly Till, string PDFPath)
        {
            try
            {
                Document document = new Document();
                Aspose.Pdf.Page page1 = document.Pages.Add();
                page1.PageInfo = new PageInfo()
                {
                    Margin = new MarginInfo(15, 15, 15, 30),
                    IsLandscape = true
                };
                var Titel = new TextFragment($"Report von {From.ToLongDateString()} zu zum {Till.ToLongDateString()}");
                Titel.TextState.FontStyle = FontStyles.Bold;
                Titel.TextState.FontSize = 15;
                Titel.Margin = new MarginInfo(0,20,0,0);
                page1.Paragraphs.Add( Titel );
                #region Table
                var ColCount = ImagePaths.Count > 0 ? 9 : 8;
                Table table = new Table();
                table.ColumnWidths = "50 120 250 40 40 100 60 100";
                if (ImagePaths.Count > 0)
                    table.ColumnWidths += " 20";
                table.Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.DarkGray));
                table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.DarkGray));

                Row row = table.Rows.Add();
                row.MinRowHeight = 20;
                row.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Center;
                row.DefaultCellTextState = new TextState() { HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center };
                row.Cells.Add("Type");
                row.Cells.Add("Name");
                row.Cells.Add("Beschreibung");
                row.Cells.Add("Zeit").ColSpan = 2;
                row.Cells.Add("Stärke / Menge");
                row.Cells.Add("Wo");
                row.Cells.Add("Zubereitungsart");
                if (ImagePaths.Count > 0)
                    row.Cells.Add("Hat ein Bild");

                foreach (var Day in Days.OrderBy(t => t.Date))
                {
                    row = table.Rows.Add();
                    row.Cells.Add("").ColSpan = ColCount;

                    row = table.Rows.Add();
                    row.MinRowHeight = 15;
                    row.Cells.Add(Day.Date.ToShortDateString()).ColSpan = ColCount;

                    Aspose.Pdf.Color RowColor = Aspose.Pdf.Color.White;
                    foreach (var Event in Day.Events.OrderBy(e => e.StartTime))
                    {
                        row = table.Rows.Add();
                        row.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
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

                        if (ImagePaths.Count > 0)
                            row.Cells.Add(Event.HasImage ? Event.ID.ToString() : "").Alignment = Aspose.Pdf.HorizontalAlignment.Center;

                        if (RowColor == Aspose.Pdf.Color.White)
                            RowColor = Aspose.Pdf.Color.LightGray;
                        else
                            RowColor = Aspose.Pdf.Color.White;
                    }
                }
                page1.Paragraphs.Add(table);
                #endregion

                Aspose.Pdf.Page page2 = document.Pages.Add();
                page2.PageInfo = new PageInfo() { IsLandscape = false };
                foreach (var ImagePath in ImagePaths)
                {
                    Bitmap img = new Bitmap(ImagePath);
                    var image = new Aspose.Pdf.Image();

                    double scale = 1;
                    if(img.Width > 500)
                        scale = img.Width / 500.0;
                    else if(img.Height > 500)
                        scale = img.Height / 500.0;

                    image.FixWidth = img.Width / scale;
                    image.FixHeight = img.Height / scale;
                    image.File = ImagePath;
                    image.Title = new TextFragment(Path.GetFileName(ImagePath) ?? string.Empty);
                    page2.Paragraphs.Add(image);
                }

                document.Save(PDFPath);

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
