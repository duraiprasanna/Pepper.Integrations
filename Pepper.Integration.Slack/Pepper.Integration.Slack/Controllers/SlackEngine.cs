using Pepper.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pepper.Models.CodeFirst.Enums;
using Pepper.Framework.Slack;
using Pepper.Framework.Validation;
using Pepper.Framework.Helpers;

namespace Pepper.Integration.Slack.Controllers
{
    class SlackEngine
    {
        public static void Init()
        {
            List<IntegrationJob> slackJob = GetSlackJob();
            Console.WriteLine("slackJob Length: " + slackJob.Count);
            slackJob.ForEach(job =>
            {
                List<AuditLog> auditLog = GetAuditLog(job.LastProcessedToRecordIdentifier);
                Console.WriteLine("auditLog Length: " + auditLog.Count);
                auditLog.ForEach(aLog =>
                {
                    List<AppStoreEntityAppConfiguration> configs = GetConfig(aLog.EntityID);
                    Console.WriteLine("configs Length: " + configs.Count);
                    configs.ForEach(config =>
                    {
                        if (config.AppStoreEntityAppConfiguration1 != "" && config.AppStoreEntityAppConfiguration1 != null)
                        {
                            SlackEntityConfiguration slackEntityConfiguration = JsonConvert.DeserializeObject<SlackEntityConfiguration>(config.AppStoreEntityAppConfiguration1);
                            if (slackEntityConfiguration.access_token != "")
                            {
                                List<int> postConfigDetails = slackEntityConfiguration.postNotification.Split(',').Select(x => Int32.Parse(x)).ToList();
                                if (postConfigDetails.IndexOf(aLog.AuditLogSourceID) != -1)
                                {
                                    USER user = GetUserName((int)aLog.AuditLogUserID);
                                    SlackClient client = new SlackClient(slackEntityConfiguration.incoming_webhook.url);
                                    List<AuditChange> auditChanges = JsonConvert.DeserializeObject<List<AuditChange>>(aLog.AuditEntries);
                                    string msg = user.FirstName + " " + user.LastName + " modified ";
                                    auditChanges.ForEach(change =>
                                    {
                                        msg += change.PropertyName + ".";
                                        change.Values.ForEach(value =>
                                        {
                                            msg += " OldValue: " + value.OldValue + ", New Value: " + value.NewValue + ". ";
                                        });
                                    });

                                    client.PostMessage(msg);
                                    job.LastProcessedToRecordIdentifier = aLog.AuditLogID;
                                }
                            }
                        }
                    });

                });

                IEnumerable<ErrorInfo> errorInfo = job.Save();
            });
        }

        public static List<IntegrationJob> GetSlackJob()
        {
            List<IntegrationJob> slackJob = new List<IntegrationJob>();
            using (PepperContext context = new PepperContext())
            {
                try
                {
                    slackJob = (from q in context.IntegrationJobs
                                where q.JobId == (int)JobIds.Slack
                                select q).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return slackJob;
        }

        public static List<AuditLog> GetAuditLog(int lastProcessedToRecordIdentifier)
        {
            List<AuditLog> auditLog = new List<AuditLog>();
            using (PepperContext context = new PepperContext())
            {
                auditLog = (from q in context.AuditLogs
                            where q.AuditLogID > lastProcessedToRecordIdentifier
                            select q).ToList();
            }
            return auditLog;
        }

        public static List<AppStoreEntityAppConfiguration> GetConfig(int entityID)
        {
            List<AppStoreEntityAppConfiguration> config = new List<AppStoreEntityAppConfiguration>();
            using (PepperContext context = new PepperContext())
            {
                config = (from aec in context.AppStoreEntityAppConfigurations
                          join asea in context.AppStoreEntityApps
                          on aec.AppStoreEntityAppID equals asea.AppStoreEntityAppID
                          where asea.EntityID == entityID
                          && asea.IsEnabled == true && asea.AppStoreAppID == (int)AppStoreApplicationType.Slack
                          select aec).ToList();
            }
            return config;
        }

        public static USER GetUserName(int userID)
        {
            USER user = new USER();
            using (PepperContext context = new PepperContext())
            {
                user = (from row in context.USERs
                        where row.UserID == userID
                        select row).FirstOrDefault();
            }

            return user;
        }

    }

}
