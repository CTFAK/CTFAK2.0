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
	short firstShort = reader.readInt16();
	std::cout << firstShort << std::endl;









}