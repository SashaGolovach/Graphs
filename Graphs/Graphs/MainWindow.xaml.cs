using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Graphs
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public class Vertix
    {
        public string name;
        public int id;
        public Point pos;
        public Vertix(string _name, int _id, Point _pos)
        {
            name = _name;
            id = _id;
            pos = new Point(_pos.X - 25, _pos.Y - 25);
        }
    }

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public partial class MainWindow : Window
    {
        System.Windows.Forms.Timer bfs = new System.Windows.Forms.Timer() { Interval = 500 };
        System.Windows.Forms.Timer dfs = new System.Windows.Forms.Timer() { Interval = 400 };
        List<Vertix> graph = new List<Vertix>();
        int count = 0, id_checked = -1, colored = -1;
        int[,]  g = new int[100, 100];
        bool[] used = new bool[100];

        public MainWindow()
        {
            InitializeComponent();
            bfs.Tick += Bfs_Tick;
            dfs.Tick += Dfs_Tick;
        }

        int v, times = 0;
        int[] ans = new int[100];

        void Dfs()
        {
            if (!started() && id_checked != -1)
            {
                ans[id_checked] = -1;
                this.v = id_checked;
                dfs.Start();
                ++times;
                return;
            }
            foreach (var v in graph)
                if (!used[v.id])
                {
                    ans[v.id] = -1;
                    this.v = v.id;
                    dfs.Start();
                    ++times;
                    return;
                }
            colored = -1;
            this.v = -1;
            used = new bool[100];
            Display();
            MessageBox.Show(times > 1 ? "Не зв'язний" : "Зв'язний");
        }

        private void Dfs_Tick(object sender, EventArgs e)
        {
            used[v] = true;
            colored = v;
            Display();
            for (int i = 0; i <= count; i++)
                if (g[i, v] == 1 && !used[i])
                {
                    ans[i] = v;
                    v = i;
                    return;
                }
            if (ans[v] == -1)
            {
                dfs.Stop();
                Dfs();
            }
            else
                v = ans[v];
            Display();
            return;
        }

        void Bfs()
        {
            if (!started() && id_checked != -1)
            {
                q.Enqueue(id_checked);
                bfs.Start();
                ++times;
                return;
            }
            foreach (var v in graph)
                if (!used[v.id])
                {
                    q.Enqueue(v.id);
                    bfs.Start();
                    ++times;
                    return;
                }
            colored = -1;
            this.v = -1;
            used = new bool[100];
            Display();
            MessageBox.Show(times > 1 ? "Не зв'язний" : "Зв'язний");
        }

        Queue<int> q = new Queue<int>();

        private void Bfs_Tick(object sender, EventArgs e)
        {
            if (q.Count != 0)
            {
                int cur = q.Dequeue();
                while (used[cur])
                     cur = q.Dequeue();
                used[cur] = true;
                colored = cur;
                for (int i = 0; i <= count; i++)
                    if (g[i, cur] == 1 && !used[i])
                    {
                        q.Enqueue(i);
                    }
                Display();
            }
            else
            {
                bfs.Stop();
                Bfs();
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (action_list.SelectedIndex == 0 && GetIdByPos(e.GetPosition(canvas)) == -1)
            {
                ++count;
                int new_id = count;
                for (int i = 0; i <= count; i++)
                    if (GetIndexById(i) == -1)
                    {
                        new_id = i;
                        break;
                    }
                graph.Add(new Vertix(new_id.ToString(), new_id, e.GetPosition(canvas)));
                //Rename();
            }
            else if (action_list.SelectedIndex == 1 && GetIdByPos(e.GetPosition(canvas)) != -1)
            {
                int id = GetIdByPos(e.GetPosition(canvas));
                for (int i = 0; i <= count; i++)
                    g[id, i] = g[i, id] = 0;
                graph.RemoveAt(GetIndexById(id));
                //Rename();
            }
            else if (action_list.SelectedIndex == 2 && GetIdByPos(e.GetPosition(canvas)) != -1)
            {
                if (id_checked != -1)
                {
                    int temp = GetIdByPos(e.GetPosition(canvas));
                    if (temp != id_checked)
                    {
                        g[id_checked, temp] = g[temp, id_checked] = 1;
                        id_checked = -1;
                    }
                }
                else
                {
                    id_checked = GetIdByPos(e.GetPosition(canvas));
                }
            }
            else id_checked = -1;
            Rename();
            Display();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ans = new int[100];
            used = new bool[100];
            q = new Queue<int>();
            times = 0;
            if (algo.SelectedIndex == 1)
                Dfs();
            else
                Bfs();
        }

        private int GetIdByPos(Point p)
        {
            p = new Point(p.X - 25, p.Y - 25);
            foreach(var v in graph)
                if (Math.Sqrt(Math.Pow(p.X - v.pos.X, 2) + Math.Pow(p.Y - v.pos.Y, 2)) < 50)
                    return v.id;
            return -1;
        }

        private int GetIndexById(int id)
        {
            foreach (var v in graph)
                if (v.id == id)
                    return graph.IndexOf(v);
            return -1;
        }

        private void algo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                start.IsEnabled = !(algo.SelectedIndex == -1);
            }
            catch { }
        }

        private void Display()
        {
            canvas.Children.Clear();
            foreach(var v in graph)
            {
                var b = v.id == id_checked ? 2 : 1;
                var c = v.id == colored ? Brushes.Yellow : Brushes.White;
                if (used[v.id] && v.id != colored)
                    c = Brushes.Gray;
                var el = new Ellipse() {
                    Width = 50,
                    Height = 50,
                    StrokeThickness = b,
                    Stroke = Brushes.Black,
                    Fill = c,
                    Margin = new Thickness(v.pos.X, v.pos.Y, 0, 0)};
                Panel.SetZIndex(el, 2);
                var t = new TextBlock()
                {
                    Text = v.name,
                    Margin = new Thickness(v.pos.X + 18, v.pos.Y + 18, 0, 0),
                    TextAlignment = TextAlignment.Center,
                };
                Panel.SetZIndex(t, 3);
                canvas.Children.Add(el);
                canvas.Children.Add(t);
            }
            for(int i = 0; i <= count; i++)
                for(int j = 0; j <= count; j++)
                    if(g[i,j] == 1)
                    {
                        int v = GetIndexById(i), to = GetIndexById(j);
                        Line line = new Line() {
                            X1 = graph[v].pos.X + 25,
                            X2 = graph[to].pos.X + 25,
                            Y1 = graph[v].pos.Y + 25,
                            Y2 = graph[to].pos.Y + 25,
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.5,
                        };
                        canvas.Children.Add(line);
                    }
        }

        private bool started()
        {
            foreach (var v in graph)
                if (used[v.id])
                {
                    return true;
                }
            return false;
        }

        private bool ExistName(string name)
        {
            foreach (var v in graph)
                if (v.name == name)
                    return true;
            return false;
        }

        private void Rename()
        {
            // get new names
            for (int i = 0; i < graph.Count; i++)
            {
                string new_name = graph[i].name;
                for (int j = 0; j < graph.Count; j++)
                    if (!ExistName(j.ToString()))
                    {
                        new_name = j.ToString();
                        break;
                    }
                graph[i].name = new_name;
            }
        }
    }
}
