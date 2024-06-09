using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using src;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        private string toCompareImagePath = string.Empty;
        private string selectedAlgorithm = "KMP";
        private string bestMatchImagePath = string.Empty;
        private Biodata data = new();

        public MainWindow()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            choosePictureButton.Click += ChoosePictureButton_Click;
            searchButton.Click += SearchButton_Click;
            kmpRadioButton.Checked += RadioButton_Checked;
            bmRadioButton.Checked += RadioButton_Checked;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                selectedAlgorithm = radioButton.Content.ToString(); // This assumes the Content directly contains the algorithm name ("KMP" or "BM")
            }
        }

        private async void ChoosePictureButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Select a Fingerprint Image"
            };

            if (dialog.ShowDialog() == true)
            {
                toCompareImagePath = dialog.FileName;
                var fingerprintImage = FindName("fingerprintImage") as Image;
                if (fingerprintImage != null)
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(toCompareImagePath));
                    fingerprintImage.Source = bitmap;
                    fingerprintImage.Width = 200;
                    fingerprintImage.Height = 200;
                    fingerprintImage.Stretch = Stretch.Uniform;
                }
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var fingerprintImage = FindName("fingerprintImage") as Image;
            var dynamicMessage = FindName("dynamicMessage") as TextBlock;
            if (fingerprintImage?.Source == null)
            {
                UpdateMessage("Please select a fingerprint image first.");
                return;
            }

            UpdateMessage("LOADING ...");
            string[] imagePathsFromDatabase = DatabaseManager.GetImagePathsFromDatabase();
            List<string> databaseName = DatabaseManager.GetAlayNamesFromDatabase();
            List<string> correctNames = DatabaseManager.GetCorrectNamesFromDatabase();

            Dictionary<string, string> nameMap = new Dictionary<string, string>();

            foreach (string nama2 in databaseName)
            {
                string fixedName = AlayFixer.FixAlayText(nama2, correctNames);
                if (!nameMap.ContainsKey(nama2))
                {
                    nameMap.Add(nama2, fixedName);
                }
            }

            UpdateMessage("mAP DAH KEKUMPUL");

            string name = "";
            long execution = 0;
            double bestLevenshteinSimilarity = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            UpdateMessage("Mulai ...");
            await Task.Run(() =>
            {
                try
                {
                    List<string> binaryStrings1 = ImageProcessor.ConvertImageToBinaryString2(toCompareImagePath);
                    List<string> asciiStrings1 = ImageProcessor.ConvertBinaryToAscii2(binaryStrings1);
                    string asciiBlock1 = ImageProcessor.ExtractCentralAsciiBlock2(asciiStrings1, 30);

                    int bestMatchPosition = -1;
                    bestLevenshteinSimilarity = 0;

                    foreach (string imagePath2 in imagePathsFromDatabase)
                    {
                        List<string> binaryStrings2 = ImageProcessor.ConvertImageToBinaryString2(imagePath2);
                        List<string> asciiStrings2 = ImageProcessor.ConvertBinaryToAscii2(binaryStrings2);

                        int matchPosition = -1;
                        if (selectedAlgorithm == "BM")
                        {
                            Dispatcher.Invoke(() => UpdateMessage("dengan bm ..."));
                            BoyerMoore bm = new BoyerMoore(asciiBlock1);
                            foreach (string asciiString2 in asciiStrings2)
                            {
                                matchPosition = bm.Search(asciiString2);
                                if (matchPosition != -1)
                                {
                                    break;
                                }
                            }
                        }
                        else if (selectedAlgorithm == "KMP")
                        {
                            Dispatcher.Invoke(() => UpdateMessage("dengan kmp ..."));
                            KMP kmp = new KMP(asciiBlock1);
                            foreach (string asciiString2 in asciiStrings2)
                            {
                                matchPosition = kmp.Search(asciiString2);
                                if (matchPosition != -1)
                                {
                                    break;
                                }
                            }
                        }

                        if (matchPosition != -1)
                        {
                            bestMatchPosition = matchPosition;
                            bestMatchImagePath = imagePath2;
                            break;
                        }
                        else
                        {
                            double levenshteinSimilarity = 0;
                            foreach (string asciiString2 in asciiStrings2)
                            {
                                levenshteinSimilarity = Levenshtein.CalculateLevenshteinBlockString(asciiBlock1, asciiString2);
                            }
                            if (levenshteinSimilarity > bestLevenshteinSimilarity)
                            {
                                bestLevenshteinSimilarity = levenshteinSimilarity;
                                bestMatchImagePath = imagePath2;
                            }
                        }
                    }

                    if (bestMatchPosition != -1)
                    {
                        Console.WriteLine("Match found at position: " + bestMatchPosition);
                        Console.WriteLine("Matching image path: " + bestMatchImagePath);
                        name = DatabaseManager.GetNameFromImagePath(bestMatchImagePath);
                        bestLevenshteinSimilarity = 100;
                    }
                    else
                    {
                        Console.WriteLine("No exact match found. Best similarity match:");
                        Console.WriteLine("Image path: " + bestMatchImagePath);
                        Console.WriteLine($"Levenshtein similarity percentage: {bestLevenshteinSimilarity:F2}%");
                        name = DatabaseManager.GetNameFromImagePath(bestMatchImagePath);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    Dispatcher.Invoke(() => {
                        UpdateMessage("An error occurred during the search process.");
                        MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }

                try
                {
                    string keyNameToDatabase = DatabaseManager.FindAlayName(nameMap, name);
                    data = DatabaseManager.GetBiodataForName(keyNameToDatabase);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while fetching biodata: " + ex.Message);
                    Dispatcher.Invoke(() => {
                        UpdateMessage("An error occurred while fetching biodata.");
                        MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }

                stopwatch.Stop();
                execution = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
            });

            UpdateMessage("Search Complete!");
            dynamicMessage.Text = "Search Complete!";
            matchFoundMessage.Visibility = Visibility.Visible;
            executionTimeMessage.Text = $"{execution} ms";
            similarityPercentageMessage.Text = $"{bestLevenshteinSimilarity:F2}%";
            ShowSearchResults(true, data, bestMatchImagePath);
        }

        private void UpdateMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var dynamicMessage = FindName("dynamicMessage") as TextBlock;
                if (dynamicMessage != null)
                {
                    dynamicMessage.Text = message;
                }
            });
        }

        private void ShowSearchResults(bool show, Biodata biodata, string ImagePathResult)
        {
            Dispatcher.Invoke(() =>
            {
                matchFoundMessage.Visibility = show ? Visibility.Visible : Visibility.Hidden;
                detailsGrid.Visibility = show ? Visibility.Visible : Visibility.Hidden;

                var resultData = new ResultData
                {
                    NIK = biodata.NIK,
                    Name = biodata.Nama,
                    Place = biodata.TempatLahir,
                    Birthdate = biodata.TanggalLahir,
                    BloodType = biodata.GolonganDarah,
                    Gender = biodata.JenisKelamin,
                    Address = biodata.Alamat,
                    Religion = biodata.Agama,
                    MaritalStatus = biodata.StatusPerkawinan,
                    WorkStatus = biodata.Pekerjaan,
                    Nationality = biodata.Kewarganegaraan,
                    ImagePath = Path.GetFullPath(ImagePathResult)
                //     ExecutionTime = $"{elapsedMilliseconds} ms",
                // SimilarityPercentage = bestLevenshteinSimilarity == 1.0 ? "100%" : $"{bestLevenshteinSimilarity:F2}%"x`x`
                };

                var panel = FindName("rightPanel") as StackPanel;
                if (panel != null)
                {
                    panel.DataContext = resultData;
                }
            });
        }
    }
}
