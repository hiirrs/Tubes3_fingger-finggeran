using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace src;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        SetupEventHandlers();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SetupEventHandlers()
    {
        var choosePictureButton = this.FindControl<Button>("choosePictureButton");
        var searchButton = this.FindControl<Button>("searchButton");
        if (choosePictureButton != null)
            choosePictureButton.Click += ChoosePictureButton_Click;
        if (searchButton != null)
            searchButton.Click += SearchButton_Click;
    }

    private async void ChoosePictureButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = (this.VisualRoot as Window);
        if (window is null) return;
        
        var dialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = "Select a Fingerprint Image",
            Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Image Files", Extensions = new List<string> { "jpg", "jpeg", "png", "bmp" } } }
        };

        var result = await dialog.ShowAsync(window);
        if (result != null && result.Length > 0)
        {
            var fingerprintImage = this.FindControl<Image>("fingerprintImage");
            if (fingerprintImage != null)
            {
                fingerprintImage.Source = new Bitmap(result[0]);
                fingerprintImage.Width = 200;  // Set the width as needed
                fingerprintImage.Height = 200; // Set the height as needed
                fingerprintImage.Stretch = Avalonia.Media.Stretch.Uniform;
            }
        }
    }

    private async void SearchButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateMessage("LOADING...");
        await Task.Delay(3000); // Simulate a 3-second loading process
        UpdateMessage("Search Complete!");
        ShowSearchResults(true);
    }

    private void UpdateMessage(string message)
    {
        var dynamicMessage = this.FindControl<TextBlock>("dynamicMessage");
        if (dynamicMessage != null)
        {
            dynamicMessage.Text = message;
        }
    }

    private void ShowSearchResults(bool show)
    {
        var matchFoundMessage = this.FindControl<TextBlock>("matchFoundMessage");
        var detailsGrid = this.FindControl<Grid>("detailsGrid");

        matchFoundMessage.IsVisible = show;
        detailsGrid.IsVisible = show;

        var resultData = new ResultData
        {
            NIK = "1234",
            Name = "John Doe",
            Place = "New York",
            Birthdate = "01-01-1990",
            BloodType = "O+",
            Gender = "Male",
            Address = "1234 Anywhere St",
            Religion = "None",
            MaritalStatus = "Single",
            WorkStatus = "Employed",
            Nationality = "American",
            ImagePath = "D:/Akademik/SEMESTER 4/STIMA/TUBES/Tubes3_Fingerfingeran/test/dataset/1__M_Left_index_finger.BMP"
        };

        var panel = this.FindControl<StackPanel>("rightPanel");
        if (panel != null)
        {
            panel.DataContext = resultData;
        }

        
    }

    private void DisplayResults()
    {
        var resultData = new ResultData
        {
            Name = "John Doe",
            Place = "New York",
            Birthdate = "01-01-1990",
            BloodType = "O+",
            Gender = "Male",
            Address = "1234 Anywhere St",
            Religion = "None",
            MaritalStatus = "Single",
            WorkStatus = "Employed",
            Nationality = "American"
        };

        var panel = this.FindControl<StackPanel>("rightPanel");
        if (panel != null)
        {
            panel.DataContext = resultData;
        }
    }

    private void PerformSearch()
    {
        Console.WriteLine("Searching...");
        // Placeholder for the actual search logic
    }
}
