using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Inventory.WPF;

public partial class MainWindow : Window
{

    private readonly HttpClient _httpClient = new HttpClient();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var loginPayload = new
        {
            email = EmailTextBox.Text,
            password = PasswordBox.Password
        };

        var json = JsonConvert.SerializeObject(loginPayload);
        var content = new StringContent(json, Encoding.UTF8,"application/json");


        try
        {
            var response = await _httpClient.PostAsync("http://localhost:5243/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic userData = JsonConvert.DeserializeObject(result);
                string token = userData.token;

                MessageBox.Show("Login successful!\nToken: " + token);
                
            }
            else
            {
                StatusTextBlock.Text = "Login failed: " + response.ReasonPhrase;
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Error: " + ex.Message;
        }
    }
}