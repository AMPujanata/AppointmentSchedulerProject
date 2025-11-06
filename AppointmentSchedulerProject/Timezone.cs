namespace AppointmentSchedulerProject
{
    public class TimezoneInfo(string name, int offset)
    {
        public string timezoneName = name;
        public int timezoneOffset = offset;

        
    };

    public static class TimezoneGetter
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
    }
}
