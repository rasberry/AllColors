# AllColors
Program that creates a png with every possible 24-bit color. Includes optional sorting methods.

Inspired by [formula-to-determine-brightness-of-rgb-color](https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color#answer-24213274)

## Patterns:
1. BitOrder<br/>
   Native order of the C# [Color](https://msdn.microsoft.com/en-us/library/system.drawing.color.aspx) class
1. Luminance<br/>
   Order by [Luminance](https://en.wikipedia.org/wiki/Luminance_%28relative%29) from dark to light
1. AERT</br>
   Order by Accessibility [AERT](http://www.w3.org/TR/AERT#color-contrast) Contrast
1. HSP</br>
   Order by Alternative color model [HSP](http://alienryderflex.com/hsp.html) by Darel Rex Finley
1. WCAG2</br>
   Order by Accessibility [WCAG 2.0](http://www.w3.org/TR/WCAG20/#relativeluminancedef) luminance
1. VofHSV</br>
   Order by [HSV](https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness) value component
1. IofHSI</br>
   Order by [HSL](https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness) lightness component
1. LofHSL</br>
   Order by [HSI](https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness) intensity component

## Usage:
```
>AllColors.exe
AllColors [options] (image name)
 Options:
  -p (pattern name / number)     Choose a specific pattern (see below)
  -v                             Show Progress
 Patterns:
  1 BitOrder
  2 Luminance
  3 AERT
  4 HSP
  5 WCAG2
  6 VofHSV
  7 IofHSI
  8 LofHSL
```