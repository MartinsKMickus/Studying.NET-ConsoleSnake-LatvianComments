using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading;

namespace _2020_11_15_Snake
{
    public class Snake
    {
        Exception WrongRes = new Exception("To start new game old save has to be deleted!\nReason is because of too small resolution of the screen or too large console symbols."); // Nevēlos rakstīt latviešu simbolus izpildāmā kodā, tāpēc rakstu angliski.
        Exception HigherRes = new Exception("Looks You are trying to use higher resolution than previously recorded\nIf You are going to start and save the game it won't work for previous resolution."); //
        Exception MissingFiles = new Exception("Missing Audio Files"); //
        public List<int> snakex = new List<int>(), snakey = new List<int>(), tarx = new List<int>(), tary = new List<int>(); // snakex un snakey (čūskas atrašanās vieta) ir kā divdimensionāls masīvs, lai to var saglabāt failā. tarx un tary ir ēdiena atrašanās vietas.

        AudioFileReader audio; // Zaudējuma.
        AudioFileReader audio1; // Bonusa.
        WaveOutEvent LoseEvent, BonusEvent; // Atskaņošanai.

        int posxtar, posytar, startposx = 2, startposy = 2, XSpeed = 100, YSpeed = 130, XBonSpeed = 50, YBonSpeed = 80, Points = 0; // Speed norāda noklusējuma ātrumus milisekundēs katra kadra nomaiņai.
        public int SXRes = -1, SYRes = -1, SetupQuantity = 1, SetupMQuantity = 10, SetupTriggerMove = 5, BonussX = -1, BonussY = -1, Moves = 0, col = 0, RecSpeed = 100, snakeCol = 0, LastM = -1, PrevHScore = 0;
        public bool Stop = true; // Norāda vai jāapstājas koda izpildei.
        ScreenRender Render;

