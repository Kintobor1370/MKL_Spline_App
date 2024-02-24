# MKL_Spline_App
This application approximates function values using two cubic interpolation splines and visualises the function.
\
The splines' first derivative value at the start and the end of the measured data segment can be entered by the user. 

The user can choose between three types of functions:
- Cubic polynomial: _y = x^3 + 3x^2 - 6x - 18_
- Exponential function
- Pseudorandom number generator
<a/>
\

Application features:
- made via Windows Presentation Foundation
- utilises MVVM pattern
- to affect the precision of the interpolation, the user can modify the data segment's boundaries, derivative values, and number of nodes the segment will be divided on.
- Intel MKL math library is used for spline interpolations (has to be installed manually).
