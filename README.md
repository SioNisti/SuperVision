# SuperVision
A desktop program to read the RAM of Super Mario Kart using (Q)USB2SNES.

The program consists of one or more enabled widgets (like splits or an attempt counter for example).

These widgets can be set in any order and you can define the font/background colors through the Layout Editor.

# Contributing
Adding new widgets should be decently easy.

Clone the repository, open in Visual Studio and take a look at the existing widgets inside the "Widgets" directory for examples on how to make your own.

Each widget decides what memory address(es) it reads (if any), what it does and how it looks.

For simple widgets you shouldn't need to touch any part of the project that isn't the widget itself.

Inside the "Services" directory is the "AttemptDataService.cs" which is like an invisible widget you can't remove.  It handles saving the time trial attempts to json along with the session specific data (this might change).

In the future this widget system could be expanded to allow external widgets (drop in plugins) instead of only these built in ones.