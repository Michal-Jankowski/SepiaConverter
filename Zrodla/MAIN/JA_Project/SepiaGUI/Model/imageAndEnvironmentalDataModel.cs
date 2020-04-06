

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
// v. 0.1 20/10/19r.
// Dodanie metod kontroli zliczania czasu dla odciążenia głównej klasy z pomniejszych metod
// w celu poprawy czytelności kodu.
//
// v. 0.2 26/10/19r.
// Przenesienie metod przechowujących informacje o ilości fizycznych i logicznych procesorów w komputerze 
// z głównej klasy programu. Dodano również możliwość ich ustawiania do odpowiednich Labeli widoku aplikacji.
//
// v. 0.3  2/11/19r.
// Utworzenie metod wykorzystywanych do poprawnego ustawienia szczegółów dotyczących ładowanego obrazu 
// w celu poprawnego przetworzenia do algorytmu Sepii. Dodanie filtra wyboru formatu obrazka oraz jego konwersje na bitmape
//
// v. 0.4 4/11/19r.
// Dodanie metody sprzwdzającej poprawność wykorzystywanego frameworka
//
// v. 0.5 15/11/19r. 
// Wstępne dodwanie komentarzy do kodu oraz stworzenie metody  tworzącej log dotyczący  procesu konwersji obrazka wykorzstywanej w głównym programie
//
// v. 0.6 21/11/19r.
// Poprawa czytelności kodu poprzez dodanie odpowiednich odstępów oraz odpowienie skomentowanie linii kodu w podanej klasie dotyczących parametrów metody.
//
// v. 1.0 20/12/19r.
// Usunięcie zbędnych metod oraz niewykorzysystywanych zmiennych.
// Ostateczna kontrola poprawności wykonywanych metod w podanej klasie.
//

// wykorzystywane  przestrzenie nazw
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;


// przestrzeń nazw SepiaGUI.Model
// parametry wejściowe znajdują się w adnotacjach <param name>, brak tej adnostacjii oznacza brak parametrów wejściowych
// parametry wyjściowe w adnotacji <returns> lub returns
namespace SepiaGUI.Model
{
    //Klasa odpowiedzialna za przetrzymywanie  pomniejszych metod dla widoku
    // Pozwala na większą przejrzystość głównej klasy modelu
    class ImageAndEnvironmentalDataModel
    {

        private int logIter;                                                                                          // zmienna int do zliczania wartości wykonanej konwersji obrazu w programie. Wykorzystywana jest w logu.

                                                                                                                      // Obiekt Stopwatch wykorzystywany do zliczania czasu
        readonly Stopwatch watch = new Stopwatch();
        /// <summary>
        /// Metoda ustawiająca ilość logicznych procesorów w labelu logicalProcessorsLabel
        /// </summary>
        /// <param name="logicalProcessorsLabel"> obiekt Label z widoku do ustawienia liczby logicznych procesorów</param>
        /// <param name="logicalProcessors"> string przechowujay informacje o libczie logicznych procesorow</param>
        /// <returns> void </returns> 
        public void SetLogicalProcessors(Label logicalProcessorsLabel, String logicalProcessors)
        {
                                                                                                                    // Ustawienie w labelu informacji o liczbie logicznych proceosrów
            logicalProcessorsLabel.Text = logicalProcessors;

        }
        /// <summary>
        /// Metoda zliczająca optymalną ilość logicznych procesorów wykorzystująca rejestry
        /// </summary>
        /// <param name="optimal"> wartość bool do ustawiania czy potrzbujemy optymalna czy nieoptymalna liczbie logicznych procesorów</param>
        /// <returns>String informujący o optymalnej liczbie logicznych procesorów</returns>
        
