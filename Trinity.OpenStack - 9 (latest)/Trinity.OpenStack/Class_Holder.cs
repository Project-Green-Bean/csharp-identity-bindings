using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Trinity.OpenStack
{
    #region Token Class            
    //=======================================================================================//
    //--                                 WebRequestToken  - Post                           --//
    //---------------------------------------------------------------------------------------//
    //--                                                                                   --//
    //--  Purpose : Send a Post request to the Keystone Service and recieved back a        --//
    //--            json string containing an access token                                 --//
    //--                                                                                   --//
    //--  Written By : Dr. Ed Boland                      Operating System : Windows 7     --//
    //--        Date : 9/25/2012                                  Language : VS 2012 C#    --//
    //=======================================================================================//
    public class Token
    {    
        //------------------------ Class Attributes ------------------------------------------//
            public string token_id;
            public string token_expiration;
            public string user_id;
            public string user_name;
            public string user_username;
            public string user_roles;
            public string user_roles_links;
            public string SC_name;
            public string SC_type;
            public string SC_endpoints;
            public string token_error;

        //------------------------ Class Methods ----------------------------------------------//

        public static Token Request_NoTenant(string url, string user_name, string password)
        {
            string ret = string.Empty;
            Token _retrieved_token = new Token();

            String postData = "{\"auth\":{\"passwordCredentials\":{\"username\": \"" + user_name +
                              "\", \"password\": \"" + password + "\"}}}";
            StreamWriter requestWriter;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 1000;

                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                _retrieved_token = Token.Parse(ret);

                return _retrieved_token;


            }
            catch (Exception e)
            {

                _retrieved_token.token_error = e.ToString();
                return _retrieved_token; ;
            }
        }


        //=======================================================================================//
        //--                                 WebRequestToken  - Get                            --//
        //---------------------------------------------------------------------------------------//
        //--                                                                                   --//
        //--  Purpose : Send a Get request to the Keystone Service and recieved back a        --//
        //--            json string containing an access token                                 --//
        //--                                                                                   --//
        //--  Written By : Dr. Ed Boland                      Operating System : Windows 7     --//
        //--        Date : 9/25/2012                                  Language : VS 2012 C#    --//
        //=======================================================================================// 
        public static string Get(string url, string getData)
        {
            string ret = string.Empty;
            StreamWriter requestWriter;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(getData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                

                return ret;
            }
            catch
            {
                return "Request_Token.Get failed to retrieve data.";
            }
        }

        //=======================================================================================//
        //--                                Parse                                              --//
        //---------------------------------------------------------------------------------------//
        //--                                                                                   --//
        //--  Purpose : To extract the token from the CURL return for use in other commands.   --//
        //--                                                                                   --//
        //--                                                                                   --//
        //--  Written By : Dr. Ed Boland                      Operating System : Windows 7     --//
        //--        Date : 9/25/2012                                  Language : VS 2012 C#    --//
        //=======================================================================================// 

        public static Token Parse(string string_to_parse)
        {

            Token _token = new Token();

            try
            {
                JObject oServerReturn = JObject.Parse(string_to_parse);
                String accessStr = oServerReturn["access"].ToString();

                JObject oAccess = JObject.Parse(accessStr);
                String tokenStr = oAccess["token"].ToString();
                String serviceCatalogStr = oAccess["serviceCatalog"].ToString();
                String userStr = oAccess["user"].ToString();

                JObject oToken = JObject.Parse(tokenStr);
                String strTokenExpires = oToken["expires"].ToString();
                String strTokenID = oToken["id"].ToString();

                JObject oUser = JObject.Parse(userStr);
                String strUserID = oUser["id"].ToString();
                String strUser_UserName = oUser["username"].ToString();
                String strUserName = oUser["name"].ToString();
                String strUserRoles = oUser["roles"].ToString();

                _token.token_expiration = strTokenExpires;
                _token.token_id = strTokenID;
                _token.user_id = strUserID;
                _token.user_name = strUserName;
                _token.user_roles = strUserRoles;
                _token.user_username = strUser_UserName;
                _token.token_error = "";
                

                /*switch (item_choice)
                {
                    case "token_expiration_date":
                        return strTokenExpires;
                    case "token_value":
                        return strTokenID;
                    case "user_name":
                        return strUserName;
                    case "user_username":
                        return strUser_UserName;
                    case "user_id":
                        return strUserID;
                    default:
                        return "Invalid Return Choice";
                }*/
                return _token;
            }
            catch
            {
                 _token.token_error = "Token.Parse method failed.";           
                return _token;
            }
 
         } // end parse method 
  
    //=======================================================================================//
    //--                                 Request with tenantID                             --//
    //---------------------------------------------------------------------------------------//
    //--                                                                                   --//
    //--  Purpose : Send a Post request to the Keystone Service and recieved back a        --//
    //--            json string containing an access token                                 --//
    //--                                                                                   --//
    //--  Written By : Dr. Ed Boland                      Operating System : Windows 7     --//
    //--        Date : 9/25/2012                                  Language : VS 2012 C#    --//
    //=======================================================================================//
  
        public static Token Request_WithTenantID(string _url, string username, string password, string tenant)
        {
            string request_url = _url + "/v2.0/tokens";
            Token _retrieved_token = new Token();

            try
            {
                string ret = string.Empty;                

                String catalogData = "{\"auth\":{\"passwordCredentials\":{\"username\": \"" + username +
                                            "\", \"password\": \"" + password + "\"},\"tenantName\":\"" + tenant + "\"}}";

                StreamWriter requestWriter;

                var webRequest = System.Net.WebRequest.Create(request_url) as HttpWebRequest;
                if (webRequest != null)
                {
                    webRequest.Method = "POST";
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 2000;

                    webRequest.ContentType = "application/json";
                    //POST the data.
                    using (requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        requestWriter.Write(catalogData);
                    }
                }

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                
                _retrieved_token = Token.Parse(ret);

                return _retrieved_token;


            }
            catch (Exception e)
            {
                _retrieved_token.token_error = "Request_WithTenantID failed";
                return _retrieved_token;
            }    
    }

  } // end class
    #endregion  

    #region API_Info Class
    public class API_Info
    {
        public static string Get(string url)
        {
            string ret = string.Empty;
            
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }
            catch
            {
                return "Request_Token.Get failed to retrieve data.";
            }
        } // end Get method
    } //end API_Info class