        public int NotificationY { get { return Render.resy + 3; } }
        public Snake()
        {
            WrongRes.HResult = 1;
            HigherRes.HResult = 2;
            BonusEvent = new WaveOutEvent();
            LoseEvent = new WaveOutEvent();
            try
            {
                audio = new AudioFileReader("Sounds/Lose.m4a");
                audio1 = new AudioFileReader("Sounds/Bonus.m4a");
                LoseEvent.Init(audio);
                BonusEvent.Init(audio1);
            }
            catch (Exception)
            {
                throw MissingFiles;
            }
            LoseEvent.Volume = 0.4f;
            BonusEvent.Volume = 0.3f;
            Render = new ScreenRender();
            SXRes = Render.resx;
            SYRes = Render.resy;
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 5);
            Console.Write("Score: ");
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 7);
            Console.Write("Previous High Score: ");
        }
        /// <summary>
        /// Veic darbību, lai atskaņotu zaudējuma skaņu.
        /// </summary>
        void LoseSound()
        {
            try
            {
                if (audio != null)
                {
                    audio.Position = 0;
                    LoseEvent.Play();
                }
                while (LoseEvent.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(6200);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        /// <summary>
        /// Veic darbību, lai atskaņotu bonusa skaņu.
        /// </summary>
        void BonusSound()
        {
            try
            {
                if (audio1 != null)
                {
                    audio1.Position = 0;
                    BonusEvent.Play();
                }
            }
            catch (Exception)
            {
                return;
            }
        }
        /// <summary>
        /// Funkcija Čūskas iesākšanai, kas ievieto pirmo simbolu un izdrukā ekrānu.
        /// </summary>
        public void Start()
        {
            snakex.Add(startposx + Render.startx);
            snakey.Add(startposy + Render.starty);
            Render.Snake(snakex, snakey);
            AddBite();
            Render.PrintScreen();
        }
        /// <summary>
        /// Funkcija rezultāta izdrukai.
        /// </summary>
        void Score()
        {
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 5);
            Console.Write("Score: ");
            Console.Write(Points);
        }
        /// <summary>
        /// Jāveic, kad notiek saglabātas spēles ielāde, lai visu izdrukātu.
        /// </summary>
        /// <param name="HRes">Ignorēt pāreju no mazas izšķirtspējas uz lielu.</param>
        public void Refresh(bool HRes = true) // Ja iepriekš saglabāts progress ar bonusu, tad tas netiek ielādēts.
        {
            if (Render.resx < SXRes || Render.resy < SYRes) // Izšķirtspēju nesaskaņu pārbaude
            {
                throw WrongRes;
            }
            if (HRes && (Render.resx > SXRes || Render.resy > SYRes)) // Izšķirtspēju nesaskaņu pārbaude
            {
                throw HigherRes;
            }
            SXRes = Render.resx;
            SYRes = Render.resy;
            Render.ClearScreen(false);
            Points = snakex.Count - 1;
            Render.EditSnake(snakex, snakey, snakeCol, 2);

            for (int pos = 0; pos < tarx.Count; pos++) // Ēdiena izdruka
            {
                Render.PutPixel(tarx[pos], tary[pos]);
            }
            //for (int pos = 0; pos < tarx.Count - 1; pos++) // Ēdiena izdruka
            //{
            //    Render.PutPixel(tarx[pos], tary[pos]);
            //}
            for (int pos = 0; pos < snakex.Count - 1; pos++)
            {
                if (snakex[pos] == snakex[pos + 1] && snakey[pos] == snakey[pos + 1])
                {
                    Render.PutPixel(snakex[pos], snakey[pos], "0", 1);
                }
            }
            if (BonussX != -1)
            {
                Render.PutPixel(BonussX, BonussY, "@", 2);
            }
            Score();
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 7);
            Console.Write("Previous High Score: ");
            Console.Write("            ");
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 7);
            Console.Write("Previous High Score: ");
            Console.Write(PrevHScore);
            Render.PrintScreen();
        }
        /// <summary>
        /// Restartē progresu un sāk no nulles, protams, saglabājot augstākos punktus. (Easter egg: Netiek mainīts maksimālo ēdienu skaits, kas var tikt uzģenerēts. Katru reizi būs arvien vairāk līdz neizdzēsīs saglabāto spēli.)
        /// Ja viss laukums tiek aizpildīts iespējams notiks apstāšanās pie AddBite() funkcijas, jo varētu būt ieciklošanās.
        /// </summary>
        void Reset()
        {
            if (BonusEvent.PlaybackState == PlaybackState.Playing)
            {
                BonusEvent.Stop();
            }
            Stop = true;

            LoseSound();


            //outEvent.Stop();
            if (Points > PrevHScore) PrevHScore = Points;
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 5);
            Console.Write("Score: ");
            Console.Write("            ");
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 7);
            Console.Write("Previous High Score: ");
            Console.Write("            ");
            Console.SetCursorPosition(Render.resx + 5, Render.starty + 7);
            Console.Write("Previous High Score: ");
            Console.Write(PrevHScore);
            Points = 0;
            RecSpeed = 100;
            snakeCol = 0;
            LastM = -1;
            Render.EditSnake(snakex, snakey);
            snakex.Clear();
            snakey.Clear();
            snakex = new List<int>();
            snakey = new List<int>();
            snakex.Add(startposx + Render.startx);
            snakey.Add(startposy + Render.starty);
            Render.Snake(snakex, snakey);
            Render.ClearScreen(false);
            Render.PrintScreen();
        }
        /// <summary>
        /// Funkcija, ja grib padarīt ēdienu kustīgu, netiek izmantota.
        /// </summary>
        public void MoveBite()
        {
            Random random = new Random();
            int move = 0;
            for (int pos = 0; pos < tarx.Count; pos++)
            {
                move = random.Next(0, 3);
                Render.DestroyPixel(tarx[pos], tary[pos]);
                switch (move)
                {
                    case 0:
                        tarx[pos]--;
                        //tary[pos];
                        break;
                    case 1:
                        tarx[pos]++;
                        //tary[pos];
                        break;
                    case 2:
                        //tarx[pos]--;
                        tary[pos]--;
                        break;
                    case 3:
                        //tarx[pos]--;
                        tary[pos]++;
                        break;
                }

                try
                {
                    Render.PutPixel(tarx[pos], tary[pos]);
                }
                catch (Exception) // Ja ēdiens iziet no robežām, tad tas tiek iznīcināts.
                {

                    tarx.RemoveAt(pos);
                    tary.RemoveAt(pos);
                }
            }
        }
        /// <summary>
        /// Ēdiena pievienošana. Laukumam tiks pievienoti ēdieni, lai tie būtu SetupQuantity daudzumā.
        /// </summary>
        public void AddBite()
        {
            bool Fake_News = false, Fake_News1, Fake_News2;
            int q = SetupQuantity;
            Random random = new Random();
            while (q-- > tarx.Count)
            {
                if (BonussX == -1 && Points % 10 == 0 && snakeCol != 2) // Bonuss.
                {
                    do
                    {
                        Fake_News1 = false;
                        BonussX = random.Next(Render.startx + 2, Render.resx - 2);
                        BonussY = random.Next(Render.starty + 3, Render.resy - 3);
                        for (int pos = 0; pos < snakex.Count; pos++) // Pārbaude vai neieģenerējas čūskā.
                        {
                            if (BonussX == snakex[pos] && BonussY == snakey[pos])
                            {
                                Fake_News1 = true;
                                break;
                            }
                        }
                    } while (Fake_News1);
                    Render.PutPixel(BonussX, BonussY, "@", 2);
                }
                do // Ēdiens.
                {
                    Fake_News2 = false;
                    posxtar = random.Next(Render.startx + 2, Render.resx - 2);
                    posytar = random.Next(Render.starty + 3, Render.resy - 3);
                    for (int pos = 0; pos < snakex.Count; pos++) // Pārbaude vai neieģenerējas čūskā.
                    {
                        if (posxtar == snakex[pos] && posytar == snakey[pos])
                        {
                            Fake_News2 = true;
                            break;
                        }
                    }
                } while (posxtar == BonussX && posytar == BonussY || Fake_News2);
                for (int pos = 0; pos < tarx.Count; pos++) // Pārbaude vai neieģenerējas jau esošā ēdienā.
                {
                    if (tarx[pos] == posxtar && tary[pos] == posytar)
                    {
                        Fake_News = true;
                        q++;
                    }
                }
                if (!Fake_News)
                {
                    tarx.Add(posxtar);
                    tary.Add(posytar);
                    Render.PutPixel(posxtar, posytar);
                }
                Fake_News = false;
            }
            Render.ClearScreen(false);
            Render.PrintScreen();
        }
        /// <summary>
        /// Čūskas atrašanās vietas pārvietošana par vienu uz priekšu. Virziena neatkarīgi.
        /// </summary>
        void Move()
        {
            for (int pos = snakex.Count - 1; pos >= 1; pos--)
            {
                snakex[pos] = snakex[pos - 1];
                snakey[pos] = snakey[pos - 1];
            }
        }
        /// <summary>
        /// Pārbaude uz to, kas atrodas čūskas pirmajā laukā.
        /// </summary>
        /// <returns></returns>
        bool IsFood()
        {
            for (int pos = 1; pos < snakex.Count; pos++)
            {
                if (snakex[0] == snakex[pos] && snakey[0] == snakey[pos]) // Čūska saskrienās ar sevi.
                {
                    Reset();
                    return false;
                }
            }
            if (BonusEvent.PlaybackState != PlaybackState.Playing && (RecSpeed == XBonSpeed || RecSpeed == YBonSpeed)) // Bonusa skaņa ir beigusi skanēt.
            {
                RecSpeed = YSpeed;
                snakeCol = 0;
            }
            if (BonussX == snakex[0] && BonussY == snakey[0]) // Čūska saskrējās ar bonusa lauciņu.
            {
                BonusSound();
                BonussX = -1;
                BonussY = -1;
                snakeCol = 2;
                Render.EditSnake(snakex, snakey, 2, 1);
                RecSpeed = 50;
            }
            for (int pos = 0; pos < tarx.Count; pos++) // Ēdienu pārbaude.
            {   
                if (tarx[pos] == snakex[0] && tary[pos] == snakey[0]) // Čūska saskrējās ar ēdiena lauciņu.
                {
                    new Thread(() => Console.Beep()).Start();
                    col = 1;
                    posxtar = tarx[pos];
                    posytar = tary[pos];
                    tarx.RemoveAt(pos);
                    tary.RemoveAt(pos);

                    if (tarx.Count < SetupQuantity) AddBite(); // Jaunu ēdienu pievienošana.

                    if (RecSpeed == XBonSpeed || RecSpeed == YBonSpeed) // Bonusa effekts.
                    {
                        BonusEvent.Stop();
                        Random random = new Random();
                        SetupQuantity = random.Next(1, SetupMQuantity);
                        SetupMQuantity += 20;
                        AddBite();
                        SetupQuantity = 1;
                        RecSpeed = YSpeed;
                        snakeCol = 0;
                        IncreaseSize();
                        IncreaseSize();
                        IncreaseSize();
                    }
                    else
                        IncreaseSize();
                    Score();
                    return true;
                }
            }
            col = 0;
            return false;
        }
        /// <summary>
        /// Čūskas izmēra palielināšana.
        /// </summary>
        public void IncreaseSize()
        {
            Points++;
            snakex.Insert(0, snakex[0]);
            snakey.Insert(0, snakey[0]);
        }
        /// <summary>
        /// Pārvieto čūsku uz augšu, pārbaudot arī virzienu uz kuru tā devās iepriekš (savādāk uz leju).
        /// </summary>
        public void MoveUp()
        {
            if (RecSpeed == XSpeed)
            {
                RecSpeed = YSpeed;
            }
            if (RecSpeed == XBonSpeed)
            {
                RecSpeed = YBonSpeed;
            }
            if (LastM == 2)
            {
                MoveDown();
                return;
            }
            LastM = 0;
            try
            {
                Move();
                snakey[0]--;
                IsFood();
                if (col == 0)
                {
                    Render.Snake(snakex, snakey, snakeCol);
                }
                else
                    Render.Snake(snakex, snakey, col);
                Render.ClearScreen(false);
                Render.PrintScreen();
            }
            catch (Exception)
            {
                Reset();
            }
        }
        /// <summary>
        /// Pārvieto čūsku uz leju, pārbaudot arī virzienu uz kuru tā devās iepriekš (savādāk uz augšu).
        /// </summary>
        public void MoveDown()
        {
            if (RecSpeed == XSpeed)
            {
                RecSpeed = YSpeed;
            }
            if (RecSpeed == XBonSpeed)
            {
                RecSpeed = YBonSpeed;
            }
            if (LastM == 0)
            {
                MoveUp();
                return;
            }
            LastM = 2;
            try
            {
                Move();
                snakey[0]++;
                IsFood();
                if (col == 0)
                {
                    Render.Snake(snakex, snakey, snakeCol);
                }
                else
                    Render.Snake(snakex, snakey, col);
                Render.ClearScreen(false);
                Render.PrintScreen();
            }
            catch (Exception)
            {
                Reset();
            }
        }
        /// <summary>
        /// Pārvieto čūsku pa kreisi, pārbaudot arī virzienu uz kuru tā devās iepriekš (savādāk pa labi).
        /// </summary>
        public void MoveLeft()
        {
            if (RecSpeed == YSpeed)
            {
                RecSpeed = XSpeed;
            }
            if (RecSpeed == YBonSpeed)
            {
                RecSpeed = XBonSpeed;
            }
            if (LastM == 3)
            {
                MoveRight();
                return;
            }
            LastM = 1;
            try
            {
                Move();
                snakex[0]--;
                IsFood();
                if (col == 0)
                {
                    Render.Snake(snakex, snakey, snakeCol);
                }
                else
                    Render.Snake(snakex, snakey, col);
                Render.ClearScreen(false);
                Render.PrintScreen();
            }
            catch (Exception)
            {
                Reset();
            }
        }
        /// <summary>
        /// Pārvieto čūsku pa labi, pārbaudot arī virzienu uz kuru tā devās iepriekš (savādāk pa kreisi).
        /// </summary>
        public void MoveRight()
        {
            if (RecSpeed == YSpeed)
            {
                RecSpeed = XSpeed;
            }
            if (RecSpeed == YBonSpeed)
            {
                RecSpeed = XBonSpeed;
            }
            if (LastM == 1)
            {
                MoveLeft();
                return;
            }
            LastM = 3;
            try
            {
                Move();
                snakex[0]++;
                IsFood();
                if (col == 0)
                {
                    Render.Snake(snakex, snakey, snakeCol);
                }
                else
                    Render.Snake(snakex, snakey, col);
                Render.ClearScreen(false);
                Render.PrintScreen();
            }
            catch (Exception)
            {
                Reset();
            }
        }
    }
}
