# ThreeXPlusOne

A .NET 7 Console app that generates a visualization of the Collatz Conjecture (aka 3x+1).

# What is the Collatz Conjecture?

Watch this Veritasium video: https://www.youtube.com/watch?v=094y1Z2wpJg

The basics:

* Pick any positive integer
* If it is even, divide it by 2
* If it is odd, multiply by 3 and add 1
* Repeat until an infinite loop is reached of 4, 2, 1

This means that all selected integers result in the inifinite loop. When plotting the generated numbers as a Directed Graph, all number series lead to a root of 4, 2, 1. Where numbers are shared in the series, branching occurs.

# Usage

* A 'settings.json' file must exist in the same folder of either the executable or the DLL, depending on how you run the app.
* Running the executable or dll with the flag --help will output help text regarding values for the settings.
