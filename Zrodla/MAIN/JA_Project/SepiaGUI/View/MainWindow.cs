
///////
/* Autor: Michał Jankowski
* Dzień utworzenia: 13.12.2019r.
* Przedmiot: Języki Asemblerowe
* Informatyka
* Semestr: 5
* Grupa dziekańska: 1
* Sekcja: 2
* Temat: Efekt Sepii
*/
////////////////////////////////////////
//////

//Changelog:
//version 0.1 19/10/19r.
// Utworzenie GUI z niezbędnymi komponentami wizualnymi m.in. textbox, button oraz imagebox.
// Wstępna koncepcja wyglądu GUI dla użytkownika

// v. 0.2   20/10/19r.
// Dodanie niezbędnych metod pod obsługę wydarzeń (eventów) z odpowiednich przycisków oraz komponentów GUI.
// Sprawdzenie poprawności ich działania oraz wstępne wyświetlanie informacji w logu RichTextBoxa.

// v. 0.3  26/10/19r.
// Implementacja komunikacji użytkownika z dllkami c# i asm.
// Przekazywanie tablicy do sprawdzenia działania transferu danych do dllek.
// Stwierdzono poprawność przekazywancyh danych. Dodano również metody odpowiedzialne za przetwarzanie obrazka do obiektu Bitmap i jego przetworzenie
// za pomocą metody Marshal Copy do tablicy bajtów dla algorytmu Sepii 
//
//v. 0.4  5/11/19r.
// Dodanie podziału na wątki w aplikacji i przekazywanie ich działania do dllek. Sprawdzanie czy nie wystąpi hazard.
// Dodanie przedziału, w celu uniknięcia wyścigu wątków podczas koloryzacji obrazu przez algorytm.
//
// v. 0.5  29/11/19r. 
// Dodanie wstępnych komentarzy oraz poprawa przejrzystości kodu poprzez jego odpowiednie odzielenie od siebie
// Przekazano również mniej ważne metody z punktu widzenia algorytmu do klasy imageAndEnvironmentalDataModel 
//
// v. 1.0 11/01/19r.
// Ostateczna kontrola poprawności wykonywanych akcji, wydarzeń przez przyciski GUI
// Dodanie metody sprawdzającej funkcjonalność obsługiwanych instrukcji procesora utworzonych w asemblerze.
// Wyłączenie możliwości przetwarzania w asemblerze w przypadku braku niezbędnych funkcjonalności.
// Dodanie również kontroli frameworka .NET programu oraz zakmnięcie aplikacji w przypadku przestarzałej wersji lub jej braku.


// wykorzystywane przestrzenie nazw
using SepiaGUI.Model;  
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// przestrzeń nazw SepiaGUI.Model
// parametry wejściowe znajdują się w adnotacjach <param name>, brak tej adnostacjii oznacza brak parametrów wejściowych
// parametry wyjściowe w adnotacji <returns> lub returns
// przestrzeń nazw SepiaApp
namespace SepiaApp
{
                                                                                                             // Klasa główna programu. Wykonuję wszystkie operacje związane z widokiem oraz przetwarzaniem modelu.
                                                                                                             // Tutaj wywoływane są dllki do wykonania algorytmu Sepii
    public partial class SepiaProgram : Form
    {   
                                                                                                             // zmienna dla talicy bajtów obrazka
        byte[] RGBValuesOfImage;
                                                                                                            // obiekt tablicy wątków
        Thread[] arrayOfThreads;
                                                                                                            // obiekt tablicy obiektów danych do przekazania jako parametry dllki
        object[] arrayOfArguments;

                                                                                                            // obiekt klasy imageAndEnvironmentalDataModel do tworzenia niezbędnych metod dla modelu i widoku
        readonly ImageAndEnvironmentalDataModel model = new ImageAndEnvironmentalDataModel();
        
                                                                                                            // import asm dllki przetwarzania efektu sepii obrazka
       [DllImport("SepiaAsmDll.dll")]
        /// <summary>
        ///  import asm dllki do wykonania efektu sepii
        /// </summary>
        /// <param name="tab"> wskaźnik byte* tablicy bajtów</param>
        /// <param name="start"> wartość int zmiennej wartości początka przedziału</param>
        /// <param name="stop"> wartość int zmiennej wartości końca przedziału</param>
        /// <param name="toneValue"> wartość  int zmiennej współczynnika wypełnienia sepii</param>
        /// <returns> void </returns> 
        unsafe public static extern void Sepia(byte* tab, int start,int stop, int toneValue, int depthValue);
                                                                                                            
