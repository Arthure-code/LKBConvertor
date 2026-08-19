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
        private static Page PageActive =>
            Application.Current?.Windows?.FirstOrDefault()?.Page
            ?? throw new InvalidOperationException("Aucune page active.");

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
