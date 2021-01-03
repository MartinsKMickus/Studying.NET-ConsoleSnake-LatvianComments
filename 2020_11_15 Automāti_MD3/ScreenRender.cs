using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace _2020_11_15_Snake
{
    public class ScreenRender
    {
        int ResX, ResY, PosX, PosY, StartX = 8, StartY = 8;
        Exception OutOfRange = new Exception("To start new game old save has to be deleted!\nReason is because of too small resolution of the screen or too large console symbols."); // Šis brīdinājums īsti nav pareizs, jo šeit var būt ir kāda cita kļūda.

        string[,] RenderedScreen; // Loģiskie simboli.
        int[,] ScreenColor; // Loģisko simbolu krāsa.
        bool[,] ActivePoints, InactivePoints; // Aktīvie punkti, iededzinātie punkti.

        [DllImport("kernel32.dll", ExactSpelling = true)]// Nākamās rindiņas ņemtas no interneta pamācības par konsoles maksimizēšanu.
        private static extern IntPtr GetConsoleWindow();//
        private static IntPtr ThisConsole = GetConsoleWindow();//
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]//
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);//
        private const int HIDE = 0;//
        private const int MAXIMIZE = 3;//
        private const int MINIMIZE = 6;//
        private const int RESTORE = 9; // Līdz šai rindiņa

        public int resx { get { return ResX; } }
        public int resy { get { return ResY; } }
        public int startx { get { return StartX; } }
        public int starty { get { return StartY; } }

        /// <summary>
        /// Maksimizē konsoli un nomēra garuma un platuma izšķirtspēju. Iezīmē laukumu, kas var tikt rediģēts. Uzzīmē laukuma robežas.
        /// </summary>
        /// <param name="size">Izmērs: 0 - ļoti mazs(lieliem ekrāniem), 1 - mazs, 2 - parasts</param>
        public ScreenRender(int size = 2)
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight); // Konsoles maksimizēsānai, ņemts no interneta.
            ShowWindow(ThisConsole, MAXIMIZE); // Konsoles maksimizēsānai, ņemts no interneta.

            try // Šis ir mans veids kā maksimizēt konsoli, lai nerastos tinjoslas.
            {
                for (int res = 40; true; res++)
                {
                    Console.WindowWidth = res;
                    Console.BufferWidth = res;
                    ResX = res;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("X Asis Adjusted");
            }

            try // Šis ir mans veids kā maksimizēt konsoli, lai nerastos tinjoslas.
            {
                for (int res = 10; true; res++)
                {
                    Console.WindowHeight = res;
                    Console.BufferHeight = res;
                    ResY = res;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Y Asis Adjusted");
                Console.WriteLine();
            }

            switch (size) // Darbojošā loga izmērs.
            {
                case 0:
                    ResX = Console.LargestWindowWidth / 4;
                    ResY = Console.LargestWindowHeight / 4;
                    break;
                case 1:
                    ResX = Console.LargestWindowWidth / 3;
                    ResY = Console.LargestWindowHeight / 3;
                    break;
                case 2:
                    ResX = (int)(Console.LargestWindowWidth / 1.5);
                    ResY = (int)(Console.LargestWindowHeight / 1.5);
                    break;
                default:
                    ResX = (int)(Console.LargestWindowWidth / 1.5);
                    ResY = (int)(Console.LargestWindowHeight / 1.5);
                    break;
            }
            Console.SetCursorPosition(StartX + (ResX / 2 - 9), StartY - 2);
            //Console.WriteLine("WELCOME TO THE GAME!!!");
            Console.Beep();
            Console.WriteLine("!!! Esc To Save !!!"); // Īsti nav pareizā vieta kur šo rakstīt, jo ScreenRender nenosaka taustiņus.
            Thread.Sleep(500);
            Console.Beep();
            Console.SetCursorPosition(StartX + (ResX / 2 - 10), StartY - 1);
            Console.WriteLine("!!! Space To Pause !!!"); // Īsti nav pareizā vieta kur šo rakstīt, jo ScreenRender nenosaka taustiņus.
            Thread.Sleep(1000);
            Console.Beep();

            RenderedScreen = new string[ResX, ResY]; // Loģiskie simboli
            ActivePoints = new bool[ResX, ResY]; // Aktīvie punkti.
            InactivePoints = new bool[ResX, ResY]; // Iededzinātie punkti.
            ScreenColor = new int[ResX, ResY]; // Loģisko simbolu krāsa.

            for (PosY = StartY; PosY < ResY - 1; PosY++) // Darbojošā laukuma ģenerēšana
            {
                for (PosX = StartX; PosX < ResX; PosX++)
                {
                    if (PosY == StartY || PosY == ResY - 2) // Pirmā, pēdējā Y līnija
                    {
                        RenderedScreen[PosX, PosY] = "-";
                        ActivePoints[PosX, PosY] = true;
                        InactivePoints[PosX, PosY] = true;
                    }
                    else
                    if (PosX == StartX || PosX == ResX - 1) // Pirmā, pēdējā X līnija
                    {
                        RenderedScreen[PosX, PosY] = "|";
                        ActivePoints[PosX, PosY] = true;
                        InactivePoints[PosX, PosY] = true;
                    }
                    else // Pildījums
                    {
                        RenderedScreen[PosX, PosY] = " ";
                        ActivePoints[PosX, PosY] = true;
                        InactivePoints[PosX, PosY] = false;
                    }
                }
            }
            PrintScreen(true);
            //for (PosY = StartY; PosY < ResY; PosY++) 
            //{
            //    for (PosX = StartX; PosX < ResX && ActivePoints[PosX, PosY]; PosX++)
            //    {
            //        Console.SetCursorPosition(PosX, PosY);
            //        Console.Write(RenderedScreen[PosX, PosY]);
            //        ActivePoints[PosX, PosY] = false;
            //        //InactivePoints[PosX, PosY] = false;
            //        ScreenColor[PosX, PosY] = 0;
            //    }
            //    //Console.Write("\n");
            //}
            Console.SetCursorPosition(0, ResY - 1);
        }

        /// <summary>
        /// Izdrukā uz ekrāna visus renderētos vai tikai aktīvos un neiededzinātos simbolus ar attiecīgo krāsu kāda ir saglabāta konkrētajam simbolam.
        /// Padara izdrukāto punktu neaktīvu.
        /// </summary>
        /// <param name="all">Drukāt visu</param>
        public void PrintScreen(bool all = false)
        {
            //Console.Clear();
            for (PosY = 0; PosY < ResY; PosY++)
            {
                for (PosX = 0; PosX < ResX; PosX++)
                {
                    if (all || (ActivePoints[PosX, PosY] == true && InactivePoints[PosX, PosY] == false)) // Drukā aktīvos un neiededzinātos simbolus vai visus, izvēloties renderēto krāsu un simbolu.
                    {
                        switch (ScreenColor[PosX, PosY])
                        {
                            case 0:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            case 1:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case 2:
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                        }

                        Console.SetCursorPosition(PosX, PosY);
                        ActivePoints[PosX, PosY] = false;
                        Console.Write(RenderedScreen[PosX, PosY]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                }
                //Console.Write("\n");
            }
            Console.SetCursorPosition(0, ResY - 1);

        }

        /// <summary>
        /// Notīra no ekrāna visu vai tikai nulles, kuras nav iededzinātas un nav arī aktīvas.
        /// Izdrukā mainītos simbolus.
        /// </summary>
        /// <param name="all">Dzēst visu</param>
        public void ClearScreen(bool all = false)
        {
            for (PosY = 0; PosY < ResY; PosY++)
            {
                for (PosX = 0; PosX < ResX; PosX++)
                {
                    if (all || (RenderedScreen[PosX, PosY] == "0" && ActivePoints[PosX, PosY] == false && InactivePoints[PosX, PosY] == false)) // Parasti dzēš nulles, kas nav aktīvas un iededzinātas.
                    {
                        RenderedScreen[PosX, PosY] = " ";
                        Console.SetCursorPosition(PosX, PosY);
                        ActivePoints[PosX, PosY] = false;
                        Console.Write(RenderedScreen[PosX, PosY]);
                    }

                }
                //Console.Write("\n");
            }
            Console.SetCursorPosition(0, ResY);
        }

        /// <summary>
        /// Loģiski un fiziski pārmaina norādīto pozīciju uz norādīto simbolu un krāsu.
        /// Nemaina iededzinātā simbola statusu.
        /// Izdrukā konkrēto simbolu.
        /// </summary>
        /// <param name="x">X Koordināta</param>
        /// <param name="y">Y Koordināta</param>
        /// <param name="s">Simbols</param>
        /// <param name="col">0 - parasta, 1 - sarkana, 2 - zaļa</param>
        public void PutPixel(int x, int y, string s = "#", int col = 0)
        {
            switch (col)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case 1:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            RenderedScreen[x, y] = s;
            ScreenColor[x, y] = col;
            Console.SetCursorPosition(x, y);
            Console.Write(s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Loģiski izdzēš esošo simbolu uz norādīto tukšo simbolu un atiestata simbola krāsu.
        /// Padara esošo lauku par aktīvu un noņem iededzinātā simbola (InactivePoints) statusu.
        /// Neko neizdrukā.
        /// </summary>
        /// <param name="x">X koordināta</param>
        /// <param name="y">Y koordināta</param>
        /// <param name="empty">Tukšais simbols</param>
        public void DestroyPixel(int x, int y, string empty = " ")
        {
            RenderedScreen[x, y] = empty;
            ActivePoints[x, y] = true;
            InactivePoints[x, y] = false;
            ScreenColor[x, y] = 0;
        }


        //public void SmallCircle(int ValX, int ValY, int radius, bool burn = false)    <------    Aizkomentēta funkcija zīmēja nesmuku apli ar norādīto rādiusu un pozīciju. Snake nav nepieciešams, jo bija pamatā eksperimentiem.
        //{
        //    //double Sciradius = Math.Sqrt(radius);

        //    if(radius != 0)
        //    {
        //        for(double SciPosX = Math.Round(-Math.Sqrt(radius)); SciPosX<=Math.Sqrt(radius);SciPosX++)
        //        {
        //            if(SciPosX * SciPosX <= radius)
        //            {
        //                PosY = (int)Math.Round(Math.Sqrt(radius - (SciPosX * SciPosX)));
        //                RenderedScreen[(int)(SciPosX * 1.5 + ValX), PosY + ValY] = "0";                     
        //                //RenderedScreen[(int)(-SciPosX + ValX), PosY + ValY] = " ";
        //                //ActivePoints[(int)(-SciPosX + ValX), PosY + ValY] = true;
        //                //RenderedScreen[(int)(-SciPosX + ValX), -PosY + ValY] = " ";
        //                //ActivePoints[(int)(-SciPosX + ValX), -PosY + ValY] = true;
        //                RenderedScreen[(int)(SciPosX * 1.5 + ValX), -PosY + ValY] = "0";
        //                //if(!burn)
        //                //{
        //                    ActivePoints[(int)(SciPosX * 1.5 + ValX), PosY + ValY] = true;
        //                    ActivePoints[(int)(SciPosX * 1.5 + ValX), -PosY + ValY] = true;
        //                //}  
        //            }                   
        //        }
        //    }           
        //}

        /// <summary>
        /// Loģiski un fiziski pārmaina pirmo pozīciju uz nulli un iekrāso to norādītajā krāsā.
        /// Nomaina iededzinātā simbola statusu uz true pirmajai koordinātei un uz false pēdējajai koordinātei.
        /// Izdrukā pirmās koordinātas simbolu.
        /// </summary>
        /// <param name="snakex">X Koordinātas pēc kārtas</param>
        /// <param name="snakey">Y Koordinātas pēc kārtas</param>
        /// <param name="col">Krāsa pirmajam simbolam. 0 - parasta, 1 - sarkana, 2 - zaļa</param>
        public void Snake(List<int> snakex, List<int> snakey, int col = 0)
        {
            try
            {
                if (RenderedScreen[snakex[0], snakey[0]] != "-" && RenderedScreen[snakex[0], snakey[0]] != "|") // Slikta pārbaude vai darbība nenotiek saskarē ar robežu. (Atstāju šādi, bet varēju likt, lai pārbauda uz StartX StartY un Res.
                {
                    ActivePoints[snakex[0], snakey[0]] = true;
                    PutPixel(snakex[0], snakey[0], "0", col);
                    InactivePoints[snakex[0], snakey[0]] = true;
                    InactivePoints[snakex[snakex.Count - 1], snakey[snakex.Count - 1]] = false;
                }
                else throw OutOfRange;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Izdara norādīto darbību (opt) ar padotajām pozīcijām. Noņem simbolu, ievieto simbolu, vai iededzina visus simbolus izņemot pēdējo.
        /// </summary>
        /// <param name="snakex">X Koordinātas pēc kārtas</param>
        /// <param name="snakey">Y Koordinātas pēc kārtas</param>
        /// <param name="col">Krāsa. 0 - parasta, 1 - sarkana, 2 - zaļa</param>
        /// <param name="opt">Veids. 0 - izdzēst visus simbolus norādītajās koordinātās, 1 - ievietot konkrētu simbolu ar konkrētu krāsu norādītajās koordinātās, 2 - padarīt simbolus izņemot pēdējo norādītās koordinātās iededzinātus</param>
        public void EditSnake(List<int> snakex, List<int> snakey, int col = 0, int opt = 0)
        {
            try
            {
                for (int pos = 0; pos < snakex.Count; pos++)
                {
                    if (RenderedScreen[snakex[pos], snakey[pos]] == "0" || opt == 2)
                    {
                        switch (opt)
                        {
                            case 0:
                                DestroyPixel(snakex[pos], snakey[pos]);
                                break;
                            case 1:
                                PutPixel(snakex[pos], snakey[pos], "0", col);
                                break;
                            case 2:
                                PutPixel(snakex[pos], snakey[pos], "0", col);
                                if (snakex.Count > 2 && pos < snakex.Count - 1)
                                {
                                    InactivePoints[snakex[pos], snakey[pos]] = true;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw OutOfRange;
            }
        }
    }
}
