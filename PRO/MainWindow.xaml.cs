using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using Microsoft.Win32;
using PRO;
using System.Globalization;
using CsvHelper;
using System.Text;

namespace WebScraping.ListView_control
{
    public partial class MainWindow : Window
    {
        //----declare global variable----//
        string[] btn_add;
        List<User> items = new List<User>();
        string url = "";

        public string urlResponse;
        public MainWindow()
        {
            InitializeComponent();
        }
       
        public void search()
        {
            CollectionView collection = (CollectionView)CollectionViewSource.GetDefaultView(lstContent.ItemsSource);
            collection.Filter = UserFilter;
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtsearch.Text))
                return true;
            else
            {
                  var i = (item as User).Name.IndexOf(txtsearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                  var j = (item as User).TContent.IndexOf(txtsearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;

                if(i == true)
                {
                    return i;
                }
                else if( j == true)
                {
                    return j;
                }
                return i; 
            }



        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            search();
            CollectionViewSource.GetDefaultView(lstContent.ItemsSource).Refresh();
        }
        
        //----OK button click----///
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(Url_text.Text))
            {
                MessageBox.Show("Input the URL");
                return;
            }
            else
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                url = Url_text.Text;
                urlResponse = URLRequest(url);

                //----network & url check----//
                if (urlResponse == null)
                {
                    MessageBox.Show("Incorrect your URL");
                    return;
                }
                if (urlResponse == "error" )
                {
                    MessageBox.Show("Address Error");
                    return;
                }
                else if (urlResponse == "Network error"){
                    MessageBox.Show("NetWork Error");
                }

                htmlDoc.LoadHtml(urlResponse);

                //  -----   Title     ----  //
                var titleNodes = htmlDoc.DocumentNode.SelectNodes("//title");
                if(titleNodes != null)
                {
                    items.Add(new User() { Name = "The title is", TContent = titleNodes.FirstOrDefault().InnerText.Replace("\n" ,"") });
                }

                //---Keyword---//
                var keywwordNodes = htmlDoc.DocumentNode.SelectNodes("//meta[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = 'keywords']");

                if (keywwordNodes != null)
                {
                    string[] item = { };
                    List<string> list = new List<string>();

                    foreach (var keywordNode in keywwordNodes)
                    {
                        string Content = keywordNode.GetAttributeValue("Content", "").Replace("=" ,"");
                        if (!String.IsNullOrEmpty(Content))
                        {
                            /*
                            string[] keywords = Content.Split(new string[] { "," }, StringSplitOptions.None);
                            string[] item = { } ;
                            foreach (var keyword in keywords)
                            {
                               item = keywords.Select(i => String.Format("{0}", keyword)).ToArray();
                            }*/
                            list.Add(String.Format("{0}", Content));
                            item = list.ToArray();
                        }
                    }
                    items.Add(new User() { Name = "Keyword", TContent = string.Join(",", item) });
                    //    lstContent.ItemsSource = items;

                }

