using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;

namespace YummySoupExporter
{

    public class JSONReaderWriter<T> where T : class
    {
        public static T ReadFromString(string input)
        {
            //	Trim off the beginning and ending quotes.  JSON never starts or ends with quotes.
            input = input.Trim().Trim('"');

            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(input)))
                return Read(ms);
        }
        public static T Read(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return Read(stream);
            }
        }

        public static T Read(Stream stream)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
            return jsonSerializer.ReadObject(stream) as T;
        }

        public static string WriteToString(T toBeWritten)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Write(memoryStream, toBeWritten);
                memoryStream.Position = 0;

                StreamReader sr = new StreamReader(memoryStream);
                return sr.ReadToEnd();
            }
        }

        public static void Write(Stream destination, T toBeWritten)
        {
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
            jsonSerializer.WriteObject(destination, toBeWritten);
        }

        public static void Write(string fileName, T toBeWritten)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
            {
                Write(stream, toBeWritten);
            }
        }

        public static T ReadCompressed(Stream destination)
        {
            GZipStream compressed = new GZipStream(destination, CompressionMode.Decompress);
            return Read(compressed);
        }

        public static void WriteCompressed(Stream destination, T toBeWritten)
        {
            GZipStream compressed = new GZipStream(destination, CompressionMode.Compress);
            Write(compressed, toBeWritten);
        }
    }
}
