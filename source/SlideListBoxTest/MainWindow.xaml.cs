namespace SlideListBoxTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            var list = new List<SampleItem>()
            {
                new SampleItem("Resources/SuperMushroom.ico", "スーパーなキノコ"),
                new SampleItem("Resources/FireFlower.ico", "燃える花"),
                new SampleItem("Resources/IceFlower.ico", "凍える花"),
                new SampleItem("Resources/Star.ico", "ピカピカ"),
                new SampleItem("Resources/1upMushroom.ico", "一人増えるキノコ"),
                new SampleItem("Resources/Lakitu.ico", "ジュゲム"),
            };

            this.listbox.ItemsSource = list;
            this.itemscontrol.ItemsSource = Enumerable.Range(0, list.Count);
        }

        private void LeftRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.listbox.SelectedIndex = this.listbox.SelectedIndex > 0 ? this.listbox.SelectedIndex - 1 : this.listbox.Items.Count - 1;
        }

        private void RightRepeatButton_Click(object sender, RoutedEventArgs e)
        {
            this.listbox.SelectedIndex = this.listbox.SelectedIndex < this.listbox.Items.Count - 1 ? this.listbox.SelectedIndex + 1 : 0;
        }

        private DelegateCommand<int> _changeIndexCommand;
        public DelegateCommand<int> ChangeIndexCommand
        {
            get
            {
                return this._changeIndexCommand ?? (this._changeIndexCommand = new DelegateCommand<int>(
                p =>
                {
                    this.listbox.SelectedIndex = p;
                }));
            }
        }
    }

    internal class SampleItem
    {
        public SampleItem(string path, string name)
        {
            this.Path = path;
            this.Name = name;
        }

        public string Path { get; private set; }
        public string Name { get; private set; }
    }

    public class DelegateCommand<T> : ICommand
    {
        private Action<T> _execute;

        public DelegateCommand(Action<T> execute)
        {
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            if (this._execute != null) this._execute((T)parameter);
        }
    }
}