#endregion

    #region Tenants
    public class Tenants
    {
        public static string Get(string url, string User_Token)
        {
            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }
            catch
            {
                return "Request_Token.Get failed to retrieve data.";
            }
        }

        private string Tenant_List(string url, string User_Token)
        {


            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/tenants");

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                 return ret;

            }
            catch (Exception x)
            {
               return  x.ToString();
            }

        }

        private string BtnCreateTenant_Click(string adminUrl, string User_Token, string tenantName, string tenantDescrption)
        {

            string ret = string.Empty;
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + tenantDescrption + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(adminUrl + "/v2.0/tenants");
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                 return ret;
            }
            catch (Exception x)
            {
               return x.ToString();
            }
        }

        private string Delete_Tenant(string adminUrl, string User_Token, string tenantId)
        {
         
            string ret = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(adminUrl + "/v2.0/tenants/" + tenantId);
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";


                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }
            catch (Exception x)
            {
               return x.ToString();
            }
        }

        private string Update_Tenant(string adminUrl, string User_Token, string tenantId, string tenantDescription)
        {

            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"description\":\"" + tenantDescription + "\"" +
                                            "}}";


            string ret = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(adminUrl + "/v2.0/tenants/" + tenantId);
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Method = "PUT";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }
            catch (Exception x)
            {
              return x.ToString();
            }
        }
    }

    #endregion

    #region User Methods

    public class User
    {
        #region Class Object Attributes

        public string name;
        public string id;
        public string email;
        public string tenantid;
        public string enabled;
        public string password = "";

        #endregion

        //==============================================================================//
        //
        //                              Add User
        //
        //
        //==============================================================================//
        public static User Add(string url, string name, string password, string enabled, string tenantId, 
                                             string email,  string admin_token)
            {
                
                User return_user = new User();
                string ret = string.Empty;

                StreamWriter requestWriter;

                String postData = "{" +
                                    "\"user\": {" +
                                                "\"name\": \"" + name + "\", " +
                                                "\"password\": \"" + password + "\"," +
                                                "\"email\": \"" + email + "\"," +
                                                "\"tenantId\": \"" + tenantId + "\"," +
                                                "\"enabled\": " + enabled +
                                                "}}";
                    try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Headers.Add("X-Auth-Token", admin_token);
                    webRequest.Method = "POST";
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 2000;
                    webRequest.ContentType = "application/json";

                    requestWriter = new StreamWriter(webRequest.GetRequestStream());
                    requestWriter.Write(postData);
                    requestWriter.Close();

                    HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                    Stream resStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream);
                    ret = reader.ReadToEnd();

                    //parse server return ------------------------------------
                    JObject oServerReturn = JObject.Parse(ret);
                    String userStr = oServerReturn["user"].ToString();

                    JObject oUserString = JObject.Parse(userStr);
                    String user_name = oUserString["name"].ToString();
                    String user_email = oUserString["email"].ToString();
                    String user_tenantid = oUserString["tenantId"].ToString();
                    String user_id = oUserString["id"].ToString();
                    String user_enabled = oUserString["enabled"].ToString();
                    String user_password = oUserString["password"].ToString();

                    return_user.name = user_name;
                    return_user.email = user_email;
                    return_user.tenantid = user_tenantid;
                    return_user.id = user_id;
                    return_user.enabled = user_enabled;
                    return_user.password = user_password;
                    //--------------------------------------------------------

                    return (return_user);


                }
                catch (Exception x)
                {
                    return_user.name = x.ToString();
                    return (return_user);
                }
            }
        
        

        //==============================================================================//
        //
        //                              Delete User
        //
        //
        //==============================================================================//
        
        public static string Delete(string url, string user_id, string admin_token)
            {
                string ret = string.Empty;
                try
                {
                    string delete_url = url + "/v2.0/users/" + user_id;
                    
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(delete_url);

                    webRequest.Method = "DELETE";
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Headers.Add("X-Auth-Token", admin_token);
                    webRequest.Timeout = 2000;

                    HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                    Stream resStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream);
                    ret = reader.ReadToEnd();

                    return "User was successfully deleted.";

                }
                catch (Exception x)
                {
                    return ("Exception caught: \n" + x.ToString());
                } 
            }

        //==============================================================================//
        //
        //                              Update User
        //
        //
        //==============================================================================//

        public static string Update(string admin_token,string NewID, string UserName, string Email,
                                                string Enabled, string TenantID, string url )
        {
            string ret = string.Empty;
            User parsed_user = new User();

            string postData = "{ " +
                               "\"user\": { " +
                                    "\"id\": \"" + NewID + "\"," +
                                    "\"name\": \"" + UserName + "\"," +
                                    "\"email\": \"" + Email + "\"," +
                                    "\"enabled\":" + Enabled + "," +
                                    "\"tenantId\":\"" + TenantID + "\"" +
                                "}" +
                                "}";

            StreamWriter requestWriter;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "PUT";
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 1000;

                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                parsed_user = User.Parse(ret);

                return(ret);
            }
            catch (Exception x)
            {
                return("Update failed: " + x.ToString());
                
            }
        }
        
        //==============================================================================//
        //
        //                              List Users
        //
        //
        //==============================================================================//
        
        public static string List(string url, string User_Token)
        {

            string ret = string.Empty;

            string list_url = url + "/v2.0/users/";

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(list_url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", User_Token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();
                JObject oServerReturn = JObject.Parse(ret);
                String usersStr = oServerReturn["users"].ToString();

                return usersStr;

            }
            catch (Exception x)
            {
                return (x.ToString());
            }
        } // end method

        //==============================================================================//
        //
        //                       List Roles for a User on a Tenant
        //
        //
        //==============================================================================//

        public static string List_User_Roles(string url, string UserId, string TenantId, string AdminId)
        {

            string List_User_Roles_url = url + "/v2.0/tenants/" + TenantId + "/users/" + UserId + "/roles"; 
            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(List_User_Roles_url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", AdminId);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                JObject oServerReturn = JObject.Parse(ret);
                String rolesStr = oServerReturn["roles"].ToString();

                // return ret;
                return (rolesStr);
            }
            catch (Exception x)
            {
                return (" Exception caught." + x.ToString());
            }  
        }

        //==============================================================================//
        //
        //                                      Get User by ID
        //
        //
        //==============================================================================//
        public static User Get_User_ID(string url, string AdminID)
        {
            User return_user = new User();

            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", AdminID);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return_user = User.Parse(ret); 

                return return_user;
            }
            catch (Exception x)
            {
                return_user.enabled = x.ToString();
                return return_user;
            }
        }

        //==============================================================================//
        //
        //                       Add role to user
        //
        //
        //==============================================================================//
        public static string Add_Role_To_User(string url, string UserId, string TenantId, string RoleId, string Admin_Token)
        {
            string Add_Role_url = url + "/v2.0/tenants/" + TenantId+ "/users/" + UserId +
                                "/roles/OS-KSADM/" + RoleId;
            string ret = string.Empty;
            
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Add_Role_url);
                webRequest.Headers.Add("X-Auth-Token", Admin_Token);
                webRequest.Method = "Put";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();
                return ret;
            }
            catch (Exception x)
            {
                return ("Exception caught: \n" + x.ToString());
            }
        }


        //==============================================================================//
        //
        //                       Delete role of user
        //
        //
        //==============================================================================//
        public static string Delete_Role_Of_User(string url, string UserId, string TenantId, string RoleId, string Admin_Token)
            {
                string Remove_Role_url = url + "/v2.0/tenants/" + TenantId + "/users/" + UserId +
                                "/roles/OS-KSADM/" + RoleId;
                string ret = string.Empty;

                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Remove_Role_url);
                    webRequest.Headers.Add("X-Auth-Token", Admin_Token);
                    webRequest.Method = "DELETE";
                    webRequest.ServicePoint.Expect100Continue = false;
                    webRequest.Timeout = 2000;
                    webRequest.ContentType = "application/json";

                    HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                    Stream resStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(resStream);
                    ret = reader.ReadToEnd();
                    return ret;
                }
                catch (Exception x)
                {
                    return ("Exception caught: \n" + x.ToString());
                }
            }

        public static User Parse(string server_return)
        {
            User return_user = new User();

            try
            {
                //parse server return
                JObject oServerReturn = JObject.Parse(server_return);
                String userStr = oServerReturn["user"].ToString();

                JObject oUserString = JObject.Parse(userStr);
                String user_name = oUserString["name"].ToString();
                String user_email = oUserString["email"].ToString();
                String user_tenantid = oUserString["tenantId"].ToString();
                String user_id = oUserString["id"].ToString();
                String user_enabled = oUserString["enabled"].ToString();

                return_user.name = user_name;
                return_user.email = user_email;
                return_user.tenantid = user_tenantid;
                return_user.id = user_id;
                return_user.enabled = user_enabled;

                return return_user;
            }
            catch
            {
                return null;
            }           
        }  //end parse method

    } //end class

