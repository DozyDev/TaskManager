using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace TaskManager_Server {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    int port;
    IPAddress ip;
    TcpListener listener;
    Thread listenerThread;

    TaskManagerEntities db = new TaskManagerEntities();

    public MainWindow() {
      InitializeComponent();
    }

    private void TurnOnButton_Click(object sender, RoutedEventArgs e) {
      try {
        ip = IPAddress.Parse(IpField.Text);
        port = int.Parse(PortField.Text);
        listener = new TcpListener(ip, port);
        listener.Start();

        listenerThread = new Thread(new ThreadStart(ListenMethod));
        listenerThread.IsBackground = true;
        listenerThread.Start();

        TurnOnButton.IsEnabled = false;
        TurnOffButton.IsEnabled = true;
        JournalBox.Text += $"{DateTime.Now.ToString()} -> Сервер успешно включен\r\n";
      } catch (Exception err) {
        MessageBox.Show($"Ошибка запуска сервера -> {err.Message}");
      }
    }

    private string Auth(string credentials) {
      string login = credentials.Split('/')[0];
      string passw = credentials.Split('/')[1];
      var res = db.Users.Where(u => u.Login == login && u.Passw == passw).FirstOrDefault();
      if (res == null)
        return "failed";
      else {
        int uid = res.Id;
        return $"success/{uid}";
      }
    }

    private void DisplayMessage(string message) {
      JournalBox.Dispatcher.Invoke(new Action(() => {
        JournalBox.Text += $"{DateTime.Now.ToString()} -> {message}\r\n";
      }));
    }

    private void AddTask(string data) {
      string[] segments = data.Split('/');
      Tasks task = new Tasks() {
        Title = segments[0],
        About = segments[1],
        Start = Convert.ToDateTime(segments[2]),
        Finish = Convert.ToDateTime(segments[3]),
        StatusId = Convert.ToInt32(segments[4]),
        UserId = Convert.ToInt32(segments[5])
      };
      db.Tasks.Add(task);
      db.SaveChanges();
    }
    private void DelTask(string data) {
      string[] parts = data.Split('/');
      string str = parts[0];
      int num = Convert.ToInt32(parts[1]);
      var res = db.Tasks.Where(t => t.Title == str && t.UserId == num).ToList();
      db.Tasks.Remove(res.First());
      db.SaveChanges();
    }

    private List<Tasks> GetTasks(int uid) {
      var res = db.Tasks.Where(t => t.UserId == uid).ToList();
      return res;
    }

    private void ListenMethod() {
      try {
        while (true) {
          TcpClient acceptor = listener.AcceptTcpClient();
          NetworkStream ns = acceptor.GetStream();
          StreamReader sr = new StreamReader(ns);
          StreamWriter sw = new StreamWriter(ns);

          string mess = sr.ReadLine();
          DisplayMessage(mess);

          // Менеджер операций:
          string[] parts = mess.Split(':');
          string mode = parts[0];
          if (mode == "auth") {
            string authResult = Auth(parts[1]);
            DisplayMessage(authResult);
            sw.WriteLine(authResult);
            sw.Flush();     // !!!
          } else if (mode == "add") {
            AddTask(parts[1]);
          } else if (mode == "get") {
            string tasks_str = "";
            int uid = int.Parse(parts[1]);
            foreach (Tasks t in GetTasks(uid)) {
              tasks_str += t.Id + "$";
              tasks_str += t.Title + "$";
              tasks_str += t.About + "$";
              tasks_str += t.Start + "$";
              tasks_str += t.Finish + "$";
              tasks_str += t.StatusId + "$";
              tasks_str += t.UserId + "#";
            }
            sw.WriteLine(tasks_str);
            sw.Flush();     // !!!
          } else if (mode == "del") {// delete
            DelTask(parts[1]);
          }
          sw.Close();
          sr.Close();
          ns.Close();
          acceptor.Close();
        }
      } catch (Exception err) {
        MessageBox.Show($"Ошибка цикла обмена сообщениями -> {err.Message}");
      }
    }

    private void TurnOffButton_Click(object sender, RoutedEventArgs e) {
      listenerThread.Suspend();
      listener.Stop();

      TurnOnButton.IsEnabled = true;
      TurnOffButton.IsEnabled = false;
      JournalBox.Text += $"{DateTime.Now.ToString()} -> Сервер успешно выключен\r\n";
    }
  }
}

