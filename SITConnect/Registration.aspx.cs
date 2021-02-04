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
using System.Text.RegularExpressions;

namespace SITConnect
{
    public partial class Registration : System.Web.UI.Page
    {
        string MYDBConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MYDBConnection"].ConnectionString;
        static string finalHash;
        static string salt;
        byte[] Key;
        byte[] IV;
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        private int checkPassword(string password)
        {
            int score = 0;
            if (password.Length < 8)
            {
                return 1;
            }
            else
            {
                score = 1;
            }
            if (Regex.IsMatch(password, "[a-z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[A-Z]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[0-9]"))
            {
                score++;
            }
            if (Regex.IsMatch(password, "[!@#$%^&*]"))
            {
                score++;
            }

            return score;
        }

        protected void btnRegSubmit(object sender, EventArgs e)
        {
            bool validInput = ValidateInput();
            if (validInput)
            {
                string password = tb_password.Text.ToString().Trim(); ;
                //Generate random "salt"
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] saltByte = new byte[8];
                //Fills array of bytes with a cryptographically strong sequence of random values.
                rng.GetBytes(saltByte);
                salt = Convert.ToBase64String(saltByte);
                SHA512Managed hashing = new SHA512Managed();
                string pwdWithSalt = password + salt;
                byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(password));
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));
                finalHash = Convert.ToBase64String(hashWithSalt);
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.GenerateKey();
                Key = cipher.Key;
                IV = cipher.IV;
                createAccount();
                {
                    Response.Redirect("Login.aspx");
                    
                }
            }
        }

        public void createAccount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(MYDBConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Account VALUES(@Email, @FirstName, @LastName, @CreditCard, @PasswordHash, @PasswordSalt, @Dob, @Status)"))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            DateTime dob = Convert.ToDateTime(tb_dob.Text);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Email", tb_email.Text.Trim());
                            cmd.Parameters.AddWithValue("@FirstName", tb_firstname.Text.Trim());
                            cmd.Parameters.AddWithValue("@LastName", tb_lastname.Text.Trim());
                            cmd.Parameters.AddWithValue("@CreditCard", encryptData(tb_creditno.Text.Trim()));
                            cmd.Parameters.AddWithValue("@PasswordHash", finalHash);
                            cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                            cmd.Parameters.AddWithValue("@Dob", dob);
                            cmd.Parameters.AddWithValue("@Status", "Open");
                            cmd.Connection = con;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        protected byte[] encryptData(string data)
        {
            byte[] cipherText = null;
            try
            {
                RijndaelManaged cipher = new RijndaelManaged();
                cipher.IV = IV;
                cipher.Key = Key;
                ICryptoTransform encryptTransform = cipher.CreateEncryptor();
                //ICryptoTransform decryptTransform = cipher.CreateDecryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(data);
                cipherText = encryptTransform.TransformFinalBlock(plainText, 0, plainText.Length);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { }
            return cipherText;
        }

        private bool ValidateInput()
        {
            lblMsg.Text = String.Empty;
            lblMsg.ForeColor = Color.Red;
            if (tb_email.Text == "")
            {
                lblMsg.Text += "Email is required!" + "<br/>";
            }
            if (!Regex.IsMatch(tb_email.Text, @"^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$"))
            {
                lblMsg.Text += "Invalid email input!" + "<br/>";
            }
            try
            {
                SqlConnection con = new SqlConnection(MYDBConnectionString);
                con.Open();
                string sqlQuery = string.Format("Select * from [Account] where Email=@0");
                SqlCommand cmd = new SqlCommand(sqlQuery, con);
                cmd.Parameters.AddWithValue("@0", tb_email.Text);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                con.Close();

                if (dt.Rows.Count > 0)
                {
                    lblMsg.Text += "Email already exists!" + "<br/>";
                }
                if (String.IsNullOrEmpty(tb_firstname.Text))
                {
                    lblMsg.Text += "First Name is required!" + "<br/>";
                }
                if (String.IsNullOrEmpty(tb_lastname.Text))
                {
                    lblMsg.Text += "Last Name is required!" + "<br/>";
                }
                if (String.IsNullOrEmpty(tb_password.Text))
                {
                    lblMsg.Text += "Password is required!" + "<br/>";
                }
                if (String.IsNullOrEmpty(tb_creditno.Text))
                {
                    lblMsg.Text += "Credit Card is required!" + "<br/>";
                }
                if (tb_dob.Text == "")
                {
                    lblMsg.Text += "Date of birth is required!" + "<br/>";
                }
                int scores = checkPassword(tb_password.Text);
                string status = "Password need to be at least 8 characters, at least 1 numeric, at least 1 lowercase, at least 1 uppercase and at least 1 special character";
                //switch (scores)
                //{
                //    case 1:
                //        status = "Very Weak";
                //        break;
                //    case 2:
                //        status = "Weak";
                //        break;
                //    case 3:
                //        status = "Medium";
                //        break;
                //    case 4:
                //        status = "Strong";
                //        break;
                //    case 5:
                //        status = "Very Strong";
                //        break;
                //    default:
                //        break;
                //}
                lbl_pwdchecker.Text = status;
                if (scores < 5)
                {
                    lbl_pwdchecker.ForeColor = Color.Red;
                    return false;
                }
                if (String.IsNullOrEmpty(lblMsg.Text))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (SqlException ex)
            {
                lblMsg.Text = "Invalid Input Detected";
                return false;
            }
            
        }

    }
}