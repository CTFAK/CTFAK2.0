#include "encryption.h"
#include <string>
#include <string.h>
#include <cstring>
#include "tinf.h"
//#include "tinflate.c"


#ifdef _WIN32
#define DllExport  __declspec(dllexport) 
#else
#define DllExport __attribute__ ((visibility ("default")))
#endif // DEBUG

extern "C" {
	__m128_temp _xmmword;
	vector<uint8_t> _magic_key;

	DllExport char* make_key(char* c_title, char* c_copyright, char* c_project, char magic_char) {
		_xmmword.m128i_u32[0] = 0x0;
		_xmmword.m128i_u32[1] = 0x1;
		_xmmword.m128i_u32[2] = 0x2;
		_xmmword.m128i_u32[3] = 0x3;
		_magic_key.resize(256);

		char* buffer_ptr = (char*)&_magic_key[0];

		size_t s_len = strlen(c_title) + strlen(c_copyright) + strlen(c_project);

		memset(buffer_ptr, 0, 256);
		if (buffer_ptr)
		{

			size_t str_size = 0;
			memcpy(buffer_ptr, c_title, strlen(c_title));
			str_size += strlen(c_title);
			memcpy(buffer_ptr + str_size, c_copyright, strlen(c_copyright));
			str_size += strlen(c_copyright);
			memcpy(buffer_ptr + str_size, c_project, strlen(c_project));

			memset(buffer_ptr + 128, 0, 0x80u);


			size_t v33 = strlen(buffer_ptr);
			uint8_t v35 = magic_char;

			if ((signed int)(v33 + 1) > 0)
			{
				uint8_t v34 = magic_char;

				for (size_t i = 0; i <= v33; i++)

				{
					v34 = (v34 << 7) + (v34 >> 1);
					*buffer_ptr ^= v34;
					v35 += *buffer_ptr++ * ((v34 & 1) + 2);

				}

			}
			*buffer_ptr = v35;

			return (char*)&_magic_key[0];
		}
		return nullptr;
	};
	DllExport char* make_key_combined(char* data, char magic_char) {
		_xmmword.m128i_u32[0] = 0x0;
		_xmmword.m128i_u32[1] = 0x1;
		_xmmword.m128i_u32[2] = 0x2;
		_xmmword.m128i_u32[3] = 0x3;
		_magic_key.resize(256);

		char* buffer_ptr = (char*)&_magic_key[0];

		size_t s_len = strlen(data);

		memset(buffer_ptr, 0, 256);
		if (buffer_ptr)
		{

			size_t str_size = 0;
			memcpy(buffer_ptr, data, strlen(data));
			str_size += strlen(data);


			memset(buffer_ptr + 128, 0, 0x80u);


			size_t v33 = strlen(buffer_ptr);
			uint8_t v35 = magic_char;

			if ((signed int)(v33 + 1) > 0)
			{
				uint8_t v34 = magic_char;

				for (size_t i = 0; i <= v33; i++)

				{
					v34 = (v34 << 7) + (v34 >> 1);
					*buffer_ptr ^= v34;
					v35 += *buffer_ptr++ * ((v34 & 1) + 2);

				}

			}
			*buffer_ptr = v35;

			return (char*)&_magic_key[0];
		}
		return nullptr;
	};
	DllExport char* make_key_w(wchar_t* c_title, wchar_t* c_copyright, wchar_t* c_project, char magic_char) {
		//setlocale(LC_ALL, "Russian");
		//SetConsoleOutputCP(866);
		_xmmword.m128i_u32[0] = 0x0;
		_xmmword.m128i_u32[1] = 0x1;
		_xmmword.m128i_u32[2] = 0x2;
		_xmmword.m128i_u32[3] = 0x3;
		_magic_key.resize(256);

		char* buffer_ptr = (char*)&_magic_key[0];


		size_t s_title = wcslen(c_title) * 2;
		size_t s_copyright = wcslen(c_copyright) * 2;
		size_t s_project = wcslen(c_project) * 2;
		size_t s_len = s_title + s_copyright + s_project;
		wprintf(L"Title last byte is zero? %d\n", (int)(((char*)c_title)[s_title - 1] == 0));
		if (((char*)c_title)[s_title - 1] == 0 && s_title)s_title--;
		if (((char*)c_copyright)[s_copyright - 1] == 0 && s_copyright)s_copyright--;
		if (((char*)c_project)[s_project - 1] == 0 && s_project)s_project--;
		wprintf(L"%s len: %d\n", c_title, s_title);
		wprintf(L"%s len: %d\n", c_copyright, s_copyright);
		wprintf(L"%s len: %d\n", c_project, s_project);




		memset(buffer_ptr, 0, 256);
		if (buffer_ptr)
		{
			size_t str_size = 0;

			memcpy(buffer_ptr, c_title, s_title);
			str_size += s_title;
			memcpy(buffer_ptr + str_size, c_copyright, s_copyright);
			str_size += s_copyright;
			memcpy(buffer_ptr + str_size, c_project, s_project);

			memset(buffer_ptr + 128, 0, 0x80u);


			wprintf(L"Combined data %s\n", (wchar_t*)buffer_ptr);
			size_t v33 = wcslen((wchar_t*)buffer_ptr) * 2;
			uint8_t v35 = magic_char;

			if ((signed int)(v33 + 1) > 0)
			{
				uint8_t v34 = magic_char;

				for (size_t i = 0; i <= v33; i++)
				{
					v34 = (v34 << 7) + (v34 >> 1);
					*buffer_ptr ^= v34;
					v35 += *buffer_ptr++ * ((v34 & 1) + 2);

				}

			}
			*buffer_ptr = v35;

			return (char*)&_magic_key[0];
		}
		return nullptr;
	};
	DllExport uint8_t* decode_chunk(char* chunk_data, int chunk_size, char magic_key, char* wrapperKey) {
		uint8_t* buff = (uint8_t*)malloc(chunk_size);
		vector<uint8_t> chunk_buffer(&chunk_data[0], &chunk_data[chunk_size]);
		vector<uint8_t> key_buffer(&wrapperKey[0], &wrapperKey[257]);

		DecodeChunk(chunk_buffer, key_buffer, &_xmmword, magic_key);
		auto p = (unsigned char*)buff;
		for (auto&& b : chunk_buffer) {
			*p++ = b;
		}
		return buff;

	};

	typedef int32_t _WORD;
	typedef int32_t _DWORD;

	DllExport uint32_t GenChecksum(wchar_t* name, wchar_t* password)
	{
		signed int xor_result; // eax@1
		__int16 v2; // cx@1
		wchar_t* v3; // edx@1
		wchar_t* v4; // esi@3
		const wchar_t* v5; // edx@3
		const wchar_t* v6; // edx@4
		wchar_t v7; // cx@4
		const wchar_t* v8; // ecx@4

		xor_result = 0x3939;
		v2 = name[0];
		v3 = name;
		if (v2)
		{
			do
			{
				++v3;
				xor_result += v2 ^ 0x7FFF;
				v2 = *v3;
			} while (*v3);
		}
		v4 = password;
		v5 = L"mqojhm:qskjhdsmkjsmkdjhq\u0063clkcdhdlkjhd";
		if (password[0])
		{
			do
			{
				v6 = v5 + 1;
				v7 = *(v6 - 1) + (*v4 ^ 0xC3C3);
				++v4;
				*(v4 - 1) = v7;
				xor_result += v7 ^ 0xF3F3;
				v8 = L"mqojhm:qskjhdsmkjsmkdjhq\u0063clkcdhdlkjhd";
				if (*v6)
					v8 = v6;
				v5 = v8;
			} while (*v4);
		}
		return xor_result;
	}
	DllExport int32_t decompressOld(uint8_t* source, int source_size, uint8_t* output, uint32_t output_size) {
		tinf_init();
		uint32_t actual_size = output_size;
		auto result = tinf_uncompress(output, &actual_size, source, source_size);
		if (result > 0)
			return result;
		else {
			return actual_size;
		}

	}
}

