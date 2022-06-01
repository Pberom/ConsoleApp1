namespace ConsoleApp1;

public class Person
{
    public string name { get; set; }
    
    public int age { get; set; }
    public string f { get; set; }
    public string o { get; set; }
    public string BDay { get; set; }
    public string adress { get; set; }

    public override string ToString()
    {
        return $"Имя: {name}, возраст: {age}. Фамилия: {f} Отчество: {o} Дата рождения: {BDay} Адресс - {adress}";
    }
}