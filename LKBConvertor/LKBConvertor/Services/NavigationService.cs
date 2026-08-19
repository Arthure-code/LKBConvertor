namespace LKBConvertor.Services
{
    public interface INavigationService
    {
        Task PushAsync(Page page);
        Task PopAsync();
        Task<bool> AfficherAlerte(string titre, string message, string accepter, string annuler);
        Task<string?> AfficherOptions(string titre, string annuler, string? destructif, params string[] boutons);
    }

    public class NavigationService : INavigationService
    {
        private static Page PageActive
        {
            get
            {
                var windows = Application.Current?.Windows;
                if (windows == null || windows.Count == 0 || windows[0].Page == null)
                    throw new InvalidOperationException("Aucune page active.");
                return windows[0].Page!;
            }
        }

        public Task PushAsync(Page page) => PageActive.Navigation.PushAsync(page);

        public Task PopAsync() => PageActive.Navigation.PopAsync();

        public Task<bool> AfficherAlerte(
            string titre, string message, string accepter, string annuler) =>
            PageActive.DisplayAlert(titre, message, accepter, annuler);

        public Task<string?> AfficherOptions(
            string titre, string annuler, string? destructif, params string[] boutons) =>
            PageActive.DisplayActionSheet(titre, annuler, destructif, boutons)!;
    }
}
