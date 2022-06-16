#include <iostream>
#include <fstream>
#include <string>



class ByteReader
{
private:
	std::ifstream* file;
public:
	ByteReader(std::ifstream *input)
	{
		file = input;
	};
	std::string readAscii(int len = -1)
	{
		std::string str("");
		if (len >= 0)
		{
			for (size_t i = 0; i < len; i++)
			{
				char c;
				(*file).read(&c, 1);
				str += c;
			}
		}
		else
		{
			while (true)
			{
				char c;
				(*file).read(&c, 1);
				if (c == 0)break;
				str += c;
			}
		}
		return str;
	}
	std::wstring readUnicode(int len = -1)
	{
		std::wstring str(L"");
		if (len >= 0)
		{
			for (size_t i = 0; i < len; i++)
			{
				wchar_t c;
				(*file).read((char*)&c, 2);
				str += c;
			}
		}
		else
		{
			while (true)
			{
				wchar_t c;
				(*file).read((char*)&c, 2);
				if (c == 0)break;
				str += c;
			}
		}
		return str;
	}
	void seek(int pos)
	{
		(*file).seekg(pos);
	}
	void skip(int len)
	{
		(*file).seekg(len, std::ios::cur);
	}
	int readInt32()
	{
		int num;
		(*file).read((char*)(&num), 4);
		return num;
	}
	short readInt16()
	{
		short num;
		(*file).read((char*)(&num), 2);
		return num;
	}

	int tell()
	{
		return (*file).tellg();
	}
	bool check(int size)
	{
		return file->left >= size;
	}
};









int main(int argc, char* argv)
{
	std::ifstream file("fnaf.exe", std::ios::binary);
	ByteReader reader(&file);
	//if (file.g ood())std::cout << "Opened file\n";

	reader.readAscii(2); //MZ
	reader.seek(60);
	auto hdrOffset = reader.readInt16();
	reader.seek(hdrOffset);
	reader.readAscii(2); //PE
	reader.skip(4);
	short numOfSections = reader.readInt16();
	reader.skip(240);
	int possition = 0;
	for (size_t i = 0; i < numOfSections; i++)
	{
		int entry = reader.tell();
		auto sectionName = reader.readAscii(); //sectionName
		if (sectionName == ".extra")
		{
			reader.seek(entry + 20);
			possition = reader.readInt32();
			break;
		}
		if (i >= numOfSections - 1)
		{
			reader.seek(entry + 16);
			int size = reader.readInt32();
			int address = reader.readInt32();
			possition = address + size;
			break;
		}
		reader.seek(entry + 40);
	}
	reader.seek(possition);
	int start = reader.tell();
	reader.skip(8);
	int headerSize = reader.readInt32();
	int dataSize = reader.readInt32();
	reader.seek(start + dataSize - 32);
	reader.readAscii(4); //PAMU header
	reader.seek(start + 28);


	int count = reader.readInt32();
	int offset = reader.tell();
	for (size_t i = 0; i < count; i++)
	{
		int nameLen = reader.readInt16();
		std::wstring namee = reader.readUnicode(nameLen);
		std::wcout << namee << std::endl;
		reader.readInt32();//bingo
		int dataLen = reader.readInt32();
		reader.skip(dataLen);
	}

	//CCN

	reader.readAscii(4); //PAMU header, once again

	int runtimeVersion = reader.readInt16();
	int runtimeSubversion = reader.readInt16();
	int productVersion = reader.readInt32();
	int productBuild = reader.readInt32();
	std::cout << "Game build number: " << productBuild << std::endl;
	while (true)
	{
		int chunkId = reader.readInt16();
		int chunkFlag = reader.readInt16();
		int chunkSize = reader.readInt32();
		if (chunkId == 26214)
		{

			int imageCount = reader.readInt32();
			std::cout << "Imagebank Found. " << imageCount << " images" << std::endl;
			for (size_t imgNum = 0; imgNum < imageCount; imgNum++)
			{
				int imgHandle = reader.readInt32();

			}









			break;
		}
		else reader.skip(chunkSize);
	}





}