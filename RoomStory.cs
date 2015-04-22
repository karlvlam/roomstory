/*
 * Created by SharpDevelop.
 * User: karl
 * Date: 17/7/2007
 * Time: 15:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace roomGift
{
	/// <summary>
	/// Description of RoomStory.
	/// </summary>
	public class RoomStory
	{
		/* [tag]filename
		 * [tag]md5(filename)
		 * [tag]description
		 * [tag]md5(description)
		 * [tag]md5(org file)
		 * [tag]md5(encrypted file)
		 * [tag]orgfile */
		private int headerSize = 4096;		
		private long blockSize = 1048576 * 4;
		private string tag;
		private string tagCrypt;
		private byte[] bTagCrypt;		
		private byte[] bXOR;
		
		public string fileFull;
		
		public string fileDir;
		public string fileDest;
		public string fileDesc;
		public string fileRoom;
		public string fileExt = ".roomstory";
		public string md5Hash;
		public string md5Dest;
		public string md5Room;
		
		//public string[] headers;
		//public string header;
		public string roomstory;
		public string headFile;
		public string headDesc;
		public string headSrcHash;
		public string headCryptHash;	
		public string headHash;
				
		
		public RoomStory()
		{
			tag = md5String("[rOoMsToRy]");
			tagCrypt = md5String(tag);
			bXOR = new byte[blockSize];
			bTagCrypt = new byte[headerSize];
			fillBytes(bTagCrypt, tagCrypt);
			
			fileDesc = "Description";
			md5Dest = "";
			md5Room = "Karl.roomstory";
			
			
						
		}
		
		public bool load(string file){
			
			try{
				fileFull = file;
				fileDir = Path.GetDirectoryName(fileFull);
				fileDest = Path.GetFileName(fileFull);
				fileRoom = Path.GetFileNameWithoutExtension(fileFull);
				
				md5Dest = md5File(fileFull);
			}catch (Exception caught){
				return false;
			}
			return true;
			
		}
		
		public bool loadRoom(string file){
			this.roomstory = file;
			return checkFormat(this.roomstory);
		}
		
		public bool checkFormat(string file){
			
			byte[] bHead = new byte[this.headerSize];
			
			FileStream fr = new FileStream(file, FileMode.Open);
			fr.Read(bHead, 0, bHead.Length);
			fr.Close();
			
			// decrypt the header (XOR)			
			for (int i=0; i < bHead.Length; i++){
				bHead[i] = (byte)((int)bHead[i] ^ (int)this.bTagCrypt[i]);
			}
			
			string headerFull = BytesToString(bHead, true);
			string headerHash = headerFull.Substring(0, 32);
			string header = headerFull.Substring(32);
			// trim the header tail ( the bytes of 0s)
			header = header.Substring(0, header.LastIndexOf(this.tag) + this.tag.Length);
			
			if(!md5String(header).Equals(headerHash)) return false;
		/* md5(header)
		 * [tag]filename
		 * [tag]md5(filename)
		 * [tag]description
		 * [tag]md5(description)
		 * [tag]md5(org file)
		 * [tag]md5(encrypted file)
		 * [tag]md5(hash)
		 * [tag]orgfile 
		 * */
			
			// get filename
			int offset = 32;
			int offnext = header.IndexOf(this.tag, offset);
			this.headFile = header.Substring(offset, offnext - offset);
			//get md5 filename
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			//get description
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			this.headDesc = header.Substring(offset, offnext - offset);			
			//get md5 description
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			//get md5 source file
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			this.headSrcHash = header.Substring(offset, offnext - offset);	
			//get md5 crypted file
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			this.headCryptHash = header.Substring(offset, offnext - offset);
			//get md5 crypted file
			offset = offnext + this.tag.Length;
			offnext = header.IndexOf(this.tag, offset);
			this.headHash = header.Substring(offset, offnext - offset);
			
			//check the crypted file
			long fLen = 0;
			byte[] f;
			fr = new FileStream(file, FileMode.Open);
			fLen = fr.Length - this.headerSize;
			f = new byte[fLen];
			
			double copyRatio = fLen/this.blockSize;
			int copyTime = (int)Math.Floor(copyRatio);
			int copyLeft = (int)(fLen - (copyTime * blockSize));
			byte[] bBlock = new byte[blockSize];
			byte[] bLeft = new byte[copyLeft];
			
			
			for(int i = 0; i < this.headerSize; i++){
				fr.ReadByte();
			}
			
			// copy the 1st to n-1 blocks
			offset = 0;
			for(int i=0; i < copyTime; i++){
				
				for(int j=0; j < blockSize; j++){
				
					bBlock[j] = (byte)fr.ReadByte();
					
				}
				bBlock.CopyTo(f, offset);
				
				offset += (int)blockSize;
			}
			
			// copy the last block
			
			for(int i=0; i < copyLeft; i++){
					bLeft[i] = (byte)fr.ReadByte();
					
			}
			bLeft.CopyTo(f, offset);		
			
			fr.Close();
			//this.header = md5Bytes(f);
			if(!md5Bytes(f).Equals(this.headCryptHash)) return false;
						
			
			return true;
		}
		
		private string genHeader(){
		/* md5(header)
		 * [tag]filename
		 * [tag]md5(filename)
		 * [tag]description
		 * [tag]md5(description)
		 * [tag]md5(org file)
		 * [tag]md5(encrypted file)
		 * [tag]md5(hash)
		 * [tag]orgfile */
			string header = "";
			header += tag + this.fileDest;
			header += tag + md5String(this.fileDest);
			header += tag + this.fileDesc;
			header += tag + md5String(this.fileDesc);
			header += tag + this.md5Dest;
			header += tag + this.md5Room;
			header += tag + this.md5Hash;
			header += tag;
			
			// finally add the md5 hash at the beginning
			header = md5String(header) + header;
			
			
			return header;
		}
		
		public bool makeRoom(){
			string sep = "" + Path.DirectorySeparatorChar;
			// template file for md5 hashing, ...
			string tmpFile = this.fileDir + sep + Path.GetRandomFileName();
			// Roomstory file location
			string roomFile = this.fileDir + sep + this.fileRoom + this.fileExt;
			// hash code for the file (random)
			string hash = md5String(Path.GetRandomFileName() + Path.GetRandomFileName());
			// encrypt the file to the template file
			encryptFile(tmpFile, hash);
			// get the md5 hash from the template file
			this.md5Room = md5File(tmpFile);
			this.md5Hash = hash;
			
			// get the full header
			string header = genHeader();
			
			// make the roomstoy file
			makeRoomFinal(header, tmpFile, roomFile);
			
			return true;
		}
		
		private bool makeRoomFinal(string header, string tmpFile, string roomFile){
			
			// prepare the 4k header
			
			byte[] bHeader = StringToBytes(header, true);
			byte[] bHeadFull = new byte[this.headerSize];
			bHeader.CopyTo(bHeadFull, 0);
			
			// encrypt the header (XOR)			
			for (int i=0; i < bHeadFull.Length; i++){
				bHeadFull[i] = (byte)((int)bHeadFull[i] ^ (int)this.bTagCrypt[i]);
			}
			
			
			// open file streams
			FileStream fr = new FileStream(tmpFile, FileMode.Open);
			FileStream fw = new FileStream(roomFile, FileMode.Create);
			
			long fileSize = fr.Length;	//get file size
			long blockSize = this.blockSize;	// set the block size (4M)
			double copyRatio = fileSize/blockSize;
			int copyTime = (int)Math.Floor(copyRatio);
			int copyLeft = (int)(fileSize - (copyTime * blockSize));
			
			byte[] bBlock = new byte[blockSize];			
			byte[] bLeft = new byte[copyLeft];
			
			// write the header at beginning
			fw.Write(bHeadFull, 0,bHeadFull.Length);
			
			// copy the 1st to n-1 blocks
			for(int i=0; i < copyTime; i++){
				
				for(int j=0; j < blockSize; j++){
				
					bBlock[j] = (byte)fr.ReadByte();					
				}
				fw.Write(bBlock, 0, bBlock.Length);
				
			}
			
			// copy the last block
			
			for(int i=0; i < copyLeft; i++){
					bLeft[i] = (byte)fr.ReadByte();					
			}
			fw.Write(bLeft, 0, bLeft.Length);	
			
			
			fr.Close();
			fw.Close();
			
			File.Delete(tmpFile);
			
			return true;
		}
		public bool restore(){
			
			return decryptFile();
		}
		
		private bool decryptFile(){
			string hash = this.headHash;
			fillBytes(this.bXOR, hash);
			FileStream fr = new FileStream(this.roomstory, FileMode.Open);
			FileStream fw = new FileStream(this.headFile, FileMode.Create);
			
			long fileSize = fr.Length - this.headerSize;	//get file size
			long blockSize = this.blockSize;	// set the block size (4M)
			double copyRatio = fileSize/blockSize;
			int copyTime = (int)Math.Floor(copyRatio);
			int copyLeft = (int)(fileSize - (copyTime * blockSize));
			
			byte[] bBlock = new byte[blockSize];			
			byte[] bLeft = new byte[copyLeft];
			
			// skip header
			for(int i=0; i < this.headerSize; i++){
				fr.ReadByte();
			}
			
			// copy the 1st to n-1 blocks
			for(int i=0; i < copyTime; i++){
				
				for(int j=0; j < blockSize; j++){
				
					bBlock[j] = (byte)fr.ReadByte();
					bBlock[j] = (byte)((int)bBlock[j] ^ (int)bXOR[j]);

					
				}
				fw.Write(bBlock, 0, bBlock.Length);
				
			}
			
			// copy the last block
			
			for(int i=0; i < copyLeft; i++){
					bLeft[i] = (byte)fr.ReadByte();
					bLeft[i] = (byte)((int)bLeft[i] ^ (int)bXOR[i]);
			}
			fw.Write(bLeft, 0, bLeft.Length);	
			
			
			fr.Close();
			fw.Close();
			
			return true;
		}
		
		private bool encryptFile(string file, string hash){
			fillBytes(this.bXOR, hash);
			FileStream fr = new FileStream(fileFull, FileMode.Open);
			FileStream fw = new FileStream(file, FileMode.Create);
			
			long fileSize = fr.Length;	//get file size
			long blockSize = this.blockSize;	// set the block size (4M)
			double copyRatio = fileSize/blockSize;
			int copyTime = (int)Math.Floor(copyRatio);
			int copyLeft = (int)(fileSize - (copyTime * blockSize));
			
			byte[] bBlock = new byte[blockSize];			
			byte[] bLeft = new byte[copyLeft];
			
			// copy the 1st to n-1 blocks
			for(int i=0; i < copyTime; i++){
				
				for(int j=0; j < blockSize; j++){
				
					bBlock[j] = (byte)fr.ReadByte();
					bBlock[j] = (byte)((int)bBlock[j] ^ (int)bXOR[j]);

					
				}
				fw.Write(bBlock, 0, bBlock.Length);
				
			}
			
			// copy the last block
			
			for(int i=0; i < copyLeft; i++){
					bLeft[i] = (byte)fr.ReadByte();
					bLeft[i] = (byte)((int)bLeft[i] ^ (int)bXOR[i]);
			}
			fw.Write(bLeft, 0, bLeft.Length);	
			
			
			fr.Close();
			fw.Close();
			
			return true;
		}
		
		private bool fillBytes(byte[] b, string s){
			
			byte[] tmp = StringToBytes(s, false);			
			
			for(int i=0; i < (int)b.Length/tmp.Length; i++){
				tmp.CopyTo(b, i * tmp.Length);
				// in the block, n = md5(n - 1)
				tmp = StringToBytes(md5String(BytesToString(tmp, false)), false);
				
			}
			return true;
		}
		
		public static byte[] StringToBytes(string str, bool utf8){
			System.Text.UTF8Encoding encUTF8 = new System.Text.UTF8Encoding();
			System.Text.ASCIIEncoding encASCII = new System.Text.ASCIIEncoding();
			if(utf8)
				return encUTF8.GetBytes(str);
			else
				return encASCII.GetBytes(str);
    		
    		    		
		}
		
		private static string BytesToString(byte[] b, bool utf8){
			System.Text.UTF8Encoding encUTF8 = new System.Text.UTF8Encoding();
			System.Text.ASCIIEncoding encASCII = new System.Text.ASCIIEncoding();
			byte [] dBytes = b;
			string str;
			if(utf8)
				str = encUTF8.GetString(dBytes);			
			else
				str = encASCII.GetString(dBytes);					
			
			return str;
		}
		
		private string md5File(string file){
			byte[] f;
			FileStream fr = new FileStream(file, FileMode.Open);
			f = new byte[fr.Length];
			
			double copyRatio = f.Length/this.blockSize;
			int copyTime = (int)Math.Floor(copyRatio);
			int copyLeft = (int)(f.Length - (copyTime * blockSize));
			byte[] bBlock = new byte[blockSize];
			byte[] bLeft = new byte[copyLeft];
			
			/*
			for(long i = 0; i < fr.Length; i++){
				f[i] = (byte)fr.ReadByte();
			}
			*/
			// copy the 1st to n-1 blocks
			int offset = 0;
			for(int i=0; i < copyTime; i++){
				
				for(int j=0; j < blockSize; j++){
				
					bBlock[j] = (byte)fr.ReadByte();
					
				}
				bBlock.CopyTo(f, offset);
				
				offset += (int)blockSize;
			}
			
			// copy the last block
			
			for(int i=0; i < copyLeft; i++){
					bLeft[i] = (byte)fr.ReadByte();
					
			}
			bLeft.CopyTo(f, offset);
			fr.Close();
			
	
			return md5Bytes(f);
		}
		
		public static string md5String(string input){
        	// Create a new instance of the MD5CryptoServiceProvider object.
        	MD5 md5Hasher = MD5.Create();

        	// Convert the input string to a byte array and compute the hash.
        	byte[] data = md5Hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(input));

        	// Create a new Stringbuilder to collect the bytes
        	// and create a string.
        	StringBuilder sBuilder = new StringBuilder();

        	// Loop through each byte of the hashed data 
        	// and format each one as a hexadecimal string.
        	for (int i = 0; i < data.Length; i++)
        	{
            	sBuilder.Append(data[i].ToString("x2"));
        	}

        	// Return the hexadecimal string.
        	return sBuilder.ToString();
    	}
		
		private static string md5Bytes(byte[] input){
        	// Create a new instance of the MD5CryptoServiceProvider object.
        	MD5 md5Hasher = MD5.Create();

        	// Convert the input string to a byte array and compute the hash.
        	byte[] data = md5Hasher.ComputeHash(input);

        	// Create a new Stringbuilder to collect the bytes
        	// and create a string.
        	StringBuilder sBuilder = new StringBuilder();

        	// Loop through each byte of the hashed data 
        	// and format each one as a hexadecimal string.
        	for (int i = 0; i < data.Length; i++)
        	{
            	sBuilder.Append(data[i].ToString("x2"));
        	}

        	// Return the hexadecimal string.
        	return sBuilder.ToString();
    	}
	}
}
