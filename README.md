# ThreeXPlusOne

A .NET 8 Console app that generates a visualization of the [Collatz Conjecture](https://en.wikipedia.org/wiki/Collatz_conjecture) (aka 3x+1).

## What is the Collatz Conjecture?

Veritasium has a great [explainer video](https://www.youtube.com/watch?v=094y1Z2wpJg).

The basics:

* Pick any positive integer
* If it is even, divide it by 2
* If it is odd, multiply it by 3 and add 1
* Repeat until an infinite loop is reached of 4, 2, 1

This means that (in theory) all selected positive integers result in the inifinite loop. When running the algorithm on many positive integers and plotting the generated numbers as a Directed Graph, all number series lead to a root of 4, 2, 1. Where numbers are shared in the series, branching occurs.

## What does this app do?

The app creates:

* Either a 2D or pseudo-3D Directed Graph based on running the conjecture with a list of random numbers or numbers provided by the user
* A histogram showing the distribution of numbers in the generated series that start with 1 - 9, which illustrates that the calculated numbers follow [Benford's law](https://en.wikipedia.org/wiki/Benford%27s_law).
* A metadata file containing information about the given run of the process

Directed Graphs are drawn via the SkiaSharp library. All output can be toggled on and off, and a variety of items can be configured via the app's settings.

## Usage

* Running the executable (or the DLL, depending on how you run the app) with the flag `--help` will output help text.
* Running the executable/DLL with the flag `--config` will output information about each available custom setting and suggested starting points for the values of the app settings.
* In order to provide custom settings to the app, an `appSettings.json` file must be provided. Either:
  * Put the file in the same folder as the executable/DLL; or
  * Use the `--settings` flag to provide a directory path of your choice containing the `appSettings.json` file.
* If the `appSettings.json` file is not supplied by any of the above means, defaults will be used.
* Run the executable from the command line with no arguments (with or without `appSettings.json` in place)
* Running the process with the same list of numbers repeatedly will put all generated output into the same folder that was created by the process on the first run of the given number list

## Performance

* The app creates a canvas large enough to put a bounding box around the maximum x and y node coordinates, and this is influenced by the `RandomNumberMax`, `NodeRadius`, `XNodeSpacer`, and `YNodeSpacer` values in `appSettings.json`
* The higher the value given to `RandomNumberMax` in `appSettings.json`, the more likely you will get a very high y-axis maximum, which can make the canvas size very large, and therefore result in large PNG file sizes
*  Generally a `RandomNumberMax` of under 3000 is manageable, but may still generate a y-axis maximum of ~50,000 pixels
* The app will show you in the console output the size of the canvas it will generate. If it is too large, you can cancel the image creation.
* Machines with lower-spec GPUs may fail to render large canvas sizes; your mileage may vary.

## Example output

These three images use the same number series with different settings applied to the graph. See <a href="https://github.com/wdthem/ThreeXPlusOne/raw/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt" target="_blank">ExampleOutputSettings.txt</a> for the full settings for each graph.
<table>
  <tr>
    <td><img src="https://github.com/wdthem/ThreeXPlusOne/raw/main/ThreeXPlusOne.ExampleOutput/2D-NoRotation.png" width="250" height="250" alt="ThreeXPlusOne - 2D without node rotation"><br />2D without node rotation</td>
    <td><img src="https://github.com/wdthem/ThreeXPlusOne/raw/main/ThreeXPlusOne.ExampleOutput/2D-WithRotation.png" width="250" height="250" alt="ThreeXPlusOne - 2D with node rotation"><br />2D with node rotation</td>
    <td><img src="https://github.com/wdthem/ThreeXPlusOne/raw/main/ThreeXPlusOne.ExampleOutput/3D-WithRotation.png" width="250" height="250" alt="ThreeXPlusOne - 3D with node rotation"><br />3D with node rotation</td>
  </tr>
</table>



