﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Web.Services;
using System.Configuration;

namespace DevOpsDashboard
{
    public partial class JobHistory : System.Web.UI.Page
    {
        string strjenkinsURL = ConfigurationManager.AppSettings["AllJobURL"].ToString();
        string strJobURL =ConfigurationManager.AppSettings["JobURL"].ToString();
        string strAPI =ConfigurationManager.AppSettings["LastBuildApi"].ToString(); 

        string strExtension = "/api/xml";
        Common oCommon = new Common();
        System.Net.WebClient client = new System.Net.WebClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                PopulateProjectNamesJenkins();
                BindJobDetails();
                BindEmptyTable();
            }
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
                        string strResponse = client.DownloadString(strJobURL + jobitems.Value + strAPI);
                        dtTable = oCommon.GetJobdetails(dtTable, strResponse);
                    }
                }
            }

            gridViewJenkins.DataSource = dtTable;
            gridViewJenkins.DataBind();
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
                divChart.Visible = true;
                GetChartData(dtTable);
            }
            else
            {
                BindEmptyTable();

            }

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


        public void GetChartData(DataTable dt)
        {
            string category = "";
            Decimal[] values = new Decimal[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                category = category + "," + dt.Rows[i]["BuildNumber"].ToString();
                if (dt.Rows[i]["Status"].ToString() == "True")
                {
                    values[i] = 1;// Convert.ToDecimal((dt.Rows[i]["Status"].ToString() == "True" ? 1 : 2));
                }
               
            }

            BarChart1.CategoriesAxis = category.Remove(0, 1);
            BarChart1.Series.Add(new AjaxControlToolkit.BarChartSeries { Data = values, BarColor = "#32C4C9", Name = "Build No" });
            BarChart1.ChartTitle = string.Format("{0}  Job History ", ddlProjectsJenkins.SelectedItem.Value);
            string category1 = string.Empty;
            Decimal[] values1 = new Decimal[dt.Rows.Count];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                category1 = category1 + "," + dt.Rows[i]["BuildNumber"].ToString();
                if (dt.Rows[i]["Status"].ToString() != "True")
                {

                    values1[i] = 2;// Convert.ToDecimal((dt.Rows[i]["Status"].ToString() == "True" ? 1 : 2));
                }
            }
            BarChart1.CategoriesAxis = category1.Remove(0, 1);
            BarChart1.Series.Add(new AjaxControlToolkit.BarChartSeries { Data = values1, BarColor = "#FFFFDE", Name = "Build No" });
           
            piechat1.PieChartValues.Add(new AjaxControlToolkit.PieChartValue

            {
                //Category = dt.Rows[i]["BuildNumber"].ToString(),
                //Data = Convert.ToDecimal((dt.Rows[i]["Status"].ToString() == "True" ? 1 : 2))

            });
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
                    if (TotalBuild <= 20)
                    {
                        string strResponse = client.DownloadString(strJobURL + ddlProjectsJenkins.SelectedItem.Value + "/" + i + strExtension);
                        dtTable = oCommon.GetJobdetails(dtTable, strResponse);
                        TotalBuild = TotalBuild + 1;
                    }

                }
                catch (Exception ex)
                {
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

        protected void grdJobHistory_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }
    }
}