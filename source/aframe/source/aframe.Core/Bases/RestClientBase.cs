using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using aframe.Models;
using aframe.ViewModels;

namespace aframe
{
    public abstract class RestClientBase :
        IDisposable
    {
        public abstract Uri BaseAddress { get; }

        private readonly HttpClient client = new HttpClient();

        public RestClientBase()
        {
            this.InitializeClient();
        }

        private void InitializeClient()
        {
            this.client.BaseAddress = this.BaseAddress;
            this.client.DefaultRequestHeaders.Accept.Clear();
            this.client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> CreateAsync<T>(
            string uri,
            T data,
            NameValueCollection queryString = null)
        {
            if (queryString != null)
            {
                uri += "?" + queryString.ToString();
            }

            var res = await this.client.PostAsJsonAsync(uri, data);
            res.EnsureSuccessStatusCode();
            return res;
        }

        public async Task<HttpResponseMessage> GetAsync(
            string uri,
            NameValueCollection queryString = null)
        {
            if (queryString != null)
            {
                uri += "?" + queryString.ToString();
            }

            return await this.client.GetAsync(uri);
        }

        public async Task<HttpResponseMessage> UpdateAsync<T>(
            string uri,
            object id,
            T data,
            NameValueCollection queryString = null)
        {
            uri = Path.Combine(uri, id.ToString());
            uri = uri.Replace("\\", "/");

            if (queryString != null)
            {
                uri += "?" + queryString.ToString();
            }

            var res = await this.client.PutAsJsonAsync(uri, data);
            res.EnsureSuccessStatusCode();
            return res;
        }

        public async Task<HttpResponseMessage> DeleteAsync(
            string uri,
            object id,
            NameValueCollection queryString = null)
        {
            uri = Path.Combine(uri, id.ToString());
            uri = uri.Replace("\\", "/");

            if (queryString != null)
            {
                uri += "?" + queryString.ToString();
            }

            var res = await client.DeleteAsync(uri);
            res.EnsureSuccessStatusCode();
            return res;
        }

        public async Task GetVersionAsync()
        {
            try
            {
                var result = await this.GetAsync("version");
                var vi = await result?.Content?.ReadAsAsync<VersionInfoModel>();
                if (vi != null)
                {
                    HelpViewModel.Instance.AddVersionInfos(vi);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public NameValueCollection CreateQueryStringParser()
            => HttpUtility.ParseQueryString(string.Empty);

        public void Dispose()
        {
            this.client?.Dispose();
        }
    }
}
