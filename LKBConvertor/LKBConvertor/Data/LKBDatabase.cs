using SQLite;
using LKBConvertor.Models;

namespace LKBConvertor.Data
{
    public class LKBDatabase
    {
        private const int MAX_HISTORIQUE = 100;
        private readonly SQLiteConnection _conn;

        public LKBDatabase()
        {
            var chemin = Path.Combine(
                FileSystem.AppDataDirectory, "lkbconvertor_db.sqlite");
            _conn = new SQLiteConnection(chemin);
            _conn.CreateTable<ConversionHistory>();
        }

        public List<ConversionHistory> ObtenirHistorique() =>
            _conn.Table<ConversionHistory>()
                 .OrderByDescending(h => h.Id)
                 .ToList();

        public List<ConversionHistory> ObtenirRecentes() =>
            _conn.Table<ConversionHistory>()
                 .OrderByDescending(h => h.Id)
                 .Take(3)
                 .ToList();

        public void Inserer(ConversionHistory item)
        {
            _conn.Insert(item);
            AppliquerMaxHistorique();
        }

        public void Supprimer(ConversionHistory item) => _conn.Delete(item);

        public void EffacerTout() => _conn.DeleteAll<ConversionHistory>();

        private void AppliquerMaxHistorique()
        {
            _conn.Execute(
                @"DELETE FROM ConversionHistory
                  WHERE Id NOT IN (
                      SELECT Id FROM ConversionHistory
                      ORDER BY Id DESC LIMIT ?
                  )",
                MAX_HISTORIQUE);
        }
    }
}
