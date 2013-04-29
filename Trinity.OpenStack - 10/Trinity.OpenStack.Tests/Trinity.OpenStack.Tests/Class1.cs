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

using Trinity.OpenStack;


namespace Trinity.OpenStack.Tests
{
    #region EndpointTests
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

           try{
               Create_Test_Tenant(ref testTenantId, testTenantName, admin_url2, admin_token);                       //Create Tenant
            
                endpoint_testTenantid = testTenantId;
                try {
                    Create_Test_Service(ref testServiceId, testServiceName, admin_url2, admin_token);                   //Create Service
               
                    endpoint_testServiceid = testServiceId;
                    User u = new User();
                    try{
                        u = User.Add(admin_url, testUserName, testUserPw, "true", testTenantId, "null", admin_token);

                        endpoint_testUser = u;
                        EPTestToken = Token.Request_NoTenant(admin_url, testUserName, testUserPw);
                       if (EPTestToken.token_error.Equals(String.Empty))
                        {
                            em = new List<Endpoint>();
                            em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                            BaseCount = em.Count;
                            return true;
                        }
                        else
                        {
                            Tear_Down_Create_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                            return false;
                        }

                    }
                    catch (Exception x)
                    {
                        try
                        {
                            Tear_Down_Create_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                        }
                        catch
                        {

                            Delete_Test_Service(testServiceId, admin_url2, admin_token);
                            Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                        }
                        throw x;
                    }
                }
                catch(Exception x)
                {
                    Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
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
            try
            {
                User.Delete(admin_url, u.id, admin_token);
            }
            catch
            { }
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
            try {
                Test_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber);
            
                Endpoint ep = Endpoint.Create_Endpoint(admin_token, admin_token, admin_url, service_id, region + iterationNumber, service_id, serviceurl, public_url, tenant_id);
                DisposableEndpoints.Add(ep);
                if (trace == true)
                {
                    output = ep.ToString();
                }
            }
            catch (Exception x)
            {
               throw x;
            }

            return Test_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber + 1);
        }


        public bool Test_Endpoint_List(ref List<Endpoint> em, string token, string admin_url, string admin_token, int iterationNumber)
        {
            try
            {
                em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                return em.Count == (iterationNumber + BaseCount);
            }
            catch (Exception x)
            {
               throw x;
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


    public class TestDeleteEndpoint
    {
        public string endpoint_testTenantid = String.Empty;
        public string endpoint_testServiceid = String.Empty;
        public User endpoint_testUser = new User();
        public List<Endpoint> em = new List<Endpoint>();
        public Token EPTestToken;
        public  List<Endpoint> DisposableEndpoints;
        protected int BaseCount = 0;

        public Boolean Set_Up_Delete_Endpoints_Test(string admin_url, string admin_token, string testTenantName, string testServiceName)
        {
            Boolean ret = true;
            string admin_url2 = admin_url + "/v2.0/";

            DisposableEndpoints = new List<Endpoint>();
            string testTenantId = String.Empty;
            string testUserName = "EndpointsTestUser";
            string testUserPw = "eptu123";
            string testServiceId = String.Empty;

          try
            {
                Create_Test_Tenant(ref testTenantId, testTenantName, admin_url2, admin_token);                       //Create Tenant

                endpoint_testTenantid = testTenantId;
                try
                {
                    Create_Test_Service(ref testServiceId, testServiceName, admin_url2, admin_token);                   //Create Service

                    endpoint_testServiceid = testServiceId;
                    User u = new User();
                    try
                    {
                        u = User.Add(admin_url, testUserName, testUserPw, "true", testTenantId, "null", admin_token);

                        endpoint_testUser = u;
                        EPTestToken = Token.Request_NoTenant(admin_url, testUserName, testUserPw);
                        if (EPTestToken.token_error.Equals(String.Empty))
                        {
                            em = new List<Endpoint>();
                            em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                            BaseCount = em.Count;

                            try
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    Endpoint ep = Endpoint.Create_Endpoint(admin_token, admin_token, admin_url, testServiceId, "testDeleteEndpoint111213_" + i, testServiceId, admin_url + ":5000", admin_url + ":5000", testTenantId);
                                    DisposableEndpoints.Add(ep);
                                }
                            }
                            catch (Exception x)
                            {
                                Tear_Down_Delete_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                                return false;
                            }

                            return true;
                        }
                        else
                        {
                            Tear_Down_Delete_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                            return false;
                        }

                    }
                    catch (Exception x)
                    {
                        try
                        {
                            Tear_Down_Delete_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                        }
                        catch
                        {

                            Delete_Test_Service(testServiceId, admin_url2, admin_token);
                            Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                        }
                        throw x;
                    }
                }
                catch (Exception x)
                {
                    Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return true;

        }


        public Boolean Tear_Down_Delete_Endpoints_Test(string admin_url, string admin_token, User u, string testServiceId, string testTenantId)
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

        public Boolean Run_Test_Delete_Endpoints(string admin_url, string admin_token, string token,  int iterationNumber)
        {
            try 
            {
                Test_Delete_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber);
                int i = 0;
                Endpoint endp = em[i];

                while (i < em.Count)
                {
                    if (endp.region.Equals(DisposableEndpoints[i].region))
                    {
                        endp.Delete_Endpoint(admin_url, admin_token);
                        break;
                    }
                    i += 1;
                }
               
                
            }
            catch (Exception x)
            {
                throw x;
            }

            return Test_Delete_Endpoint_List(ref em, admin_token, admin_url, admin_token, iterationNumber + 1);


            

        }


        public bool Test_Delete_Endpoint_List(ref List<Endpoint> em, string token, string admin_url, string admin_token, int iterationNumber)
        {
            try
            {
                em = Endpoint.List_Endpoints(admin_url, admin_token, admin_token);
                return em.Count == ( BaseCount + (10- iterationNumber));
            }
            catch (Exception x)
            {
                throw x;
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


    public class TestEmptyListEndPoint
    {

        public string endpoint_testTenantid = String.Empty;
        public string endpoint_testServiceid = String.Empty;
        public User endpoint_testUser = new User();
        public List<Endpoint> em = new List<Endpoint>();
        public Token EPTestToken;
        protected int BaseCount = 0;

        public Boolean Set_Up_EmptyList_Endpoints_Test(string admin_url, string admin_token, string testTenantName, string testServiceName)
        {
            Boolean ret = true;
            string admin_url2 = admin_url + "/v2.0/";


            string testTenantId = String.Empty;
            string testUserName = "EndpointsTestUser";
            string testUserPw = "eptu123";
            string testServiceId = String.Empty;

            try
            {
                Create_Test_Tenant(ref testTenantId, testTenantName, admin_url2, admin_token);                       //Create Tenant

                endpoint_testTenantid = testTenantId;
                try
                {
                    Create_Test_Service(ref testServiceId, testServiceName, admin_url2, admin_token);                   //Create Service

                    endpoint_testServiceid = testServiceId;
                    User u = new User();
                    try
                    {
                        u = User.Add(admin_url, testUserName, testUserPw, "true", testTenantId, "null", admin_token);

                        endpoint_testUser = u;
                        EPTestToken = Token.Request_NoTenant(admin_url, testUserName, testUserPw);
                        if (EPTestToken.token_error.Equals(String.Empty))
                        {
                            em = new List<Endpoint>();
                            em = Endpoint.List_Endpoints(admin_url, EPTestToken.token_id, admin_token);
                            return true;
                        }
                        else
                        {
                            Tear_Down_EmptyList_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                            return false;
                        }

                    }
                    catch (Exception x)
                    {
                        try
                        {
                            Tear_Down_EmptyList_Endpoints_Test(admin_url, admin_token, u, testServiceId, testTenantId);
                        }
                        catch
                        {

                            Delete_Test_Service(testServiceId, admin_url2, admin_token);
                            Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                        }
                        throw x;
                    }
                }
                catch (Exception x)
                {
                    Delete_Test_Tenant(testTenantId, admin_url2, admin_token);
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return true;
        }


        public Boolean Tear_Down_EmptyList_Endpoints_Test(string admin_url, string admin_token, User u, string testServiceId, string testTenantId)
        {
          

            Boolean ret = true;
            try
            {
                User.Delete(admin_url, u.id, admin_token);
            }
            catch
            { }
            ret |= Delete_Test_Service(testServiceId, admin_url + "/v2.0/", admin_token);
            ret |= Delete_Test_Tenant(testTenantId, admin_url + "/v2.0/", admin_token);
            if (ret == true)
            {
                endpoint_testServiceid = String.Empty;
                endpoint_testTenantid = String.Empty;
            }
            return ret;

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
    #region userTests
    public class TestAddUser
    {
        public string testTenantID = String.Empty;
        public List<User> users = new List<User>();
        public List<User> disposableUsers;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName)
        {
            disposableUsers = new List<User>();
            try
            {
                users = User.List(admin_url, admin_token);
                return Create_Test_Tenant(testTenantName, admin_url, admin_token, ref testTenantID);
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            try
            {
                while (disposableUsers.Count > 0)
                {
                    User.Delete(admin_url, disposableUsers[0].id, admin_token);
                    disposableUsers.RemoveAt(0);
                }
            }
            catch { }

            users = User.List(admin_url, admin_token);

            Delete_Test_Tenant(admin_url, admin_token, testTenantID);
            testTenantID = String.Empty;

            return true;
        }

        public Boolean run(string admin_url, string admin_token, string name, string password, string email)
        {
            try
            {
                User u = User.Add(admin_url, name, password, "true", testTenantID, email, admin_token);
                List<User> newUsers = User.List(admin_url, admin_token);
                if (newUsers.Count == users.Count+1) {
                    users = newUsers;
                    disposableUsers.Add(u);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception x)
            {
                tearDown(admin_url, admin_token);
                throw x;
            }
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token, ref string tenID)
        {
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants");
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
                tenID = ret["tenant"]["id"].ToString();

                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token, string tenID)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants/" + tenID);
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
    }

    public class TestDeleteUser
    {
        public string testTenantID = String.Empty;
        public List<User> users = new List<User>();
        protected List<User> disposableUsers;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName)
        {
            disposableUsers = new List<User>();
            try
            {
                Create_Test_Tenant(testTenantName, admin_url, admin_token, ref testTenantID);
                try
                {
                    for (int i = 0; i < 10; ++i)
                        disposableUsers.Add(User.Add(admin_url, "TestUser_" + i, "testPass" + i, "true", testTenantID, "email" + i + "@email.com", admin_token));
                    users = User.List(admin_url, admin_token);
                    return true;
                }
                catch (Exception x)
                {
                    Delete_Test_Tenant(admin_url, admin_token, testTenantID);
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            try
            {
                while (disposableUsers.Count > 0)
                {
                    User.Delete(admin_url, disposableUsers[0].id, admin_token);
                    disposableUsers.RemoveAt(0);
                }
            }
            catch { }

            users = User.List(admin_url, admin_token);

            Delete_Test_Tenant(admin_url, admin_token, testTenantID);
            testTenantID = String.Empty;

            return true;
        }

        public Boolean run(string admin_url, string admin_token, string userID)
        {
            try
            {
                int usersCount = users.Count;

                User.Delete(admin_url, userID, admin_token);
                users = User.List(admin_url, admin_token);

                if (users.Count == usersCount-1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception x)
            {
                tearDown(admin_url, admin_token);
                throw x;
            }
        }

        public List<User> getDisposable()
        {
            return disposableUsers;
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token, ref string tenID)
        {
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants");
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
                tenID = ret["tenant"]["id"].ToString();

                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token, string tenID)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants/" + tenID);
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
    }

    public class TestAddUserRole
    {
        protected List<Role> roles;
        public List<Role> disposableRoles;
        public List<Role> userRoles;
        protected User roleTestUser;
        public string testTenantId = String.Empty;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName)
        {
            try
            {
                Create_Test_Tenant(testTenantId, admin_url, admin_token, ref testTenantId);
                try
                {
                    roleTestUser = User.Add(admin_url, "roleTestUser", "password", "true", testTenantId, "email@email.email", admin_token);
                    return true;
                }
                catch (Exception x)
                {
                    Delete_Test_Tenant(admin_url, admin_token, testTenantId);
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            try
            {
                //delete all test roles
                while (disposableRoles.Count > 0)
                {
                    User.DeleteRoleOfUser(admin_url, roleTestUser.id, testTenantId, disposableRoles[0].id, admin_token);
                    Role.Delete(admin_url, disposableRoles[0].id, admin_token);
                    disposableRoles.RemoveAt(0);
                }
            }
            catch { }
            Boolean ret = true;

            //delete test user
            try
            {
                User.Delete(admin_url, roleTestUser.id, admin_token);
            }
            catch { }

            //delete test tenant
            ret |= Delete_Test_Tenant(admin_url, admin_token, testTenantId);
            if (ret == true)
            {
                testTenantId = String.Empty;
            }
            return ret;
        }

        public Boolean run(string admin_url, string admin_token, string roleName)
        {
            try
            {
                Role r = Role.Add(admin_url, roleName, admin_token);
                User.AddRoleToUser(admin_url, roleTestUser.id, roleTestUser.tenantid, r.id, admin_token);
                disposableRoles.Add(r);
                if (disposableRoles.Count == userRoles.Count + 1)
                {
                    userRoles = User.List_Roles(admin_url, roleTestUser.id, testTenantId, admin_token);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception x)
            {
                tearDown(admin_url, admin_token);
                throw x;
            }
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token, ref string tenID)
        {
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants");
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
                tenID = ret["tenant"]["id"].ToString();

                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token, string tenID)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants/" + tenID);
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
    }

    public class TestDeleteUserRole
    {
        public List<Role> disposableRoles;
        public Token roleTestToken;
        User roleTestUser;
        public string role_testTenantid = String.Empty;
        protected int BaseCount = 0;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName, int iterations)
        {
            try
            {
                //create test tenant
                Create_Test_Tenant(testTenantName, admin_url + "/v2.0/", admin_token);

                try
                {
                    //create test user
                    string testUserName = "RoleAddTestUser";
                    string testUserPass = "userpass1";
                    roleTestUser = User.Add(admin_url, testUserName, testUserPass, "true", role_testTenantid, "null", admin_token);

                    roleTestToken = Token.Request_NoTenant(admin_url, roleTestUser.name, roleTestUser.password);
                    if (roleTestToken.token_error.Equals(String.Empty))
                    {
                        //initialize list of test user roles  
                        disposableRoles = User.List_Roles(admin_url, roleTestUser.id, role_testTenantid, admin_token);
                        BaseCount = disposableRoles.Count;

                        try
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                Role r = Role.Add(admin_url, "UserAddRoleTest" + i, admin_token);
                                User.AddRoleToUser(admin_url, roleTestUser.id, roleTestUser.tenantid, r.id, admin_token);
                                disposableRoles.Add(r);
                            }
                        }
                        catch (Exception x)
                        {
                            tearDown(admin_url, admin_token);
                            return false;
                        }
                    }
                    else
                    {
                        tearDown(admin_url, admin_token);
                    }
                }
                catch (Exception x)
                {
                    try
                    {
                        tearDown(admin_url, admin_token);
                    }
                    catch
                    {
                        Delete_Test_Tenant(admin_url + "/v2.0/", admin_token);
                    }
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return true;
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            try
            {
                //delete all test roles
                while (disposableRoles.Count > 0)
                {
                    User.DeleteRoleOfUser(admin_url, roleTestUser.id, role_testTenantid, disposableRoles[0].id, admin_token);
                    Role.Delete(admin_url, disposableRoles[0].id, admin_token);
                    disposableRoles.RemoveAt(0);
                }
            }
            catch
            {
                //do nothing
            }
            Boolean ret = true;

            //delete test user
            try
            {
                User.Delete(admin_url, roleTestUser.id, admin_token);
            }
            catch { }

            //delete test tenant
            ret |= Delete_Test_Tenant(admin_url + "/v2.0/", admin_token);
            if (ret == true)
            {
                role_testTenantid = String.Empty;
            }
            return ret;
        }

        public Boolean run(string admin_url, string admin_token)
        {
            try
            {
                Test_Delete_User_Role_List(admin_url, admin_token, disposableRoles.Count, 0);

                for (int i = 0; i < disposableRoles.Count; ++i)
                {
                    Role r = disposableRoles[i];
                    User.DeleteRoleOfUser(admin_url, roleTestUser.id, roleTestUser.tenantid, r.id, admin_token);
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return Test_Delete_User_Role_List(admin_url, admin_token, 0, 0);
        }

        public bool Test_Delete_User_Role_List(string admin_url, string admin_token, int iterations, int iterationNumber)
        {
            try
            {
                disposableRoles = User.List_Roles(admin_url, roleTestUser.id, roleTestUser.tenantid, admin_token);
                return disposableRoles.Count == (BaseCount + (iterations - iterationNumber));
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token)
        {
            StreamWriter requestWriter;

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
                role_testTenantid = ret["tenant"]["id"].ToString();


                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "tenants/" + role_testTenantid);
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
    }

    public class TestUpdateUser
    {
        public User updateTestUser;
        List<User> updates = new List<User>();
        public string testTenantId = String.Empty;
        public string testTenantId2 = String.Empty;
        public string testUserID = String.Empty;
        public string updateTestUserName = String.Empty;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName, string testTenantName2)
        {
            try
            {
                Create_Test_Tenant(testTenantName, admin_url, admin_token, ref testTenantId);
                Create_Test_Tenant(testTenantName2, admin_url, admin_token, ref testTenantId2);
                try
                {
                    string testUserName = "updateTestUser";
                    string testUserPass = "userpass1";
                    updateTestUserName = testUserName;
                    updateTestUser = User.Add(admin_url, testUserName, testUserPass, "true", testTenantId, "null", admin_token);
                    updates = new List<User>();
                    updateTestUserName = updateTestUser.name;
                    testUserID = updateTestUser.id;
                }
                catch (Exception x)
                {
                    try
                    {
                        tearDown(admin_url, admin_token);
                    }
                    catch
                    {
                        Delete_Test_Tenant(admin_url, admin_token, testTenantId);
                        Delete_Test_Tenant(admin_url, admin_token, testTenantId2);
                    }
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return true;
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            Boolean ret = true;

            //delete test user
            try
            {
                User.Delete(admin_url, updateTestUser.id, admin_token);
            }
            catch { }

            //delete test tenant
            ret |= Delete_Test_Tenant(admin_url, admin_token, testTenantId);
            ret |= Delete_Test_Tenant(admin_url, admin_token, testTenantId2);
            if (ret == true)
            {
                testTenantId = String.Empty;
            }
            return ret;
        }

        public Boolean run(string admin_url, string admin_token, string userID, string username, string email, string enabled, string tenantID)
        {
            User update = new User();
            update.name = updateTestUser.name;
            update.id = updateTestUser.id;
            update.email = updateTestUser.email;
            update.enabled = updateTestUser.enabled;
            update.tenantid = updateTestUser.tenantid;
            update.password = updateTestUser.password;

            try
            {
                bool eq = testUserID == updateTestUser.id;
                update = User.Update(admin_token, userID, userID, username, email, enabled, tenantID, admin_url);
                updateTestUser = User.GetUserById(admin_url, admin_token, userID);
                return compareUpdateAndNew(update);
            }
            catch (Exception x)
            {
                tearDown(admin_url, admin_token);
                throw x;
            }

        }

        public bool compareUpdateAndNew(User u)
        {
            if (u.name != updateTestUser.name)
                return false;

            if (u.email != updateTestUser.email)
                return false;

            if (u.enabled != updateTestUser.enabled)
                return false;

            if (u.tenantid != updateTestUser.tenantid)
                return false;

            return true;
        }

        public User getTestUser()
        {
            User ret = updateTestUser;
            return ret;
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token, ref string tenID)
        {
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants");
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
                tenID = ret["tenant"]["id"].ToString();

                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token, string tenID)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants/" + tenID);
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

        public User test_Update(string admin_token, string old_id, string new_id, string UserName, string Email,
                                        string Enabled, string TenantID, string url)
        {
            string update_url = url + "/v2.0/users/" + old_id;

            string ret = string.Empty;
            User updated_user = new User();

            string postData = "{ " +
                               "\"user\": { " +
                                    "\"id\": \"" + new_id + "\"," +
                                    "\"name\": \"" + UserName + "\"," +
                                    "\"email\": \"" + Email + "\"," +
                                    "\"enabled\":" + Enabled + "," +
                                    "\"tenantId\":\"" + TenantID + "\"" +
                                "}" +
                                "}";

            StreamWriter requestWriter;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(update_url);
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

                //parse the return to extract the variables.
                JObject oServerReturn = JObject.Parse(ret);
                String userStr = oServerReturn["user"].ToString();

                JObject oUser = JObject.Parse(userStr);
                string user_id = oUser["id"].ToString();
                string user_name = oUser["name"].ToString();
                string extra_string = oUser["extra"].ToString();

                JObject oExtra = JObject.Parse(extra_string);
                string user_password = oExtra["password"].ToString();
                string user_enabled = oExtra["enabled"].ToString();
                string user__email = oExtra["email"].ToString();
                string user_tenantId = oExtra["tenantId"].ToString();

                // load the variables into a user object
                updated_user.id = user_id;
                updated_user.name = user_name;
                updated_user.enabled = user_enabled;
                updated_user.email = user__email;
                updated_user.tenantid = user_tenantId;
                updated_user.error = "";

                // return a User object
                return (updated_user);
            }
            catch (Exception x)
            {
                throw OpenStackObject.Parse_Error(x);
            }
        }
    }

    public class TestGetUser
    {
        public User getTestUser;
        public User user = new User();
        public string testTenantId = String.Empty;

        public Boolean setUp(string admin_url, string admin_token, string testTenantName)
        {
            try
            {
                Create_Test_Tenant(testTenantName, admin_url, admin_token, ref testTenantId);
                try
                {
                    string testUserName = "updateTestUser";
                    string testUserPass = "userpass1";
                    getTestUser = User.Add(admin_url, testUserName, testUserPass, "true", testTenantId, "null", admin_token);
                }
                catch (Exception x)
                {
                    try
                    {
                        tearDown(admin_url, admin_token);
                    }
                    catch
                    {
                        Delete_Test_Tenant(admin_url, admin_token, testTenantId);
                    }
                    throw x;
                }
            }
            catch (Exception x)
            {
                throw x;
            }
            return true;
        }

        public Boolean tearDown(string admin_url, string admin_token)
        {
            try
            {
                User.Delete(admin_url, getTestUser.id, admin_token);
            }
            catch { }

            Delete_Test_Tenant(admin_url, admin_token, testTenantId);
            testTenantId = String.Empty;

            return true;
        }

        public Boolean run(string admin_url, string admin_token, string userID)
        {
            try
            {
                user = User.GetUserById(admin_url, admin_token, userID);
                return true;
            }
            catch (Exception x)
            {
                tearDown(admin_url, admin_token);
                throw x;
            }
        }

        private bool Create_Test_Tenant(string tenantName, string admin_url, string admin_token, ref string tenID)
        {
            StreamWriter requestWriter;

            string postData = "{" +
                                "\"tenant\":{" +
                                            "\"name\":\"" + tenantName + "\", " +
                                            "\"description\":\"" + "Delete if still present" + "\", " +
                                            "\"enabled\":" + "true" +
                                            "}}";


            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants");
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
                tenID = ret["tenant"]["id"].ToString();

                return true;
            }
            catch (Exception x)
            {
                return false;
            }

        }

        private bool Delete_Test_Tenant(string admin_url, string admin_token, string tenID)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(admin_url + "/v2.0/tenants/" + tenID);
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
    }

    public class TestListUser
    {
        public List<User> users = new List<User>();

        public Boolean run(string admin_url, string admin_token)
        {
            try
            {
                users = User.List(admin_url, admin_token);
                return true;
            }
            catch (Exception x)
            {
                throw x;
            }
        }
    }
    #endregion
}
