using System.Net;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using SND.SMP.APIRequestResponses;
using SND.SMP.ItemTrackingEvents;
using SND.SMP.ItemTrackingRetriever.EF;

namespace SND.SMP.ItemTrackingRetriever
{
    public class ItemTrackingRetriever
    {
        public ItemTrackingRetriever() { }

        private static System.Timers.Timer taskTimer; // Define the timer
        private static List<DateTime> taskSchedule;
        private static bool isRunning = false;

        public async Task RetrieveItemTrackingDetails()
        {
            using db db = new();

            var tracking_Interval = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("TrackingIntervalPerDay"));
            var grace_period = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("GracePeriod"));

            int timesToPerform = tracking_Interval is not null ? Convert.ToInt32(tracking_Interval.Value) : 4; // Example: 4 times per day (e.g., every 6 hours)
            int gracePeriodInMinutes = grace_period is not null ? Convert.ToInt32(grace_period.Value) : 2; // Set the grace period in minute(s)

            taskSchedule = ScheduleRemainingTasks(timesToPerform);

            Console.WriteLine("Task Schedule for Today:");
            foreach (var time in taskSchedule)
            {
                Console.WriteLine(time.ToString("hh:mm tt"));
            }

            // Set up the timer to check every minute
            taskTimer = new System.Timers.Timer(60000 * gracePeriodInMinutes); // (60000 ms = 1 minute) x gracePeriodInMinutes
            taskTimer.Elapsed += async (sender, e) => await CheckForScheduledTaskAsync(sender, e, gracePeriodInMinutes, timesToPerform);
            taskTimer.AutoReset = true;
            taskTimer.Start();

