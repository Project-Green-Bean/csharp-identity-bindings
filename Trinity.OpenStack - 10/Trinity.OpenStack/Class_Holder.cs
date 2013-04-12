using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/tokens");
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

        public string Tenant_List(string url, string User_Token)
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
                return x.ToString();
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

        public string Delete_Tenant(string adminUrl, string User_Token, string tenantId)
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
        public string error;

        #endregion

        //==============================================================================//
        //
        //                              Add User
        //
        //
        //==============================================================================//
        public static User Add(string url, string name, string password, string enabled, string tenantId,
                                             string email, string admin_token)
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
            } catch (Exception x) {
               // MessageBox.Show("Http error " + x.ToString());
                return_user.name = ("error while parsing: " + x.ToString());
                return (return_user);
            }

            try{
                //parse server return ------------------------------------
                JObject oServerReturn = JObject.Parse(ret);
                String userStr = oServerReturn["user"].ToString();

                JObject oUserString = JObject.Parse(userStr);
                String user_name = oUserString["name"].ToString();
                String user_email = oUserString["email"].ToString();
                String user_tenantid = oUserString["tenantId"].ToString();
                String user_id = oUserString["id"].ToString();
                String user_enabled = oUserString["enabled"].ToString();
 //               String user_password = oUserString["password"].ToString();

                return_user.name = user_name;
                return_user.email = user_email;
                return_user.tenantid = user_tenantid;
                return_user.id = user_id;
                return_user.enabled = user_enabled;
   //             return_user.password = user_password;
                //--------------------------------------------------------

                return (return_user);


            }
            catch (Exception x)
            {
                //return ("error while parsing: " +x.ToString());
                return_user.name = ("error while parsing: " + x.ToString());
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

        public static string Update(string admin_token, string NewID, string UserName, string Email,
                                                string Enabled, string TenantID, string url)
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

                return (ret);
            }
            catch (Exception x)
            {
                return ("Update failed: " + x.ToString());

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
        public static User Get(string url, string userID, string admin_token)
        {
            string ret = string.Empty;
            User return_user = new User();

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + "/v2.0/users/" + userID);

                webRequest.Method = "GET";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                ret = reader.ReadToEnd();

                return_user = Parse(ret);

                return return_user;
            }
            catch (Exception x)
            {
                return_user.error = x.ToString();
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
            string Add_Role_url = url + "/v2.0/tenants/" + TenantId + "/users/" + UserId +
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

    #region User Manager
    public class UserManager : OpenStackObject
    {
        public List<User> user_list;
        public Exception user_manager_error;

        public UserManager()
        {
            base.Type = "User Manager";
        }

        public void List_User(string url, string userToken, string AdminToken)
        {
            List<User> User_List = new List<User>();
            string ret = string.Empty;
            try
            {

                ret = User.List(url, AdminToken);

                JObject root = JObject.Parse(ret);
                JArray ServerReturn = (JArray)root["users"];

                if (ServerReturn != null)
                {

                    for (int i = 0; i < ServerReturn.Count; i++)
                    {
                        User newRole = new User();

                        try
                        {
                            newRole = User.Parse(ServerReturn[i].ToString());
                        }
                        catch (Exception x)
                        {
                            user_manager_error = x;
                            throw x;
                        }

                        User_List.Add(newRole);
                    }


                    user_list = User_List;
                }
                else
                {
                    user_list = new List<User>();
                }

            }
            catch (Exception x)
            {
                throw x;
            }

        }

    }
    #endregion

    #region Endpoints
    public class Endpoint : OpenStackObject
    {

        public string internal_url;
        public string public_url;
        public string admin_url;
        public string region;
        public string name;
        public string endpoint_type;
        public string id;

        public string endpoint_error;

        public Endpoint()
        {
            base.Type = "Endpoint";
        }


        public Boolean Delete_Endpoint(string url, string adminToken)
        {
            string ret = string.Empty;
            try
            {

                ret = base.DELETE(adminToken, url + "/v2.0/endpoints/", this.id);
                return true;

            }
            catch (Exception x)
            {
                // MessageBox.Show("Exception caught: \n" + x.ToString());
                x.ToString();

                throw new ObjectNotFound("Endpoint Not Found; Could not delete"); // Change later
            }
        }

        public override string ToString()
        {
            string ret = "name: " + this.name + Environment.NewLine +
                         "admin URL: " + this.admin_url + Environment.NewLine +
                         "region: " + this.region + Environment.NewLine +
                         "internal URL: " + this.internal_url + Environment.NewLine +
                         "type: " + this.endpoint_type + Environment.NewLine +
                         "id: " + this.id + Environment.NewLine +
                         "public URL: " + this.public_url + Environment.NewLine;
            return ret;
        }


        public static Endpoint Parse(string string_to_parse)
        {

            Endpoint _endpoint = new Endpoint();

            try //try catch for parsing endpoint
            {
                JObject oServerReturn = JObject.Parse(string_to_parse);
                try //try catch for name (not crucial)
                {
                    String nameStr = oServerReturn["name"].ToString();
                    _endpoint.name = nameStr;
                }
                catch
                { }

                String adminURLStr = oServerReturn["adminURL"].ToString();
                String internalURL = oServerReturn["internalURL"].ToString();
                String publicURLStr = oServerReturn["publicURL"].ToString();


                String regionStr = oServerReturn["region"].ToString();
                String idStr = oServerReturn["id"].ToString();

                try//try catch for tyoe (not crucial)
                {
                    String typeStr = oServerReturn["type"].ToString();
                    _endpoint.endpoint_type = typeStr;
                }
                catch
                {
                    //do nothing
                }

                _endpoint.admin_url = adminURLStr;
                _endpoint.internal_url = internalURL;
                _endpoint.public_url = publicURLStr;
                _endpoint.region = regionStr;

                _endpoint.id = idStr;
                _endpoint.endpoint_error = "";

                return _endpoint;
            } catch {
                    throw new BadJson("Json command contained incorrect fields");
                } //end try catch for parsing endpoint
            }
        


        public static Endpoint Create_Endpoint(string admin_token_id, string user_id, string admin_url, string service_name, string region, string service_id, string public_url, string internal_url, string tenant_id)
        {
            string ret = string.Empty;
            StreamWriter requestWriter;
            string postData = "{" +
                                "\"endpoint\": {" +
                                            "\"name\": \"" + service_name + "\", " +
                                            "\"region\": \"" + region + "\", " +
                                            "\"service_id\": \"" + service_id + "\"," +
                                            "\"publicurl\": \"" + public_url + "/v2.0/" + tenant_id + "\"," +
                                            "\"adminurl\": \"" + admin_url + "/v2.0/" + tenant_id + "\"," +
                                            "\"internalurl\": \"" + internal_url + "/v2.0/" + tenant_id + "\" }}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/endpoints");
                webRequest.Headers.Add("X-Auth-Token", admin_token_id);
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
            }
            catch (Exception x)
            {
                Exception ex = OpenStackObject.Parse_Error(x);
                throw ex;
            }

            try
            {
                return Endpoint.Parse(ret);
            }
            catch
            {
                throw new BadJson("Endpoint Failed to parse correctly, but was still created");
            }
        }

        public static List<Endpoint> List_Endpoints(string url, string userId, string AdminToken)
        {
            List<Endpoint> Endpoint_List = new List<Endpoint>();
            Endpoint newEndpoint = new Endpoint();
            string ret = string.Empty;
            try //Try catch for server return (with catch if empty)
            {

                try //try catch for the web request
                {
                    ret = newEndpoint.GET(AdminToken, url + "/v2.0/tokens/" + userId + "/endpoints");
                }  catch (Exception x) {

                Exception ex = OpenStackObject.Parse_Error(x);
                throw ex;
            } //end try catch for web request


                JObject root = JObject.Parse(ret);
                JArray ServerReturn = (JArray)root["endpoints"];


                for (int i = 0; i < ServerReturn.Count; i++)
                {
                    newEndpoint = new Endpoint();

                    try //try catch for endpiont parsing
                    {
                        newEndpoint = Endpoint.Parse(ServerReturn[i].ToString());
                    }
                    catch (Exception x)
                    {
                        throw x;
                    } //end try catch for endpoint parsing

                    Endpoint_List.Add(newEndpoint);
                }


                return Endpoint_List;

            }
            catch  
            {
               return new List<Endpoint>();
            } //end try catch for server return

        }
    
    }

    #endregion
