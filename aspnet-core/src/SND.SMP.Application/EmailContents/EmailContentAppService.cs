using Abp;
using Abp.Application.Services;
using Abp.Extensions;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SND.SMP.EmailContents.Dto;
using System.Net.Mail;
using SND.SMP.ApplicationSettings;
using MimeKit;
using SND.SMP.Authorization.Users;
using System.Reflection;

namespace SND.SMP.EmailContents
{
    public class EmailContentAppService(IRepository<EmailContent, int> repository,
    IRepository<ApplicationSetting, int> applicationSettingRepository, UserManager userManager) : AsyncCrudAppService<EmailContent, EmailContentDto, int, PagedEmailContentResultRequestDto>(repository)
    {
        private readonly IRepository<ApplicationSetting, int> _applicationSettingRepository = applicationSettingRepository;
        private readonly UserManager _userManager = userManager;
        protected override IQueryable<EmailContent> CreateFilteredQuery(PagedEmailContentResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Name.Contains(input.Keyword) ||
                    x.Subject.Contains(input.Keyword) ||
                    x.Content.Contains(input.Keyword));
        }

        public async Task<bool> SendPreAlertFailureEmailAsync(PreAlertFailureEmail input)
        {
            try
            {
                Type type = input.GetType();

                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var applicationSettings = await _applicationSettingRepository.GetAllListAsync();

                var emailContent = await Repository.FirstOrDefaultAsync(x => x.Name.Equals("Pre-Alert Failure For Executives"));

                string subject = emailContent.Subject;
                string content = emailContent.Content;

                string emailHost = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailHost")).Value;
                string emailFrom = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFrom")).Value;
                string emailFromPassword = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFromPassword")).Value;
                string emailFromPort = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFromPort")).Value;
                string emailDisplayName = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailDisplayName")).Value;
                string bcc = applicationSettings.FirstOrDefault(x => x.Name.Equals("BCC")).Value;

                string[] bccEmailList = [];
                if (!string.IsNullOrWhiteSpace(bcc)) bccEmailList = bcc.Split(",");

                SmtpClient smtpClient = new()
                {
                    Host = emailHost,
                    Port = Convert.ToInt16(emailFromPort),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(emailFrom, emailFromPassword)
                };

                MailMessage mail = new()
                {
                    From = new MailAddress(emailFrom, emailDisplayName)
                };

                var users = await _userManager.GetUsersInRoleAsync("Email");
                foreach (var user in users) mail.To.Add(new MailAddress(user.EmailAddress));
                if (bccEmailList.Length > 0)
                {
                    foreach (var bccEmail in bccEmailList) mail.Bcc.Add(bccEmail);
                }
                foreach (var property in properties)
                {
                    string fieldNamePlaceholder = "{" + property.Name + "}";

                    if (subject.Contains(fieldNamePlaceholder))
                    {
                        string fieldValue = property.GetValue(input)?.ToString();
                        subject = subject.Replace(fieldNamePlaceholder, fieldValue);
                    }

                    if (content.Contains(fieldNamePlaceholder))
                    {
                        string fieldValue = property.GetValue(input)?.ToString();
                        content = content.Replace(fieldNamePlaceholder, fieldValue);
                    }
                }

                content += $"<br /><h1><strong>Error(s)</strong></h1>";

                int count = 1;
                foreach (var validation in input.validations)
                {
                    content += $"&nbsp;<h2><u>{count}.{validation.Category}</u></h2>";
                    content += validation.Message == "" ? "" : $"&nbsp;&nbsp;<h4><strong>{validation.Message}</strong></h4>";

                    if (validation.ItemIds.Count > 0)
                    {
                        foreach (var item in validation.ItemIds)
                        {
                            content += $"&nbsp;&nbsp;&nbsp;<p>{item}</p>";
                        }
                    }

                    count++;
                }

                mail.Subject = subject;
                mail.Body = content;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;

                smtpClient.Send(mail);
                smtpClient.Dispose();


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendPreAlertSuccessEmailAsync(PreAlertSuccessEmail input)
        {
            try
            {
                Type type = input.GetType();

                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                var applicationSettings = await _applicationSettingRepository.GetAllListAsync();

                var emailContent = await Repository.FirstOrDefaultAsync(x => x.Name.Equals("Pre-Alert Success For Executives"));

                string subject = emailContent.Subject;
                string content = emailContent.Content;

                string emailHost = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailHost")).Value;
                string emailFrom = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFrom")).Value;
                string emailFromPassword = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFromPassword")).Value;
                string emailFromPort = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailFromPort")).Value;
                string emailDisplayName = applicationSettings.FirstOrDefault(x => x.Name.Equals("EmailDisplayName")).Value;
                string bcc = applicationSettings.FirstOrDefault(x => x.Name.Equals("BCC")).Value;

                string[] bccEmailList = [];
                if (!string.IsNullOrWhiteSpace(bcc)) bccEmailList = bcc.Split(",");

                SmtpClient smtpClient = new()
                {
                    Host = emailHost,
                    Port = Convert.ToInt16(emailFromPort),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(emailFrom, emailFromPassword)
                };

                MailMessage mail = new()
                {
                    From = new MailAddress(emailFrom, emailDisplayName)
                };

                var users = await _userManager.GetUsersInRoleAsync("Email");
                foreach (var user in users) mail.To.Add(new MailAddress(user.EmailAddress));
                if (bccEmailList.Length > 0)
                {
                    foreach (var bccEmail in bccEmailList) mail.Bcc.Add(bccEmail);
                }
                foreach (var property in properties)
                {
                    string fieldNamePlaceholder = "{" + property.Name + "}";

                    if (subject.Contains(fieldNamePlaceholder))
                    {
                        string fieldValue = property.GetValue(input)?.ToString();
                        subject = subject.Replace(fieldNamePlaceholder, fieldValue);
                    }

                    if (content.Contains(fieldNamePlaceholder))
                    {
                        string fieldValue = property.GetValue(input)?.ToString();
                        content = content.Replace(fieldNamePlaceholder, fieldValue);
                    }
                }
                mail.Subject = subject;
                mail.Body = content;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;

                smtpClient.Send(mail);
                smtpClient.Dispose();


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
