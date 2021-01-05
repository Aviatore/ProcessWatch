# Process Watch

## Story

Task managers are nice to use but usually they are limited.
On the other hand you can find and manipulate processes from the command line any way,
but it is really not a user-friendly experience.

So your team decided to implement your own graphical but versatile task manager.

## What are you going to learn?

- How to build a console application with C#
- What are the properties of processes in an operating system.


## Tasks

1. Gather the current processes from the computer and display in the application.
    - Display as much information about the running processes as possible
        - If one property of a process can't be retrieved then still display the process and note that the information is not available for the given attribute

        2. Add a functionality to refresh list of active processes.
            - When a certain command or key is inputted to the console, the list of active processes shall be re-requested and displayed

            3. Provide some way for the user to filter the displayed processes.
                - Filter the processes of a given user
                    - Display all the processes that have the same parent process

                    4. Provide some way for the user to kill processes.
                        - Provide a way to select some of the processes
                            - Provide a way to select all of the **displayed** processes
                                - Provide a way to terminate selected processes

                                5. Provide contextual help / usage information for the user.
                                    - Have some kind of instructions on how the application controls work 

                                    6. Add an "About" section displaying general description about the application.
                                        - Add a feature that gives a brief description of the application

## General requirements

None

## Hints


Since it is meant to be a Console Application, there is no GUI framework bulit into the project.
You need to design and create the Menu, User Interface and Display of the application just as if you
would with any other console app. This might remind you about the ERP project back in ProgBasics.
Try to consider the following elements when planning your work: 

- Create application logic for User inputs, data actions and display with separation of concerns,
- Obtain information on the active system processes with the OOP approach,
- Pass data into an OOP compliant manner into a proper storage structure using classes and collections,
- Implement methods for data manipulation in the storage structure, possibly using ORM Linq syntax,
- Handle possible exceptions as it is quite likely you will not be allowed to close every system process,
- Display elements in readable format: there can be hundreds of lines to display and will not fit one screen.
  
  Have fun!



## Background materials

- <i class="far fa-video"></i> [5 minutes long video on Process Class basics](https://www.youtube.com/watch?v=XPpC4TQNniI)
- <i class="far fa-exclamation"></i> [Microsoft Documentation on Process Class](https://docs.microsoft.com/pl-pl/dotnet/api/system.diagnostics.process?view=netcore-3.1)
- <i class="far fa-video"></i> [Video on Linq basics](https://www.youtube.com/watch?v=yClSNQdVD7g)
- <i class="far fa-exclamation"></i> [Page large result sets: useful to create user friendly UX in console apps](https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/org-service/page-large-result-sets-linq)
- <i class="far fa-exclamation"></i> [C# Properties](https://www.tutlane.com/tutorial/csharp/csharp-properties-get-set)
- <i class="far fa-exclamation"></i> [Static Classes and Members](https://www.c-sharpcorner.com/UploadFile/74ce7b/static-class-in-C-Sharp/)

