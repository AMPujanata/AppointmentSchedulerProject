using MongoDB.Driver;

namespace AppointmentSchedulerProject
{
    public class UserMenu
    {
        private static UserInfo _currentUser = new();
        public static void InitializeUserMenu(UserInfo loginUser)
        {
            _currentUser = loginUser; // set user for this current login session
            ShowUserMenu();
        }

        private static void ShowUserMenu()
        {
            Console.Clear();
            Console.WriteLine("Welcome, " + _currentUser.name);
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
            } while (choice == -1);

            switch (choice)
            {
                case 1:
                    CreateAppointment();
                    break;
                case 2:
                    ViewAppointments();
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
            Console.Clear();
            Console.WriteLine("You are now creating an appointment.");

            string? appointmentName = "";
            List<UserInfo> invitedUsers = [];
            DateOnly appointmentDate = new();
            TimeOnly finalStartingTimeOfDay = new();
            TimeOnly finalEndingTimeOfDay = new();

            do
            {
                Console.Write("Enter your appointment name: ");
                appointmentName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(appointmentName))
                {
                    Console.WriteLine("Appointment name cannot be empty!"); // it would be strange to have a blank appointment name, especially when viewing appointments
                }
                Console.WriteLine();
            }
            while (string.IsNullOrWhiteSpace(appointmentName));

            string? inviteInput = "";
            do
            {
                Console.Write("Enter the username you'd like to invite (Leave blank to continue): ");
                inviteInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(inviteInput)) continue; // don't run the rest of this if user left it blank

                if (inviteInput == _currentUser.username) // user is already 'invited' to their own appointment
                {
                    Console.WriteLine("You are already the creator of the appointment!");
                    continue;
                }
                
                IMongoCollection<UserInfo> usersCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<UserInfo>("users");
                FilterDefinition<UserInfo> usernameFilter = Builders<UserInfo>.Filter
                    .Eq(u => u.username, inviteInput);

                UserInfo invitedUser = usersCollection.Find(usernameFilter).FirstOrDefault();

                if (invitedUser == null)
                {
                    Console.WriteLine("Error: No user found with that username!");
                }
                else
                {
                    Console.WriteLine("Successfully added user " + invitedUser.name + " to the appointment!");
                    invitedUsers.Add(invitedUser);
                }
                Console.WriteLine();
            } while (!string.IsNullOrWhiteSpace(inviteInput));

            // calculate available times, per hour, based on invited users
            int creatorOffset = _currentUser.timezone_offset;

            TimeOnly creatorStartTime = new(8, 0); // can be adjusted based on normal working hours
            TimeOnly creatorEndTime = new(17, 0);

            TimeOnly availableStartTime = creatorStartTime;
            TimeOnly availableEndTime = creatorEndTime;
            foreach (UserInfo user in invitedUsers)
            {
                int offsetDifference = creatorOffset - user.timezone_offset; // Ex. Creator' timezone is +8, invitee's timezone is +1, +7 diff
                // Which means that the invitee's 09:00 is Creator's 16:00. Update accordingly

                TimeOnly invitedUserStartTime = creatorStartTime.AddHours(offsetDifference);
                TimeOnly invitedUserEndTime = creatorEndTime.AddHours(offsetDifference);

                if (availableStartTime < invitedUserStartTime) availableStartTime = invitedUserStartTime;
                if ((availableEndTime > invitedUserEndTime) && (invitedUserEndTime.Hour != 0)) availableEndTime = invitedUserEndTime; 
                // TimeOnly treats midnight as 00:00; technically correct, but it should be treated as 24 for the End Time
            }

            if (availableStartTime > availableEndTime) // no available times for this group of users; cancel operations
            {
                Console.WriteLine("Error! No available times for this group of users!");
                Console.WriteLine("Press any key to return to user menu.");
                Console.ReadKey();
                ShowUserMenu();
                return;
            }

            Console.WriteLine("Available time: " + availableStartTime.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " - " + availableEndTime.ToString("HH:mm"), System.Globalization.CultureInfo.InvariantCulture);

            string? dateInput = "";
            do
            {
                Console.Write("Enter the appointment date for your timezone (Example: 12-25-2030): "); // mm-dd-yyyy; could not think of a more user friendly way to write out the required format
                dateInput = Console.ReadLine();

                if (!DateOnly.TryParse(dateInput, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out appointmentDate))
                {
                    Console.WriteLine("The date format was not correct. Try again!");
                    continue;
                }
                if (appointmentDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    Console.WriteLine("You cannot set an appointment date in the past. Please try again!");
                    appointmentDate = default;
                    continue;
                }
                Console.WriteLine();
            } while (appointmentDate == default); // while appointment date hasn't been set correctly

            string? startTimeInput = "";
            do
            {
                Console.Write("Enter starting time for your timezone (Example: 09:30): "); // this format is forced, due to overlap in values between HH:mm and mm:ss
                startTimeInput = Console.ReadLine();

                if (!TimeOnly.TryParseExact(startTimeInput, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out finalStartingTimeOfDay))
                {
                    Console.WriteLine("The time format was not correct. Try again!");
                    continue;
                }
                if(finalStartingTimeOfDay < availableStartTime || finalStartingTimeOfDay >= availableEndTime)
                {
                    Console.WriteLine("Cannot set a starting time that is not within the available hours! Please try again!");
                    finalStartingTimeOfDay = default;
                    continue;
                }
                Console.WriteLine();
            } while (finalStartingTimeOfDay == default);

