using LKBConvertor.Models;
using SQLite;

namespace LKBConvertor
{
    public partial class App : Application
    {
        public static string CheminBD;

        public App()
        {
            InitializeComponent();

            var nomBD = "lkbconvertor_db.sqlite";
            var repertoire = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal);
            var cheminAcces = Path.Combine(repertoire, nomBD);

            CheminBD = cheminAcces;

            using (var conn = new SQLiteConnection(cheminAcces))
            {
                conn.CreateTable<ConversionHistory>();
            }

            MainPage = new NavigationPage(new Views.HomePage());
        }
    }
}