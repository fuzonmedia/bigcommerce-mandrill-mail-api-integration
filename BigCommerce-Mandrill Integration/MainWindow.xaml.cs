using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Win32; 

namespace BigCommerce_Mandrill_Integration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
           
            RegistryKey registory_data = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("FUZONMEDIA_APPS");
            // Open a subKey as read-only
           
            // If the RegistrySubKey doesn't exist -> (null)
            if (registory_data == null)
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("FUZONMEDIA_APPS");
                test_emailID.Text = "niladridey933@gmail.com";
                RO_template.Text = "ROver2";
                REVIEW_template.Text = "IFU";
                mandrill_apiKey.Text = "KEY";
               
            }
            else
            {
                string tst_email =(string)registory_data.GetValue("test_email".ToUpper());
                if (tst_email == null)
                {
                    test_emailID.Text = "niladridey933@gmail.com";
                }
                else
                {
                    test_emailID.Text = tst_email;
                }

                string ro_template = (string)registory_data.GetValue("ro_template".ToUpper());
                if (ro_template == null)
                {
                    RO_template.Text = "ROver2";
                }
                else
                {
                    RO_template.Text = ro_template;
                }

                string review_template = (string)registory_data.GetValue("review_template".ToUpper());
                if (review_template == null)
                {
                    REVIEW_template.Text = "IFU";
                }
                else
                {
                    REVIEW_template.Text = review_template;
                }

                string mandrill_api = (string)registory_data.GetValue("mandrill_api".ToUpper());
                if (mandrill_api == null)
                {
                    mandrill_apiKey.Text = "CCw9uS6xNGOIoHymneLSSw";
                }
                else
                {
                    mandrill_apiKey.Text = mandrill_api;
                }
            }

        }
        class task_data
        {
            public string order_id { get; set; }
            public string c_name { get; set; }
            public string c_email { get; set; }
            public string c_phone { get; set; }
            public string total_cost { get; set; }
            public string shipping_address { get; set; }
            public string item_details { get; set; }
            public string c_message { get; set; }
            public string pr_img { get; set; }
            public string pr_url { get; set; }
            public string pr_desc { get; set; }

        }
        class result_data
        {
            public string order_id { get; set; }
            public string status { get; set; }
        }

        private delegate void UpdateProgressBarDelegate(
  System.Windows.DependencyProperty dp, Object value);

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar1.Minimum = 0;

            ProgressBar1.Value = 0;
            ProgressBar1.Maximum = added_order.Items.Count;
            double p_value = 0;
            UpdateProgressBarDelegate updatePbDelegate =
            new UpdateProgressBarDelegate(ProgressBar1.SetValue);

            if (added_order.Items.Count > 0)
            {
                List<result_data> rdata = new List<result_data>();

                process_status.Content = "Please wait while we sending email ....";
                button1.Content = "Wait...";
                button1.IsEnabled = false;
                if (MessageBox.Show("Are you sure want to proceed?", "Warning!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    for (int cnt = 0; cnt < added_order.Items.Count; cnt++)
                    {
                        p_value += 1;
                        Dispatcher.Invoke(updatePbDelegate,
                                                    System.Windows.Threading.DispatcherPriority.Background,
                                                    new object[] { ProgressBar.ValueProperty, p_value });
                    try
                    {

                        WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/" + ((ListBoxItem)added_order.Items[cnt]).Uid);
                        HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                        httpreq_order_count.Method = "GET";
                        httpreq_order_count.ContentType = "text/xml; charset=utf-8";

                        httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                        HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();

                        StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                        string result_order = rdr_product_count.ReadToEnd();
                        //textBox1.Text = result_order;
                        bool order_send = false;

                        if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                        {
                            XDocument doc_orders = XDocument.Parse(result_order);
                            foreach (XElement order_data in doc_orders.Descendants("order"))
                            {

                                task_data tsata = new task_data();
                                tsata.order_id = order_data.Element("id").Value.ToString();
                                // MessageBox.Show(tsata.order_id);
                                tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                                tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString();
                                tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);
                                if (testmail_check.IsChecked == true)
                                {
                                    tsata.c_email = test_emailID.Text;
                                }
                                else
                                {

                                    tsata.c_email = order_data.Element("billing_address").Element("email").Value.ToString();
                                }
                                tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                                //  MessageBox.Show("shiipping_Addes");

                                WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                                HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                                httpreq_shipping_count.Method = "GET";
                                httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                                httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                                StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                                string result_shipping = rdr_shipping_count.ReadToEnd();
                                if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                                {
                                    XDocument doc_shippings = XDocument.Parse(result_shipping);
                                    foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                    {
                                        tsata.shipping_address = order_shipping.Element("street_1").Value.ToString() + " " + order_shipping.Element("street_2").Value.ToString();
                                        tsata.shipping_address += "\\n" + order_shipping.Element("city").Value.ToString() + "," + order_shipping.Element("state").Value.ToString() + "," + order_shipping.Element("zip").Value.ToString();
                                        break;

                                    }
                                }
                                 // MessageBox.Show("Products");
                                WebRequest req_big_productcount = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/products");
                                HttpWebRequest httpreq_product_count = (HttpWebRequest)req_big_productcount;
                                httpreq_product_count.Method = "GET";
                                httpreq_product_count.ContentType = "text/xml; charset=utf-8";
                                httpreq_product_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                HttpWebResponse res_product = (HttpWebResponse)httpreq_product_count.GetResponse();
                                StreamReader rdr_product_data = new StreamReader(res_product.GetResponseStream());
                                string result_product = rdr_product_data.ReadToEnd();
                                //MessageBox.Show(result_product);
                                string content_mandrill = "";
                                bool count_pr = false;
                                if (res_product.StatusCode == HttpStatusCode.OK || res_product.StatusCode == HttpStatusCode.Accepted)
                                {
                                    XDocument doc_products = XDocument.Parse(result_product);
                                    foreach (XElement order_product in doc_products.Descendants("product"))
                                    {
                                        if (order_product.Element("product_id").Value.ToString() != "0")
                                        {

                                            string pr_op = "";
                                            foreach (XElement order_product_options in order_product.Descendants("product_options").Descendants("option"))
                                            {
                                                // MessageBox.Show(order_product_options.Element("display_value").Value.ToString());
                                                pr_op += order_product_options.Element("display_value").Value.ToString().Replace("\"", "\\\"") + " ";

                                            }



                                         //   tsata.item_details = order_product.Element("name").Value.ToString().Replace("\"", "\\\"") + " X " + order_product.Element("quantity").Value.ToString() + " - " + pr_op + " - $" + Convert.ToDouble(order_product.Element("price_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture) +"\\n";
                                            tsata.item_details = order_product.Element("name").Value.ToString().Replace("\"", "\\\"") + " - " + pr_op;
                                            //MessageBox.Show(tsata.item_details);

                                            WebRequest req_big_productimg = WebRequest.Create(big_storeurl.Text + "products/" + order_product.Element("product_id").Value.ToString() + "/images");
                                            HttpWebRequest httpreq_product_img = (HttpWebRequest)req_big_productimg;
                                            httpreq_product_img.Method = "GET";
                                            httpreq_product_img.ContentType = "text/xml; charset=utf-8";
                                            httpreq_product_img.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                            HttpWebResponse res_product_img = (HttpWebResponse)httpreq_product_img.GetResponse();
                                            StreamReader rdr_product_data_img = new StreamReader(res_product_img.GetResponseStream());
                                            string result_product_img = rdr_product_data_img.ReadToEnd();

                                           // MessageBox.Show(result_product_img);
                                            if (res_product_img.StatusCode == HttpStatusCode.OK || res_product_img.StatusCode == HttpStatusCode.Accepted)
                                            {
                                                XDocument doc_products_img = XDocument.Parse(result_product_img);
                                                foreach (XElement order_product_img in doc_products_img.Descendants("image"))
                                                {
                                                    tsata.pr_img = order_product_img.Element("image_file").Value.ToString();
                                                    break;
                                                }
                                            }

                                            WebRequest req_big_product = WebRequest.Create(big_storeurl.Text + "products/" + order_product.Element("product_id").Value.ToString() +".xml");
                                            HttpWebRequest httpreq_product = (HttpWebRequest)req_big_product;
                                            httpreq_product.Method = "GET";
                                            httpreq_product.ContentType = "text/xml; charset=utf-8";
                                            httpreq_product.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                            HttpWebResponse res_product_single = (HttpWebResponse)httpreq_product.GetResponse();
                                            StreamReader rdr_product_single = new StreamReader(res_product_single.GetResponseStream());
                                            string result_product_single = rdr_product_single.ReadToEnd();

                                           // MessageBox.Show(result_product_single);
                                            if (res_product_single.StatusCode == HttpStatusCode.OK || res_product_single.StatusCode == HttpStatusCode.Accepted)
                                            {
                                                XDocument doc_product_single = XDocument.Parse(result_product_single);
                                                foreach (XElement order_product_single in doc_product_single.Descendants("product"))
                                                {
                                                    tsata.pr_url = "https://www.bigcommerce-domain.com" + order_product_single.Element("custom_url").Value.ToString() + "?utm_campaign=Reorder&utm_source=Mandrill&utm_medium=referral";
                                                   // tsata.pr_desc = order_product_single.Element("description").Value.ToString().Replace("\"", "\\\"");
                                                    tsata.pr_desc = "Last Purchased Product";
                                                    break;
                                                }
                                            }


                                            if (!count_pr)
                                            {
                                                string tmp_pr = "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"leftColumnContent\\\"><table border=\\\"0\\\" cellpadding=\\\"20\\\" cellspacing=\\\"0\\\" width=\\\"100%\\\"><tr><td valign=\\\"top\\\" align=\\\"center\\\"><a href=\\\"" + tsata.pr_url + "\\\"><img src=\\\"https://www.sandiegopetfooddelivery.com/product_images/" + tsata.pr_img + "\\\" style=\\\"border:#999  thin solid\\\" mc:label=\\\"image\\\" height=\\\"100\\\" width=\\\"100\\\" alt=\\\""+tsata.pr_desc+"\\\"><div style=\\\"\\\"><h4 class=\\\"h4\\\">" + tsata.item_details + "</h4></div></a></td></tr></table></td>";
                                                content_mandrill += "<tr>" + tmp_pr;

                                                count_pr = true;
                                            }
                                            else
                                            {
                                                string tmp_pr = "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"rightColumnContent\\\"><table border=\\\"0\\\" cellpadding=\\\"20\\\" cellspacing=\\\"0\\\" width=\\\"100%\\\"><tr><td valign=\\\"top\\\" align=\\\"center\\\"><a href=\\\"" + tsata.pr_url + "\\\"><img src=\\\"https://www.sandiegopetfooddelivery.com/product_images/" + tsata.pr_img + "\\\" style=\\\"border:#999  thin solid\\\" mc:label=\\\"image\\\" height=\\\"100\\\" width=\\\"100\\\"  alt=\\\"" + tsata.pr_desc + "\\\"><div style=\\\"\\\"><h4 class=\\\"h4\\\">" + tsata.item_details + "</h4></div></a></td></tr></table></td>";


                                                content_mandrill += tmp_pr + "</tr>";
                                                count_pr = false;
                                            }

                                            //MessageBox.Show(tsata.item_details);


                                        }

                                    }
                                    if (count_pr)
                                    {
                                        content_mandrill += "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"rightColumnContent\\\"></td></tr>";
                                    }

                                }
                                // MessageBox.Show(content_mandrill);

                                WebRequest req_mandrill = WebRequest.Create("https://mandrillapp.com/api/1.0/messages/send-template.json");
                                HttpWebRequest httpreq_mandrill = (HttpWebRequest)req_mandrill;
                                httpreq_mandrill.Method = "POST";

                                httpreq_mandrill.ContentType = "application/json";
                                // httpreq_mandrill.Headers.Add("Authorization", "Basic " + asana_APIKey.Text);
                                Stream str_mandrill = httpreq_mandrill.GetRequestStream();
                                StreamWriter strwriter_mandrill = new StreamWriter(str_mandrill, Encoding.ASCII);


                                //string soaprequest_mandrill = "{\"key\": \"CCw9uS6xNGOIoHymneLSSw\",\"template_name\": \"ROver2\",\"template_content\": [{\"name\": \"customer_name\",\"content\": \"<h1>Hi " + tsata.c_name + ",</h1>\"},{\"name\": \"product_section\",\"content\": \"" + content_mandrill + "\"}],\"message\": {\"subject\": \"San Diego Pet Food Delivery - Reorder Now\",\"from_email\": \"contact@sdpfd.com\",\"from_name\": \"SDPFD\",\"to\": [{\"email\": \"" + tsata.c_email + "\",\"name\": \"" + tsata.c_name + "\"}]}, \"async\": true}";

                                string soaprequest_mandrill = "{\"key\": \""+ mandrill_apiKey.Text+"\",\"template_name\": \""+ RO_template.Text+"\",\"template_content\": [{\"name\": \"customer_name\",\"content\": \"Hi " + tsata.c_name + ",\"},{\"name\": \"product_section\",\"content\": \"" + content_mandrill + "\"}],\"message\": {\"subject\": \"San Diego Pet Food Delivery - Reorder Now\",\"from_email\": \"contact@sdpfd.com\",\"from_name\": \"SDPFD\",\"to\": [{\"email\": \"" + tsata.c_email + "\",\"name\": \"" + tsata.c_name + "\"}]}, \"async\": true}";
                                //MessageBox.Show(soaprequest_mandrill);

                                strwriter_mandrill.Write(soaprequest_mandrill.ToString());
                                strwriter_mandrill.Close();
                                HttpWebResponse res_mandrill = (HttpWebResponse)httpreq_mandrill.GetResponse();
                                StreamReader rdr_mandrill = new StreamReader(res_mandrill.GetResponseStream());
                                string result_mandrill = rdr_mandrill.ReadToEnd();
                                //  MessageBox.Show(result_mandrill);
                                order_send = true;
                                rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = "Send Successfully" });

                            }
                        }




                    }

                    catch (Exception ex)
                    {
                       // MessageBox.Show(ex.Message.ToString());

                        rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = ex.Message.ToString() });
                       
                        
                    }
                }
                    display_result.ItemsSource = rdata;
                    process_status.Content = "Task Completed";
                    button1.IsEnabled = true;

                    button1.Content = "Search & Send";
                }
                else
                {
                    process_status.Content = "";
                    button1.IsEnabled = true;

                    button1.Content = "Search & Send";
                }
                
            }
            else
            {
                MessageBox.Show("Please Enter  at least one order ID ");
            }
        }

      

       

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if(oid.Text!="")
            {
            bool add_able = false;

            for (int i = 0; i < added_order.Items.Count; i++)
            {
                if (((ListBoxItem)added_order.Items[i]).Uid == oid.Text)
                {
                    add_able = true;
                    MessageBox.Show("This Order ID is already added ");
                    break;
                }

            }
            if (!add_able)
            {
                added_order.Items.Add(new ListBoxItem { Content = oid.Text, Uid = oid.Text, ToolTip = oid.Text });
                oid.Text = "";
            }
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (added_order.SelectedIndex >= 0)
            {
                added_order.Items.RemoveAt(added_order.SelectedIndex);
            }
        }

        private void added_order_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            test_emailID.IsEnabled = true;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            test_emailID.IsEnabled = false;
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey registory_data = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("FUZONMEDIA_APPS", true);
                registory_data.SetValue("test_email".ToUpper(), test_emailID.Text);
                MessageBox.Show("Data Saved !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey registory_data = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("FUZONMEDIA_APPS", true);
                registory_data.SetValue("ro_template".ToUpper(), RO_template.Text);
                MessageBox.Show("Data Saved !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey registory_data = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("FUZONMEDIA_APPS", true);
                registory_data.SetValue("review_template".ToUpper(), REVIEW_template.Text);
                MessageBox.Show("Data Saved !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistryKey registory_data = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("FUZONMEDIA_APPS", true);
                registory_data.SetValue("mandrill_api".ToUpper(), mandrill_apiKey.Text);
                MessageBox.Show("Data Saved !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button9_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar1.Minimum = 0;

            ProgressBar1.Value = 0;
            ProgressBar1.Maximum = added_order.Items.Count;
            double p_value = 0;
            UpdateProgressBarDelegate updatePbDelegate =
            new UpdateProgressBarDelegate(ProgressBar1.SetValue);

            if (added_order.Items.Count > 0)
            {
                List<result_data> rdata = new List<result_data>();

                process_status.Content = "Please wait while we sending email ....";
                button1.Content = "Wait...";
                button1.IsEnabled = false;
                if (MessageBox.Show("Are you sure want to proceed?", "Warning!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    for (int cnt = 0; cnt < added_order.Items.Count; cnt++)
                    {
                        p_value += 1;
                        Dispatcher.Invoke(updatePbDelegate,
                                                    System.Windows.Threading.DispatcherPriority.Background,
                                                    new object[] { ProgressBar.ValueProperty, p_value });
                        try
                        {

                            WebRequest req_big_order_count = WebRequest.Create(big_storeurl.Text + "orders/" + ((ListBoxItem)added_order.Items[cnt]).Uid);
                            HttpWebRequest httpreq_order_count = (HttpWebRequest)req_big_order_count;
                            httpreq_order_count.Method = "GET";
                            httpreq_order_count.ContentType = "text/xml; charset=utf-8";

                            httpreq_order_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                            HttpWebResponse res_order = (HttpWebResponse)httpreq_order_count.GetResponse();

                            StreamReader rdr_product_count = new StreamReader(res_order.GetResponseStream());
                            string result_order = rdr_product_count.ReadToEnd();
                            //textBox1.Text = result_order;
                            bool order_send = false;

                            if (res_order.StatusCode == HttpStatusCode.OK || res_order.StatusCode == HttpStatusCode.Accepted)
                            {
                                XDocument doc_orders = XDocument.Parse(result_order);
                                foreach (XElement order_data in doc_orders.Descendants("order"))
                                {

                                    task_data tsata = new task_data();
                                    tsata.order_id = order_data.Element("id").Value.ToString();
                                    // MessageBox.Show(tsata.order_id);
                                    tsata.c_message = order_data.Element("customer_message").Value.ToString().Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");
                                    tsata.c_name = order_data.Element("billing_address").Element("first_name").Value.ToString();
                                    tsata.total_cost = Convert.ToDouble(order_data.Element("total_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture);
                                    if (testmail_check.IsChecked == true)
                                    {
                                        tsata.c_email = test_emailID.Text;
                                    }
                                    else
                                    {

                                        tsata.c_email = order_data.Element("billing_address").Element("email").Value.ToString();
                                    }
                                    tsata.c_phone = order_data.Element("billing_address").Element("phone").Value.ToString();
                                    //  MessageBox.Show("shiipping_Addes");

                                    WebRequest req_big_shipping_count = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/shippingaddresses");
                                    HttpWebRequest httpreq_shipping_count = (HttpWebRequest)req_big_shipping_count;
                                    httpreq_shipping_count.Method = "GET";
                                    httpreq_shipping_count.ContentType = "text/xml; charset=utf-8";
                                    httpreq_shipping_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                    HttpWebResponse res_shipping = (HttpWebResponse)httpreq_shipping_count.GetResponse();
                                    StreamReader rdr_shipping_count = new StreamReader(res_shipping.GetResponseStream());
                                    string result_shipping = rdr_shipping_count.ReadToEnd();
                                    if (res_shipping.StatusCode == HttpStatusCode.OK || res_shipping.StatusCode == HttpStatusCode.Accepted)
                                    {
                                        XDocument doc_shippings = XDocument.Parse(result_shipping);
                                        foreach (XElement order_shipping in doc_shippings.Descendants("address"))
                                        {
                                            tsata.shipping_address = order_shipping.Element("street_1").Value.ToString() + " " + order_shipping.Element("street_2").Value.ToString();
                                            tsata.shipping_address += "\\n" + order_shipping.Element("city").Value.ToString() + "," + order_shipping.Element("state").Value.ToString() + "," + order_shipping.Element("zip").Value.ToString();
                                            break;

                                        }
                                    }
                                    // MessageBox.Show("Products");
                                    WebRequest req_big_productcount = WebRequest.Create(big_storeurl.Text + "orders/" + tsata.order_id + "/products");
                                    HttpWebRequest httpreq_product_count = (HttpWebRequest)req_big_productcount;
                                    httpreq_product_count.Method = "GET";
                                    httpreq_product_count.ContentType = "text/xml; charset=utf-8";
                                    httpreq_product_count.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                    HttpWebResponse res_product = (HttpWebResponse)httpreq_product_count.GetResponse();
                                    StreamReader rdr_product_data = new StreamReader(res_product.GetResponseStream());
                                    string result_product = rdr_product_data.ReadToEnd();
                                    //MessageBox.Show(result_product);
                                    string content_mandrill = "";
                                    bool count_pr = false;
                                    if (res_product.StatusCode == HttpStatusCode.OK || res_product.StatusCode == HttpStatusCode.Accepted)
                                    {
                                        XDocument doc_products = XDocument.Parse(result_product);
                                        foreach (XElement order_product in doc_products.Descendants("product"))
                                        {
                                            if (order_product.Element("product_id").Value.ToString() != "0")
                                            {

                                                string pr_op = "";
                                                foreach (XElement order_product_options in order_product.Descendants("product_options").Descendants("option"))
                                                {
                                                    // MessageBox.Show(order_product_options.Element("display_value").Value.ToString());
                                                    pr_op += order_product_options.Element("display_value").Value.ToString().Replace("\"", "\\\"") + " ";

                                                }



                                                //   tsata.item_details = order_product.Element("name").Value.ToString().Replace("\"", "\\\"") + " X " + order_product.Element("quantity").Value.ToString() + " - " + pr_op + " - $" + Convert.ToDouble(order_product.Element("price_inc_tax").Value.ToString()).ToString("0.00", CultureInfo.InvariantCulture) +"\\n";
                                                tsata.item_details = order_product.Element("name").Value.ToString().Replace("\"", "\\\"") + " - " + pr_op;
                                                //MessageBox.Show(tsata.item_details);

                                                WebRequest req_big_productimg = WebRequest.Create(big_storeurl.Text + "products/" + order_product.Element("product_id").Value.ToString() + "/images");
                                                HttpWebRequest httpreq_product_img = (HttpWebRequest)req_big_productimg;
                                                httpreq_product_img.Method = "GET";
                                                httpreq_product_img.ContentType = "text/xml; charset=utf-8";
                                                httpreq_product_img.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                                HttpWebResponse res_product_img = (HttpWebResponse)httpreq_product_img.GetResponse();
                                                StreamReader rdr_product_data_img = new StreamReader(res_product_img.GetResponseStream());
                                                string result_product_img = rdr_product_data_img.ReadToEnd();

                                                // MessageBox.Show(result_product_img);
                                                if (res_product_img.StatusCode == HttpStatusCode.OK || res_product_img.StatusCode == HttpStatusCode.Accepted)
                                                {
                                                    XDocument doc_products_img = XDocument.Parse(result_product_img);
                                                    foreach (XElement order_product_img in doc_products_img.Descendants("image"))
                                                    {
                                                        tsata.pr_img = order_product_img.Element("image_file").Value.ToString();
                                                        break;
                                                    }
                                                }

                                                WebRequest req_big_product = WebRequest.Create(big_storeurl.Text + "products/" + order_product.Element("product_id").Value.ToString() + ".xml");
                                                HttpWebRequest httpreq_product = (HttpWebRequest)req_big_product;
                                                httpreq_product.Method = "GET";
                                                httpreq_product.ContentType = "text/xml; charset=utf-8";
                                                httpreq_product.Credentials = new NetworkCredential(big_user.Text, big_pass.Text);
                                                HttpWebResponse res_product_single = (HttpWebResponse)httpreq_product.GetResponse();
                                                StreamReader rdr_product_single = new StreamReader(res_product_single.GetResponseStream());
                                                string result_product_single = rdr_product_single.ReadToEnd();

                                                // MessageBox.Show(result_product_single);
                                                if (res_product_single.StatusCode == HttpStatusCode.OK || res_product_single.StatusCode == HttpStatusCode.Accepted)
                                                {
                                                    XDocument doc_product_single = XDocument.Parse(result_product_single);
                                                    foreach (XElement order_product_single in doc_product_single.Descendants("product"))
                                                    {
                                                        tsata.pr_url = "https://www.bigcommerce-domain.com" + order_product_single.Element("custom_url").Value.ToString() + "?utm_campaign=Reorder&utm_source=Mandrill&utm_medium=referral";
                                                        // tsata.pr_desc = order_product_single.Element("description").Value.ToString().Replace("\"", "\\\"");
                                                        tsata.pr_desc = "Last Purchased Product";
                                                        break;
                                                    }
                                                }


                                                if (!count_pr)
                                                {
                                                    string tmp_pr = "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"leftColumnContent\\\"><table border=\\\"0\\\" cellpadding=\\\"20\\\" cellspacing=\\\"0\\\" width=\\\"100%\\\"><tr><td valign=\\\"top\\\" align=\\\"center\\\"><a href=\\\"" + tsata.pr_url + "\\\"><img src=\\\"https://www.sandiegopetfooddelivery.com/product_images/" + tsata.pr_img + "\\\" style=\\\"border:#999  thin solid\\\" mc:label=\\\"image\\\" height=\\\"100\\\" width=\\\"100\\\" alt=\\\"" + tsata.pr_desc + "\\\"><div style=\\\"\\\"><h4 class=\\\"h4\\\">" + tsata.item_details + "</h4></div></a></td></tr></table></td>";
                                                    content_mandrill += "<tr>" + tmp_pr;

                                                    count_pr = true;
                                                }
                                                else
                                                {
                                                    string tmp_pr = "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"rightColumnContent\\\"><table border=\\\"0\\\" cellpadding=\\\"20\\\" cellspacing=\\\"0\\\" width=\\\"100%\\\"><tr><td valign=\\\"top\\\" align=\\\"center\\\"><a href=\\\"" + tsata.pr_url + "\\\"><img src=\\\"https://www.sandiegopetfooddelivery.com/product_images/" + tsata.pr_img + "\\\" style=\\\"border:#999  thin solid\\\" mc:label=\\\"image\\\" height=\\\"100\\\" width=\\\"100\\\"  alt=\\\"" + tsata.pr_desc + "\\\"><div style=\\\"\\\"><h4 class=\\\"h4\\\">" + tsata.item_details + "</h4></div></a></td></tr></table></td>";


                                                    content_mandrill += tmp_pr + "</tr>";
                                                    count_pr = false;
                                                }

                                                //MessageBox.Show(tsata.item_details);


                                            }

                                        }
                                        if (count_pr)
                                        {
                                            content_mandrill += "<td valign=\\\"top\\\" width=\\\"180\\\" class=\\\"rightColumnContent\\\"></td></tr>";
                                        }

                                    }
                                    // MessageBox.Show(content_mandrill);

                                    WebRequest req_mandrill = WebRequest.Create("https://mandrillapp.com/api/1.0/messages/send-template.json");
                                    HttpWebRequest httpreq_mandrill = (HttpWebRequest)req_mandrill;
                                    httpreq_mandrill.Method = "POST";

                                    httpreq_mandrill.ContentType = "application/json";
                                    // httpreq_mandrill.Headers.Add("Authorization", "Basic " + asana_APIKey.Text);
                                    Stream str_mandrill = httpreq_mandrill.GetRequestStream();
                                    StreamWriter strwriter_mandrill = new StreamWriter(str_mandrill, Encoding.ASCII);


                                    string soaprequest_mandrill = "{\"key\": \"" + mandrill_apiKey.Text + "\",\"template_name\": \"" + REVIEW_template.Text + "\",\"template_content\": [{\"name\": \"customer_name\",\"content\": \"Hi " + tsata.c_name + ",\"}],\"message\": {\"subject\": \"Delivery Follow Up\",\"from_email\": \"contact@sdpfd.com\",\"from_name\": \"SDPFD\",\"to\": [{\"email\": \"" + tsata.c_email + "\",\"name\": \"" + tsata.c_name + "\"}]}, \"async\": true}";
                                    //MessageBox.Show(soaprequest_mandrill);

                                    strwriter_mandrill.Write(soaprequest_mandrill.ToString());
                                    strwriter_mandrill.Close();
                                    HttpWebResponse res_mandrill = (HttpWebResponse)httpreq_mandrill.GetResponse();
                                    StreamReader rdr_mandrill = new StreamReader(res_mandrill.GetResponseStream());
                                    string result_mandrill = rdr_mandrill.ReadToEnd();
                                    //  MessageBox.Show(result_mandrill);
                                    order_send = true;
                                    rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = "Send Successfully" });

                                }
                            }




                        }

                        catch (Exception ex)
                        {
                            // MessageBox.Show(ex.Message.ToString());

                            rdata.Add(new result_data { order_id = ((ListBoxItem)added_order.Items[cnt]).Uid, status = ex.Message.ToString() });


                        }
                    }
                    display_result.ItemsSource = rdata;
                    process_status.Content = "Task Completed";
                    button1.IsEnabled = true;

                    button1.Content = "Search & Send";
                }
                else
                {
                    process_status.Content = "";
                    button1.IsEnabled = true;

                    button1.Content = "Search & Send";
                }

            }
            else
            {
                MessageBox.Show("Please Enter  at least one order ID ");
            }
        }
    }
}
