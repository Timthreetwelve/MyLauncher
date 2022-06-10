﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyLauncher;

public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
{
    #region Properties

    public bool AllowRightButton
    {
        get => allowRightButton;
        set
        {
            allowRightButton = value;
            OnPropertyChanged();
        }
    }

    public int BorderWidth
    {
        get { return borderWidth; }
        set
        {
            borderWidth = value;
            OnPropertyChanged();
        }
    }

    public int DarkMode
    {
        get => darkmode;
        set
        {
            darkmode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeDebug
    {
        get => includeDebug;
        set
        {
            includeDebug = value;
            OnPropertyChanged();
        }
    }

    public bool KeepOnTop
    {
        get => keepOnTop;
        set
        {
            keepOnTop = value;
            OnPropertyChanged();
        }
    }

    public int LastHostID
    {
        get => lastHostID;
        set
        {
            lastHostID = value;
            OnPropertyChanged();
        }
    }

    public int ListBoxFontWeight
    {
        get { return listBoxFontWeight; }
        set
        {
            listBoxFontWeight = value;
            OnPropertyChanged();
        }
    }

    public int ListBoxSpacing
    {
        get => listBoxSpacing;
        set
        {
            listBoxSpacing = value;
            OnPropertyChanged();
        }
    }

    public bool MinimizeToTray
    {
        get => minimizeToTray;
        set
        {
            minimizeToTray = value;
            OnPropertyChanged();
        }
    }

    public bool PlaySound
    {
        get => playSound;
        set
        {
            playSound = value;
            OnPropertyChanged();
        }
    }

    public int PrimaryColor
    {
        get => primaryColor;
        set
        {
            primaryColor = value;
            OnPropertyChanged();
        }
    }

    public int SecondaryColor
    {
        get => secondaryColor;
        set
        {
            secondaryColor = value;
            OnPropertyChanged();
        }
    }

    public bool ShowExitButton
    {
        get { return showExitButton; }
        set
        {
            showExitButton = value;
            OnPropertyChanged();
        }
    }

    public bool ShowFileIcons
    {
        get => showFileIcons;
        set
        {
            showFileIcons = value;
            OnPropertyChanged();
        }
    }

    public bool StartMinimized
    {
        get => startMinimized;
        set
        {
            startMinimized = value;
            OnPropertyChanged();
        }
    }

    public bool StartWithWindows
    {
        get => startWithWindows;
        set
        {
            startWithWindows = value;
            OnPropertyChanged();
        }
    }

    public string TitleText
    {
        get => titleText;
        set
        {
            titleText = value;
            OnPropertyChanged();
        }
    }

    public int UISize
    {
        get => uiSize;
        set
        {
            uiSize = value;
            OnPropertyChanged();
        }
    }

    public double WindowHeight
    {
        get
        {
            if (windowHeight < 100)
            {
                windowHeight = 100;
            }
            return windowHeight;
        }
        set => windowHeight = value;
    }

    public double WindowLeft
    {
        get
        {
            if (windowLeft < 0)
            {
                windowLeft = 0;
            }
            return windowLeft;
        }
        set => windowLeft = value;
    }

    public double WindowTop
    {
        get
        {
            if (windowTop < 0)
            {
                windowTop = 0;
            }
            return windowTop;
        }
        set => windowTop = value;
    }

    public double WindowWidth
    {
        get
        {
            if (windowWidth < 100)
            {
                windowWidth = 100;
            }
            return windowWidth;
        }
        set => windowWidth = value;
    }
    #endregion Properties

    #region Private backing fields
    private bool allowRightButton = true;
    private int borderWidth = 2;
    private int darkmode = (int)ThemeType.Light;
    private bool includeDebug = false;
    private bool keepOnTop = false;
    private int lastHostID = 100;
    private int listBoxFontWeight = (int)Weight.Regular;
    private int listBoxSpacing = (int)Spacing.Comfortable;
    private bool minimizeToTray = false;
    private bool playSound = true;
    private int primaryColor = (int)AccentColor.Blue;
    private int secondaryColor = (int)AccentColor.Red;
    private bool showExitButton = true;
    private bool showFileIcons = true;
    private bool startMinimized = false;
    private bool startWithWindows = false;
    private string titleText = "Click on any App Below to Open it";
    private int uiSize = (int)MySize.Default;
    private double windowHeight = 500;
    private double windowLeft = 100;
    private double windowTop = 100;
    private double windowWidth = 500;
    #endregion Private backing fields

    #region Handle property change event
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Handle property change event
}