                // --- content ----//
                var anchorNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class = 'search search01 clearfix']");
                //    var anchorNodes = htmlDoc.DocumentNode.Descendants("p").Where(node => !node.GetAttributeValue("class", "").Contains("tit")).ToList();
                //var anchorNodes1 = htmlDoc.DocumentNode.SelectNodes("//p[@class='evaluation']");
                // var anchorNodes1 = htmlDoc.DocumentNode.SelectNodes("//div[@class = 'search search01 clearfix']//p[@class='evaluation']");
                var detail = htmlDoc.DocumentNode.SelectNodes("//p[@class='comBtn']//a");

                if (anchorNodes != null)
               {

                    items.Add(new User() { Name = "Find tag count", TContent = String.Format("{0}", anchorNodes.Count) });
                    btn_add = new string[anchorNodes.Count];
                   // string[] item_title;
                    int i = 0;
                    foreach(var anchorNode in anchorNodes)
                    {
                        // itemm_name = String.Format("{0}", anchor.InnerText);
                        
                        items.Add(new User() { Name = String.Format("{0}", anchorNode.SelectNodes("//p[@class='tit']")[i].InnerText.Replace("&nbsp", "").Replace("\n" , " ")),
                            TContent = String.Format("{0}", anchorNode.SelectNodes("//p[@class='evaluation']")[i].InnerText.Replace("&nbsp;" , "  ").Replace("\n", ""))});
                        //   items.Add(new User() { Name = String.Format("{0}", anchorNode.SelectSingleNode("//p[@class='tit']").InnerText.Replace("&nbsp ", "")), TContent = String.Format("{0}", anchorNode.SelectSingleNode("//p[@class='evaluation']").InnerText.Replace("\n" , ""))});
                        btn_add[i] = detail[i].GetAttributeValue("href","").ToString();
                        i++;
                    }

                }
                detail_get();
                lstContent.ItemsSource = items;
                
                
            }
        }

        private void detail_get()
        {

            HtmlWeb web = new HtmlWeb();
            int i = 0;
            
            //string[] content_1 = new string[5];
            string content_1 = "";
            string content_2 = "";
            string content_3 = "";
            if (btn_add.Count() == 0)
            {
                Console.WriteLine("URL ERROR");
                return;
            }

            while( i <btn_add.Length)
            {
                url =  btn_add[i].ToString();
                HtmlDocument doc = web.Load(url);
                var evalutate = doc.DocumentNode.SelectNodes("//div[@id = 'conts']");
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectSingleNode("//h2").InnerText.Replace("アリミノ", "").Replace("\n", "")), TContent = "" });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='clearfix']//dt").InnerText) , TContent = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='clearfix']//dd").InnerText.Replace("&nbsp;","").Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//dl[@class='clearfix']//dt")[1].FirstChild.InnerText.Replace("\n","")) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//dl[@class='clearfix']//dd")[1].InnerText.Replace("&nbsp;","").Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[0].FirstChild.InnerText) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[0].LastChild.InnerText.Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[1].FirstChild.InnerText) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[1].LastChild.InnerText.Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[2].FirstChild.InnerText) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[2].LastChild.InnerText.Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[3].FirstChild.InnerText) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[3].LastChild.InnerText.Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[4].FirstChild.InnerText) , TContent = String.Format("{0}", evalutate[0].SelectNodes("//ul[@class='evaluate']//li")[4].LastChild.InnerText.Replace("\n","")) });
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectSingleNode("//div[@class='price']//dl[@class='clearfix']//dt").InnerText), TContent = String.Format("{0}", evalutate[0].SelectSingleNode("//div[@class='price']//dl[@class='clearfix']//dd").InnerText)});
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectNodes("//div[@class='price']//dl[@class='clearfix']//dt")[1].InnerText), TContent = String.Format("{0}", evalutate[0].SelectNodes("//div[@class='price']//dl[@class='clearfix']//dd")[1].InnerText).Replace("\n" , "")});
                items.Add(new User() { Name = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='comDl mb30 clearfix']//dt").InnerText), TContent = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='comDl mb30 clearfix']//dd").InnerText)});
                try
                {
                    var TContent_1 = evalutate[0].SelectNodes("//dl[@class='comDl mb0 clearfix']//dd//p[not(contains(@style,'margin-top'))]").ToList();
                    foreach (var object_1 in TContent_1)
                    {
                        content_1 += object_1.InnerText.Replace("&nbsp", "").Replace("\n" , "").ToString();
                    }
                }
                catch(Exception ex)
                {
                    Console.Write(ex.Message);
                    content_1 = "";
                }
                  
                   
                items.Add(new User()
                {
                    Name = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='comDl mb0 clearfix']//dt").InnerText),
                    TContent = String.Format("{0}", content_1)
                });
                try
                {
                    content_2 = String.Format("{0}", evalutate[0].SelectSingleNode("//dl[@class='comDl mb0 clearfix']//dd//a").InnerText);
                    content_3 = String.Format("{0}", evalutate[0].SelectNodes("//dl[@class='comDl mb0 clearfix']//dd//p[(contains(@style,'margin-top'))]")[0].InnerText);
                }
                catch(Exception ex)
                {
                    Console.Write(ex.Message);
                    content_2 = "";
                    content_3 = "";
                }
                items.Add(new User() { Name = content_3, TContent= content_2  });
                items.Add(new User() { Name = String.Format("{0}", "画像URL"), TContent = String.Format("{0}", evalutate[0].SelectSingleNode("//div[@class='photoBox']//img/@src").Attributes["src"].Value.ToString()) });
                i++;
           }
        }
        static string URLRequest(string url)
        {

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "Get";
                request.Timeout = 10000;
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";


                string responseContent = null;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    WebHeaderCollection headers = response.Headers;
                    using (Stream answer = response.GetResponseStream())
                    {
                        StreamReader streamreader = new StreamReader(answer);
                        responseContent = streamreader.ReadToEnd();
                    }
                }
                return responseContent;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    var answer = "Network error";
                    // StreamReader streamreader = new StreamReader(answer);

                    return answer;
                }
                return "error";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private void Ex_csv(object sender, RoutedEventArgs e)
        {
            if(items.Count == 0)
            {
                MessageBox.Show("Empty list.");
                return;
            }
            try
            {

                using (var writer = new StreamWriter( "example.csv",false ,Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                
                    foreach (var item in (lstContent.Items))
                    {
                        csv.WriteField(((MainWindow.User)item).Name);
                        csv.WriteField(((MainWindow.User)item).TContent);
                        csv.NextRecord();
                    }
                    MessageBox.Show("successed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_browser(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                lstContent.Items.Add(File.ReadAllText(openFileDialog.FileName));
            }
        }

        public class User
        {
            public string Name
            {
                get; set;
            }
            public string TContent { get; set; }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            items.Clear();
        }
    }

}
