using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FreeFall
{

    /// <summary>
    /// For loading data from LG .RES files used in SSHOCK and TNOVA.
    /// </summary>
    public class Resloader : Loader
    {

        public struct Chunk  //this needs to move into a table that supports subdirs
        {
            public int chunkUnpackedLength;
            public int chunkCompressionType;//compression type
            public int chunkPackedLength;
            public int chunkContentType;
            public byte[] data;    
        };

        public static bool LoadChunk(byte[] archive_ark, int chunkNo, out Chunk data_ark)
        {
            long blockAddress = 0;    //  int chunkId;
                                      //long chunkUnpackedLength=0;
                                      //int chunkType=0;//compression type
                                      // long chunkPackedLength=0;
                                      //  long chunkContentType;
            data_ark.chunkPackedLength = 0;
            data_ark.chunkUnpackedLength = 0;
            data_ark.chunkContentType = 0;
            data_ark.chunkCompressionType = 0;
            //get the level info data from the archive
            blockAddress = getResBlockAddress(chunkNo, archive_ark, ref data_ark.chunkPackedLength, ref data_ark.chunkUnpackedLength, ref data_ark.chunkCompressionType, ref data_ark.chunkContentType);
            if (blockAddress == -1)
            {
                data_ark.data = new byte[1];
                return false;
            }
            data_ark.data = new byte[data_ark.chunkUnpackedLength];
            LoadResChunk(blockAddress, data_ark.chunkCompressionType, archive_ark, ref data_ark.data, data_ark.chunkPackedLength, data_ark.chunkUnpackedLength);
            return true;
        }

        static long LoadResChunk(long AddressOfBlockStart, int chunkType, byte[] archive_ark, ref byte[] OutputChunk, long chunkPackedLength, long chunkUnpackedLength)
        {
            //Util to return an uncompressed shock block. Will use this for all future lookups and replace old ones

            //Find the address of the block. This will also return the file size.
            //AddressOfBlockStart = getShockBlockAddress(ChunkNo,archive_ark,&chunkPackedLength,&chunkUnpackedLength,&chunkType);   
            if (AddressOfBlockStart == -1) { return -1; }

            //if (chunkType ==1)
            switch (chunkType & 0x3)
            {
                case 0://Flat uncompressed
                    {
                        for (long k = 0; k < chunkUnpackedLength; k++)
                        {
                            OutputChunk[k] = archive_ark[AddressOfBlockStart + k];
                        }
                        return chunkUnpackedLength;
                    }
                case 1://flat Compressed
                    {
                        byte[] temp_ark = new byte[chunkPackedLength];
                        for (long k = 0; k < chunkPackedLength; k++)
                        {
                            temp_ark[k] = archive_ark[AddressOfBlockStart + k];
                        }

                        unpack_data(temp_ark, ref OutputChunk, chunkUnpackedLength);
                        return chunkUnpackedLength;
                    }

                case 3://Subdir compressed
                    {
                        //uncompress the sub chunks
                        int NoOfEntries = (int)getAt(archive_ark, AddressOfBlockStart, 16);
                        int SubDirHeaderLength = (NoOfEntries + 1) * 4 + 2;
                        byte[] temp_ark = new byte[chunkPackedLength];
                        byte[] tmpchunk = new byte[chunkUnpackedLength];
                        for (long k = 0; k < chunkPackedLength; k++)
                        {
                            temp_ark[k] = archive_ark[AddressOfBlockStart + k + SubDirHeaderLength];
                        }
                        unpack_data(pack: temp_ark, unpack: ref tmpchunk, unpacksize: chunkUnpackedLength);
                        //Merge my subdir and uncompressed subdir data back together.
                        for (long k = 0; k < SubDirHeaderLength; k++)
                        {//Subdir
                            OutputChunk[k] = archive_ark[AddressOfBlockStart + k];
                        }
                        for (long k = SubDirHeaderLength; k < chunkUnpackedLength; k++)
                        {//Subdir
                            OutputChunk[k] = tmpchunk[k - SubDirHeaderLength];
                        }
                        return chunkUnpackedLength;
                    }
                case 2://Subdir uncompressed, return the subchunk data to be loaded seperately.
                default://Uncompressed. 
                    {                        
                        for (long k = 0; k < chunkUnpackedLength; k++)
                        {
                            OutputChunk[k] = archive_ark[AddressOfBlockStart + k];
                        }
                        return chunkUnpackedLength;
                    }
            }
        }





        /* The following procedure comes straight from Jim Cameron
       http://madeira.physiol.ucl.ac.uk/people/jim
       Specifically, this procedure can be found on his "Unofficial System Shock
       Specifications" page at
       http://madeira.physiol.ucl.ac.uk/people/jim/games/ss-res.txt
    */
        public static void unpack_data(byte[] pack, ref byte[] unpack, long unpacksize)
        {

            //unsigned char *byteptr;
            long byteptr = 0;
            //unsigned char *exptr;
            long exptr = 0;
            int word = 0;  /* initialise to stop "might be used before set" */
            int nbits;
            /*    int type; */
            int val;

            int ntokens = 0;
            long[] offs_token = new long[16384];
            int[] len_token = new int[16384];
            int[] org_token = new int[16384];

            int i;

            for (i = 0; i < 16384; ++i)
            {
                len_token[i] = 1;
                org_token[i] = -1;
            }
            //memset (unpack, 0, unpacksize); Probably not needed here. Initialises the unpacked array with zeros.


            byteptr = 0; //pack;
            exptr = 0;//unpack;
            nbits = 0;

            // while (exptr - unpack < unpacksize)
            while (exptr < unpacksize)
            {

                while (nbits < 14)
                {
                    word = (word << 8) + pack[byteptr++]; //   *byteptr++;
                    nbits += 8;
                }

                nbits -= 14;
                val = (word >> nbits) & 0x3FFF;
                if (val == 0x3FFF)
                {
                    break;
                }

                if (val == 0x3FFE)
                {
                    for (i = 0; i < 16384; ++i)
                    {
                        len_token[i] = 1;
                        org_token[i] = -1;
                    }
                    ntokens = 0;
                    continue;
                }

                if (ntokens < 16384)
                {
                    //offs_token [ntokens] = exptr - unpack;
                    offs_token[ntokens] = exptr;// - unpack;
                    if (val >= 0x100)
                    {
                        org_token[ntokens] = val - 0x100;
                    }
                }
                ++ntokens;

                if (val < 0x100)
                {
                    // *exptr++ = val;
                    unpack[exptr++] = (byte)val;
                }
                else
                {
                    val -= 0x100;

                    if (len_token[val] == 1)
                    {
                        if (org_token[val] != -1)
                        {
                            len_token[val] += len_token[org_token[val]];
                        }
                        else
                        {
                            len_token[val] += 1;
                        }
                    }
                    for (i = 0; i < len_token[val]; ++i)
                    {
                        if (i + offs_token[val] < unpacksize)
                        {
                            // *exptr++ = unpack[i + offs_token[val]];
                            unpack[exptr++] = unpack[i + offs_token[val]];
                        }
                        else
                        {
                            //Debug.Log("Oh shit");
                        }
                    }
                }
            }
        }

        // End of Jim Cameron's procedure

        /// <summary>
        /// Finds the target block in the .res file.
        /// </summary>
        /// <param name="TargetBlockNo"></param>
        /// <param name="tmp_ark"></param>
        /// <param name="chunkPackedLength"></param>
        /// <param name="chunkUnpackedLength"></param>
        /// <param name="chunkCompressionType"></param>
        /// <param name="chunkContentType"></param>
        /// <returns></returns>
        static int getResBlockAddress(long TargetBlockNo, byte[] tmp_ark, ref int chunkPackedLength, ref int chunkUnpackedLength, ref int chunkCompressionType, ref int chunkContentType)
        {
            //Finds the address of the block based on the directory block no.
            //Justs loops through until it finds a match.
            bool blnBlockFound = false;
            var DirectoryAddress = getAt(tmp_ark, 124, 32);
            var NoOfChunks = (int)getAt(tmp_ark, DirectoryAddress, 16);
            var firstChunkAddress = (int)getAt(tmp_ark, DirectoryAddress + 2, 32);

            var address_pointer = DirectoryAddress + 6;
            var AddressOfBlockStart = firstChunkAddress;
            for (int k = 0; k < NoOfChunks; k++)
            {
                int chunkId = (int)getAt(tmp_ark, address_pointer, 16);
                chunkUnpackedLength = (int)getAt(tmp_ark, address_pointer + 2, 24);
                chunkCompressionType = (int)getAt(tmp_ark, address_pointer + 5, 8);  //Compression.
                chunkPackedLength = (int)getAt(tmp_ark, address_pointer + 6, 24);
                chunkContentType = (short)getAt(tmp_ark, address_pointer + 9, 8);

                //Debug.Log(chunkId + " of type " + chunkContentType + " compress=" + chunkCompressionType + " packed= " + chunkPackedLength + " unpacked=" + chunkUnpackedLength + " at file address " + AddressOfBlockStart);

                if (chunkId == TargetBlockNo)
                {
                    blnBlockFound = true;
                    address_pointer = 0;
                    break;
                }


                AddressOfBlockStart = AddressOfBlockStart + chunkPackedLength;
                if ((AddressOfBlockStart % 4) != 0)
                    AddressOfBlockStart = AddressOfBlockStart + 4 - (AddressOfBlockStart % 4); // chunk offsets always fall on 4-byte boundaries


                address_pointer = address_pointer + 10;
            }

            if (!blnBlockFound)
            {
                //printf("Level not found"); 
                return -1;
            }
            else
            {
                return AddressOfBlockStart;
            }
        }


        /// <summary>
        /// Browses the content of a .res file and lists what it contains.
        /// </summary>
        /// <param name="resfile"></param>
        public static List<int> EnumerateResFile(string resfile, out byte[] data)
        {            
            var result = new List<int>();
            if (System.IO.File.Exists(resfile))
            {
                var tmp_ark = File.ReadAllBytes(resfile);
                data = tmp_ark;
                if (tmp_ark.Length > 124)
                {
                    var DirectoryAddress = getAt(tmp_ark, 124, 32);
                    var NoOfChunks = (int)getAt(tmp_ark, DirectoryAddress, 16);
                    var firstChunkAddress = (int)getAt(tmp_ark, DirectoryAddress + 2, 32);

                    var address_pointer = DirectoryAddress + 6;
                    var AddressOfBlockStart = firstChunkAddress;

                    for (int k = 0; k < NoOfChunks; k++)
                    {
                        int chunkId = (int)getAt(tmp_ark, address_pointer, 16);
                        result.Add(chunkId);
                        var chunkUnpackedLength = (int)getAt(tmp_ark, address_pointer + 2, 24);
                        var chunkCompressionType = (int)getAt(tmp_ark, address_pointer + 5, 8);  //Compression.
                        var chunkPackedLength = (int)getAt(tmp_ark, address_pointer + 6, 24);
                        var chunkContentType = (short)getAt(tmp_ark, address_pointer + 9, 8);
                        //if ((chunkUnpackedLength == 1024) ||  (chunkUnpackedLength == 768))
                        // {
                        //var diff = AddressOfBlockStart - 0x8D78D;
                        Debug.Print($"{resfile} has {chunkId} of type {chunkContentType} compression={chunkCompressionType} packedlength={chunkPackedLength} unpacked={chunkUnpackedLength} at file address 0x{AddressOfBlockStart.ToString("x").ToUpper()}   Chunk {chunkId} {System.IO.Path.GetFileName(resfile)}");

                        // }                        

                        AddressOfBlockStart = AddressOfBlockStart + chunkPackedLength;
                        if ((AddressOfBlockStart % 4) != 0)
                            AddressOfBlockStart = AddressOfBlockStart + 4 - (AddressOfBlockStart % 4); // chunk offsets always fall on 4-byte boundaries


                        address_pointer = address_pointer + 10;
                    }
                }
            }
            else
            {
                data = null;
            }
            return result;
        }

        public static bool FindDataInRes(string ToFind, string ResFile, int datalen)
        {
            byte[] finddata; byte[] resdata;
            if (ReadStreamFile(Path: ToFind, buffer: out finddata) && ReadStreamFile(Path: ResFile, buffer: out resdata))
            {
                var offset = matchdata(ToFind: finddata, ToSearch: resdata, datalen: datalen);
                if (offset>=0)
                {
                    Debug.Print($"match found in {ResFile} at {offset.ToString("x").ToUpper()}");
                }
            }
            return false;
        }

        public static int matchdata (byte[] ToFind, byte[]ToSearch, int datalen)
        {
            for (int o = 0; o <= ToSearch.GetUpperBound(0); o++)
            {
                for (int i = 0; i<datalen;i++)
                {
                    if (o+i>ToSearch.GetUpperBound(0))
                    {
                        return -1;
                    }
                    else
                    {
                        if (ToFind[i] != ToSearch[o+i])
                        {
                            break;
                        }
                        else
                        {
                            //data matching so far.
                            if (i+1 == datalen)
                            {
                                //all bytes scanned and matching.
                                Debug.Print($"match found at offset 0x{o.ToString("x").ToUpper()}");
                                return o;
                            }
                        }
                    }
                }
            }
            return -1;
        }

    }//end class
}//end namespace