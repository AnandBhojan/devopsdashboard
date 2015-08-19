using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using mattberther.chef;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Digests;
using System.Net.Http;
using System.Net.Security;

namespace DevOpsDashboard
{
    public partial class JobHistorytab : System.Web.UI.Page
    {
        string strjenkinsURL = ConfigurationManager.AppSettings["AllJobURL"].ToString();
        string strJobURL = ConfigurationManager.AppSettings["JobURL"].ToString();
        string strAPI = ConfigurationManager.AppSettings["LastBuildApi"].ToString();
        string strExtension = "/api/xml";
        Common oCommon = new Common();
        System.Net.WebClient client = new System.Net.WebClient();
        private WebRequest Myrequest;
        private string signature = String.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
               // NewMethod();
                //Uri oUri = new Uri("https://chefsrv.foo800.local/organizations/");
                //AuthenticatedRequest orequest = new AuthenticatedRequest("ChefUser", oUri);
                //ChefServer oserver = new ChefServer(oUri);
                //orequest.Sign(string.Empty);
                //string str = oserver.SendRequest(orequest);
                PopulateProjectNamesJenkins();
                BindJobDetails();
                BindEmptyTable();
                populateProjectNamesddl();
                getCommitHistoryfromGIT();
            }
        }

        private string ToBase64EncodedSha1String(string input)
        {
            return
                Convert.ToBase64String(new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input)));
        }


        private void NewMethod()
        {
            const string path = "/organizations/emc/cookbooks";
            const string basePath = "https://chefsrv.foo800.local";

            var timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var method = "GET";
            var clientName = "chefuser";

            var hashedPath = ToBase64EncodedSha1String(path);
            var hashedBody = ToBase64EncodedSha1String(String.Empty);

            var canonicalHeader = String.Format("Method:{0}\nHashed Path:{1}\nX-Ops-Content-Hash:{2}\nX-Ops-Timestamp:{3}\nX-Ops-UserId:{4}",
                    method, hashedPath, hashedBody, timestamp, clientName);

            string paths = @"C:\Custom File\paul3.pem";
            byte[] input = Encoding.Default.GetBytes(canonicalHeader);
            StreamReader sr = new StreamReader(paths);
            PemReader pr = new PemReader(sr);
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            var key = KeyPair.Private;
            ISigner signer = new RsaDigestSigner(new NullDigest());
            signer.Init(true, key);
            signer.BlockUpdate(input, 0, input.Length);
            signature = Convert.ToBase64String(signer.GenerateSignature());

           

            var client = new HttpClient();
            var message = new HttpRequestMessage();
            message.Method = HttpMethod.Get;
            message.RequestUri = new Uri(basePath + path);
            message.Headers.Add("Accept", "application/json");
            message.Headers.Add("X-Ops-Sign", "algorithm=sha1;version=1.0");
            message.Headers.Add("X-Ops-UserId", clientName);
            message.Headers.Add("X-Ops-Timestamp", timestamp);
            message.Headers.Add("X-Ops-Content-Hash", hashedBody);
            message.Headers.Add("Host", "chefsrv.foo800.local:443");
            message.Headers.Add("X-Chef-Version", "11.4.0");

            //message.RequestUri = new Uri(basePath + path);
            //message.Headers.Add("Accept", "application/json");
            //message.Headers.Add("Host", "chefsrv.foo800.local:443");
            //message.Headers.Add("X-Chef-Version", "11.12.4");
            //message.Headers.Add("X-Ops-Timestamp", timestamp);
            //message.Headers.Add("X-Ops-Sign", "algorithm=sha1;version=1.0");
            //message.Headers.Add("X-Ops-Userid", clientName);
            //message.Headers.Add("X-Ops-Content-Hash", hashedBody);
            //message.Headers.Add("User-Agent", "Chef Knife/11.4.0 (ruby-1.9.2-p320; ohai-6.16.0; x86_64-darwin11.3.0; +http://opscode.com)");

            var currentItem = new StringBuilder();
            var i=0;
            foreach (var line in signature.Split(60))
            {
                message.Headers.Add(String.Format("X-Ops-Authorization-{0}", i++), line);
            }
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback
                        (
                           delegate { return true; }
                        );

            var result = client.SendAsync(message).Result;
            var kk = result.StatusCode;
            var content = result.Content.ReadAsStringAsync();
        
        }


        private void PopulateProjectNamesJenkins()
        {

            List<string> strJobs = oCommon.PopulateProjectNamesJenkins();
            ddlProjectsJenkins.DataSource = strJobs;
            ddlProjectsJenkins.DataBind();
            ddlProjectsJenkins.Items.Insert(0, "-ALL-");
        }


        private void BindJobDetails()
        {
            try
            {
                List<string> strJobs = new List<string>();
                DataTable dtTable = CreateDataTable();
                System.Net.WebClient client = new System.Net.WebClient();
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.BaseAddress = strjenkinsURL;
                if (ddlProjectsJenkins.SelectedValue != "-ALL-")
                {
                    string strResponse = client.DownloadString(strJobURL + ddlProjectsJenkins.SelectedItem.Value + strAPI);
                    dtTable = oCommon.GetJobdetails(dtTable, strResponse);

                }
                else
                {
                    foreach (ListItem jobitems in ddlProjectsJenkins.Items)
                    {
                        if (jobitems.Value != "-ALL-")
                        {
                            try
                            {
                                string strResponse = client.DownloadString(strJobURL + jobitems.Value + strAPI);
                                dtTable = oCommon.GetJobdetails(dtTable, strResponse);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }

                gridViewJenkins.DataSource = dtTable;
                gridViewJenkins.DataBind();
            }
            catch(Exception ex)
            {
                BindEmptyTable();
            }
        }

        public decimal GetTotalresult(bool oStatus, DataTable oDt)
        {
            int numberOfRecords = oDt.AsEnumerable()
               .Count(row => row.Field<bool>("Status") == oStatus);
            decimal val = ((decimal)numberOfRecords / oDt.Rows.Count) * 100;
            double db = numberOfRecords / oDt.Rows.Count;
            decimal db1 = numberOfRecords / oDt.Rows.Count;
            return val;
        }
        private DataTable CreateDataTable()
        {
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("BuildNumber", typeof(string));
            dtTable.Columns.Add("BuildOn", typeof(string));
            dtTable.Columns.Add("StartedBy", typeof(string));
            dtTable.Columns.Add("Status", typeof(bool));
            dtTable.Columns.Add("JobName", typeof(string));
            dtTable.Columns.Add("Artifacts", typeof(string));
            dtTable.Columns.Add("JobUrl", typeof(string));
            return dtTable;
        }

        protected void ddlProjectsJenkins_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindJobDetails();
            DataTable dtTable = BindonLoad();
            if (dtTable.Rows.Count > 0)
            {
                grdJobHistory.DataSource = dtTable;
                grdJobHistory.DataBind();
                divChart.Visible = true; Session["Mydata"] = dtTable;
                //BindBarChart(dtTable);
                //GetChartData();
            }
            else
            {
                BindEmptyTable();

            }

        }
        [WebMethod(EnableSession = true)]
        public static List<object> BindBarChart()
        {
            List<object> chartData = new List<object>();
            DataTable dttable = HttpContext.Current.Session["Mydata"] as DataTable;
            chartData.Add(new object[]
            {
                "Build No", "Status"
            });
            if (dttable != null)
            {
                foreach (DataRow dr in dttable.Rows)
                {
                    int oStatus = dr["Status"].ToString() == "True" ? 1 : -1;
                    chartData.Add(new object[] { dr["BuildNumber"], oStatus });
                }
            }
            return chartData;
            //string category = string.Empty;
            //decimal[] values = new decimal[dt.Rows.Count];
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    category = category + "," + dt.Rows[i]["BuildNumber"].ToString();
            //    if (dt.Rows[i]["Status"].ToString() == "True")
            //    {
            //        values[i] = 1;
            //    }

            //}

            //BarChart1.CategoriesAxis = category.Remove(0, 1);
            //BarChart1.Series.Add(new AjaxControlToolkit.BarChartSeries { Data = values, BarColor = "#32C4C9", Name = "Build No" });
            //BarChart1.ChartTitle = string.Format("{0}  Job History ", ddlProjectsJenkins.SelectedItem.Value);
            //string category1 = string.Empty;
            //Decimal[] values1 = new Decimal[dt.Rows.Count];
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    category1 = category1 + "," + dt.Rows[i]["BuildNumber"].ToString();
            //    if (dt.Rows[i]["Status"].ToString() != "True")
            //    {
            //        values1[i] = 2;// Convert.ToDecimal((dt.Rows[i]["Status"].ToString() == "True" ? 1 : 2));
            //    }
            //}
            //BarChart1.CategoriesAxis = category1.Remove(0, 1);
            //BarChart1.Series.Add(new AjaxControlToolkit.BarChartSeries { Data = values1, BarColor = "#FFFFDE", Name = "Build No" });
        }
        private void BindEmptyTable()
        {
            DataTable dtTable = CreateDataTable();
            dtTable.Rows.Add(dtTable.NewRow());
            grdJobHistory.DataSource = dtTable;
            grdJobHistory.DataBind();
            int columncount = grdJobHistory.Rows[0].Cells.Count;
            grdJobHistory.Rows[0].Cells.Clear();
            grdJobHistory.Rows[0].Cells.Add(new TableCell());
            grdJobHistory.Rows[0].Cells[0].ColumnSpan = columncount;
            grdJobHistory.Rows[0].Cells[0].Text = "No Records Found";
            divChart.Visible = false;
        }



        [WebMethod(EnableSession = true)]
        public static List<object> GetChartData()
        {
            JobHistorytab jb = new JobHistorytab();
            List<object> chartData = new List<object>();
            DataTable dt = HttpContext.Current.Session["Mydata"] as DataTable;
            if (dt != null)
            {
                decimal[] dResult = new decimal[2];
                dResult[0] = jb.GetTotalresult(true, dt);
                dResult[1] = jb.GetTotalresult(false, dt);

                chartData.Add(new object[]
            {
                "Status", "Percentage"
            });
                chartData.Add(new object[] { "SUCCESS", dResult[0] });
                chartData.Add(new object[] { "FAILURE", dResult[1] });

            }
            return chartData;
        }

        private DataTable BindonLoad()
        {
            DataTable dtTable = new DataTable();
            try
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.BaseAddress = strjenkinsURL;
                dtTable = CreateDataTable();
                if (ddlProjectsJenkins.SelectedValue != "-ALL-")
                {
                    string strResponse = client.DownloadString(strJobURL + ddlProjectsJenkins.SelectedItem.Value + strAPI);
                    int BuildNo = GetBuildNo(strResponse);
                    BindJobHistoryDetails(BuildNo, dtTable);

                }
            }
            catch (Exception ex)
            {

            }
            return dtTable;
        }

        private void BindJobHistoryDetails(int BuildNo, DataTable dtTable)
        {

            int TotalBuild = 0;
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.BaseAddress = strjenkinsURL;
            List<string> strJobs = new List<string>();
            for (int i = BuildNo; i > 0; i--)
            {
                try
                {
                    if (TotalBuild < 20)
                    {
                        string strResponse = client.DownloadString(strJobURL + ddlProjectsJenkins.SelectedItem.Value + "/" + i + strExtension);
                        dtTable = oCommon.GetJobdetails(dtTable, strResponse);
                        TotalBuild = TotalBuild + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (WebException ex)
                {
                    continue;
                }
            }
        }



        private int GetBuildNo(string strResponse)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(strResponse);
            int BuildID = Convert.ToInt32(xml.FirstChild["id"].InnerText);
            return BuildID;
        }

        protected void btnbvack_Click(object sender, EventArgs e)
        {
            Response.Redirect("DashBoard.aspx", true);
        }

        private void populateProjectNamesddl()
        {
            List<string> projectNames = new List<string>();
            projectNames.Add("WordPress");
            foreach (string projectName in projectNames)
            {
                ddlProjects.Items.Add(projectName);
            }

        }

        private void getCommitHistoryfromGIT()
        {

            var client = new RestClient("https://api.github.com/");
            string repositoryName = "repos/DevOpsTestAdmin/" + ddlProjects.SelectedValue + "/commits";
            client.Authenticator = new SimpleAuthenticator("username", "devopstestadmin", "password", "password2015");
            var request = new RestRequest(repositoryName, Method.GET);
            var response = client.Execute(request);
            var responsecontent = response.Content;
            deserializeGITCommitsResponse(responsecontent);
        }

        private void deserializeGITCommitsResponse(string responsecontent)
        {

            var commitsList = JsonConvert.DeserializeObject<dynamic>(responsecontent);
            if (commitsList != null && commitsList.Count > 0)
            {
                CommitDetails CommitDetailsRec = new CommitDetails();
                List<CommitDetails> CommitDetailsList = new List<CommitDetails>();
                for (int i = 0; i < 10; i++)
                {
                    CommitDetailsRec = new CommitDetails();
                    var commitmessage = commitsList[i].commit.message.Value;
                    var commitername = commitsList[i].commit.committer.name.Value;
                    var commitdate = commitsList[i].commit.committer.date.Value;
                    CommitDetailsRec.Name = commitername;
                    CommitDetailsRec.Date = commitdate.ToString();
                    CommitDetailsRec.Message = commitmessage;
                    CommitDetailsList.Add(CommitDetailsRec);
                }
                bindCommitsHistoryToWebpage(CommitDetailsList);
            }
        }

        private void bindCommitsHistoryToWebpage(List<CommitDetails> CommitDetailsList)
        {

            gridViewGITHistory.DataSource = CommitDetailsList;
            gridViewGITHistory.DataBind();
        }

        protected void ddlProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            getCommitHistoryfromGIT();
        }
    }
}