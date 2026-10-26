using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

internal static class Program
{
    static byte[]? _key;
    const int BufSize = 1 << 20; // 1 MB, xử lý theo luồng nên file hàng GB vẫn không tốn RAM

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.WriteLine("\n===== BÀI 1: AES (đối xứng) =====");
            Console.WriteLine($"Khoá hiện tại: {(_key == null ? "(chưa có)" : _key.Length * 8 + " bit")}");
            Console.WriteLine("1. Sinh khoá AES");
            Console.WriteLine("2. Lưu khoá ra file");
            Console.WriteLine("3. Import khoá từ file");
            Console.WriteLine("4. Mã hoá văn bản (in kết quả base64)");
            Console.WriteLine("5. Giải mã văn bản (nhập base64)");
            Console.WriteLine("6. Mã hoá file (có đo thời gian)");
            Console.WriteLine("7. Giải mã file (có đo thời gian)");
            Console.WriteLine("8. So sánh 2 file theo byte");
            Console.WriteLine("9. Tạo file test ngẫu nhiên (dung lượng MB)");
            Console.WriteLine("0. Thoát");
            Console.Write("Chọn: ");
            try
            {
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": GenKey(); break;
                    case "2": SaveKey(); break;
                    case "3": LoadKey(); break;
                    case "4": EncText(); break;
                    case "5": DecText(); break;
                    case "6": FileOp(true); break;
                    case "7": FileOp(false); break;
                    case "8": Compare(); break;
                    case "9": CreateTestFile(); break;
                    case "0": return;
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi: " + ex.Message); }
        }
    }

    static string Ask(string prompt, string def = "")
    {
        Console.Write(prompt + (def == "" ? ": " : $" [{def}]: "));
        var s = Console.ReadLine()?.Trim().Trim('"');
        return string.IsNullOrEmpty(s) ? def : s;
    }

    static byte[] RequireKey() => _key ?? throw new InvalidOperationException("Chưa có khoá. Hãy sinh (1) hoặc import (3) khoá.");

    static void GenKey()
    {
        int bits = int.Parse(Ask("Độ dài khoá (128/192/256)", "256"));
        if (bits != 128 && bits != 192 && bits != 256) throw new ArgumentException("Chỉ hỗ trợ 128/192/256.");
        _key = RandomNumberGenerator.GetBytes(bits / 8);
        Console.WriteLine("Khoá (base64): " + Convert.ToBase64String(_key));
    }

    static void SaveKey()
    {
        var k = RequireKey();
        string path = Ask("Đường dẫn file lưu khoá", "aes.key");
        File.WriteAllText(path, Convert.ToBase64String(k));
        Console.WriteLine("Đã lưu: " + Path.GetFullPath(path));
    }

    static void LoadKey()
    {
        string path = Ask("Đường dẫn file khoá", "aes.key");
        var k = Convert.FromBase64String(File.ReadAllText(path).Trim());
        if (k.Length != 16 && k.Length != 24 && k.Length != 32) throw new InvalidDataException("Khoá không hợp lệ (phải 16/24/32 byte).");
        _key = k;
        Console.WriteLine($"Đã import khoá {k.Length * 8} bit.");
    }

    // Định dạng: [IV 16 byte][ciphertext]. AES-CBC + PKCS7, IV ngẫu nhiên cho mỗi lần mã hoá.
    static void EncText()
    {
        var key = RequireKey();
        string text = Ask("Nhập văn bản");
        using var aes = Aes.Create();
        aes.Key = key; aes.GenerateIV();
        var plain = Encoding.UTF8.GetBytes(text);
        var ct = aes.CreateEncryptor().TransformFinalBlock(plain, 0, plain.Length);
        Console.WriteLine("Kết quả (base64 của IV + ciphertext):\n" + Convert.ToBase64String(aes.IV.Concat(ct).ToArray()));
    }

    static void DecText()
    {
        var key = RequireKey();
        var all = Convert.FromBase64String(Ask("Nhập base64"));
        using var aes = Aes.Create();
        aes.Key = key; aes.IV = all[..16];
        var plain = aes.CreateDecryptor().TransformFinalBlock(all, 16, all.Length - 16);
        Console.WriteLine("Văn bản gốc: " + Encoding.UTF8.GetString(plain));
    }

    static void FileOp(bool encrypt)
    {
        var key = RequireKey();
        string inp = Ask("File nguồn");
        string outp = Ask("File đích", inp + (encrypt ? ".enc" : ".dec"));
        var sw = Stopwatch.StartNew();
        using (var fin = new FileStream(inp, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize))
        using (var fout = new FileStream(outp, FileMode.Create, FileAccess.Write, FileShare.None, BufSize))
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            if (encrypt)
            {
                aes.GenerateIV();
                fout.Write(aes.IV);
                using var cs = new CryptoStream(fout, aes.CreateEncryptor(), CryptoStreamMode.Write);
                fin.CopyTo(cs, BufSize);
            }
            else
            {
                var iv = new byte[16];
                fin.ReadExactly(iv);
                aes.IV = iv;
                using var cs = new CryptoStream(fin, aes.CreateDecryptor(), CryptoStreamMode.Read);
                cs.CopyTo(fout, BufSize);
            }
        }
        sw.Stop();
        double mb = new FileInfo(inp).Length / 1048576.0;
        Console.WriteLine($"{(encrypt ? "Mã hoá" : "Giải mã")} xong: {mb:F1} MB trong {sw.Elapsed.TotalSeconds:F3} s ({mb / sw.Elapsed.TotalSeconds:F1} MB/s)");
    }

    static void Compare()
    {
        string a = Ask("File 1 (nguồn)"), b = Ask("File 2 (sau giải mã)");
        var sw = Stopwatch.StartNew();
        long la = new FileInfo(a).Length, lb = new FileInfo(b).Length;
        if (la != lb) { Console.WriteLine($"KHÁC kích thước: {la} vs {lb} byte"); return; }
        using var fa = new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize);
        using var fb = new FileStream(b, FileMode.Open, FileAccess.Read, FileShare.Read, BufSize);
        var ba = new byte[BufSize]; var bb = new byte[BufSize];
        long pos = 0;
        while (true)
        {
            int n = fa.Read(ba, 0, BufSize);
            if (n == 0) break;
            fb.ReadExactly(bb, 0, n);
            var m = ba.AsSpan(0, n).CommonPrefixLength(bb.AsSpan(0, n));
            if (m < n) { Console.WriteLine($"KHÁC nhau tại byte offset {pos + m}"); return; }
            pos += n;
        }
        Console.WriteLine($"GIỐNG HỆT nhau ({la} byte). Thời gian so sánh: {sw.Elapsed.TotalSeconds:F3} s");
    }

    static void CreateTestFile()
    {
        string path = Ask("File test", "test.bin");
        long mb = long.Parse(Ask("Dung lượng (MB)", "1024"));
        var buf = new byte[BufSize];
        using var f = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufSize);
        for (long i = 0; i < mb; i++) { RandomNumberGenerator.Fill(buf); f.Write(buf); }
        Console.WriteLine("Đã tạo " + Path.GetFullPath(path));
    }
}