namespace LKBConvertor.Services
{
    public static class ShareHelper
    {
        public static async Task PartagerFichierAsync(string cheminFichier, string? titre = null)
        {
            if (string.IsNullOrEmpty(cheminFichier) || !File.Exists(cheminFichier))
                return;

            titre ??= Path.GetFileName(cheminFichier);

#if ANDROID
            if (PartagerAndroid(cheminFichier, titre))
                return;
#endif
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = titre,
                File = new ShareFile(cheminFichier, DevinerMime(cheminFichier))
            });
        }

        private static string DevinerMime(string chemin) =>
            Path.GetExtension(chemin).ToLowerInvariant() switch
            {
                ".pdf"  => "application/pdf",
                ".rtf"  => "application/rtf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc"  => "application/msword",
                _       => "application/octet-stream"
            };

#if ANDROID
        private static bool PartagerAndroid(string cheminFichier, string titre)
        {
            try
            {
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (activity == null) return false;

                var file = new Java.IO.File(cheminFichier);
                var authority = activity.PackageName + ".share.fileprovider";
                var uri = AndroidX.Core.Content.FileProvider.GetUriForFile(activity, authority, file);

                var mime = DevinerMime(cheminFichier);
                var intent = new Android.Content.Intent(Android.Content.Intent.ActionSend);
                intent.SetType(mime);
                intent.PutExtra(Android.Content.Intent.ExtraStream, uri);
                intent.PutExtra(Android.Content.Intent.ExtraSubject, titre);

                // ClipData propage la permission URI aux apps cibles (Gmail, etc.)
                // même après la fin de l'activité chooser — indispensable pour l'envoi différé.
                intent.ClipData = Android.Content.ClipData.NewRawUri(string.Empty, uri);
                intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission);

                var chooser = Android.Content.Intent.CreateChooser(intent, titre);
                activity.StartActivity(chooser);
                return true;
            }
            catch
            {
                return false;
            }
        }
#endif
    }
}
