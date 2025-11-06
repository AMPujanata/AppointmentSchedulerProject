// See https://aka.ms/new-console-template for more information
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace AppointmentSchedulerProject
{
    public class MainProgram
    {
        public static void Main()
        {
            Console.WriteLine("Welcome to the Appointment Scheduler!");
            MainMenu();
        }

        public static void MainMenu()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Debug");
            Console.WriteLine("4. Exit Program");
            Console.WriteLine("Type in the number according to the choice you want to make.");

            int choice = -1;
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine(); // move all other info to a new line
                if (int.TryParse(keyInfo.KeyChar.ToString(), out int number))
                {
                    if (number == 1 || number == 2 || number == 3 || number == 4) // all available choices
                    {
                        choice = number;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again!");
                }
                break;
            } while (choice == -1);

            switch (choice)
            {
                case 1:
                    Console.WriteLine("This function is not currently implemented.");
                    MainMenu();
                    break;
                case 2:
                    Register();
                    break;
                case 3:
                    DebugDocument();
                    break;
                case 4:
                    return;
            }
        }

        public static void Register()
        {
            Console.WriteLine("You are now registering a new user. You will need to provide the following information:");
            Console.WriteLine("a) Your name b) Your username c) Your preferred timezone");

            Console.Write("Enter your real name: ");
            string? realname = Console.ReadLine();
            Console.Write("Enter your username: ");
            string? username = Console.ReadLine();

            List<TimezoneInfo> allTimezones = TimezoneGetter.GetAllTimezones();
            int timezonesPerPage = 8;
            int totalPages = allTimezones.Count / timezonesPerPage;
            if (allTimezones.Count % timezonesPerPage != 0) totalPages += 1; // if exact amount, it doesnt need additional page, but otherwise it does
            int currentPage = 0;

            int choice = -1;
            do
            {
                Console.WriteLine("Choose a timezone by typing its number: ");
                for (int i = 0; i < timezonesPerPage; i++)
                {
                    int index = i + (currentPage * timezonesPerPage);
                    if (index >= allTimezones.Count) break; // don't attempt to access out of bound indexes
                    TimezoneInfo info = allTimezones[index];
                    int choiceNumber = i + 1;
                    string convertedUTC = info.timezoneOffset >= 0 ? "+" + info.timezoneOffset.ToString() : info.timezoneOffset.ToString();
                    Console.WriteLine(choiceNumber + ". (UTC " + convertedUTC + ") " + info.timezoneName);
                }

                if (currentPage > 0)
                {
                    Console.WriteLine("9. Previous Page");
                }

                if (currentPage < (totalPages - 1))
                {
                    Console.WriteLine("0. Next Page");
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine(); // move all other info to a new line
                if (int.TryParse(keyInfo.KeyChar.ToString(), out int number))
                {
                    if (number >= 1 && number <= 8) // user selected a timezone
                    {
                        choice = number;
                    }
                    else if (number == 9) // user went back a page
                    {
                        currentPage -= 1;
                    }
                    else if (number == 0) // user went forward a page
                    {
                        currentPage += 1;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again!");
                }
            } while (choice == -1);

            TimezoneInfo selectedTimezone = allTimezones[choice - 1 + (currentPage * timezonesPerPage)];
            string selectedUTC = selectedTimezone.timezoneOffset >= 0 ? "+" + selectedTimezone.timezoneOffset.ToString() : selectedTimezone.timezoneOffset.ToString();
            Console.WriteLine("Real name: " + realname);
            Console.WriteLine("Username: " + username);
            Console.WriteLine("Selected timezone: (UTC " + selectedUTC + ") " + selectedTimezone.timezoneName);
            Console.Write("Press any key to continue. (Function incomplete)");
            Console.ReadKey();
        }

        public static void DebugDocument()
        {
            string connectionUri = "mongodb+srv://andrewpujanata_db_user:YXxwfOx7EhKVBUMQ@appointmentcluster.9ohqrbw.mongodb.net/?appName=AppointmentCluster";

            var client = new MongoClient(connectionUri);
            var collection = client.GetDatabase("sample_mflix").GetCollection<BsonDocument>("movies");
            var filter = Builders<BsonDocument>.Filter.Eq("title", "Back to the Future");
            var document = collection.Find(filter).First();
            Console.WriteLine(document.ToJson(new JsonWriterSettings { Indent = true }));

            MainMenu();
        }
    }
}


