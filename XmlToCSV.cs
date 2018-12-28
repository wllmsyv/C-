/*
 * This design file is part of a larger design which includes a UI that handles input validations to 
 * ensure the existence of files and correct file types.
 * 
 * The purposes of this class is to take a specific type of XML file, strip the useful information out,
 * and store it in csv format. If the user chooses to the do so, it can also be compressed. This application
 * can also decompress files created by this application and restore it to a csv format. The reason for doing
 * this is to reduce the storage space needed to store a large number of these files. By converting from the 
 * original XML format, to a csv and compressing the file, storage needs for the nominal file were reduced by 
 * 97%. However, the tradeoff for compression is the overhead needed to decompress the file when doing mass 
 * file processing.
 * 
 * All of the work to convert the file is done by calling the constructot. This reduces scalability of the application
 * but due to the limited use case, scaling the application up is not likely. 
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace fileconversion
{
    public class XmlToCSV
    {



        private Dictionary<String, String> fileInfo;
        private List<List<String>> labelData;
        private List<String> dataNames;



        public enum conversonType
        {
            csvOnly, compressCSV, decompress
        };



        /*
         * Constructor
         * Dependency: extractData(),printToFile(),compressCSVData(),deCompressCSVData()
         */
        public XmlToCSV(String fileName, conversonType type)
        {

            if (type == conversonType.csvOnly || type == conversonType.compressCSV)
            {
                fileInfo = new Dictionary<String, String>();
                labelData = new List<List<String>>();
                dataNames = new List<String>();
                extractData(fileName);
                String csvString = toCSVString();
                if (type == conversonType.csvOnly)
                {
                    printToFile(csvString, fileName);
                }
                else if (type == conversonType.compressCSV)
                {
                    compressCSVData(csvString, fileName);
                }
            }
            else if(type == conversonType.decompress)
            {
                deCompressCSVData(fileName);
            }

        }



        /*
         * Reads in a specific type of XML file, extracts the useful information
         * and stores is in 3 local variable data structures.
         * 
         * Called by:   constructor
         * Parameter:   String fileName Must be the complete file path name.
         * Return:      None
         * Dependency:  None
         */
        private void extractData(String fileName)
        {

            XmlTextReader rdr = new XmlTextReader(fileName);
            String nodeName = "";
            while (rdr.Read())
            {
                
                switch (rdr.NodeType)
                {
                    case XmlNodeType.Element:
                        nodeName = rdr.Name;
                        break;
                    case XmlNodeType.Text:
                        if(rdr.Value != "")
                        {
                            fileInfo[nodeName] = rdr.Value;
                        }
                        break;
                }
                
                if (rdr.HasAttributes)
                {
                    String dataName = "";
                    while (rdr.MoveToNextAttribute())
                    {
                        if(rdr.Name == "name" && dataNames.Contains(rdr.Value))
                        {
                            dataName = rdr.Value;
                        }
                        else if (rdr.Name == "name")
                        {
                            List<String> temp = new List<String>();
                            labelData.Add(temp);
                            dataNames.Add(rdr.Value);
                            dataName = rdr.Value;
                        }
                        else if (rdr.Name == "value")
                        {
                            labelData.ElementAt(dataNames.IndexOf(dataName)).Add(rdr.Value);
                        }
                      
                    }
                }

            }

        }



        /*
         * Takes the information stored in the 3 local variable data structures and stores it
         * in a string in a csv format. The data is stored in rows rather than columns for faster
         * traversal when reading with another application.
         * 
         * Called by:   constructor
         * Parameter:   None
         * Return:      String which contains all the information from the original file.
         * Dependency:  No other functions are called, however it is a assumed that extractData() was called
         * first which populated the 3 data structures need to create the csv string.
         */
        private String toCSVString()
        {
            StringBuilder csvFileData = new StringBuilder();
            char carriageReturn = (char)0xD;
            char comma = (char)0x2C;
            
            foreach (KeyValuePair<String, String> kvp in fileInfo)
            {
                csvFileData.Append(kvp.Key);
                csvFileData.Append(comma);
                csvFileData.Append(kvp.Value);
                csvFileData.Append(carriageReturn);
                
            }
            
            foreach(String label in dataNames)
            {
                csvFileData.Append(label);
                csvFileData.Append(comma);
                csvFileData.Append(String.Join(",", labelData.ElementAt(dataNames.IndexOf(label))));
                csvFileData.Append(carriageReturn);
            }
            return csvFileData.ToString().TrimEnd();
        }



        /*
         * Prints the outputData string to file. assumes the data to be csv so the file path is named
         * accordingly.
         * 
         * Called by:   constructor
         * Parameter:   String outputData, String of information assumed to be in csv format.
         *              String fileName Must be the complete file path name.
         * Return:      void
         * Dependency:  None
         */
        private void printToFile(String outputData, String fileName)
        {
            String[] fileNameArray = fileName.Split('.');
            String newFileName = "";
            newFileName = fileNameArray[0] + ".csv";
            StreamWriter file = new StreamWriter(newFileName);
            file.Write(outputData);
            file.Close();
        }



        /*
         * Compresses the csv string and saves the file in the same location as the original 
         * file with a ".cmp" extension.
         * 
         * Called by:   constructor
         * Parameter:   String csvString, String of information assumed to be in csv format.
         *              String fileName Must be the complete file path name.
         * Return:      void
         * Dependency:  None
         */
        private void compressCSVData(String csvString, String fileName)
        {
            // Give the destination file a new extention.
            // Note: This could be an issue if the filenaming convention uses multiple '.'.
            // However, the source files are generated by a machine and this shouldn't be 
            // an issue as long as nobody renames the files.
            String[] fileNameArray = fileName.Split('.');
            String newFileName = fileNameArray[0] + ".cmp";

            // Create new files and store the compressed file information
            byte[] byteArray;
            FileStream fileToCompress = File.Create(newFileName);
            byteArray = Encoding.UTF8.GetBytes(csvString);
            
            using (DeflateStream compressionStream = new DeflateStream(fileToCompress, CompressionMode.Compress))
            {
                compressionStream.Write(byteArray, 0, byteArray.Length);
            }
            fileToCompress.Close();

            // Reopen file and Postpend the compressed file with the file size in (bytes) of the decompressed file
            // so we know how big to make the decompressed array size.
            // Note: There is likely room for optimization by not closing and reopening the file, however the use
            // case suggests that the optimization would have minimal impact.
            fileToCompress = File.Open(newFileName, FileMode.Append);
            Int32 fileSize = csvString.Length;
            byte[] fileSizeArr = Encoding.UTF8.GetBytes(fileSize.ToString());
            byte[] outputToFile = new byte[Encoding.UTF8.GetBytes(fileSize.ToString()).Length + 1];
            Array.Copy(fileSizeArr,0,outputToFile,1, fileSizeArr.Length);
            fileToCompress.Write(outputToFile, 0, outputToFile.Length);
            fileToCompress.Close();
        }



        /*
         * Decompresses a file that was compressed by this application. It is assumed that
         * the file being decompressed is in csv formation.
         * 
         * Called by: constructor
         * Parameter: String fileName Must be the complete file path name.
         * Return: void
         * Dependency: getDecompressedFileSize()
         */
        private void deCompressCSVData(String fileName)
        {

            // Give the destination file a new extention.
            // Note: This could be an issue if the filenaming convention uses multiple '.'.
            // However, the source files are generated by a machine and this shouldn't be 
            // an issue as long as nobody renames the files.
            String[] fileNameArray = fileName.Split('.');
            String newFileName = fileNameArray[0] + ".csv";

            // Get the extracted files size that is appended to the end of the file.
            Int32 outputFileSize = getDecompressedFileSize(fileName);

            if(outputFileSize > 0)
            {
                FileStream inputFile = new FileStream(fileName, FileMode.Open);
                byte[] decompressedBytes = new byte[outputFileSize];
                DeflateStream decompressionStream = new DeflateStream(inputFile, CompressionMode.Decompress);
                decompressionStream.Read(decompressedBytes, 0, outputFileSize);
                inputFile.Close();
                decompressionStream.Close();

                FileStream outputFile = File.Create(newFileName);
                MemoryStream fileBytes = new MemoryStream(decompressedBytes);
                fileBytes.CopyTo(outputFile);
                outputFile.Close();
            }
        }



        /*
         * Opens a file that was compressed for by this application and gets the
         * the number of bytes of the original file which was appended to the end
         * of the compressed file during the compression.
         * 
         * Called by: deCompressCSVData()
         * Parameter: String fileName: Must be the complete file path name.
         * Return: Int32 which indications the size of the file in bytes
         * Return: -1 if there was no information stored.
         * Dependency: None
         */
        private Int32 getDecompressedFileSize(String fileName)
        {

            StringBuilder fileSize = new StringBuilder();
            FileStream file = new FileStream(fileName, FileMode.Open);
            byte[] buffer = new byte[file.Length];
            MemoryStream fileBytes = new MemoryStream(buffer);
            file.CopyTo(fileBytes);
            int index = buffer.Length - 1;
            file.Close();

            // Location Null terminator at the end of the file
            while (index >= 0)
            {
                if(buffer[index] == (byte)0x0)
                {
                    break;
                }
                index--;
            }

            // Start at the location of the null terminator
            // and collect the remaining characters.
            while (index < buffer.Length - 1 && index > 0)
            {
                index++;
                fileSize.Append((char)buffer[index]);
            }

            if(index > 0)
            {
                return Int32.Parse(fileSize.ToString());
            }
            else
            {
                return -1;
            }
            
        }
    }
}
