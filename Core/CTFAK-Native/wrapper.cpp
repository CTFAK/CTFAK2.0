#include "encryption.h"


#include "tinf.h"
//#include "tinflate.c"


#ifdef _WIN32
#define DllExport  __declspec(dllexport)
#else
#define DllExport __attribute__ ((visibility ("default")))
#endif // DEBUG

extern "C" {
vector<uint8_t> _magic_key;


DllExport uint8_t* decode_chunk(char* chunk_data, int chunk_size, char magic_key, char* wrapperKey)
{
    auto buff = static_cast<uint8_t*>(malloc(chunk_size));
    vector<uint8_t> chunk_buffer(&chunk_data[0], &chunk_data[chunk_size]);
    vector<uint8_t> key_buffer(&wrapperKey[0], &wrapperKey[257]);

    DecodeChunk(chunk_buffer, key_buffer, magic_key);
    auto p = buff;
    for (auto&& b : chunk_buffer)
    {
        *p++ = b;
    }
    return buff;
};
DllExport int32_t decompressOld(uint8_t* source, int source_size, uint8_t* output, uint32_t output_size)
{
    tinf_init();
    uint32_t actual_size = output_size;
    auto result = tinf_uncompress(output, &actual_size, source, source_size);
    if (result > 0)
        return result;
    return actual_size;
}
}





