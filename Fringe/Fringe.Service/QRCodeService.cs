using Microsoft.Extensions.Logging;
using QRCoder;


namespace Fringe.Service
{
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
        }

        // Simple QR code generation
        public string GenerateQRCodeBase64(string content)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);

                return Convert.ToBase64String(qrCodeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                throw;
            }
        }

        // Generate QR code with custom size
        public string GenerateQRCodeBase64(string content, int pixelsPerModule)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);

            return Convert.ToBase64String(qrCodeBytes);
        }

        // Generate QR code as byte array
        public byte[] GenerateQRCodeBytes(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }

        // Generate QR code with logo (requires System.Drawing.Common on Windows)
        public string GenerateQRCodeBase64WithLogo(string content, byte[] logoBytes)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            // For cross-platform .NET 8, use base64 without logo
            // Logo support requires platform-specific image libraries
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return Convert.ToBase64String(qrCodeBytes);
        }

        // Generate styled QR code
        public string GenerateStyledQRCodeBase64(string content, string darkColorHex = "#000000", string lightColorHex = "#FFFFFF")
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            // Convert hex colors to RGB bytes
            var darkColor = HexToRgb(darkColorHex);
            var lightColor = HexToRgb(lightColorHex);

            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20, darkColor, lightColor);

            return Convert.ToBase64String(qrCodeBytes);
        }

        private static byte[] HexToRgb(string hexColor)
        {
            hexColor = hexColor.Replace("#", "");

            return new byte[]
            {
                Convert.ToByte(hexColor.Substring(0, 2), 16),
                Convert.ToByte(hexColor.Substring(2, 2), 16),
                Convert.ToByte(hexColor.Substring(4, 2), 16)
            };
        }
    }
}
