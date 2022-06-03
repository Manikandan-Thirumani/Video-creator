using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Pdf;

namespace CommonLibrary
{
   public class FileConversion:IDisposable
   {
       private Document pdfDocument;
        public void Dispose()
        {
            pdfDocument.Dispose();

        }

        public void pdftoword(string filepath, string outputfile)
        {
            // For complete examples and data files, please go to https://github.com/aspose-pdf/Aspose.PDF-for-.NET
            // The path to the documents directory.

            // Open the source PDF document
             pdfDocument = new Document(filepath);

            // Instantiate DocSaveOptions object
            DocSaveOptions saveOptions = new DocSaveOptions();
            // Specify the output format as DOCX
            saveOptions.Format = DocSaveOptions.DocFormat.DocX;
            // Save document in docx format
            pdfDocument.Save(outputfile, saveOptions);
        }
    }
}
