using Avalonia.Media.Imaging;
using System;
using System.IO;

namespace src
{
    public class ResultData
    {
        public string Name { get; set; }
        public string Place { get; set; }
        public string Birthdate { get; set; }
        public string BloodType { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }
        public string WorkStatus { get; set; }
        public string Nationality { get; set; }
        public string ImagePath { get; set; }

        public Bitmap ImageSource
        {
            get
            {
                if (File.Exists(ImagePath))
                {
                    return new Bitmap(ImagePath);
                }
                return null;
            }
        }
    }
}
