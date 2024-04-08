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
        public static OperatingResult GeneratingReports(List<Day> Days)
        {
            try
            {
                //Mehere Seiten?
                Document document = new Document();
                Aspose.Pdf.Page page = document.Pages.Add();
                Table table = new Table();
                table.Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));
                table.DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.FromRgb(System.Drawing.Color.LightGray));

                foreach (var Day in Days.OrderBy(t => t.Date))
                {
                    Row row = table.Rows.Add();
                    row.Cells.Add(Day.Date.ToShortDateString()).ColSpan = 4;


                    foreach (var Event in Day.Events)
                    {
                        row = table.Rows.Add();
                        row.Cells.Add(Event.Name);
                        row.Cells.Add(Event.Description); // Es gibt warp
                        if (Event.FullTime)
                            row.Cells.Add("Ganztägig").ColSpan = 2;
                        else
                        {
                            row.Cells.Add(Event.StartTime.ToString());
                            row.Cells.Add(Event.EndTime.ToString());
                        }
                    }
                    row = table.Rows.Add();
                    row.Cells.Add().ColSpan = 4;
                }

                page.Paragraphs.Add(table);


                document.Save($"C:\\Temp\\Generated-PDF-{DateTime.Now.ToShortDateString()}.pdf");

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