        [DllImport("SepiaAsmDll.dll")]
        /// <summary>
        /// import asm dllki do sprawdzenie kompatybilności obsługi instrukcji AVX w rejestrze ecx
        /// </summary>
        /// <param name="check"> Wartość szukanej funkcjonalności w procesorze</param>
        /// <returns> wartość bool prawda lub fałsz danej funkcjonalności</returns>
        public static extern bool DetectFeatureECX(int check);
                                                                                                           
        [DllImport("SepiaAsmDll.dll")]
        /// <summary>
        /// import Asm dllki do sprawdzenie kompatybilności obsługi instrukcji MMX w rejestrze edx 
        /// </summary>
        /// <param name="check"> Wartość szukanej funkcjonalności w procesorze</param>
        /// <returns>wartość bool prawda lub fałsz danej funkcjonalności</returns> 
        public static extern bool DetectFeatureEDX(int check);
                  
        /// <summary>
        /// Konsturktor klasy SepiaProgram() wykonujący metodę inicjalizującą główne komponenty widoku
        /// </summary>
        /// <returns> konstruktor w C# nie może niczego zwracać </returns>
        public SepiaProgram()
        {
         
            InitializeComponent();                                                                          // metoda inicjalizująca główne komponenty programu. Utworzona automatycznie.
        }

