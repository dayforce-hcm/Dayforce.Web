<%@ Control Language="C#" AutoEventWireup="true" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            lblMessage.Text = "User control loaded at " + DateTime.Now.ToString("HH:mm:ss");
        }
    }
</script>

<div class="user-control">
    <h4>
        <span style="font-size: 20px;">🔧</span> Test User Control (.ascx)
    </h4>
    <p>
        This is a reusable user control that can be embedded in multiple pages.
    </p>
    <div class="user-control-message">
        <asp:Label ID="lblMessage" runat="server" Text="Control not initialized" />
    </div>
    <p class="user-control-footer">
        User controls (.ascx) allow component reuse across Web Forms pages.
    </p>
</div>
