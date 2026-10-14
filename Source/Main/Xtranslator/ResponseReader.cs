using System;
using System.IO;
using System.Text;

namespace Xtranslator
{
    internal static class ResponseReader
    {
        internal const int MaximumCharacters = 1024 * 1024;
        internal static string Read(TextReader reader, int maximum = MaximumCharacters)
        {
            var result = new StringBuilder(); var buffer = new char[4096]; int count;
            while ((count = reader.Read(buffer, 0, Math.Min(buffer.Length, maximum - result.Length + 1))) > 0)
            {
                if (result.Length + count > maximum) throw new InvalidOperationException("服务返回内容过大，已停止读取，请缩短原文或检查接口。");
                result.Append(buffer, 0, count);
            }
            return result.ToString();
        }
        internal static string Line(TextReader reader, int maximum = 65536)
        {
            var result = new StringBuilder(); int ch;
            while ((ch = reader.Read()) >= 0)
            {
                if (ch == '\n') return result.ToString().TrimEnd('\r');
                if (result.Length >= maximum) throw new InvalidOperationException("服务返回的流式数据过大，已停止读取。");
                result.Append((char)ch);
            }
            return result.Length == 0 ? null : result.ToString().TrimEnd('\r');
        }
    }
}
