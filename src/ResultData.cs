using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

public class ResultData : INotifyPropertyChanged
{
    private string nik;
    public string NIK
    {
        get => nik;
        set
        {
            nik = value;
            OnPropertyChanged(nameof(NIK));
        }
    }

    private string name;
    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    private string place;
    public string Place
    {
        get => place;
        set
        {
            place = value;
            OnPropertyChanged(nameof(Place));
        }
    }

    private string birthdate;
    public string Birthdate
    {
        get => birthdate;
        set
        {
            birthdate = value;
            OnPropertyChanged(nameof(Birthdate));
        }
    }

    private string bloodType;
    public string BloodType
    {
        get => bloodType;
        set
        {
            bloodType = value;
            OnPropertyChanged(nameof(BloodType));
        }
    }

    private string gender;
    public string Gender
    {
        get => gender;
        set
        {
            gender = value;
            OnPropertyChanged(nameof(Gender));
        }
    }

    private string address;
    public string Address
    {
        get => address;
        set
        {
            address = value;
            OnPropertyChanged(nameof(Address));
        }
    }

    private string religion;
    public string Religion
    {
        get => religion;
        set
        {
            religion = value;
            OnPropertyChanged(nameof(Religion));
        }
    }

    private string maritalStatus;
    public string MaritalStatus
    {
        get => maritalStatus;
        set
        {
            maritalStatus = value;
            OnPropertyChanged(nameof(MaritalStatus));
        }
    }

    private string workStatus;
    public string WorkStatus
    {
        get => workStatus;
        set
        {
            workStatus = value;
            OnPropertyChanged(nameof(WorkStatus));
        }
    }

    private string nationality;
    public string Nationality
    {
        get => nationality;
        set
        {
            nationality = value;
            OnPropertyChanged(nameof(Nationality));
        }
    }

    private string imagePath;
    public string ImagePath
    {
        get => imagePath;
        set
        {
            imagePath = value;
            OnPropertyChanged(nameof(ImagePath));
        }
    }

    public BitmapImage ImageSource
    {
        get
        {
            if (File.Exists(ImagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ImagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            return null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
