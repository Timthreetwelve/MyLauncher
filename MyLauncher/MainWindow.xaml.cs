﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public partial class MainWindow : Window
{
    #region NLog Instance
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    #endregion NLog Instance

    #region Stopwatch
    private readonly Stopwatch stopwatch = new();
    #endregion Stopwatch

    #region MainWindow Instance
    internal static MainWindow Instance { get; private set; }
    #endregion MainWindow Instance

    #region Properties
    public static bool CleanLaunch { get; set; } = true;
    #endregion Properties

    public MainWindow()
    {
        InitializeSettings();

        InitializeComponent();

        ReadSettings();

        ReadJson();

        GetIcons();

        lbDocs.ItemsSource = EntryClass.Entries;
    }

    #region Settings
    private void InitializeSettings()
    {
        stopwatch.Start();

        UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

        Instance = this;
    }

    public void ReadSettings()
    {
        // Set NLog configuration
        NLHelpers.NLogConfig(true);

        // Unhandled exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Put the version number in the title bar
        Title = $"{AppInfo.AppName} - {AppInfo.TitleVersion}";

        // Startup message in the temp file
        log.Info($"{AppInfo.AppName} {AppInfo.AppVersion} is starting up");

        // Window position
        Top = UserSettings.Setting.WindowTop;
        Left = UserSettings.Setting.WindowLeft;
        Height = UserSettings.Setting.WindowHeight;
        Width = UserSettings.Setting.WindowWidth;
        Topmost = UserSettings.Setting.KeepOnTop;

        // Log the .NET version, app framework and OS platform
        string version = Environment.Version.ToString();
        log.Debug($".NET version: {AppInfo.RuntimeVersion.Replace(".NET", "")}  ({version})");
        log.Debug(AppInfo.Framework);
        log.Debug(AppInfo.OsPlatform);

        // Light or dark
        SetBaseTheme((ThemeType)UserSettings.Setting.DarkMode);

        // Primary color
        SetPrimaryColor((AccentColor)UserSettings.Setting.PrimaryColor);

        // UI size
        double size = UIScale((MySize)UserSettings.Setting.UISize);
        MainGrid.LayoutTransform = new ScaleTransform(size, size);

        // Settings change event
        UserSettings.Setting.PropertyChanged += UserSettingChanged;
    }
    #endregion Settings

    #region Setting change
    private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
    {
        PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
        object newValue = prop?.GetValue(sender, null);
        log.Debug($"Setting change: {e.PropertyName} New Value: {newValue}");
        switch (e.PropertyName)
        {
            case nameof(UserSettings.Setting.KeepOnTop):
                Topmost = (bool)newValue;
                break;

            case nameof(UserSettings.Setting.IncludeDebug):
                NLHelpers.SetLogLevel((bool)newValue);
                break;

            case nameof(UserSettings.Setting.DarkMode):
                SetBaseTheme((ThemeType)newValue);
                break;

            case nameof(UserSettings.Setting.PrimaryColor):
                SetPrimaryColor((AccentColor)newValue);
                break;

            case nameof(UserSettings.Setting.UISize):
                int size = (int)newValue;
                double newSize = UIScale((MySize)size);
                MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
                break;
        }
    }
    #endregion Setting change

    #region Navigation
    private void NavigateToPage(NavPage selectedIndex)
    {
        switch (selectedIndex)
        {
            default:
                NavDrawer.IsLeftDrawerOpen = false;
                break;

            case NavPage.Maintenance:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowManitenanceDialog();
                break;

            case NavPage.Settings:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowSettingsDialog();
                break;

            case NavPage.About:
                NavDrawer.IsLeftDrawerOpen = false;
                DialogHelpers.ShowAboutDialog();
                break;

            case NavPage.Exit:
                Application.Current.Shutdown();
                break;
        }
    }

    private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NavigateToPage((NavPage)NavListBox.SelectedIndex);
        NavListBox.SelectedItem = null;
    }
    #endregion Navigation

    #region Get file icons
    public static void GetIcons()
    {
        foreach (EntryClass item in EntryClass.Entries)
        {
            if (!string.IsNullOrEmpty(item.FilePathOrURI))
            {
                string docPath = item.FilePathOrURI.TrimEnd('\\');
                if (File.Exists(docPath))
                {
                    Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(docPath);
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is a file");
                }
                // expand environmental variables for folders
                else if (Directory.Exists(Environment.ExpandEnvironmentVariables(docPath)))
                {
                    Icon temp = Properties.Resources.folder;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is a directory");
                }
                // if complete path wasn't supplied check the path
                else if (docPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new(docPath, 2048);
                    bool found = NativeMethods.PathFindOnPath(sb, new string[] { null });
                    if (found)
                    {
                        Icon temp = System.Drawing.Icon.ExtractAssociatedIcon(sb.ToString());
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.FilePathOrURI} was found on the Path");
                    }
                    else
                    {
                        Icon temp = Properties.Resources.question;
                        item.FileIcon = IconToImageSource(temp);
                        log.Debug($"{item.FilePathOrURI} was not found on the Path");
                    }
                }
                else if (docPath.Equals("outlookcal:", StringComparison.OrdinalIgnoreCase))
                {
                    Icon temp = Properties.Resources.calendar;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is Calendar app");
                }
                else if (docPath.Equals("xboxliveapp-1297287741:", StringComparison.OrdinalIgnoreCase))
                {
                    Icon temp = Properties.Resources.cards;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is Solitaire app");
                }
                else if (IsValidUrl(docPath))
                {
                    Icon temp = Properties.Resources.globe;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is valid URL");
                }
                else
                {
                    Icon temp = Properties.Resources.question;
                    item.FileIcon = IconToImageSource(temp);
                    log.Debug($"{item.FilePathOrURI} is something else");
                }
            }
            else
            {
                Icon temp = Properties.Resources.question;
                item.FileIcon = IconToImageSource(temp);
                log.Debug("Document path is empty or null");
            }
        }
    }

    #region Check URL
    private static bool IsValidUrl(string uriName)
    {
        const string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
        Regex Rgx = new(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return Rgx.IsMatch(uriName);
    }
    #endregion Check URL

    private static ImageSource IconToImageSource(Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            new Int32Rect(0, 0, icon.Width, icon.Height),
            BitmapSizeOptions.FromEmptyOptions());
    }

    #endregion Get file icons

    #region Launch app or URI
    private void ListBoxItem_MouseClick(object sender, MouseButtonEventArgs e)
    {
        if (((ListBoxItem)sender).Content is not EntryClass || lbDocs.SelectedItem == null)
        {
            return;
        }

        EntryClass entry = (EntryClass)lbDocs.SelectedItem;
        LaunchApp(entry);
    }

    private static async void LaunchApp(EntryClass item)
    {
        using Process launch = new();
        try
        {
            launch.StartInfo.FileName = Environment.ExpandEnvironmentVariables(item.FilePathOrURI);
            launch.StartInfo.UseShellExecute = true;
            _ = launch.Start();
            log.Info($"Opening \"{item.Title}\"");
        }
        catch (Exception ex)
        {
            CleanLaunch = false;
            log.Error(ex, "Open failed for \"{0}\" - \"{1}\"", item.Title, item.FilePathOrURI);
            SystemSounds.Exclamation.Play();
            ErrorDialog error = new()
            {
                Message = $"Error launching \"{item.Title}\" {item.FilePathOrURI}" +
                $"\n\n{ex.Message}"
            };
            _ = await DialogHost.Show(error, "MainDialogHost");
        }
    }
    #endregion Launch app or uri

    #region PopupBox button events

    private void BtnData_Click(object sender, RoutedEventArgs e)
    {
        string dir = AppInfo.AppDirectory;
        TextFileViewer.ViewTextFile(Path.Combine(dir, "MyLauncher.json"));
    }

    private void BtnLog_Click(object sender, RoutedEventArgs e)
    {
        TextFileViewer.ViewTextFile(NLHelpers.GetLogfileName());
    }

    private void BtnReadme_Click(object sender, RoutedEventArgs e)
    {
        string dir = AppInfo.AppDirectory;
        TextFileViewer.ViewTextFile(Path.Combine(dir, "ReadMe.txt"));
    }
    #endregion PopupBox button events

    #region Set light or dark theme
    private static void SetBaseTheme(ThemeType mode)
    {
        //Retrieve the app's existing theme
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        switch (mode)
        {
            case ThemeType.Light:
                theme.SetBaseTheme(Theme.Light);
                break;
            case ThemeType.Dark:
                theme.SetBaseTheme(Theme.Dark);
                break;
            case ThemeType.System:
                if (GetSystemTheme().Equals("light", StringComparison.OrdinalIgnoreCase))
                {
                    theme.SetBaseTheme(Theme.Light);
                }
                else
                {
                    theme.SetBaseTheme(Theme.Dark);
                }
                break;
            default:
                theme.SetBaseTheme(Theme.Light);
                break;
        }

        //Change the app's current theme
        paletteHelper.SetTheme(theme);
    }

    private static string GetSystemTheme()
    {
        BaseTheme? sysTheme = Theme.GetSystemTheme();
        if (sysTheme != null)
        {
            return sysTheme.ToString();
        }
        return string.Empty;
    }
    #endregion Set light or dark theme

    #region Set primary color
    private static void SetPrimaryColor(AccentColor color)
    {
        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        PrimaryColor primary;
        switch (color)
        {
            case AccentColor.Red:
                primary = PrimaryColor.Red;
                break;
            case AccentColor.Pink:
                primary = PrimaryColor.Pink;
                break;
            case AccentColor.Purple:
                primary = PrimaryColor.Purple;
                break;
            case AccentColor.DeepPurple:
                primary = PrimaryColor.DeepPurple;
                break;
            case AccentColor.Indigo:
                primary = PrimaryColor.Indigo;
                break;
            case AccentColor.Blue:
                primary = PrimaryColor.Blue;
                break;
            case AccentColor.LightBlue:
                primary = PrimaryColor.LightBlue;
                break;
            case AccentColor.Cyan:
                primary = PrimaryColor.Cyan;
                break;
            case AccentColor.Teal:
                primary = PrimaryColor.Teal;
                break;
            case AccentColor.Green:
                primary = PrimaryColor.Green;
                break;
            case AccentColor.LightGreen:
                primary = PrimaryColor.LightGreen;
                break;
            case AccentColor.Lime:
                primary = PrimaryColor.Lime;
                break;
            case AccentColor.Yellow:
                primary = PrimaryColor.Yellow;
                break;
            case AccentColor.Amber:
                primary = PrimaryColor.Amber;
                break;
            case AccentColor.Orange:
                primary = PrimaryColor.Orange;
                break;
            case AccentColor.DeepOrange:
                primary = PrimaryColor.DeepOrange;
                break;
            case AccentColor.Brown:
                primary = PrimaryColor.Brown;
                break;
            case AccentColor.Grey:
                primary = PrimaryColor.Grey;
                break;
            case AccentColor.BlueGray:
                primary = PrimaryColor.BlueGrey;
                break;
            default:
                primary = PrimaryColor.Blue;
                break;
        }
        System.Windows.Media.Color primaryColor = SwatchHelper.Lookup[(MaterialDesignColor)primary];
        theme.SetPrimaryColor(primaryColor);
        paletteHelper.SetTheme(theme);
    }
    #endregion Set primary color

    #region UI scale converter
    private static double UIScale(MySize size)
    {
        switch (size)
        {
            case MySize.Smallest:
                return 0.90;
            case MySize.Smaller:
                return 0.95;
            case MySize.Default:
                return 1.0;
            case MySize.Larger:
                return 1.05;
            case MySize.Largest:
                return 1.1;
            default:
                return 1.0;
        }
    }
    #endregion UI scale converter

    #region Keyboard Events
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
        {
            if (e.Key == Key.L)
            {
                NavigateToPage(NavPage.Maintenance);
            }

            if (e.Key == Key.M)
            {
                switch (UserSettings.Setting.DarkMode)
                {
                    case (int)ThemeType.Light:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Dark;
                        break;
                    case (int)ThemeType.Dark:
                        UserSettings.Setting.DarkMode = (int)ThemeType.System;
                        break;
                    case (int)ThemeType.System:
                        UserSettings.Setting.DarkMode = (int)ThemeType.Light;
                        break;
                }
                SnackbarMsg.ClearAndQueueMessage($"Theme set to {(ThemeType)UserSettings.Setting.DarkMode}");
            }
            if (e.Key == Key.Add)
            {
                EverythingLarger();
            }
            if (e.Key == Key.Subtract)
            {
                EverythingSmaller();
            }
            if (e.Key == Key.OemComma)
            {
                if (!DialogHost.IsDialogOpen("MainDialogHost"))
                {
                    DialogHelpers.ShowSettingsDialog();
                }
                else
                {
                    DialogHost.Close("MainDialogHost");
                    DialogHelpers.ShowSettingsDialog();
                }
            }
        }

        if (e.Key == Key.F1)
        {
            if (!DialogHost.IsDialogOpen("MainDialogHost"))
            {
                DialogHelpers.ShowAboutDialog();
            }
            else
            {
                DialogHost.Close("MainDialogHost");
                DialogHelpers.ShowAboutDialog();
            }
        }
    }
    #endregion Keyboard Events

    #region Smaller/Larger
    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
            return;

        if (e.Delta > 0)
        {
            EverythingLarger();
        }
        else if (e.Delta < 0)
        {
            EverythingSmaller();
        }
    }

    public void EverythingSmaller()
    {
        int size = UserSettings.Setting.UISize;
        if (size > 0)
        {
            size--;
            UserSettings.Setting.UISize = size;
            double newSize = UIScale((MySize)size);
            MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            SnackbarMsg.ClearAndQueueMessage($"Size set to {(MySize)UserSettings.Setting.UISize}");
        }
    }

    public void EverythingLarger()
    {
        int size = UserSettings.Setting.UISize;
        if (size < 4)
        {
            size++;
            UserSettings.Setting.UISize = size;
            double newSize = UIScale((MySize)size);
            MainGrid.LayoutTransform = new ScaleTransform(newSize, newSize);
            SnackbarMsg.ClearAndQueueMessage($"Size set to {(MySize)UserSettings.Setting.UISize}");
        }
    }
    #endregion Smaller/Larger

    #region Window Events
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        stopwatch.Stop();
        log.Info($"{AppInfo.AppName} is shutting down.  Elapsed time: {stopwatch.Elapsed:h\\:mm\\:ss\\.ff}");

        // Shut down NLog
        LogManager.Shutdown();

        // Save settings
        UserSettings.Setting.WindowLeft = Math.Floor(Left);
        UserSettings.Setting.WindowTop = Math.Floor(Top);
        UserSettings.Setting.WindowWidth = Math.Floor(Width);
        UserSettings.Setting.WindowHeight = Math.Floor(Height);
        UserSettings.SaveSettings();
    }
    #endregion Window Events

    #region Get the menu JSON file name
    private static string GetJsonFile()
    {
        return Path.Combine(AppInfo.AppDirectory, "MyLauncher.json");
    }
    #endregion Get the menu JSON file name

    #region Read the JSON file
    public static void ReadJson()

    {
        string jsonfile = GetJsonFile();

        log.Debug($"Reading JSON file: {jsonfile}");
        try
        {
            string json = File.ReadAllText(jsonfile);
            EntryClass.Entries = JsonSerializer.Deserialize<BindingList<EntryClass>>(json);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
        {
            log.Error(ex, "File or Directory not found {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"File or Directory not found:\n\n{ex.Message}\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            log.Error(ex, "Error reading file: {0}", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"Error reading file:\n\n{ex.Message}",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
        }

        if (EntryClass.Entries == null)
        {
            log.Error("File {0} is empty or is invalid", jsonfile);
            SystemSounds.Exclamation.Play();
            _ = new MDCustMsgBox($"File {jsonfile} is empty or is invalid\n\nUnable to continue.",
                "My Launcher Error", ButtonType.Ok).ShowDialog();
            Environment.Exit(2);
        }

        if (EntryClass.Entries.Count == 1)
        {
            log.Info($"Read {EntryClass.Entries.Count} entry from {jsonfile}");
        }
        else
        {
            log.Info($"Read {EntryClass.Entries.Count} entries from {jsonfile}");
        }
    }
    #endregion Read the JSON file

    #region Mouse enter/leave shadow effect
    private void Card_MouseEnter(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth3);
    }

    private void Card_MouseLeave(object sender, MouseEventArgs e)
    {
        Card card = sender as Card;
        ShadowAssist.SetShadowDepth(card, ShadowDepth.Depth2);
    }
    #endregion Mouse enter/leave shadow effect

    #region Exit button event
    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
    #endregion Exit button event

    #region Unhandled Exception Handler
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        log.Error("Unhandled Exception");
        Exception e = (Exception)args.ExceptionObject;
        log.Error(e.Message);
        if (e.InnerException != null)
        {
            log.Error(e.InnerException.ToString());
        }
        log.Error(e.StackTrace);

        _ = new MDCustMsgBox("An error has occurred. See the log file",
            "My Launcher Error", ButtonType.Ok).ShowDialog();
    }
    #endregion Unhandled Exception Handler
}