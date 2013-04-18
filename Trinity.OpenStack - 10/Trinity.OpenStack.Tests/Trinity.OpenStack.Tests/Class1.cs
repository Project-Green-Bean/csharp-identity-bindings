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
}
