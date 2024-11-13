using System.IO.Compression;
using System.IO;
using System.Text;

namespace DATN.Client.Service
{
    public class CompressService
    {
        public byte[] Compress(string json)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(jsonBytes, 0, jsonBytes.Length);
                }

                return output.ToArray();
            }
        }
    }
}
