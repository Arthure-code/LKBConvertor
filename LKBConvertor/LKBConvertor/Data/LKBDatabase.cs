using SQLite;
using LKBConvertor.Models;

namespace LKBConvertor.Data
{
    public class LKBDatabase
    {
        private const int MAX_HISTORIQUE = 100;

        public List<ConversionHistory> ObtenirHistorique()
        {
            using (var conn = new SQLiteConnection(App.CheminBD))
            {
                return conn.Table<ConversionHistory>()
                           .OrderByDescending(h => h.Id)
                           .ToList();
            }
        }

        public List<ConversionHistory> ObtenirRecentes()
        {
            using (var conn = new SQLiteConnection(App.CheminBD))
            {
                return conn.Table<ConversionHistory>()
                           .OrderByDescending(h => h.Id)
                           .Take(3)
                           .ToList();
            }
        }

        public void Inserer(ConversionHistory item)
        {
            using (var conn = new SQLiteConnection(App.CheminBD))
            {
                conn.Insert(item);
            }
            AppliquerMaxHistorique();
        }

        public void Supprimer(ConversionHistory item)
        {
            using (var conn = new SQLiteConnection(App.CheminBD))
            {
                conn.Delete(item);
            }
        }

        public void EffacerTout()
        {
            using (var conn = new SQLiteConnection(App.CheminBD))
            {
                conn.DeleteAll<ConversionHistory>();
            }
        }

        private void AppliquerMaxHistorique()
        {
            var liste = ObtenirHistorique();
            if (liste.Count > MAX_HISTORIQUE)
            {
                var aSupprimer = liste.Skip(MAX_HISTORIQUE).ToList();
                foreach (var item in aSupprimer)
                    Supprimer(item);
            }
        }
    }
}