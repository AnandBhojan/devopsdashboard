<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="JobHistorytab.aspx.cs" Inherits="DevOpsDashboard.JobHistorytab" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="head1" runat="server">
    <title>Job History</title>
    <%--  <meta http-equiv="refresh" content="30">--%>
    <link href="StyleSheets/jquery-ui.theme.min.css" type="text/css" rel="stylesheet" />
    <link href="resources/css/jquery-ui-themes.css" type="text/css" rel="stylesheet" />
    <link href="StyleSheets/jquery-ui.min.css" type="text/css" rel="stylesheet" />
    <link href="StyleSheets/jquery-ui.css" type="text/css" rel="stylesheet" />
    <style>
        .bar {
            border-style: none !important;
        }

        .title {
            color: #535d5e;
            font-family: Calibri;
            font-size: 17px;
            font-weight: bold;
            text-align: left;
        }

        .pageview {
            float: left;
            margin: 5px;
            padding: 5px;
            width: 99%;
        }

        .gridviews {
            float: left;
            margin: 5px;
            padding: 5px;
            width: 99%;
        }
    </style>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jquery.ui/1.8.9/jquery-ui.js" type="text/javascript"></script>
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>

    <script type="text/javascript">
        google.load("visualization", "1", { packages: ["corechart"] });
        google.setOnLoadCallback(drawChart);
        google.setOnLoadCallback(drawBarChart);

        $(function () {
            $("#tabs").tabs();
            $("#tabsgit").tabs();
            $("#dvAccordian").accordion({
                autoHeight: false,
                collapsible: true,

            });
        });

        function drawChart() {

            var options = {
                is3D: true,
                width: 500,
                height: 500
            };
            $.ajax({
                type: "POST",
                url: "JobHistorytab.aspx/GetChartData",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var data = google.visualization.arrayToDataTable(r.d);
                    var chart = new google.visualization.PieChart($("#chart")[0]);
                    chart.draw(data, options);
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        }

        function drawBarChart() {

            var options = {
                is3D: true,
                width: 800,
                height: 500,
                point: 10,
                vAxis: {
                    title: "Status",
                    viewWindowMode: 'explicit',
                    viewWindow: {
                        max: 1,
                        min: -1
                    }
                }
            };
            $.ajax({
                type: "POST",
                url: "JobHistorytab.aspx/BindBarChart",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var data = google.visualization.arrayToDataTable(r.d);
                    var barchart = new google.visualization.ColumnChart($("#chart1")[0]);
                    barchart.draw(data, options);
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        }
    </script>
</head>

<body>
    <form id="form1" runat="server">
        <div class="pageview">
            <cc1:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server">
            </cc1:ToolkitScriptManager>
            <div style="vertical-align: middle; text-align: center; font-family: Calibri; font-size: x-large; font-weight: 400; color: #000000;" aria-atomic="False">
                <strong>DEVOPS DASHBOARD
                </strong>
                <br />
                <br />
            </div>
            <div id="dvAccordian" style="font-family: Calibri;">
                <h3>JENKINS</h3>
                <div>
                    <div id="tabs" style="font-family: Calibri;">
                        <ul>
                            <li><a href="#tabs-1">ALL JOBS</a></li>
                            <li><a href="#tabs-2">BUILD HISTORY</a></li>
                            <li><a href="#tabs-3">GRAPHS</a></li>
                        </ul>
                        <div id="tabs-1">

                            <table style="font-family: Calibri; font-size: 15px; width: 100%">
                                <tr>
                                    <td colspan="2" height="20px"></td>
                                </tr>
                                <tr style="font-family: Calibri; font-size: 14px">
                                    <td width="20%">Select Job</td>
                                    <td width="80%" allign="Right">
                                        <asp:DropDownList ID="ddlProjectsJenkins" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlProjectsJenkins_SelectedIndexChanged">
                                        </asp:DropDownList>
                                        <br />
                                </tr>
                                <tr>
                                    <td>
                                        <br />
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="2">

                                        <asp:GridView ID="gridViewJenkins" runat="server" Width="99%" GridLines="None" AutoGenerateColumns="false" CssClass="gridviews">
                                            <AlternatingRowStyle BackColor="White" />
                                            <HeaderStyle BackColor="#589DB8" Height="30px" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
                                            <RowStyle BackColor="#FAFAFA" />

                                            <Columns>
                                                <asp:TemplateField HeaderText="Status">
                                                    <ItemTemplate>
                                                        <asp:Image runat="server" ID="ImgCatStatus" ImageUrl='<%#Eval("Status").Equals(true) ?"~/images/dasboard/checkmark_64.png" : "~/images/dasboard/error_64.png" %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Build Number">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblBuildNo" runat="server" Text='<%# Bind("BuildNumber") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Build Date">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblBuildDuration" runat="server" Text='<%# Bind("BuildOn") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Started By">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblBuildstartedBy" runat="server" Text='<%# Bind("StartedBy") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Job Name">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblJobName" runat="server" Text='<%# Bind("JobName") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Artifacts">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblArtifacts" runat="server" Text='<%# Bind("Artifacts") %>'></asp:Label>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:TemplateField HeaderText="Job URL">
                                                    <ItemTemplate>
                                                        <asp:HyperLink ID="hlView" Target="_blank" runat="server" Text="More Details" NavigateUrl='<%# Bind("JobUrl") %>'></asp:HyperLink>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                        </asp:GridView>

                                    </td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                </tr>

                            </table>

                        </div>

                        <div id="tabs-2">

                            <table width="100%" style="font-family: Calibri; font-size: 15px">
                                <tr>
                                    <td colspan="2">
                                        <div style="height: 400px; overflow: auto">
                                            <asp:GridView ID="grdJobHistory" runat="server" Width="99%" GridLines="None" AutoGenerateColumns="false" CssClass="gridviews">
                                                <AlternatingRowStyle BackColor="White" />
                                                <HeaderStyle BackColor="#589DB8" Height="30px" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
                                                <RowStyle BackColor="#FAFAFA" />
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Status">
                                                        <ItemTemplate>
                                                            <asp:Image runat="server" ID="ImgCatStatus" ImageUrl='<%#Eval("Status").Equals(true) ?"~/images/dasboard/checkmark_64.png" : "~/images/dasboard/error_64.png" %>' />
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Build Number">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblBuildNo" runat="server" Text='<%# Bind("BuildNumber") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField HeaderText="Build Date">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblBuildDuration" runat="server" Text='<%# Bind("BuildOn") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Started By">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblBuildstartedBy" runat="server" Text='<%# Bind("StartedBy") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Job Name">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblJobName" runat="server" Text='<%# Bind("JobName") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Artifacts">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblArtifacts" runat="server" Text='<%# Bind("Artifacts") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Job URL">
                                                        <ItemTemplate>
                                                            <asp:HyperLink ID="hlView" Target="_blank" runat="server" Text="More Details" NavigateUrl='<%# Bind("JobUrl") %>'></asp:HyperLink>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                </Columns>
                                            </asp:GridView>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <br />
                                    </td>
                                </tr>
                                <tr style="font-family: Calibri; font-size: 14px">
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </div>

                        <div id="tabs-3">
                            <div width="100%" id="divChart" runat="server" style="height: 400px; overflow: auto; vertical-align: middle; text-align: center; font-family: Calibri; font-size: small; border: 1px solid Gray">
                                <table width="100%">
                                    <tr>
                                        <td width="80%" class="title" colspan="2">BUILD HISTORY
                                        </td>
                                        <td width="20%">
                                            <table style="width: 100%" border="0">
                                                <tr>
                                                    <td>1 - Success</td>
                                                    <td>-1 - Failure</td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <div id="chart1">
                                            </div>
                                            <br />
                                            <br />
                                            <br />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="title" colspan="2">SUCCESS & FAILURE RATE 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="2">
                                            <div id="chart">
                                            </div>
                                        </td>
                                    </tr>

                                </table>
                            </div>


                        </div>
                    </div>
                </div>
                <h3>GIT</h3>
                <div>
                    <div id="tabsgit" style="font-family: Calibri;">
                        <ul>
                            <li><a href="#tabs-1git">COMMIT DETAILS</a></li>
                        </ul>
                        <div id="tabs-1git">

                            <table style="font-family: Calibri; font-size: 15px; width: 100%">
                                <tr>
                                    <td colspan="2" height="20px"></td>
                                </tr>
                                <tr style="font-family: Calibri; font-size: 14px">
                                    <td width="20%">Select Project</td>
                                    <td width="80%" allign="Right">
                                        <asp:DropDownList ID="ddlProjects" OnSelectedIndexChanged="ddlProjects_SelectedIndexChanged" AutoPostBack="true" runat="server"></asp:DropDownList>
                                        <br />
                                </tr>
                                <tr>
                                    <td>
                                        <br />
                                    </td>
                                </tr>

                                <tr>
                                    <td colspan="2">

                                        <asp:GridView ID="gridViewGITHistory" runat="server" GridLines="None" CssClass="gridviews" Width="100%" HeaderStyle-HorizontalAlign="Left">
                                            <AlternatingRowStyle BackColor="White" />
                                            <HeaderStyle BackColor="#589DB8" Height="30px" Font-Bold="True" ForeColor="White" HorizontalAlign="Left" />
                                            <RowStyle BackColor="#FAFAFA" />

                                        </asp:GridView>

                                    </td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                </tr>

                            </table>

                        </div>
                    </div>
                </div>
            </div>
            <div>
                <asp:Button ID="btnbvack" runat="server" Text="Back" OnClick="btnbvack_Click" Width="80px" />

            </div>
        </div>

    </form>
</body>
</html>