/*
    #region Endpoint Manager
    public class EndpointManager : OpenStackObject
    {
        public List<Endpoint> endpoint_list;
        public Exception endpoint_manager_error;

        public EndpointManager()
        {
            base.Type = "Endpoint Manager";
        }

        public void List_Endpoints(string url, string userToken, string AdminToken)
        {
            List<Endpoint> Endpoint_List = new List<Endpoint>();
            string ret = string.Empty;
            try
            {

                ret = base.GET(AdminToken, url + "/v2.0/tokens/" + userToken + "/endpoints");

                JObject root = JObject.Parse(ret);
                JArray ServerReturn = (JArray)root["endpoints"];


                for (int i = 0; i < ServerReturn.Count; i++)
                {
                    Endpoint newEndpoint = new Endpoint();

                    try
                    {
                        newEndpoint = Endpoint.Parse(ServerReturn[i].ToString());
                    }
                    catch (Exception x)
                    {
                        endpoint_manager_error = x;
                    }

                    Endpoint_List.Add(newEndpoint);
                }


                endpoint_list = Endpoint_List;

            }
            catch (Exception x)
            {
                endpoint_list = new List<Endpoint>();
            }

        }

    }
    #endregion
    */

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

    #region Role Manager
    public class RoleManager : OpenStackObject
    {
        public List<Role> role_list;
        public Exception role_manager_error;

        public RoleManager()
        {
            base.Type = "Role Manager";
        }

        public void List_Roles(string url, string userToken, string AdminToken)
        {
            List<Role> Role_List = new List<Role>();
            string ret = string.Empty;
            try
            {

                ret = Role.List(url, AdminToken);

                JObject root = JObject.Parse(ret);
                JArray ServerReturn = (JArray)root["roles"];

                if (ServerReturn != null)
                {

                    for (int i = 0; i < ServerReturn.Count; i++)
                    {
                        Role newRole = new Role();

                        try
                        {
                            newRole = Role.Parse(ServerReturn[i].ToString());
                        }
                        catch (Exception x)
                        {
                            role_manager_error = x;
                            throw x;
                        }

                        Role_List.Add(newRole);
                    }


                    role_list = Role_List;
                }
                else
                {
                    role_list = new List<Role>();
                }

            }
            catch (Exception x)
            {
                throw x;
            }

        }

    }
    #endregion

    #region Service Methods
    //------------------------------------------------
    // by Arnold Yang
    // last updated on 3:25 PM 22 Jan 2013
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
        /// <summary>
        /// Creates a new user-define service by making a POST call.
        /// </summary>
        /// <param name=”url”> The url is the OpenStack server address. </param>
        /// <param name=”name”> The name of the new service to create. </param>
        /// <param name=”service_type”> The type of the service to create. </param>
        /// <param name=”description”> The description of the service to create. </param>
        /// <param name=”admin_token”> The admin’s token ID. </param>
        /// <returns> If successful, returns the response from Keystone, which is a 
        /// json string that includes the new ID, type, name, and description of the 
        /// newly created service. If unsuccessful, returns an error message 
        /// as a string. </returns>
        /// Function approved by: David Nikaido & Kendall Bailey
        //------------------------------------------------
        public static string Create(string url, string name, string service_type, string description,
                                            string admin_token)
        {


            //Service return_user = new Service();
            string ret = "";
            StreamWriter requestWriter;

            String post_data = "{" + "\"OS-KSADM:service\": {" +
                                     "\"type\": \"" + service_type + "\", " +
                                     "\"description\": \"" + description + "\", " +
                                     "\"name\": \"" + name +
                                     "\"}}";

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
        /// <summary>
        /// Deletes the user-specified service by making a DELETE call.
        /// </summary>
        /// <param name=”url”> The url is the OpenStack server address. </param>
        /// <param name=”service_id”> The id of the service to be deleted. </param>
        /// <param name=”admin_token”> The admin’s token ID. </param>
        /// <returns> If successful, returns “Service was successfully deleted.”
        /// If unsuccessful, returns an error message as a string. </returns>
        /// Function approved by: David Nikaido & Kendall Bailey
        //------------------------------------------------
        public static string Delete(string url, string service_id, string admin_token)
        {
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
        /// <summary>
        /// Gets the user-specified service with a GET call.
        /// </summary>
        /// <param name=”url”> The url is the OpenStack server address. </param>
        /// <param name=”service_id”> The id of the service to be retrieved. </param>
        /// <param name=”admin_token”> The admin’s token ID. </param>
        /// <returns> If successful, returns the response from Keystone, 
        /// which is a json string that includes the ID, type, name, and 
        /// description of the user-specified service. If unsuccessful, 
        /// returns an error message as a string. </returns>
        /// Function approved by: David Nikaido & Kendall Bailey
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
        /// <summary>
        /// Lists all the services associated with the admin token with a GET call.
        /// </summary>
        /// <param name=”url”> The url is the OpenStack server address. </param>
        /// <param name=”admin_token”> The admin’s token ID. </param>
        /// <returns> If successful, returns the response from Keystone, 
        /// which is a json string that lists all the services associated 
        /// with the admin’s token as well as the ID, type, name, and description 
        /// of each of the services. If unsuccessful, returns an error message as a string. </returns>
        /// Function approved by: David Nikaido & Kendall Bailey
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

    #endregion

    #region Exception Objects





    /// <summary>
    /// Functions that throw this exception:
    ///     Endpoint.Parse
    ///     Create_Endpoint
    /// </summary>
    public class BadJson : Exception
    {
        public BadJson() : base() { }

        public BadJson(string message) : base(message) { }

        public BadJson(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Http status: 400
    /// </summary>
    public class BadRequest : Exception
    {
        public BadRequest() : base() { }

        public BadRequest(string message) : base(message) { }

        public BadRequest(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Http status: 409
    /// 
    /// message: Conflict
    /// </summary>
    public class Conflict : Exception
    {
        public Conflict() : base() { }

        public Conflict(string message) : base(message) { }

        public Conflict(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// The service catalog is empty (talk to arnold: do we really need this? or is this otherwise handled?)
    /// </summary>
    public class EmptyCatalog : Exception
    {
        public EmptyCatalog() : base() { }

        public EmptyCatalog(string message) : base(message) { }

        public EmptyCatalog(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Http status : 403
    /// 
    /// Forbidden: your credentials don't give you access to this resource
    /// </summary>
    public class Forbidden : Exception
    {
        public Forbidden() : base() { }

        public Forbidden(string message) : base("Forbidden" ) { }

        public Forbidden(string message, System.Exception inner) : base(message, inner) { }

    }


    /// <summary>
    /// Http status : 501
    /// 
    /// Not Implemented: the server does not support this operation
    /// </summary>
    public class NotImplemented : Exception
    {
        public NotImplemented() : base() { }

        public NotImplemented(string message) : base("Not Implemented") { }

        public NotImplemented(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Look into how to do this?
    /// 
    /// This form of authentication does not support looking up endpoints from an existing token.
     /// </summary>
    public class NoTokenLookup : Exception
    {
        public NoTokenLookup() : base() { }

        public NoTokenLookup(string message) : base("Not Implemented") { }

        public NoTokenLookup(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// 
    /// Unable to find unique resource
    /// </summary>
    public class NoUniqueMatch : Exception
    {
        public NoUniqueMatch() : base() { }

        public NoUniqueMatch(string message) : base("Not Implemented") { }

        public NoUniqueMatch(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Http status: 413
    /// 
    /// Over limit: you’re over the API limits for this time period.
    /// </summary>
    public class OverLimit : Exception
    {
        public OverLimit() : base() { }

        public OverLimit(string message) : base(message) { }

        public OverLimit(string message, System.Exception inner) : base(message, inner) { }

    }

    /// <summary>
    /// Http status: 503
    /// 
    /// Service Unavailable: The server is currently unavailable.
    /// </summary>
    public class ServiceUnavailable : Exception
    {
        public ServiceUnavailable() : base() { }

        public ServiceUnavailable(string message) : base(message) { }

        public ServiceUnavailable(string message, System.Exception inner) : base(message, inner) { }

    }


    /// <summary>
    /// Http status: 404
    /// 
    /// Function that throw this exception:
    ///     Delete Endpoint
    /// </summary>
    public class ObjectNotFound : Exception
    {
        public ObjectNotFound() : base() { }

        public ObjectNotFound(string message) : base(message) { }

        public ObjectNotFound(string message, System.Exception inner) : base(message, inner) { }

    }


    /// <summary>
    /// Http status: 401
    /// 
    /// 
    /// Unauthorized: bad credentials
    /// </summary>
    public class Unauthorized : Exception
    {
        public Unauthorized() : base() { }

        public Unauthorized(string message) : base(message) { }

        public Unauthorized(string message, System.Exception inner) : base(message, inner) { }

    }


    #endregion

    #region "OpenStackObject"

    public abstract class OpenStackObject
    {

        private string object_type;

        public string Type
        {
            get { return object_type; }
            set { object_type = value; }
        }

        protected string POST(string User_Token, string url, string postData)
        {
            string ret = string.Empty;
            StreamWriter requestWriter;

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
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
                return (x.ToString());
            }
        }

        protected string DELETE(string User_Token, string url, string id)
        {

            string ret = string.Empty;
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url + id);

                webRequest.Method = "DELETE";
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
                // MessageBox.Show("Exception caught: \n" + x.ToString());
                throw x;
            }

        }

        public static Exception Parse_Error(Exception ex){
            string error_message = ex.ToString();
            Match match_400 = Regex.Match(error_message, @"400", RegexOptions.IgnoreCase);

            return ex;

            //switch (ex.ErrorCode)
            //{
            //    case 400:
            //        return new BadRequest();
            //    case 401:
            //        return new Unauthorized();
            //    case 403:
            //        return new Forbidden();
            //    case 404:
            //        return new ObjectNotFound();
            //    case 409:
            //        return new Conflict();
            //    case 413:
            //        return new OverLimit();
            //    case 501:
            //        return new NotImplemented();
            //    case 503:
            //        return new ServiceUnavailable();
            //    default:
            //        return ex;
            //}

        }

        //public static Exception Parse_Error(Exception ex, String Message)
        //{

        //    switch (ex.ErrorCode)
        //    {
        //        case 400:
        //            return new BadRequest(Message);
        //        case 401:
        //            return new Unauthorized(Message);
        //        case 403:
        //            return new Forbidden(Message);
        //        case 404:
        //            return new ObjectNotFound(Message);
        //        case 409:
        //            return new Conflict(Message);
        //        case 413:
        //            return new OverLimit(Message);
        //        case 501:
        //            return new NotImplemented(Message);
        //        case 503:
        //            return new ServiceUnavailable(Message);
        //        default:
        //            return ex;
        //    }

        //}


        protected string GET(string User_Token, string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            string ret;

            try
            {
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
                //  OpenStackException.MakeString(x);
                return x.ToString();
            }
        }
    }

    #endregion

    #region Test Functions

    public class TestCreateEndpoint
    {
        public string endpoint_testTenantid = String.Empty;
        public string endpoint_testServiceid = String.Empty;
        public User endpoint_testUser = new User();
        public List<Endpoint> em = new List<Endpoint>();
        public Token EPTestToken;
        protected List<Endpoint> DisposableEndpoints;
        protected int BaseCount = 0;

        public Boolean Set_Up_Create_Endpoints_Test(string admin_url, string admin_token, string testTenantName, string testServiceName)
        {
            Boolean ret = true;
            string admin_url2 = admin_url + "/v2.0/";

            DisposableEndpoints = new List<Endpoint>();
            string testTenantId = String.Empty;
            string testUserName = "EndpointsTestUser";
            string testUserPw = "eptu123";
            string testServiceId = String.Empty;

            if (Create_Test_Tenant(ref testTenantId, testTenantName, admin_url2, admin_token))                            //Create Tenant
            {
                endpoint_testTenantid = testTenantId;
                if (Create_Test_Service(ref testServiceId, testServiceName, admin_url2, admin_token))                     //Create Service
                {
                    endpoint_testServiceid = testServiceId;
                    User u = User.Add(admin_url2 + "users/", testUserName, testUserPw, "true", testTenantId, "null", admin_token);
                    if (u.name.Equals("EndpointsTestUser"))
                    {
                        endpoint_testUser = u;
                        EPTestToken = Token.Request_NoTenant(admin_url, testUserName, testUserPw);
                        if (EPTestToken.token_error.Equals(String.Empty))
                        {

                            
                            em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                            BaseCount =em.Count;
                            return true;
                        }
                        else
                        {
                            Tear_Down_Create_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                            return false;
                        }

                    }
                    else
                    {
                        Delete_Test_Service(testServiceId, admin_url2, admin_token);
                        Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                        return false;
                    }
                }
                else
                {
                    Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }


        public Boolean Tear_Down_Create_Endpoints_Test(string admin_url, string admin_token, User u, string testServiceId, string testTenantId)
        {
            try
            {
                while (DisposableEndpoints.Count > 0)
                {
                    DisposableEndpoints[0].Delete_Endpoint(admin_url, admin_token);
                    DisposableEndpoints.RemoveAt(0);
                }
            }
            catch
            {
                //do nothing
            }


            Boolean ret = true;
            User.Delete(admin_url, u.id, admin_token);
            ret |= Delete_Test_Service(testServiceId, admin_url + "/v2.0/", admin_token);
            ret |= Delete_Test_Tenant(testTenantId, admin_url + "/v2.0/", admin_token);
            if (ret == true)
            {
                endpoint_testServiceid = String.Empty;
                endpoint_testTenantid = String.Empty;
            }
            return ret;

        }

        public Boolean Run_Test_Endpoints(string admin_url, string serviceurl, string public_url, string admin_token, string token, string tenant_id, string service_id, string service_name, string region, int iterationNumber, string EndpointName, Boolean trace, ref string output)
        {
            if (Test_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber))
            {
                Endpoint ep = Endpoint.Create_Endpoint(admin_token, admin_token, admin_url, service_id, region + iterationNumber, service_id, serviceurl, public_url, tenant_id);
                DisposableEndpoints.Add(ep);
                if (trace == true)
                {
                    output = ep.ToString();
                }
            }
            else
            {
                return false;
            }

            return Test_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber + 1);
        }


        public bool Test_Endpoint_List(ref List<Endpoint> em, string token, string admin_url, string admin_token, int iterationNumber)
        {
            try
            {
                em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                return em.Count == (iterationNumber + BaseCount);
            } catch {
                return false;
            }


        }


        private bool Delete_Test_Service(string service_id, string admin_url, string admin_token)
        {
            try
            {
                string delete_url = admin_url + "OS-KSADM/services/" + service_id;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(delete_url);

                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Timeout = 2000;

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                string ret = reader.ReadToEnd();

                return true;

            }
            catch (Exception x)
            {

                throw x;
            }
        }


        private bool Create_Test_Service(ref string testServiceId, string testServiceName, string admin_url, string admin_token)
        {
            String post_data = "{" + "\"OS-KSADM:service\": {" +
                         "\"type\": \"" + "testing" + "\", " +
                         "\"description\": \"" + "If still here please Delete" + "\", " +
                         "\"name\": \"" + testServiceName +
                         "\"}}";

            try
            {
                string create_url = admin_url + "OS-KSADM/services/";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(create_url);
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Method = "POST";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";

                StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream());
                requestWriter.Write(post_data);
                requestWriter.Close();

                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                string ret = reader.ReadToEnd();
                JObject parsed = JObject.Parse(ret);
                //   MessageBox.Show(parsed.ToString());
                testServiceId = parsed["OS-KSADM:service"]["id"].ToString();

                return true;

            }
            catch (Exception x)
            {
                return false;
            }


        }


        private bool Delete_Test_Tenant(string tenantId, string admin_url, string admin_token)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "tenants/" + tenantId);
                webRequest.Headers.Add("X-Auth-Token", admin_token);
                webRequest.Method = "DELETE";
                webRequest.ServicePoint.Expect100Continue = false;
                webRequest.Timeout = 2000;
                webRequest.ContentType = "application/json";


                HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse();
                Stream resStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);

                return true;

            }
            catch (Exception x)
            {
                throw x;
            }
        }


        private bool Create_Test_Tenant(ref string testTenantId, string tenantName, string admin_url, string admin_token)
        {
            StreamWriter requestWriter;
            testTenantId = String.Empty;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "tenants");
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

                JObject ret = JObject.Parse(reader.ReadToEnd());
                // MessageBox.Show(ret.ToString());
                testTenantId = ret["tenant"]["id"].ToString();


                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

    }

    #endregion

} // end Trinity.OpenStack Namespace
