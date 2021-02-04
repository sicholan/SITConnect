using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace SITConnect
{

    public partial class Login : System.Web.UI.Page
    {
        static String lockstatus;
        static int attemptcount = 0;
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public class MyObject
        {
            public string success { get; set; }
            public List<string> ErrorMessage { get; set; }
        }

        protected void LoginMe(object sender, EventArgs e)
        {
            if (ValidateCaptcha())
            {
                bool validInput = ValidateInput();
                if (validInput)
                {
                    string pwd = tb_loginpass.Text.ToString().Trim();
                    string userid = tb_loginid.Text.ToString().Trim();
                    SHA512Managed hashing = new SHA512Managed();
                    string dbStatus = DBStatus(userid);
                    string dbHash = getDBHash(userid);
                    string dbSalt = getDBSalt(userid);
                    if (dbStatus != null && dbStatus.Length > 0)
                    {
                        if (dbStatus == "Open")
                        {
                            try
                            {

                                if (dbSalt != null && dbSalt.Length > 0 && dbHash != null && dbHash.Length > 0)
                                {
                                    string pwdWithSalt = pwd + dbSalt;
                                    byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                                    string userHash = Convert.ToBase64String(hashWithSalt);
                                    if (userHash.Equals(dbHash))
                                    {
                                        Session["LoggedIn"] = tb_loginid.Text.Trim();
                                        // creat a new GUID and save it into the session
                                        string guid = Guid.NewGuid().ToString();
                                        Session["AuthToken"] = guid;

                                        // to create a new cookie with this guid value
                                        Response.Cookies.Add(new HttpCookie("AuthToken", guid));

                                        Response.Redirect("HomePage.aspx", false);

                                    }
                                    else if (attemptcount < 3)
                                    {
                                        attemptcount++;
                                        lblMessage.ForeColor = Color.Red;
                                        lblMessage.Text = "Password is invalid. You have " + (3 - attemptcount) + " attempts remaining" + "<br/>";

                                    }
                                    else if (attemptcount == 3)
                                    {
                                        setlockstatus(userid);
                                        lblMessage.ForeColor = Color.Red;
                                        lblMessage.Text = "Your account is locked.";
                                    }

                                }
                                else
                                {
                                    lblMessage.ForeColor = Color.Red;
                                    lblMessage.Text += "Email or password is not valid. Please try again." + "<br/>";
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.ToString());
                            }
                        }
                        else
                        {
                            lblMessage.ForeColor = Color.Red;
                            lblMessage.Text = "Your account is locked.";
                        }
                    }

                        
                    
                }
            }
            else
            {
                lblMessage.ForeColor = Color.Red;
                lblMessage.Text += "Failed to verify captcha" + "<br/>";
            }
            
        }
        protected string getDBHash(string userid)
        {
            string h = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = string.Format("Select PasswordHash FROM [Account] WHERE Email=@0");
            SqlCommand command = new SqlCommand(sql, con);
            command.Parameters.AddWithValue("@0", userid);
            try
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["PasswordHash"] != null)
                        {
                            if (reader["PasswordHash"] != DBNull.Value)
                            {
                                h = reader["PasswordHash"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            return h;
        }
        protected string getDBSalt(string userid)
        {
            string s = null;
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            string sql = string.Format("select PasswordSalt FROM [Account] WHERE Email=@0");
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@0", userid);
            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["PASSWORDSALT"] != null)
                        {
                            if (reader["PASSWORDSALT"] != DBNull.Value)
                            {
                                s = reader["PASSWORDSALT"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
        private void setlockstatus(String userid)
        {
            SqlConnection connection = new SqlConnection(MYDBConnectionString);
            String updatedata = "Update Account set status=@Status where Email=@0";
            connection.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@0", userid);
            cmd.Parameters.AddWithValue("@Status", "Locked");
            cmd.CommandText = updatedata;
            cmd.Connection = connection;
            cmd.ExecuteNonQuery();
        }
        protected string DBStatus(string userid)
        {
            string s = null;
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            string sql = string.Format("Select Status FROM [Account] WHERE Email=@0");
            SqlCommand command = new SqlCommand(sql, con);
            command.Parameters.AddWithValue("@0", userid);
            try
            {
                con.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if (reader["Status"] != null)
                        {
                            if (reader["Status"] != DBNull.Value)
                            {
                                s = reader["Status"].ToString();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { con.Close(); }
            return s;
        }

        private bool ValidateInput()
        {
            lblMessage.Text = String.Empty;
            lblMessage.ForeColor = Color.Red;
            if (tb_loginid.Text == "")
            {
                lblMessage.Text += "Email is required!" + "<br/>";
            }
            SqlConnection con = new SqlConnection(MYDBConnectionString);
            con.Open();
            string sqlQuery = string.Format("Select * from [Account] where Email=@0");
            SqlCommand cmd = new SqlCommand(sqlQuery, con);
            cmd.Parameters.AddWithValue("@0", tb_loginid.Text);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            con.Close();

            if (dt.Rows.Count < 1)
            {
                lblMessage.Text += "Email does not exists!" + "<br/>";
            }
            if (String.IsNullOrEmpty(tb_loginpass.Text))
            {
                lblMessage.Text += "Password is required!" + "<br/>";
            }

            if (String.IsNullOrEmpty(lblMessage.Text))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ValidateCaptcha()
        {
            bool result = true;
            string captchaResponse = Request.Form["g-recaptcha-response"];
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6LfjjEgaAAAAADrLd7XuHLyYIFru_4yjuiUHjl1g &response=" + captchaResponse);

            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        JavaScriptSerializer js = new JavaScriptSerializer();

                        MyObject jsonObject = js.Deserialize<MyObject>(jsonResponse);
                        result = Convert.ToBoolean(jsonObject.success);
                    }
                }
                return result;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }
}