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
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace TaskManager {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    int uid;
    int port;
    IPAddress ip;
    TcpClient client;
    List<Tasks> tasks;

    public MainWindow() {
      InitializeComponent();
      tasks = new List<Tasks>();
    }

    private void ConvertTasksList(string list_str) {
      string[] segments = list_str.Split('#');
      tasks.Clear();
      foreach (string segment in segments) {
        if (segment != String.Empty) {
          string[] fragments = segment.Split('$');
          Tasks task = new Tasks() {
            Id = int.Parse(fragments[0]),
            Title = fragments[1],
            About = fragments[2],
            Start = Convert.ToDateTime(fragments[3]),
            Finish = Convert.ToDateTime(fragments[4]),
            StatusId = int.Parse(fragments[5]),
            UserId = int.Parse(fragments[6])
          };
          tasks.Add(task);
        }
      }
    }

    private void GetTasksList() {
      try {
        TasksList.Items.Clear();
        ip = IPAddress.Parse(IpField.Text);
        port = int.Parse(PortField.Text);
        client = new TcpClient();
        client.Connect(ip, port);

        NetworkStream ns = client.GetStream();
        StreamWriter sw = new StreamWriter(ns);
        string mess = $"get:{uid}";
        sw.WriteLine(mess);
        sw.Flush();    // !!!

        StreamReader sr = new StreamReader(ns);
        string result = sr.ReadLine();
        ConvertTasksList(result);   // TasksList - Fill
        TasksList.Items.Clear();
        foreach (Tasks t in tasks) {
          TasksList.Items.Add(t.Title);
        }

        sr.Close();
        sw.Close();
        ns.Close();
        client.Close();
      } catch (Exception err) {
        MessageBox.Show($"Ошибка получения списка задач -> {err.Message}");
      }
    }

    private void ConnectButton_Click(object sender, RoutedEventArgs e) {
      try {
        ip = IPAddress.Parse(IpField.Text);
        port = int.Parse(PortField.Text);
        client = new TcpClient();
        client.Connect(ip, port);

        NetworkStream ns = client.GetStream();
        StreamWriter sw = new StreamWriter(ns);
        string mess = $"auth:{LoginField.Text}/{PasswordField.Password}";
        sw.WriteLine(mess);
        sw.Flush();    // !!!

        MessageBox.Show("Сообщение успешно отправлено");
        StreamReader sr = new StreamReader(ns);
        string result = sr.ReadLine();
        if (result.Contains("success")) {
          uid = int.Parse(result.Split('/')[1]);
          MessageBox.Show($"Вы успешно авторизованы -> {uid}");
          ConnectButton.IsEnabled = false;
          DisconnectButton.IsEnabled = true;
          AddButton.IsEnabled = true;
          DelButton.IsEnabled = true;
          SaveButton.IsEnabled = true;
          ClearButton.IsEnabled = true;
          LoginField.IsReadOnly = true;
          PasswordField.Clear();
          GetTasksList(); // !
        } else
          MessageBox.Show($"Авторизация провалена");

        sr.Close();
        sw.Close();
        ns.Close();
        client.Close();
      } catch (Exception err) {
        MessageBox.Show($"Ошибка соединения с сервером -> {err.Message}");
      }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e) {
      try {
        ip = IPAddress.Parse(IpField.Text);
        port = int.Parse(PortField.Text);
        client = new TcpClient();
        client.Connect(ip, port);

        NetworkStream ns = client.GetStream();
        StreamWriter sw = new StreamWriter(ns);
        //
        string mess =
            $"add:{TitleField.Text}/{AboutField.Text}/{StartTask.Text}/{FinishTask.Text}/{uid}/{StatusList.SelectedIndex + 1}";
        sw.WriteLine(mess);
        sw.Flush();    // !!!
        MessageBox.Show("Задача будет успешно добавлена в базу данных!");
        GetTasksList();

        sw.Close();
        ns.Close();
        client.Close();
      } catch (Exception err) {
        MessageBox.Show($"Ошибка соединения с сервером -> {err.Message}");
      }
    }

    private void DelButton_Click(object sender, RoutedEventArgs e) {
      try {
        ip = IPAddress.Parse(IpField.Text);
        port = int.Parse(PortField.Text);
        client = new TcpClient();
        client.Connect(ip, port);

        NetworkStream ns = client.GetStream();
        StreamWriter sw = new StreamWriter(ns);
        //
        string mess =
            $"del:{TasksList.SelectedItem.ToString()}/{uid}";
        sw.WriteLine(mess);
        sw.Flush();    // !!!
        MessageBox.Show("Задача будет успешно удалена из базы данных!");
        GetTasksList();

        sw.Close();
        ns.Close();
        client.Close();
      } catch (Exception err) {
        MessageBox.Show($"Ошибка соединения с сервером -> {err.Message}");
      }
    }
  }
}