#endregion

    #region "End Points"
    public class Endpoint
    {


        private string Delete_Endpoint(string url, string adminToken, string endpointId)
        {
            string ret = string.Empty;
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/endpoints/" + endpointId);

                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", adminToken);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;


            }
            catch (Exception x)
            {
                // MessageBox.Show("Exception caught: \n" + x.ToString());
                return x.ToString();
            }
        }

        private string List_Endpoints(string url, string userToken)
        {
            string ret = string.Empty;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/tokens/" + userToken + "/endpoints");

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", userToken);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();



                return ret;

            }
            catch (Exception x)
            {
                return x.ToString();
            }
        }

    }
    
#endregion

    #region Roles
    public class Role
    {
        public string name;
        public string id;
        public string error;

        public static Role Add(string url, string name, string admin_token)
        {
            Role return_role = new Role();
            string ret = string.Empty;

            StreamWriter requestWriter;

            string postData = "{\"role\": {\"name\":\"" + name + "\"}}";

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/OS-KSADM/roles");
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return_role = Parse(ret);

                return return_role;
            }
            catch (Exception x)
            {
                return_role.error = x.ToString();
                return return_role;
            }
        }

        public static string Delete(string url, string role_id, string admin_token)
        {

            string ret = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/OS-KSADM/roles/" + role_id);
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }
            catch (Exception x)
            {
                return ("Exception caught: \n" + x.ToString());
            }
        }

        public static Role Get(string url, string role_id, string admin_token)
        {
            string ret = string.Empty;
            Role return_role = new Role();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/OS-KSADM/roles/" + role_id);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return_role = Parse(ret);

                return return_role;
            }
            catch (Exception x)
            {
                return_role.error = x.ToString();
                return return_role;
            }
        }

        public static string List(string url, string admin_token)
        {
            string ret = string.Empty;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/OS-KSADM/roles");

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return ret;
            }

            catch (Exception x)
            {
                return ("Exception caught: " + x.ToString());
            }
        }

        public static Role Parse(string server_return)
        {
            Role return_role = new Role();

            JObject oServerReturn = JObject.Parse(server_return);
            String roleStr = oServerReturn["role"].ToString();

            JObject oRoleStr = JObject.Parse(roleStr);
            String role_id = oRoleStr["id"].ToString();
            String role_name = oRoleStr["name"].ToString();

            return_role.id = role_id;
            return_role.name = role_name;

            return return_role;
        }
    }

    #endregion

    # region Services
    //------------------------------------------------
    // by Arnold Yang
    // last updated on 3:51 PM 5 Dec 2012
    //------------------------------------------------

    public class Service
    {

        #region Class Object Attributes

        public string name;
        public string type;
        public string description;

        #endregion

        // from https://github.com/openstack/python-keystoneclient/blob/master/keystoneclient/v2_0/services.py
        // firefox plugin RESTClient
        // rackspace admin url http://198.61.199.47:35357/v2.0/

        //------------------------------------------------
        // Service - Create
        // Add service to service catalog
        // Method: POST
        //------------------------------------------------
        public static string Create(string url, string name, string service_type, string description,
                                            string admin_token)
        {
            // url: "/OS-KSADM/services/%s"
            // post: "OS-KSADM:services"
            /* 
             * body = {"OS-KSADM:service": {'name': name,
                                     'type': service_type,
                                     'description': description}}
               return self._create("/OS-KSADM/services", body, "OS-KSADM:service")
            */

            Service return_user = new Service();
            string ret = "";
            StreamWriter requestWriter;

            /*String post_data = "{" + "\"OS-KSADM:service\": { " +
                                     "\'name\': \"" + name + "\", " +
                                     "\'type\': \"" + service_type + "\", " +
                                     "\'description\': \"" + description +
                                     "}}";*/

            String post_data = "{" + "\"OS-KSADM:service\": { " +
                                     "\'type\': \"" + service_type + "\", " +
                                     "\'description\': \"" + description + "\", " +
                                     "\'name\': \"" + name +
                                     "}}";

            try
            {
                string create_url = url + "/v2.0/OS-KSADM/services/";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(create_url);
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(post_data);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                /*
                //parse server return ------------------------------------
                JObject oServerReturn = JObject.Parse(ret);
                String userStr = oServerReturn["user"].ToString();

                JObject oUserString = JObject.Parse(userStr);
                String user_name = oUserString["name"].ToString();
                String user_email = oUserString["email"].ToString();
                String user_tenantid = oUserString["tenantId"].ToString();
                String user_id = oUserString["id"].ToString();
                String user_enabled = oUserString["enabled"].ToString();
                String user_password = oUserString["password"].ToString();

                return_user.name = user_name;
                return_user.email = user_email;
                return_user.tenantid = user_tenantid;
                return_user.id = user_id;
                return_user.enabled = user_enabled;
                return_user.password = user_password;
                //--------------------------------------------------------
             
                return (return_user);
                */
                return ret;

            }
            catch (Exception x)
            {   /*
                return_user.name = x.ToString();
                return (return_user); */
                return x.ToString();
            }
        }

        //------------------------------------------------
        // Service - Delete
        // Delete service from service catalog
        // Method: DELETE
        //------------------------------------------------
        public static string Delete(string url, string service_id, string admin_token)
        {
            // url: "/OS-KSADM/services/%s" % id

            string ret = "";
            try
            {
                string delete_url = url + "/v2.0/OS-KSADM/services/" + service_id;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(delete_url);

                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return "Service was successfully deleted.";

            }
            catch (Exception x)
            {
                return ("Exception caught: \n" + x.ToString());
            }

        }

        //------------------------------------------------
        // Service - Get
        // Display service from service catalog
        // GET
        //------------------------------------------------
        public static string Get(string url, string service_id, string admin_token)
        {
            Service return_service = new Service();

            string ret = "";
            try
            {
                string get_url = url + "/v2.0/OS-KSADM/services/" + service_id;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(get_url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                /*return_user = User.Parse(ret); 

                return return_user;*/
                return ret;
            }
            catch (Exception x)
            {   /*
                return_user.enabled = x.ToString();
                return return_user;*/
                return "failed";
            }
        }

        //------------------------------------------------
        // Service - List
        // List all services in service catalog
        // GET
        //------------------------------------------------
        public static string List(string url, string admin_token)
        {
            string ret = "";

            string list_url = url + "/v2.0/OS-KSADM/services/";

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(list_url);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();
                /*
                JObject oServerReturn = JObject.Parse(ret);
                String usersStr = oServerReturn["users"].ToString();

                return usersStr;
                 */
                return ret;

            }
            catch (Exception x)
            {
                return (x.ToString());
            }
        }

    } //end class

    # endregion Services

} // end Trinity.OpenStack Namespace