        public String CountLogicalProcessors(bool optimal)
        {
                                                                                                                      // String pobierający ze zmiennej środowiskowej informacje o liczbie logicznych procesorów
            String numberOfLogicalProcessors = Environment.ProcessorCount.ToString();
                                                                                                                      // niepotymalna liczba proceosorów (warunek)
            if (optimal == false)
                return numberOfLogicalProcessors;

                                                                                                                      // ustawienie optymalnej liczby procesorów
            int logicalProcessors = Convert.ToInt32(numberOfLogicalProcessors) - 1;
                                                                                                                      //zwrócenie danej liczby proceosrów w zależności od ustawienia boola
            return (logicalProcessors.ToString());

        }
        /// <summary>
        /// Metoda zliczająca liczbe rdzeni procesora i ustawiająca je w labelu CoresLabel
        /// </summary>
        /// <param name="CoresLabel"> Label odpowiadający za liczbę rdzeni w procesorze i ustawiający tę wartość w Labelu widoku</param>
        /// <returns> void </returns>
        public void CountCores(Label CoresLabel)
        {
            using (var disposer = new ManagementObjectDisposer() )                                                       // zmienna ManagmentObjectDisposer do tworzenia disposera dla klasy Manage
            {
                int coreCount = 0;                                                                                       // zmienna na ilość rdzeni procesora
                foreach (var item in disposer.Search("Select * from Win32_Processor"))                                   // wykorzystanie zapytania w celu uzyskania z rejestru liczby rdzeni
                {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                    item.Dispose();                                                                                      // liczba rdzenie w postaci int
                }                                                                                                        // dealokacja obiektu typu item

                CoresLabel.Text = coreCount.ToString();
                                                                                                                         // ustawienie wartości rdzeni w labelu 
            }
           
        }

        
        /// <summary>
        /// /Metoda zliczjąca liczbę fizycznych rdzeni i  ich uwstawienie w setPhysicalProcessors
        /// </summary>
        /// <param name="physicalProcessorsLabel"> Label ustawiający liczbę fizycznych rdzenie w widoku</param>
        /// <returns> void </returns> 
        public void CountPhysicalProcessors(Label physicalProcessorsLabel)
        {
            using (var moDisposer = new ManagementObjectDisposer())                                                               // utworzzenie zminnej moDisposer dla obiektów zarządzanych ManagementObjectDisposer, aby można było jej usunąc
            {
                int physicalCores = 0;                                                                                              // zmienna na liczbę fizycznych rdzeni
                foreach (var item in moDisposer.Search("Select * from Win32_ComputerSystem"))                                       // wykorzystanie zapytania w celu uzyskania z rejestru liczby fizycznych rdzeni
                {

                    physicalCores += int.Parse(item["NumberOfProcessors"].ToString());                                            // liczba fizycznych rdzeni w postaci int
                    item.Dispose();                                                                                               // dealokacja zmiennej item, gdy jest zbedna
                }

                SetPhysicaProcessors(physicalProcessorsLabel, physicalCores);                                                    // ustawienie fizycznych procesorów w metodzie SetPhysicalProcessors
            }
        }
        /// <summary>
        ///  Mtoda ustawiająca liczbę fizycznych rdzeni w widoku
        /// </summary>
        /// <param name="textLabel"> Label do ustawienia fizycznych rdzeni w widoku</param>
        /// <param name="physicalCores"> int wartość fizycznych rdzeni</param>
        /// <returns> void </returns>
        public void SetPhysicaProcessors(Label textLabel, int physicalCores)
        {
            textLabel.Text = physicalCores.ToString();                                                                       // ustawienie w labelu wartości fizycznych rdzeni
        }
        //
        /// <summary>
        /// Metoda czyszcząca pole załadowanego obrazka w GUI, aby zapobiec jego nawarstwianiu się.
        /// </summary>
        /// <param name="picture"> obiekt PictureBox zawierającego obrazek w GUI </param>
        /// <returns> void </returns> 
        public void ClearImage(PictureBox picture)
        {
            if (picture.Image != null)                                                                                        // warunek sprawdzajacy czy obrazek nie jest nullem
            {
                picture.Image.Dispose();                                                                                      // usuwanie zawartości obrazka z GUI
                picture.Image = null;                                                                                         // ustawienie pola  obrazka na null                                                              
            }
        }
        /// <summary>
        /// Metoda zwracająca aktualnie wybraną wartość combobox w postaci inta
        /// </summary>
        /// <param name="combobox"> dany combobox, który posiada wartość int</param>
        /// <returns> wartość combobox w int</returns>
        public int ConvertComboboxItemSelected(ComboBox combobox)
        {
            if (combobox.SelectedItem != null)                                                                                  // szukana wartość null
            {
                int value = Int32.Parse(combobox.SelectedItem.ToString());                                                      // wartość int combbox szukanej wartości
                return value;                                                                                                   // zwrócenie wartości int z comboboxa
            }
            else
            {
                return 0;                                                                                                        // jeśli nie znajdzie to ustawia 0 poprzez return 0
            }
        }
        /// <summary>
        /// Metoda ustawiająca aktualnie obsługiwany format obrazka jako  bmp
        /// </summary>
        /// <param name="openfile"> OpenFileDialog obiekt otwieranego okienka GUI w widoku</param>
        /// <returns> void </returns> 
        public void SetImageFilter(OpenFileDialog openfile)
        {
            openfile.Filter = "Image Files(*.bmp;) | *.bmp;";                                                                       // ustala obłsugę .bmp plików

        }
        
