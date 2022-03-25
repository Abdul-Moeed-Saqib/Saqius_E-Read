using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Lab02.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string tableName = "User";

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AccessDynamoDBClient.GetDynamoDBClient();
            await AccessDynamoDBClient.ListTables();
            await CreateUserTable(CheckUserTable(tableName));
            await InsertUsers(tableName);
        }

        private async void loginButton1_Click(object sender, RoutedEventArgs e)
        {
            var userName = userNameTextBox.Text;
            var password = passwordTextBox.Password;

            var getUser = new Dictionary<string, AttributeValue>
            {
                { "UserName", new AttributeValue { S = userName } },
                { "Password", new AttributeValue {S = password } }
            };

            GetItemRequest request = new GetItemRequest
            {
                TableName = tableName,
                Key = getUser
            };

            try
            {
                var result = await AccessDynamoDBClient.userDynamoDB.GetItemAsync(request);

                if (result.Item.Count > 0)
                {
                    User user = new User();
                    foreach (var item in result.Item)
                    {
                        switch (item.Key)
                        {
                            case "UserName":
                                user.UserName = item.Value.S;
                                break;
                            case "Password":
                                user.Password = item.Value.S;
                                break;
                        }
                    }

                    AccessDynamoDBClient.user = user;
                    invalidLabel.Visibility = Visibility.Hidden;
                    emptyBoxLabel.Visibility = Visibility.Hidden;
                    DisplayBooks display = new DisplayBooks(this);

                    display.Show();
                    this.Close();
                }
                else
                {
                    invalidLabel.Visibility = Visibility.Visible;
                    emptyBoxLabel.Visibility = Visibility.Hidden;
                }
            } 
            catch (InternalServerErrorException ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.ErrorCode, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ResourceNotFoundException ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.ErrorCode, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (AmazonDynamoDBException)
            {
                emptyBoxLabel.Visibility = Visibility.Visible;
                invalidLabel.Visibility = Visibility.Hidden;
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private bool CheckUserTable(string name)
        {
            return AccessDynamoDBClient.listTables.TableNames.Contains(name);
        }

        private async Task CreateUserTable(bool isTableExist)
        {
            if (!isTableExist)
            {
                //string tableName = tableName;
                CreateTableRequest request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "UserName",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Password",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "UserName",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "Password",
                            KeyType = "RANGE"
                        }
                    },
                    BillingMode = BillingMode.PROVISIONED,
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };

                try
                {
                    var response = await AccessDynamoDBClient.userDynamoDB.CreateTableAsync(request);
                    //MessageBox.Show("User table has been created successfully!!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There has been an error: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task InsertUsers(string tableName)
        {
            if (!CheckUserTable(tableName))
            {
                User user1 = new User();
                User user2 = new User();
                User user3 = new User();

                SetUsers(user1, user2, user3);

                await Task.Delay(10000);
                try
                {
                    await AccessDynamoDBClient.userDynamoDB.PutItemAsync(InsertUser(tableName, user1.UserName, user1.Password));
                    await AccessDynamoDBClient.userDynamoDB.PutItemAsync(InsertUser(tableName, user2.UserName, user2.Password));
                    await AccessDynamoDBClient.userDynamoDB.PutItemAsync(InsertUser(tableName, user3.UserName, user3.Password));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ERROR IS: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                } 
            }
        }

        private PutItemRequest InsertUser(string tableName, string userName, string password)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "UserName", new AttributeValue { S = userName } },
                { "Password", new AttributeValue { S = password } }
            };

            return new PutItemRequest
            {
                TableName = tableName,
                Item = item
            };
        }

        private void SetUsers(User user1, User user2, User user3)
        {
            user1.UserName = "claireberenai@gmail.com";
            user2.UserName = "sahilsaqib3333@gmail.com";
            user3.UserName = "superkirby945@gmail.com";

            user1.Password = "ilovebooks123";
            user2.Password = "ineedhelp765";
            user3.Password = "hellothere945";
        }
    }
}
