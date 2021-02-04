<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SITConnect.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login</title>

    <script src="https://www.google.com/recaptcha/api.js?render=6LfjjEgaAAAAAKSymGTqeQueeZ7MnZ2SuKbD8Ke9"></script>
</head>
<body>
    <script>
        grecaptcha.ready(function () {
            grecaptcha.execute('6LfjjEgaAAAAAKSymGTqeQueeZ7MnZ2SuKbD8Ke9', { action: 'Login' }).then(function (token) {
                document.getElementById("g-recaptcha-response").value = token;
            });
        });
    </script>
    <form id="form1" runat="server">
        <div>
            <fieldset>
                <legend>Login</legend>
                <p>Username : <asp:TextBox ID="tb_loginid" runat="server" Height="25px" Width="137px" TextMode="Email" /> </p>
                <p>Password : <asp:TextBox ID="tb_loginpass" runat="server" Height="25px" Width="137px" TextMode="Password" /> </p>
                <p><asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="LoginMe" Height="27px" Width="133px" /></p>
                    <br />
                    <br />
                    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" ></asp:Label>
                    <br />
                    <br />
                    <input type="hidden" id="g-recaptcha-response" name="g-recaptcha-response"></input>
                    
            </fieldset>
        </div>
    </form>
</body>
</html>
