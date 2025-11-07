using MongoDB.Driver;

namespace AppointmentSchedulerProject
{
    public class UserMenu
    {
        private static UserInfo? _currentUser;
        public static void InitializeUserMenu(UserInfo loginUser)
        {
            _currentUser = loginUser;
            Console.WriteLine("Welcome, " + _currentUser.Realname);
            ShowUserMenu();
        }

        private static void ShowUserMenu()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1. Create an appointment");
            Console.WriteLine("2. View your appointments");
            Console.WriteLine("3. View your user data");
            Console.WriteLine("4. Logout");

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
                    CreateAppointment();
                    break;
                case 2:
                    // method for viewing appointments
                    Console.WriteLine("Function not currently supported!");
                    ShowUserMenu();
                    break;
                case 3:
                    ViewUserData();
                    break;
                case 4:
                    Logout();
                    break;
            }
        }

        private static void CreateAppointment()
        {
            Console.WriteLine("You are now creating an appointment.");

            string? appointmentName = "";
            List<UserInfo> invitedUsers = [];
            DateOnly appointmentDate = new();
            TimeOnly startingTimeOfDay = new();
            TimeOnly endingTimeOfDay = new();

            do
            {
                Console.Write("Enter your appointment name: ");
                appointmentName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(appointmentName))
                {
                    Console.WriteLine("Appointment name cannot be empty!");
                }
            }
            while (string.IsNullOrWhiteSpace(appointmentName));

            string? inviteInput = "";
            do
            {
                Console.Write("Enter the username you'd like to invite (Leave blank to continue): ");
                inviteInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inviteInput)) continue; // don't run the rest of this if user left it blank

                IMongoCollection<UserInfo> usersCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<UserInfo>("users");
                FilterDefinition<UserInfo> usernameFilter = Builders<UserInfo>.Filter
                    .Eq(u => u.Username, inviteInput);

                UserInfo invitedUser = usersCollection.Find(usernameFilter).FirstOrDefault();

                if (invitedUser == null)
                {
                    Console.WriteLine("Error: No user found with that username!");
                }
                else
                {
                    Console.WriteLine("Successfully added user " + invitedUser.Realname + " to the appointment!");
                    invitedUsers.Add(invitedUser);
                }

            } while (!string.IsNullOrWhiteSpace(inviteInput));

            // calculate available times, per hour, based on invited users
            int creatorOffset = _currentUser.TimezoneOffset;

            TimeOnly creatorStartTime = new(8, 0);
            TimeOnly creatorEndTime = new (17, 0);

            foreach (UserInfo user in invitedUsers)
            {
                int offsetDifference = creatorOffset - user.TimezoneOffset; // Ex. Creator is +7, invite is +2, -5 diff

                TimeOnly invitedUserStartTime = creatorStartTime.AddHours(offsetDifference);
                TimeOnly invitedUserEndTime = creatorEndTime.AddHours(offsetDifference);

                if (creatorStartTime < invitedUserStartTime) creatorStartTime = invitedUserStartTime;
                if (creatorEndTime > invitedUserEndTime) creatorEndTime = invitedUserEndTime;
            }

            if (creatorStartTime > creatorEndTime)
            {
                Console.WriteLine("Error! No available times for this group of users!");
                Console.ReadKey();
                ShowUserMenu();
                return;
            }

            Console.WriteLine("Available time: " + creatorStartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " - " + creatorEndTime.ToString("HH:mm"), System.Globalization.CultureInfo.InvariantCulture);

            string? dateInput = "";
            do
            {
                Console.Write("Enter the appointment date for your timezone (Example: 12-25-2030): ");
                dateInput = Console.ReadLine();

                if (!DateOnly.TryParse(dateInput, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out appointmentDate))
                {
                    Console.WriteLine("The date format was not correct. Try again!");
                }
            } while (appointmentDate == default); // while appointment date hasn't been set correctly

            string? startTimeInput = "";
            do
            {
                Console.Write("Enter starting time for your timezone (Example: 09:30): ");
                startTimeInput = Console.ReadLine();

                if (!TimeOnly.TryParseExact(startTimeInput, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startingTimeOfDay))
                {
                    Console.WriteLine("The time format was not correct. Try again!");
                    continue;
                }
            } while (startingTimeOfDay == default);

            string? endTimeInput = "";
            do
            {
                Console.Write("Enter ending time for your timezone (Example: 11:00): ");
                endTimeInput = Console.ReadLine();

                if (!TimeOnly.TryParseExact(endTimeInput, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endingTimeOfDay))
                {
                    Console.WriteLine("The time format was not correct. Try again!");
                    continue;
                }
            } while (endingTimeOfDay == default);

            DateTime startOfAppointmentDateTime = appointmentDate.ToDateTime(startingTimeOfDay);
            DateTime endOfAppointmentDateTime = appointmentDate.ToDateTime(endingTimeOfDay);
            string allInvitedUsersString = "";
            foreach (UserInfo user in invitedUsers)
            {
                if (string.IsNullOrWhiteSpace(allInvitedUsersString)) // don't add comma if it's the first user
                {
                    allInvitedUsersString = user.Realname;
                }
                else
                {
                    allInvitedUsersString = string.Join(", ", allInvitedUsersString, user.Realname);
                }
            }
            
            Console.WriteLine("Final appointment details: ");
            Console.WriteLine("Appointment Name: " + appointmentName);
            Console.WriteLine("Appointment Starting Time: " + startOfAppointmentDateTime.ToString());
            Console.WriteLine("Appointment Ending Time: " + endOfAppointmentDateTime.ToString());
            Console.WriteLine("Invited user(s): " + allInvitedUsersString);

            Console.Write("Create an appointment with these details? (Y/N): ");
            char finalChoice;
            do
            {
                finalChoice = Console.ReadKey().KeyChar;
                if (char.IsLower(finalChoice)) finalChoice = char.ToUpper(finalChoice);
                if (finalChoice == 'Y')
                {
                    // prepare data to send
                    List<string> invitedUsernames = [];
                    foreach (UserInfo user in invitedUsers)
                    {
                        invitedUsernames.Add(user.Username);
                    }

                    AppointmentInfo appointmentToUpload = new()
                    {
                        Title = appointmentName,
                        Creator_Id = _currentUser.Username,
                        StartTime = startOfAppointmentDateTime,
                        EndTime = endOfAppointmentDateTime,
                        Invited_Users = invitedUsernames
                    };

                    Console.WriteLine("Function unimplemented currently");
                    ShowUserMenu();
                    // upload document
                }
                else if (finalChoice == 'N')
                {
                    ShowUserMenu();
                }
            } while (!(finalChoice == 'Y' || finalChoice == 'N'));
        }
        
        private static void ViewUserData()
        {
            Console.WriteLine("Your current data: ");
            Console.WriteLine("Real name: " + _currentUser.Realname);
            Console.WriteLine("Username: " + _currentUser.Username);
            TimezoneInfo preferredTimezone = TimezoneHelper.GetTimezoneInfoByOffset(_currentUser.TimezoneOffset);
            Console.WriteLine("Your current timezone: " + TimezoneHelper.GetReadableTimezoneString(preferredTimezone));
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey(true);
            ShowUserMenu();
        }
        
        private static void Logout()
        {
            _currentUser = null; // purge user data since we're not using it anymore
            MainMenu.ShowMainMenu();
        }
    }
}