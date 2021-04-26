using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace Activision_Mendeleyev_table.HelperClasses
{
    class Report
    {
        private BinSystem sys;
        private BinSystem sys_ap;
        private System.Drawing.Image DoD;
        private System.Drawing.Image Hsm;
        private System.Drawing.Image Gsm;
        private List<List<double>> points;
        private System.Data.DataTable dat;

        public Report(BinSystem sys, BinSystem sys_ap = null, System.Data.DataTable dat = null, System.Drawing.Image DoD = null, 
            System.Drawing.Image Hsm = null, System.Drawing.Image Gsm = null, List<List<double>> points = null)
        {
            this.sys_ap = sys_ap;
            this.sys = sys;
            this.dat = dat;
            this.DoD = DoD;
            this.Hsm = Hsm;
            this.Gsm = Gsm;
            this.points = points;
        }

        public void CreateReport()
        {
            var doc = new Document();
            doc.SetPageSize(PageSize.A4.Rotate());
            using (var writer = PdfWriter.GetInstance(doc, new FileStream("Report " + sys.ToString() + ".pdf", FileMode.Create)))
            {
                doc.Open();

                doc.Add(CreateParagraph("Система " + sys.ToString()));

                PdfPTable table = new PdfPTable(13);
                table.AddCell("r1, Å");
                table.AddCell("r2, Å");
                table.AddCell("r3, Å");
                table.AddCell("delR/Rmin, Å");
                table.AddCell("n");
                table.AddCell("A");
                table.AddCell("x1");
                table.AddCell("x2");
                table.AddCell("x3");
                table.AddCell("c");
                table.AddCell("Eps1");
                table.AddCell("Eps2");
                table.AddCell("delEps");
                table.AddCell(String.Format("{0:f4}", sys.r_1));
                table.AddCell(String.Format("{0:f4}", sys.r_2));
                table.AddCell(String.Format("{0:f4}", sys.r_3));
                table.AddCell(String.Format("{0:f4}", sys.delR / Math.Min(sys.R(0), sys.R(1))));
                table.AddCell(sys.n.ToString());
                table.AddCell(String.Format("{0:f4}", sys.A));
                table.AddCell(String.Format("{0:f4}", sys.x_1));
                table.AddCell(String.Format("{0:f4}", sys.x_2));
                table.AddCell(String.Format("{0:f4}", sys.x_3));
                table.AddCell(String.Format("{0:f4}", sys.c));
                table.AddCell(String.Format("{0:f4}", sys.Eps(1)));
                table.AddCell(String.Format("{0:f4}", sys.Eps(2)));
                table.AddCell(String.Format("{0:f4}", sys.delEps));
                doc.Add(table);

                doc.Add(CreateParagraph(""));

                if (dat != null)
                {
                    table = new PdfPTable(dat.Columns.Count);

                    foreach (System.Data.DataColumn col in dat.Columns)
                    {
                        table.AddCell((col.Caption == "" || col.Caption == " ") ? col.ColumnName : (col.ColumnName[0] == '=') ? col.Caption + col.ColumnName : col.ColumnName + ", " + col.Caption);
                    }
                    foreach (System.Data.DataRow row in dat.Rows)
                        foreach (var cell in row.ItemArray)
                        {
                            table.AddCell(cell.ToString());
                        }          

                    doc.Add(table);
                    doc.NewPage();
                }

                if (DoD != null)
                {               
                    doc.Add(CreateParagraph("Купол распада"));

                    Image jpg = Image.GetInstance(DoD, System.Drawing.Imaging.ImageFormat.Jpeg);
                    jpg.Alignment = Element.ALIGN_CENTER;
                    doc.Add(jpg);

                    if (sys_ap != null)
                    {
                        table = new PdfPTable(12);
                        table.AddCell("r1, Å");
                        table.AddCell("r2, Å");
                        table.AddCell("r3, Å");
                        table.AddCell("delR/Rmin, Å");
                        table.AddCell("x1");
                        table.AddCell("x2");
                        table.AddCell("x3");
                        table.AddCell("c");
                        table.AddCell("Eps1");
                        table.AddCell("Eps2");
                        table.AddCell("delEps");
                        table.AddCell("Tcr, °С");
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_1));
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_2));
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_3));
                        table.AddCell(String.Format("{0:f4}", sys_ap.delR / Math.Min(sys_ap.R(0), sys_ap.R(1))));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_1));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_2));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_3));
                        table.AddCell(String.Format("{0:f4}", sys_ap.c));
                        table.AddCell(String.Format("{0:f4}", sys_ap.Eps(1)));
                        table.AddCell(String.Format("{0:f4}", sys_ap.Eps(2)));
                        table.AddCell(String.Format("{0:f4}", sys_ap.delEps));
                        table.AddCell(String.Format("{0:f4}", sys_ap.Tmax - 273));
                        doc.Add(table);
                    }
                    doc.NewPage();
                }

                if (Hsm != null)
                {
                    doc.Add(CreateParagraph("Термодинамическая функция смешения, Hсм"));

                    Image jpg = Image.GetInstance(Hsm, System.Drawing.Imaging.ImageFormat.Jpeg);
                    jpg.Alignment = Element.ALIGN_CENTER;
                    doc.Add(jpg);

                    if (sys_ap != null)
                    {
                        table = new PdfPTable(10);
                        table.AddCell("r1, Å");
                        table.AddCell("r2, Å");
                        table.AddCell("r3, Å");
                        table.AddCell("x1");
                        table.AddCell("x2");
                        table.AddCell("x3");
                        table.AddCell("c");
                        table.AddCell("Eps1");
                        table.AddCell("Eps2");
                        table.AddCell("delEps");
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_1));
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_2));
                        table.AddCell(String.Format("{0:f4}", sys_ap.r_3));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_1));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_2));
                        table.AddCell(String.Format("{0:f4}", sys_ap.x_3));
                        table.AddCell(String.Format("{0:f4}", sys_ap.c));
                        table.AddCell(String.Format("{0:f4}", sys_ap.Eps(1)));
                        table.AddCell(String.Format("{0:f4}", sys_ap.Eps(2)));
                        table.AddCell(String.Format("{0:f4}", sys_ap.delEps));
                        doc.Add(table);
                    }
                    doc.NewPage();
                }

                if (Gsm != null)
                {
                    doc.Add(CreateParagraph("Свободная энергия Гиббса, Gсм"));

                    Image jpg = Image.GetInstance(Gsm, System.Drawing.Imaging.ImageFormat.Jpeg);
                    jpg.Alignment = Element.ALIGN_CENTER;
                    doc.Add(jpg);

                    //todo
                }

                doc.Close();
            }

        }

        private Paragraph CreateParagraph(string text)
        {
            string ttf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF");
            //Create a base font object making sure to specify IDENTITY-H
            BaseFont bf = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            //Create a specific font object
            iTextSharp.text.Font f = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.NORMAL);

            Paragraph par = new Paragraph(text, f)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20,
                SpacingBefore = 20
            };

            return par;
        }

        public static System.Drawing.Image ResizeImage(int newSize, System.Drawing.Image originalImage)
        {
            if (originalImage.Width <= newSize)
                newSize = originalImage.Width;

            var newHeight = originalImage.Height * newSize / originalImage.Width;

            if (newHeight > newSize)
            {
                newSize = originalImage.Width * newSize / originalImage.Height;
                newHeight = newSize;
            }

            return originalImage.GetThumbnailImage(newSize, newHeight, null, IntPtr.Zero);
        }
    }
}
