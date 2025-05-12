using Firebase.Auth;
using PeriodLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PeriodCounter.Classes
{
    public class PeriodAPI()
    {
        private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(Constants.RootApiAddress)};

        public async Task<List<PeriodStartTime>?> GetAll(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            List<PeriodStartTime>? startTime = await _httpClient.GetFromJsonAsync<List<PeriodStartTime>>($"/get/all");

            if (null != startTime)
            {
                return startTime;
            }
            else
            {
                return null;
            }
        }

        public async Task<PeriodStartTime?> GetLast(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                PeriodStartTime? startTime = await _httpClient.GetFromJsonAsync<PeriodStartTime>($"/get/lastsubmitdate");

                if (null != startTime)
                {
                    return startTime;
                }
                else
                {
                    throw new Exception("null post");
                }
            }
            catch { }

            return null;
        }

        public async Task PostNew(PeriodStartTime periodStartTime, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await _httpClient.PostAsJsonAsync<PeriodStartTime>("/post/newdate", periodStartTime);
            }
            catch { }
        }

        public async Task DeviceRegister(DeviceRegistration registration, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                await _httpClient.PostAsJsonAsync<DeviceRegistration>("/post/newdeviceregistration", registration);
            }
            catch { }
        }
    }
}

