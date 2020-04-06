using System;
using System.Windows.Forms;

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
// przestrzeń nazw SepiaGUI.Model
// parametry wejściowe znajdują się w adnotacjach <param name>, brak tej adnostacjii oznacza brak parametrów wejściowych
// parametry wyjściowe w adnotacji <returns> lub returns

// Główna klasa programu
// Generowana autmoatycznie w przypadku towrzenia aplikacji okienka w WidowsForms
namespace SepiaApp
{
                                                     // statyczna klasa Programu tworzona przez WindowsForms
    static class Program
    {
       

       
        [STAThread]
       
        //Główna metoda program w której tworzony jest program
        ///<returns> void </returns>
        static void Main()
        {
            
            Application.EnableVisualStyles();                       // aktywacja styli visuala
            Application.SetCompatibleTextRenderingDefault(false);   //ustawienie kompatybilności renderowania
            Application.Run(new SepiaProgram());                    // uruchomienie aplikacji       

          
           

        }
    }
}
