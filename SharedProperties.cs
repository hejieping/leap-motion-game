using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class SharedProperties
    {
        public static int SCREEN_WIDTH = 990;
        public static int SCREEN_HEIGHT = 570;

        public static double MYELLIPSEPERCENT = 0.1;    //敌人移动的距离关于 我方坐标的数据 的比例
        public static double ENEMYELLIPSEPERCENT = 0.9;  //敌人移动的距离关于 敌方坐标的数据的比例

        public static int MOVETIME = 5;  //敌人移动一定距离的时间比例

        public static int LEVELUPTIME = 20;
        public static int INIT = 3;
        public static double STARTTIMELABELX = 2.2;            //开始动画倒计时空间
        public static double ANIMATIONHEIGHTPERCENT = 3.5;    //动画控件高度相对于canvas高度的比例
        public static double STARTLABELX = 3.5;

        //Level up Score
        public static int LEVEL_TWO = 50;//50
        public static int LEVEL_THREE = 100;//200
        public static int LEVEL_FOUR = 200;//300
        public static int LEVEL_FIVE = 500;//500
        public static int R = 300;

        //Level up Time
        public static int LEVEL_NEW_TIME = 5;//new enemy appear, seconds
        public static int ENEMY_LIFE_TIME = 10000; //enemy lift time, million seconds

        //Score
        public static int CLEAR_SCORE = 50;
        public static int SUPER_POWER_SCORE = 200;
        public static int ENEMY_SCORE = 2;

        public static double PI = 3.1415926;
    }
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public struct Pos
    {
        public double x;
        public double y;
        Pos(double _x = 0d,double _y =0d)
        {
            x = _x;
            y = _y;
        }
    }
}
