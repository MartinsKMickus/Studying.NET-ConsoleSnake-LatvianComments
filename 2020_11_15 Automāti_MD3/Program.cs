using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace _2020_11_15_Snake
{
    /// <summary>
    /// No sākuma programmu veidoju ar citu funkcionalitāti, bet pēc tam to pārveidoju uz Snake. Programmā ir nelietotas funkcijas, piemēram, rāmja izmēra maiņa vai čūskas ēdiena kustēšanās, jo šīs lietas nepielāgoju gala programmai.
    /// Kodu stingri netīriju, tāpēc iespējami lieki fragmenti. Diemžēl neesmu salicis Try Catch blokus visur, kur tie varētu būtu nepieciešami.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Snake snake = null;

            if (File.Exists("Snake.xml")) // Ielāde.
            {
                Stream stream = File.OpenRead("Snake.xml");
                XmlSerializer xml = new XmlSerializer(typeof(Snake));

                if (stream.ReadByte() != -1)
                {
                    stream.Seek(-1, SeekOrigin.Current);

                    try
                    {
                        snake = (Snake)xml.Deserialize(stream);
                    }
                    catch (Exception)
                    {
                        Console.Clear();
                        Console.WriteLine("Missing or corrupted file/s");
                        Thread.Sleep(1000);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Beep();
                        Thread.Sleep(500);
                        Console.Write("Window will close");
                        Thread.Sleep(5000);

                        return;
                    }
                    stream.Close();

                    try
                    {
                        snake.Refresh();
                    }
                    catch (Exception e)
                    {
                        if (e.HResult == 2)
                        {
                            Console.SetCursorPosition(0, snake.NotificationY);
                            Console.Write(e.Message + " Continue? Yes/y No/n");
                            var opt = Console.ReadKey(false).KeyChar;

                            if (opt == 'y' || opt == 'Y')
                            {
                                snake.Refresh(false);
                            }
                            else return;
                        }
                        else
                        {
                            Console.SetCursorPosition(0, snake.NotificationY);
                            Console.Write(e.Message + " Delete Old Save? Yes/y No/n");
                            var opt = Console.ReadKey(false).KeyChar;

                            if (opt == 'y' || opt == 'Y')
                            {
                                Console.Clear();
                                snake = new Snake();
                                snake.Start();
                            }
                            else return;
                        }
                    }
                }
            }

            if (snake == null)
            {
                snake = new Snake();
                snake.Start();
            }

            ConsoleKey pressed = new ConsoleKey();

            while (true) // Galvenā darbība.
            {
                if (Console.KeyAvailable || snake.Stop)
                {
                    pressed = Console.ReadKey(false).Key;
                    snake.Stop = false;
                }
                Thread.Sleep(snake.RecSpeed);

                switch (pressed)
                {
                    case ConsoleKey.Spacebar:
                        pressed = Console.ReadKey(false).Key;
                        break;
                    case ConsoleKey.LeftArrow:
                        //posx--;
                        snake.MoveLeft();
                        break;
                    case ConsoleKey.UpArrow:
                        //posy--; 
                        snake.MoveUp();
                        break;
                    case ConsoleKey.RightArrow:
                        //posx++;
                        snake.MoveRight();
                        break;
                    case ConsoleKey.DownArrow:
                        //posy++;
                        snake.MoveDown();
                        break;
                    case ConsoleKey.Escape:
                        Stream stream = File.Create("Snake.xml");
                        XmlSerializer xml = new XmlSerializer(typeof(Snake));
                        xml.Serialize(stream, snake);
                        stream.Close();
                        return;
                }
            }

        }
    }
}