            Console.WriteLine("\nTimer started. Waiting for the next scheduled task...");
            Console.ReadLine(); // Keeps the app running to listen to the timer events
        }

        private async Task CheckForScheduledTaskAsync(object sender, ElapsedEventArgs e, int gracePeriodInMinutes, int timesToPerform)
        {
            try
            {
                DateTime currentTime = DateTime.Now;
                DateTime? nextTaskTime = GetNextTaskTime(currentTime, taskSchedule);

                if (nextTaskTime.HasValue)
                {
                    TimeSpan timeUntilNextTask = nextTaskTime.Value - currentTime;
                    if (timeUntilNextTask.TotalMinutes <= gracePeriodInMinutes && timeUntilNextTask.TotalMinutes >= 0 && !isRunning)
                    {
                        await PerformTaskAsync();
                    }
                }
                else
                {
                    DateTime midnight = DateTime.Today.AddDays(1);
                    double timeToMidnight = (midnight - currentTime).TotalMilliseconds;
                    taskTimer.Interval = timeToMidnight;
                    Console.WriteLine("All tasks for today are complete. Waiting until midnight...");
                    taskSchedule = ScheduleRemainingTasks(timesToPerform);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckForScheduledTaskAsync: {ex.Message}");
            }
        }

        private static List<DateTime> ScheduleRemainingTasks(int times)
        {
            int totalMinutesInDay = 1440;
            int interval = totalMinutesInDay / times;

            List<DateTime> taskTimes = [];
            DateTime startTime = DateTime.Today;

            // Generate all task times for the day
            for (int i = 0; i < times; i++)
            {
                DateTime taskTime = startTime.AddMinutes(i * interval);
                // Only add future tasks based on current time
                if (taskTime > DateTime.Now)
                {
                    taskTimes.Add(taskTime);
                }
            }

            return taskTimes;
        }

        private static DateTime? GetNextTaskTime(DateTime currentTime, List<DateTime> schedule)
        {
            foreach (var taskTime in schedule)
            {
                if (taskTime > currentTime)
                {
                    return taskTime;
                }
            }
            return null; // No upcoming tasks today
        }

        private async Task PerformTaskAsync()
        {
            isRunning = true;
            Console.WriteLine($"Task started at {DateTime.Now:hh:mm tt}");
            try
            {
                // Here you can add the specific task you want to perform

                using db db = new();
                var DevEnvironment = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_DevEnvironment"));
                var isDevEnvironment = DevEnvironment.Value.Trim() != "false" && (DevEnvironment.Value.Trim() == "true");
                var ParcelTrackingUrl = isDevEnvironment ? db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_ParcelTrackingUrl_Dev")) : db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_ParcelTrackingUrl_Prod"));
                var token_expiration = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_TokenExpiration"));
                var token = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_Token"));

                List<ItemTrackingEvent> events = [];
                string country = "SA";

                var SA_items_not_delivered = db.Items.Where(x => x.CountryCode.Equals("SA") && x.IsDelivered != 1 && x.DateStage1 != null).ToList();

                string saToken = token.Value.Trim() == "" ? await GetSAToken() : token.Value.Trim();

                if (token.Value.Trim() != "")
                {
                    var dateString = token_expiration.Value.Replace(" UTC", "");
                    var token_expiration_date = DateTime.Parse(dateString);
                    if (token_expiration_date < DateTime.Now) saToken = await GetSAToken();
                }

                var httpstatus = HttpStatusCode.Unauthorized;

                if (ParcelTrackingUrl != null && SA_items_not_delivered.Count > 0)
                {
                    foreach (var item in SA_items_not_delivered)
                    {
                        var saClient = new HttpClient();
                        saClient.DefaultRequestHeaders.Clear();
                        saClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", saToken);

                        InSATrack request = new()
                        {
                            ItemBarCode = item.Id
                        };

                        APIRequestResponse apiRequestResponse = new()
                        {
                            URL = ParcelTrackingUrl.Value.Trim(),
                            RequestBody = JsonConvert.SerializeObject(request),
                            RequestDateTime = DateTime.Now
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        var saRequestMessage = new HttpRequestMessage
                        {
                            Method = HttpMethod.Post,
                            RequestUri = new Uri(ParcelTrackingUrl.Value.Trim()),
                            Content = content,
                        };
                        using var apgResponse = await saClient.SendAsync(saRequestMessage);
                        httpstatus = apgResponse.StatusCode;

                        var saBody = await apgResponse.Content.ReadAsStringAsync();

                        apiRequestResponse.ResponseBody = saBody;
                        apiRequestResponse.ResponseDateTime = DateTime.Now;
                        apiRequestResponse.Duration = (apiRequestResponse.ResponseDateTime - apiRequestResponse.RequestDateTime).Seconds;

                        await db.APIRequestResponses.AddAsync(apiRequestResponse);
                        await db.SaveChangesAsync().ConfigureAwait(false);

                        if (httpstatus == HttpStatusCode.OK)
                        {
                            var saResult = JsonConvert.DeserializeObject<OutSATrack>(saBody);

                            if (saResult != null)
                            {
                                if (saResult.IsSuccess && saResult.ItemsWithStatus != null)
                                {
                                    var latestStatusList = saResult.ItemsWithStatus.OrderByDescending(x => DateTime.Parse(x.CreationDate)).ToList();

                                    var itemTrackingEvents = db.ItemTrackingEvents.Where(x => x.TrackingNo.Equals(item.Id) && x.Country.Equals("SA")).OrderByDescending(y => y.EventTime).ToList();

                                    // Assign latestStatusList to new_statuses if itemTrackingEvents has no item, else filter to get new statuses.
                                    var new_statuses = itemTrackingEvents.Count == 0 ? latestStatusList : latestStatusList.Where(x => itemTrackingEvents.All(y => !y.Status.Equals(x.EventNameEN.Trim()))).ToList();

                                    // Get the latest stored event for incremental event number.
                                    int lasted_event = itemTrackingEvents.Count == 0 ? 0 : itemTrackingEvents.FirstOrDefault().Event;

                                    new_statuses = [.. new_statuses.OrderBy(x => x.CreationDate)];

                                    foreach (var status in new_statuses)
                                    {
                                        DateTime date = DateTime.MinValue;
                                        date = DateTime.Parse(status.CreationDate);

                                        var delivered = status.EventId == "18";

                                        var stage1Date = item.DateStage1;
                                        var statusDesc = status.EventNameEN is null ? "" : status.EventNameEN.Trim();

                                        var dispatch = db.Dispatches.FirstOrDefault(x => x.Id.Equals(item.DispatchId));

                                        lasted_event += 1;
                                        events.Add(new ItemTrackingEvent()
                                        {
                                            TrackingNo = item.Id,
                                            Event = lasted_event,
                                            Status = statusDesc,
                                            Country = country,
                                            EventTime = date,
                                            DispatchNo = dispatch.DispatchNo
                                        });

                                        if (delivered)
                                        {
                                            var daysDelivered = date.Subtract(stage1Date == null ? DateTime.MinValue : stage1Date.Value).TotalDays;
                                            item.DateSuccessfulDelivery = DateTime.Parse(status.CreationDate);
                                            item.DeliveredInDays = Convert.ToInt32(daysDelivered);
                                            item.IsDelivered = 1;

                                            await db.SaveChangesAsync().ConfigureAwait(false);
                                        }
                                    }
                                    await db.ItemTrackingEvents.AddRangeAsync(events);
                                    await db.SaveChangesAsync().ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error caught at {DateTime.Now:hh:mm tt} : {ex.Message.ToString()}");
            }
            finally
            {
                Console.WriteLine($"Task completed at {DateTime.Now:hh:mm tt}");
                isRunning = false;
            }
        }

        private async Task<string> GetSAToken()
        {
            using db db = new();
            var TokenGenerationUrl = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_TokenGenerationUrl"));
            var DevEnvironment = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_DevEnvironment"));
            var isDevEnvironment = DevEnvironment.Value.Trim() != "false" && (DevEnvironment.Value.Trim() == "true");
            var username = isDevEnvironment ? db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_UserName_Dev")) : db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_UserName_Prod"));
            var password = isDevEnvironment ? db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_Password_Dev")) : db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_Password_Prod"));

            var saTokenClient = new HttpClient();
            saTokenClient.DefaultRequestHeaders.Clear();

            var tokenRequest = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("UserName", username.Value.Trim()),
                new KeyValuePair<string, string>("Password", password.Value.Trim()),
                new KeyValuePair<string, string>("grant_type", "password")
            ]);

            APIRequestResponse apiRequestResponse = new()
            {
                URL = TokenGenerationUrl.Value.Trim(),
                RequestBody = JsonConvert.SerializeObject(tokenRequest),
                RequestDateTime = DateTime.Now
            };

            var saTokenRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(TokenGenerationUrl.Value.Trim()),
                Content = tokenRequest,
            };
            using var saTokenResponse = await saTokenClient.SendAsync(saTokenRequestMessage);
            saTokenResponse.EnsureSuccessStatusCode();
            var saTokenBody = await saTokenResponse.Content.ReadAsStringAsync();

            apiRequestResponse.ResponseBody = saTokenBody;
            apiRequestResponse.ResponseDateTime = DateTime.Now;
            apiRequestResponse.Duration = (apiRequestResponse.ResponseDateTime - apiRequestResponse.RequestDateTime).Seconds;

            await db.APIRequestResponses.AddAsync(apiRequestResponse).ConfigureAwait(false);

            var saTokenResult = JsonConvert.DeserializeObject<SATokenResponse>(saTokenBody);

            if (saTokenResult != null)
            {
                var token_expiration = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_TokenExpiration"));
                token_expiration.Value = saTokenResult.expires;

                var token = db.ApplicationSettings.FirstOrDefault(x => x.Name.Equals("SA_Token"));
                token.Value = saTokenResult.token;

                db.ApplicationSettings.Update(token_expiration);
                db.ApplicationSettings.Update(token);
                await db.SaveChangesAsync().ConfigureAwait(false);

                return saTokenResult.token;
            }

            return "";
        }
    }
}
