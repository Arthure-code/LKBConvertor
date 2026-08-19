# Contribuer à LKBConvertor

Merci de votre intérêt pour ce projet. Voici comment participer efficacement.

## Prérequis

- .NET 9 SDK
- JDK 17 (Temurin recommandé)
- Android SDK API 35 (installable via `dotnet build -t:InstallAndroidDependencies -f net9.0-android`)
- Visual Studio 2022 17.10+ ou VS Code + extension .NET MAUI
- Émulateur Android API 32+ ou appareil physique

## Setup local

```bash
git clone https://github.com/Arthure-code/LKBConvertor.git
cd LKBConvertor
dotnet workload install maui-android
dotnet restore LKBConvertor/LKBConvertor.csproj
dotnet build LKBConvertor/LKBConvertor.csproj -f net9.0-android
```

## Workflow git

1. Fork le dépôt
2. Branche depuis `develop` avec le préfixe adéquat :
   - `feat/<slug>` — nouvelle fonctionnalité
   - `fix/<slug>` — correction de bug
   - `chore/<slug>` — maintenance, config, dépendances
   - `docs/<slug>` — documentation
   - `refactor/<slug>` — refactorisation sans changement fonctionnel
3. Commits descriptifs, en français ou anglais, à l'impératif
4. Pull Request vers `develop`, jamais directement vers `main`

## Convention de code

- Nommage des identifiants métier en français (ex. `ChoisirFichierAsync`)
- Nommage des identifiants techniques en anglais (ex. `IServiceProvider`)
- MVVM strict — pas de logique métier dans les code-behind
- Injection de dépendances via `Microsoft.Extensions.DependencyInjection`
- Async/await partout où I/O est impliqué, jamais d'`async void` sauf handlers d'événement

## Tests

Le projet n'a pas encore de suite de tests automatisés. Toute PR ajoutant une fonctionnalité importante doit être validée manuellement sur émulateur Android API 32+.

## Signalements

- **Bug** : ouvrez une issue avec le template "Bug report", incluez la version Android, une capture d'écran et les logs LogCat pertinents
- **Fonctionnalité** : ouvrez une discussion avant de créer une PR

## Contact

Maintainer : Arthure Lekoubou Djune ([github.com/Arthure-code](https://github.com/Arthure-code))
