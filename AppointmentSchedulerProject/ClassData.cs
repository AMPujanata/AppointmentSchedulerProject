using dotenv.net;
using MongoDB.Driver;

namespace AppointmentSchedulerProject
{
    public static class MongoData
    {
        public static readonly MongoClient ConnectionClient = new(DotEnv.Read()["MONGODB_URI"]);
    }

    public class UserInfo()
    {
        public MongoDB.Bson.ObjectId Id; // MongoDB will automatically handle unique IDs
        public string? Username; // This should always be unique for each user
        public string? Realname;
        public int TimezoneOffset;
    }
    
    public class TimezoneInfo(string name, int offset)
    {
        public string TimezoneName = name;
        public int TimezoneOffset = offset;
    };

    public static class TimezoneHelper
    {
        public static List<TimezoneInfo> GetAllTimezones()
        {
            List<TimezoneInfo> allTimezones = [];

            allTimezones.Add(new TimezoneInfo("Hawaii", -10));
            allTimezones.Add(new TimezoneInfo("Alaska", -9));
            allTimezones.Add(new TimezoneInfo("Pacific Time", -8));
            allTimezones.Add(new TimezoneInfo("Mountain Time", -7));
            allTimezones.Add(new TimezoneInfo("Central Time", -6));
            allTimezones.Add(new TimezoneInfo("Eastern Time", -5));
            allTimezones.Add(new TimezoneInfo("Atlantic Time", -4));
            allTimezones.Add(new TimezoneInfo("Brasilia", -3));
            allTimezones.Add(new TimezoneInfo("Greenland", -2));
            allTimezones.Add(new TimezoneInfo("Azores", -1));
            allTimezones.Add(new TimezoneInfo("Greenwich Mean Time", 0));
            allTimezones.Add(new TimezoneInfo("Central European Standard Time", 1));
            allTimezones.Add(new TimezoneInfo("Central European Summer Time", 2));
            allTimezones.Add(new TimezoneInfo("Moscow", 3));
            allTimezones.Add(new TimezoneInfo("Abu Dhabi", 4));
            allTimezones.Add(new TimezoneInfo("Astana", 5));
            allTimezones.Add(new TimezoneInfo("Bangladesh", 6));
            allTimezones.Add(new TimezoneInfo("Jakarta", 7));
            allTimezones.Add(new TimezoneInfo("Singapore", 8));
            allTimezones.Add(new TimezoneInfo("Tokyo", 9));
            allTimezones.Add(new TimezoneInfo("Brisbane", 10));
            allTimezones.Add(new TimezoneInfo("Sakhalin", 11));
            allTimezones.Add(new TimezoneInfo("New Zealand", 12));

            return allTimezones;
        }

        public static TimezoneInfo GetTimezoneInfoByOffset(int timezoneOffset)
        {
            List<TimezoneInfo> tempList = GetAllTimezones();
            TimezoneInfo? foundTimezone = tempList.Find(t => t.TimezoneOffset == timezoneOffset);
            if (foundTimezone == null)
            {
                Console.WriteLine("Error finding timezone. Offset does not exist!");
                return null;
            }
            else
            {
                return foundTimezone;
            }
        }
        
        public static string GetReadableTimezoneString(TimezoneInfo info)
        {
            string convertedUTC = info.TimezoneOffset >= 0 ? "+" + info.TimezoneOffset.ToString() : info.TimezoneOffset.ToString();
            return "(UTC " + convertedUTC + ") " + info.TimezoneName;
        }
    }
}
