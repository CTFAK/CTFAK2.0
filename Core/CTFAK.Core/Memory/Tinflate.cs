/*
* tinflate  -  tiny inflate
*
* Copyright (c) 2003 by Joergen Ibsen / Jibz
* All Rights Reserved
*
* http://www.ibsensoftware.com/
*
* This software is provided 'as-is', without any express
* or implied warranty.  In no event will the authors be
* held liable for any damages arising from the use of
* this software.
*
* Permission is granted to anyone to use this software
* for any purpose, including commercial applications,
* and to alter it and redistribute it freely, subject to
* the following restrictions:
*
* 1. The origin of this software must not be
*    misrepresented; you must not claim that you
*    wrote the original software. If you use this
*    software in a product, an acknowledgment in
*    the product documentation would be appreciated
*    but is not required.
*
* 2. Altered source versions must be plainly marked
*    as such, and must not be misrepresented as
*    being the original software.
*
* 3. This notice may not be removed or altered from
*    any source distribution.
*/

using System;
using System.Runtime.InteropServices;

namespace CTFAK.Memory;

public unsafe struct TINF_TREE
{
    public fixed ushort table[16];
    public fixed ushort trans[288];
}

public unsafe struct TINF_DATA
{
    public byte* source;
    public uint tag;
    public uint bitcount;

    public byte* dest;
    public int* destLen;

    public TINF_TREE ltree; /* dynamic length/symbol tree */
    public TINF_TREE dtree; /* dynamic distance tree */
}

public unsafe class Tinflate
{
    public static TINF_TREE* sltree = (TINF_TREE*)Marshal.AllocHGlobal(sizeof(TINF_TREE));
    public static TINF_TREE* sdtree = (TINF_TREE*)Marshal.AllocHGlobal(sizeof(TINF_TREE));

    public static byte* length_bits = (byte*)Marshal.AllocHGlobal(30);

    public static ushort* length_cBase = (ushort*)Marshal.AllocHGlobal(30 * sizeof(ushort));

    public static byte* dist_bits = (byte*)Marshal.AllocHGlobal(30);
    public static ushort* dist_cBase = (ushort*)Marshal.AllocHGlobal(30 * sizeof(ushort));


