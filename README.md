# ThreeXPlusOne

A .NET 7 Console app that generates a visualization of the Collatz Conjecture (aka 3x+1).

# What is the Collatz Conjecture?

Watch this Veritasium video: https://www.youtube.com/watch?v=094y1Z2wpJg

The basics:

* Pick any positive integer
* If it is even, divide it by 2
* If it is odd, multiply it by 3 and add 1
* Repeat until an infinite loop is reached of 4, 2, 1

This means that all selected integers result in the inifinite loop. When plotting the generated numbers as a Directed Graph, all number series lead to a root of 4, 2, 1. Where numbers are shared in the series, branching occurs.

# What does this app do?

The app creates:

* Either a 2D or pseudo-3D Directed Graph based on running the conjecture with a list of random numbers or numbers provided by the user
* A histogram showing the distribution of numbers in the generated series that start with 1 - 9
* A metadata file containing information about the given run of the process

All output can be toggled on and off.

# Usage

* A 'settings.json' file must exist in the same folder of either the executable or the DLL, depending on how you run the app.
* Running the executable or dll with the flag --help will output help text regarding a suggested starting point for the values of the settings.
* With the 'settings.json' file in place, run the executable from the command line with no arguments
* Running the process with the same list of numbers will put any generated output into the same folder name as created by the process itself

Note that the app needs to create a large canvas in order to plot the points. This can be resource intensive. The app could fail on machines with low GPU specs.
