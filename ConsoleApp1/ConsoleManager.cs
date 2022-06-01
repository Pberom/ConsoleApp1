using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;

namespace ConsoleApp1;

public class ConsoleManager
{
    public static int getCurrentLine()
    {
        return System.Console.CursorTop;
    }

    private static int _start_record;

    public static void startRecord()
    {
        _start_record = getCurrentLine();
    }

    private static int _end_record;

    public static void endRecord()
    {
        _end_record = getCurrentLine();
    }

    public static void clearRecordLine()
    {
        clearLines(_start_record, _end_record);
    }

    public static void clearLines(int start, int end)
    {
        Console.MoveBufferArea(
            0,
            start,
            Console.BufferWidth,
            end,
            System.Console.BufferWidth,
            end);
        Console.SetCursorPosition(0, start);
    }

    public static string read(string? command = null)
    {
        print(command ?? ">> ", false);
        return Console.ReadLine() ?? "";
    }

    public static int inputInt(string? command = null, string? error = null)
    {
        string res = ConsoleManager.read(command);
        int result = -1;
        while (!int.TryParse(res, out result))
        {
            print(error ?? "Это не число");
            res = ConsoleManager.read(command);
        }

        return result;
    }

    public static void print(string[] data, bool newLine = true)
    {
        print("\t" + string.Join("\n\t", data), newLine);
    }

    public static void print(string message, bool newLine = true)
    {
        if (newLine)
        {
            Console.WriteLine(message);
        }
        else
        {
            Console.Write(message);
        }
    }
}