        /// <summary>
        ///  metoda wykonywana podczas włączenia okna
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns> void </returns> 
        private void WindowLoad(object sender, EventArgs e)
        {                                                                                                   
                                                                                                             // metoda, która zlicza ilość fizycznych procesorów
            model.CountPhysicalProcessors(physicalProcessorsLabel);
                                                                                                             // metoda, która zlicza liczbę rdzeni w CPU
            model.CountCores(coresLabel);
                                                                                                             // metoda, któr zlicza liczbę logicznych procesorów
            model.SetLogicalProcessors(logicalProcessorsLabel, model.CountLogicalProcessors(false));
                                                                                                             // ustawia liczbę optymalnych wątków dla aplikacji
            ActiveThreadsComboBox.SelectedItem = model.CountLogicalProcessors(true);
                                                                                                             // ustawia domyślną wartość sepii na 0
            SepiaComboBox.SelectedItem = "0";
                                                                                                             // ustawia domyślną wartość głębokości wypełnienia sepii an 0
            SepiaDepthCombobox.SelectedItem = "0";                                                           
                                                                                                             // ustawia domyślną wartość radiobutton dla dll, domyślnie dla C#
            CsharpRadioButton.Checked = true;
                                                                                                             // sprawdza obsługę MMX i AVX oraz w przypadku braku obsługi instrukcji uniemożliwia jej wykonanie
            if(CheckCompabilites() == false)
            {
                                                                                                             // informacja w przypadku braku obsługi asemblera
                AsmRadioButton.Enabled = false;                                                              // wyłączenie obsługi radiobuttona ASM
                MessageBox.Show("Processor do not support MMX or AVX instructions set", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                                                                                                             // sprawdza wersję frameworku i zamyka aplikacje gdy jej brakuje
            if(ImageAndEnvironmentalDataModel.CheckFrameworkVersion() == false)
            {
                                                                                                             // obsługa odpowiedniej wersji frameworka dla programu
                MessageBox.Show("Outdated version of .NET framework. Version 4.8 or newer is required!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Windows.Forms.Application.Exit();                                                    // zamknięcie okna aplikacji
            }

        }

        /// <summary>
        ///  metoda sprawdzająca kompatybilność MMX oraz AVX wykorzystując asemblerową dllke
        /// </summary>
        /// <returns> bool wartość danej funkcjonalności</returns>
        private bool CheckCompabilites()
        {
            if (DetectFeatureEDX(23) == true && DetectFeatureEDX(25) == true && DetectFeatureEDX(26) == true)     // metoda asemblerowa sprawdzająca czy są obsługiwane instrukcje MMX 
            {

                if (DetectFeatureECX(28) == true && DetectFeatureECX(19) == true && DetectFeatureECX(20) == true)  // metoda asemblerowa sprawdzająca czy są obsługiwane instrukcje AVX
                {

                    return true;                                                                                   // zwraca prawdę w przypadku posiadania funkcjonalności
                }

            }
            return false;                                                                                          // zwraca fałsz w przypadku nie posiadania funkcjonalności


        }

        /// <summary>
        ///  metoda do ładowania obrazka za pomocą przycisku upload image
        /// </summary>
        /// <param name="sender"> obiekt wysyłanej akcji w przypadku wciśnięcia przycisku</param>
        /// <param name="e"> Obiekt przekazujący informacje o wciśniętym przycisku</param>
        /// <returns> void </returns> 
        private void UploadImageButton(object sender, EventArgs e)
        {
                                                                                                         // obiekt wykorzystywany do utworzenia list rozwijanej przy wybieraniu obrazka

           
               
                                                                                                            // ustawienie obsługiwanych formatów obrazków. Są to: bmp. 
                model.SetImageFilter(openFileDialogDisplay);
              try                                                                                         // blok try catch do zabezpieczenia przed ewentualnym błędem pliku
                {                                                                                        // jeżeli otworzono listę rozwijaną, wybierz obrazek i zainicjalizuj go w UI jako Image oraz dla dalszej obługi jako Bitmape
                    if (openFileDialogDisplay.ShowDialog() == DialogResult.OK)
                    {
                        using( FileStream file = new FileStream(openFileDialogDisplay.FileName, FileMode.Open, FileAccess.Read))            // wykorzystanie obiektu typu FileStream do otwarcia pliku ze strumienia
                    {
                        using(Image image = Image.FromStream(stream: file,                                                                  // otwarcie okna programu
                                                             useEmbeddedColorManagement:false,                                              // obiekt Image do pobrania rozmiaru pliku
                                                             validateImageData: false))
                        {        
                            float width = image.PhysicalDimension.Width;                                                                    // zmienna szerokości do sprawdzenia fizycznego rozmiaru długości pliku
                            float height = image.PhysicalDimension.Height;                                                                  // zmienna wysokości do sprawdzenia fizycznego rozmiaru wysokości pliku
                            
                            if(width * height * 8 > 350000000)                                  // sprawdzenie przed załadowaniem obrazka czy jego rozmiar jest odpowiedni dla aplikacji, założono iż pliki o ilości wartości większych niż 350 mln wartości mogą być niebezpieczne do przetworzenia
                            {
                                DialogResult dialogResult = MessageBox.Show("File dimension is huge!  Proceed at your own risk that it may fail to load/convert image!", "Warning", MessageBoxButtons.YesNo);
                                

                                 if(dialogResult == DialogResult.No)                           // jeżeli użytkownik nie podjął się próby konwersji obrazka to powrót do głównej pętli programu i anuluj dalsze wykonywanie konwersji
                                {
                                    return;                                                     //  wyjście z funkcji programu działania i powrót do głównej pętli
                                }


                            }

                        }
                    }
                        uploadImageTextBox.Text = openFileDialogDisplay.FileName;                                        //  zapamiętanie nazwy obrazka z OpenFIleDialog (listy rozwijanej) wybranego przez użytkownika

                        InsertImage.SizeMode = PictureBoxSizeMode.StretchImage;                                          // dopasowanie załadowanego obrazka do ImageBoxa w programie

                        InsertImage.Image = model.ConvertToBitmap(openFileDialogDisplay.FileName);                       // konwersja obrazka załadowanego wcześniej prez użtkownika do bitmapy
                   
                    }
                }
                catch (Exception)                                                                              // łapanie wyjątku w przypadku błędu podczas ładowania obrazka oraz wyswietlanie informacji o nim użytkownikowi
            {
                    MessageBox.Show("Error while loading the image !  File was corrupted or of incorrect file extension! ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  //Informacja o  błędzie w czasie konwersji, Error
                   
                }
            

        }

        /// <summary>
        ///  metoda obsługująca konwersję obrazka do Sepii za pomocą przycisku convert// metoda obsługująca konwersję obrazka do Sepii za pomocą przycisku convert
        /// </summary>
        /// <param name="sender"> obiekt wysyłanej akcji w przypadku wciśnięcia przycisku</param>
        /// <param name="e"> Obiekt przekazujący informacje o wciśniętym przycisku </param>
        /// <eturns> void </eturns> 
        private void ConvertImageButton(object sender, EventArgs e)
        {
            int filterValue = 0;                                                                               // zmienna lokalna do przechowywania informacji o wypełnieniu sepią dla algorytmu. Domyślnie wartość 0.

            int depthValue = 0;                                                                                // zmienna lokalna do przechowywania informacji o wartości głębii efetu sepii dla algorytmu. Domyślnie wartość 0.

            Bitmap bitmapImage;                                                                                 // obiekt Bitmap przechowujący informacje o skonwertowanym obrazku przez algorytm dllki.

                                                                                                               // czyszczenie za każdym załadowaniem obrazka poprzedniego starego obrazka
            model.ClearImage(ConvertImage);

                                                                                                              // przepisywanie obiektu zdjęcia
            bitmapImage = new Bitmap(InsertImage.Image);
            
                try
                {
                                                                                                              // wybranie wartości sepii w comboboxie
                    if (SepiaComboBox.SelectedItem != null)                                                   // nie wyrano wartości dla intensywności Sepii
                    {
                                                                                                              // przepisywanie wartości intensywności sepii do zmiennej z comboboxa
                        filterValue = int.Parse(SepiaComboBox.SelectedItem.ToString());
                    }

                    if (SepiaDepthCombobox.SelectedItem != null)                                               //  wartość głębi Sepii
                    {
                        depthValue = int.Parse(SepiaDepthCombobox.SelectedItem.ToString());                   // ustaw wartość głębi sepii z comboboxa
                    }



                    if (SepiaBitmap(bitmapImage, filterValue, depthValue))                                        // metoda wyciągająca z obrazka niezbędne dane i rozpoczynająca konwersję na Sepie oraz jednocześnie ustawia boola czy skonwertowano obrazek                                     
                    {                                                                                             // ustawienie rozmiaru okna obrazka do "rozciągnietego"
                        ConvertImage.SizeMode = PictureBoxSizeMode.StretchImage;
                                                                                                                 // ustawienie nowego obrazka w UI
                        ConvertImage.Image = bitmapImage;
                        if (System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))  // sprawdzenie czy ścieżka do pliku istnieje
                        {
                            bitmapImage.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//Converted_Image.bmp");
                        }
                                                                                                                  // zapisywanie obrazka o formacie Bmp jako "Converted_Image"
                    }
                
                }
                catch (Exception)                                                                                  // łapanie wyjątków podczas błędów ładowania konwersji obrazka aplikacji oraz wyświetlanie informacji o błędzie dla użytkownika
                {
                    MessageBox.Show("Error while trying to convert image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);   // wyświetlenie wiadomości o niepowodzeniu konwersji

                }

                model.DisableButton(2, ConvertButton);                                                              // wylączenie przycisku na 2 sekundy    
          
        }


        /// <summary>
        /// metoda wyciągająca tablice bajtów z obrazka oraz przekazująca je do obróbki w dllce
        /// </summary>
        /// <param name="bitmapImage">  przekazany obraz do przetworzenia Sepii </param>
        /// <param name="toneValue"> wartość zmiennej dla współczynnika wypełnienia </param>
        /// <returns>wartość bool prawda lub fałsz czy udało się skonwertować obrazek </returns> 
        private bool SepiaBitmap(Bitmap bitmapImage, int toneValue, int depthValue)
        {
          
            Rectangle rectangle = new Rectangle(0, 0, bitmapImage.Width, bitmapImage.Height);                    // rozmiar obrazka jako x * y, gdzie x to szerokość i y to wysokość
                                                                                                                
            BitmapData data = bitmapImage.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);  // dane o stosowanej konwecji ARGB
                                                                                                                    
            int depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;                                         // głębokość obrazka, czyli ilość bajtów przypadająca na  dany piksel

            int smallImageSizeMultiplier = 2048;                                                                 // zmienna do powiększania obrazka, aby poprawnie wykonać algorytm, 2048 =  32 * 64  = 2048, gdzie 32 to minimalna ilość przetwarzanych w jednym obiegu wartości w tablicy bajtów,a  64 to  maksymalna ilość wątków


            RGBValuesOfImage = new byte[data.Width * data.Height * depth];                                      // tworzenie tablicy bajtów obrazka na podstawie jego rozmiaru oraz ilości bajtów na piksel

            if (RGBValuesOfImage.Length < smallImageSizeMultiplier)                                              // warunek sprawdzający czy obrazek jest za mały
            {
                MessageBox.Show("Image is too small to be converted! Please provide bigger image at least  64 x 64!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);  // komunikat wyświetlający informacje o zbyt małym zadanym obrazie przez użytkownika do przetworzenia

                bitmapImage.UnlockBits(data);                                                                       // odblokowanie bitów tablicy danych obrazka         
                return false;                                                                                       // zwrócenie fałszu jako informacja o niepowodzeniu konwersji
            }
            try {
                                                                                                                  // wykorzystywanie Marshal.Copy do zablokowanie na wyłączność podanej tablicy i skopiowanie danych z zdjęcia do niej   
             Marshal.Copy(data.Scan0, RGBValuesOfImage, 0, RGBValuesOfImage.Length);
                                                                                                                  // metoda rozpoczynająca konwersje obrazka w dll
             InvokeDll(CsharpRadioButton, AsmRadioButton, model.ConvertComboboxItemSelected(ActiveThreadsComboBox), toneValue, depthValue);
                                                                                                                   // zakończenie pracy zegara
              model.StopWatch();
                                                                                                                   // wpisanie do logRichBoxa długości konwersji w mikrosekundach
                
              model.ParseToRichTextBox(LogRichTextBox,model.ConvertComboboxItemSelected(ActiveThreadsComboBox), AsmRadioButton,CsharpRadioButton,toneValue,depthValue, bitmapImage);

              Marshal.Copy(RGBValuesOfImage, 0, data.Scan0, RGBValuesOfImage.Length);                           // kopiowanie zawartości tablicy RGBValuesOfImage do obrazka, aby załadować skonwertowane piksele  do tablicy bajtów obrazka dla zwykłego obrazka                                                                                                                              

             this.Invalidate();                                                                                 // informacja  dla programu (GUI) o załadowaniu nowych warotści pikseli do obrazka i przerysowanie obrazu

                                                                                                                // odblokowanie bitów tablicy danych obrazka
            bitmapImage.UnlockBits(data);

            }
            catch (Exception)                                                                                   // wyjątek w przypadku błedu podczas konwersji obrazka
            {

                bitmapImage.UnlockBits(data);                                                                   // odblokowanie bitów tablicy danych obrazka
                MessageBox.Show("There was an error during image conversion!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // komunikat wyświetlający informacje o błędzie podczas konwersji obrazka do przetworzenia m.in. uszkodzony obraz, sztucznie podmieniony format na .jpg lub .bmp
                return false;                                                                                       // zwrócenie fałszu w przypadku niepowodzenia konwersji
            }
            

            RGBValuesOfImage = null;                                                                                // ustawienie null dla tablicy bajtów obrazka dla Garbage Collectora do dealokacji zbędnych wartości

            arrayOfArguments = null;                                                                                // ustawienie null dla obiektów tablicy argumentów dla Garbage Collectora do dealokacji zbędnych wartości
     
            arrayOfThreads = null;                                                                                  // ustawienie null dla obiektów wątków dla Garbage Collectora do dealokacji zędbych wartości

            return true;                                                                                            // zwrócenie prawdy w przypadku powodzenia konwersji
        }

       
        /// <summary>
        /// Metoda przygotowująca delegaty i interwały wątków
        /// </summary>
        /// <param name="csharp"> radio button dla dllki C# w GUI </param>
        /// <param name="asm"> radio button dla dllki asm w GUI</param>
        /// <param name="threads"> wartość zmiennej int ilości wybranych wątków z comboboxa</param>
        /// <param name="toneValue">  wartość zmiennej współczynnika wypełnienia sepii </param>
        /// <returns> void </returns> 
        public void PrepareDelegates(RadioButton csharp,RadioButton asm, int threads, int toneValue, int depthValue) 
        {
                                                                                                                        // warunek sprawdzający czy nie przekazano żadnej tablicy bajtów obrazka albo tablica jest pusta
            if (RGBValuesOfImage == null)
                return;
            arrayOfThreads = new Thread[threads];                                                                       // tworzenie pustej tablicy wątków na podstawie podanej ilości wątków przez użytkownika

            arrayOfArguments = new object[threads];                                                                     // tworzenie tablicy argumentów potrzebnych do przetworzenia algorytmu przez użytkownika na podstawie podanej ilości wątków przez użytkownika

                                                                                                                       // podział tablice na wątki na podstawie ilości elementów do przetworzenia oraz ilości wątków wybranych przez użytkownika (rozmiar tablicy bajtów obrazka / ilość wybranych wątków).
            int interval = RGBValuesOfImage.GetLength(0) / threads;
                                                                                                                       // pętla tworząca przedziały dla wątku
            for(int i = 0; i < arrayOfThreads.GetLength(0); ++i)                                        
            {
                int start = i * interval;                                                                              // zmienna start do tworzenia początku przedziały na podstawie aktualnie wykonywanej iteracji pętli oraz ilości pikseli przypadających na jeden wątek                                                                        

                                                                                                                       // uzupełenienie do pełnego piksela dla początku przedziału

                while(start %32 != 0)                                                                                  // modulo 32 dla startu przedziału
                {
                    start += 1;                                                                                        // inkrementacja zmiennej start o 1
                }
                int stop = (i + 1) * interval;                                                                         // zmienna stop do tworzenia początku przedziały na podstawie aktualnie wykonywanej iteracji pętli  oraz ilości pikseli przypadających na jeden wątek                                                                        

                                                                                                                       // uzupełenienie do pełnego piksela dla końcu przedziału
                while (stop %32 != 0)                                                                                  // modulo 32 dla stopu przedziału
                { 
                    stop += 1;                                                                                         // ikrementacja zmiennej stop o 1
                }
                if (stop > RGBValuesOfImage.Length || ( (i == threads - 1) && stop != RGBValuesOfImage.Length))         // warunek sprawdzający czy nieprzekroczonoy maksymalnej wartości tablicy, aby dany wątek nie wyszedł poza przedział tablicy
                    stop = RGBValuesOfImage.Length;                                                                     // ustawienie maksymalnej wartości tablicy dla danego przedzuału wątku

                                                                                                                       // tablica obiektów przekazywana dla każdego wątku z odpowiednią wartością sepii
                arrayOfArguments[i] = new object[5] { start, stop, RGBValuesOfImage, toneValue, depthValue };
                                                                                                                       // nowy wątek dla C# z podanymi parametrami przedzału, wypełnienia oraz tablic obrazka do przetworzenie
                if (csharp.Checked) arrayOfThreads[i] = new Thread(new ParameterizedThreadStart(SepiaDll.Sepia.CSharpDllFunc));
                                                                                                                       // nowy wątek dla asmeblera z podanymi parametrami przedział, wypełnienia oraz tablicy orabzka do przetworzenia
                else if (asm.Checked) arrayOfThreads[i] = new Thread(new ParameterizedThreadStart(AssemblerFunction)); 

            }  
        }
       
        /// <summary>
        /// Metoda tworząca i rozpoczynająca pracę nowych wątków
        /// </summary>
        /// <returns> void </returns> 
        private void CreateThreads()
        {
                                                                                                                      //  warunek sprawdzający czy brak tablicy wątków lub tablica jest pusta
                                                                                                                      // zabazepieczenie przed podaniem pustej tablicy wątków
            if (arrayOfThreads == null)
                return;                                                                                               // zwrocenie funkcji
                                                                                                                      // początek zliczania czasu
            model.StartWatch();

            for (int i = 0; i< arrayOfThreads.GetLength(0); ++i)                                                      // pętla rozpoczynająca pracę wątków dla algorytmu
            {
                                                                                                                      // start tablicy wątków za pomocą metody Start z parametryzowanym startem argumentów do przetworzenia
                arrayOfThreads[i].Start(arrayOfArguments[i]);
              
            }
        }
        /// <summary>
        /// Metoda czekająca i sprawdzająca czy wątek się zakończył
        /// </summary>
        /// <returns> void </returns> 
        private void WaitForThreads()
        {
                                                                                                                       // pusta tablica wątków 
            if (arrayOfThreads == null) return;

            bool done = false;                                                                                         // lokalna zmienna bool done do sprawdzania czy praca wątków się zakończyła

                                                                                                                       // sprawdzenie czy wątki zakończył pracę
            while (!done)
            {
                done = true;                                                                                          // zmienna bool done ustawiona na true, aby dalej sprawdzić czy poniższy warunek działania wątków się spełnił

                for (int i = 0; i < arrayOfThreads.GetLength(0); ++i)                                                 // pętla sprawdzająca wszystkie wątki w  tablicy do sprawdzania czy aktualnie wykonują pracę

                                                                                                                      // wartość bool sprawdzająca czy tablica wątków jest pusta lub czy dany wątek "nie żyje" . Jeżeli praca zakończona done = true
                    done &= (arrayOfThreads[i] == null || !arrayOfThreads[i].IsAlive);
            }  

        }

        /// <summary>
        ///  Metoda przechowująca 3 metody: 1 metoda tworzenia delegatów, 2 metoda tworzenia wątkow oraz 3 metoda czekająca na wątki
        /// Proces przetwarzania danych do dllki, kontrola tworzenia i zakończenia trwania wątków
        /// </summary>
        /// <param name="csharpRadioButton"> radio button dla dllki C# w GUI</param>
        /// <param name="asmRadioButton"> radio button dla dllki asm w GUI</param>
        /// <param name="threads">wartość zmiennej int ilości wybranych wątków z comboboxa</param>
        /// <param name="toneValue"> wartość zmiennej współczynnika wypełnienia sepii</param>
        /// <returns> void </returns> 
        public void InvokeDll( RadioButton csharpRadioButton,RadioButton asmRadioButton, int threads, int toneValue, int depthValue)
        {
                                                                                                                        // metoda wykonująca przygytowoania na podział na wątki dla wykonania dllki
            PrepareDelegates(csharpRadioButton, asmRadioButton, threads, toneValue, depthValue);
                                                                                                                        // metoda tworzenia wątków
            CreateThreads();
                                                                                                                       // metoda czekająca na wykonanie wątków
            WaitForThreads();

        }
       
        /// <summary>
        ///  // Metoda specjalna dla asemblera, aby rozłożyć danych obiekt na typy proste dla asemblera
        /// Wykorzystano klauzule unsafe, aby móc wykorzystać wątki
        /// </summary>
        /// <param name="argum"> obiekt 4 elementów zawierających informacje o początku oraz końcu przedziału i zarówno tablicy bajtów obrazu i wartości wypełnienia sepii</param>
        /// <returns> void </returns> 
        unsafe private void AssemblerFunction(object argum)
        {
                                                                                                                      // tworzenie oiektu typu Array, aby przechować niebędne elementy do wykonania dla asm dllki
             Array args = new object[5];
                                                                                                                      // rzutowanie parametru metody na obiekt typu Array utworzony wcześniej
             args = (Array)argum;
                                                                                                                      // zmienna int przechowująca wartość początku przedziału tablicy bajtów
             int start = (int)args.GetValue(0);
                                                                                                                      // zmienna int przechowująca wartość końca przedziału tablicy bajtów
             int stop  = (int)args.GetValue(1);
                                                                                                                      // wskaźnik na tablice bajtów obrazka
             byte[] table = (byte[])args.GetValue(2);
                                                                                                                      // zmienna int przechowująca wartość współczynnika wypełnienia sepii
             int toneValue = (int)args.GetValue(3);
                                                                                                                      
             int depthValue = (int)args.GetValue(4);                                                                  // zmienna int przechowująca wartość głębokości odcienii sepii

            fixed (byte* imagePtr = &table[0])                                                                         // stały rozmiar tablicy, aby można było ją przekazać do rejestru i wykonania dla asm dllki
            {
                                                                                                                        // Start wykonywania dllki dla asemblera
                Sepia(imagePtr, start, stop, toneValue, depthValue);
            }
        }
    }

}
