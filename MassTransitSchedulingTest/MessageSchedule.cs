using MassTransit.Scheduling;
using System;

namespace MassTransitSchedulingTest
{
    public class MessageSchedule : DefaultRecurringSchedule
    {
        public MessageSchedule()
        {
            CronExpression = "0 0/1 * 1/1 * ? *";
            TimeZoneId = TimeZoneInfo.Local.Id;
        }
    }
}
