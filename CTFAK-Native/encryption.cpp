#include "encryption.h"
bool DecodeWithKey(DecodeBuffer* decodeBuffer, const vector<uint8_t>& magic_key, char magic_char, const __m128_temp* xmmword)
{

    //std::cout << "Decoding with magic key " << magic_char << std::endl;
    __m128_temp* bufferPtr = &decodeBuffer->buffer[0]; // edx@1
    __m128i offset = _mm_load_si128((const __m128i*)xmmword); // xmm1@1
    for (int i = 0; i < 256; i += 4) // eax@1
    {
        _mm_storeu_si128((__m128i*)bufferPtr, _mm_add_epi32(_mm_shuffle_epi32(_mm_cvtsi32_si128(i), 0), offset));
        bufferPtr++;
    }
    decodeBuffer->i32_1 = 0;
    decodeBuffer->i32_2 = 0;
    size_t magic_key_pos = 0; // esi@3
    uint8_t magic_char_2 = magic_char; // ch@1
    uint8_t magic_char_3 = magic_char; // cl@1
    uint8_t v17 = 0;
    bool v15 = true; // v11 eax@4
    bool rtn = false; // ebx@1
    for (size_t i = 0; i < 256; i++) // v16
    {
        magic_char_3 = (magic_char_3 << 7) + (magic_char_3 >> 1);
        if (v15)
        {
            magic_char_2 += ((magic_char_3 & 1) + 2) * magic_key[magic_key_pos];
        }
        uint8_t temp_char = magic_char_3 ^ magic_key[magic_key_pos]; // v12 dl@6
        if (magic_char_3 == magic_key[magic_key_pos])
        {
            if (v15)
                rtn = magic_char_2 == magic_key[magic_key_pos + 1];
            if (!rtn) cout << (char)magic_char_2 << " " << (char)magic_char_3 << " " << (char)magic_key[magic_key_pos + 1] << endl;
            magic_char_3 = (magic_char >> 1) + (magic_char << 7);
            magic_key_pos = 0;
            v15 = false;
            temp_char = magic_char_3 ^ magic_key[0];
        }
        int32_t v13 = decodeBuffer->buffer[0].m128i_i32[i]; // eax@10
        v17 += (temp_char + v13);
        decodeBuffer->buffer[0].m128i_i32[i] = decodeBuffer->buffer[0].m128i_i32[v17];
        magic_key_pos++;
        decodeBuffer->buffer[0].m128i_i32[v17] = v13;
    }
    return rtn;
}

void FinishDecode(DecodeBuffer* decodeBuffer, vector<uint8_t>& chunk_buffer)
{
    for (size_t i = 0; i < chunk_buffer.size(); i++)
    {
        decodeBuffer->i32_1 = 0xFF & (decodeBuffer->i32_1 + 1);
        int32_t v7 = decodeBuffer->buffer[0].m128i_i32[decodeBuffer->i32_1];

        decodeBuffer->i32_2 = 0xFF & (decodeBuffer->i32_2 + v7);
        int32_t v9 = decodeBuffer->buffer[0].m128i_i32[decodeBuffer->i32_2];

        decodeBuffer->buffer[0].m128i_i32[decodeBuffer->i32_1] = v9;
        decodeBuffer->buffer[0].m128i_i32[decodeBuffer->i32_2] = v7;

        chunk_buffer[i] ^= decodeBuffer->buffer[0].m128i_u8[4 * (uint8_t)(v7 + v9)];
    }
}

void DecodeChunk(vector<uint8_t>& chunk_buffer, const vector<uint8_t>& magic_key, const __m128_temp* xmmword, char magic_char)
{
    DecodeBuffer decodeBuffer;
    //cout << "decoding" << endl;
    if (DecodeWithKey(&decodeBuffer, magic_key, magic_char, xmmword))
    {
        //cout << "decoded" << endl;
        FinishDecode(&decodeBuffer, chunk_buffer);
    }
}
