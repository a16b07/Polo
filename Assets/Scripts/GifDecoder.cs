using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GifDecoder
{
    public struct GifFrame { public Texture2D tex; public float delay; }

    public static GifFrame[] Load(string path)
    {
        if (!File.Exists(path)) { Debug.LogWarning("GIF not found: " + path); return Array.Empty<GifFrame>(); }
        try   { return Decode(File.ReadAllBytes(path)); }
        catch (Exception e) { Debug.LogWarning("GIF decode error: " + e.Message); return Array.Empty<GifFrame>(); }
    }

    // ── top-level parser ─────────────────────────────────────────────────────
    static GifFrame[] Decode(byte[] b)
    {
        int p = 0;
        if (b.Length < 6 || b[0] != 'G' || b[1] != 'I' || b[2] != 'F') return Array.Empty<GifFrame>();
        p = 6;

        int sw = RL(b, ref p), sh = RL(b, ref p);
        int pak = b[p++]; p += 2;
        Color32[] gct = (pak & 0x80) != 0 ? ReadCT(b, ref p, (pak & 7) + 1) : null;

        var     frames    = new List<GifFrame>();
        Color32[] canvas  = new Color32[sw * sh];
        Color32[] prevCvs = null;
        float delay = 0.1f; int transIdx = -1; bool hasTr = false; int dispose = 0;

        while (p < b.Length - 1)
        {
            int blk = b[p++];
            if (blk == 0x3B) break;

            if (blk == 0x21)                          // extension
            {
                int lbl = b[p++];
                if (lbl == 0xF9 && b[p] == 4)        // Graphic Control
                {
                    p++;                               // block size
                    int ctrl = b[p++];
                    delay    = Math.Max(0.02f, RL(b, ref p) / 100f);
                    transIdx = b[p++]; p++;            // terminator
                    hasTr    = (ctrl & 1) != 0;
                    dispose  = (ctrl >> 3) & 7;
                }
                else SkipSubs(b, ref p);
                continue;
            }

            if (blk == 0x2C)                          // image descriptor
            {
                int ix = RL(b, ref p), iy = RL(b, ref p);
                int iw = RL(b, ref p), ih = RL(b, ref p);
                int ip = b[p++];
                bool hasLCT = (ip & 0x80) != 0, inter = (ip & 0x40) != 0;
                Color32[] ct = hasLCT ? ReadCT(b, ref p, (ip & 7) + 1) : gct;

                int    minCS  = b[p++];
                byte[] cdata  = ReadSubs(b, ref p);
                int[]  idx    = LZW(cdata, minCS);

                // Pick base canvas according to disposal method
                Color32[] draw = dispose == 3 && prevCvs != null ? (Color32[])prevCvs.Clone()
                               : dispose == 2                     ? new Color32[sw * sh]
                                                                  : (Color32[])canvas.Clone();
                prevCvs = (Color32[])canvas.Clone();

                for (int i = 0; i < idx.Length && i < iw * ih; i++)
                {
                    int ci = idx[i];
                    if (hasTr && ci == transIdx) continue;
                    int row = inter ? Deinter(i / iw, ih) : i / iw;
                    int cx = ix + i % iw, cy = iy + row;
                    if (cx < sw && cy < sh && ct != null && ci < ct.Length)
                        draw[(sh - 1 - cy) * sw + cx] = ct[ci]; // flip Y for Unity
                }

                canvas = draw;
                var tex = new Texture2D(sw, sh, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.SetPixels32(draw);
                tex.Apply();
                frames.Add(new GifFrame { tex = tex, delay = delay });

                hasTr = false; transIdx = -1; delay = 0.1f; dispose = 0;
            }
        }
        return frames.ToArray();
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    static int RL(byte[] b, ref int p) { int v = b[p] | (b[p + 1] << 8); p += 2; return v; }

    static Color32[] ReadCT(byte[] b, ref int p, int bits)
    {
        int n = 1 << bits; var ct = new Color32[n];
        for (int i = 0; i < n; i++) ct[i] = new Color32(b[p++], b[p++], b[p++], 255);
        return ct;
    }

    static void SkipSubs(byte[] b, ref int p)
    { while (p < b.Length) { int n = b[p++]; if (n == 0) break; p += n; } }

    static byte[] ReadSubs(byte[] b, ref int p)
    {
        var r = new List<byte>();
        while (p < b.Length) { int n = b[p++]; if (n == 0) break; for (int i = 0; i < n; i++) r.Add(b[p++]); }
        return r.ToArray();
    }

    static int Deinter(int row, int h)
    {
        int p0 = (h + 7) / 8; if (row < p0) return row * 8;      row -= p0;
        int p1 = (h + 3) / 8; if (row < p1) return 4 + row * 8;  row -= p1;
        int p2 = (h + 1) / 4; if (row < p2) return 2 + row * 4;  row -= p2;
        return 1 + row * 2;
    }

    // ── LZW decompressor ─────────────────────────────────────────────────────
    static int[] LZW(byte[] comp, int minCS)
    {
        int clr = 1 << minCS, eoi = clr + 1;
        var table = new List<int[]>(512);

        void Reset()
        {
            table.Clear();
            for (int i = 0; i < clr; i++) table.Add(new[] { i });
            table.Add(Array.Empty<int>()); // clr placeholder
            table.Add(Array.Empty<int>()); // eoi placeholder
        }
        Reset();

        int cs = minCS + 1, mask = (1 << cs) - 1;
        int buf = 0, bits = 0, bp = 0;
        var out_ = new List<int>(comp.Length * 2);
        int[] prev = null;

        while (bp < comp.Length || bits >= cs)
        {
            while (bits < cs && bp < comp.Length) { buf |= comp[bp++] << bits; bits += 8; }
            if (bits < cs) break;
            int code = buf & mask; buf >>= cs; bits -= cs;

            if (code == clr) { Reset(); cs = minCS + 1; mask = (1 << cs) - 1; prev = null; continue; }
            if (code == eoi) break;

            int[] entry;
            if (code < table.Count && table[code].Length > 0) entry = table[code];
            else if (code == table.Count && prev != null)
            { entry = new int[prev.Length + 1]; Array.Copy(prev, entry, prev.Length); entry[prev.Length] = prev[0]; }
            else break;

            foreach (var v in entry) out_.Add(v);

            if (prev != null && table.Count < 4096)
            {
                var ne = new int[prev.Length + 1];
                Array.Copy(prev, ne, prev.Length); ne[prev.Length] = entry[0];
                table.Add(ne);
                if (table.Count > mask && cs < 12) { cs++; mask = (1 << cs) - 1; }
            }
            prev = entry;
        }
        return out_.ToArray();
    }
}
