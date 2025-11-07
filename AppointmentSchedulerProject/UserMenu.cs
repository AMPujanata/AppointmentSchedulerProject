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
                    // method for creating appointments
                    Console.WriteLine("Function not currently supported!");
                    ShowUserMenu();
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