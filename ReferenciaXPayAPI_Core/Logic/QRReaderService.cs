using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.ImageSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using PDFtoImage;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ReferenciaXPayAPI_Core.Logic
{
    public class QRReaderService
    {
        public QRReaderService()
        {
        }

        /// <summary>
        /// Reads a QR code from a stream containing an image (.png, .jpg, etc.)
        /// </summary>
        public string ReadQRFromImage(Stream imageStream)
        {
            try
            {
                using (var image = Image.Load<Rgba32>(imageStream))
                {
                    var reader = new ZXing.ImageSharp.BarcodeReader<Rgba32>
                    {
                        AutoRotate = true,
                        Options = new ZXing.Common.DecodingOptions
                        {
                            TryHarder = true,
                            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                        }
                    };

                    var result = reader.Decode(image);
                    return result?.Text;
                }
            }
            catch (Exception)
            {
                // Return null if it fails to decode an image or finds no QR
                return null;
            }
        }

        /// <summary>
        /// Renders each page of a PDF as an image and scans for QR codes.
        /// Falls back to extracting embedded images if rendering fails.
        /// </summary>
        public string ReadQRFromPdf(Stream pdfStream)
        {
            // Copy the stream to a byte array so we can reuse it for fallback
            byte[] pdfBytes;
            using (var memStream = new MemoryStream())
            {
                pdfStream.CopyTo(memStream);
                pdfBytes = memStream.ToArray();
            }

            // --- Strategy 1: Render pages as images using PDFium (handles vector QR codes) ---
            try
            {
                int pageCount = Conversion.GetPageCount(pdfBytes);

                for (int i = 0; i < pageCount; i++)
                {
                    try
                    {
                        // Render the page at 300 DPI for good QR readability
                        var renderOptions = new RenderOptions(Dpi: 300);
                        using (var pngStream = new MemoryStream())
                        {
                            Conversion.SavePng(pngStream, pdfBytes, null, i, renderOptions);
                            pngStream.Position = 0;

                            string qrText = ReadQRFromImage(pngStream);
                            if (!string.IsNullOrEmpty(qrText))
                            {
                                return qrText;
                            }
                        }
                    }
                    catch
                    {
                        // If rendering a specific page fails, try the next one
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // PDFium rendering failed entirely, fall through to PdfPig fallback
            }

            // --- Strategy 2: Fallback - extract embedded images with PdfPig ---
            try
            {
                using (var fallbackStream = new MemoryStream(pdfBytes))
                using (var document = PdfDocument.Open(fallbackStream, new ParsingOptions { }))
                {
                    foreach (var page in document.GetPages())
                    {
                        var images = page.GetImages();
                        foreach (var image in images)
                        {
                            try
                            {
                                if (image.TryGetPng(out byte[] pngBytes2))
                                {
                                    using (var ms = new MemoryStream(pngBytes2))
                                    {
                                        string qrText = ReadQRFromImage(ms);
                                        if (!string.IsNullOrEmpty(qrText))
                                        {
                                            return qrText;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return null if PDF is invalid or unreadable
                return null;
            }

            return null; // No QR found with either strategy
        }
    }
}

