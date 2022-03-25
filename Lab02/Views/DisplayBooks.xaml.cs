using Amazon.DynamoDBv2.Model;
using Amazon.S3.Model;
using Lab02.Data;
using Syncfusion.Windows.PdfViewer;
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
using System.Windows.Shapes;

namespace Lab02.Views
{
    /// <summary>
    /// Interaction logic for DisplayBooks.xaml
    /// </summary>
    public partial class DisplayBooks : Window
    {
        Dictionary<string, AttributeValue> books = new Dictionary<string, AttributeValue>();

        private LoginWindow win;
        private string bucName;

        public DisplayBooks(LoginWindow window)
        {
            InitializeComponent();
            win = window;
            userTextBlock.Text = AccessDynamoDBClient.user.UserName;
            bucName = "books4lab02";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            easterEggImage.Visibility = Visibility.Hidden;

            if (AccessDynamoDBClient.user.UserName.Equals("claireberenai@gmail.com"))
            {
                easterEggImage.Visibility = Visibility.Visible;
            }

            await AccessBooks();

            List<Book> bookNames = new List<Book>();

            foreach (var item in books)
            {
                bookNames.Add(new Book() { BookName = item.Value.S });
            }

            bookDataGrid.ItemsSource = bookNames;
            System.Windows.Data.CollectionViewSource bookViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("bookViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // bookViewSource.Source = [generic data source]
        }

        private async Task AccessBooks()
        {
            var getBook = new Dictionary<string, AttributeValue>
            {
                { "UserName", new AttributeValue {S = AccessDynamoDBClient.user.UserName} }
            };

            GetItemRequest request = new GetItemRequest
            {
                TableName = "Bookshelfs",
                Key = getBook
            };

            var result = await AccessDynamoDBClient.userDynamoDB.GetItemAsync(request);


            foreach (var item in result.Item)
            {
                if (item.Key.Equals("Books"))
                {
                    books = item.Value.M;
                }
            }
        }

        private async void bookDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var book = bookDataGrid.SelectedItem as Book;
            var redBook = books.Single(x => x.Value.S == book.BookName);
            GetObjectRequest request = new GetObjectRequest();
            request.BucketName = bucName;
            request.Key = redBook.Key;
            GetObjectResponse response = (await AccessDynamoDBClient.s3User.GetObjectAsync(request));

            eReader r = new eReader(response, this);
            r.Show();
            this.Hide();
        }

        private void logOutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
            easterEggImage.Visibility = Visibility.Hidden;
        }
    }
}
