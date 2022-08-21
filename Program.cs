using Newtonsoft.Json;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MatrixMultiplicationCSharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int size = 3;
            await SetNumberAsync(size).ContinueWith(async (task) => {
             if (task.IsCompleted)
             {
                 if (task.Result)
                 {
                     int[][] matrixA = await RetrieveMatrixAsync("A", size, true);
                   
                     int[][] matrixB = await RetrieveMatrixAsync("B", size, false);
                        Console.WriteLine("Matrix A:");
                        PrintMatrix(matrixA);

                        Console.WriteLine("\nMatrix B:");
                        PrintMatrix(matrixB);
                        int[][] resultMatrix = MultiplyMatrices(matrixA, matrixB);
                        Console.WriteLine("Matirx AB:");
                        PrintMatrix(resultMatrix);
                        string resultString = FlattenMatrix(resultMatrix);
                        Console.WriteLine($"The result of joined matrix in string is {resultString}");
                     string md5Hash = CalculateMd5Hash(resultString);
                        Console.WriteLine($"\nThis is md5Hash String: {md5Hash}");
                        string submitUrl = $"https://recruitment-test.investcloud.com/api/numbers/validate";
                     string response = await SubmitHashAsync(submitUrl, md5Hash);

                     Console.WriteLine(response);
                 }
             }
         
         });
            Console.ReadKey();
        }
        static async Task<bool> SetNumberAsync(int size)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apiUrl = $"https://recruitment-test.investcloud.com/api/numbers/init/{size}";
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("There is an error number initialization api");
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        static async Task<int[][]> RetrieveMatrixAsync(string dataset, int size, bool isRow)
        {
            using (HttpClient client = new HttpClient())
            {
                int[][] matrix = new int[size][];
                for (int i = 0; i < size; i++)
                {
                    string type = isRow ? "row" : "col";
                    string apiUrl = $"https://recruitment-test.investcloud.com/api/numbers/{dataset}/{type}/{i}";
                    string response = await client.GetStringAsync(apiUrl);
                    var resp = JsonConvert.DeserializeObject<ArrayResponse>(response);
                    matrix[i] = resp.Value.ToArray();
                }
                return matrix;
            }
        }

        static int[][] MultiplyMatrices(int[][] matrixA, int[][] matrixB)
        {
            int size = matrixA.Length;
            int[][] resultMatrix = new int[size][];
            for (int i = 0; i < size; i++)
            {
                resultMatrix[i] = new int[size];
                for (int j = 0; j < size; j++)
                {
                    resultMatrix[i][j] = Enumerable.Range(0, size)
                        .Sum(k => matrixA[i][k] * matrixB[k][j]);
                }
            }
            return resultMatrix;
        }
        static string FlattenMatrix(int[][] matrix)
        {
            return string.Concat(matrix.SelectMany(row => row));
        }
        static string CalculateMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        static async Task<string> SubmitHashAsync(string submitUrl,string md5Hash)
        {
            using (HttpClient client = new HttpClient())
            {
                var content=new StringContent(md5Hash, Encoding.UTF8,"application/json");
                HttpResponseMessage response = await client.PostAsync(submitUrl, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        static void PrintMatrix(int[][] matrix)
        {
            foreach (var row in matrix)
            {
                Console.WriteLine(string.Join(" ", row));
            }
        }
    }
    public class ArrayResponse
    {
        public List<int> Value { get; set; }
        public object Cause { get; set; }
        public bool Success { get; set; }
    }

}
