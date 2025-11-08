**Appointment Scheduler**

https://github.com/user-attachments/assets/59e1ea95-19a0-4adc-9b0f-cab5480301a5

![photo1](https://github.com/user-attachments/assets/e62cddba-2a13-4471-89f3-9f9b0050d1db)
![photo2](https://github.com/user-attachments/assets/5830c122-1b48-4d2b-869a-c63c05828d5a)

This is a console application built on the .NET language, used to create appointments that you can invite other users to. The program will automatically find available working times for each invited user, making it easy to schedule with users in different timezones.

**Setup Instructions**

1. Download the latest exe release from the Releases tab on the right side of the main Github page.
2. You may immediately run the application. The application was mainly tested on Windows machines.

**How to Use**

NOTE: As a console application, the application is mainly controlled through keyboard inputs. The main way of doing so is by typing a number when presented with a list of numbered choices, or typing in the required information then pressing ENTER to submit the information. When you are told to "press any key", any key on the keyboard will advance the prompt.

1. When you start the application, you will be on the main menu. You will need to register a user first before you can use the application. Press 2 to open the registration menu, and fill in the information it requires.
2. Once you're finished registering, you can now log in using your username.
3. Once logged in, you can view your appointment or user data. Currently, you have no appointments, so feel free to create an appointment.
4. When creating an appointment, you'll invite other users to the same appointment. There are a list of other users also on the database that you can invite, for this purpose (can be seen in the next section). However, note that if your timezone is too drastically different from that user's (9 or more hours away from their timezone), you may not have any available times left for the appointment.
5. Fill in the rest of the appointment details, and save it. You can now view that appointment, and other users you've invited will also be able to view that appointment.

**Other Example Users**

Username: eagle
Name: Reid Searcher
Timezone: UTC - 7

Username: wolf
Name: Lewie Jackson
Timezone: UTC + 1

Username: otter
Name: Jeanne Baker
Timezone: UTC + 8

Username: doors
Name: Jimmy Jillies
Timezone: UTC + 5


**Tools Used**

Main Programming Language: .NET, using the Visual Studio Code editor
Database: MongoDB Atlas, an online hosted version of MongoDB
Git: For version control, and hosting the source code for the application
.NET Libraries: MongoDB c# driver for connecting to the database, Dotenv Net for storing secret variables like the database connection url

**Other Notes**

API endpoints and session-based authentication were planned to be implemented, in order to prevent several instances of the same user from working on the same database. However, due to complications with the Atlas version of the MongoDB database, this was not possible to implement in a reasonable amount of time, and so I was forced to forego it, instead using the MongoDB Driver's built-in authentication.

I originally intended to use Unity in order to have a more visual, UI-based approach to making this application. However, it was not very easy to import the drivers into Unity despite it also using the C# language, mainly due to Unity using a Mono approach to programming. Because of this, I decided to create a .NET Console application instead.

This project was created over the span of three days.

If you want to run the source code version of this application, you will need the environment file (.env). If you have it (likely because it was attached to a message), then you will need to paste the .env file into the same folder as the .sln file for the application. This will allow the dotenv net library to automatically read the .env file.

![envimage](https://github.com/user-attachments/assets/5bc61e21-b937-476d-aa62-ae885a726586)
