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
using System.Collections.Generic;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private MyLeapListener listener;  //leap motion 监听
        private Controller controller = new Controller();   //leap motino controller
        private DispatcherTimer timer = new DispatcherTimer();  //敌人生成事件
        private DispatcherTimer timer_check = new DispatcherTimer();  //检测碰撞事件
        private DispatcherTimer timer_track = new DispatcherTimer(); //敌人跟踪事件
        private DispatcherTimer timer_score = new DispatcherTimer();//分数事件
        private DispatcherTimer timer_gestureTest = new DispatcherTimer();
        private DispatcherTimer timer_levelup = new DispatcherTimer();
        private DispatcherTimer timer_getusergesture = new DispatcherTimer();

        private int health = 100;
        private int currentlevel = 1;
        private int notExtended = 0;
        private List<Enemy> enemylist;
        private List<Ellipse> ellipselist = new List<Ellipse>();
        private List<Ellipse> ewl = new List<Ellipse>();
        private List<Pos> ewlpos = new List<Pos>();
        private List<Ellipse> lenemylist = new List<Ellipse>();
        private List<Ellipse> cenemylist = new List<Ellipse>();
        private int time = 0;
        private int time_score = 0;
        private int score = 0;

        public MainWindow()
        {

            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer_check.Tick += new EventHandler(interactCheck);
            timer_track.Tick += new EventHandler(track);
            timer_score.Tick += new EventHandler(addScore);

            timer_gestureTest.Interval = TimeSpan.FromSeconds(1);
            timer_track.Interval = TimeSpan.FromSeconds(0.1);
            timer_check.Interval = TimeSpan.FromSeconds(0.1);
            timer_score.Interval = TimeSpan.FromSeconds(1);
            timer.Interval = TimeSpan.FromSeconds(1);   //设置刷新的间隔时间

            timer_levelup.Tick += new EventHandler(updateLevel);
            timer_levelup.Interval = TimeSpan.FromSeconds(SharedProperties.LEVEL_NEW_TIME);

            timer_getusergesture.Tick += new EventHandler(getUserGesture);
            timer_getusergesture.Interval = TimeSpan.FromMilliseconds(100);
        }

        public void testImageMove()
        {
        //    BitmapImage myBitmapImage = new BitmapImage(new Uri(@"‪pack://application:,,,", UriKind.Absolute));

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("D:/Projects/WpfApplication1/Particle.png");
            image.EndInit();

            System.Windows.Controls.Image img = new System.Windows.Controls.Image();

            img.Source = image;

            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                //img.Arrange(new Rect(new Size(10d,10d)));               
                canvas.Children.Add(img);
                Canvas.SetLeft(img, 100);
                Canvas.SetTop(img, 100);
                img.RenderSize = new Size(20d, 20d);
            }), null);
        }
        //
        public void testEnemy()
        {
            Enemy enemy = new Enemy();
            this.Dispatcher.BeginInvoke(new Action(delegate
            {               
                canvas.Children.Add(enemy);
                Canvas.SetLeft(enemy, 200);
                Canvas.SetTop(enemy, 200);
                //img.RenderSize = new Size(20d, 20d);
            }), null);

        }

        void testGestures(object sender, EventArgs e)  //手势测试
        {
            
            Leap.GestureList glist = controller.Frame().Gestures();
            
            foreach(Leap.Gesture g in glist)
            {
                switch (g.Type)
                {
                    case Gesture.GestureType.TYPE_CIRCLE:
                        this.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            gestureLabel.Content = "circle";
                        }), null);

                        break;
                    case Gesture.GestureType.TYPE_KEY_TAP:
                        this.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            gestureLabel.Content = "keytap";
                        }), null);

                        break;
                    case Gesture.GestureType.TYPE_SCREEN_TAP:
                        this.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            gestureLabel.Content = "screentap";
                        }), null);
                        break;
                    case Gesture.GestureType.TYPE_SWIPE:
                        this.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            gestureLabel.Content = "swipe";
                        }), null);
                        break;
                    default: break;

                }
            }
        }
        void startGame()  //启动游戏所需事件
        {
            startGameAnimation();

            listener = new MyLeapListener();
            listener.OnFrameEvent += listener_OnFrameEvent;
            controller.AddListener(listener);
            gameStartButton.IsEnabled = false;        /* btn1表示“开始游戏” */
            health = 100;
            health_block.Text = health.ToString();
            currentlevel = 1;
            score = 0;
            time = 0;


            superPower();

            timer.Start();
            timer_track.Start();
            timer_check.Start();
            timer_score.Start();
            timer_gestureTest.Start();
            timer_levelup.Start();
            timer_getusergesture.Start();

        }
        void finishGame()
        {

            timer.Stop();
            timer_track.Stop();
            timer_check.Stop();
            timer_score.Stop();
            timer_gestureTest.Stop();
            timer_levelup.Stop();
            timer_getusergesture.Stop();

            score = 0;
            if (controller.IsConnected)
            {
                controller.RemoveListener(listener);
            }
            gameOverAnimation();
        }
        void addScore(object sender, EventArgs e)  //每秒触发一次该事件，score+1
        {
            time++;
            if (time / 10 > time_score)
            {
                score = score + 10;
                time_score++;
            }

            time_block.Text = time.ToString();
            score_block.Text = score.ToString();
        }
        void track(object sender, EventArgs e)                                                                /* 敌人动画跟踪 */
        {
            foreach(Ellipse ellipse in ellipselist)
            {
                Point p = new Point();
                p.X = (Canvas.GetLeft(myEllipse) * SharedProperties.MYELLIPSEPERCENT + Canvas.GetLeft(ellipse) * SharedProperties.ENEMYELLIPSEPERCENT);
                p.Y = (Canvas.GetTop(myEllipse) * SharedProperties.MYELLIPSEPERCENT + Canvas.GetTop(ellipse) * SharedProperties.ENEMYELLIPSEPERCENT);
                double xTime, yTime;                                                            /* x，y方向移动到p左边所需要的时间 */
                if ((Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) > 0)
                {
                    xTime = (Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) * SharedProperties.MOVETIME;
                }
                else
                {
                    xTime = (Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) * SharedProperties.MOVETIME * -1;
                }
                if ((Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) > 0)
                {
                    yTime = (Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) * SharedProperties.MOVETIME;
                }
                else
                {
                    yTime = (Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) * SharedProperties.MOVETIME * -1;
                }

                Storyboard storyboard = new Storyboard(); /* 新建一个动画板 */

                /* 添加X轴方向的动画 */
                DoubleAnimation doubleAnimation = new DoubleAnimation(
                    Canvas.GetLeft(ellipse), p.X, new Duration(TimeSpan.FromMilliseconds(xTime)));
                Storyboard.SetTarget(doubleAnimation, ellipse);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
                storyboard.Children.Add(doubleAnimation);

                /* 添加Y轴方向的动画 */
                doubleAnimation = new DoubleAnimation(
                    Canvas.GetTop(ellipse), p.Y, new Duration(TimeSpan.FromMilliseconds(yTime)));
                Storyboard.SetTarget(doubleAnimation, ellipse);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Top)"));
                storyboard.Children.Add(doubleAnimation);

                if (!Resources.Contains("rectAnimation"))
                {
                    Resources.Add("rectAnimation", storyboard);
                }
                storyboard.Begin(); /* 开始运行动画 */
            }

            foreach(Ellipse ellipse in cenemylist)
            {
                Point p = new Point();
                p.X = (Canvas.GetLeft(myEllipse) * SharedProperties.MYELLIPSEPERCENT + Canvas.GetLeft(ellipse) * SharedProperties.ENEMYELLIPSEPERCENT);
                p.Y = (Canvas.GetTop(myEllipse) * SharedProperties.MYELLIPSEPERCENT + Canvas.GetTop(ellipse) * SharedProperties.ENEMYELLIPSEPERCENT);
                double xTime, yTime;                                                            /* x，y方向移动到p左边所需要的时间 */
                if ((Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) > 0)
                {
                    xTime = (Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) * SharedProperties.MOVETIME;
                }
                else
                {
                    xTime = (Canvas.GetLeft(myEllipse) - Canvas.GetLeft(ellipse)) * SharedProperties.MOVETIME * -1;
                }
                if ((Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) > 0)
                {
                    yTime = (Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) * SharedProperties.MOVETIME;
                }
                else
                {
                    yTime = (Canvas.GetTop(myEllipse) - Canvas.GetTop(ellipse)) * SharedProperties.MOVETIME * -1;
                }

                Storyboard storyboard = new Storyboard(); /* 新建一个动画板 */

                /* 添加X轴方向的动画 */
                DoubleAnimation doubleAnimation = new DoubleAnimation(
                    Canvas.GetLeft(ellipse), p.X, new Duration(TimeSpan.FromMilliseconds(xTime/2)));
                Storyboard.SetTarget(doubleAnimation, ellipse);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
                storyboard.Children.Add(doubleAnimation);

                /* 添加Y轴方向的动画 */
                doubleAnimation = new DoubleAnimation(
                    Canvas.GetTop(ellipse), p.Y, new Duration(TimeSpan.FromMilliseconds(yTime/2)));
                Storyboard.SetTarget(doubleAnimation, ellipse);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Top)"));
                storyboard.Children.Add(doubleAnimation);

                if (!Resources.Contains("rectAnimation"))
                {
                    Resources.Add("rectAnimation", storyboard);
                }
                storyboard.Begin(); /* 开始运行动画 */
            }
        }
        void timer_Tick(object sender, EventArgs e)  //每秒出发一次该事件，生成一个敌人，并且敌人五秒消失
        {
            /* 敌方ellipse定义 */
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 20;
            ellipse.Height = 20;
            ellipse.Fill = new SolidColorBrush(Colors.Black);
            Random ran = new Random();
            int ellpise_x = ran.Next(0, SharedProperties.SCREEN_WIDTH);
            int ellpise_y = ran.Next(0, SharedProperties.SCREEN_HEIGHT);
            ellipse.SetValue(Canvas.LeftProperty, (double)ellpise_x);
            ellipse.SetValue(Canvas.TopProperty, (double)ellpise_y);



            canvas.Children.Add(ellipse);
            ellipselist.Add(ellipse);

            Thread t = new Thread(() =>
            {
                System.Threading.Thread.Sleep(SharedProperties.ENEMY_LIFE_TIME);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(ellipse);
                    ellipselist.Remove(ellipse);
                    
                }));
            });
            t.Start();
        }
        void listener_OnFrameEvent(object sender, EventArgs e)        /* leap motion监听事件，用来控制myellipse位置 */
        {
            Leap.Frame frame = controller.Frame();                  /* using LeapFrame = Leap.Frame; */

            HandList handlist = frame.Hands;
            double x = 0d, y = 0d;
            Leap.Vector s_pos, n_pos;
            if(handlist != null)
            {
                s_pos = handlist[0].PalmPosition;
                InteractionBox box_pos = frame.InteractionBox;
                n_pos = box_pos.NormalizePoint(s_pos);
                x = n_pos.x * SharedProperties.SCREEN_WIDTH;
                y = (1d - n_pos.y) * SharedProperties.SCREEN_HEIGHT;
            }
            else
            {
                MessageBox.Show("Please put your hands above the leap motion controller!");
            }

            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                Canvas.SetLeft(myEllipse, x);
                Canvas.SetTop(myEllipse, y);
            }), null);
        }

        void gameOverAnimation()  //游戏结束后场景
        {
            Label gameOverLabel = new Label();
            gameOverLabel.FontSize = 50;
            gameOverLabel.FontFamily = new FontFamily("Comic Sans MS Bold");
            gameOverLabel.Content = "Game Over!";
            gameOverLabel.Foreground = Brushes.Red;
            Canvas.SetLeft(gameOverLabel, SharedProperties.SCREEN_WIDTH/2);
            Canvas.SetTop(gameOverLabel, 0);
            canvas.Children.Add(gameOverLabel);
            Thread time = new Thread(() =>
            {
                System.Threading.Thread.Sleep(3000);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(gameOverLabel);
                    gameStartButton.IsEnabled = true;
                }));
            });
            time.Start();

            myEllipse.Width = 20;
            myEllipse.Height = 20;
        }   
        void levelUpAnimation()
        {
            Label levelup = new Label();
            levelup.FontSize = 50;
            levelup.FontFamily = new FontFamily("Comic Sans MS Bold");
            levelup.Foreground = Brushes.White;
            //set position
            Canvas.SetLeft(levelup, SharedProperties.SCREEN_WIDTH/2);
            Canvas.SetTop(levelup, 0d);
            canvas.Children.Add(levelup);
            Thread time = new Thread(() =>
            {
                //System.Threading.Thread.Sleep(800);
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //    levelup.Content = "Level";
                //}));
                //System.Threading.Thread.Sleep(500);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    levelup.Content = "New Enemy Is Coming!";
                }));
                System.Threading.Thread.Sleep(1000);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(levelup);
                }));
            });
            time.Start();
            /////////////实现
            /////////////实现
        }
        void startGameAnimation() //游戏开始动画
        {
            Label xlabel = new Label();
            xlabel.FontSize = 70;
            xlabel.FontFamily = new FontFamily("Comic Sans MS Bold");
            xlabel.Foreground = Brushes.Green;
            double a = canvas.Width / 2;
            double b = canvas.Height / 2;
            Canvas.SetLeft(xlabel, SharedProperties.SCREEN_WIDTH/2);
            Canvas.SetTop(xlabel, 0d);
            this.Dispatcher.Invoke(new Action(() =>
            {
                canvas.Children.Add(xlabel);
            }));


            Thread t = new Thread(() =>
            {
                int N = SharedProperties.INIT;
                while (N >= 1)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        xlabel.Content = N.ToString();
                        N--;
                    }));
                    Thread.Sleep(1000);
                }
                if (N < 1)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {

                        xlabel.Content = "Start!";
                        Canvas.SetLeft(xlabel, SharedProperties.SCREEN_WIDTH/2);
                    }));
                    Thread.Sleep(1000);
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        canvas.Children.Remove(xlabel);
                        N = SharedProperties.INIT;
                    }));
                }
            });
            t.Start();
            ////////////////////实现
        }
 
        private void Button_Click_end(object sender, RoutedEventArgs e)
        {
            gameOverButton.IsEnabled = false;
            finishGame();
            Environment.Exit(0);// 退出程序
        }
        private void Button_Click_start(object sender, RoutedEventArgs e)
        {
            /* 判断是否连接leap motion */
            if (controller.IsConnected)
            {
                MessageBox.Show("Leap Motion Controller Is Connected!");
                startGame();
            }
            else
            {
                MessageBox.Show("Leap Motion is not Connected!");
            }
        }

        void interactCheck(object sender, EventArgs e)
        {
            /* 碰撞检查 定义r1 r2 两个矩形框，两矩形框分别于我方ellipse和敌方ellipse相切，判断两矩形是否有相交来检测碰撞 */
            Rect r1 = new Rect(Canvas.GetLeft(myEllipse), Canvas.GetTop(myEllipse), myEllipse.ActualWidth, myEllipse.ActualWidth);
            Rect enemy_pos;
            //check enemylist
            if (ellipselist.Count != 0)
            {
                Ellipse etest = new Ellipse();
                foreach (Ellipse enemy in ellipselist)
                {
                    enemy_pos = new Rect(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.ActualWidth, enemy.ActualHeight);
                    if (r1.IntersectsWith(enemy_pos))
                    {
                        health -= 20;
                        health_block.Text = health.ToString();

                        etest = enemy;
                        if (health <= 0)
                        {
                            //If your health equals zero, than you are DEAD! GAME OVER
                            finishGame();
                        }
                        score += SharedProperties.ENEMY_SCORE;
                    }
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(etest);
                    ellipselist.Remove(etest);
                }));
            }
            //check lenemylist
            if (lenemylist.Count != 0)
            {
                Ellipse etest = new Ellipse();
                foreach (Ellipse lenemy in lenemylist)
                {
                    enemy_pos = new Rect(Canvas.GetLeft(lenemy), Canvas.GetTop(lenemy), lenemy.ActualWidth, lenemy.ActualHeight);
                    if (r1.IntersectsWith(enemy_pos))
                    {
                        health -= 5;
                        health_block.Text = health.ToString();

                        etest = lenemy;
                        if (health <= 0)
                        {
                            //If your health equals zero, than you are DEAD! GAME OVER
                            finishGame();
                        }
                        score += SharedProperties.ENEMY_SCORE;
                    }
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(etest);
                    ellipselist.Remove(etest);
                    score += SharedProperties.ENEMY_SCORE;
                }));
            }
            //check cenemylist
            if (cenemylist.Count != 0)
            {
                Ellipse etest = new Ellipse();
                foreach (Ellipse cenemy in cenemylist)
                {
                    enemy_pos = new Rect(Canvas.GetLeft(cenemy), Canvas.GetTop(cenemy), cenemy.ActualWidth, cenemy.ActualHeight);
                    if (r1.IntersectsWith(enemy_pos))
                    {
                        health -= 5;
                        health_block.Text = health.ToString();

                        etest = cenemy;
                        if (health <= 0)
                        {
                            //If your health equals zero, than you are DEAD! GAME OVER
                            finishGame();
                        }
                        score += SharedProperties.ENEMY_SCORE;
                    }
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    canvas.Children.Remove(etest);
                    ellipselist.Remove(etest);
                    score += SharedProperties.ENEMY_SCORE;
                }));
            }

        }
        void getUserGesture(object sender,EventArgs e)
        {
            //Get the frame of the controller
            Leap.Frame frame = controller.Frame();

            //Test the number of not extended fingers
            int lastNotExtended = notExtended;
            notExtended = 0;
            foreach (Finger finger in frame.Fingers)
            {
                if (!finger.IsExtended)
                    notExtended++;
            }
            if (notExtended == 2 && lastNotExtended == 5 && score >= SharedProperties.SUPER_POWER_SCORE)
            {
                score = score - SharedProperties.SUPER_POWER_SCORE;
                superPower();
            }
            else 
            if (notExtended == 0 && lastNotExtended == 5 && score >= SharedProperties.CLEAR_SCORE)
            {
                score = score - SharedProperties.CLEAR_SCORE;
                clearAll();
            }

            //do something else
            moveLineEnemy();
        }
        void updateLevel(object sender,EventArgs e)
        {
            if (myEllipse.Width <= 40)
            {
                myEllipse.Width = myEllipse.Width * 1.2f;
                myEllipse.Height = myEllipse.Height * 1.2f;
            }
            switch (currentlevel)
            {
                case 1:
                    if (score >= SharedProperties.LEVEL_TWO)
                    {
                        levelUpAnimation();
                        currentlevel = 2;
                    }
                    break;
                case 2:
                    setTwo();
                    if (score >= SharedProperties.LEVEL_THREE)
                    {
                        levelUpAnimation();
                        currentlevel = 3;
                    }
                    break;
                case 3:
                    setThree();
                    if (score >= SharedProperties.LEVEL_FOUR)
                    {
                        levelUpAnimation();
                        currentlevel = 4;
                    }
                    break;
                case 4:
                    setFour();
                    if (score >= SharedProperties.LEVEL_FIVE)
                        levelUpAnimation();
                    currentlevel = 5;
                    break;
                case 5:
                    setDevil();
                    break;
                default:
                    break;
            }

            //setTwo();
            //setThree();
            //setFour();
            //setDevil();
        }
        void setTwo()
        {
            generateConnerEnemy();
        }
        void setThree()
        {
            generateLineEnemy();
        }
        void setFour()
        {
            generateCircleEnemy();
        }
        void setDevil()
        {
            generateConnerEnemy();
            generateLineEnemy();
            generateCircleEnemy();
        }
        void generateConnerEnemy()
        {
            for (int i = 0; i < 8; i++)
            {
                int j = i % 4;
                Ellipse e = new Ellipse();
                e.Width = 30;
                e.Height = 30;
                e.Fill = new SolidColorBrush(Colors.White);
                Random random = new Random();

                Pos epos = new Pos();

                switch (j)
                {
                    case 0:
                        //epos.x = random.Next(0, SharedProperties.SCREEN_WIDTH);
                        epos.x = 0d;
                        epos.y = 0d;
                        break;

                    case 1:
                        //epos.y = random.Next(0, SharedProperties.SCREEN_HEIGHT);
                        epos.x = 0d;
                        epos.y = SharedProperties.SCREEN_HEIGHT;
                        break;

                    case 2:
                        //epos.x = random.Next(0, SharedProperties.SCREEN_WIDTH);
                        epos.x = SharedProperties.SCREEN_WIDTH;
                        epos.y = SharedProperties.SCREEN_HEIGHT;
                        break;

                    case 3:
                        epos.x = SharedProperties.SCREEN_WIDTH;
                        epos.y = 0;
                        //epos.y = random.Next(0, SharedProperties.SCREEN_HEIGHT);
                        break;
                }

                //Update the ellipse wait list
                ewl.Add(e);
                ewlpos.Add(epos);

            }
            for (; ewl.Count != 0 && ewlpos.Count != 0;)
            {
                Ellipse ellipse = new Ellipse();
                ellipse = ewl[0];
                Pos ellipsepos = new Pos();
                ellipsepos = ewlpos[0];

                canvas.Children.Add(ellipse);
                ellipselist.Add(ellipse);

                ellipse.SetValue(Canvas.LeftProperty, ellipsepos.x);
                ellipse.SetValue(Canvas.TopProperty, ellipsepos.y);

                ewl.RemoveAt(0);
                ewlpos.RemoveAt(0);

                Thread t = new Thread(() =>
                {
                    System.Threading.Thread.Sleep(SharedProperties.ENEMY_LIFE_TIME);
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        canvas.Children.Remove(ellipse);
                        ellipselist.Remove(ellipse);
                    }));
                });
                t.Start();
            }
        }
        void generateLineEnemy()
        {
            for(int i = 1; i < 10;i++)
            {
                Ellipse line = new Ellipse();
                line.Width = 20;
                line.Height = 20;
                line.Fill = new SolidColorBrush(Colors.Red);

                double xpos = SharedProperties.SCREEN_WIDTH / 10 * i;
                double ypos = 0d;
                lenemylist.Add(line);
                canvas.Children.Add(line);
                line.SetValue(Canvas.LeftProperty,xpos);
                line.SetValue(Canvas.TopProperty, ypos);
            }
        }
        void moveLineEnemy()
        {
            foreach(Ellipse line in lenemylist)
            {
                line.SetValue(Canvas.LeftProperty, Canvas.GetLeft(line));
                line.SetValue(Canvas.TopProperty, Canvas.GetTop(line)+5d);

                if(Canvas.GetRight(line) <= line.ActualWidth)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        canvas.Children.Remove(line);
                        ellipselist.Remove(line);
                    }));
                }
            }
        }
        void generateCircleEnemy()
        {
            for(int i = 0;i < 8;i++)
            {
                Ellipse cenemy = new Ellipse();
                cenemy.Width = 15;
                cenemy.Height = 15;
                cenemy.Fill = new SolidColorBrush(Colors.Blue);

                double xpos = Math.Cos(2 * SharedProperties.PI / 8 * i) * SharedProperties.R + Canvas.GetLeft(myEllipse);
                double ypos = Math.Sin(2 * SharedProperties.PI / 8 * i) * SharedProperties.R + Canvas.GetTop(myEllipse);

                cenemylist.Add(cenemy);
                canvas.Children.Add(cenemy);
                cenemy.SetValue(Canvas.LeftProperty, xpos);
                cenemy.SetValue(Canvas.TopProperty, ypos);
            }
        }

        void clearAll()
        {
            foreach(Ellipse ellipse in ellipselist)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    canvas.Children.Remove(ellipse);
                    ellipselist.Remove(ellipse);
                    score += SharedProperties.ENEMY_SCORE;
                }));
            }
        }
        void superPower()
        {
            if (ellipselist.Count != 0)
            {
                foreach (Ellipse ellipse in ellipselist)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        canvas.Children.Remove(ellipse);
                        ellipselist.Remove(ellipse);
                        score += SharedProperties.ENEMY_SCORE;
                    }));
                }
            }
            if (lenemylist.Count != 0)
            {
                foreach (Ellipse lellipse in lenemylist)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        canvas.Children.Remove(lellipse);
                        lenemylist.Remove(lellipse);
                        score += SharedProperties.ENEMY_SCORE;
                    }));
                }
            }
            if (cenemylist.Count != 0)
            {
                foreach (Ellipse cellipse in cenemylist)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        canvas.Children.Remove(cellipse);
                        cenemylist.Remove(cellipse);
                        score += SharedProperties.ENEMY_SCORE;
                    }));
                }
            }


        }

        private void s(object sender, System.Windows.Input.KeyEventArgs e)
        {

            switch (e.Key)
            {
                case System.Windows.Input.Key.F2:
                    setTwo(); break;
                case System.Windows.Input.Key.F3:
                    setThree(); break;
                case System.Windows.Input.Key.F4:
                    setFour(); break;
                case System.Windows.Input.Key.F5:
                    setDevil(); break;
                case System.Windows.Input.Key.F1:
                    superPower();
                    break;
                default:
                    break;
            }
        }
    }
}
