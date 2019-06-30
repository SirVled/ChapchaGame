using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChapchaGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool trueClick = false; // Проверка на зажатую левую клавишу мыши
        private bool correctLineCancel = false; // Проверка на "запрещенный блок" 
        private bool startClick = true; // Первый клик

        private const int maxCapch = 10; // Максимальное кол-во капч

        private DispatcherTimer time; // Время

        private Ellipse rememberlockEl { get; set; } // Запоминаем последний круг

        private Ellipse[] startEl = new Ellipse[2]; // Стартовые круги

        //Запреты на создание некоторых линий
        private List<Ellipse> Angle = new List<Ellipse>(); // Углы
        private List<Ellipse> LRGran = new List<Ellipse>(); // Левый-Правый
        private List<Ellipse> TBGran = new List<Ellipse>(); // Вверхний-Нижний

        private List<Ellipse> capchaEl = new List<Ellipse>(); //Массив кругов капчи

        private List<Line> lineFullCapcha = new List<Line>(); // Все линии которые находятся в капче
        private List<Line> lineOfCapcha = new List<Line>(); // Все линии которые находятся в капче(Без повторок)

        private List<Line> playerLine = new List<Line>(); // Все линии которые создал игрок
        private List<Line> playerFullGrid = new List<Line>(); // Все линии которые создал игрок(Без повторок)

        private int quantityTrueCapcha = 0; // Кол-во правильных капчей
        private Random rnd = new Random(); 

        private Line line; // Линия котороя соединяет ключи

        /// <summary>
        /// Загрузка окна
        /// </summary>
        /// <param name="sender">Window</param>
        /// <param name="e">Loaded</param>
        private void Start(object sender, RoutedEventArgs e)
        {
            Angle.Add(Angle1);
            Angle.Add(Angle2);
            Angle.Add(Angle3);
            Angle.Add(Angle4);

            TBGran.Add(TopGran);
            TBGran.Add(BottGran);
            LRGran.Add(LeftGran);
            LRGran.Add(RightGran);

            int temp = 1;
            Time();
            for(int i = 0; i < 3; i++)
            {
                for(int j = 1; j <= 3; j++)
                {
                    Rectangle rec = new Rectangle
                    {                       
                        Stroke = Brushes.Black,
                        Width = 50,
                        Height = 50,
                        Margin = new Thickness(10 + (55 * j - 1), 10 + (55 * i),0,0),

                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    Ellipse el = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(rec.Margin.Left + (rec.Width / 2) - (30 / 2), rec.Margin.Top + (rec.Height / 2) - (30 / 2), 0, 0),
                        Tag = temp,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    Capcha.Children.Add(rec);
                    Capcha.Children.Add(el);

                    capchaEl.Add(el);
                    temp++;
                }
            }

            RandomCapcha();
        }

        /// <summary>
        /// Отображение времени
        /// </summary>
        private void Time()
        {
            int second = 0;
            int minute = 0;

            time = new DispatcherTimer(DispatcherPriority.Normal);
            time.Interval = TimeSpan.FromSeconds(1);



            time.Tick += (s, e) =>
            {
                second++;

          
                if (second >= 60)
                {
                    minute++;                
                    second = 0;
                }

                Times.Content = "Время : " + ((minute < 10) ? "0" + minute : minute.ToString()) + ":" +
                    ((second < 10) ? "0" + second : second.ToString());
            };

            time.Start();
        }


        /// <summary>
        /// Создание случайной капчи
        /// </summary>
        private void RandomCapcha()
        {
            int quantityLine = rnd.Next(5, 11);
            int nLine = -1;

            Quntity.Content = "Успешных капч : " + quantityTrueCapcha;
            quantityTrueCapcha++;

            if (quantityTrueCapcha > maxCapch)
            {

                WinGame();
            }
            for(int i = 0; i <= quantityLine; i++)
            {
                int n = rnd.Next(1,10);
               
                if (n != nLine)
                {                 
                    if (RandomDontStopCreateLine(n, nLine))
                    {
                        if (nLine != -1)
                        {
                            Line ln = CreateLine(capchaEl[nLine - 1], 3);

                            ln.X1 = capchaEl[n - 1].Margin.Left + capchaEl[n - 1].Width / 2;
                            ln.Y1 = capchaEl[n - 1].Margin.Top + capchaEl[n - 1].Height / 2;

                            ln.X2 = capchaEl[nLine - 1].Margin.Left + capchaEl[nLine - 1].Width / 2;
                            ln.Y2 = capchaEl[nLine - 1].Margin.Top + capchaEl[nLine - 1].Height / 2;
                            Capcha.Children.Add(ln);

                            ln.Tag = nLine + "," + n;

                            lineFullCapcha.Add(ln);
                            CheckRepeatLine(ln, ref lineOfCapcha);
                        }
                        else
                        {
                            startEl[1] = capchaEl[n - 1];
                            capchaEl[n - 1].Stroke = Brushes.Red;
                        }
                        nLine = n;
                    }
                    else
                        --i;
                }
                else
                    --i;
            }
        }

       
        /// <summary>
        /// Сравнивает, есть ли такая линия в списке
        /// </summary>
        /// <param name="ln">Линия</param>
        private void CheckRepeatLine(Line ln, ref List<Line> line)
        {
            bool tr = true;
            foreach (var lin in line)
            {
                string[] ss1 = ln.Tag.ToString().Split(',');
                string[] ss2 = lin.Tag.ToString().Split(',');

                if (ss1[0].Equals(ss2[1]) && ss1[1].Equals(ss2[0]) || ss1[0].Equals(ss2[0]) && ss1[1].Equals(ss2[1]))
                    tr = false;
            }

            if(tr)
                line.Add(ln);
        }

        /// <summary>
        /// Ограничение в рандоме
        /// </summary>
        /// <param name="n">Конечные координаты линии</param>
        /// <param name="nLine">Первые координаты линии</param>
        /// <returns>Состояние ограничения</returns>
        private bool RandomDontStopCreateLine(int n, int nLine)
        {
            switch(n)
            {
                case 1 :
                    if (nLine == 3 || nLine == 7 || nLine == 9)
                        return false;
                    break;
                case 2:
                    if (nLine == 8)
                        return false;
                    break;
                case 3:
                    if (nLine == 1 || nLine == 7 || nLine == 9)
                        return false;
                    break;
                case 4:
                    if (nLine == 6)
                        return false;
                    break;
                case 6:
                    if (nLine == 4)
                        return false;
                    break;
                case 7:
                    if (nLine == 1 || nLine == 3 || nLine == 9)
                        return false;
                    break;
                case 8:
                    if (nLine == 2)
                        return false;
                    break;
                case 9:
                    if (nLine == 1 || nLine == 3 || nLine == 7)
                        return false;
                    break;
            }

            return true;
        }

       
        /// <summary>
        /// Нажатие на круг
        /// </summary>
        /// <param name="sender">Ellipse</param>
        /// <param name="e">MouseLeftDownButton</param>
        private void ClickEl(object sender, MouseButtonEventArgs e)
        {
            trueClick = true;
            correctLineCancel = true;
            if (startClick)
            {
                startEl[0] = (sender as Ellipse);
                (sender as Ellipse).Stroke = Brushes.Red;
                startClick = false;
            }

            rememberlockEl = (sender as Ellipse);
            CreateNewLine(sender);
        }

        /// <summary>
        /// Если юзер отпустил левую клавишу мыши, удалить последнию созданую линию
        /// </summary>
        /// <param name="sender">Окно</param>
        /// <param name="e">MouseLeftUpButton</param>
        private void CancelEl(object sender, MouseButtonEventArgs e)
        {
            trueClick = false;
            if (correctLineCancel)
            {
                TouchGrid.Children.Remove(line);
                correctLineCancel = false;
                CheckRobotPlayerOrNo();
            }
        }

        /// <summary>
        /// Перемещение мыши по форме
        /// </summary>
        /// <param name="sender">Окно</param>
        /// <param name="e">MouseMove</param>
        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if(trueClick && e.LeftButton.ToString().Equals("Pressed"))
            {
                Point point = e.GetPosition(this);
                line.X1 = point.X - TouchGrid.Margin.Left;
                line.Y1 = point.Y - TouchGrid.Margin.Top;
            }
            
            if(correctLineCancel && !e.LeftButton.ToString().Equals("Pressed"))
            {
                TouchGrid.Children.Remove(line);
                correctLineCancel = false;

                CheckRobotPlayerOrNo();
            }
        }

        /// <summary>
        /// Проверка капчи
        /// </summary>
        private void CheckRobotPlayerOrNo()
        {
            if (startEl[0].Tag.ToString().Equals(startEl[1].Tag.ToString()))
            {
                bool trueClear = true;
                int tempLintCount = 0;
                if (lineOfCapcha.Count == playerLine.Count)
                {
                    for (int i = 0; i < playerLine.Count; i++)
                    {
                        foreach (var ln in lineOfCapcha)
                        {
                            string[] ss1 = playerLine[i].Tag.ToString().Split(',');
                            string[] ss2 = ln.Tag.ToString().Split(',');

                            if (ss1[0].Equals(ss2[1]) && ss1[1].Equals(ss2[0]) || ss1[0].Equals(ss2[0]) && ss1[1].Equals(ss2[1]))
                            {
                                tempLintCount++;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    trueClear = false;
                }
                if (playerLine.Count != tempLintCount)
                {
                    trueClear = false;
                }

                if (trueClear)
                {
                    startEl[1].Stroke = Brushes.Black;
                    ClearBort(ref lineFullCapcha, ref lineOfCapcha, Capcha);
                    RandomCapcha();
                }

                startEl[0].Stroke = Brushes.Black;
                startClick = true;
                ClearBort(ref playerFullGrid, ref playerLine, TouchGrid);           
            }
            else
            {
                startEl[0].Stroke = Brushes.Black;
                startClick = true;
                ClearBort(ref playerFullGrid, ref playerLine, TouchGrid);
            }
        }

        /// <summary>
        /// Очистка панели от линий
        /// </summary>
        /// <param name="lineOfGrid">Массив линий которые нужно очистить</param>
        /// <param name="line">Массив линий которые нужно очистить</param>
        /// <param name="grid">Grid в котором лежат линии</param>
        private void ClearBort(ref List<Line> lineOfGrid, ref List<Line> line, Grid grid)
        {
            foreach (var ln in lineOfGrid)
            {
                grid.Children.Remove(ln);
            }

            lineOfGrid.Clear();
            line.Clear();
        }
        
        /// <summary>
        /// Создание новой линии
        /// </summary>
        /// <param name="sen">Ellipse в котором будет создаваться линия</param>
        /// <param name="strokeThickness">Размер линии</param>
        /// <returns>Линия</returns>
        private Line CreateLine(object sen, int strokeThickness)
        {
            Ellipse el = (sen as Ellipse);
            double x = el.Margin.Left + el.ActualWidth / 2 ;
            double y = el.Margin.Top + el.ActualHeight / 2;

            Line ln = new Line
            {
                Fill = Brushes.Black,
                Stroke = Brushes.Black,
                StrokeThickness = strokeThickness,
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y,
                Tag = el.Tag + ","
            };

            return ln;
        }

        /// <summary>
        /// Если пользователь наводится на круг и он находится "В режиме создания капчи" то создается новая линия
        /// </summary>
        /// <param name="sender">Ellipse</param>
        /// <param name="e">MouseEnter</param>
        private void AcceptEl(object sender, MouseEventArgs e)
        {
            if(trueClick && !rememberlockEl.Equals((sender as Ellipse)))
            {
                bool angle = CheckToTrueLine(sender);

                if (angle)
                {
                    rememberlockEl = (sender as Ellipse);

                    Ellipse el = (sender as Ellipse);
                    double x = el.Margin.Left + el.ActualWidth / 2;
                    double y = el.Margin.Top + el.ActualHeight / 2;

                    line.X1 = x;
                    line.Y1 = y;

                    line.Tag += el.Tag.ToString();

                    playerFullGrid.Add(line);
                    CheckRepeatLine(line, ref playerLine);
                    CreateNewLine(sender);
                }
            }
        }

        /// <summary>
        /// Проверка на то, можно поставить линию в выбранном блоке
        /// </summary>
        /// <param name="sender">Блок на который навелись</param>
        /// <returns>Состояние проверки</returns>
        private bool CheckToTrueLine(object sender)
        {
            foreach (var el in Angle)
            {
                if (el.Equals(rememberlockEl))
                {
                    for (int i = 0; i < Angle.Count; i++)
                    {
                        if (Angle[i].Equals((sender as Ellipse)))
                        {
                            return false;
                        }
                    }
                    break;
                }
            }

            foreach (var el in TBGran)
            {
                if (el.Equals(rememberlockEl))
                {
                    for (int i = 0; i < TBGran.Count; i++)
                    {
                        if (TBGran[i].Equals((sender as Ellipse)))
                        {
                            return false;
                        }
                    }
                }
            }

            foreach (var el in LRGran)
            {
                if (el.Equals(rememberlockEl))
                {
                    for (int i = 0; i < LRGran.Count; i++)
                    {
                        if (LRGran[i].Equals((sender as Ellipse)))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Создание новой линии
        /// </summary>
        /// <param name="sender">Линия</param>
        private void CreateNewLine(object sender)
        {
            line = CreateLine(sender,5);
            TouchGrid.Children.Add(line);           
        }


        /// <summary>
        /// Победа!
        /// </summary>
        private void WinGame()
        {
            time.Stop();
            MessageBox.Show(Quntity.Content + "   " + Times.Content);
            Close();
        }

    }
}