            string? endTimeInput = "";
            do
            {
                Console.Write("Enter ending time for your timezone (Example: 11:00): ");
                endTimeInput = Console.ReadLine();

                if (!TimeOnly.TryParseExact(endTimeInput, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out finalEndingTimeOfDay))
                {
                    Console.WriteLine("The time format was not correct. Try again!");
                    continue;
                }
                if (finalEndingTimeOfDay <= availableStartTime || finalEndingTimeOfDay > availableEndTime)
                {
                    Console.WriteLine("Cannot set an ending time that is not within the available hours! Please try again!");
                    finalEndingTimeOfDay = default;
                    continue;
                }
                if (finalEndingTimeOfDay <= finalStartingTimeOfDay)
                {
                    Console.WriteLine("Cannot set an ending time that is before the starting time! Please try again!");
                    finalEndingTimeOfDay = default;
                    continue; 
                }
                Console.WriteLine();
            } while (finalEndingTimeOfDay == default);

            DateTime startOfAppointmentDateTime = appointmentDate.ToDateTime(finalStartingTimeOfDay);
            DateTime endOfAppointmentDateTime = appointmentDate.ToDateTime(finalEndingTimeOfDay);
            string allInvitedUsersString = "";
            foreach (UserInfo user in invitedUsers)
            {
                if (string.IsNullOrWhiteSpace(allInvitedUsersString)) // don't add comma to start of string if it's the first user
                {
                    allInvitedUsersString = user.name ?? "";
                }
                else
                {
                    allInvitedUsersString = string.Join(", ", allInvitedUsersString, user.name);
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
                        invitedUsernames.Add(user.username ?? "");
                    }
                    startOfAppointmentDateTime.AddHours(-(_currentUser.timezone_offset)); // convert DateTime to UTC + 0 format first
                    endOfAppointmentDateTime.AddHours(-(_currentUser.timezone_offset));

                    AppointmentInfo appointmentToUpload = new()
                    {
                        title = appointmentName,
                        creator_id = _currentUser.username,
                        start_time = startOfAppointmentDateTime,
                        end_time = endOfAppointmentDateTime,
                        invited_users = invitedUsernames
                    };

                    UploadCreatedAppointment(appointmentToUpload);
                }
                else if (finalChoice == 'N') // cancel appointment creation
                {
                    ShowUserMenu();
                }
            } while (!(finalChoice == 'Y' || finalChoice == 'N'));
        }

        private static void UploadCreatedAppointment(AppointmentInfo appointmentToRegister)
        {
            Console.WriteLine();
            Console.WriteLine("Creating an appointment...");

            try
            {
                IMongoCollection<AppointmentInfo> appointmentsCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<AppointmentInfo>("appointments");

                appointmentsCollection.InsertOne(appointmentToRegister);

                Console.WriteLine("Appointment successfully created!");
                Console.WriteLine("Press any key to return to user menu.");
                Console.ReadKey(true);

                ShowUserMenu();
            }
            catch (MongoException mexp)
            {
                Console.WriteLine("Unable to create appointment due to an error: " + mexp);
                Console.WriteLine("Press any key to return to user menu.");
                Console.ReadKey(true);
                ShowUserMenu();
            }
        }

        private static void ViewAppointments()
        {
            Console.Clear();
            try
            {
                IMongoCollection<AppointmentInfo> appointmentsCollection = MongoData.ConnectionClient.GetDatabase("appointment_project").GetCollection<AppointmentInfo>("appointments");
                
                FilterDefinition<AppointmentInfo> appointmentFilter = Builders<AppointmentInfo>.Filter
                    .Eq(a => a.creator_id, _currentUser.username);
                appointmentFilter |= Builders<AppointmentInfo>.Filter.AnyEq(a => a.invited_users, _currentUser.username);

                List<AppointmentInfo> yourAppointments = appointmentsCollection.Find(appointmentFilter).ToList();

                if (yourAppointments.Count == 0)
                {
                    Console.WriteLine("You do not currently have any appointments scheduled.");
                    Console.WriteLine("Press any key to return to user menu.");
                    Console.ReadKey();
                    ShowUserMenu();
                    return;
                }

                Console.WriteLine("Your current appointments: ");
                int appointmentNumber = 1;
                foreach (AppointmentInfo info in yourAppointments)
                {
                    DateTime adjustedStartingTime = info.start_time.AddHours(_currentUser.timezone_offset); // adjust time to display in the user's preferred timezone
                    DateTime adjustedEndingTime = info.end_time.AddHours(_currentUser.timezone_offset);
                    Console.WriteLine(appointmentNumber + ". " + info.title + " by " + info.creator_id + ": " + adjustedStartingTime.ToLongDateString() + ", " + adjustedStartingTime.ToShortTimeString() + " - " + adjustedEndingTime.ToShortTimeString());
                }
                
                Console.WriteLine("Press any key to return to user menu.");
                Console.ReadKey();
                ShowUserMenu();
            }
            catch (MongoException mexp)
            {
                Console.WriteLine("Unable to view appointments due to an error: " + mexp);
                Console.WriteLine("Press any key to return to user menu.");
                Console.ReadKey(true);
                ShowUserMenu();
            }
        }
        
        private static void ViewUserData()
        {
            Console.Clear();
            Console.WriteLine("Your current data: ");
            Console.WriteLine("Real name: " + _currentUser.name);
            Console.WriteLine("Username: " + _currentUser.username);
            TimezoneInfo preferredTimezone = TimezoneHelper.GetTimezoneInfoByOffset(_currentUser.timezone_offset);
            Console.WriteLine("Your current timezone: " + TimezoneHelper.GetReadableTimezoneString(preferredTimezone));
            Console.WriteLine("Press any key to return to user menu.");
            Console.ReadKey(true);
            ShowUserMenu();
        }
        
        private static void Logout()
        {
            _currentUser = new UserInfo(); // purge user data since we're not using it anymore
            MainMenu.ShowMainMenu();
        }
    }
}