![alt text](https://github.com/BiologyTools/Bio/blob/master/banner.jpg)
# BioCore

A .NET6 version of Bio library. The difference being no Windows GUI Automation functions. Bio is a application & library for editing & annotating various microscopy image formats. Supports all bioformats supported images. Integrates with ImageJ, running ImageJ filters & macro functions. Download the [nuget package](https://www.nuget.org/packages/BioCore/2.7.7.2) for easy usage. Also check out the wiki for [library usage.](https://github.com/BiologyTools/Bio/wiki/Library-Usage) or check out the [documentation.](https://biologytools.github.io/)

[![NuGet version (BioCore)](https://img.shields.io/nuget/v/BioCore.svg?style=flat-square)](https://www.nuget.org/packages/BioCore/2.7.7.2)
[![NuGet version (BioCore)](https://img.shields.io/nuget/dt/BioCore?color=g)](https://www.nuget.org/packages/BioCore/2.7.7.2)
## Features

- C# scripting with sample tool-script and other sample scripts in "/Scripts/" folder. [See samples](https://github.com/BioMicroscopy/BioImage-Scripts)

- Supports running ImageJ macro commands on images open in Bio. New Console to run ImageJ macro commands and Bio C# functions.

- Supports Pyramidal images with multiple resolutions. Like whole slide images.

- Multiple view modes like Emission, and Filtered. ROI's shown for each channel can be configured in ROI Manager.

- Supports drawing shapes & colors onto 16 bit & 48 bit images, unlike System.Drawing.Graphics.

- Convenient viewing of image stacks with scroll wheel moving Z-plane and mouse side buttons scrolling C-planes.

- Editing & saving ROI's in images to OME format image stacks.

- Copy & Paste to quickly annotate images and name them easily by right click.

- Select multiple points by holding down control key, for delete & move tools. 

- Exporting ROI's from each OME image in a folder of images to CSV.

- Easy freeform annotation with magic select tool which selects based on blob detection.

- Use AForge filters by opening filters tool window and right click to apply. Currently supports only some AForge filters as many of them do not support 16bit and 48bit images. Convert to 8bit image to make use of more filters. Applyed filters can be easily recorded and used in scripts. Bio impliments some filters like crop for 16 & 48 bit images.

- `Star this project on Github to help spread the word about Bio!`

## Dependencies
- [BioFormatsNet6](https://github.com/BiologyTools/BioFormatsNET6)
- [IKVM](http://www.ikvm.net/)
- [AForgeImagingCore](https://github.com/BiologyTools/AForgeImagingCore)
- [LibTiff.Net](https://bitmiracle.com/libtiff/)
- [Cs-script](https://github.com/oleg-shilo/cs-script/blob/master/LICENSE)
- [ImageJ](https://imagej.nih.gov/ij/) (Only needed when running ImageJ macro commands)

## Licenses
- BioImage [GPL3](https://www.gnu.org/licenses/gpl-3.0.en.html)
- AForge [LGPL](http://www.aforgenet.com/framework/license.html)
- BioFormatsNet6 [GPL3](https://www.gnu.org/licenses/gpl-2.0.en.html)
- [IKVM](https://github.com/gluck/ikvm/blob/master/LICENSE)
- LibTiff.Net [BSD](https://bitmiracle.com/libtiff/)
- Cs-script [MIT](https://github.com/oleg-shilo/cs-script/blob/master/LICENSE)

## Scripting
-  Save scripts into "StartupPath/Scripts" with ".cs" ending.
-  Open script editor and recorder from menu.
-  Double click on script name in Script runner to run script.
-  Scripts saved in Scripts folder will be loaded into script runner.
-  Program installer include sample filter & tool script.
-  Use Script recorder to record program function calls and script runner to turn recorder text into working scripts. (See sample [scripts](https://github.com/BioMicroscopy/BioImage-Scripts)

## Sample Tool Script
```
//css_reference BioCore.dll;
using System;
using System.Windows.Forms;
using System.Drawing;
using Bio;
using System.Threading;

public class Loader
{

	//Point ROI Tool Example
	public string Load()
	{
		int ind = 1;
		do
		{
			Bio.Scripting.State s = Bio.Scripting.GetState();
			if (s != null)
			{
				if (!s.processed)
				{
					if (s.type == Bio.Scripting.Event.Down && s.buts == MouseButtons.Left)
					{
						ZCT cord = Bio.App.viewer.GetCoordinate();
						Bio.Scripting.LogLine(cord.ToString() + " Coordinate");
						Bio.ROI an = Bio.ROI.CreatePoint(cord, s.p.X, s.p.Y);
						Bio.ImageView.SelectedImage.Annotations.Add(an);
						Bio.Scripting.LogLine(cord.ToString() + " Coordinate");
						an.Text = "Point" + ind;
						ind++;
						Bio.Scripting.LogLine(s.ToString() + " Point");
						//ImageView.viewer.UpdateOverlay();
					}
					else
					if (s.type == Bio.Scripting.Event.Up)
					{
						Bio.Scripting.LogLine(s.ToString());
					}
					else
					if (s.type == Bio.Scripting.Event.Move)
					{
						Bio.Scripting.LogLine(s.ToString());
					}
					s.processed = true;
				}
			}
		} while (true);
		return "OK";
	}
}
```
