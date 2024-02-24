# MKL_Spline_App
This application approximates function values using two cubic interpolation splines and visualises the function.
The splines' first derivative value at the start and the end of the measured data segment can be entered by the user. 
\
The user can choose between three types of functions:
- Cubic polynomial: _y = x^3 + 3x^2 - 6x - 18_
- Exponential function
- Pseudorandom number generator\
\
This application is made via Windows Presentation Foundation following MVVM pattern and uses the Intel MKL math library for spline interpolations.
The Intel MKL library has to be installed manually.
