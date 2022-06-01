using System.Xml;
using ConsoleApp1;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

string fileName = "save.xml";
bool work = true;

ConsoleManager.startRecord();

while (work)
{
    Console.WriteLine("add, del, clear, exit, send, parse");
    switch (ConsoleManager.read())
    {
       
        case "add":
            add();
            break;
        case "del":
            del();
            break;
        case "list":
            printPersons(ReadXmlFile(fileName));
            break;
        case "clear":
            ConsoleManager.endRecord();
            ConsoleManager.clearRecordLine();
            break;
        case "exit":
            work = false;
            break;
        case "send":
            UdpFileServer.parse();
            break;
        case "parse":
            UdpFileClient.GetFileDetails();
            break;
        default:
            ConsoleManager.print("Команда не распознана");
            break;
    }
}



void add()
{
    
    List<Person> pl = ReadXmlFile(fileName);
    Person p = new Person() { name = ConsoleManager.read("Введите имя: ") };
    Console.WriteLine("Желаете ввести какие-либо данные взамен возраста? Да или нет.");
    string a =  Console.ReadLine();
    if (a == "Нет" || a == "нет")
    {
        p.age = ConsoleManager.inputInt("Введите возраст: ");
        pl.Add(p);
        WriteXmlFile(fileName, pl);
    }
    else if (a == "Да" || a == "да")
    {
        Console.WriteLine("Выбирите, что будем вводить: \n 1. Фамилию \n 2. Отчество \n 3. Дату рождения \n 4. Адрес проживания");
        string b = Console.ReadLine();
        if (b == "1" || b == "1.")
        {
            p.f = Console.ReadLine();
            pl.Add(p);
            WriteXmlFile(fileName, pl);
        }
        else if (b == "2" || b == "2.")
        {
            p.o = Console.ReadLine();
            pl.Add(p);
            WriteXmlFile(fileName, pl);
        }
        else if (b == "3" || b == "3.")
        {
            p.BDay = Console.ReadLine();
            pl.Add(p);
            WriteXmlFile(fileName, pl);
        }
        else if (b == "4" || b == "4.")
        {
            p.adress = Console.ReadLine();
            pl.Add(p);
            WriteXmlFile(fileName, pl);
        }
        else Console.WriteLine("Некорректность в введении данных");
    }
    else
        Console.WriteLine("Ошибка добавления");
}

void del()
{
    List<Person> pl = ReadXmlFile(fileName);

    string name = ConsoleManager.read("Введите имя или номер для удаления: ");
    int index;

    if (int.TryParse(name, out index))
    {
        pl.Remove(pl[index - 1]);
        ConsoleManager.print($"Удален id: {index}");
    }
    else
    {
        ConsoleManager.print($"Удалено: {pl.RemoveAll(p => p.name == name)}");
    }

    WriteXmlFile(fileName, pl);
}


void WriteXmlFile(string filename, List<Person> persons)
{
    var doc = new XmlDocument();

    var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

    doc.AppendChild(xmlDeclaration);

    var root = doc.CreateElement("persons");

    foreach (var phone in persons)
    {
        var personNode = doc.CreateElement("person");

        AddChildNode("name", phone.name, personNode, doc);

        AddChildNode("age", phone.age.ToString(), personNode, doc);

        root.AppendChild(personNode);
    }

    doc.AppendChild(root);

    doc.Save(filename);
}

void AddChildNode(string childName, string childText, XmlElement parentNode, XmlDocument doc)
{
    var child = doc.CreateElement(childName);
    child.InnerText = childText;
    parentNode.AppendChild(child);
}


List<Person> ReadXmlFile(string filename)
{
    if (!File.Exists(filename))
        return new List<Person>();
    List<Person> pl = new List<Person>();
    var doc = new XmlDocument();
    doc.Load(filename);
    var root = doc.DocumentElement;


    foreach (var child in root.ChildNodes)
    {
        Person p = new Person();
        foreach (XmlElement VARIABLE in (XmlElement)child)
        {
            switch (@VARIABLE.Name)
            {
                case "name":
                    p.name = VARIABLE.InnerText;
                    break;
                case "age":
                    p.age = int.Parse(VARIABLE.InnerText);
                    break;
            }
        }

        pl.Add(p);
    }

    return pl;
}


