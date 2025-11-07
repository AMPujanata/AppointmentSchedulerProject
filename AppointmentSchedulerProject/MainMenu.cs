// See https://aka.ms/new-console-template for more information
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace AppointmentSchedulerProject
{
    public class MainMenu
    {
        
        private static void Main()
        {
            ShowMainMenu();
        }

        public static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Appointment Scheduler!");
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit Program");
            Console.WriteLine("Type in the number according to the choice you want to make.");

            int choice = -1;
            do
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine(); // move all other info to a new line
                if (int.TryParse(keyInfo.KeyChar.ToString(), out int number))
                {
                    if (number == 1 || number == 2 || number == 3) // all available choices
                    {
                        choice = number;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please try again!");
                }
            } while (choice == -1);

            switch (choice)
            {
                case 1:
                    Login();
                    break;
                case 2:
                    Register();
                    break;
                case 3:
                    return;
            }
        }

        private static void Login()
        {
            Console.Clear();
            Console.WriteLine("You are now logging in.");
            Console.Write("Please enter your username: ");
            string? enteredUsername = Console.ReadLine();

            try
            {
                IMongoCollection<UserInfo> usersCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<UserInfo>("users");

                FilterDefinition<UserInfo> usernameFilter = Builders<UserInfo>.Filter
                    .Eq(u => u.username, enteredUsername);

                UserInfo loginUser = usersCollection.Find(usernameFilter).FirstOrDefault();
                if (loginUser == null)
                {
                    Console.WriteLine("Error: No user found with that username!");
                    RetryLogin();
                    return;
                }
                else
                {
                    UserMenu.InitializeUserMenu(loginUser);
                }
            }
            catch (MongoException mexp)
            {
                Console.WriteLine("Unable to log in due to an error: " + mexp);
                RetryLogin();
            }
        }

        private static void RetryLogin()
        {
            Console.WriteLine();
            Console.WriteLine("Would you like to try logging in again? (Y/N)");

            char retryChoice;
            do
            {
                retryChoice = Console.ReadKey().KeyChar;
                if (char.IsLower(retryChoice)) retryChoice = char.ToUpper(retryChoice);
                if (retryChoice == 'Y')
                {
                    Login();
                }
                else if (retryChoice == 'N')
                {
                    ShowMainMenu();
                }
            } while (!(retryChoice == 'Y' || retryChoice == 'N'));
        }
        
        private static void Register()
        {
            Console.Clear();
            Console.WriteLine("You are now registering a new user.");

            Console.Write("Enter your real name: ");
            string? realName = Console.ReadLine();
            Console.Write("Enter your username: ");
            string? newUserName = Console.ReadLine();

            List<TimezoneInfo> allTimezones = TimezoneHelper.GetAllTimezones();
            int timezonesPerPage = 8;
            int totalPages = allTimezones.Count / timezonesPerPage;
            if (allTimezones.Count % timezonesPerPage != 0) totalPages += 1; // if exact amount, it doesnt need additional page, but otherwise it does
            int currentPage = 0;

            int choice = -1;
            do
            {
                Console.WriteLine();
                Console.WriteLine("Choose a timezone by typing its number: ");
                for (int i = 0; i < timezonesPerPage; i++)
                {
                    int index = i + (currentPage * timezonesPerPage);
                    if (index >= allTimezones.Count) break; // don't attempt to access out of bound indexes
                    TimezoneInfo info = allTimezones[index];
                    int choiceNumber = i + 1;
                    Console.WriteLine(choiceNumber + ". " + TimezoneHelper.GetReadableTimezoneString(info));
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
            Console.WriteLine("Real name: " + realName);
            Console.WriteLine("Username: " + newUserName);
            Console.WriteLine("Selected timezone: " + TimezoneHelper.GetReadableTimezoneString(selectedTimezone));
            Console.WriteLine("Is this info correct? (Y/N)");

            char finalChoice;
            do
            {
                finalChoice = Console.ReadKey().KeyChar;
                if (char.IsLower(finalChoice)) finalChoice = char.ToUpper(finalChoice);
                if (finalChoice == 'Y')
                {
                    UserInfo userToRegister = new()
                    {
                        name = realName,
                        username = newUserName,
                        timezone_offset = selectedTimezone.TimezoneOffset
                    };

                    UploadRegistrationInfo(userToRegister);
                }
                else if (finalChoice == 'N')
                {
                    ShowMainMenu();
                }
            } while (!(finalChoice == 'Y' || finalChoice == 'N'));
        }

        private static void UploadRegistrationInfo(UserInfo userToRegister)
        {
            Console.WriteLine();
            Console.WriteLine("Registering the user...");
            try
            {
                IMongoCollection<UserInfo> usersCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<UserInfo>("users");
                
                FilterDefinition<UserInfo> usernameFilter = Builders<UserInfo>.Filter
                    .Eq(u => u.username, userToRegister.username);

                UserInfo document = usersCollection.Find(usernameFilter).FirstOrDefault();
                if(document != null)
                {
                    Console.WriteLine("Error: There is already a user with the same username!");
                    Console.WriteLine("Press any key to return to main menu.");
                    Console.ReadKey(true);
                    ShowMainMenu();
                    return;
                }

                usersCollection.InsertOne(userToRegister);

                // Prints the document
                Console.WriteLine("Registration successful!");
                Console.WriteLine("Press any key to return to main menu.");
                Console.ReadKey(true);
                ShowMainMenu();
            }
            catch (MongoException mexp)
            {
                Console.WriteLine("Unable to register due to an error: " + mexp);
                Console.WriteLine("Press any key to return to main menu.");
                Console.ReadKey(true);
                ShowMainMenu();   
            }
        }
    }
}


