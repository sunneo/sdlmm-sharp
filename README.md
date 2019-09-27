# sdlmm-sharp
SDLMM for c#

This project provides rendering primitives to regards a control as a canvas, therefore developer can customize UI controls.
* IRenderer (rendering primitives as sdlmm, sdlmm-java)
  * SharpDXControl
    * use SharpDX.Direct2D (>=directx 9.3) to implement
    * flushBMP is also implemented to render content onto a bitmap. 
  * SDLMMControl 
    * use GDI+
* IGraphics
  * wrap IRenderer into GDI+ alike API, so you can continue use most of your GDI+ codes with few changes:
```Csharp
   using(var gdiGraphics = ...) 
   {
      // your GDI+ codes goes here  
   }
```
becomes:
```Csharp
   using(var _gdiGraphics = ...) 
   using(IRenderer r = new SharpDXControl(this.Width,this.Height))
   {
      var _gdiGraphics = r.GetGraphics();
      // your GDI+ codes goes here
      
   }
```
