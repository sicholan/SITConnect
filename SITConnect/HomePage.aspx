<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HomePage.aspx.cs" Inherits="SITConnect.HomePage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <fieldset>
                <legend>HomePage</legend>
                <br />
                <asp:Label ID="lblMessage" runat="server" EnableViewState="false" ></asp:Label>
                <br />
                <br />
                <p><asp:Button ID="btnLogout" runat="server" Text="Logout" OnClick="LogoutMe" Height="27px" Width="133px" />
                    <br />
                    <br />
                    
                </p>
            </fieldset>
        </div>
    </form>
</body>
</html>

