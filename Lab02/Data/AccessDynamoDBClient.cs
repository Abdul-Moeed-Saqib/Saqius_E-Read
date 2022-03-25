using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab02.Data
{
    public static class AccessDynamoDBClient
    {
        private static string keyId = "AKIATS7UDUWP4O7DC66H";
        private static string keySecretAcess = "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";

        public static AmazonDynamoDBClient userDynamoDB;
        public static AmazonS3Client s3User;
        public static ListTablesResponse listTables;
        public static User user;

        public static void GetDynamoDBClient()
        {
            var credentials = new BasicAWSCredentials(keyId, keySecretAcess);
            userDynamoDB = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            s3User = new AmazonS3Client(keyId, keySecretAcess, RegionEndpoint.USEast1);
        }

        public static async Task ListTables()
        {
            listTables = await userDynamoDB.ListTablesAsync();
        }
    }
}