    public static byte[] clcidx =
    {
        18, 17, 16, 0, 1, 2, 3, 4, 5,
        6, 7, 8, 9, 10, 11, 12, 13,
        14, 15
    };

/* ----------------------- *
 * -- utility functions -- *
 * ----------------------- */

/* build extra bits and cBase tables */
    private static void tinf_build_bits_cBase(byte* bits, ushort* cBase, int delta, int first)
    {
        int i, sum;

        /* build bits table */
        for (i = 0; i < delta; ++i) bits[i] = 0;
        for (i = 0; i < 30 - delta; ++i) bits[i + delta] = (byte)(i / delta);

        /* build cBase table */
        for (sum = first, i = 0; i < 30; ++i)
        {
            cBase[i] = (ushort)sum;
            sum += 1 << bits[i];
        }
    }

/* build the fixed huffman trees */
    private static void tinf_build_fixed_trees(TINF_TREE* lt, TINF_TREE* dt)
    {
        int i;

        /* build fixed length tree */
        for (i = 0; i < 7; ++i) lt->table[i] = 0;

        lt->table[7] = 24;
        lt->table[8] = 152;
        lt->table[9] = 112;

        for (i = 0; i < 24; ++i) lt->trans[i] = (ushort)(256 + i);
        for (i = 0; i < 144; ++i) lt->trans[24 + i] = (ushort)i;
        for (i = 0; i < 8; ++i) lt->trans[24 + 144 + i] = (ushort)(280 + i);
        for (i = 0; i < 112; ++i) lt->trans[24 + 144 + 8 + i] = (ushort)(144 + i);

        /* build fixed distance tree */
        for (i = 0; i < 5; ++i) dt->table[i] = 0;

        dt->table[5] = 32;

        for (i = 0; i < 32; ++i) dt->trans[i] = (ushort)i;
    }

/* given an array of code lengths, build a tree */
    private static void tinf_build_tree(TINF_TREE* t, byte* lengths, uint num)
    {
        var offs = new ushort[16];
        uint i, sum;

        /* clear code length count table */
        for (i = 0; i < 16; ++i) t->table[i] = 0;

        /* scan symbol lengths, and sum code length counts */
        for (i = 0; i < num; ++i) t->table[lengths[i]]++;

        t->table[0] = 0;

        /* compute offset table for distribution sort */
        for (sum = 0, i = 0; i < 16; ++i)
        {
            offs[i] = (ushort)sum;
            sum += t->table[i];
        }

        /* create code->symbol translation table (symbols sorted by code) */
        for (i = 0; i < num; ++i)
            if (lengths[i] > 0)
                t->trans[offs[lengths[i]]++] = (ushort)i;
    }

/* ---------------------- *
 * -- decode functions -- *
 * ---------------------- */

/* get one bit from source stream */
    private static int tinf_getbit(TINF_DATA* d)
    {
        uint bit;

        /* check if tag is empty */
        if (!(d->bitcount-- > 0))
        {
            /* load next tag */
            d->tag = *d->source++;
            d->bitcount = 7;
        }

        /* shift bit out of tag */
        bit = d->tag & 0x01;
        d->tag >>= 1;

        return (int)bit;
    }

/* read a num bit value from a stream and add cBase */
    private static uint tinf_read_bits(TINF_DATA* d, int num, int cBase)
    {
        uint val = 0;

        /* read num bits */
        if (num > 0)
        {
            var limit = (uint)(1 << num);
            uint mask;

            for (mask = 1; mask < limit; mask *= 2)
                if (tinf_getbit(d) > 0)
                    val += mask;
        }

        return (uint)(val + cBase);
    }

/* given a data stream and a tree, decode a symbol */
    private static int tinf_decode_symbol(TINF_DATA* d, TINF_TREE* t)
    {
        int sum = 0, cur = 0, len = 0;

        /* get more bits while code value is above sum */
        do
        {
            cur = 2 * cur + tinf_getbit(d);

            ++len;

            sum += t->table[len];
            cur -= t->table[len];
        } while (cur >= 0);

        return t->trans[sum + cur];
    }

/* given a data stream, decode dynamic trees from it */
    private static void tinf_decode_trees(TINF_DATA* d, TINF_TREE* lt, TINF_TREE* dt)
    {
        TINF_TREE code_tree;
        var lengths = (byte*)Marshal.AllocHGlobal(288 + 32);
        uint hlit, hdist, hclen;
        uint i, num, length;

        /* get 5 bits HLIT (257-286) */
        hlit = tinf_read_bits(d, 5, 257);

        /* get 5 bits HDIST (1-32) */
        hdist = tinf_read_bits(d, 5, 1);

        /* get 4 bits HCLEN (4-19) */
        hclen = tinf_read_bits(d, 4, 4);

        for (i = 0; i < 19; ++i) lengths[i] = 0;

        /* read code lengths for code length alphabet */
        for (i = 0; i < hclen; ++i)
        {
            /* get 3 bits code length (0-7) */
            var clen = tinf_read_bits(d, 3, 0);

            lengths[clcidx[i]] = (byte)clen;
        }

        /* build code length tree */
        tinf_build_tree(&code_tree, lengths, 19);

        /* decode code lengths for the dynamic trees */
        for (num = 0; num < hlit + hdist;)
        {
            var sym = tinf_decode_symbol(d, &code_tree);

            switch (sym)
            {
                case 16:
                    /* copy previous code length 3-6 times (read 2 bits) */
                {
                    var prev = lengths[num - 1];
                    for (length = tinf_read_bits(d, 2, 3); length > 0; --length) lengths[num++] = prev;
                }
                    break;
                case 17:
                    /* repeat code length 0 for 3-10 times (read 3 bits) */
                    for (length = tinf_read_bits(d, 3, 3); length > 0; --length) lengths[num++] = 0;
                    break;
                case 18:
                    /* repeat code length 0 for 11-138 times (read 7 bits) */
                    for (length = tinf_read_bits(d, 7, 11); length > 0; --length) lengths[num++] = 0;
                    break;
                default:
                    /* values 0-15 represent the actual code lengths */
                    lengths[num++] = (byte)sym;
                    break;
            }
        }

        /* build dynamic trees */
        tinf_build_tree(lt, lengths, hlit);
        tinf_build_tree(dt, lengths + hlit, hdist);
        Marshal.FreeHGlobal(new IntPtr(lengths));
    }

/* ----------------------------- *
 * -- block inflate functions -- *
 * ----------------------------- */

/* given a stream and two trees, inflate a block of data */
    private static int tinf_inflate_block_data(TINF_DATA* d, TINF_TREE* lt, TINF_TREE* dt)
    {
        /* remember current output position */
        var start = d->dest;

        while (true)
        {
            var sym = tinf_decode_symbol(d, lt);

            /* check for end of block */
            if (sym == 256)
            {
                *d->destLen += (int)(d->dest - start);
                return 0;
            }

            if (sym < 256)
            {
                *d->dest++ = (byte)sym;
            }
            else
            {
                int length, dist, offs;
                int i;

                sym -= 257;

                /* possibly get more bits from length code */
                length = (int)tinf_read_bits(d, length_bits[sym], length_cBase[sym]);

                dist = tinf_decode_symbol(d, dt);

                /* possibly get more bits from distance code */
                offs = (int)tinf_read_bits(d, dist_bits[dist], dist_cBase[dist]);

                /* copy match */
                for (i = 0; i < length; ++i) d->dest[i] = d->dest[i - offs];

                d->dest += length;
            }
        }
    }

/* inflate an uncompressed block of data */
    private static int tinf_inflate_uncompressed_block(TINF_DATA* d)
    {
        uint length;
        uint i;

        /* get length */
        length = d->source[1];
        length = 256 * length + d->source[0];

        d->source += 2;

        /* copy block */
        for (i = length; i > 0; --i) *d->dest++ = *d->source++;

        /* make sure we start next block on a byte boundary */
        d->bitcount = 0;

        *d->destLen += (int)length;

        return 0;
    }

/* inflate a block of data compressed with fixed huffman trees */
    private static int tinf_inflate_fixed_block(TINF_DATA* d)
    {
        /* decode block using fixed trees */
        return tinf_inflate_block_data(d, sltree, sdtree);
    }

/* inflate a block of data compressed with dynamic huffman trees */
    private static int tinf_inflate_dynamic_block(TINF_DATA* d)
    {
        /* decode trees from stream */
        tinf_decode_trees(d, &d->ltree, &d->dtree);

        /* decode block using decoded trees */
        return tinf_inflate_block_data(d, &d->ltree, &d->dtree);
    }

/* ---------------------- *
 * -- public functions -- *
 * ---------------------- */

/* initialize global (static) data */
    public static void tinf_init()
    {
        /* build fixed huffman trees */
        tinf_build_fixed_trees(sltree, sdtree);

        /* build extra bits and cBase tables */
        tinf_build_bits_cBase(length_bits, length_cBase, 4, 3);
        tinf_build_bits_cBase(dist_bits, dist_cBase, 2, 1);

        /* fix a special case */
        length_bits[28] = 0;
        length_cBase[28] = 258;
    }

/* inflate stream from source to dest */
    public static int tinf_uncompress(void* dest, uint* destLen, void* source, uint sourceLen)
    {
        TINF_DATA d;
        int bfinal;

        /* initialise data */
        d.source = (byte*)source;
        d.bitcount = 0;

        d.dest = (byte*)dest;
        d.destLen = (int*)destLen;

        *destLen = 0;

        do
        {
            uint btype;
            int res;

            /* read block type (2 bits) */
            btype = tinf_read_bits(&d, 3, 0);

            /* read final block flag */
            bfinal = tinf_getbit(&d);

            /* decompress block */
            switch (btype)
            {
                case 7:
                    /* decompress uncompressed block */
                    res = tinf_inflate_uncompressed_block(&d);
                    break;
                case 5:
                    /* decompress block with fixed huffman trees */
                    res = tinf_inflate_fixed_block(&d);
                    break;
                case 6:
                    /* decompress block with dynamic huffman trees */
                    res = tinf_inflate_dynamic_block(&d);
                    break;
                default:
                    return -1;
            }

            if (res != 0) return -1;
        } while (!(bfinal > 0));

        return (int)((char*)d.source - (char*)source);
    }
}