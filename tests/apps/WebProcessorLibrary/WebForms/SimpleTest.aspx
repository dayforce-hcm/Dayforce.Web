<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Register Src="~/WebForms/Controls/TestUserControl.ascx" TagPrefix="uc" TagName="TestControl" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>ASP.NET Web Forms Test</title>
    <link rel="stylesheet" href="~/Content/WebForms.css" />
    <script type="text/javascript">
        function showResult() {
            var result = document.getElementById('resultDiv');
            result.style.display = 'block';
            result.innerHTML = '<strong>Success!</strong> Server-side button click processed at: ' + new Date().toLocaleTimeString();
            return false;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h1>ASP.NET Web Forms Test Page</h1>
            
            <div class="info">
                <strong>Technology:</strong> ASP.NET Web Forms (.aspx)<br />
                <strong>Purpose:</strong> Demonstrates legacy ASP.NET Web Forms functionality<br />
                <strong>Note:</strong> This technology is <em>not</em> supported in ASP.NET Core
            </div>

            <h3>Test User Control</h3>
            <uc:TestControl ID="TestControl1" runat="server" />

            <h3>Server Interaction</h3>
            <asp:Button ID="btnTest" runat="server" Text="Click Me (Server-Side)" 
                        CssClass="button" OnClientClick="return showResult();" />
            
            <div id="resultDiv" class="result"></div>

            <div>
                <a href="/Home/Index" class="back-link">← Back to Dashboard</a>
            </div>
        </div>
    </form>
</body>
</html>
