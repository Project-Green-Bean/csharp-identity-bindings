﻿using System;
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

                throw new ObjectNotFoundException("Object Not Found; Could not delete"); // Change later
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

            try
            {
                JObject oServerReturn = JObject.Parse(string_to_parse);
                String nameStr = oServerReturn["name"].ToString();

                String adminURLStr = oServerReturn["adminURL"].ToString();
                String internalURL = oServerReturn["internalURL"].ToString();
                String publicURLStr = oServerReturn["publicURL"].ToString();

                String typeStr = oServerReturn["type"].ToString();
                String regionStr = oServerReturn["region"].ToString();
                String idStr = oServerReturn["id"].ToString();


                _endpoint.admin_url = adminURLStr;
                _endpoint.internal_url = internalURL;
                _endpoint.public_url = publicURLStr;
                _endpoint.region = regionStr;
                _endpoint.name = nameStr;
                _endpoint.id = idStr;
                _endpoint.endpoint_type = typeStr;
                _endpoint.endpoint_error = "";

                return _endpoint;
            }
            catch (Exception x)
            {
                try
                {
                    JObject oServerReturn = JObject.Parse(string_to_parse);
                    String nameStr = oServerReturn["endpoint"]["name"].ToString();

                    String adminURLStr = oServerReturn["endpoint"]["adminurl"].ToString();
                    String internalURL = oServerReturn["endpoint"]["internalurl"].ToString();
                    String publicURLStr = oServerReturn["endpoint"]["publicurl"].ToString();

                    //String typeStr = oServerReturn["endpoint"]["type"].ToString();
                    String regionStr = oServerReturn["endpoint"]["region"].ToString();
                    String idStr = oServerReturn["endpoint"]["id"].ToString();


                    _endpoint.admin_url = adminURLStr;
                    _endpoint.internal_url = internalURL;
                    _endpoint.public_url = publicURLStr;
                    _endpoint.region = regionStr;
                    _endpoint.name = nameStr;
                    _endpoint.id = idStr;
                 //   _endpoint.type = typeStr;
                    _endpoint.endpoint_error = "";

                    return _endpoint;
                }
                catch
                {
                     throw new BadJsonException("Json command contained incorrect fields");
                }
            }
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
                                            "\"publicurl\": \"" + public_url  + "/v2.0/" + tenant_id + "\"," +
                                            "\"adminurl\": \"" + admin_url + "/v2.0/" + tenant_id + "\"," +
                                            "\"internalurl\": \"" + internal_url + "/v2.0/" + tenant_id + "\" }}";

            
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create( admin_url + "/v2.0/endpoints");
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
                throw x;
            }

            try
            {

                return Endpoint.Parse(ret);
            }
            catch(Exception x)
            {
                throw x;
            }
        }
}
    
#endregion

#region Endpoint Manager
    public class EndpointManager : OpenStackObject
    {
        public List<Endpoint> endpoint_list;
        public Exception endpoint_manager_error;

        public EndpointManager() {
            base.Type = "Endpoint Manager";
        }

        public void List_Endpoints(string url, string userToken, string AdminToken)
        {
            List<Endpoint> Endpoint_List = new List<Endpoint>();
            string ret = string.Empty;
            try
            {

                ret = base.GET(AdminToken, url + "/tokens/" + userToken + "/endpoints");

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

    #region Service Methods
    //------------------------------------------------
    // by Arnold Yang
    // last updatd on 3:47 PM 6 Dec 2012
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
        public string List(string url, string admin_token)
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


    #region "Exception Objects"


    public class BadJsonException : Exception
    {
        public BadJsonException () : base() { }

        public BadJsonException(string message) : base(message) { }

        public BadJsonException(string message, System.Exception inner) : base(message, inner) { }
      
    }

    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException() : base() { }

        public ObjectNotFoundException(string message) : base(message) { }

        public ObjectNotFoundException(string message, System.Exception inner) : base(message, inner) { }

    }

    public class OpenStackException : OpenStackObject
    {
       // private string exception_number;
        private string exception_description;

        public OpenStackException()
        {
            base.Type = "Exception";
        }

       // private Exception exception;

        public override string ToString()
        {
            return exception_description;
        }

        public static OpenStackException Parse(String exceptionMessage)
        {
            OpenStackException ex = new OpenStackException();

            try
            {

                string[] exceptioninfo  = exceptionMessage.ToString().Split(':');
                //ex.exception_number = exceptioninfo[0];
                ex.exception_description = exceptionMessage;

          //      ex.exception = exceptionMessage;
            }
            catch
            {
               // ex.exception_number = "-";
                ex.exception_description = "OpenStack Library error";

        //        ex.exception = exceptionMessage;
            }

            return ex;
        }


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
            catch(Exception x) 
            {
                // MessageBox.Show("Exception caught: \n" + x.ToString());
                throw x;
            }

       }


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

#region "Deleted Object"
    public class DeletedObject : OpenStackObject
    {
        private string message;


        public DeletedObject()
        {
            base.Type = "Deleted Object";
            message = "Object successfully deleted from database";
        }


        public override string ToString()
        {
            return message;
        }

    }


#endregion

#region Test Functions

    public class OpenStackTests
    {
        public string endpoint_testTenantid = String.Empty;
        public string endpoint_testServiceid = String.Empty;
        public User endpoint_testUser = new User();
        public EndpointManager em = new EndpointManager();


        public Boolean Set_Up_Create_Endpoints_Test(string admin_url, string admin_token, string testTenantName, string testServiceName)
        {
            Boolean ret = true;
            string admin_url2 = admin_url + "/v2.0/";

            string testTenantId = String.Empty;
            string testUserName = "EndpointsTestUser";
            string testUserPw = "eptu123";
            Token testToken;
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
                        testToken = Token.Request_NoTenant(admin_url, testUserName, testUserPw);
                        if (testToken.token_error.Equals(String.Empty))
                        {
                            em = new EndpointManager();
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
            while (em.endpoint_list.Count > 0)
            {
                em.endpoint_list[em.endpoint_list.Count].Delete_Endpoint(admin_url, admin_token);
            }



            Boolean ret = true;
            User.Delete(admin_url, u.id, admin_token);
            ret |= Delete_Test_Service(testServiceId, admin_url +"/v2.0/", admin_token);
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
            if (Test_Endpoint_List(ref em, token, admin_url, admin_token, iterationNumber))
            {
                Endpoint ep = Endpoint.Create_Endpoint(admin_token, token, admin_url, service_id, region, service_id, serviceurl, public_url, tenant_id);
                if (trace == true)
                {
                    output = ep.ToString();
                }
            }
            else
            {
                return false;
            }

            return Test_Endpoint_List(ref em, token, admin_url, admin_token, iterationNumber + 1);
        }


        public bool Test_Endpoint_List(ref EndpointManager em, string token, string admin_url, string admin_token, int iterationNumber)
        {

            em.List_Endpoints(admin_url, token, admin_token);

            if (em.endpoint_manager_error == null)
            {

                return em.endpoint_list.Count == iterationNumber;
            }
            else
            {
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