        /// <summary>
        ///  Metoda wyłączająca obsługę przyycisku konwersji obrazka na daną ilość sekund po jego kliknięciu
        /// </summary>
        /// <param name="seconds"> wartoś w sekundach opóxnienia</param>
        /// <param name="button"> danyc obiekt button który jest opóźniany</param>
        /// <returns> void </returns> 
        public async void DisableButton(int seconds, Button button)
        {
            button.Enabled = false;                                                                                              // wyłączenie działania buttona
            await Task.Delay(1000 * seconds);                                                                                    // metoda opóżniająca asynchronicznie button na zadany czas w sekundach
            button.Enabled = true;                                                                                               // włączenie obsługi buttona

        }
        
        /// <summary>
        ///  Metoda sprawdzająca wersję frameworka. Zwraca prawdę w przypadku posiadania wersji frameworka większej niz 4.6.
        ///  Zwraca fałsz w przypadku braku frameworka lub jego odpowiedniej wersji dla poprawnego działania programu.
        /// </summary>
        /// <returns> wartość bool czy istnieje framework i odpowienia wersja frameworka</returns>
        public static bool CheckFrameworkVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";                                                       // tworzenie stringa subkey w celu dotacia do danej wartości rejetstru, która przechowuję inforamcje o wersji .net frameorka

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))         // pobranie wartościu wersji .net frameworka na podstawie rejestru i danego subkeya
            {            
                if (ndpKey != null && ndpKey.GetValue("Release") != null)                                                                       // sprawdzenie czy istnieje taki subkey i czy jest zainstalowany taki framework
                {
                    if (CheckFor45PlusVersion((int)ndpKey.GetValue("Release")))
                    {                                                                                                                           // sprawdzenie czy jest to wersja 4.5> i pobranie wartości wersji frameworka
                      
                        return true;
                    }                                                                                                                           // zwrócenie prawdy w przypadku posiadanej wersji większej niż 4.5
            
                }
                else                                                                                                                            //  nie posiada odpowiedniego rejestru na wersji frameworka
                {

                    return false;                                                                                                               // zwrócenie fałszu w przypadku nie posiadania odpowiedniej wersji frameworka
                }
                
                return false;                                                                                                                   // zwrócenie fałszu w przypadku nie posiadania frameworka
            }
            /// <summary>
            ///  Metoda zwracająca infrormacje czy o obsługiwana wersja .net frameowrka jest odpowiednia dla programu
            /// </summary>
            /// <param name="releaseKey"> int wartość wyszukanej wersji frameworka</param>
            /// <returns> bool wartość czy obsługuję daną wersje frameworka </returns>
            bool CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)                                                                // warunek czy wersja frameworka wyższa  lub równa  4.6, ponieważ taką minimalnie musi użytkownik obsługiwać
                                                                                                         // w celu uruchomienia aplikacji

                    return true;                                                                         // zwrócenie prawdy w przypadku posiadanej wersji


                return false;                                                                           // zwrócenie fałszu w przypadku braku posiadanej wersji 
            }


        }
       
        /// <summary>
        /// Metoda rozpoczynająca zlicanie czasu dla Stopwatch zegara
        /// </summary>
        /// <returns> void </returns> 
        public void StartWatch()
        {
            watch.Start();                                                              // rozpoczęcie pracy zegara
        }
       
        /// <summary>
        /// Metoda zatrzymująca zlicanie czasu dla Stopwatch zegara
        /// </summary>
        /// <returns> void </returns> 
        public void StopWatch()
        {
            watch.Stop();                                                                 // zatrzymanie zegara
        }
        
        /// <summary>
        /// Metoda pobierająca czas zegara oraz restartująca zegar
        /// </summary>
        /// <returns> String wartości zliczonego czasu</returns>
        public string GetWatchInµS()
        {
            double ticks = watch.ElapsedTicks;                                              // obliczenie tyknięć z zegara StopWatch
            double microseconds = ( ticks / Stopwatch.Frequency) * 1000;                    // zliczenie milisekund zegara poprzez podzielenie wartości ticks przez częstotliwość StopWatch oraz domnożenie wartości do żądanej jednostki

            watch.Reset();                                                                  // resetowanie wartości zegara i ustawianie na 0

            return microseconds.ToString();                                                 // zwrócenie wartości zegara
        }
        /// <summary>
        /// Metoda konwertująca dany format obrazka na bitmape
        /// </summary>
        /// <param name="fileName"> string lokalizacji obrazka pobrany z widoku</param>
        /// <returns> Bitmap obiekt skonwertowanego obrazka</returns>
        public Bitmap ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;                                                                // tworzenie obiektu Bitmap, aby przetowrzyć obrzek na format .bmp
            using(Stream bmpStream = File.Open(fileName, FileMode.Open))                  // wykorzystanie obiektu Stream do otwarcia obrazka za pomocą metody File.open
            {
                bmpStream.Position = 0;
                Image image = Image.FromStream(bmpStream);                                // tworzenie skonwertowanego obrazka za pomocą obiektu Image

                bitmap = new Bitmap(image);                                              // tworzenie nowego obiektu bitmapy jako skonewertowanego obrazka
            }

            return bitmap;                                                               // zwrocenie skonwertowanego obrazka
        }
        /// <summary>
        /// Metod wypisująca informacje o parametrach programu wykorzysytwanych do utworzenia efektu Sepii podanego obrazka
        /// Wyświetlane są tutaj inforamcje m.in. o czasie konwersji obrazka w Ms oraz o wybranej dllce dla algorytmu
        /// </summary>
        /// <param name="log"> RichTextBox odopwiedzialny za wypisywanie loga na temat wykonywanego algorytmu</param>
        /// <param name="threads"> ilość wątków wykonywancyh dla zadanych parametrów w programie</param>
        /// <param name="asm"> RadioButton asemblerowej dllki</param>
        /// <param name="csharp"> RadioButton C# dllki</param>
        /// <param name="sepiaTone"> Wartość współczynnika wypełnienia sepii</param>
        /// <param name="image"> obiekt typu Bitmap przechowujący informacje o aktualnie załadowanym obrazku w programie</param>
        /// <returns> void </returns> 
        public void ParseToRichTextBox(RichTextBox log, int threads, RadioButton asm, RadioButton csharp, int sepiaTone,int depthValue, Bitmap image)
        {
            logIter++;                                                                   // iteracja kolejnego wywołania algorytmu dla loga aplikacji

            if (logIter % 5 == 0){                                                        // sprawdzenie czy wartośc iteracji dotycząca ilości wykonanego algorytmu w programie jest podzielna przez 5
                log.Clear();                                                              // Jeżeli reszta  z dzielenia wynosi 0 to czyścimy informacje o poprzednich wyowołaniach algorytmu
                log.AppendText("Log cleared! \n");                                           // Informacja o wyczyszczeniu logu
                }
            log.AppendText("Calculation done for " + logIter + " iteration ! \n " +      // dodanie informacji do loga o następujących rzeczach: * Ilości wykonanych obliczeń w trakcie wykonania programu ( ile razy użytkownik wykonał konwersje)
                "****************************************" + "\n" +                      // * przerwa w postaci " * ", aby oddzielić ważne informacje o danej iteraci algorytmu
                " Number of threads:" + threads + "\n" +                                 // * ilość wykorzystywanych wątków,
                " Asm checked:" + asm.Checked + "\n" +                                   // * czy wybrano dllke asemblerową,
                " C# checked:" + csharp.Checked + "\n" +                                 // * czy wybrano dllke c#,
                " Sepia tone value: " + sepiaTone + "\n" +                               // * wartość wypełneinia efektą sepii,
                " Sepia depth value: " + depthValue + "\n" +                             // * wartość głębii efektu sepii,
                " Image Resolution: " + image.Width + " x " + image.Height + "\n" +      // * rozmiar przetwarzanego obrazka  w pikselach  szerokość x wysokość,
                " Conversion time: " + GetWatchInµS() + " ms" + "\n" +                   // Czas konwersji w ms 
                " Log information ended!" + "\n");                                       // Informacja zwrotna o zakończeniu loga informacji dla podanej iteracji programu

        }

    }
    /// <summary>
    ///  Klasa odpowiedzialna za zarządzanie usuwaniem obiektów IDisposable wykorzysytwanej przy zarządanych obiektach (Managed Object) 
    ///  Używana podczas dealokacji obiektów metod pobierających dane z rejestru o ilości procesorów. 
    ///  Dziedziczy po IDisposable
    /// </summary>
    public class ManagementObjectDisposer : IDisposable
    {
                                                                                         // lista obiektów IDisposable tylko do odczytu dla tworzenia listy usuwanych obiektów
        private readonly List<IDisposable> disposableObjects = new List<IDisposable>(); 

        /// <summary>
        /// Metoda do usunięcia pojedynczego obiektu typu ManagementBaseObject
        /// </summary>
        /// <param name="disposableObject"> obiekt typu IDisposable do usunięcia</param>
        /// <returns> void </returns>
        public static void DisposeOne(IDisposable disposableObject)
        {
            if (disposableObject is ManagementBaseObject managmentBaseObject)           // sprawdzenie czy jest to obiekt typu ManagemntBaseObject
                managmentBaseObject.Dispose();                                          // usunięcie danego obiektu
            else
                disposableObject.Dispose();                                             // usunięcie obiektu zwykłego poprzez standardową metodę dispose
        }
        
        /// <summary>
        /// Metoda implementowana przez interfejst IDisposable 
        /// Tworzy metodę usuwającą obiekty typu IDisposable
        /// </summary>
        /// <returns> void</returns>
        public void Dispose()
        {
            Exception firstException = null;                                                 // tworzenie obiektu Exception
            foreach (IDisposable d in Enumerable.Reverse(disposableObjects))                 // pętla foreach dla obiektów typu IDisposable Enumerable, aby można utworzyć obeikty typu wyliczeniowego do usuwania obiektów Enumerable
            {
                try                                                                          // try catch do pojedynczego obiektu typi IDisposable
                {
                    DisposeOne(d);
                }
                catch (Exception exception)                                                  // łapanie wyjątkótypu Exception
                {
                    if (firstException == null)                                              // jeżeli nie wyrzucono wyjątku  za pierwszym  razem w firstException
                        firstException = exception;                                          // wyjatek typu firstException jest przypisany do exception         
                   
                       
                }
            }
            disposableObjects.Clear();                                                      // użyj metody Clear() na obiekcie disposableObjects
            if (firstException != null)                                                     // jeżeli dla obiektu Exception znaleźono wyjątek to wyrzuć go, aby potem w catchu można go złapać
                throw firstException;
        }
        /// <summary>
        /// Metoda geenryczna  do dodawania obiektó ManagmentBaseObject oraz ManagementObjectSearcher do listy obiektów do usinięcia
        /// </summary>
        /// <typeparam name="T">  obiekt generyczny może być typu  ManagmentBaseObject</typeparam>
        /// <param name="disposable"> obiekt typu generyk reprezentujący obiekt IDisposable</param>
        /// <returns> obiekt typu generycznego T, głównie obiekty typu ManagmentBaseObject</returns>
        public T Add<T>(T disposable) where T : IDisposable
        {
            disposableObjects.Add(disposable);                                      // dodaj do listy obiektów IDisposable obiekt disposable
            return disposable;                                                      // zwróć obiekt disposable
        }

        /// <summary>
        /// Metoda pomocnicza dla ManagmentObjectSearcher, aby dodać wszystkie podane obiekty do usnięcia w liście 
        /// </summary>
        /// <param name="taskQuery">Zapytanie w postaci string w celu utworzenia obiektu ManagmentObjectSearcher</param>
        /// <returns> liste ManagmentBaseObject typu IEnumerable</returns>
        public IEnumerable<ManagementBaseObject> Search(string taskQuery)
        {
            ManagementObjectSearcher seracherObject = this.Add(new ManagementObjectSearcher(taskQuery));            // utwórz obiekt ManagmentObjectSearcher na podstawie taskQuery
            return EnumerateCollection(seracherObject.Get());                                                       // zwróć element ManagmentObjectSearcher z EnumerableCollection, aby poprawnie usunąć dane
        }

        /// <summary>
        /// Metoda pomocnicza do dodawania elementów typu ManagemntBaseObejct oraz enumerowania po nich
        /// </summary>
        /// <param name="collectionList"> lista typu ManagmentObjectCollection</param>
        /// <returns>lista ManagmentBaseObject typu IEnumerable</returns>
        public IEnumerable<ManagementBaseObject> EnumerateCollection(ManagementObjectCollection collectionList)
        {
            this.Add(collectionList);                                                                                   // dodaj obiekt ManagmentObjectCollection do listy collecionList obiektów do usunięcia
            ManagementObjectCollection.ManagementObjectEnumerator enumeratingvalue = this.Add(collectionList.GetEnumerator());      // utwórz ManagmentObjectEnumerator enumeratingvalue, aby dodać go do obieku collectionList
            while (enumeratingvalue.MoveNext())                                                                                      // jeżeli istnieje wartość enumeratingValue to przesuń na kolejny obiekt z listy obiektów collectionlist
                yield return this.Add(enumeratingvalue.Current);                                                                     // zwróć aktualnie przeglądany obiekt typu ManagmentBaseObject
        }
    }
}
