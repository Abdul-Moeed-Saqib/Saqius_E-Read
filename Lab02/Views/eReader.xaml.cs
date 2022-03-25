using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3.Model;
using Lab02.Data;
using Syncfusion.Pdf.Interactive;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for eReader.xaml
    /// </summary>
    public partial class eReader : Window
    {
        private static Dictionary<string, AttributeValue> books;

        private DisplayBooks win;
        private string bookFile;
        public eReader(GetObjectResponse obj, DisplayBooks window)
        {
            InitializeComponent();
            win = window;
            bookFile = obj.Key;

            MemoryStream documentStream = new MemoryStream();
            obj.ResponseStream.CopyTo(documentStream);
            eReaderControl.LoadAsync(documentStream);
            eReaderControl.ToolbarSettings.ShowFileTools = false;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            win.Show();
        }

        private async void BookmarkButton_Click(object sender, RoutedEventArgs e)
        {
            var getBook = new Dictionary<string, AttributeValue>
            {
                { "UserName", new AttributeValue {S = AccessDynamoDBClient.user.UserName} }
            };

            Dictionary<string, AttributeValueUpdate> updateBookmarks = new Dictionary<string, AttributeValueUpdate>();

            await GetBookMarks();

            var pageNum = new AttributeValue { N = eReaderControl.CurrentPage.ToString() };
            books[bookFile] = pageNum;

            await UpdateBookMark(getBook, updateBookmarks);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await GetBookMarks();

            var b = books[bookFile];

            eReaderControl.CurrentPage = int.Parse(b.N);
        }

        private static async Task GetBookMarks()
        {
            //Dictionary<string, AttributeValue> books = new Dictionary<string, AttributeValue>();
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
                if (item.Key.Equals("Bookmarks"))
                {
                    books = item.Value.M;
                }
            }
        }

        private static async Task UpdateBookMark(Dictionary<string, AttributeValue> getBook, Dictionary<string, AttributeValueUpdate> updateBookmarks)
        {
            updateBookmarks["Bookmarks"] = new AttributeValueUpdate()
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { M = books }
            };

            UpdateItemRequest request = new UpdateItemRequest
            {
                TableName = "Bookshelfs",
                Key = getBook,
                AttributeUpdates = updateBookmarks
            };

            await AccessDynamoDBClient.userDynamoDB.UpdateItemAsync(request);
        }
    }
}
