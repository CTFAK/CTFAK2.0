#pragma once
#include <stdint.h>
#include <memory>
using std::shared_ptr;
#include <vector>
using std::vector;
#include <iostream>
using std::cout;
using std::endl;
//#include <assert.h>
#include <stdint.h>
#include <iostream>
#include <emmintrin.h>
#include <memory>

typedef union __m128_temp {
	char              m128i_i8[16];
	short             m128i_i16[8];
	int             m128i_i32[4];
	long int             m128i_i64[2];
	unsigned char     m128i_u8[16];
	unsigned short    m128i_u16[8];
	unsigned int    m128i_u32[4];
	unsigned long int     m128i_u64[2];
} __m128_temp;

struct DecodeBuffer
{
	__m128_temp buffer[64];
	int32_t i32_1;
	int32_t i32_2;
};

bool DecodeWithKey(DecodeBuffer* decodeBuffer, const vector<uint8_t>& magic_key, char magic_char, const __m128_temp* xmmword);

void FinishDecode(DecodeBuffer* decodeBuffer, vector<uint8_t>& chunk_buffer);

void DecodeChunk(vector<uint8_t>& chunk_buffer, const vector<uint8_t>& magic_key, const __m128_temp* xmmword, char magic_char);