void printPersons(List<Person> pl)
{
    ConsoleManager.print("===================");
    ConsoleManager.print("Список:");
    
    for (int i = 0; i < pl.Count; i++)
    {
        ConsoleManager.print($"{i+1}. {pl[i]}");
    }

    ConsoleManager.print("===================");
}



public class UdpFileServer
{
    // Информация о файле (требуется для получателя)
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FileDetails fileDet = new FileDetails();

    // Поля, связанные с UdpClient
    private static IPAddress remoteIPAddress;
    private const int remotePort = 5002;
    private static UdpClient sender = new UdpClient();
    private static IPEndPoint endPoint;

    // Filestream object
    private static FileStream fs;
    [STAThread]
   public static void parse()
    {
        try
        {
            // Получаем удаленный IP-адрес и создаем IPEndPoint
            Console.WriteLine("Введите удаленный IP-адрес");
            remoteIPAddress = IPAddress.Parse(Console.ReadLine().ToString());//"127.0.0.1");
            endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            // Получаем путь файла и его размер (должен быть меньше 8kb)
            Console.WriteLine("Введите путь к файлу и его имя");
            fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);

            if (fs.Length > 8192)
            {
                Console.Write("Файл должен весить меньше 8кБ");
                sender.Close();
                fs.Close();
                return;
            }

            // Отправляем информацию о файле
            SendFileInfo();

            // Ждем 2 секунды
            Thread.Sleep(2000);

            // Отправляем сам файл
            SendFile();

            Console.ReadLine();

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void SendFileInfo()
    {

        // Получаем тип и расширение файла
        fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

        // Получаем длину файла
        fileDet.FILESIZE = fs.Length;

        XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
        MemoryStream stream = new MemoryStream();

        // Сериализуем объект
        fileSerializer.Serialize(stream, fileDet);

        // Считываем поток в байты
        stream.Position = 0;
        Byte[] bytes = new Byte[stream.Length];
        stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

        Console.WriteLine("Отправка деталей файла...");

        // Отправляем информацию о файле
        sender.Send(bytes, bytes.Length, endPoint);
        stream.Close();

    }

    private static void SendFile()
    {
        // Создаем файловый поток и переводим его в байты
        Byte[] bytes = new Byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            // Закрываем соединение и очищаем поток
            fs.Close();
            sender.Close();
        }
        Console.WriteLine("Файл успешно отправлен.");
        Console.Read();
    }
}

public class UdpFileClient
{
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FileDetails fileDet;
    private static int localPort = 5002;
    private static UdpClient receivingUdpClient = new UdpClient(localPort);
    private static IPEndPoint RemoteIpEndPoint = null;

    private static FileStream fs;
    private static Byte[] receiveBytes = new Byte[0];
    public static void GetFileDetails()
    {
        try
        {
            Console.WriteLine("Информация о файле");
            receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            Console.WriteLine("Получил!");

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream1 = new MemoryStream();
            stream1.Write(receiveBytes, 0, receiveBytes.Length);
            stream1.Position = 0;
            fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
            Console.WriteLine("Получен файл типа ." + fileDet.FILETYPE +
                " имеющий размер " + fileDet.FILESIZE.ToString() + " байт");
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void ReceiveFile()
    {
        try
        {
            Console.WriteLine("Получаю файл...");
            receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            Console.WriteLine("Файл получил");

            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Write(receiveBytes, 0, receiveBytes.Length);
            Console.WriteLine("Файл сохранен...");
            Console.WriteLine("Открытие файла");
            Process.Start(fs.Name);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            fs.Close();
            receivingUdpClient.Close();
            Console.Read();
        }
    }
}
