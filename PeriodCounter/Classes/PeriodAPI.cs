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

        public async Task<PeriodStartTime> GetLast(string token)
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

        public async Task PostNew(PeriodStartTime periodStartTime, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            await _httpClient.PostAsJsonAsync<PeriodStartTime>("/post/newdate", periodStartTime);
        }

        public async Task DeviceRegister(DeviceRegistration registration, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            await _httpClient.PostAsJsonAsync<DeviceRegistration>("/post/newdeviceregistration", registration);
        }
    }
}

