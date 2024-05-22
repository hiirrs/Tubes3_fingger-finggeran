using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FingerprintCheck;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isFileUploaded = false;
    public MainWindow()
    {
        InitializeComponent();
    }
    private void ChoosePictureButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(openFileDialog.FileName));
                ChosenImage.Source = bitmap;
                isFileUploaded = true; // Set to true when a file is chosen
            }
        }
    private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                if (radioButton.Content.ToString() == "KMP")
                {
                    MessageBox.Show("KMP selected");
                }
                else if (radioButton.Content.ToString() == "BM")
                {
                    MessageBox.Show("BM selected");
                }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isFileUploaded)
            {
                MessageBox.Show("Please upload a fingerprint image before searching.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SearchResult.Visibility = Visibility.Collapsed;
            LoadingScreen.Visibility = Visibility.Visible;

            await Task.Delay(3000);

            LoadingScreen.Visibility = Visibility.Collapsed;
            SearchResult.Visibility = Visibility.Visible;

            int resultCount = 10;
            ResultText.Text = $"Search Result: {resultCount} items found.";
        }
}