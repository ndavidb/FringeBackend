namespace Fringe.Service.Interfaces
{
    public interface IQRCodeService
    {
        public string GenerateQRCodeBase64(string content);
        public byte[] GenerateQRCodeBytes(string content);
        public string GenerateQRCodeBase64(string content, int pixelsPerModule);
        public string GenerateQRCodeBase64WithLogo(string content, byte[] logoBytes);
        public string GenerateStyledQRCodeBase64(string content, string darkColorHex = "#000000", string lightColorHex = "#FFFFFF");
    }
}
