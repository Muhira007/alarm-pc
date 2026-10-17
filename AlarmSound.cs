using System.IO;
using System.Runtime.InteropServices;

namespace AlarmGiliran;

public static class AlarmSound
{
    private static byte[]? wave;
    private static GCHandle pinnedWave;

    [DllImport("winmm.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PlaySound(IntPtr sound, IntPtr module, uint flags);

    public static bool Start()
    {
        Stop();
        wave ??= CreateWave();
        pinnedWave = GCHandle.Alloc(wave, GCHandleType.Pinned);
        // ASYNC | MEMORY | LOOP | NODEFAULT. Keep the data pinned until playback stops.
        bool playing = PlaySound(pinnedWave.AddrOfPinnedObject(), IntPtr.Zero, 0x0001 | 0x0004 | 0x0008 | 0x0002);
        if (!playing) Stop();
        return playing;
    }

    public static void Stop()
    {
        PlaySound(IntPtr.Zero, IntPtr.Zero, 0);
        if (pinnedWave.IsAllocated) pinnedWave.Free();
    }

    private static byte[] CreateWave()
    {
        const int sampleRate = 22050;
        const int count = sampleRate * 3;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8); writer.Write(36 + count * 2); writer.Write("WAVEfmt "u8);
        writer.Write(16); writer.Write((short)1); writer.Write((short)1);
        writer.Write(sampleRate); writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
        writer.Write("data"u8); writer.Write(count * 2);
        for (int i = 0; i < count; i++)
        {
            double time = (double)i / sampleRate;
            int note = (int)(time / 0.42);
            double local = time - note * 0.42;
            double frequency = (note % 3) switch { 0 => 659.25, 1 => 783.99, _ => 987.77 };
            double envelope = time < 1.26 && local < 0.36 ? Math.Min(1, local / 0.015) * Math.Exp(-local * 5) : 0;
            double sample = Math.Sin(2 * Math.PI * frequency * time) * 0.65 + Math.Sin(4 * Math.PI * frequency * time) * 0.12;
            writer.Write((short)(sample * envelope * short.MaxValue * 0.55));
        }
        return stream.ToArray();
    }
}
