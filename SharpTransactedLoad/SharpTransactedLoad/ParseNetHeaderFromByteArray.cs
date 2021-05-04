using System;
using System.Collections.Generic;
using System.Linq;

namespace STL
{
    public static class ParseNetHeaderFromByteArray
    {
        public static string ParseArray(byte[] assemblyBytes)
        {
#if (DEBUG)
            Console.WriteLine("[*] Parsing header structure of provided assembly bytes");
#endif
            int architecture = (int)BitConverter.ToUInt16(assemblyBytes, 152);
            int arrayPos;
            if (architecture == 523) // PE64
            {
                arrayPos = (int)BitConverter.ToUInt32(assemblyBytes, 520) - 7680;
#if (DEBUG)
                Console.WriteLine("[*] Assembly architecture identified: PE64");
#endif
            }
            else //PE32
            {
                arrayPos = (int)BitConverter.ToUInt32(assemblyBytes, 528) - 7680;
#if (DEBUG)
                Console.WriteLine("[*] Assembly architecture identified: PE32");
#endif
            }

            //will need this index a few more times for various operations
            int metadataBaseAddress = arrayPos;
            //metadata header is dword/word/word/dword/dword/string/word/word - only interesting value is last one, which contains the # of streams
            arrayPos = arrayPos + 16;
            byte currByte = assemblyBytes[arrayPos];
            while (currByte != 0x00)
            {
                arrayPos++;
                currByte = assemblyBytes[arrayPos];
            }
            arrayPos = arrayPos + 4;
            int streamNumber = (int)BitConverter.ToUInt16(assemblyBytes, arrayPos);
#if (DEBUG)
            Console.WriteLine("[*] Metadata streams identified");
#endif

            //advance from # of streams to start of stream info
            arrayPos = arrayPos + 2;
            List<streamData> allStreams = new List<streamData>();
            //grab stream info for each stream
            for (int i = 0; i < streamNumber; i++)
            {
                int offset = (int)BitConverter.ToUInt32(assemblyBytes, arrayPos);
                int size = (int)BitConverter.ToUInt32(assemblyBytes, arrayPos + 4);
                arrayPos = arrayPos + 8;
                currByte = assemblyBytes[arrayPos];
                int stringStartPos = arrayPos;
                while (currByte != 0x00)
                {
                    arrayPos++;
                    currByte = assemblyBytes[arrayPos];
                }
                streamData singleStream = new streamData(offset, size, System.Text.Encoding.ASCII.GetString(assemblyBytes, stringStartPos, arrayPos - stringStartPos));
#if (DEBUG)
                Console.WriteLine("    --found stream: {0} with offset {1} ", singleStream.name, singleStream.offset);
#endif
                allStreams.Add(singleStream);
                //string vals are padded at the end with null bytes to get them to a valid dword blocksize (%4 == 0), will pad with 4 null if already full dword 
                arrayPos = arrayPos + 4 - (arrayPos % 4);
            }

            //at this point we have grabbed all stream data, next get offset of #~ stream (should always be immediately after stream info, but we're still using the provided value)
            int streamOffset = allStreams.Where(i => i.name == "#~").FirstOrDefault().offset;

            //first 24 bytes of stream are the header, which doesnt do much for us in this case
            streamOffset = streamOffset + 24 + metadataBaseAddress;

            bool foundStringPointer = false;
            byte[] dwordTestBytes = new byte[4];
            int stringOffset = 0;
            //after the header there is a dword for each table in the stream that lists the # of values in the specific table
            //we iterate through these looking for a dword that starts with 0x00, 0x00 as being a potential indicator of a match (although a false positive is possible during this first check)
            while (!foundStringPointer && streamOffset < assemblyBytes.Length - 4)
            {
                Array.Copy(assemblyBytes, streamOffset, dwordTestBytes, 0, 4);
                //potential match
                if (dwordTestBytes[0] == 0x00 && dwordTestBytes[1] == 0x00)
                {
                    //if we think we have a match (looking for 0x00, 0x00 first two bytes in a dword)
                    //we can check following bytes to see if they equate to what should be a known, static value.
                    //NOTE: this relies on the Generation header being zeroed (0x00, 0x00) -- in my testing this was always the case.
                    byte[] validationTestArray = new byte[6];
                    byte[] validationTestBytes = new byte[6] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    Array.Copy(assemblyBytes, streamOffset + 4, validationTestArray, 0, 6);
                    if (validationTestArray.SequenceEqual<byte>(validationTestBytes))
                    {
                        //*should* be a valid match (at least from my testing)
                        foundStringPointer = true;
                        stringOffset = BitConverter.ToUInt16(dwordTestBytes, 2);
#if (DEBUG)
                        Console.WriteLine("[*] Identified PE name offset of {0} in #Strings", BitConverter.ToUInt16(dwordTestBytes, 2));
#endif
                    }
                }
                streamOffset = streamOffset + 4;
            }
            //info we were looking for in #~ is just a number that gives us the offset from the beginning of #Strings where we can go to find the string containing the name of the assembly
            if (foundStringPointer)
            {
                int stringPos = allStreams.Where(i => i.name == "#Strings").FirstOrDefault().offset + stringOffset + metadataBaseAddress;
                List<byte> assemblyName = new List<byte>();
                byte singleByte = assemblyBytes[stringPos];
                while (singleByte != 0x00)
                {
                    assemblyName.Add(singleByte);
                    stringPos = stringPos + 1;
                    singleByte = assemblyBytes[stringPos];
                }
#if (DEBUG)
                Console.WriteLine("[+] Assembly name identified: " + System.Text.Encoding.ASCII.GetString(assemblyName.ToArray()));
#endif
                return System.Text.Encoding.ASCII.GetString(assemblyName.ToArray());
            }
            else
            {
#if (DEBUG)
                Console.WriteLine("[X] Error, unable to identify string offset, may need to manually provide PE name (including file extension)");
#endif
                return null;
            }
        }
    }

    class streamData
    {
        public streamData(int offset, int size, string name)
        {
            this.offset = offset;
            this.size = size;
            this.name = name;
        }
        public int offset { get; set; }
        public int size { get; set; }
        public string name { get; set; }
    }
}
