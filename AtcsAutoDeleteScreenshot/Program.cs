using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Collections;
using System.IO;
using System.Threading;

namespace AtcsAutoDeleteScreenshot
{
    class Program
    {

        static string myConnectionString = "server=xxxxxxxxxxxx;uid=xxxx;pwd=xxxxxxx;database=xxxxx;";
        static MySqlConnection mysqlConnection1 = new MySqlConnection(myConnectionString);
        static void Main(string[] args)
        {
            watch();

          
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();

        }

        private static void watch()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = "E:/Hosting/ATCS/screenshoot/unila3";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.CreationTime
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {

            int rowcount = countRow();
            Console.WriteLine("Lets GO!!!");
            Console.WriteLine(rowcount);
            if (rowcount >= 30)
            {
                deletedata(rowcount);
            }
            Console.WriteLine("Sleep for a while");
            Thread.Sleep(5000);
        }

        public static void deletedata(int rowcount)
        {
            ArrayList listpath = new ArrayList();
            using (mysqlConnection1)
            {
                mysqlConnection1.Open();

                MySqlDataReader myreader = null;
                MySqlCommand cmd = new MySqlCommand("select * from atcs_screenshot ORDER by id DESC limit  " + rowcount + " OFFSET 30  ", mysqlConnection1);
                myreader = cmd.ExecuteReader();

                while (myreader.Read())
                {
                    string path = myreader.GetString("path");
                    string name = myreader.GetString("name");
                    //Console.WriteLine("E:/Hosting/"+path+name);
                    listpath.Add("E:/Hosting/" + path + name);

                }
                mysqlConnection1.Close();
                mysqlConnection1.Open();
                int templimit = rowcount - 30;
                foreach (string filePath in listpath)
                {
                    Console.WriteLine(filePath);
                    try
                    {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                        Console.WriteLine("success delet "+filePath);
                    }
                    catch (Exception e)
                    {

                       // Console.WriteLine(e.ToString());
                    }
                }
                Console.WriteLine(templimit);
                MySqlDataReader myreader2 = null;
                MySqlCommand cmd2 = new MySqlCommand("delete from atcs_screenshot order by id asc limit " + templimit + " ", mysqlConnection1);
                myreader2 = cmd2.ExecuteReader();

                while (myreader2.Read())
                {
                    Console.WriteLine("deleted from database");
                }


                mysqlConnection1.Close();
            }
        }
        public static int countRow()
        {

            string commandLine = "SELECT COUNT(*) FROM atcs_screenshot";


            using (MySqlCommand cmd = new MySqlCommand(commandLine, mysqlConnection1))
            {
                mysqlConnection1.Open();
                int test = Convert.ToInt32(cmd.ExecuteScalar());
                mysqlConnection1.Close();
                return test;
            }
        }
    }

}
