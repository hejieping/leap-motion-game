using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;
using Leap;
namespace WpfApplication1
{
    public  class Enemy:System.Windows.UIElement
    {
        public System.Windows.Controls.Image img;

        public Enemy(String path = "D:/Projects/WpfApplication1/Particle.png")
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.EndInit();
            img = new System.Windows.Controls.Image();
            img.Source = image;
        }

        void Intersect(Enemy enmey)
        {

        }
    }
}
