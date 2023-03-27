using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
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
using ModelsLib;
using Client.Commands;
using System.Net.Http.Json;

namespace Client;


public partial class MainWindow : Window
{
    private HttpClient client;

    public int? Value
    {
        get { return (int?)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int?), typeof(MainWindow));

    public string Key 
    {
        get { return (string)GetValue(KeyProperty); }
        set { SetValue(KeyProperty, value); }
    }

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register("Key", typeof(string), typeof(MainWindow));


    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        client = new();
        cmbCommand.ItemsSource = Enum.GetValues(typeof(HttpMethods)).Cast<HttpMethods>();
        cmbCommand.SelectedIndex = 0;
    }

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
    {

        switch ((HttpMethods)cmbCommand.SelectedItem)
        {
            case HttpMethods.GET:
                var responseMessage = await client.GetAsync(@$"http://localhost:27001/?key={Key}");
                if (responseMessage.IsSuccessStatusCode)
                {
                    var content = await responseMessage.Content.ReadFromJsonAsync<KeyValue>();
                    Value = content.Value;
                }
                else
                    MessageBox.Show(responseMessage.StatusCode.ToString());
                break;
            case HttpMethods.POST:
                var keyValue = new KeyValue()
                {
                    Key = Key.ToCharArray()[0],
                    Value = Value.Value
                };
                var jsonPost = JsonSerializer.Serialize(keyValue);

                var dataPost = new StringContent(jsonPost);

                var response = await client.PostAsync("http://localhost:27001/", dataPost);

                if (response.StatusCode == HttpStatusCode.OK)
                    MessageBox.Show("Posted Successfully");
                else
                    MessageBox.Show("Already Exists");
                break;
            case HttpMethods.PUT:
                var keyValue2 = new KeyValue()
                {
                    Key = Key.ToCharArray()[0],
                    Value = Value.Value
                };

                var jsonPut = JsonSerializer.Serialize(keyValue2);

                var dataPut = new StringContent(jsonPut);

                var responsePut = await client.PutAsync("http://localhost:27001/", dataPut);

                if (responsePut.StatusCode == HttpStatusCode.OK)
                    MessageBox.Show("Putted Successfully");
                else
                    MessageBox.Show("Key doesn't exist");
                break;
            case HttpMethods.DELETE:
                var responseDelete = await client.DeleteAsync(@$"http://localhost:27001/?key={Key}");

                if (responseDelete.StatusCode == HttpStatusCode.OK)
                    MessageBox.Show("Deleted Successfully");
                else
                    MessageBox.Show("Key doesn't exist");
                break;
            default:
                break;
        }

    }

    private void cmbCommand_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch ((HttpMethods)cmbCommand.SelectedItem)
        {
            case HttpMethods.GET:
            case HttpMethods.DELETE:
                txtValue.IsReadOnly = true;
                break;
            case HttpMethods.POST:
            case HttpMethods.PUT:
                txtValue.IsReadOnly= false;
                break;
        }
    }


}
