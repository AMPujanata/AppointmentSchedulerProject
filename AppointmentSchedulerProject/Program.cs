// See https://aka.ms/new-console-template for more information
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using dotenv.net;
namespace AppointmentSchedulerProject
{
    public class MainProgram
    {
        private static string connectionUri = "";
        
        public static void Main()
        {
            var envVars = DotEnv.Read();
            connectionUri = envVars["MONGODB_URI"];
            Console.Write("ConnectionUri: " + connectionUri);
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
                    string convertedUTC = info.TimezoneOffset >= 0 ? "+" + info.TimezoneOffset.ToString() : info.TimezoneOffset.ToString();
                    Console.WriteLine(choiceNumber + ". (UTC " + convertedUTC + ") " + info.TimezoneName);
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
            string selectedUTC = selectedTimezone.TimezoneOffset >= 0 ? "+" + selectedTimezone.TimezoneOffset.ToString() : selectedTimezone.TimezoneOffset.ToString();
            Console.WriteLine("Real name: " + realname);
            Console.WriteLine("Username: " + username);
            Console.WriteLine("Selected timezone: (UTC " + selectedUTC + ") " + selectedTimezone.TimezoneName);
            Console.WriteLine("Is this info correct? (Y/N)");

            char finalChoice;
            do
            {
                finalChoice = Console.ReadKey().KeyChar;
                if (char.IsLower(finalChoice)) finalChoice = char.ToUpper(finalChoice);
                if (finalChoice == 'Y')
                {
                    UploadRegistrationInfo(realname, username, selectedTimezone.TimezoneOffset);
                }
                else if (finalChoice == 'N')
                {
                    MainMenu();
                }
            } while (!(finalChoice == 'Y' || finalChoice == 'N'));
        }

        public static void UploadRegistrationInfo(string? realname, string? username, int selectedTimezoneOffset)
        {
            Console.WriteLine();
            Console.WriteLine("Registering the user...");
            UserInfo registeredUser = new()
            {
                Realname = realname,
                Username = username,
                TimezoneOffset = selectedTimezoneOffset
            };
            try
            {
                MongoClient client = new(connectionUri);
                IMongoCollection<UserInfo> usersCollection = client.GetDatabase("appointment_project").GetCollection<UserInfo>("users");
                
                FilterDefinition<UserInfo> usernameFilter = Builders<UserInfo>.Filter
                    .Eq(r => r.Username, username);

                UserInfo document = usersCollection.Find(usernameFilter).FirstOrDefault();
                if(document != null)
                {
                    Console.WriteLine("Error: There is already a user with the same username! Returning to main menu...");
                    MainMenu();
                    return;
                }

                usersCollection.InsertOne(registeredUser);

                // Prints the document
                Console.WriteLine("Registration successful!");

                MainMenu();
            }
            catch (MongoException mexp)
            {
                Console.WriteLine("Unable to register due to an error: " + mexp);
                MainMenu();   
            }
        }
        
        public static void DebugDocument()
        {
            var client = new MongoClient(connectionUri);
            var collection = client.GetDatabase("sample_mflix").GetCollection<BsonDocument>("movies");
            var filter = Builders<BsonDocument>.Filter.Eq("title", "Back to the Future");
            var document = collection.Find(filter).First();
            Console.WriteLine(document.ToJson(new JsonWriterSettings { Indent = true }));

            MainMenu();
        }
    }
}


